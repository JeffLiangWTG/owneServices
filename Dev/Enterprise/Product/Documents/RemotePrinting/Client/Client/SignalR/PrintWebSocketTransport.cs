using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Enterprise.RemotePrinting.Client
{
	public class PrintWebSocketTransport : WebSocketTransport
	{
		public PrintWebSocketTransport(IHttpClient client)
			: base(client)
		{
		}

		public override void LostConnection(IConnection connection)
		{
			if (connection != null && connection.Transport != null && isDisposed)
			{
				return;
			}

			base.LostConnection(connection);
		}

		protected override void Dispose(bool disposing)
		{
			isDisposed = disposing;
			base.Dispose(disposing);
		}

		bool isDisposed;
	}
}
