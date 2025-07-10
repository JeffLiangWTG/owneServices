using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(vw_List_ContainerAvailability))]
	class vw_List_ContainerAvailabilityTest : DbCreateScriptTest
	{
		public void TestShouldSelectSingleContainerIfTwoEligibleLegs()
		{
			var refContainer = TestDataCreator.CreateRefContainer("RC1");
			var containerPK = TestDataCreator.CreateJobContainer("CONTAINER1", refContainer);
			var consolPK = TestDataCreator.CreateJobConsol();
			var leg1PK = TestDataCreator.CreateJobConsolTransport(consolPK);
			var leg2PK = TestDataCreator.CreateJobConsolTransport(consolPK);
			TestDataCreator.CreateJobConsolTransport(consolPK);

			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobContainer
SET
	JC_JK = '{consolPK}',
	JC_SystemCreateTimeUtc = GETUTCDATE(),
	JC_SystemLastEditTimeUtc = GETUTCDATE(),
	JC_SystemLastEditUser = '~BP'
WHERE
	JC_PK = '{containerPK}'");
			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobConsolTransport
SET
	JW_ParentType = 'CON',
	JW_LegOrder = 1,
	JW_SystemLastEditTimeUtc = GETUTCDATE(),
	JW_SystemLastEditUser = '~BP'
WHERE
	JW_PK = '{leg1PK}' or JW_PK = '{leg2PK}'");
			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobConsolTransport
SET
	JW_Vessel = 'Vessel1',
	JW_SystemLastEditTimeUtc = GETUTCDATE(),
	JW_SystemLastEditUser = '~BP'
WHERE
	JW_PK = '{leg1PK}'");
			Db.Connection.ExecuteNonQuery(@$"
UPDATE dbo.JobConsolTransport
SET
	JW_Vessel = 'Vessel2',
	JW_ETA = GETUTCDATE(),
	JW_SystemLastEditTimeUtc = GETUTCDATE(),
	JW_SystemLastEditUser = '~BP'
WHERE
	JW_PK = '{leg2PK}'");

			var result = DataUtils.GetDataTableFromQuery(Db.Connection, "SELECT LCV_Vessel FROM dbo.vw_List_ContainerAvailability");

			AssertEquals(1, result.Rows.Count);
			AssertEquals("Vessel2", result.Rows[0][0]);
		}
	}
}

