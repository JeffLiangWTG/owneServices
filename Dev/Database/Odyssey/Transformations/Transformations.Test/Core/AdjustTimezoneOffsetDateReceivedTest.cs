using System;
using System.Data;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(AdjustTimezoneOffsetDateReceived))]
	class AdjustTimezoneOffsetDateReceivedTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new AdjustTimezoneOffsetDateReceived();

		protected override void PrepareTestData()
		{
			_ = new GlbBranch(branchCodeAU) { GB_RL_NKHomePort = unlocoAU }.InsertAndReturnObject(TestConnection);

			PrepareJobRequiredDocumentTestData(jobPK1, dateReceived1, branchCodeAU);
			PrepareJobRequiredDocumentTestData(jobPK2, dateReceived2, branchCodeAU);

			AssertEquals("JobRequiredDocument records", 2, (int)TestConnection.ExecuteScalar("SELECT COUNT(1) FROM dbo.JobRequiredDocument WHERE EQ_DateReceived is not NULL"));
		}

		protected override void AssertTransformationResults()
		{
			AssertDateReceivedUpdated(jobPK1, dateReceived1, 11);
			AssertDateReceivedUpdated(jobPK2, dateReceived2, 10);
			AssertEquals(0, Db.Connection.ExecuteScalar<int>($"SELECT COUNT(1) FROM [dbo].[StmData] WHERE [SD_Name] = '{transformIsRunningName}'"));
		}

		void AssertDateReceivedUpdated(Guid jobPK, DateTimeOffset expectedDateTime, int expectedOffset)
		{
			TestConnection.ExecuteReader($"SELECT EQ_DateReceived, EQ_SystemLastEditTimeUtc, EQ_SystemLastEditUser FROM dbo.JobRequiredDocument WHERE EQ_PK = '{jobPK}'", reader =>
			{
				var actualDateReceived = (DateTimeOffset)reader.GetValue(0);
				var expectedDateReceived = new DateTimeOffset(expectedDateTime.DateTime, new TimeSpan(expectedOffset, 0, 0));

				AssertEquals(expectedDateReceived, actualDateReceived);

				var actualSystemLastEditTime = (DateTime)reader.GetValue(1);
				var actualSystemLastEditUser = (string)reader.GetValue(2);
				AssertGreaterThanOrEqualTo(actualSystemLastEditTime, systemAuditDate);
				AssertEquals(actualSystemLastEditUser, systemUser);
			});
		}

		void AssertDateReceivedNoUpdate(Guid jobPK, DateTimeOffset expectedDateTime, int expectedOffset)
		{
			TestConnection.ExecuteReader($"SELECT EQ_DateReceived, EQ_SystemLastEditTimeUtc, EQ_SystemLastEditUser FROM dbo.JobRequiredDocument WHERE EQ_PK = '{jobPK}'", reader =>
			{
				var actualDateReceived = (DateTimeOffset)reader.GetValue(0);
				var expectedDateReceived = new DateTimeOffset(expectedDateTime.DateTime, new TimeSpan(expectedOffset, 0, 0));

				AssertEquals(expectedDateReceived, actualDateReceived);

				var actualSystemLastEditTime = (DateTime)reader.GetValue(1);
				var actualSystemLastEditUser = (string)reader.GetValue(2);
				AssertEquals(actualSystemLastEditTime, systemAuditDate);
				AssertEquals(actualSystemLastEditUser, editedUser);
			});
		}

		void PrepareRefUNLOCOUtcOffsetTestData()
		{
			var sql = @"
IF EXISTS(SELECT NULL FROM RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = @unloco1 AND RLO_StartTimeUtc <= '2023-09-30 16:00:00' AND RLO_EndTimeUtc >= '2024-04-06 16:00:00')
BEGIN
	UPDATE dbo.RefDatabase_RefUNLOCOUtcOffset
		SET RLO_OffsetMinutesFromUtc = 660
		WHERE RLO_RL_NKCode = @unloco1 AND RLO_StartTimeUtc <= '2023-09-30 16:00:00' AND RLO_EndTimeUtc >= '2024-04-06 16:00:00'
END
ELSE
BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
	(NEWID(), @unloco1, '2023-09-30 16:00:00', '2024-04-06 16:00:00', 660)
END

IF EXISTS(SELECT NULL FROM RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = @unloco1 AND RLO_StartTimeUtc <= '2024-04-06 16:00:00' AND RLO_EndTimeUtc >= '2024-10-05 16:00:00')
BEGIN
	UPDATE dbo.RefDatabase_RefUNLOCOUtcOffset
		SET RLO_OffsetMinutesFromUtc = 600
		WHERE RLO_RL_NKCode = @unloco1 AND RLO_StartTimeUtc <= '2024-04-06 16:00:00' AND RLO_EndTimeUtc >= '2024-10-05 16:00:00'
END
ELSE
BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
	(NEWID(), @unloco1, '2024-04-06 16:00:00', '2024-10-05 16:00:00', 600)
END

IF NOT EXISTS(SELECT NULL FROM RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = @unloco2 AND RLO_StartTimeUtc <= '2023-10-29 01:00:00' AND RLO_EndTimeUtc >= '2024-03-31 01:00:00')
BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
	(NEWID(), @unloco2, '2023-10-29 01:00:00', '2024-03-31 01:00:00', 0)
END

IF NOT EXISTS(SELECT NULL FROM RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = @unloco2 AND RLO_StartTimeUtc <= '2024-03-31 01:00:00' AND RLO_EndTimeUtc >= '2024-10-27 01:00:00')
BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES
	(NEWID(), @unloco2, '2024-03-31 01:00:00', '2024-10-27 01:00:00', 60)
END
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@unloco1", SqlDbType.VarChar, 5, unlocoAU);
				cmd.AddParameter("@unloco2", SqlDbType.VarChar, 5, unlocoUK);

				cmd.ExecuteNonQuery();
			}
		}

		void PrepareJobRequiredDocumentTestData(Guid jobPK, DateTimeOffset dateReceived, string branchCode)
		{
			var sql = @"
INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_SE_NKEvent, SL_GB_NKBranch)
VALUES
(@slPK, 'JobRequiredDocument', @eqPK, GETUTCDATE(), 'ADD', @gbCode);

INSERT INTO dbo.JobRequiredDocument (EQ_PK, EQ_DateReceived, EQ_ParentID, EQ_SystemCreateTimeUtc, EQ_SystemCreateUser, EQ_SystemLastEditTimeUtc, EQ_SystemLastEditUser)
VALUES
(@eqPK, @eqDateReceived, NEWID(), GETUTCDATE(), @systemUser, @systemAuditDate, @editedUser);
";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@eqPK", SqlDbType.UniqueIdentifier, jobPK);
				cmd.AddParameter("@eqDateReceived", SqlDbType.DateTimeOffset, dateReceived);
				cmd.AddParameter("@gbCode", SqlDbType.Char, 3, branchCode);
				cmd.AddParameter("@slPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				cmd.AddParameter("@systemUser", SqlDbType.Char, 3, systemUser);
				cmd.AddParameter("@editedUser", SqlDbType.Char, 3, editedUser);
				cmd.AddParameter("@systemAuditDate", SqlDbType.SmallDateTime, systemAuditDate);

				cmd.ExecuteNonQuery();
			}
		}

		#region TestTwoBranchs

		public void TestTwoBranchs()
		{
			_ = new GlbBranch(branchCodeAU) { GB_RL_NKHomePort = unlocoAU }.InsertAndReturnObject(TestConnection);
			_ = new GlbBranch(branchCodeUK) { GB_RL_NKHomePort = unlocoUK }.InsertAndReturnObject(TestConnection);

			PrepareJobRequiredDocumentTestData(jobPK1, dateReceived1, branchCodeAU);
			PrepareJobRequiredDocumentTestData(jobPK3, dateReceived3, branchCodeUK);

			AssertDateReceivedNoUpdate(jobPK1, dateReceived1, 0);
			AssertDateReceivedNoUpdate(jobPK3, dateReceived3, 0);

			RunTestTransformAndSetup();

			AssertDateReceivedUpdated(jobPK1, dateReceived1, 11);
			AssertDateReceivedUpdated(jobPK3, dateReceived3, 1);
		}

		#endregion

		#region TestNoNeedToUpdate

		public void TestNoNeedToUpdate()
		{
			_ = new GlbBranch(branchCodeSG) { GB_RL_NKHomePort = unlocoSG }.InsertAndReturnObject(TestConnection);

			PrepareJobRequiredDocumentTestData(jobPK4, dateReceived4, branchCodeSG);
			PrepareJobRequiredDocumentTestData(jobPK5, dateReceived5, branchCodeSG);

			RunTestTransformAndSetup();

			AssertDateReceivedNoUpdate(jobPK4, dateReceived4, 0);
			AssertDateReceivedNoUpdate(jobPK5, dateReceived5, 7);
		}

		void RunTestTransformAndSetup()
		{
			var instance = GetNewTestTransformationInstance();
			instance.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		}

		#endregion

		public void TestStmDataSetInOnlinePreUpgrade()
		{
			var transformation = GetNewTestTransformationInstance();
			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals(1, Db.Connection.ExecuteScalar<int>($"SELECT COUNT(1) FROM [dbo].[StmData] WHERE [SD_Name] = '{transformIsRunningName}'"));
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			PrepareRefUNLOCOUtcOffsetTestData();
		}

		#endregion

		Guid jobPK1 = Guid.NewGuid();
		readonly DateTimeOffset dateReceived1 = DateTimeOffset.ParseExact("2024-04-01 07:01 +00:00", "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);

		Guid jobPK2 = Guid.NewGuid();
		readonly DateTimeOffset dateReceived2 = DateTimeOffset.ParseExact("2024-04-07 07:02 +00:00", "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);

		Guid jobPK3 = Guid.NewGuid();
		readonly DateTimeOffset dateReceived3 = DateTimeOffset.ParseExact("2024-04-02 07:03 +00:00", "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);

		Guid jobPK4 = Guid.NewGuid();
		readonly DateTimeOffset dateReceived4 = DateTimeOffset.ParseExact("2024-04-01 07:04 +00:00", "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);

		Guid jobPK5 = Guid.NewGuid();
		readonly DateTimeOffset dateReceived5 = DateTimeOffset.ParseExact("2024-04-01 07:04 +07:00", "yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture);

		readonly string branchCodeAU = "AU1";
		readonly string unlocoAU = "AUSYD";

		readonly string branchCodeUK = "GB1";
		readonly string unlocoUK = "GBGB1";

		readonly string branchCodeSG = "SG1";
		readonly string unlocoSG = "SGSG1";

		readonly string systemUser = "~BP";
		readonly string editedUser = "AAA";
		readonly DateTime systemAuditDate = DateTime.ParseExact("2024-04-01 07:00", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

		readonly string transformIsRunningName = "AdjustTimezoneOffsetDateReceived.IsRunning";
	}
}
