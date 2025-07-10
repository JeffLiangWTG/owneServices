using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkflowStatusUpdaterTest : BMSTestCaseWithFactory
	{
		public void TestStatus_CompanySwitching()
		{
			BMSTestHelper.CreateSystem(Factory, "SHP");
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			shipment.FillWithValidTestData();

			var jobHeader = BMSTestHelper.CreateJobHeader((IWorkflowProvider)shipment, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Steve");
			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_ShareTasksForAllCompanies = false;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				shipment = (BusinessObject)newFactory.Load<IForwardingShipment>(shipment.PK);
				AssertEquals("Shipment tasks are from another company.", 0, ((IWorkflowProvider)shipment).WorkflowItems.Tasks.Count);

				WorkflowStatusUpdater.UpdateWorkflowStatuses([workflow]);

				AssertEquals(WorkflowStatusList.Codes.Open, newFactory.Load<ProcessHeader>(workflow.PK).FH_Status);
			}
		}

		public void TestWorkflowStatusUpdater_EnsuresNonNegativeRemainingMinutes_WhenNegativeValueProvided()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var header = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(header, "Workflow 1");

			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_EstimateVariationFactor = -3;
			task.P9_EstDuration = new ZInt(10).GetDateTimeFromMinutes();

			AssertNoExceptionThrown(Factory.Save);

			workflow.Reload();

			Assert("FH_RemainingMinutesToComplete should be non-negative after service runs",
				workflow.FH_RemainingMinutesToComplete >= 0);
		}

		#region Performance

		public void TestJobCloseLog_WhenSubscriberClosesTheJob_FromLaterTimeZone_ShouldBeAddedOncePerClosedWorkflowAndJob()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "CAYHZ"; // Halifax, Nova Scotia (time zone way behind the default of Brisbane)
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_GB_HomeBranch = branch.PK;
			user.GS_GE_HomeDepartment = department.PK;

			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var job = jobHeader.Parent;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);

			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflowInFactory2 = factory2.Load<ProcessHeader>(workflow.PK);

			workflowInFactory2.Tasks.Single(x => x.P9_Sequence == 1).P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			factory2.Save();

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			BMSTestHelper.AssertWorkflowsHaveLog("There should be Job Open logs when the job is first saved, which will cause Job Close logs to be added later. SAD!", job, Events.JobOpen, jobHeader, workflow);
			BMSTestHelper.AssertWorkflowsHaveLog("There should only be one Job Close event for the closed workflow and one for the job level workflow. SAD!", job, Events.JobClose, workflow, jobHeader);
		}

		[TestDate(2025, 05, 06)]
		public void TestUpdateWorkflowStatuses_DbHits_MultipleWorkflowsInJob()
		{
			AssertUpdateWorkflowStatuses_DbHits(dbHits: new Dictionary<string, int>()
				{
					{ ProcessHeaderSchema.Constants.TableName, 2 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 3 },
				},
				siblingsCount: 5
				);
		}

		[TestDate(2025, 05, 06)]
		public void TestUpdateWorkflowStatuses_DbHits_CrazyHierarchy()
		{
			const int childrenCount = 4;
			const int siblingsCount = 5;
			const int nephewsPerSiblingCount = 3;
			const int parentsInLawCount = 4;
			const int siblingsInLawPerParentCount = 2;

			const int jobHeaderCount = 1 + parentsInLawCount;

			AssertUpdateWorkflowStatuses_DbHits(dbHits: new Dictionary<string, int>()
				{
					{ ProcessHeaderSchema.Constants.TableName, 14 },
					{ ProcessHeaderLinkSchema.Constants.TableName, jobHeaderCount },
					{ ProcessTasksSchema.Constants.TableName, 12 },
				},
				childrenCount,
				siblingsCount,
				nephewsPerSiblingCount,
				parentsInLawCount,
				siblingsInLawPerParentCount
				);
		}

		void AssertUpdateWorkflowStatuses_DbHits(Dictionary<string, int> dbHits,
			int childrenCount = 0, int siblingsCount = 0, int nephewsPerSiblingCount = 0, int parentsInLawCount = 0, int siblingsInLawPerParentCount = 0)
		{
			var headerPKs = new List<ZGuid>();

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header");
			var targetWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Target Workflow");
			headerPKs.AddRange([jobHeader.PK, targetWorkflow.PK]);

			for (var i = 0; i < childrenCount; i++)
			{
				var childWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, $"Child Workflow {i}");
				headerPKs.Add(childWorkflow.PK);
				BMSTestHelper.MakeChildOf(childWorkflow, targetWorkflow);
			}

			for (var i = 0; i < siblingsCount; i++)
			{
				var siblingWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, $"Sibling Workflow {i}");
				headerPKs.Add(siblingWorkflow.PK);

				for (var j = 0; j < nephewsPerSiblingCount; j++)
				{
					var nephewWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, $"Nephew Workflow {i}_{j}");
					headerPKs.Add(nephewWorkflow.PK);
					BMSTestHelper.MakeChildOf(nephewWorkflow, siblingWorkflow);
				}
			}

			for (var i = 0; i < parentsInLawCount; i++)
			{
				var parentInLawJobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: $"Parent-in-law Job Header {i}");
				headerPKs.Add(parentInLawJobHeader.PK);
				BMSTestHelper.MakeChildOf(targetWorkflow, parentInLawJobHeader);

				for (var j = 0; j < siblingsInLawPerParentCount; j++)
				{
					var siblingInLawWorkflow = BMSTestHelper.CreateWorkflowAndTask(parentInLawJobHeader, $"Sibling-in-law Workflow {i}_{j}");
					headerPKs.Add(siblingInLawWorkflow.PK);
				}
			}

			Factory.Save();

			var sqlText = string.Format($"UPDATE dbo.ProcessHeader SET FH_Status = 'CLS' WHERE FH_PK IN ({string.Join(", ", headerPKs.Select(pk => pk.ToSqlGuid()))})");
			TestConnection.ExecuteNonQuery(sqlText);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedWorkflow = newFactory.Load<ProcessHeader>(targetWorkflow.PK);

			using (AssertDbHitsForAllFactories(dbHits, ignoreUnspecified: true))
			{
				WorkflowStatusUpdater.UpdateWorkflowStatuses([loadedWorkflow], reloadStatusRelatedDataFromDb: true);
			}

			newFactory.Save();
			var reloadedHeaders = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, headerPKs));
			Assert("All workflows and JLWs should now be open", reloadedHeaders.All(h => h.FH_Status == WorkflowStatusList.Codes.Open));
		}

		#endregion

		#region Loop Protection

		public void TestShouldNotCrashOnParentChildLoops()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "Workflow 1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 2");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow 2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 3");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader3, "Workflow 3");

			BMSTestHelper.MakeChildOf(jobHeader2, workflow1);
			BMSTestHelper.MakeChildOf(jobHeader3, workflow2);
			BMSTestHelper.MakeChildOf(jobHeader1, workflow3); // loop

			Factory.Save();

			var headerPKs = new[] { jobHeader1.PK, workflow1.PK, jobHeader2.PK, workflow2.PK, jobHeader3.PK, workflow3.PK };
			var sqlText = string.Format($"UPDATE dbo.ProcessHeader SET FH_Status = 'CLS' WHERE FH_PK IN ({string.Join(", ", headerPKs.Select(pk => pk.ToSqlGuid()))})");
			TestConnection.ExecuteNonQuery(sqlText);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow1.PK);

			WorkflowStatusUpdater.UpdateWorkflowStatuses([loadedWorkflow]);

			newFactory.Save();
			var reloadedHeaders = newFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, headerPKs));
			Assert(reloadedHeaders.All(h => h.FH_Status == WorkflowStatusList.Codes.Open));
		}

		public void TestShouldNotCrashOnDependencyLoops()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header");
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow 3");

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow1); // loop

			Factory.Save();

			var headerPKs = new[] { jobHeader.PK, workflow1.PK, workflow2.PK, workflow3.PK };
			var sqlText = string.Format($"UPDATE dbo.ProcessHeader SET FH_Status = 'CLS' WHERE FH_PK IN ({string.Join(", ", headerPKs.Select(pk => pk.ToSqlGuid()))})");
			TestConnection.ExecuteNonQuery(sqlText);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow1.PK);

			WorkflowStatusUpdater.UpdateWorkflowStatuses([loadedWorkflow]);
			newFactory.Save();

			jobHeader.Reload();
			workflow1.Reload();
			workflow2.Reload();
			workflow3.Reload();

			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow3.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
		}

		#endregion
	}
}
