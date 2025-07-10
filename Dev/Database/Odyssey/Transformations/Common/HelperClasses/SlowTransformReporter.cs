using System;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public class SlowTransformReporter : IDisposable
	{
		public SlowTransformReporter(string transformName, TransformationSection section)
			: this(transformName, section, new SessionWaitStatsReporter("#WaitStatsForSlowTransformReporter"))
		{
		}

		public SlowTransformReporter(string transformName, TransformationSection section, ISessionWaitStatsReporter sessionWaitStatsReporter)
		{
			this.transformName = transformName;
			this.sessionWaitStatsReporter = sessionWaitStatsReporter;
			isOffline = section == TransformationSection.OfflinePostUpgrade || section == TransformationSection.OfflinePreUpgrade;
			sessionWaitStatsReporter.StartRecording();
			timer = new Stopwatch();
			timer.Start();
		}

		public void Dispose()
		{
			timer.Stop();

			if (isOffline)
			{
				var totalElapsedTimeSpan = timer.Elapsed;
				var threshold = SlowTimeThreshold.Value;
				if (totalElapsedTimeSpan > threshold)
				{
					using (ErrorReporter.GatherAdditionalInformation())
					{
						foreach (var (type, waitTime, taskCount, maxWaitTime, signalWaitTime) in sessionWaitStatsReporter.GetSessionWaits())
						{
							var key = type;
							if (type == "SOS_SCHEDULER_YIELD")
							{
								totalElapsedTimeSpan -= TimeSpan.FromMilliseconds(waitTime);
								key += " (wait time is already subtracted from transform execution time)";
							}

							ErrorReporter.SetAdditionalInfo("Wait Stats", key, $"wait_time_ms: {waitTime}, waiting_tasks_count: {taskCount}, max_wait_time_ms: {maxWaitTime}, signal_wait_time_ms: {signalWaitTime}");
						}

						if (totalElapsedTimeSpan > threshold)
						{
							ErrorReporter.ReportOnceWithAdditionalInfo($"SlowTransform: {transformName}", $"Transformation {transformName} has taken {totalElapsedTimeSpan} seconds which is over the allowed maximum of {threshold}");
						}
					}
				}
			}
		}

		readonly ISessionWaitStatsReporter sessionWaitStatsReporter;
		readonly Stopwatch timer;
		readonly string transformName;
		readonly bool isOffline;

		static Lazy<TimeSpan> SlowTimeThreshold = new Lazy<TimeSpan>(() => ReadRegistry());

		static TimeSpan ReadRegistry()
		{
			return TimeSpan.FromSeconds(DbRegistry.ReportSlowUpgradeTransformsSeconds.LoadValue(Db.Connection));
		}

		public static void ResetLazyRegistry()
		{
			SlowTimeThreshold = new Lazy<TimeSpan>(() => ReadRegistry());
		}
	}
}
