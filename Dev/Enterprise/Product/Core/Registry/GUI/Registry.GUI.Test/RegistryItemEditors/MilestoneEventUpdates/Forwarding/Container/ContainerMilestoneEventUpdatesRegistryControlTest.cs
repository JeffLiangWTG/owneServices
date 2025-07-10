using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ContainerMilestoneEventUpdatesRegistryControl))]
	sealed class ContainerMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ContainerMilestoneEventUpdatesCollection();
		}
	}
}
