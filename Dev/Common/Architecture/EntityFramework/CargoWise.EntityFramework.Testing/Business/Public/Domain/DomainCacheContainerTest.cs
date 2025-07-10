using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DomainCacheContainerTest : TestCaseWithFactory
	{
		public void TestGetCachedValueReturnsSameObjectWithKey()
		{
			ValueCacheDomainService cache = ValueCacheDomainService.Get(Factory);
			object result1 = cache.GetCachedValue("Key", delegate
			{ return new object(); }, CacheStalenessPolicy.NeverStale);
			object result2 = cache.GetCachedValue("Key", delegate
			{ return new object(); }, CacheStalenessPolicy.NeverStale);
			AssertEquals(result1, result2);
		}

		public void TestGetCachedValueReturnsDifferentObjectWithDifferentKey()
		{
			ValueCacheDomainService cache = ValueCacheDomainService.Get(Factory);
			object result1 = cache.GetCachedValue("Key1", delegate
			{ return new object(); }, CacheStalenessPolicy.NeverStale);
			object result2 = cache.GetCachedValue("Key2", delegate
			{ return new object(); }, CacheStalenessPolicy.NeverStale);
			AssertNotEquals(result1, result2);
		}

		public void TestTryGetValueFromCacheOnly()
		{
			var cache = ValueCacheDomainService.Get(Factory);

			// K1
			string result1;
			var isKey1Cached = cache.TryGetValueFromCacheOnly("K1", out result1);
			AssertEquals("[Before Caching] Key K1 cached?", false, isKey1Cached);
			AssertNull(nameof(result1), result1);

			var result2 = cache.GetCachedValue("K1", () => "K1_CACHE_VALUE", CacheStalenessPolicy.NeverStale);
			AssertEquals("Cache created from getValueDelegate", "K1_CACHE_VALUE", result2);

			string result3;
			isKey1Cached = cache.TryGetValueFromCacheOnly("K1", out result3);
			AssertEquals("[After Caching] Key K1 cached?", true, isKey1Cached);
			AssertEquals("Value read from cache", result2, result3);

			// K2
			var isKey2Cached = cache.TryGetValueFromCacheOnly<string>("K2", out _);
			AssertEquals("[Before Caching] Key K2 cached?", false, isKey2Cached);
			var result4 = cache.GetCachedValue("K2", () => "K2_VALUE", CacheStalenessPolicy.NeverStale);
			AssertEquals("Cache created from getValueDelegate", "K2_VALUE", result4);

			string result5;
			isKey2Cached = cache.TryGetValueFromCacheOnly("K2", out result5);
			AssertEquals("[After Caching] Key K2 cached?", true, isKey2Cached);
			AssertEquals("Value read from cache", result4, result5);
		}

		public void TestTryGetValueFromCacheOnly_WithNonNullableType()
		{
			var cache = ValueCacheDomainService.Get(Factory);

			// K1
			int result1;
			var isKey1Cached = cache.TryGetValueFromCacheOnly("K1", out result1);
			AssertEquals("[Before Caching] Key K1 cached?", false, isKey1Cached);
			AssertEquals("If not cached, should return default value", default, result1);

			var result2 = cache.GetCachedValue("K1", () => 12345, CacheStalenessPolicy.NeverStale);
			AssertEquals("Cache created from getValueDelegate", 12345, result2);

			int result3;
			isKey1Cached = cache.TryGetValueFromCacheOnly("K1", out result3);
			AssertEquals("[After Caching] Key K1 cached?", true, isKey1Cached);
			AssertEquals("Value read from cache", result2, result3);

			// K2
			var isKey2Cached = cache.TryGetValueFromCacheOnly<int>("K2", out _);
			AssertEquals("[Before Caching] Key K2 cached?", false, isKey2Cached);
			var result4 = cache.GetCachedValue("K2", () => 9876, CacheStalenessPolicy.NeverStale);
			AssertEquals("Cache created from getValueDelegate", 9876, result4);

			int result5;
			isKey2Cached = cache.TryGetValueFromCacheOnly("K2", out result5);
			AssertEquals("[After Caching] Key K2 cached?", true, isKey2Cached);
			AssertEquals("Value read from cache", result4, result5);
		}

		public void TestGetCachedValue_StaleOnFactorySaveTogether()
		{
			var result1_1 = Factory.GetCachedValue("Key1", () => new object(), CacheStalenessPolicy.StaleOnFactorySave);
			var result2_1 = Factory.GetCachedValue("Key1", () => new object(), CacheStalenessPolicy.StaleOnFactorySave);
			var result3_1 = Factory.GetCachedValue("Key2", () => new object(), CacheStalenessPolicy.NeverStale);

			AssertEquals("Multiple accesses of same key should produce the same instance", result1_1, result2_1);
			AssertNotEquals("Different key should produce different instance", result1_1, result3_1);
			AssertNotEquals("Different key should produce different instance", result2_1, result3_1);

			BusinessObjectFactory.SaveTogether(Factory); // Simulate what happens when forms are saved.

			var result1_2 = Factory.GetCachedValue("Key1", () => new object(), CacheStalenessPolicy.StaleOnFactorySave);
			var result2_2 = Factory.GetCachedValue("Key1", () => new object(), CacheStalenessPolicy.StaleOnFactorySave);
			var result3_2 = Factory.GetCachedValue("Key2", () => new object(), CacheStalenessPolicy.NeverStale);

			AssertEquals("Multiple accesses of same key should produce the same instance", result1_2, result2_2);
			AssertNotEquals("Same key, but factory has been saved, so should produce new instance", result1_1, result1_2);
			AssertNotEquals("Same key, but factory has been saved, so should produce new instance", result2_1, result2_2);

			AssertEquals("CacheStalenessPolicy is NeverStale, so even though factory has been saved, same instance should be produced", result3_1, result3_2);
		}

		public void TestGetCachedValue_StaleOnFactorySave()
		{
			var func = new Func<object>(() => Factory.GetCachedValue("Key", () => new object(), CacheStalenessPolicy.StaleOnFactorySave));

			var value = func();

			AssertEquals("Multiple calls to the cache with the same key should yield the sale result", value, func());

			Factory.Save();

			AssertNotEquals("Calling factory.Save should clear the cache", value, func());
		}

		public void TestGetCachedValue_StaleOnFactorySave_DataRefreshBus()
		{
			var newFactory = new BusinessObjectFactory();
			var newFactoryBizo = newFactory.New<DummyBusinessObject>();
			newFactory.Save();

			var bizo = Factory.Load<DummyBusinessObject>(newFactoryBizo.PK);
			var func = new Func<object>(() => Factory.GetCachedValue("Key", () => new object(), CacheStalenessPolicy.StaleOnFactorySave));

			var value = func();
			newFactory.Save();
			newFactoryBizo.Z0_Bool = !newFactoryBizo.Z0_Bool;
			AssertNotEquals("Precondition: Z0_Bool", newFactoryBizo.Z0_Bool, bizo.Z0_Bool);
			AssertEquals("Multiple calls to the cache with the same key should yield the sale result", value, func());

			newFactory.Save();
			CombineAssertions(() =>
			{
				AssertNotEquals("Calling newFactory.Save should clear the cache if data refresh is triggered.", value, func());
				AssertEquals("Postcondition: Z0_Bool", newFactoryBizo.Z0_Bool, bizo.Z0_Bool);
			});
		}

		public void TestGetCachedValue_StaleOnFactorySaving()
		{
			var bizo_NeverStale = Factory.New<DummyBizoWithCachedStuff>();
			var bizo_StaleOnFactorySaving = Factory.New<DummyBizoWithCachedStuff>();
			var bizo_StaleOnSaved = Factory.New<DummyBizoWithCachedStuff>();

			bizo_StaleOnFactorySaving.CachePolicy = CacheStalenessPolicy.StaleBeforeFactorySavingTransaction;
			bizo_StaleOnSaved.CachePolicy = CacheStalenessPolicy.StaleOnFactorySave;

			bizo_NeverStale.PokeCache("Before Save");
			bizo_StaleOnFactorySaving.PokeCache("Before Save");
			bizo_StaleOnSaved.PokeCache("Before Save");

			Factory.Save();

			bizo_NeverStale.PokeCache("After Save");
			bizo_StaleOnFactorySaving.PokeCache("After Save");
			bizo_StaleOnSaved.PokeCache("After Save");

			AssertEquals("Before Save", bizo_NeverStale.WhenCachePoked);
			AssertEquals("After Save", bizo_StaleOnSaved.WhenCachePoked);
			AssertEquals("OnFactorySavingBeforeTransactionCore", bizo_StaleOnFactorySaving.WhenCachePoked);
		}

		public void TestGetCachedValue_StaleOnDataRowChanging()
		{
			AssertCachePolicyCausesAdditionalElementToBePresent("Default cache policy does not clear the cache when data table changes",
				Factory, null, shouldSecondDependentBePresentAfterAssociationWithParent: false);

			AssertCachePolicyCausesAdditionalElementToBePresent("StaleWhenDataTableChanges cache policy should clear the cache when data row changes",
				Factory, CacheStalenessPolicy.StaleWhenDataTableChanges(DummyDependentBizoSchema.Constants.TableName, Factory), shouldSecondDependentBePresentAfterAssociationWithParent: true);
		}

		public void TestGetCachedValue_StaleOnDataRowDeleted()
		{
			AssertCachePolicyCausesRemovedElementToNoLongerBePresent("Default cache policy does not clear the cache when data table changes",
				Factory, null, shouldSecondDependentBePresentAfterDeletion: true);

			AssertCachePolicyCausesRemovedElementToNoLongerBePresent("StaleWhenDataTableChanges cache policy should clear the cache when data row is deleted",
				Factory, CacheStalenessPolicy.StaleWhenDataTableChanges(DummyDependentBizoSchema.Constants.TableName, Factory), shouldSecondDependentBePresentAfterDeletion: false);
		}

		#region Implementation

		class DummyBizoWithCachedStuff : DummyBusinessObject
		{
			public DummyBizoWithCachedStuff(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				base.OnFactorySavingBeforeTransactionCore();

				PokeCache(nameof(OnFactorySavingBeforeTransactionCore));
			}

			internal CacheStalenessPolicy CachePolicy { get; set; }

			internal void PokeCache(string when)
			{
				Factory.GetCachedValue(CacheKey, () => { WhenCachePoked = when; return new object(); }, CachePolicy);
			}

			internal string WhenCachePoked { get; private set; }

			string CacheKey => "DummyBizoWithCachedStuff." + PK;
		}

		class DummyBizoWithCachedRelatedCollection : DummyBusinessObject
		{
			public DummyBizoWithCachedRelatedCollection(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			internal CacheStalenessPolicy CachePolicy { get; set; }

			public DummyDependantBusinessObject[] DependentBizos
			{
				get
				{
					var cacheKey = $"{nameof(DummyBizoWithCachedRelatedCollection)}.{nameof(DependentBizos)}.{PK}";

					return Factory.GetCachedValue(cacheKey, () => Factory.Load<DummyDependantBusinessObject>(new ZQuery(DummyDependentBizoSchema.ZD1_Z0, PK)), CachePolicy);
				}
			}
		}

		static void AssertCachePolicyCausesAdditionalElementToBePresent(string message, BusinessObjectFactory factory, CacheStalenessPolicy policy, bool shouldSecondDependentBePresentAfterAssociationWithParent)
		{
			var bizo = factory.New<DummyBizoWithCachedRelatedCollection>();
			var dependent1 = factory.New<DummyDependantBusinessObject>();
			var dependent2 = factory.New<DummyDependantBusinessObject>();

			bizo.CachePolicy = policy;

			dependent1.ZD1_Z0 = bizo.PK;
			AssertContainsExactElementsInAnyOrder(new[] { dependent1 }, bizo.DependentBizos);

			var matches = shouldSecondDependentBePresentAfterAssociationWithParent ? new[] { dependent1, dependent2 } : new[] { dependent1 };
			dependent2.ZD1_Z0 = bizo.PK;

			AssertContainsExactElementsInAnyOrder(matches, bizo.DependentBizos);
		}

		static void AssertCachePolicyCausesRemovedElementToNoLongerBePresent(string message, BusinessObjectFactory factory, CacheStalenessPolicy policy, bool shouldSecondDependentBePresentAfterDeletion)
		{
			var bizo = factory.New<DummyBizoWithCachedRelatedCollection>();
			var dependent1 = factory.New<DummyDependantBusinessObject>();
			var dependent2 = factory.New<DummyDependantBusinessObject>();

			bizo.CachePolicy = policy;

			dependent1.ZD1_Z0 = bizo.PK;
			dependent2.ZD1_Z0 = bizo.PK;

			AssertContainsExactElementsInAnyOrder(new[] { dependent1, dependent2 }, bizo.DependentBizos);

			dependent2.Delete();

			var matches = shouldSecondDependentBePresentAfterDeletion ? new[] { dependent1, dependent2 } : new[] { dependent1 };

			AssertContainsExactElementsInAnyOrder(matches, bizo.DependentBizos);
		}

		#endregion
	}
}
