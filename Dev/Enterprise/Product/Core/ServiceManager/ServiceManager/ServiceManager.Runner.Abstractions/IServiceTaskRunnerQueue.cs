using System;
using System.Threading;
using ServiceManagerProto;

namespace ServiceManager.Runner.Abstractions
{
	public interface IServiceTaskRunnerQueue : IDisposable
	{
		void EnqueueResponse(ServiceTaskRunResponse commandInfo);
		ICommandInfo GetNextCommand();
		void CloseStream(bool runnerWasCancelled, CancellationToken cancellationToken);
	}
}
