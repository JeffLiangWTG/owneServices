using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetAncestorWorkflows))]
	sealed class GetAncestorWorkflowsTest : DbCreateScriptTest
	{
		public void TestGetAscendents()
		{
			var parentID = Guid.NewGuid();
			var jobHeaderPK = Guid.NewGuid();
			var workflowPK1 = Guid.NewGuid();
			var workflowPK2 = Guid.NewGuid();
			var workflowPK3 = Guid.NewGuid();
			var workflowPK4 = Guid.NewGuid();
			var workflowPK5 = Guid.NewGuid();

			var linkPK1 = Guid.NewGuid();
			var linkPK2 = Guid.NewGuid();
			var linkPK3 = Guid.NewGuid();
			var linkPK4 = Guid.NewGuid();

			var systemPK = Guid.NewGuid();
			var bucketPK = Guid.NewGuid();
			var bufferPK = Guid.NewGuid();

			var insertSql = new StringBuilder();

			insertSql.Append(BMDbTestHelper.GetBMSystemInsertSql(systemPK, "PCP"));
			insertSql.Append(BMDbTestHelper.GetBMComponentInsertSql(bucketPK, systemPK, "BUC", "Dead"));
			insertSql.Append(BMDbTestHelper.GetBMComponentInsertSql(bufferPK, systemPK, "BUF", "Buried"));

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeaderPK, parentID);
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK1, jobHeaderPK, parentID, componentPK: bucketPK));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK2, jobHeaderPK, parentID, componentPK: bucketPK));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK3, jobHeaderPK, parentID, componentPK: bucketPK));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK4, jobHeaderPK, parentID, componentPK: bucketPK));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK5, jobHeaderPK, parentID, componentPK: bucketPK));

			insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(linkPK1, workflowPK2, workflowPK1, "PCH", syncBufferPenetration: true));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(linkPK2, workflowPK3, workflowPK2, "PCH", syncBufferPenetration: true));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(linkPK3, workflowPK4, workflowPK3, "PCH", syncBufferPenetration: true));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(linkPK4, workflowPK5, workflowPK4, "PCH", syncBufferPenetration: false));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertProcessHeaderPKsReturned("workflowPK1 has no parents", workflowPK1, false);
			AssertProcessHeaderPKsReturned("workflowPK2 has workflowPK1 as a parent", workflowPK2, false, workflowPK1);
			AssertProcessHeaderPKsReturned("workflowPK3 has workflowPK1, workflowPK2 as parents", workflowPK3, false, workflowPK2, workflowPK1);
			AssertProcessHeaderPKsReturned("workflowPK4 has workflowPK1, workflowPK2, workflowPK3 as parents", workflowPK4, false, workflowPK3, workflowPK2, workflowPK1);
			AssertProcessHeaderPKsReturned("workflowPK5 has workflowPK1, workflowPK2, workflowPK3, workflowPK4 as parents", workflowPK5, false, workflowPK4, workflowPK3, workflowPK2, workflowPK1);

			CombineAssertions("When parent workflows aren't in buffers, no parents are returned when we're walking buffer penetration-synced links", () =>
			{
				AssertProcessHeaderPKsReturned("workflowPK1", workflowPK1, true);
				AssertProcessHeaderPKsReturned("workflowPK2", workflowPK2, true);
				AssertProcessHeaderPKsReturned("workflowPK3", workflowPK3, true);
				AssertProcessHeaderPKsReturned("workflowPK4", workflowPK4, true);
				AssertProcessHeaderPKsReturned("workflowPK5", workflowPK5, true);
			});

			TestConnection.ExecuteNonQuery($"UPDATE dbo.ProcessHeader SET FH_FC_CurrentComponent = '{bufferPK}' WHERE FH_PK = '{workflowPK2}'");

			CombineAssertions("When one parent workflow is in a buffer, only it is returned as a parent for its direct child workflow (when we're walking buffer penetration-synced links)", () =>
			{
				AssertProcessHeaderPKsReturned("workflowPK1", workflowPK1, true);
				AssertProcessHeaderPKsReturned("workflowPK2", workflowPK2, true);
				AssertProcessHeaderPKsReturned("workflowPK3 should find workflowPK2 as its parent as it is in the buffer", workflowPK3, true, workflowPK2);
				AssertProcessHeaderPKsReturned("workflowPK4", workflowPK4, true);
				AssertProcessHeaderPKsReturned("workflowPK5", workflowPK5, true);
			});

			TestConnection.ExecuteNonQuery($"UPDATE dbo.ProcessHeader SET FH_FC_CurrentComponent = '{bufferPK}' WHERE FH_FH_ParentHeader is not null");

			AssertProcessHeaderPKsReturned("workflowPK1 has no parents", workflowPK1, true);
			AssertProcessHeaderPKsReturned("workflowPK2 has workflowPK1 as a parent", workflowPK2, true, workflowPK1);
			AssertProcessHeaderPKsReturned("workflowPK3 has workflowPK1, workflowPK2 as parents", workflowPK3, true, workflowPK2, workflowPK1);
			AssertProcessHeaderPKsReturned("workflowPK4 has workflowPK1, workflowPK2, workflowPK3 as parents", workflowPK4, true, workflowPK3, workflowPK2, workflowPK1);
			AssertProcessHeaderPKsReturned("workflowPK5 has no parents where we are synchronising buffer penetration", workflowPK5, true);
		}

		public void TestMaxRecursionSet_ShouldNotReoccurPastLimit()
		{
			const int prereqDepth = 100; // The default number of recursions allowed by SQL Server Studio 

			var insertSql = new StringBuilder();

			var parentID = Guid.NewGuid();
			var jobHeaderPK = Guid.NewGuid();

			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(jobHeaderPK, null, parentID, status: "OPN"));

			var workflowPKList = new List<Guid>();
			for (int i = 0; i <= prereqDepth + 1; i++) // makes 102, so that we have a workflow to find the ancestors of, as well as 101 workflows to search through
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

			var results = GetResults(workflowPKList[0], false, recursionLimit: prereqDepth).ToArray();

			AssertEquals("Even with 102 workflows and 101 workflow links, this function should only recur " + prereqDepth + " times.", prereqDepth, results.Length);

			AssertExceptionThrown<SqlException>("When the max recursion limit of 100 is breached, we should have an SQL Exception thrown.", () => GetResults(workflowPKList[0], false, recursionLimit: prereqDepth + 1).ToArray());
		}

		void AssertProcessHeaderPKsReturned(string message, Guid startingPK, bool syncBufferPenetration, params Guid[] pksInOrder)
		{
			var results = GetResults(startingPK, syncBufferPenetration).ToArray();
			AssertArrayEqualsByElements(message, pksInOrder, results);
		}

		IEnumerable<Guid> GetResults(Guid startingPK, bool syncBufferPenetration, int recursionLimit = 30)
		{
			var sql = string.Format("SELECT FH_PK from dbo.GetAncestorWorkflows('{0}', {1}, {2})", startingPK, syncBufferPenetration ? 1 : 0, recursionLimit);

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
	}
}

