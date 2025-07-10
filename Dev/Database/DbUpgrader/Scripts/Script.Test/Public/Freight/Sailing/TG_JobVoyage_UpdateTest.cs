using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Sailing;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.TestFramework.FreightSailingTestHelper;

namespace Enterprise.Build.Database.Script.Public.Freight.Sailing
{
	[TestedType(typeof(TG_JobVoyage_Update))]
	class TG_JobVoyage_UpdateTest : DbCreateScriptTest
	{
		public void TestJobVoyageTrigger()
		{
			var jobVoyagePK = new FreightSailingTestHelper(TestConnection).SetupBasicLinkedTransportAndReturnPKOf(JobTypes.Voyage);

			var vessel = "Vessel";
			var flight = "Flight";
			var aircraftType = "E90";
			var systemLastEditTimeUtc = DateTime.Today;
			var systemLastEditUser = "E";

			var updateSql = $@"
				UPDATE
					dbo.JobVoyage
				SET
					JV_VoyageFlight = '{flight}',
					JV_AircraftType = '{aircraftType}',
					JV_RV_NKVessel = '{vessel}',
					JV_IsChartered = '1',
					JV_IsCargoOnly = '1',
					JV_SystemLastEditTimeUtc = '{systemLastEditTimeUtc.ToSqlFormat()}',
					JV_SystemLastEditUser = '{systemLastEditUser}'
				WHERE
					JV_PK = '{jobVoyagePK}'";
			TestConnection.ExecuteNonQuery(updateSql);

			var selectSql = @"
				SELECT 
					{0}
				FROM
					dbo.JobConsolTransport
					JOIN dbo.JobSailing ON JX_PK = JW_JX
					JOIN dbo.JobVoyOrigin ON JA_PK = JX_JA
					JOIN dbo.JobVoyage ON JV_PK = JA_JV";

			AssertEquals(vessel, TestConnection.ExecuteScalar<string>(string.Format(selectSql, "JW_Vessel")));
			AssertEquals(flight, TestConnection.ExecuteScalar<string>(string.Format(selectSql, "JW_VoyageFlight")));
			Assert(TestConnection.ExecuteScalar<bool>(string.Format(selectSql, "JW_IsCharter")));
			Assert(TestConnection.ExecuteScalar<bool>(string.Format(selectSql, "JW_IsCargoOnly")));
			AssertEquals(aircraftType, TestConnection.ExecuteScalar<string>(string.Format(selectSql, "JW_AircraftType")));
			AssertEquals(systemLastEditTimeUtc, TestConnection.ExecuteScalar<DateTime>(string.Format(selectSql, "JV_SystemLastEditTimeUtc")));
			AssertEquals(systemLastEditUser, TestConnection.ExecuteScalar<string>(string.Format(selectSql, "JV_SystemLastEditUser")));
		}
	}
}

