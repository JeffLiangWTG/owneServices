using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(UpdateCusEntryHeaderStatusAndEntryStatusAccordingToStmALog))]
	public class UpdateCusEntryHeaderStatusAndEntryStatusAccordingToStmALogTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateCusEntryHeaderStatusAndEntryStatusAccordingToStmALog();
		}

		protected override void AssertTransformationResults()
		{
			AssertResult(entryPK1, 2, "CLO", "39");
			AssertResult(entryPK2, 2, "ERO", "41");
			AssertResult(entryPK3, 1, "", "200");
			AssertResult(entryPK4, 1, "", "200");
			AssertResult(entryPK5, 1, "", "XXX");
			AssertResult(entryPK6, 1, "", "200");
			AssertResult(entryPK7, 2, "CLO", "39");
			AssertResult(entryPK8, 2, "ERO", "41");
			AssertResult(entryPK9, 2, "CLO", "39");
			AssertResult(entryPK10, 2, "ERO", "41");
			AssertResult(entryPK11, 1, "", "200");
			AssertResult(entryPK12, 1, "", "200");
			AssertResult(entryPK13, 1, "", "200");
			AssertResult(entryPK14, 1, "", "200");
		}

		void AssertResult(Guid pk, int autoVersion, string status, string entryStatus)
		{
			var sql = $"SELECT CH_Status, CH_EntryStatus FROM dbo.CusEntryHeader WHERE CH_PK = '{pk}' AND CH_AutoVersion = {autoVersion}";
			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				var count = 0;
				if (reader.Read())
				{
					count++;
					AssertEquals(status, (string)reader[0]);
					AssertEquals(entryStatus, (string)reader[1]);
				}
				AssertEquals(1, count);
			}
		}

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var caCompanyPK = testDataCreator.CreateGlbCompany("AAA", "CA");
			var caBranchPK = testDataCreator.CreateGlbBranch("DDD", caCompanyPK);
			CreateTestData(entryPK1, caCompanyPK, caBranchPK, 1, "CAD", "CA", "200", new string[] { "39", "200" }, new string[] { "2024-10-01", "2024-10-02" });
			CreateTestData(entryPK2, caCompanyPK, caBranchPK, 2, "CAD", "CA", "200", new string[] { "41", "200" }, new string[] { "2024-10-01", "2024-10-02" });
			CreateTestData(entryPK3, caCompanyPK, caBranchPK, 3, "CAD", "US", "200", new string[] { "41", "200" }, new string[] { "2024-10-01", "2024-10-02" });
			CreateTestData(entryPK4, caCompanyPK, caBranchPK, 4, "B3C", "CA", "200", new string[] { "41", "200" }, new string[] { "2024-10-01", "2024-10-02" });
			CreateTestData(entryPK5, caCompanyPK, caBranchPK, 5, "CAD", "CA", "XXX", new string[] { "41", "200" }, new string[] { "2024-10-01", "2024-10-02" });
			CreateTestData(entryPK6, caCompanyPK, caBranchPK, 6, "CAD", "CA", "200", new string[] { "41", "200" }, new string[] { "2024-09-01", "2024-09-02" });
			CreateTestData(entryPK7, caCompanyPK, caBranchPK, 7, "CAD", "CA", "200", new string[] { "41", "39", "200" }, new string[] { "2024-10-01", "2024-10-02", "2024-10-03" });
			CreateTestData(entryPK8, caCompanyPK, caBranchPK, 8, "CAD", "CA", "200", new string[] { "39", "41", "200" }, new string[] { "2024-10-01", "2024-10-02", "2024-10-03" });
			CreateTestData(entryPK9, caCompanyPK, caBranchPK, 9, "CAD", "CA", "200", new string[] { "39", "200", "ABC" }, new string[] { "2024-10-01", "2024-10-02", "2024-10-03" });
			CreateTestData(entryPK10, caCompanyPK, caBranchPK, 10, "CAD", "CA", "200", new string[] { "41", "200", "ABC" }, new string[] { "2024-10-01", "2024-10-02", "2024-10-03" });
			CreateTestData(entryPK11, caCompanyPK, caBranchPK, 11, "CAD", "CA", "200", new string[] { "41", "200", "200" }, new string[] { "2024-10-01", "2024-10-02", "2024-10-03" });
			CreateTestData(entryPK12, caCompanyPK, caBranchPK, 12, "CAD", "CA", "200", new string[] { "200", "39" }, new string[] { "2024-10-01", "2024-10-02" });
			CreateTestData(entryPK13, caCompanyPK, caBranchPK, 13, "CAD", "CA", "200", new string[] { "200", "41" }, new string[] { "2024-10-01", "2024-10-02" });
			CreateTestData(entryPK14, caCompanyPK, caBranchPK, 14, "CAD", "CA", "200", new string[] { "39", "41" }, new string[] { "2024-10-01", "2024-10-02" });
		}

		void CreateTestData(Guid entryPK, Guid companyPK, Guid branchPK, int clusterKey, string messageType, string dataModel, string entryStatus, string[] references, string[] createDates)
		{
			var sql = @$"
DECLARE @DecPK AS UNIQUEIDENTIFIER;
SET @DecPK = NEWID();
INSERT INTO JobDeclaration(JE_PK, JE_GC, JE_GB, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser, JE_DeclarationReference)
VALUES(@DecPK, '{companyPK}', '{branchPK}', {clusterKey}, '{dataModel}', GETUTCDATE(), 'HXU', GETUTCDATE(), 'HXU', 'HXU'+'{clusterKey}');
INSERT INTO CusEntryHeader(CH_PK, CH_JE, CH_MessageType, CH_ClusterKey, CH_DataModel, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser, CH_EntryStatus, CH_AutoVersion)
VALUES('{entryPK}', @DecPK, '{messageType}', {clusterKey}, '{dataModel}', GETUTCDATE(), 'HXU', GETUTCDATE(), 'HXU', '{entryStatus}', 1);
";
			for (int i = 0; i < references.Length; i++)
			{
				var reference = references[i];
				var date = createDates[i];
				sql += $@"
INSERT INTO StmALog (SL_PK, SL_Parent, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_GS_NKUser, SL_SE_NKEvent, SL_Reference) 
VALUES (NEWID(), '{entryPK}', 'CusEntryHeader', GETUTCDATE(), '{date}', 'HXU', 'CES', '{reference}');";
			}

			Db.Connection.ExecuteNonQuery(sql);
		}

		Guid entryPK1 = Guid.NewGuid();
		Guid entryPK2 = Guid.NewGuid();
		Guid entryPK3 = Guid.NewGuid();
		Guid entryPK4 = Guid.NewGuid();
		Guid entryPK5 = Guid.NewGuid();
		Guid entryPK6 = Guid.NewGuid();
		Guid entryPK7 = Guid.NewGuid();
		Guid entryPK8 = Guid.NewGuid();
		Guid entryPK9 = Guid.NewGuid();
		Guid entryPK10 = Guid.NewGuid();
		Guid entryPK11 = Guid.NewGuid();
		Guid entryPK12 = Guid.NewGuid();
		Guid entryPK13 = Guid.NewGuid();
		Guid entryPK14 = Guid.NewGuid();

		public override string[] expectedIndex => new string[] { "NONCLUSTERED INDEX [_WTG__Update Status And Entry Status Of CAD CusEntryHeader According To StmALog._1] ON [dbo].[CusEntryHeader] ([CH_DataModel], [CH_MessageType], [CH_EntryStatus]) INCLUDE ([CH_PK]) WHERE ([CH_DataModel]='CA' AND [CH_MessageType]='CAD' AND [CH_EntryStatus]='200') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)" };
	}
}
