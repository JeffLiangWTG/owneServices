using CargoWise.DbUpgrader.Scripts.Definitions.Workflow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Workflow.Testing
{
	[TestedType(typeof(GetCurrentTasks))]
	sealed class GetCurrentTasksBMEnabledTest : DbCreateScriptTest
	{
		// This function is tested at the business layer: Enterprise.MasterFiles.Module.Testing.TestCurrentTaskOnly and TestCurrentTask_ShouldOnlyReferenceProcessHeaderWhenBMEnabled
	}
}

