
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.ZArchitecture.PlugIn
{
	public abstract class ZMutexedPlugIn : ZAlwaysLoadPlugIn
	{
		protected ZMutexedPlugIn(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			hostBusinessEntity.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}

		public abstract ZGlobalMutex Mutex { get; }

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				UnlockMutexIfLockedByThisInstance();
			}
		}

		protected void UnlockMutexIfLockedByThisInstance()
		{
			if (Mutex.HasLock)
			{
				Mutex.Unlock();
			}
		}

		protected void UnHookFactorySaveEvent()
		{
			HostBusinessEntity.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					UnlockMutexIfLockedByThisInstance();
					UnHookFactorySaveEvent();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}
