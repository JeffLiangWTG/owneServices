using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RemoveWorkflowStatusUpdaterRegistryItems : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["WorkflowStatusUpdaterQueryDays", "WorkflowStatusUpdaterWorkflowQueriesEnabled", "WorkflowStatusLogSubscriberEnabled"];
		}
	}
}
