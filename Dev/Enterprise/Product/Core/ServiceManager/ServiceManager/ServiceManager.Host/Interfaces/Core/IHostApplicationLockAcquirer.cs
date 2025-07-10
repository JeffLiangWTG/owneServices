using System;

namespace Enterprise.ServiceManager.Host
{
	interface IHostApplicationLockAcquirer : IDisposable
	{
		void AcquireHostApplicationLock();
	}
}
