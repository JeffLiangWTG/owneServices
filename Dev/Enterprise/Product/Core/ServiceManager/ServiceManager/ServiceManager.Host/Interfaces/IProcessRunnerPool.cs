using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public interface IProcessRunnerPool : IDisposable
	{
		IEnumerable<ServiceTaskCodeWithRunnerProcessId> GetRunnersSnapshot();
		int RunningCount(IRunnableServiceTask task = null);
		Task<IServiceRunner> GetOrCreateRunnerAsync(ITaskRunRequest request, ITaskScheduler taskScheduler, CancellationToken cancellationToken);
		void StopAllRunners(IRunnableServiceTask task);
		void WaitForRunningTasksToComplete(TimeSpan timeout);
		void Stop(TimeSpan timeout);
	}
}
