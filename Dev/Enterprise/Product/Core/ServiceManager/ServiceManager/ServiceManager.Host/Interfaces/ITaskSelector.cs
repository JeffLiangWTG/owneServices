using System.Collections.Generic;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public interface ITaskSelector
	{
		IEnumerable<ITaskRunRequest> SelectTasksToRun(IEnumerable<IRunnableServiceTask> tasks);
		IEnumerable<IRunnableServiceTask> GetTasksPotentiallyEligibleForRunning(IEnumerable<IRunnableServiceTask> tasks);
		bool ReachedMaxSecondary(IRunnableServiceTask runnableServiceTask);
	}
}