using System;
using CargoWise.Common;

namespace Enterprise.Scheduler.GraphEngine
{
	public abstract class PerformanceMonitor
	{
		protected PerformanceMonitor(TimeSpan criticalDuration, TimeSpan resetDuration)
		{
			this.criticalDuration = criticalDuration;
			this.resetDuration = resetDuration;
		}

		readonly TimeSpan criticalDuration;
		readonly TimeSpan resetDuration;

		public IDisposable MonitorQuery()
		{
			var timer = GetStopwatch();
			timer.Start();
			return new DisposableAction(() =>
			{
				var elapsed = timer.ElapsedMilliseconds;
				if (elapsed > criticalDuration.TotalMilliseconds)
				{
					OnSlowQueryCore();
				}
				else if (elapsed < resetDuration.TotalMilliseconds)
				{
					Reset();
				}
			});
		}

		protected abstract void OnSlowQueryCore();
		protected abstract void Reset();
		protected abstract IStopwatch GetStopwatch();
	}
}
