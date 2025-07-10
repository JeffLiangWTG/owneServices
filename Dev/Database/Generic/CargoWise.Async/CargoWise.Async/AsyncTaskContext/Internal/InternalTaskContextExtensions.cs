using System;
using CargoWise.Common.Async.CausalityTracking;

namespace CargoWise.Async.AsyncTaskContext.Internal
{
	static class InternalTaskContextExtensions
	{
		internal static void AddDependencyChainedStack(this Exception ex, string callStack)
		{
			CausalityTracker.AddDependencyChainedStack(ex, "\n[Chained Frame]\n" + callStack); // Log information
		}
	}
}
