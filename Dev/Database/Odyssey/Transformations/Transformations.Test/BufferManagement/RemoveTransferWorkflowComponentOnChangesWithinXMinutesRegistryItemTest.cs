using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BufferManagement
{
	[TestedType(typeof(RemoveTransferWorkflowComponentOnChangesWithinXMinutesRegistryItem))]
	class RemoveTransferWorkflowComponentOnChangesWithinXMinutesRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => ["TransferWorkflowComponentOnChangesWithinXMinutes"];
	}
}
