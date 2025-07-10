using System;
using System.Diagnostics;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core.Telemetry;

public class TelemetryPerformanceStatisticsCollector : IPerformanceStatisticsCollector
{
	public const string SourceName = "CargoWise.Internal";
	public ActivitySource ActivitySource { get; } = new ActivitySource(SourceName);
	public EnabledState StatisticMode => IsMonitoring ? EnabledState.Simple : EnabledState.Disabled;

	public bool IsMonitoring => ActivitySource.HasListeners();

	public void AttemptFlush(bool flush = false)
	{
	}

	public IDisposable Exclude() => DisposableAction.NoAction;

	public IDisposable StartMonitoring(string statisticName, string subName = null)
	{
		return ActivitySource.StartActivity(statisticName + subName);
	}
}
