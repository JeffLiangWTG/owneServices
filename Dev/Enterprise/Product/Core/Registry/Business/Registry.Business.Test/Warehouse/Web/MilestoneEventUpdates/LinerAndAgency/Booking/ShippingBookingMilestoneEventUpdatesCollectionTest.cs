using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShippingBookingMilestoneEventUpdatesCollection))]
	sealed class ShippingBookingMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<ShippingBookingMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override ShippingBookingMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new ShippingBookingMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShippingBookingMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
