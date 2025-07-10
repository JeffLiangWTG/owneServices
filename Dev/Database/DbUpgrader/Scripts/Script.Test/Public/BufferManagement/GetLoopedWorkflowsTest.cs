using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetLoopedWorkflows))]
	sealed class GetLoopedWorkflowsTest : DbCreateScriptTest
	{
		// This function is tested at the business layer: Enterprise.BufferManagement.Business.Test.SchematicTransferLoopMonitorTest
	}
}

