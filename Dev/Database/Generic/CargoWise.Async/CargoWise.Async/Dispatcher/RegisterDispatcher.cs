using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace CargoWise.Async
{
	public static class RegisterDispatcher
	{
		public static void Register()
		{
			var dispatcher = SynchronizationContext.Current;

			if (dispatcher == null)
			{
				return;
			}

			var threadId = Thread.CurrentThread.ManagedThreadId;

			threadDispatchers.TryAdd(threadId, dispatcher);
		}

		public static void Deregister()
		{
			((IDictionary)threadDispatchers).Remove(Thread.CurrentThread.ManagedThreadId);
		}

		public static IEnumerable<int> RegisteredThreadIds
		{
			get
			{
				return threadDispatchers.Select(entry => entry.Key).ToArray();
			}
		}

		public static SynchronizationContext GetDispatcher(int threadId)
		{
			SynchronizationContext result;
			threadDispatchers.TryGetValue(threadId, out result);
			return result;
		}

		public static bool IsDispatcherRegistered(int threadId)
		{
			return threadDispatchers.ContainsKey(threadId);
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static readonly ConcurrentDictionary<int, SynchronizationContext> threadDispatchers = new ConcurrentDictionary<int, SynchronizationContext>();
	}
}
