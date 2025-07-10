using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test
{
	[TestedType(typeof(PurgeRedundantStorageMainLogsFromStmALog))]

	class DeleteRedundantStorageMainLogsFromStmALogTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeRedundantStorageMainLogsFromStmALog();

		Guid Guid1 = Guid.NewGuid();
		Guid Guid2 = Guid.NewGuid();
		Guid Guid3 = Guid.NewGuid();
		Guid Guid4 = Guid.NewGuid();
		Guid Guid5 = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			var utcNow = DateTime.UtcNow;
			var time1 = utcNow.AddMonths(-3).ToSqlFormat();
			var time2 = utcNow.ToSqlFormat();

			var sql = $@"
INSERT INTO StorageMain (SM_PK, SM_CD1, SM_CD2, SM_DB, SM_Type, SM_PhysicalLocation, SM_ParentFK, SM_SystemCreateTimeUtc, SM_SystemCreateUser, SM_SystemLastEditTimeUtc, SM_SystemLastEditUser) VALUES
('{Guid1}', 0, 0, 1, 'SHP', '', '{Guid.NewGuid()}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Guid2}', 0, 0, 1, 'SHP', '', '{Guid.NewGuid()}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Guid3}', 0, 0, 1, 'SHP', '', '{Guid.NewGuid()}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Guid4}', 0, 0, 1, 'SHP', '', '{Guid.NewGuid()}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO StmALog (SL_PK,SL_Table,SL_Parent,SL_IsEstimate,SL_IsCancelled,SL_Reference,SL_PostedTimeUtc,SL_EventTime,SL_GS_NKUser,SL_SE_NKEvent,SL_GB_NKBranch,SL_GE_NKDepartment,SL_FireWorkflow,SL_DataSource,SL_EventTimeUtc) VALUES
					('{Guid.NewGuid()}', 'StorageMain',			'{Guid1}', 'N', 'N', '', '{time1}', '{time1}', '~BP', 'ADD', 'BNE', 'BRN', 0, 'C', null),
					('{Guid.NewGuid()}', 'StorageMain',			'{Guid2}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null),
					('{Guid.NewGuid()}', 'StorageMain',			'{Guid3}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'DEL', 'BNE', 'BRN', 0, 'C', null),
					('{Guid.NewGuid()}', 'StorageMain',			'{Guid4}', 'N', 'N', '', '{time2}', '{time2}', '~BP', 'LEL', 'BNE', 'BRN', 0, 'C', null),
					('{Guid.NewGuid()}', 'GlbStaff',			'{Guid5}', 'N', 'N', '', '{time1}', '{time1}', '~BP', 'EDT', 'BNE', 'BRN', 0, 'C', null)
";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Table = 'StorageMain'"));

			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid1}'"));
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid2}'"));
			AssertEquals(false, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid3}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid4}'"));
			AssertEquals(true, TestConnection.Exists($"FROM StmALog WHERE SL_Parent = '{Guid5}'"));
		}
	}
}
