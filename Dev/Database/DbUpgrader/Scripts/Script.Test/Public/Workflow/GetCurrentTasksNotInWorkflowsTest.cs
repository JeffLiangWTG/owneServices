using CargoWise.DbUpgrader.Scripts.Definitions.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Workflow.Testing
{
	[TestedType(typeof(GetCurrentTasksNotInWorkflows))]
	sealed class GetCurrentTasksNotInWorkflowsTest : DbCreateScriptTest
	{
		// This function is tested at the business layer: Enterprise.MasterFiles.Module.Testing.TestCurrentTaskOnly and TestCurrentTask_ShouldOnlyReferenceProcessHeaderWhenBMEnabled
	}
}

