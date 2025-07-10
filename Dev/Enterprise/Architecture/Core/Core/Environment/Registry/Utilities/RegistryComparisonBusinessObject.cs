using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.OverrideLevels;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryComparisonBusinessObject : NonPersistentBusinessObject
	{
		public IOverrideLevel BaseLevel { get; private set; }
		public IOverrideLevel OverrideLevel { get; private set; }

		readonly IReadOnlyCollection<IRegistryItem> itemsThatDifferBetweenBaseAndOverride;
		readonly IReadOnlyCollection<IRegistryItem> allPassedInItems;

		public RegistryComparisonBusinessObject(IEnumerable<IRegistryItem> registryItemsToLazyLoad, IOverrideLevel baseLevel, IOverrideLevel overrideLevel, IProgress<int> findingChangesProgressCallback = null)
		{
			OverrideLevel = overrideLevel;
			BaseLevel = baseLevel;

			List<IRegistryItem> allItems, itemsThatDiffer;
			FindItemsWithChanges(registryItemsToLazyLoad, findingChangesProgressCallback ?? new NullProgress<int>(), out allItems, out itemsThatDiffer);

			allPassedInItems = allItems.AsReadOnly();
			itemsThatDifferBetweenBaseAndOverride = itemsThatDiffer.AsReadOnly();
		}

		void FindItemsWithChanges(IEnumerable<IRegistryItem> registryItems, IProgress<int> progress, out List<IRegistryItem> allItems, out List<IRegistryItem> itemsThatDiffer)
		{
			allItems = new List<IRegistryItem>();
			itemsThatDiffer = new List<IRegistryItem>(100);

			foreach (var item in registryItems)
			{
				var dataType = item.DataType;
				if (!dataType.ValuesAreEqual(OverrideLevel.GetValueOf(item), BaseLevel.GetValueOf(item)))
				{
					itemsThatDiffer.Add(item);
				}

				allItems.Add(item);

				progress.Report(1);
			}
		}

		public IReadOnlyCollection<IRegistryItem> ItemsThatDifferBetweenBaseAndOverride
		{
			get { return itemsThatDifferBetweenBaseAndOverride; }
		}

		public IReadOnlyCollection<IRegistryItem> AllComparedItems
		{
			get { return allPassedInItems; }
		}

		sealed class NullProgress<T> : IProgress<T>
		{
			public void Report(T value) { }
		}
	}
}
