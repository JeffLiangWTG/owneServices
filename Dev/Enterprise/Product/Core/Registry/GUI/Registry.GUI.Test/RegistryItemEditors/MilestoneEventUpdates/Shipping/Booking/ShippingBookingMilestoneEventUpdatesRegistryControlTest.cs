using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ShippingBookingMilestoneEventUpdatesRegistryControl))]
	sealed class ShippingBookingMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ShippingBookingMilestoneEventUpdatesCollection();
		}
	}
}
