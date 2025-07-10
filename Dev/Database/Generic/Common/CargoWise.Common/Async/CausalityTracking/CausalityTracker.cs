using System;
using System.Runtime.CompilerServices;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common.Async.CausalityTracking
{
	public static class CausalityTracker
	{
		[ThreadSafe]
		static readonly ConditionalWeakTable<Exception, string> exceptionDependencyChains = new ConditionalWeakTable<Exception, string>();

		/// <summary>
		/// Get the associated dependency chain (chain of callstacks) for an asynchronous exception
		/// </summary>
		/// <param name="ex">Exception to retrieve dependency chain for</param>
		/// <param name="dependencyChainedStack">The chain of call stacks as a string</param>
		/// <returns>True if an associated chain was found</returns>
		public static bool TryGetDependencyChainedStack(Exception ex, out string dependencyChainedStack)
		{
			return exceptionDependencyChains.TryGetValue(ex, out dependencyChainedStack);
		}

		/// <summary>
		/// Add a dependency chained stack for a given exception instance
		/// </summary>
		/// <param name="ex">The exception</param>
		/// <param name="dependencyChainedStack">The callstack</param>
		public static void AddDependencyChainedStack(Exception ex, string dependencyChainedStack)
		{
			bool append = true;
			var currentStack = exceptionDependencyChains.GetValue(
				ex,
				(exRef) =>
				{
					append = false;
					return ex.StackTrace + dependencyChainedStack;
				});

			if (append)
			{
				exceptionDependencyChains.Remove(ex);
				exceptionDependencyChains.Add(ex, currentStack + dependencyChainedStack);
			}
		}
	}
}
