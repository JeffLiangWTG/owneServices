using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Common.MemoryManagement.Internal;

namespace CargoWise.Common.MemoryManagement
{
	/// <summary>
	/// The action to be performed when requesting a memory flush
	/// </summary>
	public enum FlushAction
	{
		/// <summary>
		/// Reclaim only one unit of reclaimable objects
		/// </summary>
		Partial,
		/// <summary>
		/// Reclaim all possible memory in one action
		/// </summary>
		Full
	}

	/// <summary>
	/// Returns the result of a flush action
	/// </summary>
	public enum FlushResult
	{
		/// <summary>
		/// Some memory may have been made elegible for garbage collection, but it is known further calls may reclaim more memory
		/// </summary>
		Partial,

		/// <summary>
		/// No further reclaims are possible on this object
		/// </summary>
		Exhausted,

		/// <summary>
		/// The call is not required as memory pressure does not exists
		/// </summary>
		NotRequired
	}

	public enum FlushCallback
	{
		/// <summary>
		/// Indicates the object is thread safe and can be collected on any thread
		/// </summary>
		OnAnyThread,

		/// <summary>
		/// Indicates the object must be collected only on the thread on which it was registered
		/// </summary>
		OnRegisteringThread
	}

	public delegate FlushResult StaticReclaimWrapper(FlushAction flushAction);

	public delegate FlushResult ReclaimWrapper<T>(T target, FlushAction flushAction);

	/// <summary>
	/// MemoryManager is a registration class to control the purging of long lived object contents.
	/// Register all static and threadstatic objects - also register non static caches that may contain large numbers of objects
	/// The cleanup action should only release objects that do not affect the visible effect of each class.
	/// This means you can throw away cached values, but this must not result in an external behaviour change.
	/// </summary>
	public static class MemoryManager
	{
		public static void Enable()
		{
			if (Interlocked.CompareExchange(ref reclaimer, new Reclaimer(), null) != null)
			{
				throw new InvalidOperationException("Memory Management is already enabled");
			}
		}

		/// <summary>
		/// Completely disables the memory manager so that it does not affect the system
		/// By default memory management is enabled.
		/// Will be automatically re-enabled for each new appdomain/process.
		/// </summary>
		public static void Disable()
		{
			reclaimer = null;
		}

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
		public static void Register(string description, FlushCallback instanceType, StaticReclaimWrapper cleanupAction)
		{
			var currentReclaimer = reclaimer;
			if (currentReclaimer != null)
			{
				currentReclaimer.Register(description, instanceType, cleanupAction);
			}
		}

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
		public static void Register<T>(string description, T target, FlushCallback instanceType, ReclaimWrapper<T> cleanupAction)
		{
			Argument.NotNull(cleanupAction, nameof(cleanupAction));
			var currentReclaimer = reclaimer;
			if (currentReclaimer != null)
			{
				currentReclaimer.Register(description, target, instanceType, cleanupAction);
			}
		}

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
		public static IEnumerable<IReclaimable> Examine()
		{
			var currentReclaimer = reclaimer;
			if (currentReclaimer != null)
			{
				return currentReclaimer.Examine();
			}
			else
			{
				return new List<IReclaimable>();
			}
		}

		/// <summary>
		/// Attempts to reduce memory usage to the desired size
		/// </summary>
		/// <param name="flushAction"></param>
		/// <param name="targetMemorySizeInBytes"></param>
		/// <returns></returns>
		public static FlushResult Flush(FlushAction flushAction, long targetMemorySizeInBytes)
		{
			if (reclaimer != null)
			{
				return reclaimer.Flush(flushAction, targetMemorySizeInBytes);
			}
			else
			{
				return FlushResult.Exhausted;
			}
		}

		/// <summary>
		/// This is the production reclaimer.
		/// Tests will instantiate the Reclaimer manually
		/// </summary>
		static Reclaimer reclaimer;
	}
}
