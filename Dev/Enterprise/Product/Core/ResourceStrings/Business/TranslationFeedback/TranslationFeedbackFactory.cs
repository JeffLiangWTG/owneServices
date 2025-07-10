using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public static class TranslationFeedbackFactory
	{
		public static TopLevelTranslationFeedbackCollection Get(BusinessObjectFactory factory, string language, MultilingualString multilingualString)
		{
			var results = new TopLevelTranslationFeedbackCollection(factory);
			if (multilingualString != null)
			{
				var resString = multilingualString as ResourceString;
				if (resString != null)
				{
					var source = ResourceStringsFactory.Lookup(Res.DefaultLanguage, resString.ResourceKey);
					var target = ResourceStringsFactory.LookupWithLanguageFallback(language, resString.ResourceKey);
					if (source != null && target != null)
					{
						results.Add(StmTranslationFeedback.New(factory, source, target, TranslationFeedbackMatchTypes.Codes.Exact));
						foreach (var parameter in resString.Parameters)
						{
							if (!(parameter is NoResString))
							{
								if (parameter is MultilingualString)
								{
									results.AddRange(Get(factory, language, ((MultilingualString)parameter)));
								}
								else if (parameter is string || parameter is ZString)
								{
									results.AddRange(Get(factory, language, parameter as string));
								}
							}
						}
					}
				}
				else
				{
					var modifiedString = multilingualString as ModifiedMultilingualString;
					if (modifiedString != null)
					{
						foreach (var item in modifiedString.Strings)
						{
							results.AddRange(Get(factory, language, item));
						}
					}
					else
					{
						results.AddRange(Get(factory, language, multilingualString.ToString()));
					}
				}
			}
			return results;
		}

		public static TopLevelTranslationFeedbackCollection Get(BusinessObjectFactory factory, string language, ResourceStringData resourceStringData)
		{
			return Get(factory, language, resourceStringData, null, false);
		}

		public static TopLevelTranslationFeedbackCollection Get(BusinessObjectFactory factory, string language, ResourceStringData resourceStringData, string renderedCaption)
		{
			return Get(factory, language, resourceStringData, renderedCaption, false);
		}

		public static TopLevelTranslationFeedbackCollection Get(BusinessObjectFactory factory, string language, ResourceStringData resourceStringData, string renderedCaption, bool hasAccelerators)
		{
			if (renderedCaption != null)
			{
				renderedCaption = renderedCaption.Trim();
			}
			if (!string.IsNullOrEmpty(resourceStringData.Key))
			{
				var source = ResourceStringsFactory.Lookup(Res.DefaultLanguage, resourceStringData.Key);
				var target = ResourceStringsFactory.LookupWithLanguageFallback(language, resourceStringData.Key);
				if (source != null && target != null && (string.IsNullOrEmpty(renderedCaption) || target.GetMatchingLevel(renderedCaption, hasAccelerators) != null))
				{
					var results = new TopLevelTranslationFeedbackCollection(factory);
					foreach (ICodeDescription caption in source.GetCaptions())
					{
						var feedback = StmTranslationFeedback.New(factory, source, target, caption.Code, string.Empty);
						if (feedback.PrimaryContext != null)
						{
							using (feedback.PrimaryContext.SuspendSettingHasChanges())
							{
								feedback.PrimaryContext.XQ_MatchType = (feedback.XT_SuggestedTranslation == renderedCaption || renderedCaption == null) ? TranslationFeedbackMatchTypes.Codes.Exact : TranslationFeedbackMatchTypes.Codes.RelatedLevel;
							}
						}
						results.Add(feedback);
					}
					return results;
				}
				else
				{
					return Get(factory, language, renderedCaption);
				}
			}
			else
			{
				return Get(factory, language, renderedCaption);
			}
		}

		public static TopLevelTranslationFeedbackCollection Get(BusinessObjectFactory factory, string language, ICodeDescription codeDescription)
		{
			var results = new TopLevelTranslationFeedbackCollection(factory);
			var multilingualDescription = codeDescription as IMultilingualDescription;
			if (multilingualDescription != null)
			{
				results.AddRange(Get(factory, language, multilingualDescription.MultilingualDescription));
			}
			else
			{
				results.AddRange(Get(factory, language, codeDescription.Description));
			}

			if (codeDescription is CodeDescriptionPair)
			{
				string multilingualCode = ((CodeDescriptionPair)codeDescription).MultilingualCode;
				if ((multilingualCode.Length > 3 || HasCJK(multilingualCode)) && multilingualCode != codeDescription.Description && (multilingualDescription == null || multilingualCode != multilingualDescription.MultilingualDescription.GetUnresolvedString()))
				{
					results.AddRange(Get(factory, language, ((CodeDescriptionPair)codeDescription).MultilingualCode));
				}
			}
			else
			{
				if ((codeDescription.Code.Length > 3 || HasCJK(codeDescription.Code)) && codeDescription.Code != codeDescription.Description && (multilingualDescription == null || codeDescription.Code != multilingualDescription.MultilingualDescription.GetUnresolvedString()))
				{
					results.AddRange(Get(factory, language, codeDescription.Code));
				}
			}
			return results;
		}

		public static TopLevelTranslationFeedbackCollection Get(BusinessObjectFactory factory, string language, string renderedCaption)
		{
			return GetRecursive(factory, language, renderedCaption, Res.GetRecentlyUsedKeys(), TranslationFeedbackMatchTypes.Codes.RecentlyUsed, StringSearchOptions.RecursiveFullTextSearch);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		public static TopLevelTranslationFeedbackCollection GetForWeb(BusinessObjectFactory factory, string language, string renderedCaption, IEnumerable<string> loggedUsages)
		{
			var results = GetRecursive(factory, language, renderedCaption, loggedUsages, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, StringSearchOptions.Default);
			if (results.Count == 0 && renderedCaption.EndsWith(":"))
			{
				renderedCaption = renderedCaption.TrimEnd(':');
				results = GetRecursive(factory, language, renderedCaption, loggedUsages, TranslationFeedbackMatchTypes.Codes.RecentlyUsed, StringSearchOptions.Default);
			}
			return results;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		static TopLevelTranslationFeedbackCollection GetRecursive(BusinessObjectFactory factory, string language, string renderedCaption, IEnumerable<string> keys, string matchType, StringSearchOptions options)
		{
			var results = new TopLevelTranslationFeedbackCollection(factory);
			if (!string.IsNullOrEmpty(renderedCaption))
			{
				results = GetExact(factory, language, renderedCaption, keys, matchType);
				if (results.Count == 0)
				{
					results = GetByParsingFormatting(factory, language, renderedCaption, keys, matchType, options);
				}
				if (results.Count == 0 && options == StringSearchOptions.RecursiveFullTextSearch)
				{
					results = GetExact(factory, language, renderedCaption, null, TranslationFeedbackMatchTypes.Codes.FullText);
				}
				if (results.Count == 0 && options == StringSearchOptions.RecursiveFullTextSearch)
				{
					results = GetByParsingFormatting(factory, language, renderedCaption, null, TranslationFeedbackMatchTypes.Codes.FullText, StringSearchOptions.Default);
				}
			}
			return results;
		}

		static TopLevelTranslationFeedbackCollection GetExact(BusinessObjectFactory factory, string language, string renderedCaption, IEnumerable<string> keys, string matchType)
		{
			var results = new TopLevelTranslationFeedbackCollection(factory);
			foreach (var result in GetExactMatches(factory, language, renderedCaption, keys, matchType))
			{
				results.Add(result);
			}
			return results;
		}

		static TopLevelTranslationFeedbackCollection GetByParsingFormatting(BusinessObjectFactory factory, string language, string renderedCaption, IEnumerable<string> keys, string matchType, StringSearchOptions options)
		{
			var results = new TopLevelTranslationFeedbackCollection(factory);
			foreach (var key in keys ?? ResourceStringsFactory.GetResourceStringCache(Res.DefaultLanguage).AllKeys)
			{
				var targetData = ResourceStringsFactory.LookupWithLanguageFallback(language, key);
				if (targetData != null)
				{
					foreach (ICodeDescription levelCaption in targetData.GetCaptions())
					{
						var placeHolderMatch = placeHolderRegex.Match(levelCaption.Description);
						if (placeHolderMatch.Success && HasLetter(levelCaption.Description))
						{
							string unprocessedStr = levelCaption.Description;
							StringBuilder captionMatchRegexStr = new StringBuilder(unprocessedStr.Length + 10);
							captionMatchRegexStr.Append("^");
							do
							{
								captionMatchRegexStr.Append(Regex.Escape(unprocessedStr.Substring(0, placeHolderMatch.Index)));
								captionMatchRegexStr.Append("(.*?)");
								unprocessedStr = unprocessedStr.Substring(placeHolderMatch.Index + placeHolderMatch.Length);
								placeHolderMatch = placeHolderRegex.Match(unprocessedStr);
							}
							while (placeHolderMatch.Success);
							captionMatchRegexStr.Append(Regex.Escape(unprocessedStr));
							captionMatchRegexStr.Append("$");
							var captionMatchRegex = new Regex(captionMatchRegexStr.ToString(), RegexOptions.Multiline);
							var captionMatch = captionMatchRegex.Match(renderedCaption);
							if (captionMatch.Success && !captionMatch.NextMatch().Success)
							{
								results.Add(StmTranslationFeedback.New(factory, ResourceStringsFactory.Lookup(Res.DefaultLanguage, key), targetData, levelCaption.Code, matchType));
								for (int i = 1; i < captionMatch.Groups.Count; i++)
								{
									var recursiveResults = GetRecursive(factory, language, captionMatch.Groups[i].Value, keys, matchType, options);
									results.AddRange(recursiveResults);
									if (recursiveResults.Count == 0 && keys != null && (options & StringSearchOptions.RecursiveFullTextSearch) == StringSearchOptions.RecursiveFullTextSearch)
									{
										results.AddRange(GetRecursive(factory, language, captionMatch.Groups[i].Value, null, TranslationFeedbackMatchTypes.Codes.FullText, options));
									}
								}
							}
						}
					}
				}
			}

			return results;
		}

		[Flags]
		enum StringSearchOptions
		{
			Default = 0,
			RecursiveFullTextSearch = 0x1,
		}

		internal static IEnumerable<StmTranslationFeedback> GetExactMatches(BusinessObjectFactory factory, string language, string caption, string matchType)
		{
			return CacheLookup(factory, language, caption, matchType);
		}

		internal static IEnumerable<StmTranslationFeedback> GetExactMatches(BusinessObjectFactory factory, string language, string caption, IEnumerable<string> keys, string matchType)
		{
			if (keys == null)
			{
				foreach (var match in CacheLookup(factory, language, caption, matchType))
				{
					yield return match;
				}
			}
			else
			{
				foreach (var key in keys)
				{
					var targetData = ResourceStringsFactory.LookupWithLanguageFallback(language, key);
					if (targetData != null)
					{
						foreach (ICodeDescription levelCaption in targetData.GetCaptions())
						{
							if (levelCaption.Description == caption)
							{
								yield return StmTranslationFeedback.New(factory, ResourceStringsFactory.Lookup(Res.DefaultLanguage, key), targetData, levelCaption.Code, matchType);
							}
						}
					}
				}
			}
		}

		static bool HasLetter(string s)
		{
			foreach (var c in s)
			{
				if (char.IsLetter(c))
				{
					return true;
				}
			}
			return false;
		}

		static bool HasCJK(string s)
		{
			foreach (var c in s)
			{
				if (char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter)
				{
					return true;
				}
			}
			return false;
		}

		static readonly Regex placeHolderRegex = new Regex(@"\{[0-9]+.*?\}", RegexOptions.Multiline | RegexOptions.Compiled);

		#region Caption Cache

		static IEnumerable<StmTranslationFeedback> CacheLookup(BusinessObjectFactory factory, string language, string caption, string matchType)
		{
			List<LanguageCacheEntry> cacheHit;
			GetLanguageCache(language).TryGetValue(caption, out cacheHit);
			if (cacheHit != null)
			{
				foreach (var entry in cacheHit)
				{
					yield return StmTranslationFeedback.New(factory, language, entry.Key, entry.Level, matchType);
				}
			}
		}

		internal static IEnumerable<StmTranslationFeedback> CacheLookupBySource(BusinessObjectFactory factory, string language, string caption, string matchType)
		{
			List<LanguageCacheEntry> cacheHit;
			GetLanguageCache(Res.DefaultLanguage).TryGetValue(caption, out cacheHit);
			if (cacheHit != null)
			{
				foreach (var entry in cacheHit)
				{
					yield return StmTranslationFeedback.New(factory, language, entry.Key, entry.Level, matchType);
				}
			}
		}

		static Dictionary<string, List<LanguageCacheEntry>> GetLanguageCache(string language)
		{
			Dictionary<string, List<LanguageCacheEntry>> languageCache;
			captionCache.TryGetValue(language, out languageCache);
			if (languageCache == null)
			{
				LoadCaptionCache(language);
				languageCache = captionCache[language];
			}
			return languageCache;
		}

		static void LoadCaptionCache(string language)
		{
			LoadCaptionCache(language, ResourceStringsFactory.GetResourceStringCache(Res.DefaultLanguage).AllKeys);
		}

		static void LoadCaptionCache(string language, IEnumerable<string> keys)
		{
			Dictionary<string, List<LanguageCacheEntry>> languageCache;
			if (!captionCache.TryGetValue(language, out languageCache))
			{
				languageCache = new Dictionary<string, List<LanguageCacheEntry>>();
				captionCache.Add(language, languageCache);
			}
			foreach (string key in keys)
			{
				var targetData = ResourceStringsFactory.LookupWithLanguageFallback(language, key);
				if (targetData != null)
				{
					foreach (ICodeDescription levelCaption in targetData.GetCaptions())
					{
						List<LanguageCacheEntry> entryKeys;
						languageCache.TryGetValue(levelCaption.Description, out entryKeys);
						if (entryKeys == null)
						{
							entryKeys = new List<LanguageCacheEntry>();
							languageCache.Add(levelCaption.Description, entryKeys);
						}
						entryKeys.Add(new LanguageCacheEntry(key, levelCaption.Code));
					}
				}
			}
		}

		internal static void UpdateCache(TopLevelTranslationFeedbackCollection savedFeedbacks)
		{
			var allKeys = new Dictionary<string, HashSet<string>>();
			foreach (StmTranslationFeedback savedFeedback in savedFeedbacks)
			{
				Dictionary<string, List<LanguageCacheEntry>> languageCache;
				captionCache.TryGetValue(savedFeedback.XT_Language, out languageCache);
				if (languageCache != null)
				{
					RemoveFromCache(languageCache, savedFeedback.XT_OriginalTranslation, savedFeedback.AllContexts);
					RemoveFromCache(languageCache, savedFeedback.XT_SuggestedTranslation, savedFeedback.AllContexts);
					HashSet<string> keys;
					if (!allKeys.TryGetValue(savedFeedback.XT_Language, out keys))
					{
						keys = new HashSet<string>();
						allKeys.Add(savedFeedback.XT_Language, keys);
					}
					foreach (StmTranslationFeedbackResource context in savedFeedback.AllContexts)
					{
						if (context.Update)
						{
							keys.Add(context.XQ_ResourceStringKey);
						}
					}
				}
			}
			foreach (var keysEntry in allKeys)
			{
				LoadCaptionCache(keysEntry.Key, keysEntry.Value);
			}
		}

		static void RemoveFromCache(Dictionary<string, List<LanguageCacheEntry>> languageCache, string value, StmTranslationFeedbackResourceCollection resources)
		{
			List<LanguageCacheEntry> cacheHit;
			if (languageCache.TryGetValue(value, out cacheHit))
			{
				foreach (StmTranslationFeedbackResource contextItem in resources)
				{
					if (contextItem.Update)
					{
						var entry = cacheHit.Find(item => item.Key == contextItem.XQ_ResourceStringKey && item.Level == contextItem.XQ_ResourceStringLevel);
						if (!string.IsNullOrEmpty(entry.Key))
						{
							cacheHit.Remove(entry);
						}
					}
				}
			}
		}

		static readonly Dictionary<string, Dictionary<string, List<LanguageCacheEntry>>> captionCache = new Dictionary<string, Dictionary<string, List<LanguageCacheEntry>>>();

		struct LanguageCacheEntry
		{
			public LanguageCacheEntry(string key, string level)
			{
				this.Key = key;
				this.Level = level;
			}

			public readonly string Key;
			public readonly string Level;
		}

#if DEBUG
		public static void ClearCaptionCache()
		{
			captionCache.Clear();
		}
#endif

		#endregion

		public static TopLevelTranslationFeedbackCollection Search(BusinessObjectFactory factory, TranslationSearchCriteria searchCriteria)
		{
			string language = searchCriteria.TargetLanguage;

			TopLevelTranslationFeedbackCollection results = new TopLevelTranslationFeedbackCollection(factory);
			Regex targetRegex = null;
			if (searchCriteria.SearchTarget)
			{
				targetRegex = CreateRegex(searchCriteria.TargetText, searchCriteria.MatchCase, searchCriteria.MatchWord, searchCriteria.UseRegularExpressions);
			}
			if (searchCriteria.SearchSource)
			{
				var cache = GetLanguageCache(Res.DefaultLanguage);
				var sourceRegex = CreateRegex(searchCriteria.SourceText, searchCriteria.MatchCase, searchCriteria.MatchWord, searchCriteria.UseRegularExpressions);
				foreach (var match in Search(factory, language, cache, sourceRegex))
				{
					if (!searchCriteria.SearchTarget || targetRegex.IsMatch(match.XT_OriginalTranslation))
					{
						results.Add(match);
					}
				}
			}
			else
			{
				var cache = GetLanguageCache(language);
				results.AddRange(Search(factory, language, cache, targetRegex));
			}
			if (searchCriteria.Replace)
			{
				foreach (StmTranslationFeedback feedback in results)
				{
					feedback.XT_SuggestedTranslation = targetRegex.Replace(feedback.XT_SuggestedTranslation, searchCriteria.ReplacementText);
				}
			}

			return results;
		}

		static Regex CreateRegex(string searchText, bool matchCase, bool matchWholeWord, bool useRegularExpressions)
		{
			string regex = useRegularExpressions ? searchText : Regex.Escape(searchText);
			if (matchWholeWord)
			{
				regex = @"\b" + regex + @"\b";
			}
			return new Regex(regex, RegexOptions.Compiled | RegexOptions.Multiline | (matchCase ? RegexOptions.None : RegexOptions.IgnoreCase));
		}

		static IEnumerable<StmTranslationFeedback> Search(BusinessObjectFactory factory, string language, Dictionary<string, List<LanguageCacheEntry>> cache, Regex regex)
		{
			foreach (var cacheEntry in cache)
			{
				if (regex.IsMatch(cacheEntry.Key))
				{
					foreach (var entry in cacheEntry.Value)
					{
						yield return StmTranslationFeedback.New(factory, language, entry.Key, entry.Level, TranslationFeedbackMatchTypes.Codes.None);
					}
				}
			}
		}
	}
}
