using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WarehouseReceiveMilestoneEventUpdatesRegistryControl))]
	sealed class WarehouseReceiveMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new WarehouseReceiveMilestoneEventUpdatesCollection();
		}
	}
}
