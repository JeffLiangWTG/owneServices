using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.BufferManagement
{
	[TestedType(typeof(ChangeDateAcceptabilityDataToNewValues))]
	class ChangeDateAcceptabilityDataToNewValuesTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() =>
			new ChangeDateAcceptabilityDataToNewValues();

		protected override void PrepareTestData()
		{
			var pk1 = Guid.NewGuid();
			var pk2 = Guid.NewGuid();
			var pk3 = Guid.NewGuid();
			var pk4 = Guid.NewGuid();
			var pk5 = Guid.NewGuid();
			var pk6 = Guid.NewGuid();

			var sql = @$"
INSERT INTO dbo.ProcessHeader
	(FH_CompletionStatement, FH_DoNotStartBeforeDate, FH_PK, FH_IsActive, FH_FC_CurrentComponent, FH_ParentTemplateId, FH_ReleaseDateTime, FH_GG_ReleaseGroup, FH_VoteUpDownAmount, FH_P0_Template, FH_AgreedDeliveryDate, FH_DateAcceptability, FH_Status, FH_StaggeredReleaseDelayExpiry, FH_TimeDelayFactor, FH_TimeDelayMinutes, FH_FH_ParentHeader, FH_ParentId, FH_ParentTableCode, FH_AllowTaskAutoAssignment, FH_IsStandby, FH_IsCriticalHandover, FH_IsReleasableUnitParent, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_PlannedDurationInMinutes, FH_BufferPenetrationPercentWhenCompleted, FH_WorkflowType, FH_AgreedDeliveryDateDefaultHoursOffset, FH_AgreedDeliveryDateDefaultsFrom, FH_EarliestStartDateDefaultsFrom, FH_EarliestStartDefaultHoursOffset, FH_Category, FH_LastTransferType, FH_TaskPenetrationResetDateTimeUtc, FH_IsReleaseGroupSetByTemplate, FH_AutoVersion)
VALUES
	('TESTDeadlineType', '2023-01-23', '{pk1}', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '{pk2}', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '{pk3}', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '{pk4}', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '{pk5}', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '{pk6}', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD501', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '_|_', 'OPN', NULL, 0, 0, NULL, '{pk1}', '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD502', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '/|_', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD503', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '¯|_', 'OPN', NULL, 0, 0, NULL, '{pk2}', '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD504', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '¯|¯', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD505', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '¯|\', 'OPN', NULL, 0, 0, NULL, '{pk3}', '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD506', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '/|¯', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD507', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '/|\', 'OPN', NULL, 0, 0, NULL, '{pk4}', '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD508', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '_|¯', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD509', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '_|\', 'OPN', NULL, 0, 0, NULL, '{pk5}', '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD50A', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '???', 'OPN', NULL, 0, 0, NULL, NULL, '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1),
	('TESTDeadlineType', '2023-01-23', '8C432E47-2E39-4B46-8927-76CDF51BD50B', 1, NULL, NULL, NULL, NULL, 0, NULL, NULL, '',    'OPN', NULL, 0, 0, NULL, '{pk6}', '', 0, 0, 0, 0, GETUTCDATE(), 'E', GETUTCDATE(), 'E', 0, 0, 'NA', NULL, '', '', NULL, '', 'NA', NULL, 0, 1)
";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD501' AND FH_DeadlineType = 'HRD'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD502' AND FH_DeadlineType = 'HRD'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD503' AND FH_DeadlineType = 'HRD'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD504' AND FH_DeadlineType = 'SFT'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD505' AND FH_DeadlineType = 'SFT'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD506' AND FH_DeadlineType = 'SFT'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD507' AND FH_DeadlineType = 'SFT'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD508' AND FH_DeadlineType = 'SFT'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD509' AND FH_DeadlineType = 'SFT'"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD50A' AND FH_DeadlineType = ''"));
			Assert(Db.Connection.Exists("FROM dbo.ProcessHeader WHERE FH_PK = '8C432E47-2E39-4B46-8927-76CDF51BD50B' AND FH_DeadlineType = ''"));
		}
	}
}
