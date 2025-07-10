using System;
using System.Threading;

namespace CargoWise.Async
{
	public interface IActionExecutionStrategy
	{
		void StartOperation(Action action);
		void DoParallelisableTransform(Action action, CancellationTokenSource cancellationTokenSource = null);
		void SynchroniseIntoMainContext(Action action);
	}
}