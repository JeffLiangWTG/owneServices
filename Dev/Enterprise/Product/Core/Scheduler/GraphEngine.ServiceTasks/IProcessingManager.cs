using System;
using System.Threading;
using Enterprise.BatchProcessor;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public interface IProcessingManager : IDisposable
	{
		LoggingInformation Logger { get; }
		void ExecuteBatch(CancellationToken token);
	}
}
