using CargoWise.DbUpgrader.Scripts.Definitions.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Workflow.Testing
{
	[TestedType(typeof(GetCurrentTasksInWorkflowsIgnoringIterations))]
	sealed class GetCurrentTasksInWorkflowsIgnoringIterationsTest : DbCreateScriptTest
	{
		// This function is tested at the business layer: Enterprise.MasterFiles.Module.Testing.TestCurrentTaskOnly_WhenQIDisabled and TestCurrentTaskOnly_CheckQueryTablesForQIDisabled
	}
}
