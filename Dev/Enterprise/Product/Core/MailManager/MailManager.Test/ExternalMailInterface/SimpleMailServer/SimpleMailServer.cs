using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Enterprise.MailManager.MailFilters.Testing;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public abstract class SimpleMailServer : ISimpleMailServer
	{
		bool serverIsRunning;

		protected TcpListener listener;

		public abstract ushort Port { get; }

		internal static Dictionary<string, string> UserPasswordPairs = new ()
		{
			{ MailTestHelpers.Username1, MailTestHelpers.Password1 },
			{ MailTestHelpers.Username2, MailTestHelpers.Password2 }
		};

		internal static HashSet<string> UserPasswordBase64 = new ()
		{
			"AGNhcmdvd2lzZXVhdGNlbnRyYWx0ZXN0YWNjb3VudABDM250cjRsVDM1dA==",
			"AGFscGhhADZKbyVxOGR6"
		};

		internal static SimpleMailServer EmptyServer;

		public void Start()
		{
			if (!serverIsRunning)
			{
				var endPoint = new IPEndPoint(IPAddress.Any, Port);
				listener = new TcpListener(endPoint);
				listener.Start();

				serverIsRunning = true;
				while (serverIsRunning)
				{
					try
					{
						var client = listener.AcceptTcpClient();
						var handler = GetMailHandler(client);
						Greeting(handler);
						new Thread(handler.Handle).Start();
					}
					catch (Exception)
					{
						break;
					}
				}
			}
		}

		public abstract SimpleMailServerHandler GetMailHandler(TcpClient client);

		public abstract void Greeting(SimpleMailServerHandler handler);

		public void Stop()
		{
			serverIsRunning = false;
			listener?.Stop();
		}

		public void Dispose()
		{
			if (serverIsRunning)
			{
				Stop();
			}
		}
	}
}
