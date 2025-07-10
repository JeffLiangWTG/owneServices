using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WarehouseOrderMilestoneEventUpdatesRegistryItem))]
	sealed class WarehouseOrderMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<WarehouseOrderMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<WarehouseOrderMilestoneEventUpdatesCollection, WarehouseOrderMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new WarehouseOrderMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new WarehouseOrderMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
