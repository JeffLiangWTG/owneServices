using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Queue
{
	public interface IQueueStatusProviderFactory
	{
		IQueueStatusProvider Create(ITaskStatusProvider statusProvider);
	}
}
