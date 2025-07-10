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
	[TestedType(typeof(UpdateIM_CloseTimeUtcValueTransform))]
	public class UpdateIM_ClosedDateUtcValueTransformTest : DataTransformationTestCase
	{
		[SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Baseline")]
		protected override void AssertTransformationResults()
		{
			var incident0Result = Db.Connection.ExecuteScalar<DateTime>(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001000'");
			var incident1Result = Db.Connection.ExecuteScalar<DateTime>(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001001'");
			var incident2Result = Db.Connection.ExecuteScalar(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001002'");
			var incident3Result = Db.Connection.ExecuteScalar<DateTime>(@"SELECT IM_CloseTimeUtc FROM dbo.IncidentMain WHERE IM_IncidentNumber='CS00001003'");

			AssertEquals("The value should only be latest ICL event's SL_EventTimeUtc", DateTime.Parse("2027-07-01 12:00:00"), incident0Result);
			AssertEquals("The value should only be latest ICL event's SL_PostedTimeUtc if the event's SL_EventTimeUtc is empty", DateTime.Parse("2024-06-01 12:00:00"), incident1Result);
			AssertEquals("The IM_CloseTimeUtc should not be updated if it is NULL", DBNull.Value, incident2Result);
			AssertEquals(DateTime.Parse("2022-06-01 12:00:00"), incident3Result);
		}

		[ExpectNoExceptions]
		public void TestTransformationWhenNoIncidentMainTable()
		{
			AssertEquals(false, DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMain"));
			RunTransformation();
		}

		class UpdateIM_CloseTimeUtcValueTransformForTest : UpdateIM_CloseTimeUtcValueTransform
		{
			protected override int BatchSize => 2;
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateIM_CloseTimeUtcValueTransformForTest();
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentMain", $@"
CREATE TABLE dbo.IncidentMain
(
	[IM_PK] UNIQUEIDENTIFIER NOT NULL,
	[IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
	[IM_Status] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_CloseTimeUtc] DATETIME NULL,
	[IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_SystemLastEditTimeUtc] DATETIME NULL,
	[IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);");

			var incidentMainTestData = @"INSERT INTO dbo.IncidentMain
(IM_PK, IM_IncidentNumber, IM_Status, IM_CloseTimeUtc, IM_SystemCreateTimeUtc, IM_SystemCreateUser, IM_SystemLastEditTimeUtc, IM_SystemLastEditUser)
VALUES
('0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'CLS', '2020-06-01 12:00:00', '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('1b85f2e3-4c5f-5b6f-9c7d-8e9f0a1b2c3d', 'CS00001001', 'CLS', '2020-06-02 12:00:00', '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('2c96f3e4-5d6f-6c7f-ad8e-9f0a1b2c3d4e', 'CS00001002', 'CLS', NULL, '2020-06-03 12:00:00', 'E', '2020-06-01 12:00:00', 'E'),
('3d07f4e5-6e7f-7d8f-be9f-0a1b2c3d4e5f', 'CS00001003', 'OPN', '2020-06-04 12:00:00', '2020-06-01 12:00:00', 'E', '2020-06-01 12:00:00', 'E');";

			using (var cmd = Db.Connection.Command(incidentMainTestData))
			{
				cmd.ExecuteNonQuery();
			}

			var stmALogTestData = @"INSERT INTO dbo.StmALog
(SL_PK, SL_Table, SL_Parent, SL_Reference, SL_SE_NKEvent, SL_GS_NKUser, SL_EventTime, SL_EventTimeUtc, SL_PostedTimeUtc, SL_IsEstimate, SL_IsCancelled)
VALUES
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'APP', 'E', '2020-06-01 12:00:00', '2028-06-01 12:00:00', '2028-06-01 12:00:00', 'N', 'N'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2028-01-01 12:00:00', '2027-06-01 12:00:00', 'Y', 'Y'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2028-02-01 12:00:00', '2027-06-01 12:00:00', 'N', 'Y'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2028-03-01 12:00:00', '2027-06-01 12:00:00', 'Y', 'N'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2027-07-01 12:00:00', '2027-06-01 12:00:00', 'N', 'N'),
(NEWID(), 'IncidentMain', '0a74f1e2-3b4e-4a5f-8b6c-7d8e9f0a1b2c', 'CS00001000', 'ICL', 'E', '2020-06-01 12:00:00', '2025-06-01 12:00:00', '2025-06-01 12:00:00', 'N', 'N'),

(NEWID(), 'IncidentMain', '1b85f2e3-4c5f-5b6f-9c7d-8e9f0a1b2c3d', 'CS00001001', 'ICL', 'E', '2020-06-01 12:00:00', NULL, '2024-06-01 12:00:00', 'N', 'N'),
(NEWID(), 'IncidentMain', '1b85f2e3-4c5f-5b6f-9c7d-8e9f0a1b2c3d', 'CS00001001', 'ICL', 'E', '2020-06-01 12:00:00', NULL, '2023-06-01 12:00:00', 'N', 'N'),

(NEWID(), 'IncidentMain', '2c96f3e4-5d6f-6c7f-ad8e-9f0a1b2c3d4e', 'CS00001002', 'ICL', 'E', '2020-06-01 12:00:00', '2023-06-01 12:00:00', '2022-06-01 12:00:00', 'N', 'N'),

(NEWID(), 'IncidentMain', '3d07f4e5-6e7f-7d8f-be9f-0a1b2c3d4e5f', 'CS00001003', 'ICL', 'E', '2020-06-01 12:00:00', '2022-06-01 12:00:00', '2022-06-01 12:00:00', 'N', 'N');";

			using (var cmd = Db.Connection.Command(stmALogTestData))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
