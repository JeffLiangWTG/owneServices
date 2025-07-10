using ServiceManager.Integration.ServiceHostClient.DataContracts;

namespace ServiceManager.Host.Abstractions
{
	public interface IQueueStatusProvider
	{
		QueueListDTO GetQueueStatus();
	}
}
