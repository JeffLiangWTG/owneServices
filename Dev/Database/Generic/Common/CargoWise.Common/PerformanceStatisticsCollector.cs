using System;
using CargoWise.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Common
{
	public enum EnabledState
	{
		NotLoggedIn, Disabled, Simple, Detailed
	}

	/// <summary>
	/// A collector of runtime performance statistics.
	/// </summary>
	public interface IPerformanceStatisticsCollector
	{
		IDisposable StartMonitoring(string statisticName, string subName = null);
		IDisposable Exclude();
		void AttemptFlush(bool flush = false);
		EnabledState StatisticMode { get; }
		bool IsMonitoring { get; }
	}

	/// <summary>
	/// A generic collector of runtime performance statistics.
	/// </summary>
	public static partial class PerformanceStatisticsCollector
	{
		[ThreadStatic]
		internal static IPerformanceStatisticsCollector instance;

		public static bool IsCallerMonitoring
		{
			get { return isCallerMonitoring; }
			set { isCallerMonitoring = value; }
		}

		[ThreadStatic]
		static bool isCallerMonitoring;

		public static bool CalculateIsMonitoring()
		{
			return isCallerMonitoring || (instance?.IsMonitoring ?? false);
		}

		public static void AttemptFlush(bool force = false)
		{
			if (instance != null)
			{
				instance.AttemptFlush(force);
			}
		}

		internal class NullPerformanceStatisticsCollector : IPerformanceStatisticsCollector
		{
			public IDisposable StartMonitoring(string statisticName, string subName = null)
			{
				return DisposableAction.NoAction;
			}

			public IDisposable Exclude()
			{
				return DisposableAction.NoAction;
			}

			public void AttemptFlush(bool flush = false)
			{
			}

			public EnabledState StatisticMode
			{
				get { throw new NotSupportedException(); }
			}

			public bool IsMonitoring => false;
		}

		/// <summary>
		/// Start monitoring runtime statistics for a particular region of code.
		/// </summary>
		public static IDisposable StartMonitoring(string statisticName, string subName = null)
		{
			return ConditionallyProcessFunc(x => x.StartMonitoring(statisticName, subName));
		}

		public static IDisposable StartMonitoring(string statisticName, Type type)
		{
			return ConditionallyProcessFunc(x => x.StartMonitoring(statisticName, type.FullName));
		}

		/// <summary>
		/// Pause the current regions timer for a particular subregion of code
		/// </summary>
		public static IDisposable Exclude()
		{
			return ConditionallyProcessFunc(x => x.Exclude());
		}

		static IDisposable ConditionallyProcessFunc(Func<IPerformanceStatisticsCollector, IDisposable> func)
		{
			Argument.NotNull(func, nameof(func));

			if (suspendedCount.Value > 0)
			{
				return DisposableAction.NoAction;
			}

			if (instance == null)   // not yet sure whether we really want statistics
			{
				var potentialInstance = default(IPerformanceStatisticsCollector);
				if (GlobalServiceProvider.TryGetInstance(out var serviceProvider))
				{
					potentialInstance = serviceProvider.GetRequiredService<IPerformanceStatisticsCollector>();
				}

				if (potentialInstance == null || potentialInstance.StatisticMode == EnabledState.NotLoggedIn)
				{
					return DisposableAction.NoAction;
				}
				instance = ((potentialInstance.StatisticMode == EnabledState.Detailed) || (potentialInstance.StatisticMode == EnabledState.Simple)) ? potentialInstance : new NullPerformanceStatisticsCollector();
			}
			return func(instance);
		}

		public static IDisposable SuspendStatisticsCollector()
		{
			suspendedCount.Value++;
			return new DisposableAction(() => suspendedCount.Value--);
		}

		static readonly Overridable<int> suspendedCount = new Overridable<int>();

		public static void ResetInstance()
		{
			instance = null;
		}
	}
}
