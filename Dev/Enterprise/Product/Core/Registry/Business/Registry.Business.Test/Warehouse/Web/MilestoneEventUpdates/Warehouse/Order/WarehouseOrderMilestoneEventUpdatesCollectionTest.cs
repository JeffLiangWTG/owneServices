using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WarehouseOrderMilestoneEventUpdatesCollection))]
	sealed class WarehouseOrderMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<WarehouseOrderMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override WarehouseOrderMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new WarehouseOrderMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WarehouseOrderMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
