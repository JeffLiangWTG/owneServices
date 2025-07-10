using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TicketDragDropHandlerTest : BMSTestCaseWithFactory
	{
		public void TestReallocateTaskDoesNotSaveOriginalFactory()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var resource1 = CreateStaffInCurrentBranchDept("AAA", "Adam state");
			var resource2 = CreateStaffInCurrentBranchDept("EEE", "EEEEEE");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource1.GS_Code, 90);

			Factory.Save();

			var task2 = CreateTask(workflow, resource1.GS_Code, 90);
			AssertEquals(false, task2.IsInDatabase);

			var task1Content = new TaskCardContent(task1, viewModel);

			var visualBoardChannel1 = viewModel.CreateChannelForTest(resource1);
			var visualBoardChannel2 = viewModel.CreateChannelForTest(resource2);

			var sourceCell = new CellContent(0, 0, CellContentType.Cards) { Channel = visualBoardChannel1 };
			var destinationCell = new CellContent(0, 1, CellContentType.Cards) { Channel = visualBoardChannel2 };
			var cardContent = new TaskCardContent(task1, viewModel);

			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
			var result = TryDragBetweenCells(viewModel, sourceCell, destinationCell, cardContent);

			AssertEquals(true, result);
			AssertEquals(true, task1.IsInDatabase);
			AssertEquals(false, task2.IsInDatabase);
		}

		public void TestReassignmentToCapability()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var resource1 = CreateStaffInCurrentBranchDept("AAA", "Adam state");
			var capability1 = CreateCapability("AAA", "An Awesome Analogue");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Capability, capability1.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];

			var canTask = CreateTask(workflow, resource1.GS_Code, 90, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled, sequence: 1);
			var clsTask = CreateTask(workflow, resource1.GS_Code, 90, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2);
			var wrkTask = CreateTask(workflow, resource1.GS_Code, 90, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 4);
			var susTask = CreateTask(workflow, resource1.GS_Code, 90, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended, sequence: 3);
			var asnTask = CreateTask(workflow, resource1.GS_Code, 90, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 5);
			var opnTask = CreateTask(workflow, resource1.GS_Code, 90, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 6);

			Factory.Save();

			var visualBoardChannel1 = viewModel.CreateChannelForTest(resource1);
			var visualBoardChannel2 = viewModel.CreateChannelForTest(capability1);

			var sourceCell = new CellContent(0, 0, CellContentType.Cards) { Channel = visualBoardChannel1 };
			var destinationCell = new CellContent(0, 1, CellContentType.Cards) { Channel = visualBoardChannel2 };

			var wrkContent = new TaskCardContent(wrkTask, viewModel);

			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.AllTasksInWorkflow;
			var result = TryDragBetweenCells(viewModel, sourceCell, destinationCell, wrkContent);

			AssertEquals(true, result);

			AssertEquals(resource1.GS_Code, canTask.P9_GS_NKAssignedStaffMember);

			AssertEquals(resource1.GS_Code, clsTask.P9_GS_NKAssignedStaffMember);

			AssertEquals(resource1.GS_Code, wrkTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, wrkTask.P9_Status);

			AssertEquals(resource1.GS_Code, susTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, susTask.P9_Status);

			AssertEquals(ZString.Empty, asnTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(capability1.PK, asnTask.P9_G4_RequiredCapability);

			AssertEquals(ZString.Empty, opnTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(capability1.PK, opnTask.P9_G4_RequiredCapability);

			workflow.Reload();

			AssertEquals(8, workflow.Tasks.Count());
			AssertArrayEqualsByElements(new[] { ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Assigned }, workflow.Tasks.Where(t => t.P9_Sequence == 4).Select(t => t.P9_Status).Select(s => (string)s).ToArray());
		}

		public void TestReallocateTaskDoesNotSaveOriginalFactory_DeletedTask()
		{
			var boardConfig = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateViewModel(boardConfig.BucketSection);

			var resource1 = CreateStaffInCurrentBranchDept("AAA", "Adam state");
			var resource2 = CreateStaffInCurrentBranchDept("EEE", "EEEEEE");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(boardConfig.BucketSection, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task1 = CreateTask(workflow, resource1.GS_Code, 90);

			Factory.Save();

			var task2 = CreateTask(workflow, resource1.GS_Code, 90);
			AssertEquals(false, task2.IsInDatabase);

			var task1Content = new TaskCardContent(task1, viewModel);

			var visualBoardChannel1 = viewModel.CreateChannelForTest(resource1);
			var visualBoardChannel2 = viewModel.CreateChannelForTest(resource2);

			var sourceCell = new CellContent(0, 0, CellContentType.Cards) { Channel = visualBoardChannel1 };
			var destinationCell = new CellContent(0, 1, CellContentType.Cards) { Channel = visualBoardChannel2 };
			var cardContent = new TaskCardContent(task1, viewModel);

			var loadedTask = Factory.CreateNewFactory().Load<ProcessTask>(task1.PK);
			loadedTask.Delete();
			loadedTask.Factory.Save();

			dialogProvider.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
			var result = TryDragBetweenCells(viewModel, sourceCell, destinationCell, cardContent);
			AssertEquals(false, result);
			AssertEquals(true, task1.IsInDatabase);
			AssertEquals(false, task2.IsInDatabase);
		}

		public void TestDragDrop_JobWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var resource1 = CreateStaffInCurrentBranchDept("AAA", "A1");
			var resource2 = CreateStaffInCurrentBranchDept("EEE", "E1");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, 60, estVariationFactor: 1);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, 120, estVariationFactor: 1);

			Factory.Save();

			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(bucket1, board);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			var section2 = BMSTestHelper.CreateBoardSection(bucket2, board);
			section2.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			var viewModel2 = BMSTestHelper.CreateViewModel(section2);
			Factory.Save();

			var sourceCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var destinationCell = viewModel2.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);

			var cardContent = new WorkflowCardContent(workflow1, task1, viewModel);

			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			var result = TryDragBetweenCells(viewModel, sourceCell, destinationCell, cardContent);

			AssertEquals(false, result);
			var previousMessage = ZArchitecture.Environment.UnitTestUserNotification.Instance.PreviousMessages[0];
			AssertEquals("The destination component cannot be the same as the source component.", previousMessage.Text);

			result = TryDragBetweenCells(viewModel2, sourceCell, destinationCell, cardContent, viewModel);
			AssertEquals(true, result);
			previousMessage = ZArchitecture.Environment.UnitTestUserNotification.Instance.PreviousMessages[0];
			AssertEquals("Are you sure to move workflow Organization (XVBQP68SIYXQ) - Job Workflow from component [bucket1] to component [bucket2]? Note that based on transfer rules, your workflows may move back to their original components.", previousMessage.Text);
		}

		[GuiTest, TestDate(2019, 1, 1)]
		public void TestDragDrop_QualityIterationWorkflow_ShouldMoveIterationAndAncestorWorkflows()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			BMSTestHelper.AddTaskTypesToRegistry("ORG", "ONE", "TWO");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("TWO", "ORG");

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, taskType: "ONE", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, taskType: "TWO", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			var qiWorkflow1 = CreateQualityIteration(task1, task2, "qiWorkflow1");
			var qiWorkflow2 = CreateQualityIteration(qiWorkflow1.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow1.Tasks.Single(t => t.P9_Type == "TWO"), "qiWorkflow2");
			var qiWorkflow3 = CreateQualityIteration(qiWorkflow2.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow2.Tasks.Single(t => t.P9_Type == "TWO"), "qiWorkflow3");
			var qiWorkflow4 = CreateQualityIteration(qiWorkflow3.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow3.Tasks.Single(t => t.P9_Type == "TWO"), "qiWorkflow4");
			var qiWorkflow5 = CreateQualityIteration(qiWorkflow4.Tasks.Single(t => t.P9_Type == "ONE"), qiWorkflow4.Tasks.Single(t => t.P9_Type == "TWO"), "qiWorkflow5");

			Factory.Save();

			var context = config.Buffer.GetRelevantContext();
			var viewModel = BMSTestHelper.CreateViewModel(section);
			var sourceCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var destCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 2);
			var cardContent = new WorkflowCardContent(qiWorkflow3, qiWorkflow3.Tasks.First(), viewModel);

			const decimal initialPenetration = 0.125m;

			CombineAssertions("Initial buffer penetrations", () =>
			{
				AssertEquals("workflow", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory).Penetration);
				AssertEquals("qiWorkflow1", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow1, context, Factory).Penetration);
				AssertEquals("qiWorkflow2", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow2, context, Factory).Penetration);
				AssertEquals("qiWorkflow3", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow3, context, Factory).Penetration);
				AssertEquals("qiWorkflow4", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow4, context, Factory).Penetration);
				AssertEquals("qiWorkflow5", initialPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow5, context, Factory).Penetration);
			});

			CombineAssertions("Initial values of FH_ReleaseDateTime", () =>
			{
				AssertEquals("workflow", ZDateTime.UtcNow, workflow.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow1", ZDateTime.UtcNow, qiWorkflow1.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow2", ZDateTime.UtcNow, qiWorkflow2.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow3", ZDateTime.UtcNow, qiWorkflow3.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow4", ZDateTime.UtcNow, qiWorkflow4.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow5", ZDateTime.UtcNow, qiWorkflow5.FH_ReleaseDateTime);
			});

			AssertEquals(true, TryDragBetweenCells(viewModel, sourceCell, destCell, cardContent));
			Factory.Save();

			const decimal newPenetration = 0.2083333333333333333333333333m;

			CombineAssertions("After drag/drop, buffer penetration of all workflows should be updated", () =>
			{
				AssertEquals("workflow", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow, context, Factory).Penetration);
				AssertEquals("qiWorkflow1", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow1, context, Factory).Penetration);
				AssertEquals("qiWorkflow2", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow2, context, Factory).Penetration);
				AssertEquals("qiWorkflow3", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow3, context, Factory).Penetration);
				AssertEquals("qiWorkflow4", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow4, context, Factory).Penetration);
				AssertEquals("qiWorkflow5", newPenetration, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(qiWorkflow5, context, Factory).Penetration);
			});

			CombineAssertions("After drag/drop, only FH_ReleaseDateTime of the parent workflow should be updated", () =>
			{
				AssertEquals("workflow", new ZDateTime(2018, 12, 31, 0, 0, 0), workflow.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow1", ZDateTime.UtcNow, qiWorkflow1.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow2", ZDateTime.UtcNow, qiWorkflow2.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow3", ZDateTime.UtcNow, qiWorkflow3.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow4", ZDateTime.UtcNow, qiWorkflow4.FH_ReleaseDateTime);
				AssertEquals("qiWorkflow5", ZDateTime.UtcNow, qiWorkflow5.FH_ReleaseDateTime);
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestDragDrop_ChildWorkflow_ShouldMoveChildWorkflowOnly()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow1", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow2", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, 60);

			workflow2.GetOrCreateLinkToParent(workflow1);

			Factory.Save();

			var context = config.Buffer.GetRelevantContext();
			var viewModel = BMSTestHelper.CreateViewModel(section);
			var sourceCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 0);
			var destCell = viewModel.ComponentGrid.CardCells.Single(c => c.TimeIndex == 1);
			var cardContent = new WorkflowCardContent(workflow2, task1, viewModel);

			CombineAssertions("Initial buffer penetrations", () =>
			{
				AssertEquals("workflow1", 0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow1, context, Factory).Penetration);
				AssertEquals("workflow2", 0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow2, context, Factory).Penetration);
			});

			CombineAssertions("Initial values of FH_ReleaseDateTime", () =>
			{
				AssertEquals("workflow1", ZDateTime.UtcNow, workflow1.FH_ReleaseDateTime);
				AssertEquals("workflow2", ZDateTime.UtcNow, workflow2.FH_ReleaseDateTime);
			});

			AssertEquals(true, TryDragBetweenCells(viewModel, sourceCell, destCell, cardContent));
			Factory.Save();

			CombineAssertions("After drag/drop, only buffer penetration of the workflow that was actually drag/dropped should be updated", () =>
			{
				AssertEquals("workflow1", 0m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow1, context, Factory).Penetration);
				AssertEquals("workflow2", 0.125m, BufferPenetrationCalculator.CalculateNonCachedPenetrationPercentage(workflow2, context, Factory).Penetration);
			});

			CombineAssertions("After drag/drop, only FH_ReleaseDateTime of the workflow that was actually drag/dropped should be updated", () =>
			{
				AssertEquals("workflow1", ZDateTime.UtcNow, workflow1.FH_ReleaseDateTime);
				AssertEquals("workflow2", new ZDateTime(2018, 12, 28, 4, 0, 0), workflow2.FH_ReleaseDateTime);
			});
		}

		#region Implementation

		readonly MultiActionButtonDialogWrapper<CrossChannelTaskAssignments> dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();

		bool TryDragBetweenCells(BMBoardSectionViewModel viewModel, CellContent sourceCell, CellContent destinationCell, ICardContent cardContent, BMBoardSectionViewModel sourceSectionViewModel = null)
		{
			return new TicketDragDropHandler(Factory, viewModel, sourceCell, destinationCell, cardContent, dialogProvider, sourceSectionViewModel ?? viewModel).TryMoveToDestinationCell();
		}

		#endregion
	}
}
