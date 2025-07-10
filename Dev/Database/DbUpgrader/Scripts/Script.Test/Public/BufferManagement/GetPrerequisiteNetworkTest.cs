using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetPrerequisiteNetwork))]
	sealed class GetPrerequisiteNetworkTest : DbCreateScriptTest
	{
		public void TestGetPrerequisiteNetwork()
		{
			var parentID1 = Guid.NewGuid(); // Doesn't matter. (Actually, now it does.)
			var parentID2 = Guid.NewGuid(); // Doesn't matter. (Actually, now it does.)
			var parentID3 = Guid.NewGuid(); // Doesn't matter. (Actually, now it does.)

			var jobHeader1 = Guid.NewGuid();
			var jobHeader2 = Guid.NewGuid();
			var jobHeader3 = Guid.NewGuid();

			var workflow1_1 = Guid.NewGuid();
			var workflow1_2 = Guid.NewGuid();

			var workflow2_1 = Guid.NewGuid();
			var workflow2_2 = Guid.NewGuid();

			var workflow3_1 = Guid.NewGuid();
			var workflow3_2 = Guid.NewGuid();
			var workflow3_3 = Guid.NewGuid();

			// jobHeader1 -> jobHeader2

			// workflow1_1 -> workflow1_2
			// workflow2_1 -> workflow2_2
			// workflow3_1 -> workflow3_2

			// workflow2_2 -> workflow3_1

			// workflow3_2
			//		|
			// workflow3_3

			var insertSql = new StringBuilder();

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader1, parentID1);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader2, parentID2);
			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader3, parentID3);

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1_1, jobHeader1, parentID1));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1_2, jobHeader1, parentID1));

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2_1, jobHeader2, parentID2));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2_2, jobHeader2, parentID2));

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow3_1, jobHeader3, parentID3));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow3_2, jobHeader3, parentID3));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow3_3, jobHeader3, parentID3));

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), jobHeader1, jobHeader2, "DEP"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow1_1, workflow1_2, "DEP"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow2_1, workflow2_2, "DEP"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow2_2, workflow3_1, "DEP"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow3_1, workflow3_2, "DEP"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow3_3, workflow3_2, "PCH"));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertProcessHeaderPKsReturned("jobHeader1 has no prereqs", jobHeader1);
			AssertProcessHeaderPKsReturned("workflow1_1 has no prereqs", workflow1_1);
			AssertProcessHeaderPKsReturned("workflow1_2 has workflow1_1 as its prereq", workflow1_2, workflow1_1);

			AssertProcessHeaderPKsReturned("jobHeader2 has jobHeader1 as its prereq", jobHeader2, jobHeader1);
			AssertProcessHeaderPKsReturned("workflow2_1 has jobHeader1 as its prereq", workflow2_1, jobHeader1);
			AssertProcessHeaderPKsReturned("workflow2_2 has workflow2_1 and jobHeader1 as its prereqs", workflow2_2, workflow2_1, jobHeader1);

			AssertProcessHeaderPKsReturned("jobHeader3 has no prereqs", jobHeader3);
			AssertProcessHeaderPKsReturned("workflow3_1 has workflow2_2, workflow2_1 and jobHeader1 as its prereqs", workflow3_1, workflow2_2, workflow2_1, jobHeader1);
			AssertProcessHeaderPKsReturned("workflow3_2 has workflow3_1, workflow2_2, workflow2_1 and jobHeader1 as its prereqs", workflow3_2, workflow3_1, workflow2_2, workflow2_1, jobHeader1);
			AssertProcessHeaderPKsReturned("workflow3_3 inherits its prereqs from workflow3_2, hence has workflow3_1, workflow2_2, workflow2_1 and jobHeader1 as its prereqs", workflow3_3, workflow3_1, workflow2_2, workflow2_1, jobHeader1);
		}

		public void TestGetPrerequisiteNetwork_WhenUpstreamParentChildLinksExist()
		{
			var parentID = Guid.NewGuid(); // Doesn't matter.

			var jobHeader = Guid.NewGuid();

			var workflow1 = Guid.NewGuid();
			var workflow2 = Guid.NewGuid();
			var workflow3 = Guid.NewGuid();
			var workflow4 = Guid.NewGuid();
			var workflow5 = Guid.NewGuid();
			var workflow6 = Guid.NewGuid();

			//            1 -> 2
			//                /
			//               3 -> 4 -> 5
			//                   /
			//                  6

			var insertSql = new StringBuilder();

			insertSql.AppendFormat(BMDbTestHelper.ProcessJobHeaderInsertSql, jobHeader, parentID);

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow1, jobHeader, parentID));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow2, jobHeader, parentID));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow3, jobHeader, parentID));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow4, jobHeader, parentID));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow5, jobHeader, parentID));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderInsertSql(workflow6, jobHeader, parentID));

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow1, workflow2, "DEP"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow3, workflow4, "DEP"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow4, workflow5, "DEP"));

			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow3, workflow2, "PCH"));
			insertSql.AppendFormat(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), workflow6, workflow4, "PCH"));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			AssertProcessHeaderPKsReturned("jobHeader has no prereqs", jobHeader);

			AssertProcessHeaderPKsReturned("workflow1 has no prereqs", workflow1);
			AssertProcessHeaderPKsReturned("workflow2 has workflow1 as its direct prereq", workflow2, workflow1);
			AssertProcessHeaderPKsReturned("workflow3 inherits workflow1 as its prereq via workflow2", workflow3, workflow1);
			AssertProcessHeaderPKsReturned("workflow4 has workflow3 as its direct prereq, and inherits workflow1 via workflow2", workflow4, workflow3, workflow1);
			AssertProcessHeaderPKsReturned("workflow5 has workflow4 and workflow3 as its direct prereqs (not via parent/child relationships), and inherits workflow1 via workflow2", workflow5, workflow4, workflow3, workflow1);
			AssertProcessHeaderPKsReturned("workflow6 inherits workflow4 and workflow3 as its prereqs via its parent workflow4, and inherits workflow1 via workflow2", workflow6, workflow3, workflow1);
		}

		public void TestGetPrerequisiteNetwork_WhenPrerequisiteChainLongerThanSqlServerLimit_ShouldNotCrashWhenSafeRecursionLimitProvided()
		{
			const int prereqDepth = 101;

			var insertSql = new StringBuilder();

			var parentID = Guid.NewGuid();
			var jobHeaderPK = Guid.NewGuid();
			var targetWorkflowPK = Guid.NewGuid();

			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(jobHeaderPK, null, parentID, status: "OPN"));

			var previousWorkflowPK = Guid.Empty;

			for (var i = 0; i <= prereqDepth; i++)
			{
				var workflowPK = Guid.NewGuid();

				if (previousWorkflowPK != Guid.Empty)
				{
					insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK, jobHeaderPK, parentID, completionStatement: "Workflow" + i.ToString("000"), status: "CLS"));
					insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), previousWorkflowPK, workflowPK, "DEP"));
				}
				else
				{
					insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(workflowPK, jobHeaderPK, parentID, completionStatement: "Workflow" + i.ToString("000")));
					insertSql.Append(BMDbTestHelper.GetProcessTasksInsertSql(Guid.NewGuid(), workflowPK, parentID, "FH", "OPN"));
				}

				previousWorkflowPK = workflowPK;
			}

			insertSql.Append(BMDbTestHelper.GetProcessHeaderInsertSql(targetWorkflowPK, jobHeaderPK, parentID, completionStatement: "Target", status: "BLK"));
			insertSql.Append(BMDbTestHelper.GetProcessTasksInsertSql(Guid.NewGuid(), targetWorkflowPK, parentID, "FH", "OPN"));
			insertSql.Append(BMDbTestHelper.GetProcessHeaderLinkInsertSql(Guid.NewGuid(), previousWorkflowPK, targetWorkflowPK, "DEP"));

			TestConnection.ExecuteNonQuery(insertSql.ToString());

			var results = GetResults(targetWorkflowPK, prereqDepth - 1).ToArray();

			AssertEquals("Limiting prereq depth to the default value of MAXRECURSION is safe", prereqDepth, results.Length);

			AssertExceptionThrown<SqlException>("Going beyond the default value of MAXRECURSION is not safe", () => GetResults(targetWorkflowPK, prereqDepth).ToArray());
		}

		void AssertProcessHeaderPKsReturned(string message, Guid startingPK, params Guid[] pksInOrder)
		{
			AssertProcessHeaderPKsReturned(message, startingPK, 100, pksInOrder);
		}

		void AssertProcessHeaderPKsReturned(string message, Guid startingPK, int recursionLimit, params Guid[] expectedPrereqPKs)
		{
			var results = GetResults(startingPK, recursionLimit).ToArray();

			AssertContainsExactElementsInAnyOrder(message, expectedPrereqPKs, results);
		}

		IEnumerable<Guid> GetResults(Guid startingPK, int recursionLimit)
		{
			var sql = string.Format("SELECT PrereqPK from dbo.GetPrerequisiteNetwork('{0}', {1})", startingPK, recursionLimit);

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

