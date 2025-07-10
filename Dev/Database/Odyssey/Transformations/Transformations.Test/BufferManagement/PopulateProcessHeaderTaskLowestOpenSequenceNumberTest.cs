using System;
using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BufferManagement
{
	[TestedType(typeof(PopulateProcessHeaderTaskLowestOpenSequenceNumber))]
	class PopulateProcessHeaderTaskLowestOpenSequenceNumberTest : DataTransformationTestCase
	{
		readonly Guid systemPK = Guid.NewGuid();
		readonly Guid componentPK = Guid.NewGuid();
		readonly Guid job1PK = Guid.NewGuid();
		readonly Guid job2PK = Guid.NewGuid();
		readonly Guid jobHeader1PK = Guid.NewGuid();
		readonly Guid jobHeader2PK = Guid.NewGuid();
		readonly Guid workflow1NoTasksPK = Guid.NewGuid();
		readonly Guid workflow1PK = Guid.NewGuid();
		readonly Guid workflow2PK = Guid.NewGuid();
		readonly Guid workflow3PK = Guid.NewGuid();
		readonly Guid workflow4PK = Guid.NewGuid();
		readonly Guid workflow5PK = Guid.NewGuid();
		readonly Guid workflow2NoTasksPK = Guid.NewGuid();

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var transform = new PopulateProcessHeaderTaskLowestOpenSequenceNumber();
			return transform;
		}

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
('{job1PK}', 'WI00123456', 'Work Item Summary', getutcdate(), 'E', getutcdate(), 'E')

insert into dbo.ProcessHeader 
(FH_PK, FH_CompletionStatement, FH_ParentId, FH_ParentTableCode, FH_FH_ParentHeader, FH_WorkflowType, FH_FC_CurrentComponent, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_SystemCreateUser, FH_SystemLastEditUser, FH_Status)
values
('{jobHeader1PK}', 'Job Header 1', '{job1PK}', 'WKI', null, 'WKI', null, DATEADD(week, -8, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN'),
('{workflow1NoTasksPK}', 'Workflow No Tasks 1', '{job1PK}', 'WKI', '{jobHeader1PK}', 'WKI', '{componentPK}', DATEADD(week, -1, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN'),
('{workflow1PK}', 'Workflow 1', '{job1PK}', 'WKI', '{jobHeader1PK}', 'WKI', '{componentPK}', DATEADD(week, -6, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN'),
('{jobHeader2PK}', 'Job Header 2', '{job2PK}', 'WKI', null, 'WKI', null, DATEADD(week, -4, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN'),
('{workflow2PK}', 'Workflow 2', '{job2PK}', 'WKI', '{jobHeader2PK}', 'WKI', '{componentPK}', DATEADD(week, -2, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN'),
('{workflow3PK}', 'Workflow 3', '{job2PK}', 'WKI', '{jobHeader2PK}', 'WKI', '{componentPK}', DATEADD(week, -2, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN'),
('{workflow4PK}', 'Workflow 4', '{job2PK}', 'WKI', '{jobHeader2PK}', 'WKI', '{componentPK}', DATEADD(week, -2, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN'),
('{workflow5PK}', 'Workflow 5', '{job2PK}', 'WKI', '{jobHeader2PK}', 'WKI', '{componentPK}', DATEADD(week, -2, GETDATE()), GETDATE(), '~BP', '~BP', 'CLS'),
('{workflow2NoTasksPK}', 'Workflow No Tasks 1', '{job1PK}', 'WKI', '{jobHeader2PK}', 'WKI', '{componentPK}', DATEADD(week, -1, GETDATE()), GETDATE(), '~BP', '~BP', 'OPN')
insert into dbo.ProcessTasks
(P9_PK, P9_ParentID, P9_FH_ProcessHeader, P9_Status, P9_Type, P9_ParentTableCode, P9_Sequence, P9_CompletedTimeUtc)
values
(NEWID(), '{job1PK}', '{workflow1PK}', 'CLS', 'UDF', 'Z0', 4, GETDATE()),
(NEWID(), '{job1PK}', '{workflow1PK}', 'CAN', 'UDF', 'Z0', 24, NULL),
(NEWID(), '{job2PK}', '{workflow2PK}', 'CAN', 'UDF', 'Z0', 5, NULL),
(NEWID(), '{job2PK}', '{workflow2PK}', 'SUS', 'MIL', 'Z0', 10, NULL),
(NEWID(), '{job2PK}', '{workflow2PK}', 'WRK', 'UDF', 'P0', 15, NULL),
(NEWID(), '{job2PK}', '{workflow2PK}', 'SUS', 'UDF', 'Z0', 25, NULL),
(NEWID(), '{job2PK}', '{workflow2PK}', 'SUS', 'UDF', 'Z0', 25, NULL),
(NEWID(), '{job2PK}', '{workflow3PK}', 'SUS', 'UDF', 'Z0', 100, GETDATE()),
(NEWID(), '{job2PK}', '{workflow4PK}', 'SUS', 'UDF', 'Z0', 200, GETDATE()),
(NEWID(), '{job2PK}', '{workflow5PK}', 'SUS', 'UDF', 'Z0', 1, GETDATE()),
(NEWID(), NULL, NULL, 'SUS', 'UDF', 'Z0', 7, GETDATE())
");
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertEquals(-1, TestConnection.ExecuteScalar($"select FH_TaskLowestOpenSequenceNumber from dbo.ProcessHeader where FH_PK = '{workflow1PK}'"));
				AssertEquals(25, TestConnection.ExecuteScalar($"select FH_TaskLowestOpenSequenceNumber from dbo.ProcessHeader where FH_PK = '{workflow2PK}'"));
				AssertEquals(100, TestConnection.ExecuteScalar($"select FH_TaskLowestOpenSequenceNumber from dbo.ProcessHeader where FH_PK = '{workflow3PK}'"));
				AssertEquals(200, TestConnection.ExecuteScalar($"select FH_TaskLowestOpenSequenceNumber from dbo.ProcessHeader where FH_PK = '{workflow4PK}'"));
				AssertEquals(-1, TestConnection.ExecuteScalar($"select FH_TaskLowestOpenSequenceNumber from dbo.ProcessHeader where FH_PK = '{workflow5PK}'"));
				AssertEquals(4, TestConnection.ExecuteScalar($"select count(*) from dbo.ProcessHeader where FH_PK in ('{jobHeader1PK}', '{jobHeader2PK}', '{workflow1PK}', '{workflow5PK}') and FH_TaskLowestOpenSequenceNumber = -1"));
			});
		}

		public void TestTransformation_NoData_NoExceptionThrown()
		{
			var transform = GetNewTestTransformationInstance();
			AssertNoExceptionThrown(() => transform.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None));
		}
	}
}
