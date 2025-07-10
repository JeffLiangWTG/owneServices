using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class RemoveAutoAssignCapabilityTasksOnChangesWithinXMinutesRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => ["AutoAssignCapabilityTasksOnChangesWithinXMinutes"];
	}
}
