using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	internal class RemoveTransferWorkflowComponentOnChangesWithinXMinutesRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => ["TransferWorkflowComponentOnChangesWithinXMinutes"];
	}
}
