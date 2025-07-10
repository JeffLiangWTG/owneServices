using CargoWise.DbUpgrader.Scripts.Definitions.EServices.DeniedPartyScreening;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.EServices.DeniedPartyScreening.Testing
{
	[TestedType(typeof(UpdateRelatedConsolsForOrgOrDocAddressByPhasesNew))]
	class UpdateRelatedConsolsForOrgOrDocAddressByPhasesNewTest : DbCreateScriptTest
	{
		//Tested in Enterprise.DeniedPartyScreening.

		public void TestTransactionCountBeforeExecutionIsTheSameAsAfterExecution()
		{
			DpsUpdateRelatedJobsByPhasesTestHelper.AssertTransactionCountBeforeExecutionIsTheSameAsAfterExecution(TestConnection, ScriptToTest.Name, JobConsolSchema.Constants.TableName);
		}
	}
}
