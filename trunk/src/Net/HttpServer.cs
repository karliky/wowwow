/*
 * Created by SharpDevelop.
 * User: BIOSTAT26
 * Date: 22/08/2005
 * Time: 13:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace Server
{
	/// <summary>
	/// Description of HttpServer.
	/// </summary>
	public class HttpServer : IDisposable
	{
		string ip;
		int port;
		Socket listenSocket;
		public static ArrayList clients = new ArrayList();
		IPAddress address;
		AddressFamily addressFamily;
		public int totalSend = 0;
		public int totalReceive = 0;		
		
		public HttpServer( string i, int pt )
		{			
			ip = i;
			port = pt;
            IPHostEntry lipa = Dns.GetHostEntry(IPAddress.Parse(i));
			IPEndPoint lep = new IPEndPoint( lipa.AddressList[ 0 ], port );
			addressFamily = lep.Address.AddressFamily;
			address = lep.Address;
			Start();
		}
		public bool Start()
		{
			try 
			{
				listenSocket = new Socket( addressFamily, SocketType.Stream, ProtocolType.Tcp );
				listenSocket.Bind( new IPEndPoint( IPAddress.Any, port ) );
				Console.WriteLine("Listen on port {0}, IP {1}", port, address.ToString() );
				listenSocket.Listen( 10 );
				listenSocket.BeginAccept( new AsyncCallback( this.OnAccept ), listenSocket );				
			} 
			catch(Exception e )
			{
				Console.WriteLine("Failled to list on port {0}\n{1}", port, e.Message );
				listenSocket = null;
				return false;
			}		
			return true;
		}
		public virtual void OnAccept( IAsyncResult ar ) 
		{
			try 
			{	
				Socket newSocket = listenSocket.EndAccept( ar );
				
				if ( newSocket != null ) 
				{
					HttpConnection newClient = new HttpConnection( newSocket, new RemoveClientDelegate( this.RemoveClient ) );
					clients.Add( newClient );
					newClient.Start();
				}
			} 
			catch {}
			try 
			{
				listenSocket.BeginAccept( new AsyncCallback( this.OnAccept ), listenSocket );
			} 
			catch
			{
				Dispose();
			}
		}		
		public void RemoveClient( SockClient client )
		{
			clients.Remove( client );			
		}
		
		//	public abstract void OnAccept( IAsyncResult ar );
		public void Dispose() 
		{			
			while ( clients.Count > 0) 
			{
				(( SockClient )clients[0]).Dispose();
			}
			try 
			{
				listenSocket.Shutdown(SocketShutdown.Both);
			} 
			catch
			{}
			if ( listenSocket != null )
				listenSocket.Close();			
		}		
	}
}
