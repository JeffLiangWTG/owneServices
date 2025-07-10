using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Warehouse
{
	public class RemoveEnableDockDoorAssignmentServiceRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => ["EnableDockDoorAssignmentService"];
	}
}
