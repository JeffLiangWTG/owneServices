using ServiceManager.Integration.ServiceHostClient.DataContracts;

namespace ServiceManager.Logging.Abstractions
{
	public interface IQueueMonitorLogger
	{
		void Log(QueueDTO queue);
	}
}
