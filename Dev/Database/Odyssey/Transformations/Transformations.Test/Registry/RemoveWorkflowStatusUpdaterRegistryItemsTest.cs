using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RemoveWorkflowStatusUpdaterRegistryItems))]
	class RemoveWorkflowStatusUpdaterRegistryItemsTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["WorkflowStatusUpdaterQueryDays", "WorkflowStatusUpdaterWorkflowQueriesEnabled", "WorkflowStatusLogSubscriberEnabled"];
		}
	}
}
