using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CargoWise.IO.Testing.SharpFtpServer
{
	public class FtpServer : IDisposable
	{
		bool _disposed;
		bool _listening;
		TcpListener _listener;
		List<ClientConnection> _activeConnections;

		readonly IPEndPoint _localEndPoint;
		readonly UserStore _userStore;

		public FtpServer(IPAddress ipAddress, int port, UserStore userStore)
		{
			_localEndPoint = new IPEndPoint(ipAddress, port);
			_userStore = userStore;
		}

		public void Start()
		{
			if (_listening)
			{
				return;
			}

			_listener = new TcpListener(_localEndPoint);
			_listening = true;
			_listener.Start();
			_activeConnections = new List<ClientConnection>();
			startListenerResult = _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);
		}

		internal IAsyncResult startListenerResult;

		public void Stop()
		{
			_listening = false;
			if (_listener != null)
			{
				_listener.Stop();
			}
		}

		void HandleAcceptTcpClient(IAsyncResult result)
		{
			try
			{
				var client = _listener.EndAcceptTcpClient(result);

				var connection = new ClientConnection(client, _userStore);

				_activeConnections.Add(connection);

				ThreadPool.QueueUserWorkItem(connection.HandleClient, client);
			}
			catch (Exception)
			{
				if (_listening)
				{
					throw;
				}
			}

			if (_listening)
			{
				startListenerResult = _listener.BeginAcceptTcpClient(HandleAcceptTcpClient, _listener);
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				Stop();

				foreach (var conn in _activeConnections)
				{
					conn.Dispose();
				}

				_disposed = true;
			}
		}
	}
}
