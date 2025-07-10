using NUnit.Framework;

namespace Enterprise.Builder.Generator.Cache.Testing
{
	sealed class GenericCacheTest : TestCase
	{
		public void TestCacheEnrichment()
		{
			//Arrange
			var cache = new GenericCacheForTest();
			var initialCount = cache.ExposedCache.Count;
			var cacheId = "ABC";

			//Act
			var result = cache.GetSomethingForTest(cacheId);

			//Assert
			Assert(cache.ExposedCache.ContainsKey(cacheId));
			AssertEquals(cacheId, cache.ExposedCache[cacheId]);
			AssertEquals(cacheId, result);
			AssertEquals(0, initialCount);
			AssertEquals(1, cache.ExposedCache.Count);
		}

		public void TestPersistenceOfCachedValue()
		{
			//Arrange
			var cache = new GenericCacheForTest();
			var initialCount = cache.ExposedCache.Count;
			var cacheId = "ABC";

			//Act
			cache.GetSomethingForTest(cacheId);
			// repeat
			var result = cache.GetSomethingForTest(cacheId);

			//Assert
			Assert(cache.ExposedCache.ContainsKey(cacheId));
			AssertEquals(cacheId, cache.ExposedCache[cacheId]);
			AssertEquals(cacheId, result);
			AssertEquals(0, initialCount);
			AssertEquals(1, cache.ExposedCache.Count);
		}
	}
}
