using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingMilestoneEventUpdatesCollection))]
	sealed class BillOfLadingMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<BillOfLadingMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override BillOfLadingMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new BillOfLadingMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BillOfLadingMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
