using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class DBQueryCacheManagerUnsafeTest : NUnit.Framework.TestCase
	{
		public void TestCachedValueIsReturned()
		{
			DBQueryCacheManager manager = new DBQueryCacheManager();
			string table = "XXX";
			ZQuery filter = new ZQuery();
			AssertEquals(null, manager.GetCachedValue(table, filter));
			manager.Store(rowArray, table, filter);
			AssertEquals(rowArray, manager.GetCachedValue(table, filter));
		}

		public void TestCachedValueIsNotReturnedAfterClear()
		{
			DBQueryCacheManager manager = new DBQueryCacheManager();
			string table = "XXX";
			ZQuery filter = new ZQuery();
			manager.Store(rowArray, table, filter);
			AssertEquals(rowArray, manager.GetCachedValue(table, filter));
			manager.Clear();
			AssertEquals(null, manager.GetCachedValue(table, filter));
		}

		public void TestGetKeyAffectsGetCachedValue()
		{
			DBQueryCacheManager manager = new DBQueryCacheManager();
			ZQuery filter = new ZQuery();
			manager.Store(rowArray, "", filter);
			AssertNotNull(manager.GetCachedValue("", filter));

			filter.AddToFilter(DummyBizoSchema.Z0_Code, "X");
			AssertNull(manager.GetCachedValue("", filter));
			manager.Store(rowArray, "", filter);
			AssertNotNull(manager.GetCachedValue("", filter));

			filter.MaximumRows = 123;
			AssertNull(manager.GetCachedValue("", filter));
			manager.Store(rowArray, "", filter);
			AssertNotNull(manager.GetCachedValue("", filter));

			filter.OrderBy = "XYZ";
			AssertNull(manager.GetCachedValue("", filter));
			manager.Store(rowArray, "", filter);
			AssertNotNull(manager.GetCachedValue("", filter));
		}

		public void TestIgnoreDbQueryCache()
		{
			var manager = new DBQueryCacheManager();
			var filter = new ZQuery();
			manager.Store(rowArray, "", filter);
			AssertNotNull(manager.GetCachedValue("", filter));
			filter.IgnoreDbQueryCache = true;
			AssertNull(manager.GetCachedValue("", filter));
		}

		public void TestKeyIsCaseInsensitive()
		{
			DBQueryCacheManager manager = new DBQueryCacheManager();
			ZQuery filterUpper = new ZQuery(DummyBizoSchema.Z0_Code, "XYZ");
			manager.Store(rowArray, "", filterUpper);
			ZQuery filterLower = new ZQuery(DummyBizoSchema.Z0_Code, "xyz");
			AssertNotNull(manager.GetCachedValue("", filterLower));
		}

		#region TestClear

		public void TestClearResetsCache()
		{
			DBQueryCacheManager manager = new DBQueryCacheManager();
			ZQuery filter = new ZQuery();
			manager.Store(rowArray, "", filter);
			AssertNotNull(manager.GetCachedValue("", filter));
			manager.Clear();
			AssertNull(manager.GetCachedValue("", filter));
		}

		public void TestClear_OneTableFilters()
		{
			var manager = new DBQueryCacheManager_Unsafe();
			var filter1 = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AAA");
			var filter2 = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "BBB");
			manager.Store(rowArray, "T1", filter1);
			manager.Store(rowArray, "T2", filter1);
			manager.Store(rowArray, "T2", filter2);
			manager.Store(rowArray, "T3", filter2);

			Assert(manager.cache.ContainsKey(manager.GetKey("T1", filter1)));
			Assert(manager.cache.ContainsKey(manager.GetKey("T2", filter1)));
			Assert(manager.cache.ContainsKey(manager.GetKey("T2", filter2)));
			Assert(manager.cache.ContainsKey(manager.GetKey("T3", filter2)));

			manager.Clear("T2");

			Assert(manager.cache.ContainsKey(manager.GetKey("T1", filter1)));
			Assert(!manager.cache.ContainsKey(manager.GetKey("T2", filter1)));
			Assert(!manager.cache.ContainsKey(manager.GetKey("T2", filter2)));
			Assert(manager.cache.ContainsKey(manager.GetKey("T3", filter2)));

			manager.Clear("BBB");

			Assert(manager.cache.ContainsKey(manager.GetKey("T1", filter1)));
			Assert(!manager.cache.ContainsKey(manager.GetKey("T2", filter1)));
			Assert(!manager.cache.ContainsKey(manager.GetKey("T2", filter2)));
			Assert("Should remove all keys containing table name as substring in any place", !manager.cache.ContainsKey(manager.GetKey("T3", filter2)));
		}

		public void TestClearViewsQueries()
		{
			var manager = new DBQueryCacheManager_Unsafe();
			ZQuery filter = new ZQuery();
			manager.Store(rowArray, "Table", filter);
			manager.Store(rowArray, "ViewXxx", filter);
			manager.Store(rowArray, "VW_Yyy", filter);
			manager.Store(rowArray, "VWZzz", filter);
			manager.Store(rowArray, "AViewBc", filter);

			Assert(manager.cache.ContainsKey(manager.GetKey("Table", filter)));
			Assert(manager.cache.ContainsKey(manager.GetKey("ViewXxx", filter)));
			Assert(manager.cache.ContainsKey(manager.GetKey("VW_Yyy", filter)));
			Assert(manager.cache.ContainsKey(manager.GetKey("VWZzz", filter)));
			Assert(manager.cache.ContainsKey(manager.GetKey("AViewBc", filter)));

			manager.ClearViewsQueries();

			Assert(manager.cache.ContainsKey(manager.GetKey("Table", filter)));
			Assert(!manager.cache.ContainsKey(manager.GetKey("ViewXxx", filter)));
			Assert(!manager.cache.ContainsKey(manager.GetKey("VW_Yyy", filter)));
			Assert(manager.cache.ContainsKey(manager.GetKey("VWZzz", filter)));
			Assert(manager.cache.ContainsKey(manager.GetKey("AViewBc", filter)));
		}

		#endregion

		public void TestGetKey()
		{
			var manager = new DBQueryCacheManager_Unsafe();
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "XYZ") { MaximumRows = 10, OrderBy = "Z0_Number" };

			string expectedKey1 = "Table1" + DBQueryCacheManager_Unsafe.Separator + query.LiteralTextADO + "10Z0_Number";
			AssertEquals(expectedKey1, manager.GetKey("Table1", query));

			string expectedKey2 = "Table1" + DBQueryCacheManager_Unsafe.Separator + "(" + query.LiteralTextADO + ")10Z0_Number";
			AssertEquals(expectedKey2, manager.GetKey("Table1", query, true));
		}

		public void TestGetCachedValueForFiltersWithDifferentBrackets()
		{
			DBQueryCacheManager manager = new DBQueryCacheManager();
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "XYZ");

			manager.Store(rowArray, "Table1", query);

			AssertNotNull(manager.GetCachedValue("Table1", query));

			manager.Clear();
			manager.Store(rowArray, "Table1", query);

			AssertNotNull(manager.GetCachedValue("Table1", query));
		}

		#region Implementation

		DataRow[] rowArray;

		protected override void SetUp()
		{
			base.SetUp();
			rowArray = System.Array.Empty<DataRow>();
		}

		#endregion
	}
}
