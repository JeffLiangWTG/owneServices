using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Helpers;
using Enterprise.BufferManagement.Service.Shared.Workflows.Dtos;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.BufferManagement.Service.Test
{
	public class WorkflowServiceTest : TestCaseWithFactory
	{
		WorkflowService Service;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
			Service = new WorkflowService();
		}

		protected override void TearDown()
		{
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry("ORG");
			base.TearDown();
		}

		#region Reorder

		public void TestReorder_Should_Fail_With_OnlyOneItem()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 1,

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task1.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskReorderingRequiresAtLeastTwoTasks, error.Messages.ToList()[0]);
		}

		public void TestReorder_To_Last_Position_Should_Fail_With_Wrong_Previous()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_To_Last_Position()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task3.PK.ToGuid(),
				PreviousTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task2.P9_Sequence);
		}

		public void TestReorder_To_First_Position()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				NextTaskId = task1.PK.ToGuid(),
				NextTaskSequence = 1,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(1, task2.P9_Sequence);
			AssertEquals(2, task1.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
		}

		public void TestReorder_FromLast_ToFirst_Position()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2.PK.ToGuid(),
				ActualSequence = 3,
				ActualNextTaskId = null,

				//Target
				NextTaskId = task1.PK.ToGuid(),
				NextTaskSequence = 1,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task3.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(1, task3.P9_Sequence);
			AssertEquals(2, task1.P9_Sequence);
			AssertEquals(3, task2.P9_Sequence);
		}

		public void TestReorder_To_First_Position_Should_Fail_With_Wrong_Next()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_To_Same_Position_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceIsTheSameMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_ActualPrevious_Changed_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task4.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_ActualNext_Changed_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task4.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_Same_Previous_And_Next_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task1.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.PreviousAndNextMustBeDifferentMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_Wrong_Previous_And_Next_Order_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task3.PK.ToGuid(),
				PreviousTaskSequence = 3,
				NextTaskId = task2.PK.ToGuid(),
				NextTaskSequence = 2,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.PreviousAndNextMustBeInSequentialOrderMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_Wrong_NextTask_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1_group1 = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "Task 1 - Group 1");
			var task2_group1 = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "Task 2 - Group 1");

			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "Task 3");
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4, description: "Task 4");
			var task5 = BMSTestHelper.CreateTask(workflow, sequence: 5, description: "Task 5");

			var task6_group2 = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "Task 6 - Group 2");
			var task7_group2 = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "Task 7 - Group 2");

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2_group1.PK.ToGuid(),
				ActualSequence = 3,
				ActualNextTaskId = task4.PK.ToGuid(),

				//Target
				PreviousTaskId = task5.PK.ToGuid(),
				PreviousTaskSequence = 5,
				NextTaskId = task7_group2.PK.ToGuid(),
				NextTaskSequence = 6,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task3.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.NextMustBeTheFirstInNextGroup, error.Messages.ToList()[0]);
		}

		public void TestReorder_Wrong_PreviousTask_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1_group1 = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "A");
			var task2_group1 = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "B");

			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);
			var task5 = BMSTestHelper.CreateTask(workflow, sequence: 5);

			var task6_group2 = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "F");
			var task7_group2 = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "G");

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task3.PK.ToGuid(),
				ActualSequence = 4,
				ActualNextTaskId = task5.PK.ToGuid(),

				//Target
				PreviousTaskId = task1_group1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task4.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.PreviousMustBeTheLastInPreviousGroup, error.Messages.ToList()[0]);
		}

		public void TestReorder_With_Different_Sequence_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualSequence = 4,

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertContains(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestReorder_To_InBetween()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualSequence = 1,
				ActualNextTaskId = task2.PK.ToGuid(),

				//Target
				PreviousTaskId = task3.PK.ToGuid(),
				PreviousTaskSequence = 3,
				NextTaskId = task4.PK.ToGuid(),
				NextTaskSequence = 4,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task1.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(2, task2.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task1.P9_Sequence);
			AssertEquals(5, task4.P9_Sequence);
		}

		public void TestReorder_Of_GroupedTask_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			BMSTestHelper.CreateTask(workflow, sequence: 1);

			//group
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);

			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2A.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task3.PK.ToGuid(),
				PreviousTaskSequence = 3,
				NextTaskId = task4.PK.ToGuid(),
				NextTaskSequence = 4,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2B.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(2, task2A.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task2B.P9_Sequence);
			AssertEquals(5, task4.P9_Sequence);
		}

		public void TestUngroupingReorder_Of_GroupedTask_ToLastPosition_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			BMSTestHelper.CreateTask(workflow, sequence: 1);

			//group
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2A.PK.ToGuid(),
				ActualSequence = 2,

				//Target
				PreviousTaskId = task2A.PK.ToGuid(),
				PreviousTaskSequence = 2,

				IsUngrouping = true,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2B.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(2, task2A.P9_Sequence);
			AssertEquals(3, task2B.P9_Sequence);
		}

		public void TestUngroupingReorder_Of_GroupedTask_ToLastPosition_Should_Fail_Without_Ungrouping_Flag()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			BMSTestHelper.CreateTask(workflow, sequence: 1);

			//group
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2A.PK.ToGuid(),
				ActualSequence = 2,

				//Target
				PreviousTaskId = task2A.PK.ToGuid(),
				PreviousTaskSequence = 2,
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2B.PK.ToGuid(), request, out PaveError error);

			Assert(!reordered);
			AssertNotNull(error);
		}

		public void TestUngroupingReorder_Of_GroupedTask_ToFirstPosition_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2A.PK.ToGuid(),
				ActualSequence = 2,

				//Target
				NextTaskId = task2A.PK.ToGuid(),
				NextTaskSequence = 2,

				IsUngrouping = true
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2B.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(1, task2B.P9_Sequence);
			AssertEquals(2, task2A.P9_Sequence);
		}

		public void TestUngroupingReorder_Of_GroupedTask_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);

			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 5);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2A.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task2A.PK.ToGuid(),
				PreviousTaskSequence = 2,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 5,

				IsUngrouping = true
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2B.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(2, task2A.P9_Sequence);
			AssertEquals(3, task2B.P9_Sequence);
			AssertEquals(5, task3.P9_Sequence);
		}

		public void TestUngroupingReorder_Of_GroupedTask_InBetween_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2C = BMSTestHelper.CreateTask(workflow, sequence: 2);

			var task5 = BMSTestHelper.CreateTask(workflow, sequence: 5);
			var task6 = BMSTestHelper.CreateTask(workflow, sequence: 6);

			Factory.Save();

			var request = new WorkflowTaskReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2A.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task2C.PK.ToGuid(),

				//Target
				PreviousTaskId = task5.PK.ToGuid(),
				PreviousTaskSequence = 5,
				NextTaskId = task6.PK.ToGuid(),
				NextTaskSequence = 6,

				IsUngrouping = true
			};

			bool reordered = Service.TryReorderTask(workflow.PK.ToGuid(), task2B.PK.ToGuid(), request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(2, task2A.P9_Sequence);
			AssertEquals(2, task2C.P9_Sequence);
			AssertEquals(5, task5.P9_Sequence);
			AssertEquals(6, task2B.P9_Sequence);
			AssertEquals(7, task6.P9_Sequence);
		}

		#endregion

		#region Group Single Task

		public void TestGroup_Should_Fail_With_OnlyOneItem()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);

			Factory.Save();

			var request = new WorkflowGroupTaskRequest()
			{
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 1,
			};

			var grouped = Service.TryGroupTask(workflow.PK.ToGuid(), 1, task1.PK.ToGuid(), request, out PaveError error);

			Assert(!grouped);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskGroupingRequiresAtLeastTwoTasksWithDifferentSequenceNumber, error.Messages.ToList()[0]);
		}

		public void TestGroup_With_Same_SequenceNumber_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			//group 1
			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 1);

			//task 3
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupTaskRequest()
			{
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 1,
				ActualNextTaskId = task3.PK.ToGuid(),
			};

			bool grouped = Service.TryGroupTask(workflow.PK.ToGuid(), 1, task2.PK.ToGuid(), request, out PaveError error);

			Assert(!grouped);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskGroupingRequiresAtLeastTwoTasksWithDifferentSequenceNumber, error.Messages.ToList()[0]);
		}

		public void TestGroup_ActualPrevious_Changed_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupTaskRequest()
			{
				ActualPreviousTaskId = task4.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),
			};

			bool grouped = Service.TryGroupTask(workflow.PK.ToGuid(), 1, task2.PK.ToGuid(), request, out PaveError error);

			Assert(!grouped);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroup_ActualNext_Changed_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupTaskRequest()
			{
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task4.PK.ToGuid(),
			};

			bool grouped = Service.TryGroupTask(workflow.PK.ToGuid(), 1, task2.PK.ToGuid(), request, out PaveError error);

			Assert(!grouped);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroup_With_Different_Sequence_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupTaskRequest()
			{
				ActualSequence = 4,
			};

			bool grouped = Service.TryGroupTask(workflow.PK.ToGuid(), 1, task2.PK.ToGuid(), request, out PaveError error);

			Assert(!grouped);
			AssertContains(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroup_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupTaskRequest()
			{
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualSequence = 2,
				ActualNextTaskId = task3.PK.ToGuid(),
			};

			bool grouped = Service.TryGroupTask(workflow.PK.ToGuid(), 4, task2.PK.ToGuid(), request, out PaveError error);

			Assert(grouped);
			AssertNull(error);
			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task4.P9_Sequence);
			AssertEquals(4, task2.P9_Sequence);
		}

		#endregion

		#region Group Reorder

		public void TestGroupReorder_Should_Fail_With_LessThanTwoItems()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 1, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_To_Last_Position_Should_Fail_With_Wrong_Previous()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_To_Last_Position()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task3.PK.ToGuid(),
				PreviousTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task2A.P9_Sequence);
			AssertEquals(4, task2A.P9_Sequence);
		}

		public void TestGroupReorder_To_First_Position()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				NextTaskId = task1.PK.ToGuid(),
				NextTaskSequence = 1,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(1, task2A.P9_Sequence);
			AssertEquals(1, task2B.P9_Sequence);
			AssertEquals(2, task1.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
		}

		public void TestGroupReorder_FromLast_ToFirst_Position()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task2.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task3A.PK.ToGuid(), task3B.PK.ToGuid()
				},
				ActualNextTaskId = null,

				//Target
				NextTaskId = task1.PK.ToGuid(),
				NextTaskSequence = 1,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 3, request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(1, task3A.P9_Sequence);
			AssertEquals(1, task3B.P9_Sequence);
			AssertEquals(2, task1.P9_Sequence);
			AssertEquals(3, task2.P9_Sequence);
		}

		public void TestGroupReorder_To_First_Position_Should_Fail_With_Wrong_Next()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_To_Same_Position_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceIsTheSameMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_ActualPrevious_Changed_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task4.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_ActualNext_Changed_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task4.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_Same_Previous_And_Next_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task1.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.PreviousAndNextMustBeDifferentMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_Wrong_Previous_And_Next_Order_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},
				ActualNextTaskId = task3.PK.ToGuid(),

				//Target
				PreviousTaskId = task3.PK.ToGuid(),
				PreviousTaskSequence = 3,
				NextTaskId = task2A.PK.ToGuid(),
				NextTaskSequence = 2,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.PreviousAndNextMustBeInSequentialOrderMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_Wrong_NextTask_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1A = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "Task 1 - Group 1");
			var task1B = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "Task 2 - Group 1");

			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "Task 3");
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "Task 3");

			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4, description: "Task 4");
			var task5 = BMSTestHelper.CreateTask(workflow, sequence: 5, description: "Task 5");

			var task6A = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "Task 6 - Group 2");
			var task6B = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "Task 7 - Group 2");

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task1B.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task3A.PK.ToGuid(), task3B.PK.ToGuid()
				},
				ActualNextTaskId = task4.PK.ToGuid(),

				//Target
				PreviousTaskId = task5.PK.ToGuid(),
				PreviousTaskSequence = 5,
				NextTaskId = task6B.PK.ToGuid(),
				NextTaskSequence = 6,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 3, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.NextMustBeTheFirstInNextGroup, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_Wrong_PreviousTask_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1A = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "A");
			var task1B = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "B");

			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4A = BMSTestHelper.CreateTask(workflow, sequence: 4);
			var task4B = BMSTestHelper.CreateTask(workflow, sequence: 4);
			var task5 = BMSTestHelper.CreateTask(workflow, sequence: 5);

			var task6A = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "F");
			var task6B = BMSTestHelper.CreateTask(workflow, sequence: 6, description: "G");

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualPreviousTaskId = task3.PK.ToGuid(),
				ActualGroupTasks = new[]
				{
					task4A.PK.ToGuid(), task4B.PK.ToGuid()
				},
				ActualNextTaskId = task5.PK.ToGuid(),

				//Target
				PreviousTaskId = task1A.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 4, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.PreviousMustBeTheLastInPreviousGroup, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_Wrong_PreviousTask_Should_Fail_UnorderedScenario()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1A = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "A");
			var task1B = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "B");

			//the order should be Australia, Brazil, Canada and Colombia
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "COLOMBIA");
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "BRAZIL");
			var task2C = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "CANADA");
			var task2D = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "AUSTRALIA");

			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "F");
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "G");

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualGroupTasks = new[]
				{
					task1A.PK.ToGuid(), task1B.PK.ToGuid()
				},
				ActualNextTaskId = task2A.PK.ToGuid(),

				//Target
				PreviousTaskId = task2D.PK.ToGuid(),
				PreviousTaskSequence = 2,
				NextTaskId = task3A.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 1, request, out PaveError error);

			Assert(!reordered);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.PreviousMustBeTheLastInPreviousGroup, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_Wrong_PreviousTask_Should_Work_OrderedScenario()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1A = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "A");
			var task1B = BMSTestHelper.CreateTask(workflow, sequence: 1, description: "B");

			//the order should be Australia, Brazil, Canada and Colombia
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "AUSTRALIA");
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "BRAZIL");
			var task2C = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "CANADA");
			var task2D = BMSTestHelper.CreateTask(workflow, sequence: 2, description: "COLOMBIA");

			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "F");
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3, description: "G");

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualGroupTasks = new[]
				{
					task1A.PK.ToGuid(), task1B.PK.ToGuid()
				},
				ActualNextTaskId = task2A.PK.ToGuid(),

				//Target
				PreviousTaskId = task2D.PK.ToGuid(),
				PreviousTaskSequence = 2,
				NextTaskId = task3A.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 1, request, out PaveError error);

			Assert(reordered);
		}

		public void TestGroupReorder_With_Different_Sequence_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4A = BMSTestHelper.CreateTask(workflow, sequence: 4);
			var task4B = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualGroupTasks = new[]
				{
					task4A.PK.ToGuid(), task4B.PK.ToGuid()
				},

				//Target
				PreviousTaskId = task1.PK.ToGuid(),
				PreviousTaskSequence = 1,
				NextTaskId = task3.PK.ToGuid(),
				NextTaskSequence = 3,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 4, request, out PaveError error);

			Assert(!reordered);
			AssertContains(LocalizationHelper.WorkflowTaskReordering.TaskSequenceChangedByAnotherUserMessage, error.Messages.ToList()[0]);
		}

		public void TestGroupReorder_To_InBetween()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1A = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task1B = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupReorderRequest()
			{
				//Source
				ActualGroupTasks = new[]
				{
					task1A.PK.ToGuid(), task1B.PK.ToGuid()
				},
				ActualNextTaskId = task2.PK.ToGuid(),

				//Target
				PreviousTaskId = task3.PK.ToGuid(),
				PreviousTaskSequence = 3,
				NextTaskId = task4.PK.ToGuid(),
				NextTaskSequence = 4,
			};

			bool reordered = Service.TryReorderGroup(workflow.PK.ToGuid(), 1, request, out PaveError error);

			Assert(reordered);
			AssertNull(error);
			AssertEquals(2, task2.P9_Sequence);
			AssertEquals(3, task3.P9_Sequence);
			AssertEquals(4, task1A.P9_Sequence);
			AssertEquals(4, task1B.P9_Sequence);
			AssertEquals(5, task4.P9_Sequence);
		}

		#endregion

		#region Merge Groups

		public void TestMergeGroups_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupMergeRequest()
			{
				SourceGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},

				TargetGroupTasks = new[]
				{
					task3A.PK.ToGuid(), task3B.PK.ToGuid()
				},
			};

			var merged = Service.TryMergeGroups(workflow.PK.ToGuid(), 2, 3, request, out PaveError error);

			Assert(merged);

			AssertNull(error);
			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(3, task2A.P9_Sequence);
			AssertEquals(3, task2B.P9_Sequence);
			AssertEquals(3, task3A.P9_Sequence);
			AssertEquals(3, task3B.P9_Sequence);
			AssertEquals(4, task4.P9_Sequence);
		}

		public void TestMergeGroups_Should_Fail_With_OnlyOneItem_On_Source()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupMergeRequest()
			{
				SourceGroupTasks = new[]
				{
					task2.PK.ToGuid()
				},

				TargetGroupTasks = new[]
				{
					task3A.PK.ToGuid(), task3B.PK.ToGuid()
				},
			};

			var merged = Service.TryMergeGroups(workflow.PK.ToGuid(), 2, 3, request, out PaveError error);

			Assert(!merged);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, error.Messages.ToList()[0]);
		}

		public void TestMergeGroups_Should_Fail_With_OnlyOneItem_On_Target()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupMergeRequest()
			{
				SourceGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},

				TargetGroupTasks = new[]
				{
					task3.PK.ToGuid()
				},
			};

			var merged = Service.TryMergeGroups(workflow.PK.ToGuid(), 2, 3, request, out PaveError error);

			Assert(!merged);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, error.Messages.ToList()[0]);
		}

		public void TestMergeGroups_With_Same_SequenceNumber_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2C = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2D = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowGroupMergeRequest()
			{
				SourceGroupTasks = new[]
				{
					task2A.PK.ToGuid(), task2B.PK.ToGuid()
				},

				TargetGroupTasks = new[]
				{
					task2C.PK.ToGuid(), task2D.PK.ToGuid()
				},
			};

			Factory.Save();

			bool merged = Service.TryMergeGroups(workflow.PK.ToGuid(), 2, 2, request, out PaveError error);

			Assert(!merged);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.MergeRequiresTwoDifferentGroups, error.Messages.ToList()[0]);
		}

		#endregion

		#region Merge Groups on a Task

		public void TestMergeGroups_OnTask_Should_Work()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task3C = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowMergeGroupOnTaskRequest()
			{
				SourceGroupTasks = new[]
				{
					task3A.PK.ToGuid(), task3B.PK.ToGuid(), task3C.PK.ToGuid()
				},

				TargetTaskSequence = 2,
			};

			var merged = Service.TryMergeGroupOnTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), 3, request, out PaveError error);

			Assert(merged);

			AssertNull(error);
			AssertEquals(1, task1.P9_Sequence);
			AssertEquals(2, task2.P9_Sequence);
			AssertEquals(2, task3A.P9_Sequence);
			AssertEquals(2, task3B.P9_Sequence);
			AssertEquals(2, task3C.P9_Sequence);
			AssertEquals(4, task4.P9_Sequence);
		}

		public void TestMergeGroups_OnTask_Should_Fail_With_OnlyOneItem_On_Source()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowMergeGroupOnTaskRequest()
			{
				SourceGroupTasks = new[]
				{
					task3.PK.ToGuid()
				},

				TargetTaskSequence = 2,
			};

			var merged = Service.TryMergeGroupOnTask(workflow.PK.ToGuid(), task2.PK.ToGuid(), 3, request, out PaveError error);

			Assert(!merged);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.GroupMustContainsAtLeastTwoTasks, error.Messages.ToList()[0]);
		}

		public void TestMergeGroups_OnTask_Should_Fail_With_MoreThanOneItem_On_Target()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task3A = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task3B = BMSTestHelper.CreateTask(workflow, sequence: 3);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowMergeGroupOnTaskRequest()
			{
				SourceGroupTasks = new[]
				{
					task3A.PK.ToGuid(), task3B.PK.ToGuid(),
				},

				TargetTaskSequence = 2,
			};

			var merged = Service.TryMergeGroupOnTask(workflow.PK.ToGuid(), task2A.PK.ToGuid(), 3, request, out PaveError error);

			Assert(!merged);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TargetTaskBelongsToGroup, error.Messages.ToList()[0]);
		}

		public void TestMergeGroups_OnTask_With_Same_SequenceNumber_Should_Fail()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			var task1 = BMSTestHelper.CreateTask(workflow, sequence: 1);
			var task2A = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2B = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2C = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task2D = BMSTestHelper.CreateTask(workflow, sequence: 2);
			var task4 = BMSTestHelper.CreateTask(workflow, sequence: 4);

			Factory.Save();

			var request = new WorkflowMergeGroupOnTaskRequest()
			{
				SourceGroupTasks = new[]
				{
					task2B.PK.ToGuid(), task2C.PK.ToGuid(), task2D.PK.ToGuid()
				},

				TargetTaskSequence = 2,
			};

			Factory.Save();

			var merged = Service.TryMergeGroupOnTask(workflow.PK.ToGuid(), task2A.PK.ToGuid(), 2, request, out PaveError error);

			Assert(!merged);
			AssertEquals(LocalizationHelper.WorkflowTaskReordering.TaskSequenceIsTheSameMessage, error.Messages.ToList()[0]);
		}

		#endregion
	}
}
