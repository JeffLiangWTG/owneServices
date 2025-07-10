using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test
{
	[TestedType(typeof(PurgeBMSystemFromStmALog))]

	class PurgeBMSystemFromStmALogTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeBMSystemFromStmALog();

		Guid Guid1 = Guid.NewGuid();
		Guid Guid2 = Guid.NewGuid();
		Guid Guid3 = Guid.NewGuid();
		Guid Guid4 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var utcNow = DateTime.UtcNow;
			var time1 = utcNow.AddMonths(-3).ToSqlFormat();
			var time2 = utcNow.ToSqlFormat();

			var sql = $@"
INSERT INTO BMSystem (FS_PK, FS_Name, FS_Description, FS_IsLive, FS_SystemCreateTimeUtc, FS_SystemCreateUser, FS_SystemLastEditTimeUtc, FS_SystemLastEditUser) VALUES
('{Guid1}', 'System1', 'Description', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Guid2}', 'System2', 'Description', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO StmALog (SL_PK,SL_Table,SL_Parent,SL_IsEstimate,SL_IsCancelled,SL_Reference,SL_PostedTimeUtc,SL_EventTime,SL_GS_NKUser,SL_SE_NKEvent,SL_GB_NKBranch,SL_GE_NKDepartment,SL_FireWorkflow,SL_DataSource,SL_EventTimeUtc) VALUES
('{Guid.NewGuid()}', 'BMSystem',		'{Guid1}', 'N', 'N', '', '{time1}', '{time1}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'BMSystem',		'{Guid2}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbStaff',		'{Guid3}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'ProcessHeader',	'{Guid4}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null)
";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Table = 'BMSystem'"));

			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid1}'"));
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid2}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid3}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid4}'"));
		}
	}
}
