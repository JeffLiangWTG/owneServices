using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetWorkflowAndJobLevelWorkflowDescendentHierarchy))]
	sealed class GetWorkflowAndJobLevelWorkflowDescendentHierarchyTest : DbCreateScriptTest
	{
		public void TestMaxRecursionSet_ShouldNotReoccurPastLimit()
		{
			const int prereqDepth = 100;  // The default number of recursions allowed by SQL Server Studio

			var insertSql = new StringBuilder();

			var parentID = Guid.NewGuid();
			var jobHeaderPK = Guid.NewGuid();

			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(jobHeaderPK, null, parentID, status: "OPN"));

			var workflowPKList = new List<Guid>();
			for (int i = 0; i <= prereqDepth + 1; i++) // makes 102, so that we have a workflow to find the descendents of, as well as 101 others
			{
				workflowPKList.Add(Guid.NewGuid());
			}

			foreach (var pK in workflowPKList)
			{
				insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(pK, jobHeaderPK, parentID, completionStatement: "Workflow" + workflowPKList.IndexOf(pK).ToString("000"), status: "CLS"));
			}

			foreach (var pK in workflowPKList)
			{
				if (workflowPKList.Last() != pK)
				{
					insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), pK, workflowPKList[workflowPKList.IndexOf(pK) + 1], "PCH"));
				}
			}

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			var results = GetResults(workflowPKList[prereqDepth + 1], recursionLimit: prereqDepth).ToArray();

			AssertEquals("Even with 102 workflows and 101 workflow links, this function should only recur " + prereqDepth + " times.", prereqDepth + 1, results.Length); // This function will find a thing, then reoccur, so 100 recursions means 101 things returned

			AssertExceptionThrown<SqlException>("When the max recursion limit of 100 is breached, we should have an SQL Exception thrown.", () => GetResults(workflowPKList[workflowPKList.Count - 1], recursionLimit: prereqDepth + 2).ToArray());
		}

		IEnumerable<Guid> GetResults(Guid startingPK, int recursionLimit)
		{
			var sql = string.Format("SELECT FH_PK from dbo.GetWorkflowAndJobLevelWorkflowDescendentHierarchy('{0}', {1})", startingPK, recursionLimit);

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					var result = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

					if (result != default(Guid))
					{
						yield return result;
					}
				}
			}
		}

		public void TestEnsureJobLevelWorkflowsAreReturnedAsWell()
		{
			var job1PK = Guid.NewGuid();
			var job1HeaderPK = Guid.NewGuid();
			var workflow1ParentPK = Guid.NewGuid();

			var job2PK = Guid.NewGuid();
			var job2HeaderPK = Guid.NewGuid();
			var workflow2ChildPK = Guid.NewGuid();

			var sql = new StringBuilder();

			sql.AppendFormat(WorkItemInsertSql, job1PK, "WI00000001");
			sql.AppendFormat(ProcessJobHeaderInsertSql, job1HeaderPK, "OPN", job1PK);
			sql.AppendFormat(ProcessHeaderWithCompletionStatementInsertSql, workflow1ParentPK, "OPN", job1HeaderPK, "Workflow-1 (Parent)", job1PK);

			sql.AppendFormat(WorkItemInsertSql, job2PK, "WI00000002");
			sql.AppendFormat(ProcessJobHeaderInsertSql, job2HeaderPK, "OPN", job2PK);
			sql.AppendFormat(ProcessHeaderWithCompletionStatementInsertSql, workflow2ChildPK, "OPN", job2HeaderPK, "Workflow-2 (Child)", job2PK);

			sql.AppendFormat(ProcessHeaderLinkInsertSql, job2HeaderPK, workflow2ChildPK, "PCH");
			sql.AppendFormat(ProcessHeaderLinkInsertSql, workflow2ChildPK, workflow1ParentPK, "PCH");

			TestConnection.ExecuteNonQuery(sql.ToString());

			var command = TestConnection.Command($"SELECT FH_PK FROM GetWorkflowAndJobLevelWorkflowDescendentHierarchy('{workflow1ParentPK}', 100)");
			var descendentsPK = DataUtils.GetDataTableFromCommand(command);

			CombineAssertions(() =>
			{
				AssertEquals("count", 3, descendentsPK.Rows.Count);
				AssertContainsExactElementsInAnyOrder("FH_PK", new[] { workflow1ParentPK, workflow2ChildPK, job2HeaderPK }, descendentsPK.Rows.Cast<DataRow>().Select(x => x["FH_PK"]));
			});
		}

		const string WorkItemInsertSql = @"
			INSERT dbo.WorkItem (
				WKI_PK, WKI_WorkItemNumber, WKI_SystemCreateTimeUtc, WKI_SystemLastEditTimeUtc, WKI_SystemCreateUser, WKI_SystemLastEditUser
			) VALUES (
				'{0}', '{1}', GETDATE(), GETDATE(), 'E', 'E'
			)";

		const string ProcessJobHeaderInsertSql = @"
			INSERT dbo.ProcessHeader (
				FH_PK, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_Status, FH_FH_ParentHeader, FH_WorkflowType, FH_ParentId, FH_ParentTableCode, FH_CompletionStatement, FH_SystemCreateUser, FH_SystemLastEditUser
			) VALUES (
				'{0}', GETDATE(), GETDATE(), '{1}', null, 'WKI', '{2}', 'WKI', 'Job is Complete', 'E', 'E'
			)";

		const string ProcessHeaderWithCompletionStatementInsertSql = @"
			INSERT dbo.ProcessHeader (
				FH_PK, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_Status, FH_FH_ParentHeader, FH_ParentId, FH_ParentTableCode, FH_WorkflowType, FH_CompletionStatement, FH_SystemCreateUser, FH_SystemLastEditUser
			) VALUES (
				'{0}', GETDATE(), GETDATE(), '{1}', '{2}', '{4}', 'WKI', 'WKI', '{3}', 'E', 'E'
			)";

		const string ProcessHeaderLinkInsertSql = @"
			INSERT dbo.ProcessHeaderLink (
				FP_PK, FP_FH_HeaderFrom, FP_FH_HeaderTo, FP_LinkType, FP_SystemCreateTimeUtc, FP_SystemLastEditTimeUtc, FP_SystemCreateUser, FP_SystemLastEditUser
			) VALUES (
				NEWID(), '{0}', '{1}', '{2}', GETDATE(), GETDATE(), 'E', 'E'
			)";
	}
}
