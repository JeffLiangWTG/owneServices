using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestedType(typeof(BusinessObjectFactoryStatistic))]
	sealed class BusinessObjectFactoryStatisticTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFactoryStatShouldNotBeCachedInFactory()
		{
			var reference = GetFactoryStatWeakReference();
			GC.Collect();
			GC.WaitForFullGCComplete();
			AssertNull(reference.Target);
		}

		public void TestNoProblemsWhenFactoryOwnedCrossThread()
		{
			BusinessObjectFactory factory = null;
			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory = new BusinessObjectFactory();
				}
			});
			thread.Start();
			thread.Join();
			var statistic = new BusinessObjectFactoryStatistic(factory);
			AssertEquals(false, statistic.IsOwnedByCurrentThread);
			object dummy = statistic.Name;
			dummy = statistic.FactoryCreationTime;
			dummy = statistic.BusinessObjectCount;
			dummy = statistic.DataRowCount;
			dummy = statistic.ActiveFetchHintsCount;
			dummy = statistic.DatabaseLoadCount;
			dummy = statistic.ChildFactoriesCount;
			dummy = statistic.ChildFactoryIDs;
			dummy = statistic.AllocationPath;
			dummy = statistic.CreationThreadID;
			dummy = statistic.BusinessObjectsInformation;
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestChildFactoryIDs()
		{
			var factory = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			var factory3 = new BusinessObjectFactory();
			factory.ChildFactories.Add(factory2);
			factory.ChildFactories.Add(factory3);
			var statistic = new BusinessObjectFactoryStatistic(factory);
			var expected = factory2._Instance.ToString() + ", " + factory3._Instance.ToString();

			AssertEquals(expected, statistic.ChildFactoryIDs);
		}

		WeakReference GetFactoryStatWeakReference()
		{
			return new WeakReference(new BusinessObjectFactoryStatistic(Factory));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BusinessObjectFactoryStatistic(Factory);
		}
	}
}
