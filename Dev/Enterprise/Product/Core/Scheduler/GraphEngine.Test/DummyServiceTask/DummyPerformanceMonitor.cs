using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	public class DummyPerformanceMonitor : PerformanceMonitor
	{
		public DummyPerformanceMonitor(TimeSpan criticalDuration, TimeSpan resetDuration)
			: base(criticalDuration, resetDuration)
		{
		}

		public (ZDateTime, int) PerformanceLevel { get; set; }
		public Func<ZDateTime, int, bool> ShouldChangeLevel { get; set; }
		public IStopwatch StopWatch { get; set; }
		protected override IStopwatch GetStopwatch() => StopWatch ?? new UberWatch();

		protected override void OnSlowQueryCore()
		{
			var (timeOfChange, level) = PerformanceLevel;
			if (ShouldChangeLevel?.Invoke(timeOfChange, level) ?? false)
			{
				PerformanceLevel = (ZDateTime.UtcNow, level + 1);
			}
		}

		protected override void Reset()
		{
			if (PerformanceLevel.Item2 > 0)
			{
				PerformanceLevel = (ZDateTime.UtcNow, 0);
			}
		}
	}

	public class DummyStopwatch : IStopwatch
	{
		public bool IsRunning => false;
		public long ElapsedMilliseconds { get; set; }
		public TimeSpan Elapsed { get; set; }
		public void Restart() { }
		public void Start() { }
		public void Stop() { }
	}
}
