using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test
{
	[TestedType(typeof(PurgeBMControlCustomisationFromStmALog))]

	class PurgeBMControlCustomisationFromStmALogTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeBMControlCustomisationFromStmALog();

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
INSERT INTO BMControlCustomisation (FM_PK, FM_Name, FM_ControlType, FM_IsSystemWide, FM_JobType, FM_SystemCreateTimeUtc, FM_SystemCreateUser, FM_SystemLastEditTimeUtc, FM_SystemLastEditUser) VALUES
('{Guid1}', 'Layout1', '1', 1, 'JS', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Guid2}', 'Layout2', '2', 1, 'JS', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO StmALog (SL_PK,SL_Table,SL_Parent,SL_IsEstimate,SL_IsCancelled,SL_Reference,SL_PostedTimeUtc,SL_EventTime,SL_GS_NKUser,SL_SE_NKEvent,SL_GB_NKBranch,SL_GE_NKDepartment,SL_FireWorkflow,SL_DataSource,SL_EventTimeUtc) VALUES
('{Guid.NewGuid()}', 'BMControlCustomisation',	'{Guid1}', 'N', 'N', '', '{time1}', '{time1}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'BMControlCustomisation',	'{Guid2}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'GlbStaff',				'{Guid3}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
('{Guid.NewGuid()}', 'ProcessHeader',			'{Guid4}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null)
";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Table = 'BMControlCustomisation'"));

			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid1}'"));
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid2}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid3}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid4}'"));
		}
	}
}
