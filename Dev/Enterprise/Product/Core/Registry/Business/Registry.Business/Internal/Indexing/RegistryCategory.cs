using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.Internal.Indexing.RegistryIndexingHelper;

namespace Enterprise.Registry.Business.Internal.Indexing
{
	[DebuggerDisplay("{DebuggerType,nq} Category ({DebuggerText})")]
	sealed class RegistryCategory
	{
		RegistryCategory(string knownText, CategorySegmentRef staticCategorySegmentRef, Dictionary<string, RegistryCategory> staticCategories, Dictionary<string, RegistryCategory> dynamicCategories, Dictionary<Type, List<string>> staticSetPropertyNames, List<IRegistryItem> dynamicItems)
		{
			KnownText = knownText;
			StaticCategorySegmentRef = staticCategorySegmentRef;
			StaticCategories = staticCategories;
			DynamicCategories = dynamicCategories;
			StaticSetPropertyNames = staticSetPropertyNames;
			DynamicItems = dynamicItems;
		}

		public string KnownText { get; }
		public CategorySegmentRef StaticCategorySegmentRef { get; }
		public Dictionary<string, RegistryCategory> StaticCategories { get; }
		public Dictionary<string, RegistryCategory> DynamicCategories { get; }
		public Dictionary<Type, List<string>> StaticSetPropertyNames { get; }
		public List<IRegistryItem> DynamicItems { get; }

#if DEBUG
		string DebuggerType => StaticCategories != null ? DynamicCategories != null ? "Combined" : "Static" : "Dynamic";
		object DebuggerText => KnownText ?? (object)StaticCategorySegmentRef;
#endif

		public string GetText(IReadOnlyDictionary<Type, RegistryItemSet> sets)
		{
			return KnownText ?? StaticCategorySegmentRef.GetValue(sets);
		}

		public IEnumerable<RegistryCategoryRef> GetSortedCategories(Dictionary<RegistryCategory, bool> staticCategoriesCache, IReadOnlyDictionary<Type, RegistryItemSet> sets, IRegistryItemVisibility visibility)
		{
			var staticCategories = StaticCategories?.Values
				.Where(cat => cat.HasAnyVisibleStaticItem(staticCategoriesCache, sets, visibility));

			return CombineAndSort(staticCategories, DynamicCategories?.Values, sets);
		}

		public IEnumerable<IRegistryItem> GetSortedItems(IReadOnlyDictionary<Type, RegistryItemSet> sets, IRegistryItemVisibility visibility)
		{
			return CombineAndSort(GetStaticItems(sets, visibility), DynamicItems);
		}

		IEnumerable<IRegistryItem> GetStaticItems(IReadOnlyDictionary<Type, RegistryItemSet> sets, IRegistryItemVisibility visibility)
		{
			if (StaticSetPropertyNames == null)
			{
				yield break;
			}
			var reported = false;
			foreach (var pair in StaticSetPropertyNames)
			{
				RegistryItemSet set = null;
				if (sets.TryGetValue(pair.Key, out set))
				{
					foreach (var propertyName in pair.Value)
					{
						if (set.TryGetItemByPropertyName(propertyName, out var item) && visibility.IsVisible(item))
						{
							yield return item;
						}
					}
				}
				else if (!reported)
				{
					reported = true;
					var culprit = "";
					foreach (var pair2 in StaticSetPropertyNames)
					{
						if (!sets.ContainsKey(pair2.Key))
						{
							culprit += pair2.Key.ToString() + System.Environment.NewLine;
						}
					}
					ErrorReporter.ReportOnce("GetStaticItems", string.Format("Missing type(s): {0}", culprit));
				}
			}
		}

		public bool HasAnyVisibleStaticItem(IReadOnlyDictionary<Type, RegistryItemSet> sets, IRegistryItemVisibility visibility)
		{
			return GetStaticItems(sets, visibility)?.Any() ?? false;
		}

		public static RegistryCategory NewStatic(CategorySegmentRef segmentRef)
		{
			return new RegistryCategory(null, segmentRef, new Dictionary<string, RegistryCategory>(), null, new Dictionary<Type, List<string>>(), null);
		}

		public static RegistryCategory NewDynamic(string text)
		{
			return new RegistryCategory(text, null, null, new Dictionary<string, RegistryCategory>(), null, new List<IRegistryItem>());
		}

		public static RegistryCategory Combine(RegistryCategory staticCategory, RegistryCategory dynamicCategory)
		{
			return new RegistryCategory(
				dynamicCategory.KnownText,
				staticCategory.StaticCategorySegmentRef,
				staticCategory.StaticCategories,
				dynamicCategory.DynamicCategories,
				staticCategory.StaticSetPropertyNames,
				dynamicCategory.DynamicItems);
		}
	}

	static class RegistryCategoryExtensions
	{
		public static bool HasAnyVisibleStaticItem(this RegistryCategory category, Dictionary<RegistryCategory, bool> staticCategories, IReadOnlyDictionary<Type, RegistryItemSet> sets, IRegistryItemVisibility visibility)
		{
			if (staticCategories.TryGetValue(category, out var result))
			{
				return result;
			}

			if (category.HasAnyVisibleStaticItem(sets, visibility))
			{
				staticCategories[category] = true;
				return true;
			}

			var stack = new Stack<RegistryCategory>();
			stack.Push(category);

			while (stack.Count > 0)
			{
				var currentCategory = stack.Pop();

				if (currentCategory.HasAnyVisibleStaticItem(sets, visibility))
				{
					staticCategories[category] = true;
					return true;
				}

				foreach (var subCategory in currentCategory.StaticCategories.Values)
				{
					stack.Push(subCategory);
				}
			}

			staticCategories[category] = false;
			return false;
		}
	}
}
