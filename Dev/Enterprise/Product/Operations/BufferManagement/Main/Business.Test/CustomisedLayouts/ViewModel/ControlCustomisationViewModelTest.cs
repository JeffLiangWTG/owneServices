using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ControlCustomisationViewModelTest : BMSTestCaseWithFactory
	{
		public void TestTaskWasDeleted_InvokingTransformsShouldNotDoAnything()
		{
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var customisation = Factory.New<BMControlCustomisation>();
			var task = Factory.New<ProcessTask>();
			var header = Factory.New<ProcessHeader>();
			var taskPK = task.PK;

			var content = new FactorylessCardContent(header, task, sectionViewModel, new CustomisedControlDataCache(), new TagDefinitionCache(Factory), new PopulateTaskCardStrategy());
			var viewModel = new ControlCustomisationViewModel(Factory.New<BMControlCustomisation>(), sectionViewModel, content, new CellContent(0, 0, CellContentType.Cards));
			task.Delete();
			header.Delete();
			AssertEquals("Precondition: Avoid the filter.", false, viewModel.IsPreview);
			AssertNull("Precondition: Recreating the race condition.", viewModel.Task);
			AssertNoExceptionThrown(() => viewModel.UpdateStatus("CLS"));
			AssertNoExceptionThrown(() => viewModel.VoteUp(true));
			AssertNoExceptionThrown(() => viewModel.VoteDown(true));
			AssertNoExceptionThrown(() => viewModel.Save());
		}

		public void TestConstructor_ShouldSetIsPreview()
		{
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var customisation = Factory.New<BMControlCustomisation>();
			var task = Factory.New<ProcessTask>();
			var viewModel = new ControlCustomisationViewModel(customisation, new TaskCardContent(task, sectionViewModel));
			AssertEquals(true, viewModel.IsPreview);

			viewModel = new ControlCustomisationViewModel(Factory.New<BMControlCustomisation>(), sectionViewModel, new TaskCardContent(Factory.New<ProcessTask>(), sectionViewModel), new CellContent(0, 0, CellContentType.Cards), Factory.New<ProcessTask>());
			AssertEquals(false, viewModel.IsPreview);
		}

		public void TestSelectControl()
		{
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var customisation = Factory.New<BMControlCustomisation>();
			var task = Factory.New<ProcessTask>();
			var viewModel = new ControlCustomisationViewModel(customisation, new TaskCardContent(task, sectionViewModel));

			var line = customisation.CustomisationLines.AddNew();

			viewModel.SelectedControlChanged += (s, e) => AssertEquals(line, e.SelectedControl);
			viewModel.SelectControl(null, line);
		}

		[TestDate(2014, 3, 14)]
		public void TestOnCardSaveChannelIsUpdated()
		{
			var staff1 = CreateStaffInCurrentBranchDept("BEN", "ArgyBargy");
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var statusIndicatorLine = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);
			statusIndicatorLine.Orientation = "Vertical";

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			task.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			using (DisableAsyncBehaviour())
			{
				var sectionViewModel = BMSTestHelper.CreateViewModel(section);
				var boardViewModel = sectionViewModel.BoardViewModel;
				using (var subscriber = new VisualBoardDataRefreshBusSubscriber(boardViewModel, boardViewModel.FactoryProvider, boardViewModel.SlideShowViewModel.Dispatcher))
				{
					var cell = new CellContent(0, 0, CellContentType.Cards);

					cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

					var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), cell, task);
					viewModel.Save();

					AssertEquals("Idle", viewModel.Cell.Channel.Status);

					viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Working);
					viewModel.Save();

					AssertEquals("Working", viewModel.Cell.Channel.Status);
				}
			}
		}

		public void TestSave()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var statusIndicatorLine = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);
			statusIndicatorLine.Orientation = "Vertical";

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), cell, task);

			Factory.Save();

			AssertEquals(false, task.HasChanges);

			viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Closed);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals(true, task.HasChanges);

			viewModel.Save();
			AssertEquals(false, task.HasChanges);
		}

		public void TestSave_ShouldSetTaskStatusChangeModeToSCB()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.TaskStatusIndicator, string.Empty, 0, 0, 500, 500, "Black", "White", 8, false, false);

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			var cell = new CellContent(0, 0, CellContentType.Cards);
			cell.Channel = sectionViewModel.GetOrCreateChannelForTest(section.SectionConfiguration.PrimaryAxisChannels[0], section.Factory);

			var viewModel = new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), cell, task);

			Factory.Save();

			viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Working);
			viewModel.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			var lastLogReference = task.Logs.MostRecentLogByEventTime(ZArchitecture.Business.AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons}", lastLogReference);

			viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Suspended);
			viewModel.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task.P9_Status);
			lastLogReference = task.Logs.MostRecentLogByEventTime(ZArchitecture.Business.AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons}", lastLogReference);

			viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Closed);
			viewModel.Save();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			lastLogReference = task.Logs.MostRecentLogByEventTime(ZArchitecture.Business.AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.StatusControlButtons}", lastLogReference);
		}

		public void TestSave_WorkflowChanges()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Brambo", buffer);
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);
			var loadedCustomisation = newFactory.Load<BMControlCustomisation>(customisation.PK);
			var sectionViewModel = BMSTestHelper.CreateViewModel(loadedSection);
			var viewModel = new ControlCustomisationViewModel(loadedCustomisation, sectionViewModel, new TaskCardContent(loadedTask, sectionViewModel), sectionViewModel.ComponentGrid.Cells.First(), loadedTask);

			viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Closed);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, loadedTask.P9_Status);
			viewModel.Save();

			viewModel.Task.ProcessHeader.FH_CompletionStatement = string.Empty;
			AssertEquals(true, loadedTask.ProcessHeader.HasErrors());
			AssertEquals("No Errors Should Cascade to job level", false, loadedTask.ProcessHeader.JobHeader.HasErrors());
			viewModel.Save();

			AssertEquals(@"Errors on Workflow:  [Organization (XVBQP68SIYXQ) - ]:
Error - FH_CompletionStatement: Please enter a Description.

", UnitTestUserNotification.Instance.LastMessage.Text);
			viewModel.Task.ProcessHeader.FH_CompletionStatement = "Dat workflow";
			AssertEquals(false, loadedTask.ProcessHeader.HasErrors());
			AssertEquals("No Errors Should Cascade to job level", false, loadedTask.ProcessHeader.JobHeader.HasErrors());

			AssertNoExceptionThrown(() => { viewModel.Save(); });
		}

		public void TestSave_JobChanges()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Brambo", buffer);
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);
			var loadedCustomisation = newFactory.Load<BMControlCustomisation>(customisation.PK);
			var sectionViewModel = BMSTestHelper.CreateViewModel(loadedSection);
			var viewModel = new ControlCustomisationViewModel(loadedCustomisation, sectionViewModel, new TaskCardContent(loadedTask, sectionViewModel), sectionViewModel.ComponentGrid.Cells.First(), loadedTask);

			viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Closed);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, loadedTask.P9_Status);
			viewModel.Save();

			viewModel.Task.ProcessHeader.JobHeader.FH_CompletionStatement = string.Empty;
			AssertEquals(true, loadedTask.ProcessHeader.JobHeader.HasErrors());
			viewModel.Save();

			AssertEquals(@"Errors on Job:  [Organization (XVBQP68SIYXQ) - ]:
Error - FH_CompletionStatement: Please enter a Description.

", UnitTestUserNotification.Instance.LastMessage.Text);
			viewModel.Task.ProcessHeader.JobHeader.FH_CompletionStatement = "Dat Workflow";
			AssertEquals(false, loadedTask.ProcessHeader.JobHeader.HasErrors());

			AssertNoExceptionThrown(() => { viewModel.Save(); });
		}

		public void TestSave_NotificationsOnParent()
		{
			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");

			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();

			var workflow = CreateWorkflow(jobHeader, "Brambo", buffer);
			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var loadedSection = newFactory.Load<BMBoardSection>(section.PK);
			var loadedTask = newFactory.Load<ProcessTask>(task.PK);
			var loadedCustomisation = newFactory.Load<BMControlCustomisation>(customisation.PK);
			var org = (OrgHeader)loadedTask.Parent;
			org.OH_Language = "PIX";
			var sectionViewModel = BMSTestHelper.CreateViewModel(loadedSection);
			var viewModel = new ControlCustomisationViewModel(loadedCustomisation, sectionViewModel, new TaskCardContent(loadedTask, sectionViewModel), sectionViewModel.ComponentGrid.Cells.First(), loadedTask);

			viewModel.UpdateStatus(ProcessTaskStatusCodeList.Codes.Closed);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, loadedTask.P9_Status);
			viewModel.Save();

			AssertHasError(org.OH_LanguageInfo, "Enter a valid Language.");

			AssertEquals(@"Errors on Organization (XVBQP68SIYXQ) []:
Error - OH_Language: Enter a valid Language.

", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestClose()
		{
			var viewModel = GetViewModel(Factory);
			viewModel.Closed += (s, e) => Assert(true);

			viewModel.Close();
		}

		public void TestShowParent_Task()
		{
			var viewModel = GetViewModel(Factory);
			viewModel.ParentShown += (s, e) => Assert(e.Parent is ProcessTask);
			viewModel.ShowParent();
		}

		public void TestShowParent_Workflow()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var sectionViewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(CreateBucket(CreateSystem())));
			var viewModel = GetViewModel(Factory, sectionViewModel, new WorkflowCardContent(workflow, task, sectionViewModel), new CellContent(0, 0, CellContentType.Cards));

			viewModel.ParentShown += (s, e) => AssertType<ProcessHeader>(e.Parent);
			viewModel.ShowParent();
		}

		public void TestVoteUpDown_Workflow()
		{
			var viewModel = GetViewModel(Factory);
			var workflow = viewModel.CardContent.GetWorkflow(Factory);
			var jobHeader = workflow.JobHeader;

			viewModel.VoteUp(jobCardsShown: false);
			viewModel.VoteUp(jobCardsShown: false);
			AssertEquals(new ZShort(2), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(0), jobHeader.FH_VoteUpDownAmount);

			viewModel.VoteDown(jobCardsShown: false);
			AssertEquals(new ZShort(1), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(0), jobHeader.FH_VoteUpDownAmount);
		}

		public void TestVoteUpDown_JobWorkflow()
		{
			var viewModel = GetViewModel(Factory);
			var workflow = viewModel.CardContent.GetWorkflow(Factory);
			var jobHeader = workflow.JobHeader;

			viewModel.VoteUp(jobCardsShown: true);
			viewModel.VoteUp(jobCardsShown: true);
			AssertEquals(new ZShort(0), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(2), jobHeader.FH_VoteUpDownAmount);

			viewModel.VoteDown(jobCardsShown: true);
			AssertEquals(new ZShort(0), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(1), jobHeader.FH_VoteUpDownAmount);
		}

		public void TestSave_Preview()
		{
			var viewModel = GetViewModel(Factory, true);
			Factory.Save();

			var task = viewModel.CardContent.GetTask(Factory);
			AssertEquals(false, task.HasChanges);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals(true, task.HasChanges);

			viewModel.Save();
			AssertEquals(true, task.HasChanges);
		}

		public void TestClose_Preview()
		{
			var wasClosed = false;

			var viewModel = GetViewModel(Factory, true);
			viewModel.Closed += (s, e) => wasClosed = true;

			viewModel.Close();

			AssertEquals(false, wasClosed);
		}

		public void TestShowParent_Task_Preview()
		{
			var wasParentShown = false;

			var viewModel = GetViewModel(Factory, true);
			viewModel.ParentShown += (s, e) => wasParentShown = true;
			viewModel.ShowParent();

			AssertEquals(false, wasParentShown);
		}

		public void TestShowParent_Workflow_Preview()
		{
			var wasParentShown = false;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var sectionViewModel = BMSTestHelper.CreateViewModel(CreateBoardSection(CreateBucket(CreateSystem())));
			var viewModel = GetViewModel(Factory, sectionViewModel, new WorkflowCardContent(workflow, task, sectionViewModel));

			viewModel.ParentShown += (s, e) => wasParentShown = true;
			viewModel.ShowParent();

			AssertEquals(false, wasParentShown);
		}

		public void TestVoteUpDown_Workflow_Preview()
		{
			var viewModel = GetViewModel(Factory, true);
			var workflow = viewModel.CardContent.GetWorkflow(Factory);
			var jobHeader = workflow.JobHeader;

			viewModel.VoteUp(jobCardsShown: false);
			viewModel.VoteUp(jobCardsShown: false);
			AssertEquals(new ZShort(0), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(0), jobHeader.FH_VoteUpDownAmount);

			viewModel.VoteDown(jobCardsShown: false);
			AssertEquals(new ZShort(0), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(0), jobHeader.FH_VoteUpDownAmount);
		}

		public void TestVoteUpDown_JobWorkflow_Preview()
		{
			var viewModel = GetViewModel(Factory, true);
			var workflow = viewModel.CardContent.GetWorkflow(Factory);
			var jobHeader = workflow.JobHeader;

			viewModel.VoteUp(jobCardsShown: true);
			viewModel.VoteUp(jobCardsShown: true);
			AssertEquals(new ZShort(0), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(0), jobHeader.FH_VoteUpDownAmount);

			viewModel.VoteDown(jobCardsShown: true);
			AssertEquals(new ZShort(0), workflow.FH_VoteUpDownAmount);
			AssertEquals(new ZShort(0), jobHeader.FH_VoteUpDownAmount);
		}

		public static ControlCustomisationViewModel GetViewModel(BusinessObjectFactory factory, bool isPreview = false)
		{
			var system = BMSTestHelper.CreateSystem(factory);
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(system));
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);
			var workflow = ProcessJobHeader.GetForParent(factory.NewWithValidTestData<OrgHeader>(), factory).ProcessHeaders.AddNew();
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var cardContent = new TaskCardContent(task, sectionViewModel);

			return GetViewModel(factory, sectionViewModel, cardContent, isPreview ? null : new CellContent(0, 0, CellContentType.Cards));
		}

		public static ControlCustomisationViewModel GetViewModel(BMControlCustomisation customisation, CellContent cell = null)
		{
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(customisation.Factory);
			var workflow = ProcessJobHeader.GetForParent(customisation.Factory.NewWithValidTestData<OrgHeader>(), customisation.Factory).ProcessHeaders.AddNew();
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var cardContent = new TaskCardContent(task, sectionViewModel);

			return cell != null
				? new ControlCustomisationViewModel(customisation, sectionViewModel, new TaskCardContent(task, sectionViewModel), cell, task)
				: new ControlCustomisationViewModel(customisation, new TaskCardContent(task, sectionViewModel));
		}

		public static ControlCustomisationViewModel GetViewModel(BusinessObjectFactory factory, BMBoardSectionViewModel sectionViewModel, WorkflowCardContent cardContent, CellContent cell = null)
		{
			var customisation = factory.NewWithValidTestData<BMControlCustomisation>();

			return cell != null
				? new ControlCustomisationViewModel(customisation, sectionViewModel, cardContent, cell, cardContent.Task)
				: new ControlCustomisationViewModel(customisation, cardContent);
		}

		public static ControlCustomisationViewModel GetViewModel(BusinessObjectFactory factory, BMBoardSectionViewModel sectionViewModel, TaskCardContent cardContent, CellContent cell = null)
		{
			var customisation = factory.NewWithValidTestData<BMControlCustomisation>();

			return cell != null
				? new ControlCustomisationViewModel(customisation, sectionViewModel, cardContent, cell, cardContent.Task)
				: new ControlCustomisationViewModel(customisation, cardContent);
		}
	}
}
