using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyContainerMilestoneEventUpdatesCollection))]
	sealed class LinerAndAgencyContainerMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<LinerAndAgencyContainerMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override LinerAndAgencyContainerMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
