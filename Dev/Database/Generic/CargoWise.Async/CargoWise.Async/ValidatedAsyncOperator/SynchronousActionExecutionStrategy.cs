using System;
using System.Threading;

namespace CargoWise.Async
{
	public class SynchronousActionExecutionStrategy : IActionExecutionStrategy
	{
		void IActionExecutionStrategy.StartOperation(Action action)
		{
			action();
		}

		void IActionExecutionStrategy.DoParallelisableTransform(Action action, CancellationTokenSource cancellationTokenSource)
		{
			action();
		}

		void IActionExecutionStrategy.SynchroniseIntoMainContext(Action action)
		{
			action();
		}
	}
}
