using System;

namespace ServiceManager.Runner.Abstractions
{
	public record NextRunTimeResult
	{
		public NextRunTimeResult(bool wasSkipped, DateTime nextRunTime, bool noTaskFound = false)
		{
			WasSkipped = wasSkipped;
			NextRunTime = nextRunTime;
			NoTaskFound = noTaskFound;
		}

		public static NextRunTimeResult Empty { get; } = new (wasSkipped: false, DateTime.MinValue, noTaskFound: true);

		public bool NoTaskFound { get; }
		public bool WasSkipped { get; }
		public DateTime NextRunTime { get; }
	}
}
