using System;
using System.Collections.Concurrent;
using CargoWise.Common;

namespace Enterprise.BufferManagement.Business.Test
{
	public class DummyPerformanceStatsCollector : IPerformanceStatisticsCollector
	{
		EnabledState IPerformanceStatisticsCollector.StatisticMode => EnabledState.Simple;

		void IPerformanceStatisticsCollector.AttemptFlush(bool flush)
		{
		}

		IDisposable IPerformanceStatisticsCollector.Exclude()
		{
			return DisposableAction.NoAction;
		}

		IDisposable IPerformanceStatisticsCollector.StartMonitoring(string statisticName, string subName)
		{
			var stat = new DummyStat { Name = statisticName, SubName = subName };

			CollectedStats.Add(stat);

			return new DisposableAction(() =>
			{
				stat.WasCompleted = true;
			});
		}

		bool IPerformanceStatisticsCollector.IsMonitoring => false;

		public ConcurrentBag<DummyStat> CollectedStats { get; } = new ConcurrentBag<DummyStat>();
	}

	public class DummyStat
	{
		public string Name { get; set; }
		public string SubName { get; set; }
		public bool WasCompleted { get; set; }
	}
}
