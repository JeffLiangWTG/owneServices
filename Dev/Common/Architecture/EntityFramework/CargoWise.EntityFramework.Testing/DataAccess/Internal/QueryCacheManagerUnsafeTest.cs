using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class QueryCacheManagerUnsafeTest : NUnit.Framework.TestCase
	{
		public void TestCacheKeysMatchOnMultipleOrParts()
		{
			var manager = new QueryCacheManager();
			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, "A");
			query1.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, "B");
			var query2 = new ZQuery(DummyBizoSchema.Z0_Code, "C");
			query2.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, "D");
			var combinedOrQuery = new ZQuery();
			combinedOrQuery.AddToFilter(query1, JoinCondition.Or);
			combinedOrQuery.AddToFilter(query2, JoinCondition.Or);
			AssertEquals("(Z0_Code = 'A' or Z0_Code = 'B') or (Z0_Code = 'C' or Z0_Code = 'D')", combinedOrQuery.LiteralTextSql);

			var tableName = DummyBizoSchema.Constants.TableName;
			manager.Store(tableName, combinedOrQuery);
			AssertContainsExactElementsInAnyOrder("Pre-Condition: 'Or' keys are added in parts",
				new[] { "Z0_Code = 'A'", "Z0_Code = 'B'", "Z0_Code = 'C'", "Z0_Code = 'D'" },
				manager.CachedKeys);

			var matchQuery1 = new ZQuery(DummyBizoSchema.Z0_Code, "A");
			var matchQuery2 = new ZQuery(DummyBizoSchema.Z0_Code, "A");
			matchQuery2.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, "D");
			var matchQuery3 = new ZQuery(DummyBizoSchema.Z0_Code, "A");
			matchQuery3.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, "C");

			CombineAssertions(() =>
			{
				Assert("Can match original 'Or' query: AUB is contained in AUBUCUD", manager.IsCached(tableName, query1));
				Assert("Can match original 'Or' query: CUD is contained in AUBUCUD", manager.IsCached(tableName, query2));
				Assert("Can match on single 'Or' part: A is contained in AUBUCUD", manager.IsCached(tableName, matchQuery1));
				Assert("Can match on two 'Or' parts: AUD is contained in AUBUCUD", manager.IsCached(tableName, matchQuery2));
				Assert("Can match on two 'Or' parts: AUC is contained in AUBUCUD", manager.IsCached(tableName, matchQuery3));
			});
		}

		public void TestCachedKeys()
		{
			var manager = new QueryCacheManager();
			const string table = "XXX";
			var filter = new ZQuery();
			manager.Store(table, filter);

			const string table1 = "XXX";
			var filter1 = new ZQuery();
			manager.Store(table1, filter1);

			const string table2 = "YYY";
			var filter2 = new ZQuery();
			manager.Store(table2, filter2);

			AssertEquals("Cached Keys length", 2, manager.CachedKeys.Length);
			AssertEquals(true, manager.IsCached(table, filter));
			AssertEquals(true, manager.IsCached(table1, filter1));
			AssertEquals(true, manager.IsCached(table2, filter2));
		}

		public void TestLoadWithBlobs()
		{
			var manager = new QueryCacheManager();
			const string table = "XXX";
			var filter = new ZQuery();
			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));

			filter.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			Assert(!manager.IsCached(table, filter));

			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));
		}

		public void TestLoadWithBlobsWithPK()
		{
			var manager = new QueryCacheManager();
			const string tableName = DummyBizoSchema.Constants.TableName;
			var pk = new ZGuid();
			var filter = new ZQuery(DummyBizoSchema.PK, pk);
			manager.Store(tableName, filter);
			Assert(manager.IsCached(tableName, filter));

			filter.IncludeBlob(DummyBizoSchema.Z0_VarBinaryMax);
			Assert(!manager.IsCached(tableName, filter));

			manager.Store(tableName, filter);
			Assert(manager.IsCached(tableName, filter));
		}

		public void TestCachedValueIsReturned()
		{
			var manager = new QueryCacheManager();
			const string table = "XXX";
			var filter = new ZQuery();
			Assert(!manager.IsCached(table, filter));

			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));
		}

		public void TestCachedValueIsNotReturnedAfterClear()
		{
			var manager = new QueryCacheManager();
			const string table = "XXX";
			var filter = new ZQuery();
			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));

			manager.Clear();
			Assert(!manager.IsCached(table, filter));
		}

		public void TestClearingAllTablesClearsBespoke()
		{
			var manager = new QueryCacheManager();
			const string table = "XXX";
			var filter = new ZQuery();
			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));

			manager.Clear();
			manager.Clear(table);
			Assert(!manager.IsCached(table, filter));
			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));
		}

		public void TestClearingAllViewsClearsBespoke()
		{
			var manager = new QueryCacheManager();
			const string table = "ViewXXX";
			var filter = new ZQuery();
			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));

			manager.ClearViewsQueries(Enumerable.Empty<string>()); // This means ClearAllViews... Wtf?
			manager.ClearViewsQueries(new[] { table });
			Assert(!manager.IsCached(table, filter));
			manager.Store(table, filter);
			Assert(manager.IsCached(table, filter));
		}

		public void TestAllResultsCacheReturnsTrueForAllOtherQueries()
		{
			var manager = new QueryCacheManager();
			var allResults = new ZQuery();
			manager.Store(DummyBizoSchema.Constants.TableName, allResults);

			var filter = new ZQuery(DummyBizoSchema.Z0_Code, "X");
			AssertEquals(true, manager.IsCached(DummyBizoSchema.Constants.TableName, filter));
		}

		public void TestAllResultsQueryClearsPreviousCachedValues()
		{
			var manager = new QueryCacheManager();
			var filter = new ZQuery(DummyBizoSchema.Z0_Code, "X");
			manager.Store(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(1, manager.CachedKeys.Length);

			var allResults = new ZQuery();
			manager.Store(DummyBizoSchema.Constants.TableName, allResults);
			AssertEquals(1, manager.CachedKeys.Length);
		}

		public void TestAllResultsQueriesStopsFurtherCacheEntries()
		{
			var manager = new QueryCacheManager();
			var allResults = new ZQuery();
			manager.Store(DummyBizoSchema.Constants.TableName, allResults);

			var filter2 = new ZQuery(DummyBizoSchema.Z0_Code, "Y");
			manager.Store(DummyBizoSchema.Constants.TableName, filter2);
			AssertEquals(1, manager.CachedKeys.Length);
			AssertEquals("", manager.CachedKeys[0]);
		}

		public void TestAllResultsAfterInitialResultStopsFurtherCacheEntries()
		{
			var manager = new QueryCacheManager();
			var someResults = new ZQuery(DummyBizoSchema.Z0_Code, "A");
			manager.Store(DummyBizoSchema.Constants.TableName, someResults);

			var allResults = new ZQuery();
			manager.Store(DummyBizoSchema.Constants.TableName, allResults);

			var filter2 = new ZQuery(DummyBizoSchema.Z0_Code, "Y");
			manager.Store(DummyBizoSchema.Constants.TableName, filter2);
			AssertEquals(1, manager.CachedKeys.Length);
			AssertEquals("", manager.CachedKeys[0]);
		}

		public void TestGetKeyAffectsGetCachedValue()
		{
			var manager = new QueryCacheManager();
			var filter = new ZQuery(DummyBizoSchema.Z0_Code, "X");
			Assert(!manager.IsCached("", filter));
			manager.Store("", filter);
			Assert(manager.IsCached("", filter));

			filter.MaximumRows = 123;
			Assert(manager.IsCached("", filter));

			filter.OrderBy = "XYZ";
			Assert(manager.IsCached("", filter));
		}

		public void TestKeyIsCaseInsensitive()
		{
			var manager = new QueryCacheManager();
			var filterUpper = new ZQuery(DummyBizoSchema.Z0_Code, "XYZ");
			manager.Store("", filterUpper);
			var filterLower = new ZQuery(DummyBizoSchema.Z0_Code, "xyz");
			Assert(manager.IsCached("", filterLower));
		}

		public void TestClearResetsCache()
		{
			var manager = new QueryCacheManager();
			var filter = new ZQuery();
			manager.Store("", filter);
			Assert(manager.IsCached("", filter));
			manager.Clear();
			Assert(!manager.IsCached("", filter));
		}

		public void TestClearOneTableFilters()
		{
			var manager = new QueryCacheManager();
			var filter = new ZQuery();
			manager.Store("T1", filter);
			manager.Store("T2", filter);
			manager.Store("T3", filter);

			Assert(manager.IsCached("T1", filter));
			Assert(manager.IsCached("T2", filter));
			Assert(manager.IsCached("T3", filter));

			manager.Clear("T2");

			Assert(manager.IsCached("T1", filter));
			Assert(!manager.IsCached("T2", filter));
			Assert(manager.IsCached("T3", filter));
		}

		public void TestClearViewsQueries()
		{
			var manager = new QueryCacheManager();
			var filter = new ZQuery();
			manager.Store("Table", filter);
			manager.Store("ViewXxx", filter);
			manager.Store("VW_Yyy", filter);
			manager.Store("VWZzz", filter);
			manager.Store("AViewBc", filter);

			Assert(manager.IsCached("Table", filter));
			Assert(manager.IsCached("ViewXxx", filter));
			Assert(manager.IsCached("VW_Yyy", filter));
			Assert(manager.IsCached("VWZzz", filter));
			Assert(manager.IsCached("AViewBc", filter));

			manager.ClearViewsQueries(Enumerable.Empty<string>());

			Assert(manager.IsCached("Table", filter));
			Assert(!manager.IsCached("ViewXxx", filter));
			Assert(!manager.IsCached("VW_Yyy", filter));
			Assert(manager.IsCached("VWZzz", filter));
			Assert(manager.IsCached("AViewBc", filter));
		}

		public void TestClearViewsQueries_ForViewsWithRelatedTables()
		{
			var manager = new QueryCacheManager();
			var filter = new ZQuery();

			manager.Store("ViewProcessTask", filter);
			manager.Store("ViewProcessHeader", filter);

			AssertEquals(true, manager.IsCached("ViewProcessTask", filter));
			AssertEquals(true, manager.IsCached("ViewProcessHeader", filter));

			manager.ClearViewsQueries(new[] { "OrgHeader" });
			AssertEquals(true, manager.IsCached("ViewProcessTask", filter));
			AssertEquals(true, manager.IsCached("ViewProcessHeader", filter));

			manager.ClearViewsQueries(new[] { "ProcessTasks" });
			AssertEquals(false, manager.IsCached("ViewProcessTask", filter));
			AssertEquals(true, manager.IsCached("ViewProcessHeader", filter));

			manager.ClearViewsQueries(new[] { "ProcessHeader" });
			AssertEquals(false, manager.IsCached("ViewProcessTask", filter));
			AssertEquals(false, manager.IsCached("ViewProcessHeader", filter));
		}

		public void TestStoreAndNoNullRefException()
		{
			var manager = new QueryCacheManager();
			const string table = "foo";
			var filter = new ZQuery();
			manager.Store(table, filter);
			manager.CachedKeyDictionaries[table] = null;
			manager.Store(table, filter);
			AssertNoExceptionThrown("No exception should be thrown out for null cache", () => manager.Store(table, filter));
		}

		public void TestIsCachedNoNullRefException()
		{
			var manager = new QueryCacheManager();
			const string table = "foo";
			var filter = new ZQuery();
			manager.Store(table, filter);
			AssertNoExceptionThrown("No exception should be thrown out for null filter", () => manager.IsCached(table, null));

			manager.CachedKeyDictionaries[table] = null;
			AssertNoExceptionThrown("No exception should be thrown out for null cache", () => manager.IsCached(table, filter));
		}
	}
}
