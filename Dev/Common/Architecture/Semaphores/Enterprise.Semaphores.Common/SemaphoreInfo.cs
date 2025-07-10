using System;
using CargoWise.Common;

namespace Enterprise.Semaphores.Common
{
	class SemaphoreInfo : ISemaphoreInfo
	{
		public SemaphoreInfo(IHeartbeatInfo ownerSession, ISemaphoreType semaphore, DateTime createTimeUtc)
		{
			Argument.NotNull(ownerSession, nameof(ownerSession));
			Argument.NotNull(semaphore, nameof(semaphore));

			this.ownerSession = ownerSession;
			this.semaphore = semaphore;
			this.createTimeUtc = createTimeUtc;
		}

		#region ISemaphoreInfo Members

		IHeartbeatInfo ISemaphoreInfo.OwnerSession
		{
			get { return ownerSession; }
		}
		readonly IHeartbeatInfo ownerSession;

		ISemaphoreType ISemaphoreInfo.Semaphore
		{
			get { return semaphore; }
		}
		readonly ISemaphoreType semaphore;

		DateTime ISemaphoreInfo.CreateTimeUtc
		{
			get { return createTimeUtc; }
		}
		readonly DateTime createTimeUtc;

		#endregion
	}
}
