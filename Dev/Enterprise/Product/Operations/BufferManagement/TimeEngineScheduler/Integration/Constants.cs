using System;

namespace Enterprise.TimeEngineScheduler.Integration
{
	public static class Constants
	{
		public static class TimeActionScheduleStatus
		{
			public const string Scheduled = "SCH";
			public const string Suspended = "SUS";
			public const string Closed = "CLS";
			public const string Failed = "ERR";
		}

		public static readonly byte MaxRetryAttempts = 3;
		public static readonly TimeSpan RetryAfterPeriod = new TimeSpan(0, 10, 0);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Result string")]
		public const string SchedulerActionSuccessResult = "Success";
	}
}
