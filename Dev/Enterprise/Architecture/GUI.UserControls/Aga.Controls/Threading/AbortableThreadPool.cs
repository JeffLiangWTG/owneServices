// Stephen Toub
// stoub@microsoft.com

using System;
using System.Collections.Generic;
using System.Threading;

namespace Aga.Controls.Threading
{
	public class AbortableThreadPool
	{
		private readonly LinkedList<WorkItem> _callbacks = new LinkedList<WorkItem>();
		private readonly Dictionary<WorkItem, Thread> _threads = new Dictionary<WorkItem, Thread>();

		public WorkItem QueueUserWorkItem(WaitCallback callback, object state)
		{
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));

			WorkItem item = new WorkItem(callback, state, ExecutionContext.Capture());
			lock (_callbacks)
			{
				_callbacks.AddLast(item);
			}
			ThreadPool.QueueUserWorkItem(new WaitCallback(HandleItem));
			return item;
		}

		private void HandleItem(object ignored)
		{
			WorkItem item = null;
			try
			{
				lock (_callbacks)
				{
					if (_callbacks.Count > 0)
					{
						item = _callbacks.First.Value;
						_callbacks.RemoveFirst();
					}
					if (item == null)
						return;
					_threads.Add(item, Thread.CurrentThread);

				}
				ExecutionContext.Run(item.Context,
					delegate
					{ item.Callback(item.State); }, null);
			}
			finally
			{
				lock (_callbacks)
				{
					if (item != null)
						_threads.Remove(item);
				}
			}
		}

		public bool IsMyThread(Thread thread)
		{
			lock (_callbacks)
			{
				foreach (Thread t in _threads.Values)
				{
					if (t == thread)
						return true;
				}
				return false;
			}
		}

		public void CancelAll(bool allowAbort)
		{
			lock (_callbacks)
			{
				_callbacks.Clear();
				if (allowAbort)
				{
#if NETFRAMEWORK
					foreach (Thread t in _threads.Values)
						t.Abort();
#endif
					_threads.Clear();
				}
			}
		}
	}
}
