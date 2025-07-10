using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test
{
	[TestedType(typeof(PurgeBMComponentFromStmALog))]

	class PurgeBMComponentFromStmALogTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeBMComponentFromStmALog();

		Guid Guid1 = Guid.NewGuid();
		Guid Guid2 = Guid.NewGuid();
		Guid Guid3 = Guid.NewGuid();
		Guid Guid4 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var utcNow = DateTime.UtcNow;
			var time1 = utcNow.AddMonths(-3).ToSqlFormat();
			var time2 = utcNow.ToSqlFormat();

			var systemPK = Guid.NewGuid();

			var sql = $@"
INSERT INTO BMSystem
(FS_PK, FS_Name, FS_Description, FS_SystemCreateTimeUtc, FS_SystemCreateUser, FS_SystemLastEditTimeUtc, FS_SystemLastEditUser)
VALUES
('{systemPK}', 'System', 'A BMS System', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO BMComponent (FC_PK, FC_FS_System, FC_Name, FC_Type, FC_BufferTimespanInMinutes, FC_IsActive, FC_OffsetInMinutes, FC_DisplaySequence, FC_BufferLoadLimitPercent, FC_NonCCRTemporaryOverloadLimitMultiplier, FC_BufferTimeCapacityConstraintThresholdMultiple, FC_SystemCreateTimeUtc, FC_SystemCreateUser, FC_SystemLastEditTimeUtc, FC_SystemLastEditUser) VALUES
('{Guid1}', '{systemPK}', 'Component1', 'BUC', 0, 1, 0, 1, 0, 1.0, 1.0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Guid2}', '{systemPK}', 'Component2', 'BUC', 0, 1, 0, 1, 0, 1.0, 1.0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO StmALog (SL_PK,SL_Table,SL_Parent,SL_IsEstimate,SL_IsCancelled,SL_Reference,SL_PostedTimeUtc,SL_EventTime,SL_GS_NKUser,SL_SE_NKEvent,SL_GB_NKBranch,SL_GE_NKDepartment,SL_FireWorkflow,SL_DataSource,SL_EventTimeUtc) VALUES
('{Guid.NewGuid()}', 'BMComponent',			'{Guid1}', 'N', 'N', '', '{time1}', '{time1}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'BMComponent',			'{Guid2}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbStaff',			'{Guid3}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'ProcessHeader',		'{Guid4}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null)
";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Table = 'BMComponent'"));

			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid1}'"));
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid2}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid3}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid4}'"));
		}
	}
}
