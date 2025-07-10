using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Moq;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Cache.Testing
{
	public class MultiLevelResourceStringCacheTest : TestCase
	{
		public void TestLanguagOfMultiLevelCacheIsTheLangaugeOfPrimaryCache()
		{
			var mockery = new MockRepository(MockBehavior.Default);
			var primaryCache = mockery.Create<IResourceStringCache>();
			var secondaryCache = mockery.Create<IResourceStringCache>();
			primaryCache.Setup(m => m.Language).Returns("TTT");
			var cache = new MultiLevelResourceStringCache(new[] { primaryCache.Object, secondaryCache.Object });

			AssertEquals("TTT", cache.Language);
		}

		public void TestChecksEachCacheUntilResourceIsFound()
		{
			string key = "test";
			var mockery = new MockRepository(MockBehavior.Default);
			var primaryCache = mockery.Create<IResourceStringCache>();
			var secondaryCache = mockery.Create<IResourceStringCache>();
			var cache = new MultiLevelResourceStringCache(new[] { primaryCache.Object, secondaryCache.Object });
			var data = new ResourceStringData("", "", "", "test", "test");

			primaryCache.Setup(m => m.Get(0, key)).Returns((ResourceStringData)null);
			secondaryCache.Setup(m => m.Get(0, key)).Returns(data);

			AssertEquals(data, cache.Get(0, key));
		}

		public void TestEmptyResourcesInPrimaryCacheOverridesThoseInSecondaryCache()
		{
			string key = "test";
			var mockery = new MockRepository(MockBehavior.Default);
			var primaryCache = mockery.Create<IResourceStringCache>();
			var secondaryCache = mockery.Create<IResourceStringCache>();
			var cache = new MultiLevelResourceStringCache(new[] { primaryCache.Object, secondaryCache.Object });

			primaryCache.Setup(m => m.Get(0, key)).Returns(new ResourceStringData());

			AssertEquals(null, cache.Get(0, key));
			secondaryCache.Verify(m => m.Get(0, key), Times.Never);
		}

		public void TestMultipleLanguages()
		{
			MockResourceStringCache fallbackCache = new MockResourceStringCache("ENG");
			fallbackCache.Put("K1", new ResourceStringData("k1", "Eng1"));
			fallbackCache.Put("K2", new ResourceStringData("k2", "Eng2"));
			fallbackCache.Put("K3", new ResourceStringData("k3", "Eng3"));
			fallbackCache.Put("K4", new ResourceStringData("k4", "Eng4"));

			MockResourceStringCache primaryCache = new MockResourceStringCache("CHS");
			primaryCache.Put("K1", new ResourceStringData("k1", "Chs1"));
			primaryCache.Put("K3", new ResourceStringData("k3", "Chs3"));

			IResourceStringCache multiLanguageResourceStringCache = new MultiLevelResourceStringCache(new IResourceStringCache[] { primaryCache, fallbackCache });

			AssertEquals("Chs1", multiLanguageResourceStringCache.Get(0, "K1").Caption);
			AssertEquals("Eng2", multiLanguageResourceStringCache.Get(0, "K2").Caption);
			AssertEquals("Chs3", multiLanguageResourceStringCache.Get(0, "K3").Caption);
			AssertEquals("Eng4", multiLanguageResourceStringCache.Get(0, "K4").Caption);
		}
	}
}
