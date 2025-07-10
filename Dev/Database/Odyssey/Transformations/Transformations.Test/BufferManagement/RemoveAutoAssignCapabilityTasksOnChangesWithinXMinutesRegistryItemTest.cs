using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test
{
	[TestedType(typeof(RemoveAutoAssignCapabilityTasksOnChangesWithinXMinutesRegistryItem))]
	class RemoveAutoAssignCapabilityTasksOnChangesWithinXMinutesRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => ["AutoAssignCapabilityTasksOnChangesWithinXMinutes"];
	}
}
