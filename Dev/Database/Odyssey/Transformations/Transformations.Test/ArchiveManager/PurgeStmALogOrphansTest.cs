using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(PurgeStmALogOrphans))]
	[UseSnapshotProtection]
	internal class PurgeStmALogOrphansTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var getPositiveTestCase = "SELECT COUNT(*) FROM dbo.StmALog WHERE SL_PostedTimeUtc <= '2000-01-01 00:00:00.000'";
			var positiveTestCaseCount = 0;

			using (var command = Db.Connection.Command(getPositiveTestCase))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						positiveTestCaseCount = reader.GetInt32(0);
					}
				}
			}

			var getOrphanCreatedAfterMaxProcessingDate = "SELECT COUNT(*) FROM dbo.StmALog WHERE SL_PostedTimeUtc = '2050-01-01 00:00:00.000' AND SL_TABLE='StmActivityLog' ";
			var negativeTestCaseCount = 0;

			using (var command = Db.Connection.Command(getOrphanCreatedAfterMaxProcessingDate))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						negativeTestCaseCount = reader.GetInt32(0);
					}
				}
			}

			AssertEquals(6, positiveTestCaseCount);
			AssertEquals(1, negativeTestCaseCount);
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Table.Select(Db.Connection, "dbo", nameof(PurgeStmALogOrphans), "LastProcessedDate"));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Table.Select(Db.Connection, "dbo", nameof(PurgeStmALogOrphans), "HrsToChunkBy"));
			AssertEquals("Extended Properties should be cleared after execution.", null, ExtProperty.Table.Select(Db.Connection, "dbo", nameof(PurgeStmALogOrphans), "MaxProcessingDate"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeStmALogOrphans();

		protected override void PrepareTestData()
		{
			var dropExistingView = "DROP VIEW IF EXISTS vw_randomrecord;";
			var prepareViewForTesting = "CREATE VIEW vw_randomrecord AS SELECT SL_PK FROM dbo.StmALog WHERE SL_PK = '111D1661-5716-4DA4-9537-F63DE8ACB290';";

			var insertTestData = @"
				DELETE FROM dbo.StmALog WHERE SL_PostedTimeUtc <= '2000-01-01 00:00:00.000'

				DECLARE @MinDate datetime = '2000-01-01 00:00:00.000';
					
				--Orphan records that should be erased (with VALID parent table codes)
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -1, @MinDate), NEWID(), 'GlbStaff', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -1, @MinDate), NEWID(), 'JobHeader', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -1, @MinDate), NEWID(), 'JobShipment', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -1, @MinDate), NEWID(), 'RatingHeader', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -1, @MinDate), NEWID(), 'HVLVConsignmentHeader', GETUTCDATE())

				--Orphan records that should be erased (with INVALID parent table codes)
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -2, @MinDate), NEWID(), 'I', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -2, @MinDate), NEWID(), 'dont', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -2, @MinDate), NEWID(), 'like', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -2, @MinDate), NEWID(), 'this', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -2, @MinDate), NEWID(), 'transformation', GETUTCDATE())

				--Orphan recordsthat are DEL events and should be erased
				DECLARE @PK1 uniqueidentifier = NEWID()
				DECLARE @PK2 uniqueidentifier = NEWID()
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime, SL_SE_NKEvent) VALUES(NEWID(), DATEADD(DAY, -3, @MinDate), @PK1, 'GlbStaff', GETUTCDATE(), 'DEL')
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime, SL_SE_NKEvent) VALUES(NEWID(), DATEADD(DAY, -3, @MinDate), @PK2, 'GlbStaff', GETUTCDATE(), 'DEL')

				--Relatives of the above DEL events that should also be erased 
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NEWID(), DATEADD(DAY, -3, @MinDate), @PK1, 'GlbStaff', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NEWID(), DATEADD(DAY, -3, @MinDate), @PK1, 'GlbStaff', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NEWID(), DATEADD(DAY, -3, @MinDate), @PK1, 'GlbStaff', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NEWID(), DATEADD(DAY, -3, @MinDate), @PK2, 'GlbStaff', GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NEWID(), DATEADD(DAY, -3, @MinDate), @PK2, 'GlbStaff', GETUTCDATE())

				--Orphan that exists after the max processing date (negative test case)
				DECLARE @PK3 uniqueidentifier = NEWID()
				INSERT INTO StmActivityLog (S7_PK, S7_OpenDateTimeUtc, S7_FormCaption) VALUES(@PK3, @MinDate, 'Caption')
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), '2050-01-01 00:00:00.000', @PK3, 'StmActivityLog', GETUTCDATE())

				--Positive test case (orphan record should NOT be erased because parent table exists in exception list)
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -1, @MinDate), NEWID(), 'DtbBookingQueue', GETUTCDATE())

				--Positive test case (view record should be skipped)
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -1, @MinDate), NEWID(), 'vw_randomrecord', GETUTCDATE())

				--Positive test case (valid parent object, should not be erased)
				DECLARE @S7PK uniqueidentifier = NEWID()
				INSERT INTO StmActivityLog (S7_PK, S7_OpenDateTimeUtc, S7_FormCaption) VALUES(@S7PK, @MinDate, 'Caption')
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -4, @MinDate), @S7PK, 'StmActivityLog', GETUTCDATE())

				--Positive test case (ignore StmALog parent and check that it correctly picks up info (make sure string equals is not case sensitive) )
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -4, @MinDate), NEWID(), 'stmALog', GETUTCDATE())

				--Positive test case (correctly identify child of duplicate table names across schemas and purges accordingly)
				DECLARE @PK5 uniqueidentifier = NEWID()
				
				DROP TABLE IF EXISTS dbo.FakeTable;			
				DROP TABLE IF EXISTS hrm.FakeTable;			
				CREATE TABLE dbo.FakeTable (
					FT_PK uniqueidentifier CONSTRAINT PK_UX__FT_PK PRIMARY KEY NONCLUSTERED
				);
				CREATE TABLE hrm.FakeTable (
					FT_PK uniqueidentifier CONSTRAINT PK_UX__FT_PK PRIMARY KEY NONCLUSTERED
				);
				
				INSERT INTO dbo.FakeTable(FT_PK) VALUES (@PK5)
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -4, @MinDate), @PK5, 'FakeTable', GETUTCDATE())

				-- Orphan of fake new table (does not exist in either dbo or hrm schema therefore should be purged)
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -4, @MinDate), NEWID(), 'FakeTable', GETUTCDATE())

				-- Orphan of RefExchangeRate (test manual workaround is successful)
				DECLARE @PK6 uniqueidentifier = NEWID()
				ALTER TABLE [dbo].[ZZRefExchangeRate] DROP CONSTRAINT [ZZRefExchangeRate_RE_GC_FK2_GlbCompany_RRR_120N]
				ALTER TABLE [dbo].[ZZRefExchangeRate] DROP CONSTRAINT [ZZRefExchangeRate_RE_OH_Client_FK2_OrgHeader_RRR_120N]
				ALTER TABLE [dbo].[ZZRefExchangeRate] DROP CONSTRAINT [Constraint_ClientBuySellOnly]
				ALTER TABLE [dbo].[ZZRefExchangeRate] DROP CONSTRAINT [Constraint_RE_ExRateType]
				DROP TRIGGER [dbo].[TG_ZZRefExchangeRate_AuditDetailsAreNotMissing_Insert]

				-- RefExchangeRate orphan (should be purged)
				INSERT INTO [ZZRefExchangeRate](RE_PK, RE_StartDate, RE_GC, RE_SystemCreateTimeUtc, RE_SystemCreateUser, RE_SystemLastEditTimeUtc, RE_SystemLastEditUser) VALUES (NEWID(), GETUTCDATE(), NEWID(), GETUTCDATE(), 'KGG', GETUTCDATE(), 'KGG')

				-- RefExchangeRate child (should NOT be purged)
				INSERT INTO [ZZRefExchangeRate](RE_PK, RE_StartDate, RE_GC) VALUES (@PK6, GETUTCDATE(), NEWID())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(DAY, -4, @MinDate), @PK6, 'RefExchangeRate', GETUTCDATE())

				DECLARE @MinDate2 datetime = DATEADD(DAY, -5, @MinDate);
				--Extra rows to manually test chunking by hour (all are orphans and should be purged)
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -9, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -8, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -7, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -6, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -5, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -4, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -3, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -2, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				INSERT INTO dbo.StmALog(SL_PK, SL_PostedTimeUtc, SL_Parent, SL_Table, SL_EventTime) VALUES(NewID(), DATEADD(HOUR, -1, @MinDate2), NEWID(),	'StmActivityLog' , GETUTCDATE())
				";

			_ = TestConnection.ExecuteNonQuery(dropExistingView);
			_ = TestConnection.ExecuteNonQuery(prepareViewForTesting);
			_ = TestConnection.ExecuteNonQuery(insertTestData);

			var sql = "SELECT COUNT(*) FROM dbo.StmALog";
			DataTable addedrows = new DataTable();

			using (var command = Db.Connection.Command(sql))
			{
				using (var adapter = command.NewDataAdapter())
				{
					adapter.Fill(addedrows);
				}
			}
		}
	}
}
