using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	#region DataRefreshEnabledTest

	class CellTasksControl_WhenDataRefreshEnabledTest : CellTasksControl_BaseTest
	{
		// Tests go in the base class. This is here to ensure data refresh bus functionality works as expected.

		protected override void SetUp()
		{
			base.SetUp();

			AssertEquals("UpdateTicketsWithDataRefresh should default to enabled", true, BMSRegistry.Instance.UpdateTicketsWithDataRefresh.Value);
		}
	}

	#endregion

	#region DataRefreshDisabledTest

	class CellTasksControl_WhenDataRefreshDisabledTest : CellTasksControl_BaseTest
	{
		// Tests go in the base class. This is here to ensure ticket update works when data refresh bus is disabled.

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}

	#endregion

	abstract class CellTasksControl_BaseTest : BMSGUITestCase
	{
		#region Runtime Filters

		public void TestSetupTasks_IfFilterIsAppliedShouldFilterBeforeChoosingWhich100()
		{
			var viewModel = GetViewModel(config.Bucket, config.ReleaseGroup);

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = CreateWorkflow(jobHeader, "Noop", Factory.Load<BMComponent>(viewModel.ComponentPK));
			var oneHundredAndOneTasks = Enumerable.Range(0, 101).Select(n => CreateTask(workflow, string.Empty, 20, description: "t" + n)).ToArray();
			var lastTask = oneHundredAndOneTasks[100];
			var description = "translucent";
			lastTask.P9_Description = description;
			Factory.Save();

			var filter1 = new SearchFilter(description);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel.BoardViewModel.SlideShowViewModel))
			{
				form.Show();
				Application.DoEvents();

				form.BoardViewModel.FilterManager.ApplyFilter(filter1);

				var card = form.FindAll<TaskCardControl>().Single();
				AssertEquals(lastTask.PK, card.CardContent.TaskIdentifier);

				var cellTaskButton = form.FindAll<ZButton>().Single(z => z.Name == "TotalTasksButton");
				cellTaskButton.PerformClick();
				Application.DoEvents();

				var cellTasksControl = form.FindAll<CellTasksControl>().Single();
				card = cellTasksControl.FindAll<TaskCardControl>().Single();

				AssertEquals(lastTask.PK, card.CardContent.TaskIdentifier);
			}
		}

		public void TestSetupTasks_IfVisibilityFilterIsAppliedShouldFilterCorrectly()
		{
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var job = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(job, "Dirty little secret");
			var task = BMSTestHelper.CreateTask(workflow, description: "Baby you can't see me I'm translucent");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			viewModel.FilterManager.ApplyFilter(new CurrentTaskFilter());

			using (var control = BMSGUITestCase.CreateCellTasksControl(new CellContent(0, 0, CellContentType.Cards), new TaskCardContent[] { new TaskCardContent(task, viewModel) }, viewModel))
			{
				control.SetupTasks();

				var card = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().Single();

				AssertEquals("Baby you can't see me I'm translucent", ((BusinessObject)card.CardContent.Bindable)[ProcessTasksSchema.Constants.P9_Description]);
			}
		}

		public void TestSetupTasks_IfBoardMeetingModeFilterIsAppliedShouldFilterCorrectly()
		{
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var job = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(job, "Dirty little secret");
			var task = BMSTestHelper.CreateTask(workflow, description: "Baby you can't see me I'm translucent");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			viewModel.FilterManager.ApplyFilter(new BoardMeetingModeFilter());

			var cell = new CellContent(0, 0, CellContentType.Cards);
			cell.Zone = 0;

			using (var control = BMSGUITestCase.CreateCellTasksControl(cell, new TaskCardContent[] { new TaskCardContent(task, viewModel) }, viewModel))
			{
				control.SetupTasks();

				var card = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().Single();

				AssertEquals("Baby you can't see me I'm translucent", ((TaskCardContent)card.CardContent).Task.P9_Description);
			}
		}

		#endregion

		#region Edge-case Handling

		public void TestCloseControlWithNullParent()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Noop", config.Bucket);
			var tasks = Enumerable.Range(0, 1).Select(_ => workflow.TaskCollection.AddNew()).ToArray();

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection, workflow);

			using (var control = BMSGUITestCase.CreateCellTasksControl(new CellContent(0, 0, CellContentType.Cards), tasks.Select(t => new TaskCardContent(t, viewModel)), viewModel))
			{
				AssertNoExceptionThrown(() => control.SetupTasks());
				AssertNoExceptionThrown(() => control.CloseTaskCardButton.PerformClick());
			}
		}

		#endregion

		#region Task Layout

		public void TestSetupTasks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow.");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);
			var task3 = BMSTestHelper.CreateTask(workflow);
			var task4 = BMSTestHelper.CreateTask(workflow);
			var task5 = BMSTestHelper.CreateTask(workflow);

			task1.P9_CardNote = "Something";
			task4.P9_CardNote = "Else";

			Factory.Save();

			var viewModel = GetViewModel(config.Bucket, config.ReleaseGroup);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(viewModel.BoardViewModel.SlideShowViewModel))
			{
				var cellTaskButton = form.FindAll<ZButton>().Single(z => z.Name == "TotalTasksButton");
				cellTaskButton.PerformClick();
				Application.DoEvents();

				var control = form.FindAll<CellTasksControl>().Single();
				control.SetupTasks();

				AssertEquals(5, control.TaskCardsPanel.Controls.Count);
				var cards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				var scaledPadding = ControlDpiScalingHelper.ScaleToCurrentDpiY(CellTasksControl.CardPadding);

				AssertEquals(Point.Empty, cards[0].Location);
				AssertEquals(new Point(0, cards[0].Height + scaledPadding), cards[1].Location);
				AssertEquals(new Point(0, cards[1].Top + cards[1].Height + scaledPadding), cards[2].Location);
				AssertEquals(new Point(0, cards[2].Top + cards[2].Height + scaledPadding), cards[3].Location);
				AssertEquals(new Point(0, cards[3].Top + cards[3].Height + scaledPadding), cards[4].Location);
			}
		}

		public void TestSetupTasks_ShouldCloseControlWhenNothingIsToShow()
		{
			var viewModel = GetViewModel(config.Bucket, config.ReleaseGroup);

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = CreateWorkflow(jobHeader, "Noop", Factory.Load<BMComponent>(viewModel.ComponentPK));
			Enumerable.Range(0, 3).Select(n => CreateTask(workflow, string.Empty, 20, description: "t" + n)).ToArray();

			Factory.Save();

			var filter1 = new SearchFilter("text not exists");

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel.BoardViewModel.SlideShowViewModel))
			{
				form.Show();
				Application.DoEvents();

				var cellTaskButton = form.FindAll<ZButton>().SingleOrDefault(z => z.Name == "TotalTasksButton");
				AssertNotNull(cellTaskButton);

				form.BoardViewModel.FilterManager.ApplyFilter(filter1);
				Application.DoEvents();

				cellTaskButton = form.FindAll<ZButton>().SingleOrDefault(z => z.Name == "TotalTasksButton");
				AssertNull(cellTaskButton);
			}
		}

		public void TestSetupTasks_ShouldNotShrinkWidthASecondTime()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Noop", config.Bucket);
			var task1 = workflow.TaskCollection.AddNew();

			Factory.Save();

			var cell = new CellContent(0, 0, CellContentType.Cards) { Label = "Day 1 - DJ Davey Dave" };
			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection, workflow);

			using (var form = new ZForm())
			using (var control = BMSGUITestCase.CreateCellTasksControl(cell, new List<ProcessTask> { task1 }.Select(t => new TaskCardContent(t, viewModel)), viewModel))
			{
				form.Show();
				control.SetupTasks();
				form.Controls.Add(control);

				AssertEquals("Day 1 - DJ Davey Dave", control.ChannelDayHeaderLabel.Text);
				AssertEquals(1, control.TaskCardsPanel.Controls.Count);
				var cards = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(Point.Empty, cards[0].Location);

				control.SetupTasks();
				var startingWidth = control.Width;

				int expectedSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(255);
				AssertCloseEnough(expectedSize, startingWidth);

				control.SetupTasks();
				AssertCloseEnough(startingWidth, control.Width);
			}
		}

		public void TestTasksHiddenByFilter_ShouldNotAffectHeight()
		{
			var viewModel = GetViewModel(config.Bucket, config.ReleaseGroup);

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = CreateWorkflow(jobHeader, "Noop", Factory.Load<BMComponent>(viewModel.ComponentPK));
			var tasks = Enumerable.Range(0, 2).Select(n => CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20, description: "t" + n)).ToArray();

			AssertEquals(true, tasks[0].IsStartable());
			AssertEquals(false, tasks[1].IsStartable());

			Factory.Save();

			var filter1 = new HideCurrentTasksFilter();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(viewModel.BoardViewModel.SlideShowViewModel))
			{
				var cellTaskButton = form.FindAll<ZButton>().Single(z => z.Name == "TotalTasksButton");
				cellTaskButton.PerformClick();
				Application.DoEvents();

				var cellTasksControl = form.FindAll<CellTasksControl>().Single();
				ControlTestHelper.AssertControlHeight(150, cellTasksControl);
				AssertEquals(2, cellTasksControl.TaskCardsPanel.Controls.Count);

				form.BoardViewModel.FilterManager.ApplyFilter(filter1);
				Application.DoEvents();

				cellTasksControl = form.FindAll<CellTasksControl>().Single();
				ControlTestHelper.AssertControlHeight(88, cellTasksControl);
				AssertEquals(1, cellTasksControl.TaskCardsPanel.Controls.Count);
			}
		}

		/**
		 * FYI: This test behaves differently when run from the VS test runner
		 * and the CW1 test runner. This is because it relies on forms being
		 * open and active. It works correctly in the CW1 runner and DAT by 
		 * failing when appropriate. In the VS runner it always passes. 
		 */
		public void TestSetupTasks_UnaffectedByMessageBox()
		{
			var heightWithoutMessageBox = 0;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow.");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);
			var task3 = BMSTestHelper.CreateTask(workflow);
			var task4 = BMSTestHelper.CreateTask(workflow);
			var task5 = BMSTestHelper.CreateTask(workflow);

			task1.P9_CardNote = "Something";
			Factory.Save();

			var viewModel = GetViewModel(config.Bucket, config.ReleaseGroup);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(viewModel.BoardViewModel.SlideShowViewModel))
			{
				var cellTaskButton = form.FindAll<ZButton>().Single(z => z.Name == "TotalTasksButton");
				cellTaskButton.PerformClick();
				Application.DoEvents();

				var control = form.FindAll<CellTasksControl>().Single();
				control.SetupTasks();
				Application.DoEvents();

				heightWithoutMessageBox = control.Height;
			}

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(viewModel.BoardViewModel.SlideShowViewModel))
			{
				var cellTaskButton = form.FindAll<ZButton>().Single(z => z.Name == "TotalTasksButton");

				cellTaskButton.PerformClick();
				Application.DoEvents();

				using (var messageBox = new ZMessageBox("Ribbit", "Extra extra", MessageBoxButtons.OK, MessageBoxIcon.Information))
				{
					messageBox.Show();
					Application.DoEvents();

					var control = form.FindAll<CellTasksControl>().Single();
					control.SetupTasks();
					Application.DoEvents();

					AssertEquals("Height is same with or without messagebox present", heightWithoutMessageBox, control.Height);
				}
			}
		}

		#endregion

		#region Performance

		public void TestSetupTasks_ShouldNotShowMoreThan100Tasks()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = CreateWorkflow(jobHeader, "Noop", Factory.Load<BMComponent>(config.Bucket.PK));
			var oneHundredAndOneTasks = Enumerable.Range(0, 101).Select(_ => workflow.TaskCollection.AddNew()).ToArray();

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			Factory.ResetDatabaseLoadCount();
			Factory.NameForDebugging = "Fake SetupTasks factory";

			using (AssertDbHitsForAllFactories(new Dictionary<string, int> { }, ignoreUnspecified: true, thresholdForUnspecified: 6, includeFactoryPredicate: f => Business.Test.BMSTestHelper.IsPAVEFactory(f)))
			using (var control = BMSGUITestCase.CreateCellTasksControl(new CellContent(0, 0, CellContentType.Cards), oneHundredAndOneTasks.Select(t => new TaskCardContent(t, viewModel)), viewModel))
			{
				control.SetupTasks();

				AssertEquals(100, control.TaskCardsPanel.Controls.OfType<TaskCardControl>().Count());
			}
		}

		public void TestSaveDetailedTaskCard_ShouldNotReloadTaskListMultipleTimes()
		{
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Fragle", releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				form.FindSingle<TaskPanel>().FindSingle<ZButton>().PerformClick();

				var control = form.Controls.OfType<CellTasksControl>().Single();
				AssertEquals("Called the first time when task list is shown", 1, control.SetupCardsCalledCount);

				var card = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().Single();

				AssertChangesinTaskCardAndTaskCardListRefreshCount(card, control, "Some Dummy Card Note");
			}

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				form.FindSingle<TaskPanel>().FindSingle<ZButton>().PerformClick();

				var control = form.Controls.OfType<CellTasksControl>().Single();
				AssertEquals("Called the first time when task list is shown", 1, control.SetupCardsCalledCount);

				var card = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().Single();

				AssertChangesinTaskCardAndTaskCardListRefreshCount(card, control, "Another Dummy Card Note");
			}
		}

		public void TestSaveDetailedTaskCard_ClaimATask_ShouldNotReloadTaskListMultipleTimes()
		{
			var capability = CreateCapability("ABC", "Dummy capability");
			var staff1 = CreateStaffInCurrentBranchDept("SME", "Smeagol", capability);
			var staff2 = CreateStaffInCurrentBranchDept("FRO", "Frodo", capability);
			var staff3 = CreateStaffInCurrentBranchDept("BOR", "Boromir", capability);
			config.ReleaseGroup.Staff.AddRange(staff1, staff2, staff3);

			var section = config.BufferSection;

			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, "", 0, capability: capability, description: "Task 1");

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (KUserControl.TrackInstantiatedControls_ForTest())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

				var taskPanelWithTaskCard = form.FindAll<TaskPanel>(p => p.Cell.Channel.ChannelEntityCode == "SME").Last(); // The only task in Semagol's channel
				taskPanelWithTaskCard.FindSingle<ZButton>().PerformClick();
				var control = form.Controls.OfType<CellTasksControl>().Single();
				AssertEquals("Called the first time when task list is shown", 1, control.SetupCardsCalledCount);

				var card = control.TaskCardsPanel.Controls.OfType<TaskCardControl>().Single();
				card.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				var claimButton = detailedTicket.FindSingle<CapabilityAssignmentButton>();

				AssertEquals(4, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);

				claimButton.DialogWrapperForTest.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTaskForClaimOrAssign;
				claimButton.PerformClick();
				detailedTicket.Save_ForTest();

				AssertEquals("SME", task.P9_GS_NKAssignedStaffMember);
				AssertEquals("Called the second time after detailed card is saved and data refresh bus pushes the changes to the task list", 2, control.SetupCardsCalledCount);
				AssertEquals(6, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);
			}
		}

		void AssertChangesinTaskCardAndTaskCardListRefreshCount(TaskCardControl card, CellTasksControl control, string text)
		{
			card.ShowDetailedCard();
			var parent = card.Parent;

			var detailedCardForm = card.FindForm();
			var detailedCard = detailedCardForm.FindAll<TaskCardDetailControl>().Single();
			detailedCard.ProcessTask.P9_CardNote = text;

			detailedCard.Save_ForTest();

			card = parent.Controls.OfType<TaskCardControl>().Single(c => c.CardContent.Identifier == card.CardContent.Identifier);
			AssertEquals("Called the second time after detailed card is saved", 2, control.SetupCardsCalledCount);
			AssertEquals("Summary card is also updated", text, card.CardContent.NoteText);
		}

		#endregion

		#region Filter Strips

		public void TestWorkflowCards_SectionConfigurationFiltersShouldBeApplied()
		{
			//Create a board with two identical board sections for the same component
			var bucket = config.Bucket;
			var section1 = config.BucketSection;
			section1.Row = 2;
			section1.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var section2 = BMSTestHelper.CreateBoardSection(bucket, config.BucketBoard);
			section2.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section2.BackgroundColor = Color.Azure.Name;

			//the difference between sections is: second section has a filter on it
			FilterStripsTestHelper.AddFilterStrips(section2.WorkflowFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.WorkflowStatus,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = WorkflowStatusList.Codes.Blocked
			});

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var task1 = BMSTestHelper.CreateTask(workflow1);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var task2 = BMSTestHelper.CreateTask(workflow2);

			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2Child");
			var task3 = BMSTestHelper.CreateTask(workflow2Child);
			workflow2.GetOrCreateDependencyLink(workflow2Child);

			Factory.Save();

			var viewModel = VisualBoards.GUI.Test.VisualBoardFormBasherTest.GetViewModel(config.BucketBoard);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var sectionControls = form.FindAll<BMComponentControl>();
				AssertEquals(2, sectionControls.Count());

				// this section should show only blocked workflows
				var bmComponentControl1 = sectionControls.ElementAt(0);
				var taskCards = bmComponentControl1.FindAll<TaskCardControl>();
				AssertEquals(1, taskCards.Count());
				var card1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task3.PK);
				AssertNotNull(card1);

				// this section should show startable workflows as well as blocked workflows
				var bmComponentControl2 = sectionControls.ElementAt(1);
				taskCards = bmComponentControl2.FindAll<TaskCardControl>();
				AssertEquals(3, taskCards.Count());
				var cardWorkflow1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task1.PK);
				AssertNotNull(card1);

				var cardWorkflow2 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task2.PK);
				AssertNotNull(cardWorkflow2);

				var cardWorkflow2Child = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task3.PK);
				AssertNotNull(cardWorkflow2Child);

				// edit something on opened workflow detailed job card
				cardWorkflow1.ShowDetailedCard();
				var taskDetailedCard = form.FindAll<TaskCardDetailControl>().Single();

				var textBox = taskDetailedCard.FindAll<ZTextBox>().Single(t => t.Text == "workflow1");
				textBox.Focus();
				textBox.Text = "workflow1...test";
				Application.DoEvents();

				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.S);

				Application.DoEvents();

				taskCards = bmComponentControl1.FindAll<TaskCardControl>();
				// after save completed, 'workflow1' card should not be shown on first component section, 
				AssertEquals(1, taskCards.Count());
				card1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task3.PK);
				AssertNotNull(card1);
			}
		}

		public void TestTaskCards_SectionConfigurationFiltersShouldBeApplied()
		{
			//Create a board with two identical board sections for the same component
			var bucket = config.Bucket;
			var section1 = config.BucketSection;
			section1.Row = 2;
			section1.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var section2 = BMSTestHelper.CreateBoardSection(bucket, config.BucketBoard);
			section2.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			section2.BackgroundColor = Color.Azure.Name;

			//the difference between sections is: second section has a task filter on it
			FilterStripsTestHelper.AddFilterStrips(section2.TaskFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Description",
				FilterStripValueSetter = filter => ((ModuleTextFilter)filter).Property = "Task 1"
			});

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var task1 = BMSTestHelper.CreateTask(workflow1, description: "Task 1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var task2 = BMSTestHelper.CreateTask(workflow2, description: "Task 2");

			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2Child");
			var task3 = BMSTestHelper.CreateTask(workflow2Child, description: "Task 3");

			Factory.Save();

			var viewModel = VisualBoards.GUI.Test.VisualBoardFormBasherTest.GetViewModel(config.BucketBoard);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var sectionControls = form.FindAll<BMComponentControl>();
				AssertEquals(2, sectionControls.Count());

				// this section should show only tasks with description 'Task 1' - should be 1 task
				var bmComponentControl1 = sectionControls.ElementAt(0);
				var taskCards = bmComponentControl1.FindAll<TaskCardControl>();
				AssertEquals(1, taskCards.Count());
				var card1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task1.PK);
				AssertNotNull(card1);

				// this section should show all tasks - no filters applied
				var bmComponentControl2 = sectionControls.ElementAt(1);
				taskCards = bmComponentControl2.FindAll<TaskCardControl>();
				AssertEquals(3, taskCards.Count());
				var cardWorkflow1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task1.PK);
				AssertNotNull(card1);

				var cardWorkflow2 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task2.PK);
				AssertNotNull(cardWorkflow2);

				var cardWorkflow2Child = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task3.PK);
				AssertNotNull(cardWorkflow2Child);

				// edit something on task 2
				cardWorkflow2.ShowDetailedCard();
				var taskDetailedCard = form.FindAll<TaskCardDetailControl>().Single();

				var textBox = taskDetailedCard.FindAll<ZTextBox>().Single(t => t.Text == "Task 2");
				textBox.Focus();
				textBox.Text = "Task 2...test";
				Application.DoEvents();

				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.S);

				Application.DoEvents();

				taskCards = bmComponentControl1.FindAll<TaskCardControl>();
				// after save completed, only one task card 'Task 1' should be shown, because task filter applied
				AssertEquals(1, taskCards.Count());
				card1 = taskCards.SingleOrDefault(c => c.CardContent.TaskIdentifier == task1.PK);
				AssertNotNull(card1);
			}
		}

		#endregion

		#region Bitmap Rendering

		#if !WINZOR  // Bitmap rendering is not used for Winzor, so the test does not apply.
		public void TestBitmapsRendering_ShouldUseDifferentBitmapFromSourceTicket()
		{
			var section = config.BucketSection;
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "I'll keep track of everyone's food, you know, in exchange for food.", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, description: "That's not a real job!");

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(section))
			{
				var panel = form.FindSingle<TaskPanel>();
				var ticket = panel.FindSingle<TaskCardControl>();

				var cellTasksControl = ShowAndGetCellTasksControl(panel);
				var cellTasksTicket = cellTasksControl.FindSingle<TaskCardControl>();

				AssertNotNull(ticket.BackgroundImage);
				AssertNotNull(cellTasksTicket.BackgroundImage);

				AssertNotEquals("Shouldn't use the same image for the cell task list ticket since the controls are a different size", ticket.BackgroundImage, cellTasksTicket.BackgroundImage);
			}
		}
		#endif

		#endregion

		#region Implementation

		BMBoardSectionViewModel GetViewModel(BMComponent component, GlbGroup releaseGroup = null)
		{
			var releaseGroupPK = releaseGroup?.PK ?? ZGuid.Empty;
			return BMSTestHelper.CreateSectionAndViewModel(component, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled, showZones: null, releaseGroup: releaseGroupPK).Item2;
		}

		VisualBoardTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
		}

		#endregion
	}
}
