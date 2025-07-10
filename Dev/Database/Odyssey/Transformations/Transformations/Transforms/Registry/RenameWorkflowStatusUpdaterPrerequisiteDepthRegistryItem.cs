using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RenameWorkflowStatusUpdaterPrerequisiteDepthRegistryItem : RegistryDataTransformation
	{
		public override string UserDescription => "Rename registry from WorkflowStatusUpdaterPrerequisiteDepth to MaximumDepthOfAnalyzedWorkflowHierarchy";

		protected override void OfflinePostUpgradeTransform()
		{
			UpdateRegistryItemName("WorkflowStatusUpdaterPrerequisiteDepth", "MaximumDepthOfAnalyzedWorkflowHierarchy");
		}
	}
}
