using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ShipmentMilestoneEventUpdatesCollection))]
	sealed class ShipmentMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<ShipmentMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override ShipmentMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new ShipmentMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShipmentMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
