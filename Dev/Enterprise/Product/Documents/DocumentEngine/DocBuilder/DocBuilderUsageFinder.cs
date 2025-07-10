using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class DocBuilderUsageFinder : IDocBuilderUsageFinder
	{
		public bool IsInitialized
		{
			get { return cache != null; }
		}

		public void Initialize()
		{
			if (!IsInitialized)
			{
				cache = new Dictionary<string, DocumentMacroUsageCollection>();
				var factory = new BusinessObjectFactory();
				var docBuilderTemplateTranslationHelper = new DocBuilderTemplateTranslationHelper();
				foreach (var result in docBuilderTemplateTranslationHelper.GetUniqueLabels(factory))
				{
					cache[result.Data.Key] = result.DocBuilderUsages;
				}
				foreach (var group in docBuilderTemplateTranslationHelper.GetAllResStringsUsedByBizOFields(factory).Values)
				{
					foreach (var result in group.Data)
					{
						if (!cache.ContainsKey(result.Key))
						{
							cache[result.Key] = new DocBuilderUsageCollection();
						}
						cache[result.Key].AddRange(group.DocBuilderUsages);
					}
				}
			}
		}

		public IDocBuilderUsageCollection Find(string key)
		{
			if (!IsInitialized)
			{
				throw new InvalidOperationException("DocBuilderUsageFinder is not initialized");
			}
			DocumentMacroUsageCollection result;
			cache.TryGetValue(key, out result);
			if (result == null)
			{
				result = new DocBuilderUsageCollection();
			}
			return result;
		}

		Dictionary<string, DocumentMacroUsageCollection> cache;
	}
}