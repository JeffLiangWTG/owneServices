using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BufferManagement
{
	[TestedType(typeof(MigrateMilestoneCompletionPivotToSoftKey))]
	public class MigrateMilestoneCompletionPivotToSoftKeyTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new MigrateMilestoneCompletionPivotToSoftKey();
		}

		Guid task1 = Guid.NewGuid();
		Guid task2 = Guid.NewGuid();
		Guid task3 = Guid.NewGuid();
		Guid task4 = Guid.NewGuid();
		Guid milestone1 = Guid.NewGuid();
		Guid milestone2 = Guid.NewGuid();
		Guid milestone3 = Guid.NewGuid();
		Guid milestone4 = Guid.NewGuid();
		Guid header1 = Guid.NewGuid();
		Guid header2 = Guid.NewGuid();
		Guid header3 = Guid.NewGuid();
		Guid header4 = Guid.NewGuid();

		static void InsertTasks(params Guid[] ids)
		{
			foreach (var id in ids)
			{
				var sql = $@"
INSERT INTO ProcessTasks(P9_PK, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser) values
('{id}', '5907C0BD-3EE5-481F-B3E4-99299BB982DE', 'JE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		static void InsertMilestone(Guid id, string eventCode, string description)
		{
			var sql = $@"
INSERT INTO ProcessTasks(P9_PK, P9_SE_NKMilestoneEvent, P9_Description, P9_Type, P9_ParentID, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser) values
('{id}', '{eventCode}', '{description}', 'MIL', '5907C0BD-3EE5-481F-B3E4-99299BB982DE', 'JE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}

		static void InsertProcessHeaders(params Guid[] ids)
		{
			var processJobHeader = Guid.NewGuid();
			var pjhSql = $@"
INSERT INTO ProcessHeader (FH_PK, FH_FH_ParentHeader, FH_ParentId, FH_CompletionStatement, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_WorkflowType)
VALUES ('{processJobHeader}', null, '5907C0BD-3EE5-481F-B3E4-99299BB982DE', 'Test Workflow',  GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'JE')";
			Db.Connection.ExecuteNonQuery(pjhSql);

			foreach (var id in ids)
			{
				var sql = $@"
INSERT INTO ProcessHeader (FH_PK, FH_FH_ParentHeader, FH_ParentId, FH_CompletionStatement, FH_SystemCreateTimeUtc, FH_SystemCreateUser, FH_SystemLastEditTimeUtc, FH_SystemLastEditUser, FH_WorkflowType)
VALUES ('{id}', '{processJobHeader}', '5907C0BD-3EE5-481F-B3E4-99299BB982DE', 'Test Workflow',  GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'JE')";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		static void InsertPivot(Guid milestone, Guid parentId, string tableCode)
		{
			var sql = $@"
INSERT INTO MilestoneCompletionPivot(MCP_PK, MCP_P9_Milestone, MCP_ParentId, MCP_ParentTableCode, MCP_SystemCreateTimeUtc, MCP_SystemCreateUser, MCP_SystemLastEditTimeUtc, MCP_SystemLastEditUser)
VALUES (newid(), '{milestone}', '{parentId}', '{tableCode}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sql);
		}

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			var tableSql = @"
CREATE TABLE [dbo].[MilestoneCompletionPivot](
	[MCP_PK] [uniqueidentifier] NOT NULL,
	[MCP_P9_Milestone] [uniqueidentifier] NOT NULL,
	[MCP_ParentId] [uniqueidentifier] NOT NULL,
	[MCP_ParentTableCode] [varchar](3) NOT NULL,
	[MCP_SystemCreateTimeUtc] [smalldatetime] NOT NULL,
	[MCP_SystemCreateUser] [varchar](3) NOT NULL,
	[MCP_SystemLastEditTimeUtc] [smalldatetime] NOT NULL,
	[MCP_SystemLastEditUser] [varchar](3) NOT NULL,
 CONSTRAINT [PK_UX__MCP_PK] PRIMARY KEY NONCLUSTERED 
(
	[MCP_PK] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]";
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "MilestoneCompletionPivot", tableSql);

			InsertTasks(task1, task2, task3, task4);
			InsertProcessHeaders(header1, header2, header3, header4);
			InsertMilestone(milestone1, "Z00", "Milstone");
			InsertMilestone(milestone2, "Z00", "Bilstone");
			InsertMilestone(milestone3, "Z01", new string('A', 50));
			InsertMilestone(milestone4, "Z22", "A");

			InsertPivot(milestone1, task1, "P9");
			InsertPivot(milestone2, task2, "P9");
			InsertPivot(milestone3, task3, "P9");

			InsertPivot(milestone2, header1, "FH");
			InsertPivot(milestone3, header2, "FH");
			InsertPivot(milestone4, header3, "FH");
		}

		static void AssertTaskPivotKey(string expectedKey, Guid pk)
		{
			var sql = $"select IsNull(P9_MilestoneCompletionPivotKey, '') from ProcessTasks where P9_PK = '{pk}'";
			AssertEquals(expectedKey, Db.Connection.ExecuteScalar<string>(sql));
		}

		static void AssertHeaderPivotKey(string expectedKey, Guid pk)
		{
			var sql = $"select IsNull(FH_MilestoneCompletionPivotKey, '') from ProcessHeader where FH_PK = '{pk}'";
			AssertEquals(expectedKey, Db.Connection.ExecuteScalar<string>(sql));
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();

			AssertTaskPivotKey("Z00 - Milstone", task1);
			AssertTaskPivotKey("Z00 - Bilstone", task2);
			AssertTaskPivotKey("Z01 - " + new string('A', 50), task3);
			AssertTaskPivotKey("", task4);

			AssertHeaderPivotKey("Z00 - Bilstone", header1);
			AssertHeaderPivotKey("Z01 - " + new string('A', 50), header2);
			AssertHeaderPivotKey("Z22 - A", header3);
			AssertHeaderPivotKey("", header4);
		}
	}
}
