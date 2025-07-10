using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class CapabilityTaskAutoAssignmentQueryTest : BMSTestCaseWithFactory
	{
		#region Query Performance

		public void TestQueryPerformance()
		{
			var serviceTask = new CapabilityTaskAutoAssignmentServiceTaskForTest();

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var workflowQuery = serviceTask.GetWorkflowsQueryForTest(buffer);
				var result = Factory.Load<ProcessHeader>(workflowQuery);
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.FirstOrDefault(t => t.Item1.Contains("-- WorkflowCapabilityAssignerDataAccessor"));
				var planalyzer = new QueryPlanalyzer(queryPlan.Item2.Last());

				CombineAssertions(() =>
				{
					AssertEquals("ProcessHeader", 2, planalyzer.IndexSeeks.Count(i => i.TableName == ProcessHeaderSchema.Constants.TableName));
					AssertEquals("ProcessTask", 2, planalyzer.IndexSeeks.Count(i => i.TableName == ProcessTasksSchema.Constants.TableName));
				});
			}
		}

		#endregion

		#region Task
		public void TestCancelledTask_ShouldBeExcluded()
		{
			AssertWorkflows("WHEN workflow only contain cancelled task THEN should be excluded",
				ProcessTaskStatusCodeList.Codes.Cancelled,
				expectedWorkflows: 0);
		}

		public void TestClosedTask_ShouldBeExcluded()
		{
			AssertWorkflows("WHEN workflow only contain closed task THEN should be excluded",
				ProcessTaskStatusCodeList.Codes.Closed,
				expectedWorkflows: 0);
		}

		public void TestSuspendedTask_ShouldBeIncluded()
		{
			AssertWorkflows("WHEN workflow only contain suspended task THEN should be included",
				ProcessTaskStatusCodeList.Codes.Suspended,
				expectedWorkflows: 1);
		}

		public void AssertWorkflows(string message, string taskStatus, int expectedWorkflows)
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow X", buffer, autoAssignTasks: true, releaseDateTime: ZDateTime.Today.AddDays(-1));
			var task = BMSTestHelper.CreateTask(workflow, capability: capability, description: "Task X", lowEstMinutes: 60);

			task.P9_Status = taskStatus;

			Factory.Save();

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTaskForTest();
			var workflowQuery = serviceTask.GetWorkflowsQueryForTest(buffer);

			var workflows = Factory.Load<ProcessHeader>(workflowQuery);

			AssertEquals(message, expectedWorkflows, workflows?.Length ?? 0);
		}

		#endregion

		#region Workflow

		public void TestClosedWorkflow_ShouldBeExcluded()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow X", buffer, autoAssignTasks: true, releaseDateTime: ZDateTime.Today.AddDays(-1));
			var task = BMSTestHelper.CreateTask(workflow, capability: capability, description: "Task X", lowEstMinutes: 60);

			Factory.Save();

			TestConnection.ExecuteNonQuery($"UPDATE dbo.ProcessHeader SET FH_Status = 'CLS' WHERE FH_PK='{workflow.PK}'");

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, loadedWorkflow.FH_Status);

			var serviceTask = new CapabilityTaskAutoAssignmentServiceTaskForTest();
			var workflowQuery = serviceTask.GetWorkflowsQueryForTest(buffer);

			var workflows = Factory.Load<ProcessHeader>(workflowQuery);

			AssertEquals("WHEN workflow is closed THEN should be excluded",
				0,
				workflows?.Length ?? 0);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "WTGDEV";

			buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			capability.G4_AllowTaskAutoAssignment = true;

			BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ST1", "Staff1", capability);

			Factory.Save();
		}

		BMComponent buffer;
		GlbCapability capability;

		class CapabilityTaskAutoAssignmentServiceTaskForTest : CapabilityTaskAutoAssignmentServiceTask
		{
			public ZQuery GetWorkflowsQueryForTest(BMComponent component)
			{
				return ((WorkflowCapabilityAssignerDataAccessor)GetDataAccessor()).GetWorkflowsQuery_ExposedForTest(component);
			}
		}

		#endregion
	}
}
