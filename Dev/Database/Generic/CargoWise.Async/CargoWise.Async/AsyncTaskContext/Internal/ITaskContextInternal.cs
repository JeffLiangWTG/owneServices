using System;
using System.Threading.Tasks;
using CargoWise.Async.AsyncTaskContext.Public;

namespace CargoWise.Async.AsyncTaskContext.Internal
{
	interface ITaskContextInternal : ITaskContext
	{
		bool CausalityTracking { get; }
		Task EnqueueTask(Action runTask);
		void EnqueueSelfObservingTask(Action runTask);
	}
}
