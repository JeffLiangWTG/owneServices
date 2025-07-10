using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public sealed class RestOutboundAdapterOAuthRegistry : RestOutboundAdapterOAuth
	{
		public RestOutboundAdapterOAuthRegistry(string serverAddress, INotifications notifier, IOAuth2Parameters oAuth2Parameters) : base(serverAddress, notifier, oAuth2Parameters, null)
		{
		}

		protected override bool ShouldSetAuthorizationHeader => eAdaptorRegistry.Instance.eAdaptorNextOutbound.Value.IsOAuth2Enabled;
	}
}
