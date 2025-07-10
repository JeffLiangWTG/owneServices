using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.Business
{
	public class RegistryItemSetLocator : IRegistryItemSetLocator
	{
		public RegistryItemSetLocator()
		{
		}

		public IEnumerable<IRegistryItem> GetAllRegistryItems()
		{
			foreach (var registryItemSet in GetRegistryItemSets())
			{
				var hash = new HashSet<IRegistryItem>();
				foreach (var item in registryItemSet.GetAllItems())
				{
					if (hash.Contains(item))
					{
						ErrorReporter.ReportOnce($"Registry item {item.Name} has same key in registry items set {registryItemSet.GetType().Name}");
					}
					else
					{
						hash.Add(item);
						yield return item;
					}
				}
			}
		}

		public string GetFormattedListOfRegistryItemsReferencingPK(Guid pk)
		{
			return GetFormattedListOfRegistryItemsReferencingPK(pk, true, System.Environment.NewLine);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "Append triggers this, yet is not necessary to use returned value.")]
		public string GetFormattedListOfRegistryItemsReferencingPK(Guid pk, bool startEachItemWithBullet, string itemSeparator)
		{
			var result = new StringBuilder();
			var bullet = startEachItemWithBullet ? (NoResString)"• " : "";

			IRegistryItem[] items = GetRegistryItemsReferencingPK(pk);
			Array.Sort(items, (x, y) =>
			{
				int compareResult = x.Category.CompareTo(y.Category);
				if (compareResult == 0)
				{
					compareResult = x.Caption.CompareTo(y.Caption);
				}
				return compareResult;
			});

			for (var i = 0; i < items.Length; ++i)
			{
				result.Append(bullet);
				result.Append(items[i].Category);
				result.Append(RegistryItemSet.Delimiter);
				result.Append(items[i].Caption);

				if (i != (items.Length - 1))
				{
					result.Append(itemSeparator);
				}
			}

			return result.ToString();
		}

		public IEnumerable<RegistryItemSet> GetRegistryItemSets()
		{
			return GetRegistryItemSetsCore();
		}

		public IRegistryItemSet GetRegistryItemSet(string name)
		{
			IRegistryItemSet result = null;
			if (name.Equals("AdditionalRegistryItemSet"))
			{
				result = GetClientAdditionalRegistryItemSet();
			}
			else if (name.Equals("RawRegistry"))
			{
				result = Env.Registry.RawRegistry;
			}
			else
			{
#pragma warning disable CW1156 // WI00678422 - Do not use Release interfaces with Debug classes in DebugOnlyConfiguration.xml - This is a false positive which will resolve following an update to the analyzer.
				result = ObjectFactory.Get<IRegistryItemSet>(string.Format("RegistryItemSet_{0}", name));
#pragma warning restore CW1156 // WI00678422 - Do not use Release interfaces with Debug classes in DebugOnlyConfiguration.xml
			}
			return result;
		}

		protected virtual IEnumerable<RegistryItemSet> GetRegistryItemSetsCore()
		{
			yield return Env.Registry.RawRegistry;

			foreach (RegistryItemSet itemSet in ObjectFactory.Get<ArrayList>("RegistryItemSets"))
			{
				if (DataRegistry.Instance.ProductivityWiseModeEnabled && !itemSet.IsForProductivityWise)
				{
					continue;
				}

				yield return itemSet;
			}

			var clientAdditionalRegistryItemSet = GetClientAdditionalRegistryItemSet();
			if (clientAdditionalRegistryItemSet != null)
			{
				yield return clientAdditionalRegistryItemSet;
			}
		}

		internal RegistryItemSet GetClientAdditionalRegistryItemSet()
		{
			RegistryItemSet result = null;

			ClientHook clientHook = ClientHookLoader.Instance.ClientHook;
			if (clientHook != null)
			{
				result = clientHook.AdditionalRegistryItemSet as RegistryItemSet;
			}

			return result;
		}

		public IRegistryItem[] GetRegistryItemsReferencingPK(Guid pk)
		{
			var result = new List<IRegistryItem>();
			string[] names;
			using (var registryDataAccessor = RegistryDataAccessor.DisposableInstance)
			{
				names = registryDataAccessor.GetNamesOfRegistryItemsReferencingPK(pk);
			}

			if (names.Length > 0)
			{
				var itemSets = GetRegistryItemSets();
				foreach (string name in names)
				{
					foreach (RegistryItemSet itemSet in itemSets)
					{
						IRegistryItem item = itemSet.FindByName(name);
						if (item != null)
						{
							result.Add(item);
							break;
						}
					}
				}
			}

			return result.ToArray();
		}
	}
}
