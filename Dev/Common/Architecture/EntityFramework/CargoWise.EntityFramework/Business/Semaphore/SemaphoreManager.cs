using System;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.EntityFramework
{
	public sealed class SemaphoreManager : IDisposable
	{
		public SemaphoreManager(Semaphore semaphoreItem)
		{
			Argument.NotNull(semaphoreItem, "semaphoreItem");

			DisposableLeakListener.Instance.RegisterDisposable(this);
			SemaphoreItem = semaphoreItem;
			SemaphoreItem.Increment();
		}

		public void Dispose()
		{
			if (SemaphoreItem != null)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				SemaphoreItem.Decrement();
				SemaphoreItem = null;
			}
		}

		ISemaphoreItemInternals SemaphoreItem;
	}
}
