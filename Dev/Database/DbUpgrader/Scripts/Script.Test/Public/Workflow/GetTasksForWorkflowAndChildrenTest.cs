using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.Workflow;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Workflow.Testing
{
	[TestedType(typeof(GetTasksForWorkflowAndChildren))]
	sealed class GetTasksForWorkflowAndChildrenTest : DbCreateScriptTest
	{
		public void TestGetTasks()
		{
			var jobPK = Guid.NewGuid();
			var jobHeaderPK = Guid.NewGuid();

			var workflowPK = Guid.NewGuid();
			var workflowChildPK = Guid.NewGuid();
			var workflowGrandchildPK = Guid.NewGuid();

			var taskPK = Guid.NewGuid();
			var childTaskPK = Guid.NewGuid();
			var grandchildTaskPK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(jobHeaderPK, null, jobPK));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK, jobHeaderPK, jobPK));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowChildPK, jobHeaderPK, jobPK));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowGrandchildPK, jobHeaderPK, jobPK));

			insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflowGrandchildPK, workflowChildPK, "PCH"));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflowChildPK, workflowPK, "PCH"));

			insertSql.AppendFormat(BMDbTestHelper.GetProcessTasksInsertSql(taskPK, workflowPK, jobPK, "WKI", "ASN"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessTasksInsertSql(childTaskPK, workflowChildPK, jobPK, "WKI", "ASN"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessTasksInsertSql(grandchildTaskPK, workflowGrandchildPK, jobPK, "WKI", "ASN"));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertTaskPKsReturned("Parent workflow should contain tasks in all child workflows", workflowPK, taskPK, childTaskPK, grandchildTaskPK);
			AssertTaskPKsReturned("Child workflow should contain tasks in itself and its child workflow", workflowChildPK, childTaskPK, grandchildTaskPK);
			AssertTaskPKsReturned("Grandchild workflow should contain its own tasks only", workflowGrandchildPK, grandchildTaskPK);
		}

		void AssertTaskPKsReturned(string message, Guid workflowPK, params Guid[] taskPKs)
		{
			var results = GetResults(workflowPK).ToArray();
			AssertContainsExactElementsInAnyOrder(message, taskPKs, results);
		}

		IEnumerable<Guid> GetResults(Guid workflowPK)
		{
			var sql = string.Format("SELECT P9_PK from dbo.GetTasksForWorkflowAndChildren('{0}')", workflowPK);

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					var result = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
					yield return result;
				}
			}
		}
	}
}

