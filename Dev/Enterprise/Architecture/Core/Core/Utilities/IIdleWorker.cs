using System;

namespace Enterprise.ZArchitecture
{
	[Flags]
	public enum UserIdleWorkItemOptions
	{
		AllowDuringEdit = 1,
		AllowWhenFormInactive = 2,
		DisableSlowRunningWarning = 4,
		AllowAlways = AllowDuringEdit | AllowWhenFormInactive,
		None = 0,
	}

	public interface IIdleWorker
	{
		void QueueWorkItem(Delegate method, params object[] args);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		void QueueWorkItemWithOptions(int startDelayInMilliseconds, UserIdleWorkItemOptions options, Delegate method, params object[] args);
		void Flush();
	}
}
