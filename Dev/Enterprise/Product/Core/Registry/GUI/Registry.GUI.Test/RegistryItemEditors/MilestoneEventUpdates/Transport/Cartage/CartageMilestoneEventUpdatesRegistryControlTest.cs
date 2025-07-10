using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CartageMilestoneEventUpdatesRegistryControl))]
	sealed class CartageMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CartageMilestoneEventUpdatesCollection();
		}
	}
}
