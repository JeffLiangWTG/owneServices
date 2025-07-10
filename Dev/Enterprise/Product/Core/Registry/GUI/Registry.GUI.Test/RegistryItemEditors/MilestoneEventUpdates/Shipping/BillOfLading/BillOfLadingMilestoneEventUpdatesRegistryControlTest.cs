using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BillOfLadingMilestoneEventUpdatesRegistryControl))]
	sealed class BillOfLadingMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BillOfLadingMilestoneEventUpdatesCollection();
		}
	}
}
