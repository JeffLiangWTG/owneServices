using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common;

public class CompositePerformanceStatisticsCollector : IPerformanceStatisticsCollector
{
	readonly IPerformanceStatisticsCollector[] collectors;

	public CompositePerformanceStatisticsCollector(params IPerformanceStatisticsCollector[] collectors)
	{
		this.collectors = collectors;
	}

	//Non param constructors for ObjectFactory xml configuration
	//Our xml config doesn't support array constructor args
	public CompositePerformanceStatisticsCollector() : this(Array.Empty<IPerformanceStatisticsCollector>())
	{
	}

	public CompositePerformanceStatisticsCollector(IPerformanceStatisticsCollector collector1)
		: this(new IPerformanceStatisticsCollector[] { collector1 })
	{
	}

	public CompositePerformanceStatisticsCollector(IPerformanceStatisticsCollector collector1, IPerformanceStatisticsCollector collector2)
		: this(new IPerformanceStatisticsCollector[] { collector1, collector2 })
	{
	}

	public CompositePerformanceStatisticsCollector(IPerformanceStatisticsCollector collector1, IPerformanceStatisticsCollector collector2, IPerformanceStatisticsCollector collector3)
		: this(new IPerformanceStatisticsCollector[] { collector1, collector2, collector3 })
	{
	}

	public CompositePerformanceStatisticsCollector(IPerformanceStatisticsCollector collector1, IPerformanceStatisticsCollector collector2, IPerformanceStatisticsCollector collector3, IPerformanceStatisticsCollector collector4)
		: this(new IPerformanceStatisticsCollector[] { collector1, collector2, collector3, collector4 })
	{
	}

	public CompositePerformanceStatisticsCollector(IPerformanceStatisticsCollector collector1, IPerformanceStatisticsCollector collector2, IPerformanceStatisticsCollector collector3, IPerformanceStatisticsCollector collector4, IPerformanceStatisticsCollector collector5)
		: this(new IPerformanceStatisticsCollector[] { collector1, collector2, collector3, collector4, collector5 })
	{
	}

	public IReadOnlyList<IPerformanceStatisticsCollector> Collectors => new List<IPerformanceStatisticsCollector>(collectors);
	public EnabledState StatisticMode => collectors.Max(c => c.StatisticMode);

	public bool IsMonitoring => collectors.Any(c => c.IsMonitoring);

	public void AttemptFlush(bool flush = false)
	{
		foreach (var collector in collectors)
		{
			collector.AttemptFlush(flush);
		}
	}

	public IDisposable Exclude()
	{
		var disposables = collectors.Select(c => c.Exclude()).ToArray();
		return Combine(disposables);
	}

	public IDisposable StartMonitoring(string statisticName, string subName = null)
	{
		var disposables = collectors.Select(c => c.StartMonitoring(statisticName, subName)).ToArray();
		return Combine(disposables);
	}

	DisposableAction Combine(IDisposable[] disposables)
	{
		return new DisposableAction(() =>
		{
			foreach (var disposable in disposables)
			{
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		});
	}
}
