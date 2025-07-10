using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Sailing;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.TestFramework.FreightSailingTestHelper;

namespace Enterprise.Build.Database.Script.Public.Freight.Sailing
{
	[TestedType(typeof(TG_JobVoyDestination_Update))]
	class TG_JobVoyDestination_UpdateTest : DbCreateScriptTest
	{
		public void TestJobVoyDestinationTrigger()
		{
			var jobVoyDestinationPK = new FreightSailingTestHelper(TestConnection).SetupBasicLinkedTransportAndReturnPKOf(JobTypes.Destination);

			var insertIntoOrgAddressSql = @"
			INSERT INTO dbo.OrgHeader
				(OH_PK, OH_Code)
			VALUES
				('{0}', 'XYZ')

			---------------------------------
			INSERT INTO dbo.OrgAddress
				(OA_PK, OA_OH, OA_Address1)
			VALUES
				('{1}', '{0}', 'a')";

			var portOfDischarge = "ABC";
			var arrivalCTOAddress = Guid.NewGuid();
			var sArv = DateTime.Today;
			var eArv = sArv.AddDays(1);
			var aArv = sArv.AddDays(1);

			var terminalAvailabilityDate = DateTime.Today;
			var terminalStorageDate = terminalAvailabilityDate.AddDays(1);

			var updateSql = @"
				UPDATE
					dbo.JobVoyDestination 
				SET 
					{0} = '{1}',
					JB_SystemLastEditTimeUtc = GETUTCDATE(),
					JB_SystemLastEditUser = '~BP'
				WHERE
					JB_PK = '{2}'";

			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JB_RL_NKPortOfDischarge", portOfDischarge, jobVoyDestinationPK));
			TestConnection.ExecuteNonQuery(
				string.Format(insertIntoOrgAddressSql, Guid.NewGuid(), arrivalCTOAddress) +
				string.Format(updateSql, "JB_OA_ArrivalCTOAddress", arrivalCTOAddress, jobVoyDestinationPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JB_S_ARV", sArv.ToString("s"), jobVoyDestinationPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JB_E_ARV", eArv.ToString("s"), jobVoyDestinationPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JB_A_ARV", aArv.ToString("s"), jobVoyDestinationPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JB_AvailabilityDate", terminalAvailabilityDate.ToString("s"), jobVoyDestinationPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JB_StorageDate", terminalStorageDate.ToString("s"), jobVoyDestinationPK));

			var selectSql = @"
				SELECT 
					{0}
				FROM
					dbo.JobConsolTransport
					JOIN dbo.JobSailing ON JX_PK = JW_JX
					JOIN dbo.JobVoyDestination ON JB_PK = JX_JB";

			AssertEquals(portOfDischarge, (string)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_RL_NKDiscPort")));
			AssertEquals(arrivalCTOAddress, (Guid)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_OA_ArrivalLocation")));
			AssertEquals(sArv, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_STA")));
			AssertEquals(eArv, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_ETA")));
			AssertEquals(aArv, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_ATA")));
			AssertEquals(terminalAvailabilityDate, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_TerminalAvailabilityDate")));
			AssertEquals(terminalStorageDate, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_TerminalStorageDate")));
		}
	}
}
