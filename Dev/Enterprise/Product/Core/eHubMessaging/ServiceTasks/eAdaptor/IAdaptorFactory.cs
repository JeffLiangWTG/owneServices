using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	internal interface IAdaptorFactory
	{
		IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress);

		AdapterType AdapterType { get; }
	}

	internal enum AdapterType
	{
		Adapter,
		GatewayAdapter,
		Mock
	}
}
