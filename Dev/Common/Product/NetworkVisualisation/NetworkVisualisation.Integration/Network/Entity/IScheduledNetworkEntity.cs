using System;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface IScheduledNetworkEntity : INetworkEntity
	{
		int ExplicitDurationMinutes { get; }

		int RemainingDurationMinutes { get; }

		DateTime ScheduledStartTimeLocal { get; }

		DateTime ScheduledEndTimeLocal { get; }

		bool IsCriticalPath { get; }
	}
}