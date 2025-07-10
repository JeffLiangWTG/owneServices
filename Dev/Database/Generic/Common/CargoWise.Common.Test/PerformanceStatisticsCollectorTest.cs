using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class PerformanceStatisticsListener
		: BaseTestListener
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly PerformanceStatisticsListener Instance = new PerformanceStatisticsListener();

		public override void AfterEachTest(DateTime endTime)
		{
			if (!(PerformanceStatisticsCollector.instance is PerformanceStatisticsCollector.NullPerformanceStatisticsCollector))
			{
				int pendingSaves = TestPerformanceStatisticsCollector.PendingSaves;
				if (TestPerformanceStatisticsCollector.PendingSaves > 0)
				{
					PerformanceStatisticsCollector.AttemptFlush(force: true);
				}

				PerformanceStatisticsCollector.ResetInstance();

				if (pendingSaves > 0)
				{
					TestCase.HtmlFail(string.Format("There were {0} pending saves on the statistics collector", pendingSaves));
				}
			}
		}
	}

	public interface IPerformanceStatisticsTesting
	{
		int PendingSaves
		{
			get;
		}

		int SuccessfullSaves
		{
			get;
		}

		int FailedSaves
		{
			get;
		}

		void ResetStatisticMode();
	}

	public static class TestPerformanceStatisticsCollector
	{
		public static int PendingSaves
		{
			get
			{
				var collector = FindTestCollector();
				return (collector != null) ? collector.PendingSaves : 0;
			}
		}

		public static int SuccessfullSaves
		{
			get
			{
				var collector = FindTestCollector();
				return (collector != null) ? collector.SuccessfullSaves : 0;
			}
		}

		public static int FailedSaves
		{
			get
			{
				var collector = FindTestCollector();
				return (collector != null) ? collector.FailedSaves : 0;
			}
		}

		static IPerformanceStatisticsTesting FindTestCollector()
		{
			if (PerformanceStatisticsCollector.instance is CompositePerformanceStatisticsCollector collectors)
			{
				var collector = collectors.Collectors.FirstOrDefault(c => c is IPerformanceStatisticsTesting);
				if (collector != null)
				{
					return collector as IPerformanceStatisticsTesting;
				}
			}

			return PerformanceStatisticsCollector.instance as IPerformanceStatisticsTesting;
		}

		public static void ResetStatisticMode()
		{
			var collector = FindTestCollector();
			if (collector != null)
			{
				collector.ResetStatisticMode();
			}
		}
	}

	public class PerformanceStatisticsCollectorTest : TestCase
	{
		public void TestStartMonitoringHasAcceptablePerformance()
		{
			using (TestingState.SuspendIsRunningTests())
			{
				var ranch = new Stopwatch();
				ranch.Start();
				for (int i = 0; i < 1000000; i++)
				{
					using (PerformanceStatisticsCollector.StartMonitoring("TestStat"))
					{
					}
				}

				ranch.Stop();
				Assert("Acceptable performance: Took " + ranch.Elapsed.Seconds + " ticks, expected less than 1", ranch.Elapsed < TimeSpan.FromSeconds(1));
			}
		}

		public void TestCompositePerformanceStatisticsCollector()
		{
			var collector1 = new Mock<IPerformanceStatisticsCollector>();
			var startScope1 = new Mock<IDisposable>();
			var excludeScope1 = new Mock<IDisposable>();
			collector1.Setup(c => c.IsMonitoring).Returns(true);
			collector1.Setup(c => c.StatisticMode).Returns(EnabledState.Simple);
			collector1.Setup(c => c.StartMonitoring(It.IsAny<string>(), It.IsAny<string>())).Returns(startScope1.Object);
			collector1.Setup(c => c.Exclude()).Returns(excludeScope1.Object);

			var collector2 = new Mock<IPerformanceStatisticsCollector>();
			var startScope2 = new Mock<IDisposable>();
			var excludeScope2 = new Mock<IDisposable>();
			collector2.Setup(c => c.IsMonitoring).Returns(false);
			collector2.Setup(c => c.StatisticMode).Returns(EnabledState.Disabled);
			collector2.Setup(c => c.StartMonitoring(It.IsAny<string>(), It.IsAny<string>())).Returns(startScope2.Object);
			collector2.Setup(c => c.Exclude()).Returns(excludeScope2.Object);

			IPerformanceStatisticsCollector compositeCollector = new CompositePerformanceStatisticsCollector(collector1.Object, collector2.Object);

			using (compositeCollector.StartMonitoring("Name")) { }
			using (compositeCollector.Exclude()) { }
			compositeCollector.AttemptFlush();

			Assert(compositeCollector.IsMonitoring);
			AssertEquals(EnabledState.Simple, compositeCollector.StatisticMode);

			collector1.Verify(c => c.StartMonitoring("Name", null), Times.Once());
			collector1.Verify(c => c.Exclude(), Times.Once());
			collector1.Verify(c => c.AttemptFlush(false), Times.Once());
			startScope1.Verify(s => s.Dispose(), Times.Once());
			excludeScope1.Verify(s => s.Dispose(), Times.Once());

			collector2.Verify(c => c.StartMonitoring("Name", null), Times.Once());
			collector2.Verify(c => c.Exclude(), Times.Once());
			collector2.Verify(c => c.AttemptFlush(false), Times.Once());
			startScope2.Verify(s => s.Dispose(), Times.Once());
			excludeScope2.Verify(s => s.Dispose(), Times.Once());
		}
	}
}
