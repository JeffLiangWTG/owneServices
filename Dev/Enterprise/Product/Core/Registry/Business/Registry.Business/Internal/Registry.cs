using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.Registry.Business.Internal.Indexing;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Registry.Business.Internal.Indexing.RegistryIndexingHelper;

namespace Enterprise.Registry.Business
{
	class Registry : IRegistry
	{
		public Registry(Lazy<RegistryStaticIndex> staticIndex, Lazy<IReadOnlyDictionary<Type, RegistryItemSet>> setsDictionary, Dictionary<RegistryCategory, bool> staticCategoriesCache, IRegistryItemVisibility visibility)
		{
			Argument.NotNull(staticIndex, nameof(staticIndex));
			this.setsDictionary = Argument.NotNull(setsDictionary, nameof(setsDictionary));
			staticCategories = Argument.NotNull(staticCategoriesCache, nameof(staticCategoriesCache));
			this.visibility = Argument.NotNull(visibility, nameof(visibility));

			topLevelCategories = new Lazy<IReadOnlyCollection<RegistryCategoryRef>>(() =>
			{
				var sets = setsDictionary.Value;
				var staticItems = staticIndex.Value.TopLevelCategories
					.Where(c => c.HasAnyVisibleStaticItem(staticCategories, setsDictionary.Value, visibility));
				var dynamicItems = RegistryDiscovery.DiscoverVisibleDynamicItems(sets.Values, visibility);
				return CombineAndSort(staticItems, dynamicItems, sets).ToList();
			}, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public IEnumerable<RegistryCategoryRef> GetSortedTopLevelCategories()
		{
			return topLevelCategories.Value;
		}

		public RegistryCategoryContent GetSortedContent(object categoryKey)
		{
			var cat = (RegistryCategory)categoryKey;
			var sets = setsDictionary.Value;
			return new RegistryCategoryContent(cat.GetSortedCategories(staticCategories, sets, visibility), cat.GetSortedItems(sets, visibility));
		}

		readonly Lazy<IReadOnlyCollection<RegistryCategoryRef>> topLevelCategories;
		readonly Lazy<IReadOnlyDictionary<Type, RegistryItemSet>> setsDictionary;
		readonly Dictionary<RegistryCategory, bool> staticCategories;
		readonly IRegistryItemVisibility visibility;
	}
}
