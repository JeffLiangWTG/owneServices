using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(WarehouseReceiveMilestoneEventUpdatesCollection))]
	sealed class WarehouseReceiveMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<WarehouseReceiveMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override WarehouseReceiveMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new WarehouseReceiveMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WarehouseReceiveMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
