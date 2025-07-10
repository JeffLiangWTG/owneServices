using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CartageMilestoneEventUpdatesCollection))]
	sealed class CartageMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<CartageMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override CartageMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new CartageMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CartageMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
