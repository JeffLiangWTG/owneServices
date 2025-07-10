using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BufferManagement
{
	[TestedType(typeof(ChangeProcessHeaderCompletionStatementToNVarChar512))]
	class ChangeProcessHeaderCompletionStatementToNVarChar512Test : DataTransformationTestCase
	{
		string TemporaryColumnName => $"{ColumnSynchroniser.ColumnRenamePrefix}{ProcessHeaderSchema.FH_CompletionStatement.Name}";
		const string LastProcessedChunkForParentIdName = "ChangeProcessHeaderCompletionStatementToNVarChar512.LastProcessedChunkForPK";
		const string LastProcessedChunkForPKName = "ChangeProcessHeaderCompletionStatementToNVarChar512.LastProcessedChunkForParentId";
		const string TransformationHasRunToCompletion = "ChangeProcessHeaderCompletionStatementToNVarChar512.TransformationRunToCompletion";
		const string SyncTriggerName = "TG_ProcessHeader_KeepCompletionStatementInSync";

		readonly Guid systemPK = Guid.NewGuid();
		readonly Guid componentPK = Guid.NewGuid();
		readonly Guid jobPK = Guid.NewGuid();
		readonly Guid jobHeaderPK = Guid.NewGuid();
		readonly Guid workflow1PK = Guid.NewGuid();
		readonly Guid workflow2PK = Guid.NewGuid();
		readonly Guid workflow3PK = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery($@"
insert into dbo.BMSystem
(FS_PK, FS_Name, FS_Description, FS_SystemCreateTimeUtc, FS_SystemCreateUser, FS_SystemLastEditTimeUtc, FS_SystemLastEditUser)
values ('{systemPK}', 'ASystem', 'A BMS System', getutcdate(), 'E', getutcdate(), 'E')

insert into dbo.BMComponent
(FC_PK, FC_FS_System, FC_Name, FC_Type, FC_SystemCreateTimeUtc, FC_SystemCreateUser, FC_SystemLastEditTimeUtc, FC_SystemLastEditUser)
values ('{componentPK}', '{systemPK}', 'Component1', 'BUC', getutcdate(), 'E', getutcdate(), 'E')

insert into dbo.WorkItem
(WKI_PK, WKI_WorkItemNumber, WKI_Summary, WKI_SystemCreateTimeUtc, WKI_SystemCreateUser, WKI_SystemLastEditTimeUtc, WKI_SystemLastEditUser)
values
('{jobPK}', 'WI00123456', 'Work Item Summary', getutcdate(), 'E', getutcdate(), 'E')

insert into dbo.ProcessHeader 
(FH_PK, FH_CompletionStatement, FH_ParentId, FH_ParentTableCode, FH_FH_ParentHeader, FH_WorkflowType, FH_FC_CurrentComponent, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_SystemCreateUser, FH_SystemLastEditUser)
values
('{jobHeaderPK}', '{string.Concat(Enumerable.Repeat("Job Header 1", 100))}', '{jobPK}', 'WKI', null, 'WKI', null, DATEADD(week, -8, GETDATE()), GETDATE(), '~BP', '~BP'),
('{workflow1PK}', '{string.Concat(Enumerable.Repeat("Workflow 1", 100))}', '{jobPK}', 'WKI', '{jobHeaderPK}', 'WKI', '{componentPK}', DATEADD(week, -6, GETDATE()), GETDATE(), '~BP', '~BP'),
('{workflow2PK}', 'Workflow 2', '{jobPK}', 'WKI', '{jobHeaderPK}', 'WKI', '{componentPK}', DATEADD(week, -6, GETDATE()), GETDATE(), '~BP', '~BP'),
('{workflow3PK}', '{string.Concat(Enumerable.Repeat("Workflow 3", 100))}', null, 'WKI', '{jobHeaderPK}', 'WKI', '{componentPK}', DATEADD(week, -6, GETDATE()), GETDATE(), '~BP', '~BP')");
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(string.Concat(Enumerable.Repeat("Job Header 1", 100)).Substring(0, 512), TestConnection.ExecuteScalar($"select {TemporaryColumnName} from dbo.ProcessHeader where FH_PK = '{jobHeaderPK}'"));
			AssertEquals(string.Concat(Enumerable.Repeat("Workflow 1", 100)).Substring(0, 512), TestConnection.ExecuteScalar($"select {TemporaryColumnName} from dbo.ProcessHeader where FH_PK = '{workflow1PK}'"));
			AssertEquals("Workflow 2", TestConnection.ExecuteScalar($"select {TemporaryColumnName} from dbo.ProcessHeader where FH_PK = '{workflow2PK}'"));
			AssertEquals(string.Concat(Enumerable.Repeat("Workflow 3", 100)).Substring(0, 512), TestConnection.ExecuteScalar($"select {TemporaryColumnName} from dbo.ProcessHeader where FH_PK = '{workflow3PK}'"));

			AssertNull(ExtProperty.Database.Select(Db.Connection, LastProcessedChunkForParentIdName));
			AssertNull(ExtProperty.Database.Select(Db.Connection, LastProcessedChunkForPKName));
			AssertEquals(bool.TrueString, ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion));
		}

		public void TestBatching_WorksProperly()
		{
			PrepareTestData();

			for (var i = 0; i < 25; i++)
			{
				TestConnection.ExecuteNonQuery($@"
insert into dbo.ProcessHeader 
(FH_PK, FH_CompletionStatement, FH_ParentId, FH_ParentTableCode, FH_FH_ParentHeader, FH_WorkflowType, FH_FC_CurrentComponent, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_SystemCreateUser, FH_SystemLastEditUser)
values
(NEWID(), 'Tralala ''1''', '{jobPK}', 'WKI', '{jobHeaderPK}', 'WKI', '{componentPK}', GETDATE(), GETDATE(), '~BP', '~BP'),
(NEWID(), 'Tralala ''1''', null, 'WKI', '{jobHeaderPK}', 'WKI', '{componentPK}', GETDATE(), GETDATE(), '~BP', '~BP')");
			}

			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("Batching is not working", 50, TestConnection.ExecuteScalar($"select count({TemporaryColumnName}) FROM dbo.ProcessHeader where {TemporaryColumnName} = 'Tralala ''1'''"));
		}

		public void TestTrigger_WorkProperly()
		{
			PrepareTestData();
			GetNewTestTransformationInstance().Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);

			Assert("Trigger creation is not working", DbObjectCreator.TriggerExists(TestConnection, ProcessHeaderSchema.Constants.TableName, SyncTriggerName));

			TestConnection.ExecuteNonQuery(@$"
update dbo.ProcessHeader
set FH_CompletionStatement = 'I have been changed in the middle of transformation',
    FH_SystemLastEditTimeUtc = GETDATE(),
    FH_SystemLastEditUser = '~BP'
where FH_PK = '{workflow2PK}'");

			var newWorkflowPK = Guid.NewGuid();
			TestConnection.ExecuteNonQuery(@$"
insert into dbo.ProcessHeader
(FH_PK, FH_CompletionStatement, FH_ParentId, FH_ParentTableCode, FH_FH_ParentHeader, FH_WorkflowType, FH_FC_CurrentComponent, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_SystemCreateUser, FH_SystemLastEditUser)
values
('{newWorkflowPK}', 'I have been inserted in the middle of transformation', '{jobPK}', 'WKI', '{jobHeaderPK}', 'WKI', '{componentPK}', GETDATE(), GETDATE(), '~BP', '~BP')");

			AssertEquals("I have been changed in the middle of transformation", TestConnection.ExecuteScalar($"select {TemporaryColumnName} from dbo.ProcessHeader where FH_PK = '{workflow2PK}'"));
			AssertEquals("I have been inserted in the middle of transformation", TestConnection.ExecuteScalar($"select {TemporaryColumnName} from dbo.ProcessHeader where FH_PK = '{newWorkflowPK}'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ChangeProcessHeaderCompletionStatementToNVarChar512(10);

		protected override void SetUp()
		{
			base.SetUp();
			//CREATE VIEW AView AS SELECT CAST(title AS char(50)) FROM titles

			TestConnection.ExecuteNonQuery(@"
alter table dbo.ProcessHeader drop constraint DF_ProcessHeader_FH_CompletionStatement;
drop function dbo.Report_ContainmentBarrierOutcomes;
drop view dbo.ViewProcessHeader;
drop view dbo.ViewClientProcessHeader");
			TestConnection.ExecuteNonQuery("alter table dbo.ProcessHeader alter column FH_CompletionStatement NVARCHAR(MAX)");
		}
	}
}
