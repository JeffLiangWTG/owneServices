using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class ResourceStringCacheBuilderTest : TransactionedTestCase
	{
		public void TestGetResourceStringCache()
		{
			AssertNotNull(ResourceStringCacheBuilder.Instance.GetResourceStringCache(Core.SharedConstants.Languages.EnglishAmerican));
		}

		public void TestResourceStringResearchNoExceptionThrownWithEmptyResourceStringCache()
		{
			var factory = new BusinessObjectFactory();
			var testLanguage = factory.New<RefLocalLanguage>();
			testLanguage.RA_Code = "EN";
			testLanguage.RA_RN_NKCountryCode = "CA";
			testLanguage.RA_Description = "English (Canada)";
			factory.Save();
			AssertNoExceptionThrown(() => ResourceStringCacheBuilder.Instance.GetResourceStringResearch(testLanguage.FullLanguageCode));
		}

		public void TestResGetStringIsNotSlowerThanTheCache()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
			{
				var cacheStopwatch = new Stopwatch();
				var resStopwatch = new Stopwatch();
				var allKeys = ResourceStringCacheBuilder.Instance.GetResourceStringResearch(Core.SharedConstants.Languages.French).AllKeys;
				var cache = ResourceStringCacheBuilder.Instance.GetResourceStringCache(Core.SharedConstants.Languages.French);

				for (UInt16 i = 1; i < UInt16.MaxValue; i++)
				{
					Res._GetString(i, "?", "?");
				}

				int trials = 10;
				for (int i = 0; i < trials; i++)
				{
					cacheStopwatch.Start();
					foreach (var key in allKeys)
					{
						cache.Get(ResourceStringAssemblyIdAttribute.IKnowTheAssemblyIsAlreadyLoaded, key);
					}
					cacheStopwatch.Stop();
					if (i == 0)
					{
						cacheStopwatch.Reset();
					}

					resStopwatch.Start();
					foreach (var key in allKeys)
					{
						Res._GetData(ResourceStringAssemblyIdAttribute.IKnowTheAssemblyIsAlreadyLoaded, key, string.Empty);
					}
					resStopwatch.Stop();
					if (i == 0)
					{
						resStopwatch.Reset();
					}
				}

				long difference = (100 * (resStopwatch.ElapsedTicks - cacheStopwatch.ElapsedTicks)) / cacheStopwatch.ElapsedTicks;
				Assert("Expected less than 150% difference in time between Res.GetData() and direct cache access, actual difference was " + difference + "%\r\ncacheStopwatch: " + cacheStopwatch.ElapsedTicks + "\r\nresStopwatch: " + resStopwatch.ElapsedTicks, difference <= 150);
			}
		}

		public void TestDatabaseResourceStringSource()
		{
			var defaultAssemblyLoader = AssemblyLoader.Instance;
			using (var tempDirectory = new TempDirectory())
			{
				try
				{
					AssemblyLoader.Instance = new AssemblyLoaderForTest(defaultAssemblyLoader, tempDirectory.DirectoryName);
					IResourceStringCache cache;
					ResourceStringCacheBuilder.Instance.Reset();
					cache = ResourceStringCacheBuilder.Instance.GetResourceStringCache(Core.SharedConstants.Languages.EnglishAmerican);
					AssertType(typeof(EmptyResourceStringCache), cache);
					cache = ResourceStringCacheBuilder.Instance.GetResourceStringCache(Core.SharedConstants.Languages.French);
					var item = (SimpleResourceStringCache)((MultiLevelResourceStringCache)cache).Caches[0];
					AssertType(typeof(DatabaseResourceStringSource), item.Source);
					AssertEquals(Core.SharedConstants.Languages.French, item.Language);
				}
				finally
				{
					AssemblyLoader.Instance = defaultAssemblyLoader;
					ResourceStringCacheBuilder.Instance.Reset();
				}
			}
		}

		public void TestSourceHeirarchy()
		{
			var cache = ResourceStringCacheBuilder.Instance.GetResourceStringCache(Core.SharedConstants.Languages.French) as MultiLevelResourceStringCache;
			AssertEquals(3, cache.Caches.Length);
			Assert(((SimpleResourceStringCache)cache.Caches[0]).Source is ResourcesDeltaSource || ((SimpleResourceStringCache)cache.Caches[0]).Source is DatabaseResourceStringSource);
			AssertType(typeof(TranslationFeedbackResourceStringsSource), ((SimpleResourceStringCache)cache.Caches[1]).Source);
			AssertType(typeof(ZrsResourceStringCache), cache.Caches[2]);
		}

		public void TestCustomLanguageCache()
		{
			var factory = new BusinessObjectFactory();

			var testLanguage1 = factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			factory.Save();

			var testLanguage2 = factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			testLanguage2.RA_RA_ParentLanguage = testLanguage1.PK;
			factory.Save();

			var testLanguage3 = factory.New<IRefLocalLanguage>();
			testLanguage3.RA_Code = "CC";
			testLanguage3.RA_RN_NKCountryCode = "CN";
			testLanguage3.RA_Description = "Test Language3";
			testLanguage3.RA_RA_ParentLanguage = testLanguage2.PK;
			factory.Save();

			var cache = ResourceStringCacheBuilder.Instance.GetResourceStringCache(testLanguage3.FullLanguageCode) as MultiLevelResourceStringCache;
			AssertEquals(3, cache.Caches.Length);
			AssertType(typeof(DatabaseResourceStringSource), ((SimpleResourceStringCache)cache.Caches[0]).Source);
			AssertType(typeof(TranslationFeedbackResourceStringsSource), ((SimpleResourceStringCache)cache.Caches[1]).Source);
			AssertType(typeof(MultiLevelResourceStringCache), cache.Caches[2]);
			AssertEquals("Language of first two caches should be current language", testLanguage3.FullLanguageCode, ((SimpleResourceStringCache)cache.Caches[0]).Source.Language);
			AssertEquals("Language of first two caches should be current language", testLanguage3.FullLanguageCode, ((SimpleResourceStringCache)cache.Caches[1]).Source.Language);

			var parentLanguageMultiCache = (MultiLevelResourceStringCache)cache.Caches[2];
			AssertEquals("Language of next two caches should be first parent language", testLanguage2.FullLanguageCode, parentLanguageMultiCache.Caches[0].Language);
			AssertEquals("Language of next two caches should be first parent language", testLanguage2.FullLanguageCode, parentLanguageMultiCache.Caches[1].Language);

			var baseLanguageMultiCache = (MultiLevelResourceStringCache)parentLanguageMultiCache.Caches[2];
			AssertEquals("Language of last two caches should be second parent language", testLanguage1.FullLanguageCode, baseLanguageMultiCache.Caches[0].Language);
			AssertEquals("Language of last two caches should be second parent language", testLanguage1.FullLanguageCode, baseLanguageMultiCache.Caches[1].Language);
			AssertType(typeof(ZrsResourceStringCache), baseLanguageMultiCache.Caches[2]);
			AssertEquals("Language of Zrs cache should be base system language", Core.SharedConstants.Languages.English, ((ZrsResourceStringCache)baseLanguageMultiCache.Caches[2]).Language);
		}

		public void TestGetCacheForMultipleLanguages()
		{
			var factory = new BusinessObjectFactory();

			var testLanguage1 = factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			factory.Save();

			var testLanguage2 = factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			testLanguage2.RA_RA_ParentLanguage = testLanguage1.PK;
			factory.Save();

			var testLanguage3 = factory.New<IRefLocalLanguage>();
			testLanguage3.RA_Code = "CC";
			testLanguage3.RA_RN_NKCountryCode = "CN";
			testLanguage3.RA_Description = "Test Language3";
			testLanguage3.RA_RA_ParentLanguage = testLanguage2.PK;
			factory.Save();

			var languages = new ResourceLanguage[] { new ResourceLanguage(Core.SharedConstants.Languages.French, null), testLanguage3.GetResourceLanguage() };
			var cache = ResourceStringCacheBuilder.Instance.GetResourceStringCaches(languages);

			AssertEquals(2, cache.Count);
			Assert(cache.ContainsKey(Core.SharedConstants.Languages.French) && cache.ContainsKey(testLanguage3.FullLanguageCode));

			var frenchCache = cache[Core.SharedConstants.Languages.French] as MultiLevelResourceStringCache;
			AssertEquals(3, frenchCache.Caches.Length);
			Assert(((SimpleResourceStringCache)frenchCache.Caches[0]).Source is ResourcesDeltaSource || ((SimpleResourceStringCache)frenchCache.Caches[0]).Source is DatabaseResourceStringSource);
			AssertType(typeof(TranslationFeedbackResourceStringsSource), ((SimpleResourceStringCache)frenchCache.Caches[1]).Source);
			AssertType(typeof(ZrsResourceStringCache), frenchCache.Caches[2]);

			var customLanguageCache = cache[testLanguage3.FullLanguageCode] as MultiLevelResourceStringCache;
			AssertEquals(3, customLanguageCache.Caches.Length);
			AssertType(typeof(DatabaseResourceStringSource), ((SimpleResourceStringCache)customLanguageCache.Caches[0]).Source);
			AssertType(typeof(TranslationFeedbackResourceStringsSource), ((SimpleResourceStringCache)customLanguageCache.Caches[1]).Source);
			AssertType(typeof(MultiLevelResourceStringCache), customLanguageCache.Caches[2]);
			AssertEquals("Language of first two caches should be current language", testLanguage3.FullLanguageCode, ((SimpleResourceStringCache)customLanguageCache.Caches[0]).Source.Language);
			AssertEquals("Language of first two caches should be current language", testLanguage3.FullLanguageCode, ((SimpleResourceStringCache)customLanguageCache.Caches[1]).Source.Language);

			var parentLanguageMultiCache = (MultiLevelResourceStringCache)customLanguageCache.Caches[2];
			AssertEquals("Language of next two caches should be first parent language", testLanguage2.FullLanguageCode, parentLanguageMultiCache.Caches[0].Language);
			AssertEquals("Language of next two caches should be first parent language", testLanguage2.FullLanguageCode, parentLanguageMultiCache.Caches[1].Language);

			var baseLanguageMultiCache = (MultiLevelResourceStringCache)parentLanguageMultiCache.Caches[2];
			AssertEquals("Language of last two caches should be second parent language", testLanguage1.FullLanguageCode, baseLanguageMultiCache.Caches[0].Language);
			AssertEquals("Language of last two caches should be second parent language", testLanguage1.FullLanguageCode, baseLanguageMultiCache.Caches[1].Language);
			AssertType(typeof(ZrsResourceStringCache), baseLanguageMultiCache.Caches[2]);
			AssertEquals("Language of Zrs cache should be base system language", Core.SharedConstants.Languages.English, ((ZrsResourceStringCache)baseLanguageMultiCache.Caches[2]).Language);
		}

		public void TestExcludeTranslationFeedbackResource()
		{
			var cacheBuilder = ResourceStringCacheBuilder.Instance;
			var cache = cacheBuilder.GetResourceStringCache(Core.SharedConstants.Languages.French) as MultiLevelResourceStringCache;
			AssertEquals(3, cache.Caches.Length);
			Assert("ResourceStringsCache should have TranslationFeedbackResourceStringsSource by default.", cache.Caches.OfType<SimpleResourceStringCache>().Any(c => c.Source is TranslationFeedbackResourceStringsSource));
			using (cacheBuilder.ExcludeTranslationFeedbackResource())
			{
				cache = cacheBuilder.GetResourceStringCache(Core.SharedConstants.Languages.French) as MultiLevelResourceStringCache;
				AssertEquals(2, cache.Caches.Length);
				Assert("ResourceStringsCache should have TranslationFeedbackResourceStringsSource excluded.", !cache.Caches.OfType<SimpleResourceStringCache>().Any(c => c.Source is TranslationFeedbackResourceStringsSource));
			}
			cache = cacheBuilder.GetResourceStringCache(Core.SharedConstants.Languages.French) as MultiLevelResourceStringCache;
			AssertEquals(3, cache.Caches.Length);
			Assert("ResourceStringsCache should have TranslationFeedbackResourceStringsSource included again.", cache.Caches.OfType<SimpleResourceStringCache>().Any(c => c.Source is TranslationFeedbackResourceStringsSource));
		}

		class AssemblyLoaderForTest : IAssemblyLoader
		{
			public AssemblyLoaderForTest(IAssemblyLoader defaultAssemblyLoader, string path)
			{
				this.path = path;
				this.defaultAssemblyLoader = defaultAssemblyLoader;
			}

			readonly string path;
			readonly IAssemblyLoader defaultAssemblyLoader;

			public string GetBinPath()
			{
				return path;
			}

			public string GetParentBinPath()
			{
				return Directory.GetParent(path)?.FullName;
			}

			public System.Reflection.Assembly LoadAssembly(System.Reflection.AssemblyName assemblyName)
			{
				return defaultAssemblyLoader.LoadAssembly(assemblyName);
			}
		}
	}
}
