using System;
using System.Diagnostics;
using System.Globalization;
using Enterprise.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.LogWalker.Internals
{
	/// <summary>
	/// The stop watch lets us yield to the top level handler as a kludge for avoiding memory issues.
	/// </summary>
	class LogWalkerStopWatch
	{
		public LogWalkerStopWatch()
		{
			stopwatch = Stopwatch.StartNew();
			maxDuration = TimeSpan.FromSeconds(SystemDataRegistry.Instance.SecondsUntilLogWalkerEnds.Value);
		}
		readonly Stopwatch stopwatch;
		readonly TimeSpan maxDuration;

		public bool CanRunNextSubscriber(ILogger notifier)
		{
			if (stopwatch.Elapsed < maxDuration)
			{
				return true;
			}
			else
			{
				notifier?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Yielding, as maximum run time of {0} seconds for the service task has been exceeded.", maxDuration.TotalSeconds));
				return false;
			}
		}
	}
}
