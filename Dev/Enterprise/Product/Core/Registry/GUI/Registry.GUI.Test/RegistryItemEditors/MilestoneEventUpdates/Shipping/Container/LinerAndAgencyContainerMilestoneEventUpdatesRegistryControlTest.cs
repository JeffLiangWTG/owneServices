using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(LinerAndAgencyContainerMilestoneEventUpdatesRegistryControl))]
	sealed class LinerAndAgencyContainerMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdatesCollection();
		}
	}
}
