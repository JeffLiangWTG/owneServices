using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

#if DEBUG
using CargoWiseOne.ResourceStrings.Testing;
#endif

namespace Enterprise.ResourceStrings.Business
{
	public class ResourceStringCacheBuilder : IResourceStringCacheBuilder
	{
		protected ResourceStringCacheBuilder() { }

		public static ResourceStringCacheBuilder Instance
		{
			get { return instance; }
		}

		[CargoWise.Common.Testing.SuppressThreadStaticFieldMessage]
		static readonly ResourceStringCacheBuilder instance = new ResourceStringCacheBuilder();

		#region Implementation of IResourceStringCacheBuilder

		public IResourceStringCache GetResourceStringCache(string language)
		{
			ResourceLanguage resLanguage = null;
			if (!DesignModeFinder.IsDesigning && !DataFile.IsLanguageFileExists(language))
			{
				resLanguage = LanguageHelper.GetCustomLanguageByLanguageCode(language)?.GetResourceLanguage();
			}

			if (resLanguage == null)
			{
				resLanguage = new ResourceLanguage(language, null);
			}
			return GetResourceStringCache(resLanguage);
		}

		public IResourceStringCache GetResourceStringCache(ResourceLanguage language)
		{
			return InternalCacheBuilder.GetResourceStringCache(language);
		}

		public Dictionary<string, IResourceStringCache> GetResourceStringCaches(ResourceLanguage[] languages)
		{
			return InternalCacheBuilder.GetResourceStringCaches(languages);
		}

		public ISimpleResourceStringCache GetResourceStringResearch(string language)
		{
			if (language == Res.DefaultLanguage)
			{
				return ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(language);
			}

			var mainCache = GetResourceStringCache(language);
			var multilingualCache = mainCache as MultiLevelResourceStringCache;
			if (multilingualCache != null)
			{
				return new SimpleMultiLevelResourceStringCache(GetResourceStringCacheList(multilingualCache).ToArray());
			}
			else
			{
				return ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(language);
			}
		}

		List<ISimpleResourceStringCache> GetResourceStringCacheList(MultiLevelResourceStringCache sourceCache)
		{
			var result = new List<ISimpleResourceStringCache>();
			foreach (var cache in sourceCache.Caches)
			{
				if (cache is MultiLevelResourceStringCache multiLevelResource)
				{
					result.AddRange(GetResourceStringCacheList(multiLevelResource));
				}
				else if (cache is ZrsResourceStringCache)
				{
					result.Add(ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(cache.Language));
				}
				else if (cache is ISimpleResourceStringCache simpleResourceStringCache)
				{
					result.Add(simpleResourceStringCache);
				}
			}
			return result;
		}

		public void ResetCache(string language)
		{
			InternalCacheBuilder.ResetCache(language);
		}

		public void Reset()
		{
			internalCacheBuilder = null;
		}

		bool isTranslationFeedbackResourceExcluded;
		internal IDisposable ExcludeTranslationFeedbackResource()
		{
			Reset();
			isTranslationFeedbackResourceExcluded = true;
			return new DisposableAction(() =>
			{
				Reset();
				isTranslationFeedbackResourceExcluded = false;
			});
		}

		#endregion

		#region InnerResourceStringCacheBuilder

		IResourceStringCacheBuilder InternalCacheBuilder
		{
			get
			{
				return internalCacheBuilder ??
					(internalCacheBuilder = isTranslationFeedbackResourceExcluded ?
						new CargoWiseOne.ResourceStrings.ResourceStringCacheBuilder(
						new MultipleResourceStringCacheGetter[] { ResourcesDeltaSource.CreateAll },
						new ResourceStringSourceGetter[] { ResourcesDeltaSource.Create }) :
						new CargoWiseOne.ResourceStrings.ResourceStringCacheBuilder(
						new MultipleResourceStringCacheGetter[] { ResourcesDeltaSource.CreateAll, TranslationFeedbackResourceStringsSource.CreateAll },
						new ResourceStringSourceGetter[] { ResourcesDeltaSource.Create, TranslationFeedbackResourceStringsSource.Create })
						);
			}
		}
		IResourceStringCacheBuilder internalCacheBuilder;

		#endregion

#if DEBUG

		public DisposableAction MockSources()
		{
			if (IsMocking)
			{
				return new DisposableAction(() => { });
			}
			mockSystems = new Dictionary<string, MockResourceStringCache>();
			mockDeltas = new Dictionary<string, ResourcesDeltaSource>();
			internalCacheBuilder = new CargoWiseOne.ResourceStrings.ResourceStringCacheBuilder(
				GetMockSystem,
				new MultipleResourceStringCacheGetter[] { GetMockAllDelta, TranslationFeedbackResourceStringsSource.CreateAll },
				new ResourceStringSourceGetter[] { GetMockDelta, TranslationFeedbackResourceStringsSource.Create });
			researchOverride = ResourceStringsResearch.OverrideInstance(new MockResearch());
			return new DisposableAction(StopMocking);
		}

		void StopMocking()
		{
			internalCacheBuilder = null;
			mockSystems = null;
			foreach (var mockDelta in mockDeltas.Values)
			{
				mockDelta.Delete();
			}
			mockDeltas = null;
			researchOverride.Dispose();
		}

		bool IsMocking
		{
			get { return mockDeltas != null; }
		}

		MockResourceStringCache GetMockSystem(string language)
		{
			MockResourceStringCache mockSystem;
			if (!mockSystems.TryGetValue(language, out mockSystem))
			{
				mockSystem = new MockResourceStringCache(language);
				mockSystems.Add(language, mockSystem);
			}
			return mockSystem;
		}

		ResourcesDeltaSource GetMockDelta(string language)
		{
			ResourcesDeltaSource mockDelta;
			if (!mockDeltas.TryGetValue(language, out mockDelta))
			{
				mockDelta = ResourcesDeltaSource.CreateMock(language);
				mockDeltas.Add(language, mockDelta);
			}
			return mockDelta;
		}

		Dictionary<string, ResourceStringSourceDataPair> GetMockAllDelta(string[] languages)
		{
			var result = new Dictionary<string, ResourceStringSourceDataPair>();
			foreach (var language in languages)
			{
				if (!mockDeltas.ContainsKey(language))
				{
					var mockDelta = ResourcesDeltaSource.CreateMock(language);
					mockDeltas[language] = mockDelta;
					result[language] = new ResourceStringSourceDataPair(mockDelta, mockDelta.ReadAll());
				}
			}
			return result;
		}

		Dictionary<string, ResourcesDeltaSource> mockDeltas;
		Dictionary<string, MockResourceStringCache> mockSystems;
		IDisposable researchOverride;

		class MockResearch : ResourceStringsResearch
		{
			public override ISimpleResourceStringCache GetSystemDefinedResourceStrings(string language)
			{
				return ResourceStringCacheBuilder.Instance.GetMockSystem(language);
			}
		}

#endif
	}
}
