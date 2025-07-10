using System.Diagnostics;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core.Telemetry;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Test.Telemetry;
class TelemetryPerformanceStatisticsCollectorTest : TestCase
{
	public void TestTelemetryPerformanceStatisticsCollector()
	{
		using var listener = new ActivityListener();
		Activity activityStarted = null;
		listener.ActivityStarted = activity => activityStarted = activity;
		listener.ShouldListenTo = source => source.Name == TelemetryPerformanceStatisticsCollector.SourceName;
		listener.Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded;
		ActivitySource.AddActivityListener(listener);

		IPerformanceStatisticsCollector collector = new TelemetryPerformanceStatisticsCollector();
		using (var activity = collector.StartMonitoring("Name"))
		{
		}

		Assert(collector.IsMonitoring);
		AssertEquals(EnabledState.Simple, collector.StatisticMode);
		AssertNotNull(activityStarted);
		AssertEquals("Name", activityStarted.OperationName);
	}
}
