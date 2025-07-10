using System.Collections.Generic;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public interface ITaskQueue
	{
		bool TryDequeueTask(out ITaskRunRequest dequeued);
		bool EnqueueTask(ITaskRunRequest task);
		void EmptyQueue();
		IEnumerable<IRunnableServiceTask> GetQueueSnapshot();
	}
}
