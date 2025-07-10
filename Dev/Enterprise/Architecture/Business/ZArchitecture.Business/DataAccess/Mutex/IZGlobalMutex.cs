using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Data.Mutex
{
	public interface IZGlobalMutex : IDisposable
	{
		ZBool HasLock { get; }
		ZBool IsLocked { get; }
		MutexID MutexID { get; }
		ZString RecordIdentifier { get; }

		LockInfo GetLockInfo();
		bool Lock();
		void Unlock();
	}
}
