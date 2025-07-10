using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTaskStartabilityCheckerTest : BMSTestCaseWithFactory
	{
		public void TestIsTaskStartable()
		{
			var checker = new ProcessTaskStartabilityChecker();
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 30);
			Factory.Save();
			AssertEquals("Task1 is startable", true, task1.IsStartable());
			AssertEquals("Task1 is startable", true, checker.IsTaskStartable(task1));

			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10);
			IBMTestHelper testHelper = new BMSTestHelper();
			testHelper.CreateLink(workflow2, workflow1);
			Factory.Save();
			AssertEquals("Task1 is not startable - blocked by workflow2", false, task1.IsStartable());
			AssertEquals("Task1 is not startable - blocked by workflow2", false, checker.IsTaskStartable(task1));
		}

		public void TestIsProcessHeaderBlocked()
		{
			var checker = new ProcessTaskStartabilityChecker();
			var task1 = Factory.New<ProcessTask>();
			AssertEquals("Task 1 has no process header", false, checker.IsProcessHeaderBlocked(task1));

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			IBMTestHelper testHelper = new BMSTestHelper();
			testHelper.CreateLink(workflow1, workflow2);
			var task2 = CreateTask(workflow1, string.Empty, 30, "UDF", "OPN");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 40);

			AssertEquals("Task2 - workflow1 is not bloacked", false, checker.IsProcessHeaderBlocked(task2));
			AssertEquals("Task3 - workflow2 is bloacked by workflow1", true, checker.IsProcessHeaderBlocked(task3));
		}

		public void TestIsProcessHeaderOnlyBlockedByOtherJob()
		{
			var checker = new ProcessTaskStartabilityChecker();
			var task1 = Factory.New<ProcessTask>();
			AssertEquals("Task 1 has no process header", false, checker.IsProcessHeaderOnlyBlockedByOtherJob(task1));

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var workflow1 = CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader1, "workflow2");
			IBMTestHelper testHelper = new BMSTestHelper();
			testHelper.CreateLink(workflow1, workflow2);
			var task2 = CreateTask(workflow1, string.Empty, 30, "UDF", "OPN");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 40);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow3 = CreateWorkflow(jobHeader2, "workflow3");
			testHelper.CreateLink(workflow3, workflow1);

			AssertEquals("Task2 - workflow1 is bloacked by workflow3 from job2", true, checker.IsProcessHeaderOnlyBlockedByOtherJob(task2));
			AssertEquals("Task3 - workflow2 is bloacked by workflow1 on the same job", false, checker.IsProcessHeaderOnlyBlockedByOtherJob(task3));
		}
	}
}
