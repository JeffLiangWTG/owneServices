using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(PurgeProcessHeaderOrphans))]
	[UseSnapshotProtection]
	internal class PurgeProcessHeaderOrphansTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeProcessHeaderOrphans();

		protected override void PrepareTestData()
		{
			var insertSql = @"
				DECLARE @TestDate datetime = '2000-01-01 00:00:00.000';
				DECLARE @PreviousTestDate datetime = DATEADD(DAY, -1, @TestDate);

				CREATE TABLE [dbo].[ClientGlbStaff] ([GS_PK] [uniqueidentifier] NOT NULL,[GS_LoginName] [nvarchar](104) NOT NULL, [GS_Code] [varchar](3) NOT NULL, [GS_SystemCreateTimeUtc] [datetime] NULL, [GS_SystemCreateUser] [varchar](3) NOT NULL, [GS_SystemLastEditTimeUtc] [datetime] NULL,[GS_SystemLastEditUser] [varchar](3) NOT NULL)
				ALTER TABLE [dbo].[ClientGlbStaff] SET (LOCK_ESCALATION = DISABLE)
				--Delete data before 2000-01-01 and insert test data before this time point
				DELETE FROM dbo.BMNCNAttachment WHERE BNA_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.BMNCNShape WHERE BNS_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.ProcessHeaderLink WHERE FP_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.ProcessTaskIterationLink WHERE P9I_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.ProcessTasks WHERE P9_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.ProcessHeader WHERE FH_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.ProcessTaskTemplate WHERE P0_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.GlbStaff WHERE GS_SystemCreateTimeUtc <= @TestDate;
				DELETE FROM dbo.ClientGlbStaff WHERE GS_SystemCreateTimeUtc <= @TestDate;  

				--Orphan records that should be erased in processHeader
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), NewID(), 'JS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP');
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), NewID(), 'JC', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CNT');
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), NewID(), 'JE', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'BRK');
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), NewID(), 'JK', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CON');

				--Orphan records that should be erased in processHeader(FH_ParentId and FH_P0_Template is NULL)
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CON');

				--Orphan records that should be erased in processTasks
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'JD', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'CS', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'DL', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'JS', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Orphan records that should be erased in processHeader and has associated records in tables
				DECLARE @PK1 uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(@PK1, NewID(), 'JS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP');
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_FH_ProcessHeader, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), NewID(), 'JS', @PK1, @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				DECLARE @PK2 uniqueidentifier = NEWID();
				DECLARE @PK3 uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(@PK2, NewID(), 'JS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP');
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(@PK3, NewID(), 'JD', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO dbo.ProcessTaskIterationLink(P9I_PK, P9I_P9_ContainmentBarrierTask, P9I_FH_IterationWorkflow, P9I_LinkType, P9I_Sequence, P9I_SystemCreateTimeUtc, P9I_SystemCreateUser, P9I_SystemLastEditTimeUtc, P9I_SystemLastEditUser, P9I_Outcome)
				VALUES(NewID(), @PK3, @PK2, 'TES', 1, @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 'DFR');

				--Positive test case(valid record with FH_P0_Template that should not be erased)
				DECLARE @PK6 uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessTaskTemplate(P0_PK, P0_ProcessType, P0_Name, P0_SystemCreateTimeUtc, P0_SystemCreateUser, P0_SystemLastEditTimeUtc, P0_SystemLastEditUser)
				VALUES(@PK6, 'SHP', 'SHP, AIR, IMP, global, system, test', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_P0_Template, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), @PK6, @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CON');

				--Test cases under multi-layer foreign key constraints
				DECLARE @PK4 uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(@PK4, NewID(), 'JS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP');
				DECLARE @PK5 uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(@PK5, NewID(), 'JS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP');

				DECLARE @PK7 uniqueidentifier = NEWID();
				INSERT INTO dbo.BMNCNShape(BNS_PK, BNS_ShapeType, BNS_Status, BNS_SystemCreateTimeUtc, BNS_SystemCreateUser, BNS_SystemLastEditTimeUtc, BNS_SystemLastEditUser)
				VALUES(@PK7, 'SYS', 'CAN', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S');
				DECLARE @PK8 uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeaderLink(FP_PK, FP_LinkType, FP_FH_HeaderFrom, FP_FH_HeaderTo, FP_TimeDelayFactor, FP_TimeDelayMinutes, FP_SystemCreateTimeUtc, FP_SystemCreateUser, FP_SystemLastEditTimeUtc, FP_SystemLastEditUser)
				VALUES(@PK8, 'DEP', @PK4, @PK5, 1, 10, @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S');
				INSERT INTO dbo.BMNCNAttachment(BNA_PK, BNA_BNS_ToShape, BNA_BNS_Owner, BNA_FP_ProcessHeaderLink, BNA_SystemCreateTimeUtc, BNA_SystemCreateUser, BNA_SystemLastEditTimeUtc, BNA_SystemLastEditUser)
				VALUES(NewID(), @PK7, @PK7, @PK8, @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S');

				--Test cases under self associated foreign key constraints(should be erased)
				DECLARE @PK9 uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(@PK9, NewID(), 'JS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP');
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_FH_ParentHeader, FH_P0_Template, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), @PK9, @PK6, @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CON');

				--Test Cases for tables which have a same PK Column and they are not orphan for ProcessHeader
				DECLARE @myid uniqueidentifier  
				SET @myid = NEWID()
				INSERT INTO
					GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES
					(@myid, 'user001_', 'UUZ', @PreviousTestDate, 'UUZ', GETUTCDATE(), 'UUZ');
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), @myid, 'GS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CON');

				DECLARE @myidClient uniqueidentifier  
				SET @myidClient = NEWID()
				INSERT INTO
					ClientGlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES
					(@myidClient, 'user002_', 'UUZ',@PreviousTestDate, 'UUZ', GETUTCDATE(), 'UUZ');
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), @myidClient, 'GS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CON');

				--Test Cases for tables which have a same PK Column and they are orphan for ProcessHeader
				DECLARE @myidOrphan uniqueidentifier  
				SET @myidOrphan = NEWID()
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(NewID(), @myidOrphan, 'GS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'CON');

				--Test Cases for tables which have a same PK Column and they are not orphan For ProcessTasks
				DECLARE @myidForProcessTasks uniqueidentifier  
				SET @myidForProcessTasks = NEWID()
				INSERT INTO
					GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES
					(@myidForProcessTasks, 'user003_', 'UUX', @PreviousTestDate, 'UUX', GETUTCDATE(), 'UUX');
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), @myidForProcessTasks, 'GS', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');


				DECLARE @myidClientProcessTasks uniqueidentifier  
				SET @myidClientProcessTasks = NEWID()
				INSERT INTO
					ClientGlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
				VALUES
					(@myidClientProcessTasks, 'user004_', 'UUX',@PreviousTestDate, 'UUX', GETUTCDATE(), 'UUX');
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), @myidClientProcessTasks, 'GS', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Test Cases for tables which have a same PK Column and they are orphan For ProcessTasks
				DECLARE @myidOrphanForProcessTasks uniqueidentifier  
				SET @myidOrphanForProcessTasks = NEWID()
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(NewID(), @myidOrphanForProcessTasks, 'GS', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');

				--Test orphans can be deleted when there is a self-reference with further children
				--Parent ProcessHeader (that is an orphan)
				DECLARE @ParentProcessHeaderPK uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType) 
				VALUES(@ParentProcessHeaderPK, NewID(), 'JS', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP');
				
				--Child ProcessHeader (^^ of previously created ProcessHeader) that is *not* an orphan
				DECLARE @myidForParentDummyBizo uniqueidentifier = NEWID();
				INSERT INTO DummyBizo (Z0_PK) VALUES (@myidForParentDummyBizo);
				DECLARE @ChildProcessHeaderPK uniqueidentifier = NEWID();
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType, FH_FH_ParentHeader) 
				VALUES(@ChildProcessHeaderPK, @myidForParentDummyBizo, 'Z0', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP', @ParentProcessHeaderPK);
				
				--Child of Child ProcessHeader
				INSERT INTO dbo.ProcessHeader(FH_PK, FH_ParentId, FH_ParentTableCode, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_IsActive, FH_CompletionStatement, FH_WorkflowType, FH_FH_ParentHeader) 
				VALUES(NEWID(), @myidForParentDummyBizo, 'Z0', @PreviousTestDate, 'C.S', @PreviousTestDate, 'C.S', 1, 'Job Workflow', 'SHP', @ChildProcessHeaderPK);

				--Child of ProcessTasks
				DECLARE @myidForWhsCycleCountLocationParent uniqueidentifier;
				SET @myidForWhsCycleCountLocationParent = NEWID();
				INSERT INTO dbo.ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
				VALUES(@myidForWhsCycleCountLocationParent, NewID(), 'GS', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
				
				ALTER TABLE WhsCycleCountLocation DROP CONSTRAINT WhsCycleCountLocation_WCL_WL_Location_FK2_WhsLocation_RRR_120N;
				INSERT INTO dbo.WhsCycleCountLocation(WCL_PK, WCL_P9_Task, WCL_WL_Location, WCL_JobID, WCL_SystemCreateTimeUtc, WCL_SystemCreateUser, WCL_SystemLastEditTimeUtc, WCL_SystemLastEditUser)
				VALUES(NewID(), @myidForWhsCycleCountLocationParent, NewID(), 'WC00000001', @PreviousTestDate, '~BP', @PreviousTestDate, '~BP');
			";

			TestConnection.ExecuteNonQuery(insertSql);
		}

		protected override void AssertTransformationResults()
		{
			var sqlGetHeaderCount = "SELECT COUNT(*) FROM dbo.ProcessHeader WHERE FH_SystemCreateTimeUtc <= '2000-01-01 00:00:00.000';";
			var sqlGetTasksCount = "SELECT COUNT(*) FROM dbo.ProcessTasks WHERE P9_SystemCreateTimeUtc <= '2000-01-01 00:00:00.000';";
			var sqlGetGlbStaffCount = "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'user001_';";
			var sqlGetClientGlbStaffCount = "SELECT COUNT(*) FROM dbo.ClientGlbStaff WHERE GS_LoginName =  'user002_';";
			var sqlGetGlbStaffCountForProcessTask = "SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_LoginName = 'user003_';";
			var sqlGetClientGlbStaffCountForProcessTask = "SELECT COUNT(*) FROM dbo.ClientGlbStaff WHERE GS_LoginName =  'user004_';";
			var headerCount = 0;
			using (var reader = Db.Connection.Command(sqlGetHeaderCount).ExecuteReader())
			{
				if (reader.Read())
				{
					headerCount = reader.GetInt32(0);
				}
			}
			AssertEquals("There exists one valid record.", 3, headerCount);

			var tasksCount = 0;
			using (var reader = Db.Connection.Command(sqlGetTasksCount).ExecuteReader())
			{
				if (reader.Read())
				{
					tasksCount = reader.GetInt32(0);
				}
			}
			AssertEquals(2, tasksCount);

			var glbStaffCount = 0;
			using (var reader = Db.Connection.Command(sqlGetGlbStaffCount).ExecuteReader())
			{
				if (reader.Read())
				{
					glbStaffCount = reader.GetInt32(0);
				}
			}
			AssertEquals(1, glbStaffCount);

			var glbClientStaffCount = 0;
			using (var reader = Db.Connection.Command(sqlGetClientGlbStaffCount).ExecuteReader())
			{
				if (reader.Read())
				{
					glbClientStaffCount = reader.GetInt32(0);
				}
			}
			AssertEquals(1, glbClientStaffCount);

			var glbStaffCountForProcessTask = 0;
			using (var reader = Db.Connection.Command(sqlGetGlbStaffCountForProcessTask).ExecuteReader())
			{
				if (reader.Read())
				{
					glbStaffCountForProcessTask = reader.GetInt32(0);
				}
			}
			AssertEquals(1, glbStaffCount);

			var glbClientStaffCountForProcessTask = 0;
			using (var reader = Db.Connection.Command(sqlGetClientGlbStaffCountForProcessTask).ExecuteReader())
			{
				if (reader.Read())
				{
					glbClientStaffCountForProcessTask = reader.GetInt32(0);
				}
			}
			AssertEquals(1, glbClientStaffCount);
		}
	}
}
