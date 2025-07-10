using System;
using CargoWise.Common;
using Enterprise.Scheduler.GraphEngine;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public class EDIMessagePerformanceMonitor : PerformanceMonitor
	{
		public EDIMessagePerformanceMonitor(TimeSpan criticalDuration, TimeSpan resetDuration)
			: base(criticalDuration, resetDuration)
		{
		}

		protected override IStopwatch GetStopwatch() => new UberWatch();
		protected override void OnSlowQueryCore() { }
		protected override void Reset() { }
	}
}
