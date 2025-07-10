using CargoWise.DbUpgrader.Scripts.Definitions.EServices.DeniedPartyScreening;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.EServices.DeniedPartyScreening.Testing
{
	[TestedType(typeof(UpdateRelatedShipmentsForOrgOrDocAddressByPhases))]
	class UpdateRelatedShipmentsForOrgOrDocAddressByPhasesTest : DbCreateScriptTest
	{
		//Further tested in ScreeningUpdaterTest.

		public void TestTransactionCountBeforeExecutionIsTheSameAsAfterExecution()
		{
			DpsUpdateRelatedJobsByPhasesTestHelper.AssertTransactionCountBeforeExecutionIsTheSameAsAfterExecution(TestConnection, ScriptToTest.Name, JobShipmentSchema.Constants.TableName);
		}
	}
}

