using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using CargoWise.Common.Cache;
using NUnit.Framework;

namespace Testing
{
	public class IObjectCacheExtensionsTest : TestCase
	{
		public void TestObjectCacheExtensions()
		{
			var listToCache = new List<int> { 0, 1, 2, 3 };
			Func<List<int>> list = () => listToCache;
			var cachedList = MemoryCache.Default.GetOrAdd("cacheKey", list, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1) });
			AssertArrayEqualsByElements(listToCache.ToArray(), cachedList.ToArray());
		}
	}
}