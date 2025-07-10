using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	public class VisualBoardTaskComparerTest : BMSTestCaseWithFactory
	{
		public void TestCompare()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var org = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.AddTag(config.RedTag);
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.AddTag(config.PlatinumTag);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();

			workflow3.FH_ReleaseDateTime = ZDateTime.Now;
			workflow4.FH_ReleaseDateTime = ZDateTime.Now.AddSeconds(1);

			var task1 = CreateTask(org, workflow1, "Task 1", ProcessTaskStatusCodeList.Codes.Closed, 1);
			var task3 = CreateTask(org, workflow1, "Task 3", ProcessTaskStatusCodeList.Codes.Suspended, 3);
			var task4 = CreateTask(org, workflow1, "Task 4", ProcessTaskStatusCodeList.Codes.Working, 4);
			var task5 = CreateTask(org, workflow1, "Task 5", ProcessTaskStatusCodeList.Codes.Closed, 4);
			var task6 = CreateTask(org, workflow2, "Task 6 PLT Task 1", ProcessTaskStatusCodeList.Codes.Open, 100);
			var task7 = CreateTask(org, workflow2, "Task 7 GOL Task 1", ProcessTaskStatusCodeList.Codes.Open, 100);
			var task8 = CreateTask(org, workflow3, "Task 8", ProcessTaskStatusCodeList.Codes.Open, 110);
			var task9 = CreateTask(org, workflow4, "Task 9", ProcessTaskStatusCodeList.Codes.Open, 100);
			var task10 = CreateTask(org, workflow4, "Task 10", ProcessTaskStatusCodeList.Codes.Open, 112);

			AssertEquals("Task 2 should be the current task", task3, task3.ParentTaskCollection.GetCurrentTask(workflow1));

			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var tasks = new[] { task1, task3, task4, task5, task6, task7, task8, task9, task10 }.OrderBy(t => new NaiveTaskOrderable(t, viewModel), new VisualBoardTaskComparer()).ToArray();

			AssertEquals("Working task first", task4.P9_TaskID, tasks[0].P9_TaskID);
			AssertEquals("Suspended task next", task3.P9_TaskID, tasks[1].P9_TaskID);
			AssertEquals("Current tasks next", task6.P9_TaskID, tasks[2].P9_TaskID);
			AssertEquals("High priority next (PLT - P1)", task6.P9_TaskID, tasks[2].P9_TaskID);
			AssertEquals("Next highest priority (GOL - P2)", task7.P9_TaskID, tasks[3].P9_TaskID);
			AssertEquals("Next comes tasks with no priority set, but closed with lower sequence number", task1.P9_TaskID, tasks[4].P9_TaskID);
			AssertEquals("Closed with higher sequence number", task5.P9_TaskID, tasks[5].P9_TaskID);
			AssertEquals("Has higher sequence number than Task 9, but earlier release date", task8.P9_TaskID, tasks[6].P9_TaskID);
			AssertEquals("Has lower sequence number than Task 8, but later release date", task9.P9_TaskID, tasks[7].P9_TaskID);
			AssertEquals("Same release date as Task 9, but higher sequence number", task10.P9_TaskID, tasks[8].P9_TaskID);
		}

		ProcessTask CreateTask(IWorkflowProvider parent, ProcessHeader header, string taskID, string status, int sequence)
		{
			var task = parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_TaskID = taskID;
			task.P9_Status = status;
			task.P9_Sequence = sequence;

			return task;
		}
	}
}
