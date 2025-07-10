using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectCacheTest : TestCaseWithDummy
	{
		[DeveloperOnlyTest]
		public void TestFetchPerformance()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (int i = 0; i < 1_000_000; i++)
			{
				Cache.Fetch(typeof(DynamicBusinessObject), ZGuid.BrettsGuid);
			}
			AssertLessThan(stopWatch.ElapsedMilliseconds, 1000);
		}

		public void TestEmptyOrInvalidPKGuidIn_Fetch()
		{
			AssertNull(Cache.Fetch(typeof(DummyBusinessObject), ZGuid.Empty));
			AssertNull(Cache.Fetch(typeof(DummyBusinessObject), ZGuid.Invalid));
		}
		public void TestGetBusinessObjectsForPK()
		{
			Cache.Add(Dummy);

			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			Cache.Add(dummyBase);

			DummyBusinessObject dummyOtherPK = Factory.New<DummyBusinessObject>();
			Cache.Add(dummyOtherPK);

			BusinessObject[] firstDummies = Cache.GetBusinessObjectsForPK(Dummy.PK.ToGuid());
			ArrayList firstDummiesList = new ArrayList(firstDummies);
			Assert("Has correct dummies", firstDummiesList.Contains(Dummy));
			Assert("Has correct dummies", firstDummiesList.Contains(dummyBase));
			Assert("Right number of dummies", firstDummies.Length == 2);

			BusinessObject[] otherDummies = Cache.GetBusinessObjectsForPK(dummyOtherPK.PK.ToGuid());
			Assert("Right number of dummies", otherDummies.Length == 1);
			AssertEquals("Got right dummy", dummyOtherPK, otherDummies[0]);
		}

		public void TestNumberOfBusinessObjects()
		{
			Cache.Add(Factory.New(typeof(DummyBusinessObject)));
			Cache.Add(Factory.New(typeof(DummyBusinessObject)));

			AssertEquals("Two objects must be in the Cache", 2, Cache.NumberOfBusinessObjects);
		}

		public void TestCacheReturnsNullBusinessObject()
		{
			Dummy.IsNull = true;
			Cache.Add(Dummy);
			object bizO = Cache.Fetch(typeof(DummyBusinessObject), Dummy.PK);
			Assert(bizO != null);
			AssertEquals(true, ((BusinessObject)bizO).IsNull);
		}

		public void TestAddAndIndexer()
		{
			Cache.Add(Dummy);

			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			Cache.Add(dummyBase);

			BusinessObjectsForARow bO4Row = Cache[Dummy.Row];
			AssertEquals("Dummy", bO4Row[typeof(DummyBusinessObject)], Dummy);
			AssertEquals("DummyBase", bO4Row[typeof(DummyBaseBusinessObject)], dummyBase);

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			Cache.Add(dummy2);

			BusinessObjectsForARow bO4Row2 = Cache[dummy2.Row];
			AssertEquals("Dummy2", bO4Row2[typeof(DummyBusinessObject)], dummy2);

			bO4Row = Cache[Dummy.Row];
			AssertEquals("Dummy", bO4Row[typeof(DummyBusinessObject)], Dummy);
			AssertEquals("DummyBase", bO4Row[typeof(DummyBaseBusinessObject)], dummyBase);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestAddDuplicateThrowsException()
		{
			Cache.Add(Dummy);
			Cache.Add(Dummy);
		}

		public void TestIndexerReturnsNullForUnknownRow()
		{
			Cache.Add(Dummy);
			AssertNull("Should be null", Cache[Factory.New(typeof(DummyBusinessObject)).Row]);
		}

		public void TestAllBusinessObjects()
		{
			Cache.Add(Dummy);

			DummyBaseBusinessObject dummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);
			Cache.Add(dummyBase);

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			Cache.Add(dummy2);

			AssertEquals("In cache", true, Cache.AllBusinessObjects.Contains(Dummy));
			AssertEquals("In cache", true, Cache.AllBusinessObjects.Contains(dummyBase));
			AssertEquals("In cache", true, Cache.AllBusinessObjects.Contains(dummy2));
		}

		public void TestFetchByTypeAndPK()
		{
			AssertNull(Cache.Fetch(Dummy.GetType(), Dummy.PK) as DummyBusinessObject);
			Cache.Add(Dummy);
			AssertEquals(Dummy, Cache.Fetch(Dummy.GetType(), Dummy.PK) as DummyBusinessObject);
		}

		public void TestGetBusinessObjectsForPKWithNull()
		{
			BusinessObject bizO = Factory.GetNull(typeof(DummyBusinessObject));
			Cache.Add(bizO);
			BusinessObject[] bizOs = Cache.GetBusinessObjectsForPK(bizO.PK.ToGuid());
			AssertEquals(1, bizOs.Length);
			AssertEquals(bizO, bizOs[0]);
		}

		BusinessObjectCache cache;
		BusinessObjectCache Cache
		{
			get { return cache ?? (cache = new BusinessObjectCache()); }
		}
	}
}
