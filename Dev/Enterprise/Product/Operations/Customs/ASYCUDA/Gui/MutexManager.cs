using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public class MutexManager : IDisposable
	{
		public MutexManager(IEnumerable<AsycudaBill> bills, Func<AsycudaBill, ZGlobalMutex> getMutex)
		{
			billsMutex = new List<ZGlobalMutex>();
			var lockedBills = new List<AsycudaBill>();
			unableToLockMutex = null;
			foreach (var bill in bills)
			{
				var mutex = getMutex(bill);
				if (mutex.HasLock || mutex.Lock())
				{
					lockedBills.Add(bill);
					billsMutex.Add(mutex);
				}
				else if (unableToLockMutex == null)
				{
					unableToLockMutex = mutex;
				}
			}
			SucessfulLockedBills = lockedBills.ToArray();
		}
		public readonly IEnumerable<AsycudaBill> SucessfulLockedBills;

		public bool HasAquiredLockForAllBills => unableToLockMutex == null && SucessfulLockedBills.Any();
		public string GetMutexLockByInfo() => unableToLockMutex == null ? string.Empty : unableToLockMutex.GetMutexLockByInfo();

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				billsMutex.ForEach(x =>
				{
					if (x.HasLock)
					{
						x.Unlock();
					}
				});
				billsMutex.Clear();
			}
		}

		readonly ZGlobalMutex unableToLockMutex;
		readonly List<ZGlobalMutex> billsMutex;
	}
}
