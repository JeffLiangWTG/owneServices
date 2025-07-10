using System;
using System.Threading;

namespace CargoWise.Common.MemoryManagement.Internal
{
	class ReclaimableHolder
	{
		public ReclaimableHolder(Reclaimable reclaimable, FlushCallback flushCallBack)
		{
			Argument.NotNull(reclaimable, nameof(reclaimable));
			Reclaimable = reclaimable;
			this.flushCallBack = flushCallBack;
			this.managedThreadId = Thread.CurrentThread.ManagedThreadId;
			this.appDomainId = AppDomain.CurrentDomain.Id;
		}

		public bool IsEligibleForCurrentThread
		{
			get { return flushCallBack == FlushCallback.OnAnyThread || (Thread.CurrentThread.ManagedThreadId == managedThreadId && AppDomain.CurrentDomain.Id == appDomainId); }
		}

		public bool IsAlive
		{
			get
			{
				Argument.NotNull(this.Reclaimable, nameof(this.Reclaimable)); // Suggested By ReviewBot 
				return Reclaimable.IsAlive;
			}
		}

		public Reclaimable Reclaimable { get; private set; }
		readonly int managedThreadId;
		readonly int appDomainId;
		readonly FlushCallback flushCallBack;
	}
}
