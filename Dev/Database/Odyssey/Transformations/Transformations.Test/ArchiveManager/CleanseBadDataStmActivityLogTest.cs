using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(CleanseBadDataStmActivityLog))]
	internal class CleanseBadDataStmActivityLogTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var invalidRows = new DataTable();
			var validRows = new DataTable();
			var sqlGetInvalidRows = @"
				SELECT * FROM dbo.StmActivityLog WHERE
				S7_OpenDateTimeUTC is NULL
				AND S7_CloseDateTimeUtc is NULL
				AND S7_GS_NKUser = ''
				AND S7_FormCaption = ''
				AND S7_ControllerId NOT LIKE 'd0d2148a-6188-47b4-8d59-70e591518437%'
				AND S7_ParentTableCode = ''
				";

			var sqlGetValidRows = @"
				SELECT * FROM dbo.StmActivityLog WHERE
				S7_OpenDateTimeUTC is NOT NULL
				OR S7_CloseDateTimeUtc is NOT NULL
				OR S7_ControllerId LIKE 'd0d2148a-6188-47b4-8d59-70e591518437%'
				OR NOT S7_GS_NKUser = ''
				OR NOT S7_FormCaption = ''
				OR NOT S7_ParentTableCode = ''
				";

			using (var cmd = Db.Connection.Command(sqlGetInvalidRows))
			{
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(invalidRows);
				}
			}

			using (var cmd = Db.Connection.Command(sqlGetValidRows))
			{
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(validRows);
				}
			}

			AssertEquals("All invalid rows should have been successfully purged", 0, invalidRows.Rows.Count);
			AssertEquals("All valid rows should be unaffected by transformation", 13, validRows.Rows.Count);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new CleanseBadDataStmActivityLog();

		protected override void PrepareTestData()
		{
			var sqlText = @"
				ALTER TABLE [dbo].[StmActivityLog] DROP CONSTRAINT [Constraint_S7_OpenDateTimeUtc_NoCheck]
				ALTER TABLE [dbo].[StmActivityLog] DROP CONSTRAINT [Constraint_S7_RowNotEmpty_NoCheck]

				DELETE FROM dbo.StmActivityLog

				INSERT INTO dbo.StmActivityLog(S7_PK) VALUES(NEWID())

				INSERT INTO dbo.StmActivityLog(S7_PK, S7_ControllerID) VALUES(NEWID(), 'd0d2148a-6188-47b4-8d59-70e591518437')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_OpenDateTimeUtc) VALUES(NEWID(), '2016-09-13 05:37:00')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_CloseDateTimeUtc) VALUES(NEWID(), '2016-09-13 05:37:00')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser) VALUES(NEWID(), 'CW1')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc, S7_FormCaption) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00', 'COR')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc, S7_ControllerId) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00', NEWID())
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc, S7_ParentTableCode) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00', 'GB')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc, S7_FormCaption, S7_ControllerId) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00', 'COR', NEWID())
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc, S7_FormCaption, S7_ParentTableCode) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00', 'COR', 'GB')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc, S7_ControllerId, S7_ParentTableCode) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00', 'COR', 'GB')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_GS_NKUser, S7_OpenDateTimeUtc, S7_CloseDateTimeUtc, S7_FormCaption, S7_ControllerId, S7_ParentTableCode) VALUES(NEWID(), 'CW1', '2016-09-13 05:37:00', '2016-09-13 05:37:00', 'COR', NEWID(), 'GB')
				INSERT INTO dbo.StmActivityLog(S7_PK, S7_ControllerId) VALUES(NEWID(), 'd0d2148a-6188-47b4-8d59-70e591518437')
";

			_ = TestConnection.ExecuteNonQuery(sqlText);

			var invalidRows = new DataTable();
			var validRows = new DataTable();
			var sqlGetInvalidRows = @"
				SELECT * FROM dbo.StmActivityLog WHERE
				S7_OpenDateTimeUTC is NULL
				AND S7_CloseDateTimeUtc is NULL
				AND S7_GS_NKUser = ''
				AND S7_FormCaption = ''
				AND S7_ControllerId NOT LIKE 'd0d2148a-6188-47b4-8d59-70e591518437%'
				AND S7_ParentTableCode = ''
				";

			var sqlGetValidRows = @"
				SELECT * FROM dbo.StmActivityLog WHERE
				S7_OpenDateTimeUTC is NOT NULL
				OR S7_CloseDateTimeUtc is NOT NULL
				OR S7_ControllerId LIKE 'd0d2148a-6188-47b4-8d59-70e591518437%'
				OR NOT S7_GS_NKUser = ''
				OR NOT S7_FormCaption = ''
				OR NOT S7_ParentTableCode = ''
				";

			using (var cmd = Db.Connection.Command(sqlGetInvalidRows))
			{
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(invalidRows);
				}
			}

			using (var cmd = Db.Connection.Command(sqlGetValidRows))
			{
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(validRows);
				}
			}

			AssertEquals("Precondition: There should be 1 'bad' created (targets of transformation)", 1, invalidRows.Rows.Count);
			AssertEquals("Precondition: There should be 12 'valid' rows created", 13, validRows.Rows.Count);
		}
	}
}
