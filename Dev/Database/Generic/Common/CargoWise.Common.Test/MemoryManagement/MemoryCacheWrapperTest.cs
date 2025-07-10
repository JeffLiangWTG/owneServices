using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using CargoWise.Common.MemoryManagement;
using NUnit.Framework;

namespace CargoWise.Common.Testing.MemoryManagement
{
	public class MemoryCacheWrapperTest : TestCase
	{
		public void TestAddAndGetCachedValue()
		{
			var value = memoryCache.GetCachedValue<string>("key");
			AssertNull("No value in cache yet", value);

			memoryCache.AddToCache("key", "Cache Me!");
			value = memoryCache.GetCachedValue<string>("key");

			AssertEquals("Cache Me!", value);
		}

		public void TestGetCachedOrCalculateValue()
		{
			var value = memoryCache.GetCachedValue<string>("key");
			AssertNull("No value in cache yet", value);

			value = memoryCache.GetCachedOrCalculateValue("key", () => "Cache Me!");
			AssertEquals("Cache Me!", value);

			value = memoryCache.GetCachedValue<string>("key");
			AssertEquals("Cache Me!", value);
		}

		public void TestSetInCache()
		{
			var value = memoryCache.GetCachedOrCalculateValue("key", () => "Cache Me!");
			AssertEquals("Cache Me!", value);

			value = memoryCache.GetCachedValue<string>("key");
			AssertEquals("Cache Me!", value);

			memoryCache.SetInCache("key", "I have changed!");

			value = memoryCache.GetCachedValue<string>("key");
			AssertEquals("I have changed!", value);
		}

		public void TestClear()
		{
			memoryCache.SetInCache("key1", "Cache Me 1");
			memoryCache.SetInCache("key2", "Cache Me 2");
			memoryCache.SetInCache("key3", "Cache Me 3");

			var count = MemoryCache.Default.Count(k => k.Key.StartsWith(nameof(DummyMemoryCache)));
			AssertEquals(3, count);

			memoryCache.Clear();
			count = MemoryCache.Default.Count(k => k.Key.StartsWith(nameof(DummyMemoryCache)));
			AssertEquals(0, count);
		}

		public void TestAbsoluteExpiry()
		{
			var cacheKeyExpiry = new Dictionary<string, int> { { "key1", 500 }, { "key2", 700 }, { "key3", 1200 } };

			memoryCache.MockAbsoluteExpiry = (key) => TimeSpan.FromMilliseconds(cacheKeyExpiry[key]);
			memoryCache.SetInCache("key1", "Cache Me 1");
			memoryCache.SetInCache("key2", "Cache Me 2");
			memoryCache.SetInCache("key3", "Cache Me 3");

			var value1 = memoryCache.GetCachedValue<string>("key1");
			var value2 = memoryCache.GetCachedValue<string>("key2");
			var value3 = memoryCache.GetCachedValue<string>("key3");

			AssertEquals("Cache Me 1", value1);
			AssertEquals("Cache Me 2", value2);
			AssertEquals("Cache Me 3", value3);

			Thread.Sleep(500);

			value1 = memoryCache.GetCachedValue<string>("key1");
			value2 = memoryCache.GetCachedValue<string>("key2");
			value3 = memoryCache.GetCachedValue<string>("key3");

			AssertNull(value1);
			AssertEquals("Cache Me 2", value2);
			AssertEquals("Cache Me 3", value3);

			Thread.Sleep(200);

			value1 = memoryCache.GetCachedValue<string>("key1");
			value2 = memoryCache.GetCachedValue<string>("key2");
			value3 = memoryCache.GetCachedValue<string>("key3");

			AssertNull(value1);
			AssertNull(value2);
			AssertEquals("Cache Me 3", value3);

			Thread.Sleep(500);

			value1 = memoryCache.GetCachedValue<string>("key1");
			value2 = memoryCache.GetCachedValue<string>("key2");
			value3 = memoryCache.GetCachedValue<string>("key3");

			AssertNull(value1);
			AssertNull(value2);
			AssertNull(value3);
		}

		public void TestSlidingExpiry()
		{
			var cacheKeyExpiry = new Dictionary<string, int> { { "key1", 2000 }, { "key2", 2500 } };

			memoryCache.MockSlidingExpiration = (key) => TimeSpan.FromMilliseconds(cacheKeyExpiry[key]);
			memoryCache.SetInCache("key1", "Cache Me 1");
			memoryCache.SetInCache("key2", "Cache Me 2");

			var value1 = memoryCache.GetCachedValue<string>("key1");
			var value2 = memoryCache.GetCachedValue<string>("key2");

			AssertEquals("Cache Me 1", value1);
			AssertEquals("Cache Me 2", value2);

			for (int i = 0; i < 5; i++)
			{
				value1 = memoryCache.GetCachedValue<string>("key1");
				value2 = memoryCache.GetCachedValue<string>("key2");

				Thread.Sleep(1000);

				AssertEquals("Cache Me 1", value1);
				AssertEquals("Cache Me 2", value2);
			}

			Thread.Sleep(1000);

			value1 = memoryCache.GetCachedValue<string>("key1");
			value2 = memoryCache.GetCachedValue<string>("key2");

			AssertNull(value1);
			AssertEquals("Cache Me 2", value2);

			Thread.Sleep(2500);

			value1 = memoryCache.GetCachedValue<string>("key1");
			value2 = memoryCache.GetCachedValue<string>("key2");

			AssertNull(value1);
			AssertNull(value2);
		}

		public void TestGetCalculatedValueAfterCacheExpiration()
		{
			var cacheKeyExpiry = new Dictionary<string, int> { { "key1", 50 } };

			memoryCache.MockSlidingExpiration = (key) => TimeSpan.FromMilliseconds(cacheKeyExpiry[key]);
			memoryCache.SetInCache("key1", "Cache Me 1");

			var value = memoryCache.GetCachedOrCalculateValue("key1", () => "Cache Me 2");
			AssertEquals("Should return old cache value", "Cache Me 1", value);

			memoryCache.CacheDelayForTest = TimeSpan.FromMilliseconds(100);
			value = memoryCache.GetCachedOrCalculateValue("key1", () => "Cache Me 2");
			AssertEquals("Should calculate and return new value", "Cache Me 2", value);
		}

		#region Implementation

		readonly DummyMemoryCache memoryCache = new DummyMemoryCache();

		protected override void TearDown()
		{
			base.TearDown();
			memoryCache.Clear();
			memoryCache.MockAbsoluteExpiry = null;
			memoryCache.MockSlidingExpiration = null;
		}

		class DummyMemoryCache : MemoryCacheWrapper
		{
			public Func<string, TimeSpan> MockAbsoluteExpiry { get; set; }
			public Func<string, TimeSpan> MockSlidingExpiration { get; set; }

			protected override TimeSpan GetAbsoluteExpiry(string key) => MockAbsoluteExpiry?.Invoke(key) ?? base.GetAbsoluteExpiry(key);

			protected override TimeSpan GetSlidingExpiration(string key) => MockSlidingExpiration?.Invoke(key) ?? base.GetSlidingExpiration(key);
		}

		#endregion
	}
}
