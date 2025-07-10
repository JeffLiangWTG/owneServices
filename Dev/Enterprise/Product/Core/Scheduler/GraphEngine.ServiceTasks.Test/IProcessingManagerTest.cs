using System;
using System.Threading;
using Enterprise.BatchProcessor;

namespace Enterprise.GraphEngine.ServiceTasks.Testing
{
	public class ProcessingManagerForTesting : IProcessingManager
	{
		public LoggingInformation Logger { get; set; }
		public Action<CancellationToken> ExecuteBatchForTesting;
		public void ExecuteBatch(CancellationToken token) => ExecuteBatchForTesting?.Invoke(token);
		public Action DisposeForTesting;
		public void Dispose() => DisposeForTesting?.Invoke();
	}
}
