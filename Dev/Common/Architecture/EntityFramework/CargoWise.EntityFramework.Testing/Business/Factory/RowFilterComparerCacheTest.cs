using System;
using CargoWise.Common.Collections;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class RowFilterComparerCacheTest : TestCaseWithFactory
	{
		public void TestKeyCache()
		{
			var testCode = "VIM";
			var newFactory = Factory.CreateNewFactory();

			var dummy = newFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = testCode;

			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);
			Assert(dummy.MatchesFilter(query1, identifier: testCode));

			var cache = newFactory.GetCachedValue<LRUCache<string, (string Key, bool IsQuery)>>(MatchFilterKeyCacheKey, () => { throw new InvalidOperationException(); });

			var (cachedKey, isQuery) = cache[testCode];
			AssertNotNull(cachedKey);
			AssertEquals(true, isQuery);

			AssertEquals("Identical identifiers have identical keys", query1.LiteralTextADO, cache[testCode].Key);
			AssertEquals("Other identifiers should have no value", false, cache.TryGetValue("EMACS", out var _));
		}

		public void TestKeyCache_LimitSize()
		{
			var testCode = "VIM";
			var newFactory = Factory.CreateNewFactory();

			var dummy = newFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = testCode;

			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);
			query1.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, new string('a', 5000));
			Assert(dummy.MatchesFilter(query1, identifier: testCode));

			var cache = newFactory.GetCachedValue<LRUCache<string, (string Key, bool IsQuery)>>(MatchFilterKeyCacheKey, () => { throw new InvalidOperationException(); });
			var cachedKey = cache[testCode];

			var result = cache[testCode];
			AssertEquals("Key is too large, so should just cache identifier.", testCode, result.Key);
			AssertEquals("Key is too large, so should just cache identifier.", false, result.IsQuery);
		}

		public void TestKeyCache_LimitCount()
		{
			var testCode = "VIM";
			var newFactory = Factory.CreateNewFactory();

			var dummy = newFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = testCode;

			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);

			Assert(dummy.MatchesFilter(query1, identifier: testCode));

			var numberBigEnoughToCauseWeakReferenceScrubInLRUCache = 151;
			for (var i = 0; i < numberBigEnoughToCauseWeakReferenceScrubInLRUCache; i++)
			{
				if (i % 50 == 0)
				{
					GC.Collect();
				}
				var bufferingQuery = new ZQuery(DummyBizoSchema.Z0_Code, string.Format("{0}a", i));
				dummy.MatchesFilter(bufferingQuery, identifier: i.ToString());
			}

			var cache = newFactory.GetCachedValue<LRUCache<string, (string Key, bool IsQuery)>>(MatchFilterKeyCacheKey, () => { throw new InvalidOperationException(); });
			var cachedKeyExists = cache.TryGetValue(testCode, out var result);
			AssertEquals("So many queries have been run that the first query isn't in the cache anymore", false, cachedKeyExists);
		}

		public void TestQueryCache()
		{
			var testCode = "VIM";
			var newFactory = Factory.CreateNewFactory();

			var dummy = newFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = testCode;

			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);
			var query2 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);

			Assert(dummy.MatchesFilter(query1));

			var cache = newFactory.GetCachedValue<LRUCache<string, RowFilterComparer>>(BusinessObject.RowFilterComparerCacheKey, () => { throw new InvalidOperationException(); });

			var cachedComparer = cache[query1.LiteralTextADO];
			AssertNotNull(cachedComparer);

			AssertEquals("Identical queries have identical filter comparers", cachedComparer, cache[query2.LiteralTextADO]);
		}

		public void TestQueryCache_LimitSize()
		{
			var testCode = "VIM";
			var newFactory = Factory.CreateNewFactory();

			var dummy = newFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = testCode;

			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);

			Assert(dummy.MatchesFilter(query1));

			query1.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, new string('a', 5000));
			Assert(dummy.MatchesFilter(query1));

			var cache = newFactory.GetCachedValue<LRUCache<string, RowFilterComparer>>(BusinessObject.RowFilterComparerCacheKey, () => { throw new InvalidOperationException(); });
			var cachedComparer = cache[query1.LiteralTextADO];

			AssertNull("Key is too large, so query isn't cached.", cachedComparer);
		}

		public void TestQueryCache_LimitCount()
		{
			var testCode = "VIM";
			var newFactory = Factory.CreateNewFactory();

			var dummy = newFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = testCode;

			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);

			Assert(dummy.MatchesFilter(query1));

			int numberBigEnoughToCauseWeakReferenceScrubInLRUCache = 151;
			for (int i = 0; i < numberBigEnoughToCauseWeakReferenceScrubInLRUCache; i++)
			{
				if (i % 50 == 0)
				{
					GC.Collect();
				}
				var bufferingQuery = new ZQuery(DummyBizoSchema.Z0_Code, string.Format("{0}a", i));
				dummy.MatchesFilter(bufferingQuery);
			}

			var cache = newFactory.GetCachedValue<LRUCache<string, RowFilterComparer>>(BusinessObject.RowFilterComparerCacheKey, () => { throw new InvalidOperationException(); });
			var cachedComparer = cache[query1.LiteralTextADO];

			AssertNull("So many queries have been run that the first query isn't in the cache anymore", cachedComparer);
		}

		public void TestKeyAndQueryCache_LimitSize()
		{
			var testCode = "VIM";
			var newFactory = Factory.CreateNewFactory();

			var dummy = newFactory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = testCode;

			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, testCode);
			query1.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, new string('a', 5000));
			Assert(dummy.MatchesFilter(query1, identifier: testCode));

			var keyCache = newFactory.GetCachedValue<LRUCache<string, (string Key, bool IsQuery)>>(MatchFilterKeyCacheKey, () => { throw new InvalidOperationException(); });
			var cachedKey = keyCache[testCode];
			var result = keyCache[testCode];
			AssertEquals("Key is too large, so should just cache identifier.", testCode, result.Key);
			AssertEquals("Key is too large, so should just cache identifier.", false, result.IsQuery);

			var comparerCache = newFactory.GetCachedValue<LRUCache<string, RowFilterComparer>>(BusinessObject.RowFilterComparerCacheKey, () => { throw new InvalidOperationException(); });
			var cachedComparer = comparerCache[testCode];
			AssertEquals("Should return the cached comparer", cachedComparer, comparerCache[testCode]);
		}

		const string MatchFilterKeyCacheKey = "BFD69346-5A4F-48BA-9137-1B19612EDBE2";
	}
}
