using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContainerMilestoneEventUpdatesCollection))]
	sealed class ContainerMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<ContainerMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override ContainerMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new ContainerMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContainerMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
