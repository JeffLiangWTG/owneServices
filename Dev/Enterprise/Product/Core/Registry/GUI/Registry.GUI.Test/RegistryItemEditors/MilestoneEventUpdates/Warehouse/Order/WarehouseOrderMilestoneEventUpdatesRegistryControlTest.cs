using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WarehouseOrderMilestoneEventUpdatesRegistryControl))]
	sealed class WarehouseOrderMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new WarehouseOrderMilestoneEventUpdatesCollection();
		}
	}
}
