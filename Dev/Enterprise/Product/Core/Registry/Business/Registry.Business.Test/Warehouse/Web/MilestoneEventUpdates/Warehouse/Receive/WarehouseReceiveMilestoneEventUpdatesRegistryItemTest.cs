using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WarehouseReceiveMilestoneEventUpdatesRegistryItem))]
	sealed class WarehouseReceiveMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<WarehouseReceiveMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<WarehouseReceiveMilestoneEventUpdatesCollection, WarehouseReceiveMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new WarehouseReceiveMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new WarehouseReceiveMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
