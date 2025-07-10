using System;

namespace Enterprise.Semaphores.Common
{
	public interface ISemaphoreInfo
	{
		/// <summary>
		/// Owner Heartbeat Session Information.
		/// </summary>
		IHeartbeatInfo OwnerSession { get; }

		/// <summary>
		/// Semaphore Type Identifier.
		/// </summary>
		ISemaphoreType Semaphore { get; }

		/// <summary>
		/// Date/Time handle was created.
		/// </summary>
		DateTime CreateTimeUtc { get; }
	}
}
