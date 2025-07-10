using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShippingBookingMilestoneEventUpdatesRegistryItem))]
	sealed class ShippingBookingMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<ShippingBookingMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<ShippingBookingMilestoneEventUpdatesCollection, ShippingBookingMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new ShippingBookingMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ShippingBookingMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
