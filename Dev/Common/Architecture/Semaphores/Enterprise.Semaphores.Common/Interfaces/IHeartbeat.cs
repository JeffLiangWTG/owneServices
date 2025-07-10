using System;

namespace Enterprise.Semaphores.Common
{
	public interface IHeartbeat : IDisposable
	{
		/// <summary>
		/// This heartbeat session information
		/// </summary>
		IHeartbeatInfo Info { get; }

		/// <summary>
		/// Uniquely identifies a heartbeat
		/// </summary>
		Guid HeartbeatId { get; }

		/// <summary>
		/// The minimum interval of database polling from upgrade checker or heartbeat updates
		/// </summary>
		int KeepAliveIntervalMs { get; }

		/// <summary>
		/// Updates heartbeat session information to reflect a new logged in user
		/// </summary>
		void UpdateUserContext(IHeartbeatInfoFactory heartbeatInfoFactory);

		bool RegisterIfUserPkNotAlreadyInDatabase();

		IDisposable TemporarilyDisableTimers();

#if DEBUG
		void PumpIfNeeded();
#endif
	}
}
