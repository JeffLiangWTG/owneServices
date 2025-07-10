using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Internal.Indexing
{
	class RegistryDiscovery
	{
		public static RegistryStaticIndex DiscoverStaticItems(IReadOnlyDictionary<Type, RegistryItemSet> sets)
		{
			var root = RegistryCategory.NewStatic(null);
			foreach (var (type, set) in sets.Select(p => (p.Key, p.Value)))
			{
				foreach (var (propertyName, item) in set.GetStaticItemPropertyInfos().Select(p => (p.Name, (IRegistryItem)p.GetValue(set))))
				{
					foreach (var categoryIndex in Enumerable.Range(0, item.Categories?.Length ?? 0).Where(i => !string.IsNullOrEmpty(item.Categories[i])))
					{
						var target = root;
						foreach (var segment in item.GetUntranslatedCategorySegments(categoryIndex).Select((value, index) => (value, index)))
						{
							target = target.StaticCategories.GetOrAdd(segment.value, () => RegistryCategory.NewStatic(new CategorySegmentRef(type, propertyName, categoryIndex, segment.index)));
						}

						target.StaticSetPropertyNames.GetOrAdd(type, () => new List<string>()).Add(propertyName);
					}
				}
			}

			return new RegistryStaticIndex(root.StaticCategories.Values);
		}

		public static IReadOnlyCollection<RegistryCategory> DiscoverVisibleDynamicItems(IEnumerable<RegistryItemSet> sets, IRegistryItemVisibility visibility)
		{
			var root = RegistryCategory.NewDynamic(null);
			foreach (var set in sets)
			{
				foreach (var item in set.GetDynamicItems().Where(visibility.IsVisible))
				{
					foreach (var categoryIndex in Enumerable.Range(0, item.Categories?.Length ?? 0).Where(i => !string.IsNullOrEmpty(item.Categories[i])))
					{
						var target = root;
						foreach (var segment in item.GetCategorySegments(categoryIndex))
						{
							target = target.DynamicCategories.GetOrAdd(segment, () => RegistryCategory.NewDynamic(segment));
						}

						target.DynamicItems.Add(item);
					}
				}
			}

			return root.DynamicCategories.Values;
		}
	}
}
