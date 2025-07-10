using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ClientSpecific.EDI
{
	[TestedType(typeof(UpdateIM_CloseTimeUtcValueForInconsistentTimestampsTransform))]
	public class UpdateIM_CloseTimeUtcValueForInconsistentTimestampsTransformTest : DataTransformationTestCase
	{
		[SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Baseline")]
		protected override void AssertTransformationResults()
		{
			var incident0Result = Db.Connection.ExecuteScalar<DateTime>(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001000'");
			var incident1Result = Db.Connection.ExecuteScalar<DateTime>(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001001'");
			var incident2Result = Db.Connection.ExecuteScalar<DateTime>(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001002'");
			var incident3Result = Db.Connection.ExecuteScalar<DateTime>(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001003'");

			AssertEquals("The value should only be latest ICL event's SL_EventTimeUtc", DateTime.Parse("2027-07-01 12:00:00"), incident0Result);
			AssertEquals("The value should only be latest JCM_PostedTimeUtc if the event's SL_EventTimeUtc and SL_PostedTimeUtc is empty", DateTime.Parse("2023-12-08 01:42:52.333"), incident1Result);
			AssertEquals("The value should only be latest ICL event's SL_EventTimeUtc", DateTime.Parse("2023-06-01 12:00:00"), incident2Result);
			AssertEquals("The value should only be latest JCM_PostedTimeUtc if the event's SL_EventTimeUtc and SL_PostedTimeUtc is empty", DateTime.Parse("2023-12-08 01:42:52.443"), incident3Result);
		}

		[ExpectNoExceptions]
		public void TestTransformationWhenNoIncidentMainTable()
		{
			AssertEquals(false, DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMain"));
			RunTransformation();
		}

		class UpdateIM_CloseTimeUtcValueForInconsistentTimestampsTransformForTest : UpdateIM_CloseTimeUtcValueForInconsistentTimestampsTransform
		{
			protected override int BatchSize => 2;
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateIM_CloseTimeUtcValueForInconsistentTimestampsTransformForTest();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentMain", $@"
CREATE TABLE dbo.IncidentMain
( 
	[IM_PK] UNIQUEIDENTIFIER NOT NULL,
	[IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
	[IM_IncidentType] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_Status] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_CloseTimeUtc] DATETIME NULL,
	[IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_SystemLastEditTimeUtc] DATETIME NULL,
	[IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);

ALTER TABLE  [IncidentMain]
ADD CONSTRAINT [PK_UX__IM_PK] PRIMARY KEY NONCLUSTERED  ([IM_PK] ASC)
WITH ( IGNORE_DUP_KEY = OFF)
;
");

			var incidentMainTestData = @"INSERT INTO dbo.IncidentMain
(IM_PK, IM_IncidentNumber, IM_IncidentType, IM_Status, IM_CloseTimeUtc, IM_SystemCreateTimeUtc, IM_SystemCreateUser, IM_SystemLastEditTimeUtc, IM_SystemLastEditUser)
VALUES
('0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'INC', 'CLS', NULL, '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('1b85f2e3-4c5f-5b6f-9c7d-8e9f0a1b2c3d', 'CS00001001', 'INC', 'CLS', NULL, '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('2c96f3e4-5d6f-6c7f-ad8e-9f0a1b2c3d4e', 'CS00001002', 'INC', 'OPN', '2023-06-04 12:00:22', '2024-06-03 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('3d07f4e5-6e7f-7d8f-be9f-0a1b2c3d4e5f', 'CS00001003', 'INC', 'OPN', '2023-06-04 12:00:00', '2024-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('2ddf08dd-8c93-4cc3-adc0-d1009a892245', 'CS00001004', 'INC', 'OPN', '2024-06-04 12:00:22', '2024-06-03 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('8356cfdb-21e5-4d1d-85bf-0dc3390afc2b', 'CS00001005', 'INC', 'OPN', '2024-06-04 12:00:22', '2024-06-03 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('0bcdc5dd-604f-4431-80d0-1f8c8b4dff6f', 'CS00001006', 'INC', 'OPN', '2025-06-04 12:00:22', '2024-06-03 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('8f435337-35a3-42bd-a89d-56ee6ffb4f52', 'CS00001007', 'INC', 'OPN', NULL, '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E')
;";

			using (var cmd = Db.Connection.Command(incidentMainTestData))
			{
				cmd.ExecuteNonQuery();
			}

			var stmALogTestData = @"INSERT INTO dbo.StmALog
(SL_PK, SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_GS_NKUser, SL_EventTime, SL_EventTimeUtc, SL_PostedTimeUtc, SL_IsEstimate, SL_IsCancelled)
VALUES
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'APP', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', '2028-06-01 12:00:00', 'N', 'N'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2028-01-01 12:00:00', '2027-06-01 12:00:01', 'Y', 'Y'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2028-02-01 12:00:00', '2027-06-01 12:00:02', 'N', 'Y'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2028-03-01 12:00:00', '2027-06-01 12:00:03', 'Y', 'N'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2027-07-01 12:00:00', '2027-06-01 12:00:04', 'N', 'N'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2025-06-01 12:00:00', '2025-06-01 12:00:05', 'N', 'N'),

(NEWID(), 'IncidentMain', '1b85f2e3-4c5f-5b6f-9c7d-8e9f0a1b2c3d', 'CS00001001', 'APP', 'E', '2020-06-01 12:00:00', NULL, '2024-06-01 12:00:00', 'N', 'N'),
(NEWID(), 'IncidentMain', '1b85f2e3-4c5f-5b6f-9c7d-8e9f0a1b2c3d', 'CS00001001', 'APP', 'E', '2020-06-01 12:00:00', NULL, '2023-06-01 12:00:00', 'N', 'N'),

(NEWID(), 'IncidentMain', '2c96f3e4-5d6f-6c7f-ad8e-9f0a1b2c3d4e', 'CS00001002', 'ICL', 'E', '2020-06-01 12:00:00', '2023-06-01 12:00:00', '2023-06-02 12:00:33', 'N', 'N'),

(NEWID(), 'IncidentMain', '3d07f4e5-6e7f-7d8f-be9f-0a1b2c3d4e5f', 'CS00001003', 'APP', 'E', '2020-06-01 12:00:00', '2022-06-01 12:00:00', '2022-06-01 12:00:00', 'N', 'N'),

(NEWID(), 'IncidentMain', '2ddf08dd-8c93-4cc3-adc0-d1009a892245', 'CS00001004', 'ICL', 'E', '2020-06-01 12:00:00', '2023-06-01 12:00:00', '2023-06-02 12:00:33', 'N', 'N'),

(NEWID(), 'IncidentMain', '8356cfdb-21e5-4d1d-85bf-0dc3390afc2b', 'CS00001005', 'ICL', 'E', '2020-06-01 12:00:00', '2023-06-01 12:00:00', '2023-06-02 12:00:33', 'N', 'N'),

(NEWID(), 'IncidentMain', '0bcdc5dd-604f-4431-80d0-1f8c8b4dff6f', 'CS00001006', 'ICL', 'E', '2020-06-01 12:00:00', '2023-06-01 12:00:00', '2023-06-02 12:00:33', 'N', 'N'),

(NEWID(), 'IncidentMain', '8f435337-35a3-42bd-a89d-56ee6ffb4f52', 'CS00001007', 'APP', 'E', '2020-06-01 12:00:00', '2023-06-01 12:00:00', '2023-06-02 12:00:33', 'N', 'N')
;";

			using (var cmd = Db.Connection.Command(stmALogTestData))
			{
				cmd.ExecuteNonQuery();
			}

			var jobConversationPK1 = Guid.NewGuid();
			var jobConversationPK2 = Guid.NewGuid();
			var jobConversationPK3 = Guid.NewGuid();
			var jobConversationPK4 = Guid.NewGuid();
			var jobConversationPK5 = Guid.NewGuid();
			var jobConversationPK6 = Guid.NewGuid();
			var jobConversationPK7 = Guid.NewGuid();
			var jobConversationPK8 = Guid.NewGuid();

			var jobConversationTestData = $@"INSERT INTO dbo.JobConversation
(JCC_PK, JCC_ParentTableCode, JCC_ParentID, JCC_AutoVersion)
VALUES
('{jobConversationPK1}', 'INC', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 0),
('{jobConversationPK2}', 'INC', '1b85f2e3-4c5f-5b6f-9c7d-8e9f0a1b2c3d', 0),
('{jobConversationPK3}', 'INC', '2c96f3e4-5d6f-6c7f-ad8e-9f0a1b2c3d4e', 0),
('{jobConversationPK4}', 'INC', '3d07f4e5-6e7f-7d8f-be9f-0a1b2c3d4e5f', 0),
('{jobConversationPK5}', 'INC', '2ddf08dd-8c93-4cc3-adc0-d1009a892245', 0),
('{jobConversationPK6}', 'INC', '8356cfdb-21e5-4d1d-85bf-0dc3390afc2b', 0),
('{jobConversationPK7}', 'INC', '0bcdc5dd-604f-4431-80d0-1f8c8b4dff6f', 0),
('{jobConversationPK8}', 'INC', '8f435337-35a3-42bd-a89d-56ee6ffb4f52', 0)
;";

			using (var cmd = Db.Connection.Command(jobConversationTestData))
			{
				cmd.ExecuteNonQuery();
			}

			var jobConversationMessageTestData = $@"INSERT INTO dbo.JobConversationMessage
(JCM_PK, JCM_IsInternal, JCM_Body, JCM_PostedTimeUtc, JCM_JCC_Conversation, JCM_SystemCreateUser, JCM_SystemCreateTimeUtc, JCM_SystemLastEditTimeUtc, JCM_SystemLastEditUser, JCM_Language)
VALUES
(NEWID(), 0, 'Closed As CS Stage Data Fix', '2023-12-08 01:42:52.950', '{jobConversationPK1}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),
(NEWID(), 0, 'aa', '2023-12-08 01:42:52.950', '{jobConversationPK1}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),

(NEWID(), 0, 'Closed As CS Stage Data Fix', '2023-12-08 01:42:52.333', '{jobConversationPK2}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),
(NEWID(), 0, 'aa', '2023-12-08 01:42:52.950', '{jobConversationPK2}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),

(NEWID(), 0, 'aa', '2023-12-08 01:42:52.950', '{jobConversationPK3}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),
(NEWID(), 0, 'aa', '2023-12-08 01:42:52.950', '{jobConversationPK3}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),

(NEWID(), 0, 'aa', '2023-12-08 01:42:52.950', '{jobConversationPK4}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),
(NEWID(), 0, 'Closed As CS Stage Data Fix', '2023-12-08 01:42:52.444', '{jobConversationPK4}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),

(NEWID(), 0, 'Closed As CS Stage Data Fix', '2023-12-08 01:42:52.444', '{jobConversationPK5}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),

(NEWID(), 0, 'Closed As CS Stage Data Fix', '2023-12-08 01:42:52.444', '{jobConversationPK6}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),

(NEWID(), 0, 'Closed As CS Stage Data Fix', '2023-12-08 01:42:52.444', '{jobConversationPK7}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN'),

(NEWID(), 0, 'Closed As CS Stage Data Fix', '2023-12-08 01:42:52.444', '{jobConversationPK8}', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', 'E', 'EN')
;";

			using (var cmd = Db.Connection.Command(jobConversationMessageTestData))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
