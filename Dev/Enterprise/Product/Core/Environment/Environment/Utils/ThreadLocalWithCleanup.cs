using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Common;

namespace Enterprise.Environment
{
	class ThreadLocalWithCleanup<T> : IDisposable
		where T : class
	{
		public ThreadLocalWithCleanup(Func<T> createNew)
		{
			this.createNew = createNew;
		}

		readonly Func<T> createNew;
		readonly ThreadLocal<T> threadLocal = new ThreadLocal<T>();
		readonly ConcurrentHashSet<WeakReference<T>> items = new ConcurrentHashSet<WeakReference<T>>();

		#region Public API

		public bool IsValueCreated
		{
			get { return threadLocal.Value != null; }
		}

		public T Value
		{
			get
			{
				if (threadLocal.Value == null)
				{
					threadLocal.Value = CreateNew();
				}

				return threadLocal.Value;
			}
		}

		public IEnumerable<T> Values
		{
			get
			{
				foreach (var weakItem in items)
				{
					T item;

					if (weakItem.TryGetTarget(out item))
					{
						yield return item;
					}
				}
			}
		}

		public void ClearCurrentThread()
		{
			var context = threadLocal;
			if (context != null)
			{
				context.Value = null;
			}
		}

		#endregion

		#region Implementation

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "We take the 'Create' as an oppertunity to clean out the deleted items, but the add/remove here are fully independant operations that do not depend on eachothers state.")]
		T CreateNew()
		{
			var result = createNew();

			foreach (var item in items.Where(IsCollected).ToArray())
			{
				items.TryRemove(item);
			}

			items.TryAdd(new WeakReference<T>(result));
			return result;
		}

		bool IsCollected(WeakReference<T> arg)
		{
			T value;
			return !arg.TryGetTarget(out value);
		}

		#endregion

		#region IDisposable 

		bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					threadLocal.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
