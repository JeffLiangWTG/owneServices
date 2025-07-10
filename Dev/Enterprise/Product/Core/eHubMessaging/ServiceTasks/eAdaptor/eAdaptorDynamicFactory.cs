using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	internal class eAdaptorDynamicFactory : IAdaptorFactory
	{
		public eAdaptorDynamicFactory(IEDICommunicationPartyConfig communicationPartyConfig)
		{
			this.communicationPartyConfig = communicationPartyConfig;
		}

		readonly IEDICommunicationPartyConfig communicationPartyConfig;

		public AdapterType AdapterType => AdapterType.Adapter;

		public IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress)
		{
			switch (communicationPartyConfig.Auth.AuthorizationMode)
			{
				case EDICommunicationAuthModesList.Codes.BasicAuthentication:
					return new RestOutboundAdapterBasic(serverAddress, notifier, communicationPartyConfig.Auth.Username, communicationPartyConfig.Auth.Password, communicationPartyConfig.Party.ECP_Name);

				case EDICommunicationAuthModesList.Codes.OAuthAuthentication:
					return new RestOutboundAdapterOAuth(serverAddress, notifier, communicationPartyConfig.Auth, communicationPartyConfig.Party.ECP_Name);

				case EDICommunicationAuthModesList.Codes.NoAuthentication:
					return new RestOutboundAdapterNoAuth(serverAddress, notifier, communicationPartyConfig.Party.ECP_Name);
			}

			return null; // Should never occur
		}
	}
}
