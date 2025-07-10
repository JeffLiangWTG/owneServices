using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.MasterData.MasterDataComplianceWise
{
	[TestedType(typeof(ComplianceRiskStatusCalculateJobEndDate))]
	public class ComplianceRiskStatusCalculateJobEndDateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() =>
			new ComplianceRiskStatusCalculateJobEndDate();

		readonly Guid JsPk1 = Guid.NewGuid();
		readonly Guid JsPk2 = Guid.NewGuid();
		readonly Guid JsPk3 = Guid.NewGuid();
		readonly Guid JsPk4 = Guid.NewGuid();
		readonly Guid JsPk5 = Guid.NewGuid();
		readonly Guid JsPk6 = Guid.NewGuid();
		readonly Guid JkPk1 = Guid.NewGuid();
		readonly Guid JkPk2 = Guid.NewGuid();
		readonly Guid JkPk3 = Guid.NewGuid();

		readonly Guid CorPk1 = Guid.NewGuid();
		readonly Guid CorPk2 = Guid.NewGuid();
		readonly Guid CorPk3 = Guid.NewGuid();
		readonly Guid CorPk4 = Guid.NewGuid();
		readonly Guid CorPk5 = Guid.NewGuid();
		readonly Guid CorPk6 = Guid.NewGuid();
		readonly Guid CorPk7 = Guid.NewGuid();
		readonly Guid CorPk8 = Guid.NewGuid();
		readonly Guid CorPk9 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();

			helper.CreateShipment(JsPk1, "JS0001");
			helper.CreateShipment(JsPk2, "JS0002");
			helper.CreateShipment(JsPk3, "JS0003");
			helper.CreateShipment(JsPk4, "JS0004");
			helper.CreateShipment(JsPk5, "JS0005");
			helper.CreateShipment(JsPk6, "JS0006");
			helper.CreateConsol(JkPk1, "JK0001");
			helper.CreateConsol(JkPk2, "JK0002");
			helper.CreateConsol(JkPk3, "JK0003");

			var thPk1 = helper.CreateSpotQuoteHeader("TH0001");
			var thPk2 = helper.CreateSpotQuoteHeader("TH0002");
			var thPk3 = helper.CreateSpotQuoteHeader("TH0003");

			// SHP1 with 2 Transports SHP2 with 1 Transport SHP3 with 0 Transport
			// CON1 with 2 Transports CON2 with 1 Transport CON3 with 0 Transport
			// SHP4 with 2 Transports SHP5 with 1 Transport SHP6 with 0 Transport
			var jwPk1 = helper.CreateJobConsolTransport(JsPk1, "SHP");
			var jwPk2 = helper.CreateJobConsolTransport(JsPk1, "SHP");
			var jwPk3 = helper.CreateJobConsolTransport(JsPk2, "SHP");
			var jwPk4 = helper.CreateJobConsolTransport(JkPk1, "CON");
			var jwPk5 = helper.CreateJobConsolTransport(JkPk1, "CON");
			var jwPk6 = helper.CreateJobConsolTransport(JkPk2, "CON");
			var jwPk7 = helper.CreateJobConsolTransport(JsPk4, "SHP");
			var jwPk8 = helper.CreateJobConsolTransport(JsPk4, "SHP");
			var jwPk9 = helper.CreateJobConsolTransport(JsPk5, "SHP");

			var sql = string.Format("UPDATE dbo.JobShipment SET JS_SystemCreateTimeUtc = '2021-02-01 00:00:00' WHERE JS_PK = '{0}';", JsPk1);
			sql += string.Format("UPDATE dbo.JobShipment SET JS_E_DEP = '2022-02-01 00:00:00', JS_E_ARV = '2022-02-10 00:00:00' WHERE JS_PK = '{0}';", JsPk3);

			sql += string.Format("UPDATE dbo.JobConsol SET JK_SystemCreateTimeUtc = '2021-02-01 00:00:00' WHERE JK_PK = '{0}';", JkPk1);
			sql += string.Format("UPDATE dbo.JobConsol SET JK_SystemCreateTimeUtc = '2021-02-01 00:00:00' WHERE JK_PK = '{0}';", JkPk2);
			sql += string.Format("UPDATE dbo.JobConsol SET JK_SystemCreateTimeUtc = '2021-02-01 00:00:00' WHERE JK_PK = '{0}';", JkPk3);

			sql += string.Format("UPDATE dbo.JobShipment SET JS_SystemCreateTimeUtc = '2021-02-01 00:00:00', JS_TH_OneTimeQuote = '{0}' WHERE JS_PK = '{1}';", thPk1, JsPk4);
			sql += string.Format("UPDATE dbo.JobShipment SET JS_SystemCreateTimeUtc = '2021-02-01 00:00:00', JS_TH_OneTimeQuote = '{0}' WHERE JS_PK = '{1}';", thPk2, JsPk5);
			sql += string.Format("UPDATE dbo.JobShipment SET JS_E_DEP = '2022-02-01 00:00:00', JS_E_ARV = '2022-02-10 00:00:00', JS_TH_OneTimeQuote = '{0}' WHERE JS_PK = '{1}';", thPk3, JsPk6);

			sql += string.Format("UPDATE dbo.JobConsolTransport SET JW_ETD = '2022-02-01 00:00:00', JW_ATD = '2022-02-02 00:00:00', JW_ETA = '2022-02-13 00:00:00', JW_ATA = '2022-02-14 00:00:00' WHERE JW_PK = '{0}';", jwPk1);
			sql += string.Format("UPDATE dbo.JobConsolTransport SET JW_ETD = '2022-02-01 00:00:00', JW_ATD = '2022-02-01 00:00:00', JW_ETA = '2022-02-11 00:00:00', JW_ATA = '2022-02-11 00:00:00' WHERE JW_PK = '{0}';", jwPk3);
			sql += string.Format("UPDATE dbo.JobConsolTransport SET JW_ETD = '2022-02-01 00:00:00', JW_ATD = '2022-02-02 00:00:00', JW_ETA = '2022-02-13 00:00:00', JW_ATA = '2022-02-14 00:00:00' WHERE JW_PK = '{0}';", jwPk4);
			sql += string.Format("UPDATE dbo.JobConsolTransport SET JW_ETD = '2022-02-01 00:00:00', JW_ATD = '2022-02-01 00:00:00', JW_ETA = '2022-02-11 00:00:00', JW_ATA = '2022-02-11 00:00:00' WHERE JW_PK = '{0}';", jwPk6);
			sql += string.Format("UPDATE dbo.JobConsolTransport SET JW_ETD = '2022-02-01 00:00:00', JW_ATD = '2022-02-02 00:00:00', JW_ETA = '2022-02-13 00:00:00', JW_ATA = '2022-02-14 00:00:00' WHERE JW_PK = '{0}';", jwPk7);
			sql += string.Format("UPDATE dbo.JobConsolTransport SET JW_ETD = '2022-02-01 00:00:00', JW_ATD = '2022-02-01 00:00:00', JW_ETA = '2022-02-11 00:00:00', JW_ATA = '2022-02-11 00:00:00' WHERE JW_PK = '{0}';", jwPk9);

			sql += @$"
INSERT INTO ComplianceRiskStatus (COR_PK,COR_ParentTableCode,COR_ParentID,COR_PartyRisk,COR_LocationRisk,COR_CommodityRisk,COR_OverallRisk,COR_JobEndDate,COR_SystemCreateTimeUtc,COR_SystemCreateUser,COR_SystemLastEditTimeUtc, COR_SystemLastEditUser)
VALUES
('{CorPk1}', 'JS', '{JsPk1}', 'CLR', 'CLR', 'INC', 'CLR', NULL, GETDATE(), 'USR', GETDATE(), 'USR'),
('{CorPk2}', 'JS', '{JsPk2}', 'HSK', 'BLK', 'PRS', 'OVR', NULL, GETDATE(), 'ADM', GETDATE(), 'ADM'),
('{CorPk3}', 'JS', '{JsPk3}', 'BLK', 'PSK', 'HSK', 'HLD', NULL, GETDATE(), 'SYS', GETDATE(), 'SYS'),
('{CorPk4}', 'JK', '{JkPk1}', 'PSK', 'CLR', 'BLK', 'BLK', NULL, GETDATE(), 'USR', GETDATE(), 'USR'),
('{CorPk5}', 'JK', '{JkPk2}', 'HSK', 'PSK', 'UNK', 'CLR', NULL, GETDATE(), 'SYS', GETDATE(), 'SYS'),
('{CorPk6}', 'JK', '{JkPk3}', 'BLK', 'CLR', 'NAS', 'OVR', NULL, GETDATE(), 'USR', GETDATE(), 'USR'),
('{CorPk7}', 'TH', '{thPk1}', 'CLR', 'PSK', 'PSK', 'BLK', NULL, GETDATE(), 'SYS', GETDATE(), 'SYS'),
('{CorPk8}', 'TH', '{thPk2}', 'HSK', 'CLR', 'INC', 'PSK', NULL, GETDATE(), 'USR', GETDATE(), 'USR'),
('{CorPk9}', 'TH', '{thPk3}', 'PSK', 'BLK', 'BLK', 'HLD', NULL, GETDATE(), 'ADM', GETDATE(), 'ADM');
";
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_JobShipment_SystemLastEditAuditInfoMustBeUpdated_Update", "dbo.JobShipment"))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_JobConsol_SystemLastEditAuditInfoMustBeUpdated_Update", "dbo.JobConsol"))
			using (DataTransformationHelper.SuspendTriggerIfExists("TG_JobConsolTransport_SystemLastEditAuditInfoMustBeUpdated_Update", "dbo.JobConsolTransport"))
			{
				TestConnection.ExecuteNonQuery(sql);
			}
		}

		protected override void AssertTransformationResults()
		{
			var resultList = new List<(Guid, string)>();
			TestConnection.ExecuteReader("SELECT COR_PK, COR_JobEndDate FROM dbo.ComPlianceRiskStatus", reader => resultList.Add(((Guid)reader["COR_PK"], ((DateTimeOffset)reader["COR_JobEndDate"]).ToString("yyyy-MM-dd"))));
			AssertContainsExactElementsInAnyOrder(new[] { (CorPk1, "2022-02-14")
				, (CorPk2, "2022-02-11")
				, (CorPk3, "2022-02-10")
				, (CorPk4, "2022-02-14")
				, (CorPk5, "2022-02-11")
				, (CorPk6, "2021-05-01")
				, (CorPk7, "2022-02-14")
				, (CorPk8, "2022-02-11")
				, (CorPk9, "2022-02-10") }, resultList);
		}
	}
}
