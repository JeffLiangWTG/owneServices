using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CollectionCacheTest : TestCaseWithFactory
	{
		public void TestFetchGetsData()
		{
			CollectionCacheFaker cache = new CollectionCacheFaker(new BusinessObjectFactory());
			DynamicBusinessObjectCollection resultSet = cache.Fetch("select top 1 * from dbo.orgheader", null);
			Assert(resultSet != null);
			AssertEquals("Rows in Result Set", 1, resultSet.Count);
		}

		public void TestFetchTwiceHitsDatabaseOnce()
		{
			CollectionCacheFaker cache = new CollectionCacheFaker(new BusinessObjectFactory());
			AssertEquals("Precondition", false, cache.HasHitDatabase);
			HitDatabaseWithParams(cache);
			AssertEquals("Precondition #2", true, cache.HasHitDatabase);
			cache.HasHitDatabase = false;
			HitDatabaseWithParams(cache);
			AssertEquals("Has hit DB second time", false, cache.HasHitDatabase);
		}

		void HitDatabaseWithParams(CollectionCache cache)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(Enterprise.ZArchitecture.Schema.OrgHeaderSchema.OH_Code, "X");
			ZNonPersistentDataQuery dq = filter.ParameterisedText;
			DynamicBusinessObjectCollection resultSet = cache.Fetch("select top 1 * from dbo.orgheader where " + dq.ParameterisedQueryText, dq.Parameters);
		}

		public void TestDifferentFetchHitsDatabaseTwice()
		{
			CollectionCacheFaker cache = new CollectionCacheFaker(new BusinessObjectFactory());
			DynamicBusinessObjectCollection resultSet = cache.Fetch("select top 1 * from dbo.orgheader", null);
			cache.HasHitDatabase = false;
			DynamicBusinessObjectCollection resultSet2 = cache.Fetch("select top 2 * from dbo.orgheader", null);

			Assert(cache.HasHitDatabase);
			AssertEquals("Rows in Result Set", 2, resultSet2.Count);
		}
	}
}
