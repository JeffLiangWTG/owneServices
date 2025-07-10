using System;
using System.ServiceModel;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.Registry.Business.eServices;
using ServiceConfiguration = CargoWise.eHub.Common.ServiceConfiguration;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	internal class eAdaptorFactory : IAdaptorFactory
	{
		public const int AdapterOutboxSizeLimit = 1000; // KB

		public eAdaptorFactory()
		{
		}

		public AdapterType AdapterType => AdapterType.Adapter;

		public IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress)
		{
			if (string.IsNullOrWhiteSpace(serverAddress))
			{
				return null;
			}
			else
			{
				var protocol = eAdaptorRegistry.Instance.OutboundCommunicationsProtocol.Value;
				switch (protocol)
				{
					case eAdaptorOutboundProtocolList.Codes.REST:
						return new RestOutboundAdapterOAuthRegistry(serverAddress, notifier, eAdaptorRegistry.Instance.eAdaptorNextOutbound.Value);

					case eAdaptorOutboundProtocolList.Codes.SOAP:
						return new eHubAdapter(new eAdaptorConfiguration(serverAddress), licenceCode, password, true);

					default:
						throw new NotImplementedException(FormattableString.Invariant($"Unknown protocol: {protocol}"));
				}
			}
		}

		public class eAdaptorConfiguration : ServiceConfiguration
		{
			public string ServerAddress { get; private set; }

			public eAdaptorConfiguration(string serverAddress)
			{
				ServerAddress = serverAddress;
			}

			public override EndpointAddress EndpointAddress
			{
				get { return new EndpointAddress(new Uri(ServerAddress)); }
			}
		}
	}
}
