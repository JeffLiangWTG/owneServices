using System.Net.Http;
using Microsoft.AspNet.SignalR.Client.Http;

namespace Enterprise.RemotePrinting.Client
{
	public class PrintDefaultHttpClient : DefaultHttpClient
	{
		protected override HttpMessageHandler CreateHandler()
		{
			var handler = base.CreateHandler();
			if (handler is HttpClientHandler httpHandler)
			{
				httpHandler.AllowAutoRedirect = false;
			}
			return handler;
		}
	}
}
