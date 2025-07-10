using System;
using System.Collections.Generic;

namespace ServiceManager.Integration.Abstractions
{
	public struct NudgeEventWithTime : IEquatable<NudgeEventWithTime>
	{
		readonly NudgeEventArgs eventArgs;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "ServiceManager.Shared.Abstractions should be CW1 independent")]
		public NudgeEventWithTime(NudgeEventArgs eventArgs)
		{
			this.eventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
			EventTime = DateTime.Now;
		}

		public DateTime EventTime { get; private set; }
		public string EventType => eventArgs.EventType;
		public string Description => eventArgs.Description;
		public IEnumerable<string> TaskCodes => eventArgs.TaskCodes;
		public int RetriesRemaining => eventArgs.RetriesRemaining;
		public override bool Equals(object? obj) => obj is NudgeEventWithTime other && Equals(other);
		public bool Equals(NudgeEventWithTime other) => this == other;
		public static bool operator !=(NudgeEventWithTime event1, NudgeEventWithTime event2) => !(event1 == event2);
		public static bool operator ==(NudgeEventWithTime event1, NudgeEventWithTime event2) => (event1.eventArgs == event2.eventArgs && event1.EventTime == event2.EventTime);
		public override int GetHashCode() => eventArgs.GetHashCode() ^ EventTime.GetHashCode();
	}

	public interface INudgingEventsTracker
	{
		IEnumerable<NudgeEventWithTime> Events { get; }
	}
}
