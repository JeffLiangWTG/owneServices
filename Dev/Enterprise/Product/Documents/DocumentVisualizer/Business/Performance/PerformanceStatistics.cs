using System;
using CargoWise.Common;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class PerformanceStatistics
	{
		const string statisticsName = "DocumentVisualizer";

		public static IDisposable StartMonitoring(PerformanceMonitorArea area)
		{
			return PerformanceStatisticsCollector.StartMonitoring(statisticsName, area.ToString());
		}
	}
}