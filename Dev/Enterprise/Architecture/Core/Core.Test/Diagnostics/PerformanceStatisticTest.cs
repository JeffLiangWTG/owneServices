using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestedType(typeof(PerformanceStatistic))]
	sealed class PerformanceStatisticTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNudgeEventsIgnoredWhenNotInteractive()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var nudgingTrackerCreated = false;
				using (ObjectFactory.Substitute(() =>
				{
					nudgingTrackerCreated = true;
					var mock = new Mock<INudgingEventsTracker>();
					return mock.Object;
				}))
				{
					var performanceStats = new PerformanceStatistic();
					Assert(!nudgingTrackerCreated);
				}
			}
		}

		public void TestNudgeEventsPropagatedWhenInteractive()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var nudgingTracker = new Mock<INudgingEventsTracker>() { CallBase = true };
				nudgingTracker.Setup(nt => nt.Events).Returns(new[] { new NudgeEventWithTime(new NudgeSucceededEventArgs(new[] { "TST" })) });
				using (ObjectFactory.Substitute(nudgingTracker.Object))
				{
					var performanceStats = new PerformanceStatistic();
					AssertEquals(1, performanceStats.NudgeEventsContents.Count);
					var nudgeEvent = performanceStats.NudgeEventsContents[0];
					AssertEquals("succeeded", nudgeEvent.EventType.ToString());
					AssertEquals("TST", nudgeEvent.Tasks.ToString());
				}
			}
		}

		public void TestSortCollection()
		{
			var performanceStatistic = GetNewBusinessObject() as PerformanceStatistic;

			var sortInfoName = new SortInfo("Name", System.ComponentModel.ListSortDirection.Descending);
			performanceStatistic.FactoryStatistics.Sort(sortInfoName);

			new BusinessObjectFactory() { NameForDebugging = "Afactory" };
			new BusinessObjectFactory() { NameForDebugging = "Bfactory" };
			performanceStatistic.Load();

			var factoryNames = performanceStatistic.FactoryStatistics.ToArray<BusinessObjectFactoryStatistic>().Select(f => f.Name).ToArray();
			var indexAAA = Array.IndexOf(factoryNames, "Afactory");
			var indexBBB = Array.IndexOf(factoryNames, "Bfactory");
			Assert(indexAAA > indexBBB);
		}

		public void TestFactoryReferencesStayWhenFactoryIsDeactivated()
		{
			var performanceStatistic = GetNewBusinessObject() as PerformanceStatistic;

			var factory1 = new BusinessObjectFactory() { NameForDebugging = "Afactory" };
			var factory2 = new BusinessObjectFactory() { NameForDebugging = "Bfactory" };

			using (PersistentFactoryCacheManager.Instance.MakeFactoryReferencesStrong())
			{
				performanceStatistic.Load();

				var factoryCount = performanceStatistic.FactoryStatistics.Count;

				factory2.DeactivateActiveCollectionsAndCaches();
				performanceStatistic.Load();

				var newFactoryCount = performanceStatistic.FactoryStatistics.Count;
				Assert("There should still be the same number of factories.\r\nBefore: " + factoryCount + "\r\nAfter: " + newFactoryCount, newFactoryCount == factoryCount);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PerformanceStatistic();
		}
	}
}
