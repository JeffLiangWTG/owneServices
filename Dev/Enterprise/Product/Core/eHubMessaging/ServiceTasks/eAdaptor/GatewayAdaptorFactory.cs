using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	internal class GatewayAdaptorFactory : IAdaptorFactory
	{
		public GatewayAdaptorFactory()
		{
		}

		public AdapterType AdapterType => AdapterType.GatewayAdapter;

		public IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress)
		{
			return string.IsNullOrWhiteSpace(serverAddress) ? null : new eHubAdapter(serverAddress, licenceCode, password);
		}
	}
}
