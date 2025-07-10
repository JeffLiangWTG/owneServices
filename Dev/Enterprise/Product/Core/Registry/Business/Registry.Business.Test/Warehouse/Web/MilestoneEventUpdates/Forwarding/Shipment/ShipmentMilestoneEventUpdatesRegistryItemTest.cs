using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShipmentMilestoneEventUpdatesRegistryItem))]
	sealed class ShipmentMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<ShipmentMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<ShipmentMilestoneEventUpdatesCollection, ShipmentMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new ShipmentMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ShipmentMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
