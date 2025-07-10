using Enterprise.Semaphores.Common;

namespace Enterprise.ZArchitecture.Data.Mutex
{
	class ZGlobalMutexSemaphore : ISemaphoreType
	{
		#region Testing
#if DEBUG
		public ZGlobalMutexSemaphore() : this(string.Empty, string.Empty) { }
#endif
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Mutex Lock Name")]
		public ZGlobalMutexSemaphore(string mutexId, string key)
		{
			this.lockInfo = string.Format("Mutex:{0}:{1}", mutexId, key);
		}

		#region ISemaphoreType Members

		string ISemaphoreType.Category
		{
			get { return "MTX"; }
		}

		string ISemaphoreType.LockInfo
		{
			get { return lockInfo; }
		}

		readonly string lockInfo;

		int ISemaphoreType.MaxConcurrentHandles
		{
			get { return 1; }
		}

		#endregion
	}
}
