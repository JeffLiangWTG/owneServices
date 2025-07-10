using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(GlowDeleteReviewProcessDemographicRule))]
	class GlowDeleteReviewProcessDemographicRuleTest : DbCreateScriptTest
	{
		public void TestRule()
		{
			var demographicPk1 = Guid.NewGuid();
			var demographicPk2 = Guid.NewGuid();

			PrepareTestData(demographicPk1, demographicPk2);

			RunStoredProcedure(demographicPk1, 0);

			AssertEquals(0, ReviewProcessDemographicCount(demographicPk1));
			AssertEquals(0, ReviewProcessBudgetCount(demographicPk1));
			AssertEquals(0, StmModuleFilterCount(demographicPk1));

			AssertEquals(1, ReviewProcessDemographicCount(demographicPk2));
			AssertEquals(1, ReviewProcessBudgetCount(demographicPk2));
			AssertEquals(1, StmModuleFilterCount(demographicPk2));
		}

		public void TestVersion()
		{
			var demographicPk1 = Guid.NewGuid();
			var demographicPk2 = Guid.NewGuid();

			PrepareTestData(demographicPk1, demographicPk2);

			RunStoredProcedure(demographicPk1, 1);

			AssertEquals(1, ReviewProcessDemographicCount(demographicPk1));
			AssertEquals(1, ReviewProcessBudgetCount(demographicPk1));
			AssertEquals(1, StmModuleFilterCount(demographicPk1));

			AssertEquals(1, ReviewProcessDemographicCount(demographicPk2));
			AssertEquals(1, ReviewProcessBudgetCount(demographicPk2));
			AssertEquals(1, StmModuleFilterCount(demographicPk2));
		}

		void PrepareTestData(Guid demographicPk1, Guid demographicPk2)
		{
			var sql = $@"
DECLARE @demographicPk1 UNIQUEIDENTIFIER = '{demographicPk1}';
DECLARE @demographicPk2 UNIQUEIDENTIFIER = '{demographicPk2}';

INSERT INTO [dbo].[StmModuleFilter] (S9_PK, S9_ModuleID, S9_FilterName, S9_FilterData, S9_RelatedEntityID, S9_SystemCreateTimeUtc, S9_SystemLastEditTimeUtc, S9_SystemCreateUser, S9_SystemLastEditUser) 
VALUES 
	(NEWID(), 'IReviewProcessDemographic',  'Budget Demographics filter 1', CONVERT(varbinary(max), 'dummy_filter_data'),  @demographicPk1, GetUtcDate(), GetUtcDate(), 'UUZ', 'UUZ'),
	(NEWID(), 'IReviewProcessDemographic',  'Budget Demographics filter 2', CONVERT(varbinary(max), 'dummy_filter_data'),  @demographicPk2, GetUtcDate(), GetUtcDate(), 'UUZ', 'UUZ');

INSERT INTO
	GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(NEWID(), 'user001_', 'UUZ', GETUTCDATE(), 'UUZ', GETUTCDATE(), 'UUZ');

DECLARE @GC_PK uniqueidentifier;
SELECT TOP 1 @GC_PK = [GC_PK] FROM [GlbCompany]

INSERT INTO [ReviewProcess] ([RPR_PK], [RPR_Name], [RPR_Type], [RPR_Status], [RPR_EffectiveDate], [RPR_SubmissionDate], [RPR_RX_NKCurrency], [RPR_S9_EmployeesInReview], [RPR_PrimaryHierarchy], [RPR_OverrideHierarchy], [RPR_SystemCreateTimeUtc], [RPR_SystemCreateUser], [RPR_SystemLastEditTimeUtc], [RPR_SystemLastEditUser], [RPR_ConfigType], [RPR_GC_Company])
VALUES
	(NEWID(), 'DummyRP001', 'REM', 'UNS', '2023-01-01', '2023-01-02', 'AUD', null, 'DRM', 'RRV', '2023-01-01T00:00:00', 'UUZ', '2023-01-01T12:00:00', 'UUZ', 'c', @GC_PK);

DECLARE @GS_PK uniqueidentifier;
SELECT TOP 1 @GS_PK = [GS_PK] FROM [GlbStaff] WHERE GS_Code = 'UUZ';

DECLARE @RPR_PK uniqueidentifier;
SELECT TOP 1 @RPR_PK = [RPR_PK] FROM [ReviewProcess] WHERE RPR_Name = 'DummyRP001';

DECLARE @S9_PK1 uniqueidentifier;
SELECT TOP 1 @S9_PK1 = [S9_PK] FROM [StmModuleFilter] WHERE S9_RelatedEntityID = @demographicPk1;
DECLARE @S9_PK2 uniqueidentifier;
SELECT TOP 1 @S9_PK2 = [S9_PK] FROM [StmModuleFilter] WHERE S9_RelatedEntityID = @demographicPk2;
INSERT INTO [dbo].[ReviewProcessDemographic] ([RPD_PK], [RPD_RPR_ReviewProcess], [RPD_Name], [RPD_StandardIncreasePercent], [RPD_S9_Filter], [RPD_Priority], [RPD_SystemCreateTimeUtc], [RPD_SystemCreateUser], [RPD_SystemLastEditTimeUtc], [RPD_SystemLastEditUser])
VALUES
    (@demographicPk1, @RPR_PK, 'Dummy Review Process Demographic 1', 7.50, @S9_PK1, 1, '2024-03-05 10:00:00', 'UUZ', '2024-03-05 10:00:00', 'UUZ'),
    (@demographicPk2, @RPR_PK, 'Dummy Review Process Demographic 2', 7.50, @S9_PK2, 2, '2024-03-05 10:00:00', 'UUZ', '2024-03-05 10:00:00', 'UUZ');

INSERT INTO [dbo].[ReviewProcessBudget] ([RPB_PK], [RPB_RPD_Demographic], [RPB_Entitlement], [RPB_CanOffer], [RPB_WeightingPercent], [RPB_SystemCreateTimeUtc], [RPB_SystemCreateUser], [RPB_SystemLastEditTimeUtc], [RPB_SystemLastEditUser])
VALUES
	(NEWID(), @demographicPk1, 'ENT', 1, 20.00, '2024-03-05 10:00:00', 'UUZ', '2024-03-05 10:00:00', 'UUZ'),
	(NEWID(), @demographicPk2, 'ENT', 1, 20.00, '2024-03-05 10:00:00', 'UUZ', '2024-03-05 10:00:00', 'UUZ');
";

			TestConnection.ExecuteNonQuery(sql);
		}

		object ReviewProcessDemographicCount(Guid demographicPk) => TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM ReviewProcessDemographic WHERE RPD_PK = '{demographicPk}'");

		object ReviewProcessBudgetCount(Guid demographicPk) => TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM ReviewProcessBudget WHERE RPB_RPD_Demographic = '{demographicPk}'");

		object StmModuleFilterCount(Guid demographicPk) => TestConnection.ExecuteScalar($"SELECT COUNT(*) FROM StmModuleFilter WHERE S9_RelatedEntityID = '{demographicPk}'");

		static void RunStoredProcedure(Guid pk, short version)
		{
			using (var command = Db.Connection.Command("GlowDeleteReviewProcessDemographicRule"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@ReviewProcessDemographicPK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@Version", SqlDbType.SmallInt, version);
				command.ExecuteScalar();
			}
		}
	}
}
