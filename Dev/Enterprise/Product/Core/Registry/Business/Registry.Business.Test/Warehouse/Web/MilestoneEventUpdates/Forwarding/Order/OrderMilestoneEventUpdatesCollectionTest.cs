using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrderMilestoneEventUpdatesCollection))]
	sealed class OrderMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<OrderMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override OrderMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new OrderMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrderMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
