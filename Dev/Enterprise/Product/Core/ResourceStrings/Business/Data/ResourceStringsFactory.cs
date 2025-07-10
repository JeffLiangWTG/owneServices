using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

#if DEBUG
using CargoWiseOne.ResourceStrings.Testing;
using WTG.StaticAnalysis.Annotation;
#endif

namespace Enterprise.ResourceStrings.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class ResourceStringsFactory
	{
		public static HelpDataString Lookup(string language, string key)
		{
			var metaData = ResourceStringsMetaData.GetInstance();
			var result = Lookup(language, key, true, metaData) ?? Lookup(language, key, false, metaData);
			return result;
		}

		public static HelpDataString Lookup(string language, string key, bool isCheckedOut)
		{
			return Lookup(language, key, isCheckedOut, ResourceStringsMetaData.GetInstance());
		}

		static HelpDataString Lookup(string language, string key, bool isCheckedOut, ResourceStringsMetaData metaData)
		{
			HelpDataString result = null;
			foreach (var cache in GetLeafCaches(GetResourceStringCache(language)))
			{
				if (cache.Language == language && (isCheckedOut ^ !IsCheckedOutSource(cache)))
				{
					var match = cache.Get(key);
					if (match != null)
					{
						result = LoadBusinessObject(language, isCheckedOut, !IsEditableSource(cache), match, metaData);
						break;
					}
				}
			}
			return result;
		}

		public static HelpDataString LookupWithLanguageFallback(string language, string key)
		{
			HelpDataString result = Lookup(language, key);
			if (result == null)
			{
				result = Lookup(Res.DefaultLanguage, key);
				if (result != null)
				{
					using (result.GetValidationSuspender())
					{
						result.HD_Language = language;
						result.HasChanges = false;
					}
				}
			}
			return result;
		}

		public static HelpDataString[] Load(ZQuery query)
		{
			var list = new List<HelpDataString>();
			var metaData = ResourceStringsMetaData.GetInstance();
			query.Simplify();
			var filterParts = ((IFilterPartsProvider)query).FilterParts;
			foreach (string language in DataFile.GetAvailableLanguages())
			{
				foreach (var cache in GetLeafCaches(GetResourceStringCache(language)))
				{
					if (cache.Language == language)
					{
						bool isCheckedOut = IsCheckedOutSource(cache);
						bool isReadOnly = !IsEditableSource(cache);
						foreach (var key in cache.AllKeys)
						{
							var data = cache.Get(key);
							HelpDataString bizo = null;
							if (IsMatch(language, isCheckedOut, isReadOnly, data, filterParts, metaData, ref bizo))
							{
								if (bizo == null)
								{
									bizo = LoadBusinessObject(language, isCheckedOut, isReadOnly, data, metaData);
								}
								list.Add(bizo);
							}
						}
					}
				}
			}
			return list.ToArray();
		}

		static IEnumerable<ISimpleResourceStringCache> GetLeafCaches(ISimpleResourceStringCache cache)
		{
			var multiLevelCache = cache as MultiLevelResourceStringCache;
			if (multiLevelCache == null)
			{
				return new ISimpleResourceStringCache[] { cache };
			}
			else
			{
				return multiLevelCache.Caches.SelectMany(c => GetLeafCaches((ISimpleResourceStringCache)c));
			}
		}

		static HelpDataString LoadBusinessObject(string language, bool isCheckedOut, bool isReadOnly, ResourceStringData data, ResourceStringsMetaData metaData)
		{
			var bizo = HelpDataString.CreateFromResourceStringData(data, language);
			using (bizo.GetValidationSuspender())
			{
				bizo.HD_Language = language;
				bizo.HD_IsCheckedOut = isCheckedOut;
				bizo.HD_IsReadOnly = isReadOnly;
				if (language != Res.DefaultLanguage)
				{
					var referenceData = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage).Get(data.Key);
					if (referenceData != null)
					{
						bizo.LoadFromMetaData(referenceData.MetaData);
					}
				}
				metaData.Attach(bizo);
				bizo.HasChanges = false;
			}
			return bizo;
		}

		static bool IsMatch(string language, bool isCheckedOut, bool isReadOnly, ResourceStringData data, IFilterPart[] parts, ResourceStringsMetaData metaData, ref HelpDataString bizo)
		{
			bool isMatch = true;
			JoinCondition lastJoinCondition = JoinCondition.And;
			foreach (var part in parts)
			{
				if (part is JoinCondition joinCondition)
				{
					lastJoinCondition = joinCondition;
				}
				else if (!isMatch && lastJoinCondition == JoinCondition.And)
				{
					continue;
				}
				else if (isMatch && lastJoinCondition == JoinCondition.Or)
				{
					continue;
				}
				else if (part is ZSQLInFilter zSQLInFilter)
				{
					var isMatchInZSQLInFilter = false;
					foreach (var value in zSQLInFilter.GetValues())
					{
						isMatchInZSQLInFilter |= IsMatch(language, isCheckedOut, isReadOnly, zSQLInFilter.Column.Name, ZSqlParameter.GetTruncatedBasicValueForDatabase(value, zSQLInFilter.Column), zSQLInFilter.ComparisonOperator, data, metaData, ref bizo);
					}

					if (lastJoinCondition == JoinCondition.Or)
					{
						isMatch |= isMatchInZSQLInFilter;
					}
					else
					{
						isMatch &= isMatchInZSQLInFilter;
					}
					continue;
				}
				else if (part is ZSqlParameter zSqlParameter)
				{
					bool itemIsMatch;
					itemIsMatch = IsMatch(language, isCheckedOut, isReadOnly, zSqlParameter.SchemaColumn.Name, zSqlParameter.Value, zSqlParameter.ComparisonOperator, data, metaData, ref bizo);
					if (lastJoinCondition == JoinCondition.Or)
					{
						isMatch |= itemIsMatch;
					}
					else
					{
						isMatch &= itemIsMatch;
					}
				}
				else if (part is IFilterPartsProvider filterPartsProvider)
				{
					if (lastJoinCondition == JoinCondition.Or)
					{
						isMatch |= IsMatch(language, isCheckedOut, isReadOnly, data, filterPartsProvider.FilterParts, metaData, ref bizo);
					}
					else
					{
						isMatch &= IsMatch(language, isCheckedOut, isReadOnly, data, filterPartsProvider.FilterParts, metaData, ref bizo);
					}
				}
				else
				{
					throw new NotSupportedException("Unsupported query structure " + part.ToString());
				}
			}
			return isMatch;
		}

#if DEBUG
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static internal Func<string, string, bool> InLastFailures;
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static bool IsMatch(string language, bool isCheckedOut, bool isReadOnly, string columnName, object value, SQLComparisonOperator comparisonOperator, ResourceStringData data, ResourceStringsMetaData metaData, ref HelpDataString bizo)
		{
			bool itemIsMatch;
			switch (columnName)
			{
				case HelpDataString.Schema.HD_Code:
					itemIsMatch = IsMatch(data.Key, (string)value, comparisonOperator);
					break;
				case HelpDataString.Schema.HD_Caption:
					itemIsMatch = IsMatch(data.Caption, (string)value, comparisonOperator);
					break;
				case HelpDataString.Schema.HD_FullDescription:
					itemIsMatch = IsMatch(data.FullDescription, (string)value, comparisonOperator);
					break;
				case HelpDataString.Schema.HD_ShortCaption:
					itemIsMatch = IsMatch(data.ShortCaption, (string)value, comparisonOperator);
					break;
				case HelpDataString.Schema.HD_MidCaption:
					itemIsMatch = IsMatch(data.MediumCaption, (string)value, comparisonOperator);
					break;
				case HelpDataString.Schema.HD_Language:
					itemIsMatch = IsMatch(language, (string)value, comparisonOperator);
					break;
				case HelpDataString.Schema.HD_IsCheckedOut:
					itemIsMatch = isCheckedOut == (bool)value;
					break;
				case HelpDataString.Schema.HD_EditReason:
					itemIsMatch = IsMatch(data.EditReason ?? string.Empty, (string)value, comparisonOperator);
					break;

				case HelpDataString.Schema.HD_ContextClassName:
					if (language == Res.DefaultLanguage)
					{
						itemIsMatch = IsMatch(data.MetaData != null ? data.MetaData.ContextClassName ?? string.Empty : string.Empty, (string)value, comparisonOperator);
					}
					else
					{
						if (bizo == null)
						{
							bizo = LoadBusinessObject(language, isCheckedOut, isReadOnly, data, metaData);
						}
						itemIsMatch = IsMatch(bizo.HD_ContextClassName, (string)value, comparisonOperator);
					}
					break;

				case HelpDataString.Schema.HD_ContextSourceFile:
					if (language == Res.DefaultLanguage)
					{
						itemIsMatch = IsMatch(data.MetaData != null ? data.MetaData.ContextFile ?? string.Empty : string.Empty, (string)value, comparisonOperator);
					}
					else
					{
						if (bizo == null)
						{
							bizo = LoadBusinessObject(language, isCheckedOut, isReadOnly, data, metaData);
						}
						itemIsMatch = IsMatch(bizo.HD_ContextSourceFile, (string)value, comparisonOperator);
					}
					break;

				case HelpDataString.Schema.HD_ControlPath:
					if (bizo == null)
					{
						bizo = LoadBusinessObject(language, isCheckedOut, isReadOnly, data, metaData);
					}
					itemIsMatch = IsMatch(bizo.HD_ControlPath, (string)value, comparisonOperator);
					break;

#if DEBUG
				case "InLastFailures":
					itemIsMatch = InLastFailures(language, data.Key) ^ !(bool)value;
					break;
#endif

				case "DocStripCellContent":
					if (!ObjectFactory.Get<IDocBuilderUsageFinder>().IsInitialized)
					{
						ObjectFactory.Get<IDocBuilderUsageFinder>().Initialize();
					}

					if (bizo == null)
					{
						bizo = LoadBusinessObject(language, isCheckedOut, isReadOnly, data, metaData);
					}
					itemIsMatch = bizo.DocBuilderUsages.Cast<IDocBuilderUsage>().Any(usage => IsMatch(usage.Macro, (string)value, comparisonOperator));
					break;

				default:
					throw new NotSupportedException("Unsupported query field  " + columnName);
			}

			return itemIsMatch;
		}

		public static bool IsMatch(string value1, string value2, SQLComparisonOperator op)
		{
			return op.GetPredicate(value2).Invoke(value1);
		}

		public static void UndoCheckout(params HelpDataString[] resourceStrings)
		{
			Array.ForEach(resourceStrings, resourceString => resourceString.Delete());
			Save(resourceStrings);
		}

		public static void UndoUnchanged()
		{
			var undo = new List<HelpDataString>();
			foreach (var checkedOut in ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true)))
			{
				var original = ResourceStringsFactory.Lookup(checkedOut.HD_Language, checkedOut.HD_Code, false) ?? ResourceStringsFactory.Lookup(Res.DefaultLanguage, checkedOut.HD_Code, false);
				if (original != null && original.ToResourceStringData().ContentEquals(checkedOut.ToResourceStringData()))
				{
					undo.Add(checkedOut);
				}
			}
			ResourceStringsFactory.UndoCheckout(undo.ToArray());
		}

		public static void Save(params HelpDataString[] resourceStrings)
		{
			Save(null, resourceStrings);
		}

		public static void Save(string editReason, params HelpDataString[] resourceStrings)
		{
			Save(editReason, Db.Connection, resourceStrings);
		}

		public static void Save(string editReason, DbConnection connection, params HelpDataString[] resourceStrings)
		{
			if (editReason == EditReasons.Codes.TranslationFeedback)
			{
				throw new InvalidOperationException("Cannot save transaltion feedback resource strings directly.");
			}

			var resourceStringsByLanguage = new Dictionary<string, List<HelpDataString>>();
			foreach (var resourceString in resourceStrings)
			{
				if (resourceString != null && (resourceString.HasChanges || resourceString.IsDeleted))
				{
					if (!resourceStringsByLanguage.ContainsKey(resourceString.HD_Language))
					{
						resourceStringsByLanguage.Add(resourceString.HD_Language, new List<HelpDataString>());
					}
					resourceStringsByLanguage[resourceString.HD_Language].Add(resourceString);
				}
			}

			var sourceCache = GetResourceStringCache(Res.DefaultLanguage);
			using (var hashCalculator = new ResourceStringHashCalculator())
			{
				foreach (var languageSet in resourceStringsByLanguage)
				{
					if (languageSet.Key != Res.DefaultLanguage)
					{
						foreach (var cache in GetLeafCaches(GetResourceStringCache(languageSet.Key)))
						{
							if (cache.Language == languageSet.Key && IsEditableSource(cache))
							{
								connection.RunTransactioned(delegate
								{
									((SimpleResourceStringCache)cache).Load();
									var rawCache = ((SimpleResourceStringCache)cache).Cache;
									foreach (var resourceString in languageSet.Value)
									{
										if (resourceString.IsDeleted)
										{
											rawCache.Remove(resourceString.HD_Code);
										}
										else
										{
											var sourceData = sourceCache.Get(resourceString.HD_Code);
											if (sourceData == null)
											{
												HelpDataString referenceItem = null;
												List<HelpDataString> referenceList;
												if (resourceStringsByLanguage.TryGetValue(Res.DefaultLanguage, out referenceList))
												{
													referenceItem = referenceList.Find(item => item.HD_Code == resourceString.HD_Code);
												}
												if (referenceItem == null)
												{
													throw new InvalidOperationException("No matching source found for resource string with key " + resourceString.HD_Code);
												}
												else
												{
													sourceData = referenceItem.ToResourceStringData();
												}
											}
											var data = rawCache[resourceString.HD_Code] = resourceString.ToResourceStringData(sourceHash: hashCalculator.GetHash(sourceData), editReason: editReason ?? resourceString.HD_EditReason);
											if (string.IsNullOrWhiteSpace(data.EditReason))
											{
												throw new InvalidOperationException("Trying to save a resource strings without a given reason");
											}
										}
									}
									((SimpleResourceStringCache)cache).Source.WriteAll(rawCache.Values);
								});
								break;
							}
						}
					}
				}
			}
			Array.ForEach(resourceStrings, resourceString => resourceString.HD_IsCheckedOut = true);
			Res.NotifyChange();
		}

		public static ISimpleResourceStringCache GetResourceStringCache(string language)
		{
			ISimpleResourceStringCache cache = null;
			if (cacheReferences != null)
			{
				cacheReferences.TryGetValue(language, out cache);
			}
			if (cache == null)
			{
				cache = ResourceStringCacheBuilder.Instance.GetResourceStringResearch(language);
			}
			if (cacheReferences != null && !cacheReferences.ContainsKey(language))
			{
				cacheReferences.Add(language, cache);
			}
			return cache;
		}

		static bool IsCheckedOutSource(IResourceStringCache cache)
		{
			var simpleCache = cache as SimpleResourceStringCache;
			return simpleCache != null && (simpleCache.Source is ResourcesDeltaSource || simpleCache.Source is DatabaseResourceStringSource || simpleCache.Source is TranslationFeedbackResourceStringsSource);
		}

		static bool IsEditableSource(IResourceStringCache cache)
		{
			var simpleCache = cache as SimpleResourceStringCache;
			return simpleCache != null && (simpleCache.Source is ResourcesDeltaSource || simpleCache.Source is DatabaseResourceStringSource);
		}

		public static IDisposable HoldCacheReferences()
		{
			cacheReferences = new Dictionary<string, ISimpleResourceStringCache>();
			return new DisposableAction(delegate { cacheReferences = null; });
		}
		static Dictionary<string, ISimpleResourceStringCache> cacheReferences;

#if DEBUG
		public static IDisposable MockSources()
		{
			return ResourceStringCacheBuilder.Instance.MockSources();
		}

		public static IMockResourceStringCache GetMockSource(string language)
		{
			IMockResourceStringCache result = null;
			foreach (var cache in GetLeafCaches(GetResourceStringCache(language)))
			{
				if (cache.Language == language && cache as IMockResourceStringCache != null)
				{
					result = (IMockResourceStringCache)cache;
				}
			}
			return result;
		}
#endif
	}
}
