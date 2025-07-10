using System.Net.Http;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.eHubMessaging.ServiceTasks.eAdaptor.eAdaptor;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public sealed class RestOutboundAdapterNoAuth : RestOutboundAdapterBase
	{
		public RestOutboundAdapterNoAuth(string serverAddress, INotifications notifier, string clientName) : base(serverAddress, notifier, clientName)
		{
		}

		protected override bool HandleSendMessageFailure(HttpClient client, IeHubMessage message, bool isCachedAuthHeaderValue, CancellationToken cancellationToken)
		{
			return false;
		}

		protected override bool ShouldSetAuthorizationHeader => false;

		protected override bool SetAuthorizationHeader(HttpClient client, CancellationToken cancellationToken)
		{
			return false;
		}
	}
}
