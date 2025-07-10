using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Sailing;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.TestFramework.FreightSailingTestHelper;

namespace Enterprise.Build.Database.Script.Public.Freight.Sailing
{
	[TestedType(typeof(TG_JobVoyOrigin_Update))]
	class TG_JobVoyOrigin_UpdateTest : DbCreateScriptTest
	{
		public void TestJobVoyOriginTrigger()
		{
			var jobVoyOriginPK = new FreightSailingTestHelper(TestConnection).SetupBasicLinkedTransportAndReturnPKOf(JobTypes.Origin);

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

			var portOfLoading = "ABC";
			var departureCTOAddress = Guid.NewGuid();
			var sDep = DateTime.Today;
			var eDep = sDep.AddDays(1);
			var aDep = sDep.AddDays(1);

			var documentaryCutOff = DateTime.Today;
			var vgmCutOff = documentaryCutOff.AddDays(1);
			var receivalCommences = documentaryCutOff.AddDays(2);
			var cutOff = documentaryCutOff.AddDays(3);

			var updateSql = @"
				UPDATE
					dbo.JobVoyOrigin 
				SET 
					{0} = '{1}',
					JA_SystemLastEditTimeUtc = GETUTCDATE(),
					JA_SystemLastEditUser = '~BP'
				WHERE
					JA_PK = '{2}'";

			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_RL_NKPortOfLoading", portOfLoading, jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(
				string.Format(insertIntoOrgAddressSql, Guid.NewGuid(), departureCTOAddress) +
				string.Format(updateSql, "JA_OA_DepartureCTOAddress", departureCTOAddress, jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_S_DEP", sDep.ToSqlFormat(), jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_E_DEP", eDep.ToSqlFormat(), jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_A_DEP", aDep.ToSqlFormat(), jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_DocumentaryCutoff", documentaryCutOff.ToSqlFormat(), jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_VGMCutOff", vgmCutOff.ToSqlFormat(), jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_ReceivalCommences", receivalCommences.ToSqlFormat(), jobVoyOriginPK));
			TestConnection.ExecuteNonQuery(string.Format(updateSql, "JA_CutOff", cutOff.ToSqlFormat(), jobVoyOriginPK));

			var selectSql = @"
				SELECT 
					{0}
				FROM
					dbo.JobConsolTransport
					JOIN dbo.JobSailing ON JX_PK = JW_JX
					JOIN dbo.JobVoyDestination ON JB_PK = JX_JB";

			AssertEquals(portOfLoading, (string)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_RL_NKLoadPort")));
			AssertEquals(departureCTOAddress, (Guid)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_OA_DepartureLocation")));
			AssertEquals(sDep, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_STD")));
			AssertEquals(eDep, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_ETD")));
			AssertEquals(aDep, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_ATD")));
			AssertEquals(documentaryCutOff, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_DocumentaryCutOff")));
			AssertEquals(vgmCutOff, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_VGMCutOff")));
			AssertEquals(receivalCommences, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_TerminalReceivalCommences")));
			AssertEquals(cutOff, (DateTime)TestConnection.ExecuteScalar(string.Format(selectSql, "JW_TerminalCutOff")));
		}
	}
}
