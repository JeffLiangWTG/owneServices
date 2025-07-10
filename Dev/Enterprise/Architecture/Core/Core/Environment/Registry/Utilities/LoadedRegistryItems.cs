using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common.Collections;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment.Registry
{
	public class LoadedRegistryItems
	{
#if DEBUG
		public
#else
		internal 
#endif
		LoadedRegistryItems(IEnumerable<LoadedRegistryItemValue> loadedItems)
		{
			var successfulItems = new Dictionary<string, LoadedRegistryItemValue>();
			var failedItems = new List<LoadedRegistryItemValue>();

			foreach (var item in loadedItems)
			{
				if (item.wasSuccessfullyLoaded)
				{
					successfulItems.Add(item.name, item);
				}
				else
				{
					failedItems.Add(item);
				}
			}

			this.successfulItems = successfulItems.ToImmutableDictionary();
			this.failedItems = failedItems.ToImmutableList();
		}

		readonly ImmutableDictionary<string, LoadedRegistryItemValue> successfulItems;
		readonly ImmutableList<LoadedRegistryItemValue> failedItems;

		public IReadOnlyCollection<LoadedRegistryItemValue> SuccessfulItems { get { return successfulItems.AsReadOnly(kvp => kvp.Value); } }
		public IReadOnlyCollection<LoadedRegistryItemValue> FailedItems { get { return failedItems; } }

		public int TotalCount { get { return successfulItems.Count + failedItems.Count; } }

		public IOverrideLevel AsOverrideLevel(IOverrideLevel fallbackForUnloadedItems)
		{
			return new DictionaryOverrideLevel(successfulItems, fallbackForUnloadedItems);
		}

		public RegistryComparisonBusinessObject AsComparison(IOverrideLevel levelToCompareTo)
		{
			return new RegistryComparisonBusinessObject(successfulItems.AsReadOnly(kvp => kvp.Value.registryItem), levelToCompareTo, AsOverrideLevel(levelToCompareTo));
		}
	}

	public class LoadedRegistryItemValue
	{
		public LoadedRegistryItemValue(string name, string caption, IRegistryItem registryItem, object value, bool wasSuccessfullyLoaded)
		{
			this.name = name;
			this.caption = caption;
			this.registryItem = registryItem;
			this.value = value;
			this.wasSuccessfullyLoaded = wasSuccessfullyLoaded;
		}

		public readonly string name;
		public readonly string caption;

		public readonly IRegistryItem registryItem;
		public readonly object value;

		public readonly bool wasSuccessfullyLoaded;
	}

	class DictionaryOverrideLevel : IOverrideLevel
	{
		readonly IDictionary<string, LoadedRegistryItemValue> overridenValues;
		readonly IOverrideLevel fallbackLevel;

		public DictionaryOverrideLevel(IDictionary<string, LoadedRegistryItemValue> overridenValues, IOverrideLevel fallbackLevel)
		{
			this.overridenValues = overridenValues;
			this.fallbackLevel = fallbackLevel;
		}

		public string PathRelativeToParent => string.Empty;
		public string Description => Res.GetString("5E357BF6-A797-4901-9AFB-FC2C0C6425BE", "Loaded Values");
		public IOverrideLevel Parent => fallbackLevel;
		public IEnumerable<IOverrideLevel> Children => Enumerable.Empty<IOverrideLevel>();
		public bool CanSetValueOf(IRegistryItem item) => false;
		public FallbackLevel GetFallbackLevel() => new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
		public Guid GetPkOfEntry(IRegistryItem item) => Guid.Empty;

		public object GetValueOf(IRegistryItem item)
		{
			LoadedRegistryItemValue result;
			return overridenValues.TryGetValue(item.Name, out result) ? result.value : fallbackLevel.GetValueOf(item);
		}

		public void SetValueOf(IRegistryItem item, object value)
		{
			throw new NotImplementedException();
		}
	}
}
