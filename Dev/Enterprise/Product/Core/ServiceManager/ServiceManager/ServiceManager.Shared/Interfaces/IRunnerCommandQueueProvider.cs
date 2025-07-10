using System;
using System.Threading;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Shared
{
	public interface IRunnerCommandQueueProvider : IDisposable
	{
		void Run(string assemblyName, string code, Guid requestId, string configString);
		void Run(string assemblyName, string code, Guid requestId, DateTime expectedNextRunTime, DateTime nextRunTime, string configString);
		void Stop();
		void Close();
		void StartResponseTask(Action<ServiceTaskRunResponse> responseAction, CancellationToken cancellationToken);
	}
}
