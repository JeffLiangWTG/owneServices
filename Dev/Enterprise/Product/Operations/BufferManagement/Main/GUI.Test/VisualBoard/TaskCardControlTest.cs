using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Billing.Business.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TaskCardControlTest : BMSGUITestCase
	{
		#region Move To Component

		public void TestMoveToComponent_WhenComponentIsActive()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			bucket1.FC_IsActive = true;

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var board = BMSTestHelper.CreateBoard(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = BMSTestHelper.CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket2, releaseGroupPK: group.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();

				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().First(t => t.CardContent.TaskIdentifier == task1.PK);

				((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).ShowDropDown();

				// this cause not-disposed on multi-thread test
				AssertEquals(1, ((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).DropDownItems.Count);
				AssertEquals("bucket1", ((BMComponent)((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).DropDownItems[0].Tag).FC_Name);
				AssertEquals(true, ((BMComponent)((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).DropDownItems[0].Tag).FC_IsActive);
				AssertEquals(true, ((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).DropDownItems[0].Enabled);
			}
		}

		public void TestMoveToComponent_WhenComponentIsInactive()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			bucket1.FC_IsActive = false;

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var board = BMSTestHelper.CreateBoard(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = BMSTestHelper.CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket2, releaseGroupPK: group.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();

				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().First(t => t.CardContent.TaskIdentifier == task1.PK);

				((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).ShowDropDown();

				// this cause not-disposed on multi-thread test
				AssertEquals(1, ((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).DropDownItems.Count);
				AssertNull(((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).DropDownItems[0].Tag);
				AssertEquals(false, ((ToolStripMenuItem)taskCard.ContextMenuStrip.Items["MoveToComponentToolStripMenuItem"]).DropDownItems[0].Enabled);
			}
		}

		#endregion

		#region Customised Layout

		public void TestCustomisation_WithMacroAndDotNotation()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG", "INQ");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var org = jobHeader.Parent as OrgHeader;

			AssertEquals(org.CompanyData.Company.GC_Code, "EDI");

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", releaseGroupPK: group.PK);
			var task = BMSTestHelper.CreateTask(workflow);
			var layout = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);
			layout.Height *= 2;

			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, "<CompanyData.Company.GC_Code>", PropertyTypeList.Codes.Text, "Code", 100, 0, 100, 20, Color.White.Name, Color.Black.Name, 8, false, true, true);

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(section.Board);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var ticket = BMSGUITestCase.FindTaskCardControl(form, task);

				AssertNotNull(ticket);
			}
		}

		public void TestBackColor_ShouldUseCustomisationWhenNoTagApplied()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "PNK", color: Color.HotPink);
			tag.ApplyColorToBackground = true;

			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			customisation.FM_Name = "Mai Task Crad";
			customisation.BackgroundColor = ColorList.NameFromColor(Color.LimeGreen);

			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.AddTag(tag);
			workflow1.FH_GG_ReleaseGroup = group.PK;
			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_GG_ReleaseGroup = group.PK;
			var task2 = workflow2.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item1;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);

			using (var task1Card = new TaskCardControl(task1, viewModel))
			using (var task2Card = new TaskCardControl(task2, viewModel))
			{
				AssertEquals(Color.HotPink, task1Card.BackColor);
				AssertEquals(Color.LimeGreen, task2Card.BackColor);
			}
		}

		public void TestCustomisedLayout_ShouldUseJobTypeOverride()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = BMSTestHelper.CreateBoardSection(config.Bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			layout.BackgroundColor = ColorList.NameFromColor(Color.PeachPuff);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: group.PK);
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new TaskCardControl(new TaskCardContent(task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
			{
				AssertColorEquals("There is no customised layout for task cards yet", Color.White, control.BackColor);
			}

			var link = BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new TaskCardControl(new TaskCardContent(task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
			{
				AssertColorEquals("Customised layout is configured for all job types", Color.PeachPuff, control.BackColor);
			}

			link.FML_JobType = "WKI";
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new TaskCardControl(new TaskCardContent(task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
			{
				AssertColorEquals("Customised layout is configured for a different job type", Color.White, control.BackColor);
			}

			link.FML_JobType = "ORG";
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new TaskCardControl(new TaskCardContent(task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
			{
				AssertColorEquals("Customised layout is configured for the task's job type", Color.PeachPuff, control.BackColor);
			}
		}

		#endregion

		#region Assign To Capability

		public void TestAssignToCapabilityCausesWorkflowRefresh()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			var resource1 = CreateStaffInCurrentBranchDept("S11", "Bobs your uncle", capability);
			var resource2 = CreateStaffInCurrentBranchDept("S22", "Jane is a woman so not your uncle", capability);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "Isn't she?", releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, string.Empty, 0);
			task.P9_G4_RequiredCapability = capability.PK;

			var sectionAndViewModel = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 4, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.Resource, (bool?)null, resource1, resource2);
			var viewModel = sectionAndViewModel.Item2;
			var boardSection = sectionAndViewModel.Item1;

			workflow.FH_FC_CurrentComponent = viewModel.ComponentPK;

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(boardSection.Board)))
			{
				form.Show();

				var cards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, cards.Length);

				var firstCard = cards.FirstOrDefault();
				AssertNotNull(firstCard);

				firstCard.ShowDetailedCard();
				var detailedCard = form.Controls.OfType<TaskCardDetailControl>().ToArray().FirstOrDefault();

				detailedCard.ProcessTask.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
				detailedCard.Save_ForTest();

				var newCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(1, newCards.Length);
			}
		}

		#endregion

		#region Task Reload

		public void TestTaskReload_ShouldReloadParentPanelTasks()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var board = system.Boards.AddNew();
			var section = CreateBoardSection(bucket, board);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Coughocalypse", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
				AssertEquals(task1.PK, taskCards[0].CardContent.TaskIdentifier);
				AssertEquals(task2.PK, taskCards[1].CardContent.TaskIdentifier);

				var firstCard = taskCards.FirstOrDefault();
				AssertNotNull(firstCard);

				firstCard.ShowDetailedCard();
				var detailedCard = form.Controls.OfType<TaskCardDetailControl>().ToArray().FirstOrDefault();

				detailedCard.ProcessTask.P9_Description = "Benedict is cool";
				detailedCard.Save_ForTest();

				var newTaskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
				AssertEquals(task1.PK, taskCards[0].CardContent.TaskIdentifier);
				AssertEquals(task2.PK, taskCards[1].CardContent.TaskIdentifier);

				AssertNotEquals("Control should have been regenerated", taskCards[0], newTaskCards[0]);
				AssertNotEquals("Control should have been regenerated", taskCards[1], newTaskCards[1]);
			}
		}

		public void TestSettingTaskCardNote_ShouldReloadAndUpdateText()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Coughocalypse", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindSingle<TaskCardControl>();
				AssertEquals(task1.PK, taskCard.CardContent.TaskIdentifier);
				AssertEquals(string.Empty, taskCard.NoteTextLabel.Text);

				ShowDetailedCardAndSetNoteTextAndSave(taskCard, "Booo");

				taskCard = form.FindSingle<TaskCardControl>();
				ShowDetailedCardAndSetNoteTextAndSave(taskCard, "Blap");
			}
		}

		public void TestSettingTaskCardNote_Blank_ShouldReloadAndUpdateText()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Coughocalypse", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindSingle<TaskCardControl>();
				AssertEquals(task1.PK, taskCard.CardContent.TaskIdentifier);
				AssertEquals(string.Empty, taskCard.NoteTextLabel.Text);

				ShowDetailedCardAndSetNoteTextAndSave(taskCard, "Do you like bananas?");

				taskCard = form.FindSingle<TaskCardControl>();
				ShowDetailedCardAndSetNoteTextAndSave(taskCard, "Um I dont know");

				taskCard = form.FindSingle<TaskCardControl>();
				ShowDetailedCardAndSetNoteTextAndSave(taskCard, string.Empty);

				taskCard = form.FindSingle<TaskCardControl>();
				ShowDetailedCardAndSetNoteTextAndSave(taskCard, "So you dont know?");

				form.Show();

				AssertEquals("Value should be explicitly set to 8pt on any DPI scale", 8f, taskCard.NoteTextLabel.Font.SizeInPoints);
			}
		}

		static void ShowDetailedCardAndSetNoteTextAndSave(TaskCardControl taskCard, string text)
		{
			taskCard.ShowDetailedCard();
			var form = taskCard.FindForm();
			var taskPanel = taskCard.Parent;

			var detailedCard = form.FindAll<TaskCardDetailControl>().Single();
			detailedCard.ProcessTask.P9_CardNote = text;

			detailedCard.Save_ForTest();

			taskCard = taskPanel.Controls.OfType<TaskCardControl>().Single();

			AssertEquals(text, taskCard.CardContent.NoteText);
		}

		#endregion

		#region DetailedCard

		public void TestShowDetailedCard_WorkflowBoardSection()
		{
			using (DisableAsyncBehaviour())
			{
				var system = CreateSystem("ORG");
				var group = BMSTestHelper.CreateGroup(Factory, "AAA");
				var bucket = CreateBucket(system);
				var section = CreateBoardSection(bucket);
				section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
				section.SectionConfiguration.ReleaseGroupPK = group.PK;

				var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
				workflow.FH_GG_ReleaseGroup = group.PK;
				var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

				Factory.Save();

				using (var form = GetAndShowVisualBoardForm(section))
				{
					var taskCard = TaskCardDraggingTest.FindTaskCardControl(form, task);
					AssertNotNull("Should be a task card on the board", taskCard);

					taskCard.ShowDetailedCard();

					var detailedCard = form.Controls.OfType<TaskCardDetailControl>().FirstOrDefault();
					AssertNotNull(detailedCard);
				}
			}
		}

		public void TestShowDetailedCardWhenSectionViewModelPropertyCacheIsEmpty_BoardRefreshed_ExpectNoExceptions()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system, "bbb");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(Factory, "Workflow", releaseGroupPK: group.PK);
			var task = CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 30, description: "Task");

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = TaskPanelTest.GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 200))
			{
				var taskCard = form.FindSingle<TaskCardControl>();
				AssertNotNull(taskCard);

				form.RefreshStarted += (sender, args) =>
				{
					var componentControl = form.FindSingle<BMComponentControl>();
					componentControl.ViewModel.Cache.Clear();
					taskCard.ShowDetailedCard();
				};

				form.RefreshNow_ForTest();
			}
		}

		public void TestDetailedCardShownOnClick()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system, "bbb");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = TaskPanelTest.GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 200))
			{
				var taskCards = form.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, taskCards.Length);

				var task1Card = taskCards[0];
				var task2Card = taskCards[1];

				AssertEquals(task1.PK, task1Card.CardContent.TaskIdentifier);
				AssertEquals(task2.PK, task2Card.CardContent.TaskIdentifier);

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task1, "Clicking frontmost card should show detailed card");

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, null, "Clicking same card a second time should hide detailed card");

				task2Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, null, "Should reorder stack not show the card yet");

				task2Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task2, "Clicking the new card at the front again should now open detailed card");

				task2Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, null, "Clicking same card a second time should hide detailed card");

				task2Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task2, "Clicking card still at the front should display detailed card again");

				task1Card.OnTaskCardControlClicked();
				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task1, "Clicking lower card twice should bring to front then display its detailed card");
			}
		}

		[TestDate(2016, 6, 24, 0, 0, 0)]
		public void TestDateEditOnDetailedCard_ShouldBeStampedWithCurrentDate()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(1, taskCards.Length);

				var task1Card = taskCards[0];
				AssertEquals(task.PK, task1Card.CardContent.TaskIdentifier);

				task1Card.ShowDetailedCard();

				var refreshWasStarted = false;
				form.RefreshStarted += (sender, args) =>
				{
					refreshWasStarted = true;
				};

				var dateTimeBox = form.Find(control => control is ZDateEdit).Cast<ZDateEdit>().FirstOrDefault();
				Application.DoEvents();

				BMSFormTestHelper.PressHotkeys(dateTimeBox, Keys.F5);

				AssertEquals("Date time control should still be stamped when it has focus", "24-JUN-16 00:00", dateTimeBox.Text);
				AssertEquals("Board should not be refreshed", false, refreshWasStarted);
			}
		}
		public void TestDetailedCardShownOnClick_WhenCardsNotOverlapping_ShouldShowDetailedCardWithoutBringingToFrontFirst()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				var task1Card = taskCards[0];
				var task2Card = taskCards[1];

				AssertEquals(task1.PK, task1Card.CardContent.TaskIdentifier);
				AssertEquals(task2.PK, task2Card.CardContent.TaskIdentifier);

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task1, "Clicking frontmost card should show detailed card");

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, null, "Clicking same card a second time should hide detailed card");

				task2Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task2, "Should show the detailed card since second card is not obstructed by the first - no need to reorder");
			}
		}

		public void TestDetailedCardShownWithCorrectCapabilityAssignmentButtonVisibility()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var capability = CreateCapability("CAP", "Some dummy capability");
			var staff = CreateStaffInCurrentBranchDept("STF", "Stefano", capability);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Workflow", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 0, capability: capability, description: "Task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(1, taskCards.Length);

				var task1Card = taskCards[0];

				AssertEquals(task1.PK, task1Card.CardContent.TaskIdentifier);

				task1Card.OnTaskCardControlClicked();
				var detailedCards = AssertDetailedCardShownForTask(form, task1, "Clicking the card should show detailed card");

				var capabilityAssignmentButton = detailedCards[0].FindAll<CapabilityAssignmentButton>().Single();
				Assert("Claim button is visible as it requires a resource from capability", capabilityAssignmentButton.Visible);

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, null, "Clicking same card a second time should hide detailed card");

				var newFactory = Factory.CreateNewFactory();
				newFactory.RefreshEnabled = false;
				var reloadedTask1 = newFactory.Load<ProcessTask>(task1.PK);
				reloadedTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

				newFactory.Save();

				task1Card.OnTaskCardControlClicked();
				detailedCards = AssertDetailedCardShownForTask(form, task1, "Clicking the card should show detailed card again");

				capabilityAssignmentButton = detailedCards[0].FindAll<CapabilityAssignmentButton>().Single();
				Assert("Claim button is not visible as it is already assigned", !capabilityAssignmentButton.Visible);
			}
		}

		[TestDate(2017, 10, 5)]
		public void TestDetailedCardShownWithCorrectCapabilityAssignmentButtonVisibility_DBHits()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var capability = VisualBoardsTestHelper.CreateCapability(Factory, "CAP", "Some dummy capability");
			var staff = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "STF", "Stefano", capability);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(Factory, "Workflow", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow, staffCode: string.Empty, lowEstMinutes: 0, capability: capability, description: "Task 1");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				var task1Card = taskCards[0];

				var allowedHits = new Dictionary<string, int>()
				{
					{ GlbStaffSchema.Constants.TableName, 0 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsForAllFactories(allowedHits, useOnlyNewFactories: true))
				{
					task1Card.OnTaskCardControlClicked();
				}

				var detailedCards = AssertDetailedCardShownForTask(form, task1, "Clicking the card should show detailed card");
				var capabilityAssignmentButton = detailedCards[0].FindAll<CapabilityAssignmentButton>().Single();
				Assert("Claim button is visible as it requires a resource from capability", capabilityAssignmentButton.Visible);

				task1Card.OnTaskCardControlClicked();

				AssertDetailedCardShownForTask(form, null, "Clicking same card a second time should hide detailed card");

				var otherFactory = Factory.CreateNewFactory();
				otherFactory.RefreshEnabled = false;
				var reloadedTask1 = otherFactory.Load<ProcessTask>(task1.PK);
				reloadedTask1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

				otherFactory.Save();

				allowedHits.Remove(GlbStaffSchema.Constants.TableName);

				using (AssertDbHitsForAllFactories(allowedHits, useOnlyNewFactories: true))
				{
					task1Card.OnTaskCardControlClicked();
				}

				detailedCards = AssertDetailedCardShownForTask(form, task1, "Clicking the card should show detailed card again");
				capabilityAssignmentButton = detailedCards[0].FindAll<CapabilityAssignmentButton>().Single();

				Assert("Claim button is not visible as it is already assigned", !capabilityAssignmentButton.Visible);
			}
		}

		[TestDate(2017, 10, 5)]
		public void TestDetailedCardForJobWorkflow_DBHits()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var capability = VisualBoardsTestHelper.CreateCapability(Factory, "CAP", "Some dummy capability");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var staff1 = CreateStaffInCurrentBranchDept("TST", "Test");
			var staff2 = CreateStaffInCurrentBranchDept("TS1", "Test 1");

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_GG_ReleaseGroup = group.PK;
			var workflow1 = CreateWorkflow(jobHeader, "Workflow 1", releaseGroupPK: group.PK);
			var task1_1 = CreateTask(workflow1, staff1.GS_Code, 25);
			var task1_2 = CreateTask(workflow1, staff2.GS_Code, 25);
			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				var task1Card = taskCards[0];

				var allowedHits = new Dictionary<string, int>()
				{
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 3 },
					{ ProcessTasksSchema.Constants.TableName, 4 },
				};

				using (AssertDbHitsForAllFactories(allowedHits, useOnlyNewFactories: true))
				{
					task1Card.OnTaskCardControlClicked();
				}
			}
		}

		[TestDate(2016, 2, 1)]
		public void TestDetailedCardSaved_CacheClearIsSafe()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			config.ReleaseGroup.Staff.AddRange(resource1, resource2);
			resource1.DesignateAsCCR(config.Buffer);
			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, config.Buffer.PK);

			var capability = VisualBoardsTestHelper.CreateCapability(Factory, "CAP", "Some dummy capability");

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 4;

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "No", config.Buffer, ZDateTime.UtcNow.AddDays(-20), config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow1, resource1.GS_Code);
			BMSTestHelper.CreateTask(workflow1, resource1.GS_Code);
			BMSTestHelper.CreateTask(workflow1, resource2.GS_Code);
			BMSTestHelper.CreateTask(workflow1, resource2.GS_Code);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "No", config.Buffer, ZDateTime.UtcNow.AddDays(-20), config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow2, resource2.GS_Code);
			BMSTestHelper.CreateTask(workflow2, resource2.GS_Code);
			BMSTestHelper.CreateTask(workflow2, resource1.GS_Code);
			BMSTestHelper.CreateTask(workflow2, resource1.GS_Code);

			Factory.Save();

			AssertEquals("Failure is very specific. Only applies to zone 0 tasks.", 0, workflow1.BufferZone);
			AssertEquals("Failure is very specific. Only applies to zone 0 tasks.", 0, workflow2.BufferZone);

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var task1Card = form.FindAll<TaskCardControl>().First();

				task1Card.OnTaskCardControlClicked();
				var detailedCard = form.FindAll<TaskCardDetailControl>().Single();

				form.BoardViewModel.GetSections().OfType<BMBoardSectionViewModel>().ForEach(s => s.Cache.Clear());

				AssertNoExceptionThrown(() => detailedCard.Save_ForTest());
				Application.DoEvents();
			}
		}

		public void TestStackReordering_ShouldCloseDetailedCardsIfNotPinned()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system, "bbb");
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = TaskPanelTest.GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 200))
			{
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				var task1Card = taskCards[0];
				var task2Card = taskCards[1];

				AssertEquals(task1.PK, task1Card.CardContent.TaskIdentifier);
				AssertEquals(task2.PK, task2Card.CardContent.TaskIdentifier);

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task1, "Clicking frontmost card should show detailed card");

				var detailedCard = form.Controls.OfType<TaskCardDetailControl>().First();

				task2Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, null, "Clicking task 2 card should reorder card stack and close open detailed cards");

				task2Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task2, "Clicking task 2 card again should open its own detailed card");

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, null, "Clicking task 1 card should bring it to the front");
				AssertEquals(0, form.Controls.OfType<TaskCardDetailControl>().ToArray().Length);

				task1Card.OnTaskCardControlClicked();
				AssertDetailedCardShownForTask(form, task1, "Clicking task 1 card again should open its detailed card");
			}
		}

		TaskCardDetailControl[] AssertDetailedCardShownForTask(VisualBoardForm form, ProcessTask task, string message = "")
		{
			var detailedCards = form.FindAll<TaskCardDetailControl>().ToArray();
			if (task == null)
			{
				AssertEquals("Should be no detailed task card. " + message, 0, detailedCards.Length);
			}
			else
			{
				message = string.Format("Should have displayed a detailed card for task {0}. {1}", task.P9_Description, message);
				AssertNotNull(message, detailedCards.FirstOrDefault(c => c.ProcessTask.PK == task.PK));
			}

			return detailedCards;
		}

		public void TestShowDetailedCard_ForDeletedTask_ShouldRemoveTaskCard()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Workflow", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			var otherWorkflow = CreateWorkflow(jobHeader, "Other", releaseGroupPK: group.PK);
			var task2 = CreateTask(otherWorkflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				var task1Card = taskCards[0];
				var task2Card = taskCards[1];

				AssertEquals(task1.PK, task1Card.CardContent.TaskIdentifier);
				AssertEquals(task2.PK, task2Card.CardContent.TaskIdentifier);

				task2.Delete();
				Factory.Save();

				task2Card.ShowDetailedCard();

				AssertEquals(1, form.FindAll<TaskCardControl>().Count());
				Assert("Should dispose task card", task2Card.IsDisposed);
			}
		}

		[ExpectNoExceptions]
		public void TestShowDetailedCard_ForDeletedTask_WithNoParent()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = CreateTask(workflow, string.Empty, 0, description: "Task 1");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var form = new ZForm())
			using (var taskCard = new TaskCardControl(task, viewModel))
			{
				form.Controls.Add(taskCard);

				task.Delete();
				taskCard.ShowDetailedCard();
			}
		}

		public void TestShowParentWorkflow_ForDeletedWorkflow_ShouldRemoveTaskCard()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "Workflow1", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var workflow2 = CreateWorkflow(jobHeader, "Workflow2", releaseGroupPK: group.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var task1Card = form.FindAll<TaskCardControl>().Single();

				task1.P9_FH_ProcessHeader = workflow2.PK;
				workflow1.Delete();
				Factory.Save();

				task1Card.ShowParentWorkflow();

				Assert("Should dispose task card", task1Card.IsDisposed);
				AssertEquals(0, form.FindAll<TaskCardControl>().Count());
				AssertEquals("This card's workflow has been deleted and will now be removed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowParentWorkflow_ForDeletedTask_ShouldRemoveTaskCard()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "Workflow1", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var workflow2 = CreateWorkflow(jobHeader, "Workflow2", releaseGroupPK: group.PK);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				var task1Card = taskCards[0];
				var task2Card = taskCards[1];

				AssertEquals(task1.PK, task1Card.CardContent.TaskIdentifier);
				AssertEquals(task2.PK, task2Card.CardContent.TaskIdentifier);

				task2.Delete();
				Factory.Save();

				task2Card.ShowParentWorkflow();

				Assert("Should dispose task card", task2Card.IsDisposed);
				AssertEquals(1, form.FindAll<TaskCardControl>().Count());
			}
		}

		public void TestShowParentWorkflow_SelectCorrectTask()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 2");

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				using (var task1Card = form.FindAll<TaskCardControl>().First(x => x.CardContent.TaskIdentifier == task1.PK))
				using (var task2Card = form.FindAll<TaskCardControl>().First(x => x.CardContent.TaskIdentifier == task2.PK))
				{
					task2Card.ShowParentWorkflow();
					Application.DoEvents();

					using (var formCreated = Application.OpenForms.OfType<ZOrganisationsForm>().First())
					{
						AssertNotNull(formCreated);
						AssertEquals(jobHeader.Parent.PK, formCreated.Organisation.PK);

						var mainTabControl = formCreated.FindAll<ZTemplateTabControl>().First();
						var workflowTabPage = mainTabControl.FindAll<ZWorkflowTabPage>().First();
						var workflowUserControl = workflowTabPage.FindAll<ZWorkflowUserControl>().First();

						mainTabControl.SelectedTab = workflowTabPage;
						Application.DoEvents();

						var control = workflowTabPage.FindAll<WorkflowManagementUserControl>().First();

						AssertEquals(1, control.WorkflowsGrid.ListManager.Position);
						AssertEquals("Should select the second task", 1, control.TasksGrid_ForTest.ListManager.Position);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestShowDetailedCard_WhenFormNotFound()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, header);

			using (var control = new TaskCardControl(task, viewModel))
			{
				control.ShowDetailedCard();
			}
		}

		#endregion

		#region StatusIndicators

		public void TestCurrentTaskIndicator_WorkflowCard_NoOpenPrereqs()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var component = config.Buffer;
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var task = CreateTask(workflow, string.Empty, 0);

			AssertEquals(false, workflow.HasOpenPrerequisites);

			var section = VisualBoardsTestHelper.CreateBoardSection(component);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var taskCard = new TaskCardControl(new WorkflowCardContent(workflow, task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
			{
				var statusIndicator = taskCard.FindAll<PictureBox>().FirstOrDefault();
				AssertNotNull(statusIndicator);
			}
		}

		public void TestCurrentTaskIndicator_WorkflowCard_OpenPrereqs()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var component = config.Buffer;
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var task1 = CreateTask(workflow1, string.Empty, 0);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task2 = CreateTask(workflow2, string.Empty, 0);

			workflow1.MakePrerequisiteOf(workflow2);
			AssertEquals(true, workflow2.HasOpenPrerequisites);

			var section = VisualBoardsTestHelper.CreateBoardSection(component);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);

			using (var taskCard = new TaskCardControl(new WorkflowCardContent(workflow2, task2, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
			{
				var statusIndicator = taskCard.Controls.OfType<PictureBox>().FirstOrDefault();
				AssertNull(statusIndicator);
			}
		}

		public void TestCurrentTaskIndicator_NoOpenPrereqs()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			var section = board.Sections.AddNew();
			var component = config.Buffer;
			section.MS_FC_Component = component.PK;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders.AddNew();
			var task = CreateTask(workflow, string.Empty, 0);

			Factory.Save();

			Assert(task.IsCurrent);
			AssertEquals(false, workflow.HasOpenPrerequisites);

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var taskCard = new TaskCardControl(task, viewModel))
			{
				var statusIndicator = taskCard.FindAll<PictureBox>().FirstOrDefault();
				AssertNotNull(statusIndicator);
			}
		}

		public void TestCurrentTaskIndicator_OpenPrereqs()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			var section = board.Sections.AddNew();
			var component = config.Buffer;
			section.MS_FC_Component = component.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = CreateTask(workflow1, string.Empty, 0);
			var task2 = CreateTask(workflow2, string.Empty, 0);

			Factory.Save();

			AssertEquals(false, workflow1.HasOpenPrerequisites);
			AssertEquals(true, workflow2.HasOpenPrerequisites);

			Assert(task2.IsCurrent);

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2);

			using (var taskCard = new TaskCardControl(task2, viewModel))
			{
				var statusIndicator = taskCard.Controls.OfType<PictureBox>().FirstOrDefault();
				AssertNull(statusIndicator);
			}
		}

		public void TestCurrentTaskIndicator_OpenPrereqsUpTheTree()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			var section = board.Sections.AddNew();
			var component = config.Buffer;
			section.MS_FC_Component = component.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var task1 = CreateTask(workflow1, string.Empty, 0);
			var task2 = CreateTask(workflow2, string.Empty, 0, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = CreateTask(workflow3, string.Empty, 0);

			AssertEquals(false, workflow1.HasOpenPrerequisites);
			AssertEquals(true, workflow2.HasOpenPrerequisites);
			AssertEquals(true, workflow3.HasOpenPrerequisites);

			Assert(task3.IsCurrent);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1, workflow2, workflow3);

			using (var taskCard = new TaskCardControl(task3, viewModel))
			{
				var statusIndicator = taskCard.Controls.OfType<PictureBox>().FirstOrDefault();
				AssertNull(statusIndicator);
			}
		}

		public void TestTaskStatusIndicators()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var platinum = BMSTestHelper.CreateTagMagnitude(tagGroup, "PLT", color: Color.HotPink);
			platinum.ApplyColorToBackground = true;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var task = CreateTask(workflow, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Open); // 1 hour
			AssertNotEquals(task.ParentTaskCollection.GetCurrentTask(task.ProcessHeader), task);

			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 5, 0, 0); // 5 hours
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var taskCard = new TaskCardControl(task, viewModel))
			{
				var indicators = GetVisibleStatusIndicators(taskCard).ToArray();
				AssertEquals(1, indicators.Length);

				var statusIndicator = indicators[0];
				var leftPosition = taskCard.Width - statusIndicator.Width; // already scaled

				Assert("Status indicator should have graphic background of Working status", statusIndicator is StatusIndicatorControl);
				Assert(statusIndicator.Visible);
				AssertEquals(new Point(leftPosition, ControlDpiScalingHelper.ScaleToCurrentDpiY(2)), statusIndicator.Location);
			}
		}

		IEnumerable<Control> GetVisibleStatusIndicators(TaskCardControl taskCard)
		{
			return taskCard.Find(c => c.Visible && (c is StatusIndicatorControl));
		}

		#endregion

		#region CardContents

		public void TestMakeTaskChannelMap_ShouldNotDieHorribly_WhenLoadingOnNullSection()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;
			section.SectionConfiguration.SectionNameIsOverridden = true;
			section.SectionConfiguration.SectionNameOverride = "Squanch";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				form.BeforeRefreshBoardAction_ForTest = () =>
				{
					var factorio = new BusinessObjectFactory();
					var sectionReloaded = factorio.Load(typeof(BMBoardSection), section.PK);
					sectionReloaded.Delete();
					factorio.Save();
				};
				form.ReloadBoard();
				var label = form.FindSingle<ZLabel>("LoadFailedLabel");

				AssertEquals("This section cannot be displayed.", label.Text);
			}
		}

		public void TestMakeTaskChannelMap_ShouldNotDieHorriblyAndShowNiceMessage_WhenExcessiveQueryComplexity()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BucketSection;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			Factory.AccessingPersistentValueForTesting += (BusinessObject bo, System.Data.DataColumn _) =>
			{
				if (bo is ProcessTask)
				{
					throw SqlExceptionBuilder.CreateSqlException(8623, 51, 13, Db.ServerName, "The timeout period elapsed prior to completion of the operation or the server is not responding.", string.Empty, 1);
				}
			};

			var viewModel = VisualBoardsTestHelper.CreateViewModel(section);
			var taskChannelMap = LoadCardContents.MakeTaskChannelMap(Factory, viewModel);

			AssertNotNull(taskChannelMap.Failure);
			AssertType<SqlException>(taskChannelMap.Failure.FailureException);
			AssertEquals("This section could not be loaded because the generated filter exceeded the database query complexity limit. Please try adjusting the section's workflow filters and task filters.", taskChannelMap.Failure.FailureText);
		}

		public void TestPaveLoad_WhenEnableGlowIndexSearchUsageCollectorRegistry_ShouldGenerateEDI()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BucketSection;
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section);

			using (GlowRegistry.Instance.GlowIndexSearchUsageCollector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var taskChannelMap = LoadCardContents.MakeTaskChannelMap(Factory, viewModel);

				var properties = new List<(string name, object value)>()
				{
					("SearchType", "PaveSql"),
					("ModuleID", "Bucket Board"),
					("Filters", "WorkflowFilter: [FH_P0_Template is NULL ], TaskFilter: [P9_Type <> 'EXC' AND P9_Type <> 'MIL' AND P9_Type <> 'TRG' ]")
				};

				var helper = new UsageCollectorTestHelper(Factory);
				AssertEquals("Contains message.", true, helper.AssertUsageMessagesContains("SPF", properties));
			}
		}

		public void TestMakeTaskChannelMap_ShouldNotDieHorriblyAndShowNiceMessage_WhenTimeout()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BucketSection;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			Factory.AccessingPersistentValueForTesting += (BusinessObject bo, System.Data.DataColumn _) =>
			{
				if (bo is ProcessTask)
				{
					throw SqlExceptionBuilder.CreateSqlException(3617, 51, 13, Db.ServerName, "The timeout period elapsed prior to completion of the operation or the server is not responding.", string.Empty, 1);
				}
			};

			var viewModel = VisualBoardsTestHelper.CreateViewModel(section);
			var taskChannelMap = LoadCardContents.MakeTaskChannelMap(Factory, viewModel);

			AssertNotNull(taskChannelMap.Failure);
			AssertType<SqlException>(taskChannelMap.Failure.FailureException);
			AssertEquals("This section could not be loaded because it took too long to get its data from the database. Please try adjusting the section's workflow filters and task filters.", taskChannelMap.Failure.FailureText);
		}

		public void TestMakeTaskChannelMap_ShouldReportError_WhenTwoOrMoreWorkingTasksForSameStaff()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var staff1 = CreateStaffInCurrentBranchDept("ST1", "Staff1");
			var staff2 = CreateStaffInCurrentBranchDept("ST2", "Staff2");

			var section = config.BufferSection;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var taskForStaff1InWorkflow1 = BMSTestHelper.CreateTask(workflow1, staffCode: staff1.GS_Code, description: "taskForStaff1InWorkflow1");
			var taskForStaff2InWorkflow1 = BMSTestHelper.CreateTask(workflow1, staffCode: staff2.GS_Code, description: "taskForStaff2InWorkflow1");

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var taskForStaff1InWorkflow2 = BMSTestHelper.CreateTask(workflow2, staffCode: staff1.GS_Code, description: "taskForStaff1InWorkflow2");

			Factory.Save();

			var otherFactory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var taskForStaff1InWorkflow1FromOtherFactory1 = otherFactory1.Load<ProcessTask>(taskForStaff1InWorkflow1.PK);
			var taskForStaff2InWorkflow1FromOtherFactory1 = otherFactory1.Load<ProcessTask>(taskForStaff2InWorkflow1.PK);

			var otherFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var taskForStaff1InWorkflow2FromOtherFactory2 = otherFactory2.Load<ProcessTask>(taskForStaff1InWorkflow2.PK);

			taskForStaff1InWorkflow1FromOtherFactory1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			taskForStaff2InWorkflow1FromOtherFactory1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			taskForStaff1InWorkflow2FromOtherFactory2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();
			otherFactory1.Save();

			try
			{
				otherFactory2.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				AssertEquals($"Unreconcilable concurrency error due to multiple WRK tasks for staff ST1 detected: {taskForStaff1InWorkflow1FromOtherFactory1.P9_TaskID}, {taskForStaff1InWorkflow2FromOtherFactory2.P9_TaskID}", ex.InnerException.InnerException.Message);
			}
		}

		public void TestMakeTaskChannelMap_ShouldNotRaiseAnException_WhenCheckingDeletedTask()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var staff1 = CreateStaffInCurrentBranchDept("ST1", "Staff1");
			var staff2 = CreateStaffInCurrentBranchDept("ST2", "Staff2");

			var section = config.BufferSection;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var taskForStaff1InWorkflow1 = BMSTestHelper.CreateTask(workflow1, staffCode: staff1.GS_Code, description: "taskForStaff1InWorkflow1");
			var taskForStaff2InWorkflow1 = BMSTestHelper.CreateTask(workflow1, staffCode: staff2.GS_Code, description: "taskForStaff2InWorkflow1");
			var taskToDelete = BMSTestHelper.CreateTask(workflow1);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var taskForStaff1InWorkflow2 = BMSTestHelper.CreateTask(workflow2, staffCode: staff1.GS_Code, description: "taskForStaff1InWorkflow2");

			Factory.Save();

			var otherFactory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var taskForStaff1InWorkflow1FromOtherFactory1 = otherFactory1.Load<ProcessTask>(taskForStaff1InWorkflow1.PK);
			var taskForStaff2InWorkflow1FromOtherFactory1 = otherFactory1.Load<ProcessTask>(taskForStaff2InWorkflow1.PK);

			var otherFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var taskForStaff1InWorkflow2FromOtherFactory2 = otherFactory2.Load<ProcessTask>(taskForStaff1InWorkflow2.PK);
			var taskToDeleteFromOtherFactory2 = otherFactory2.Load<ProcessTask>(taskToDelete.PK);

			taskForStaff1InWorkflow1FromOtherFactory1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			taskForStaff2InWorkflow1FromOtherFactory1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			taskForStaff1InWorkflow2FromOtherFactory2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			taskToDeleteFromOtherFactory2.Delete();

			Factory.Save();
			otherFactory1.Save();
			AssertExceptionThrown<ZSaveConcurrencyException>(() => otherFactory2.Save());
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestTaskCardContents()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "PNK", color: Color.BlanchedAlmond);
			tag.ApplyColorToBackground = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Mai Frist Organisation";
			org.OH_Code = "MFO";

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var header = jobHeader.ProcessHeaders[0];
			header.AddTag(tag);

			var task = org.WorkflowItems.AddNew();
			task.P9_Description = "Do things";
			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 1, 0, 0); // 1 hour
			task.P9_EstimateVariationFactor = 2;
			task.P9_CardNote = "Dat task.";
			task.P9_FH_ProcessHeader = header.PK;

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, header);

			using (var form = new ZForm())
			using (var taskCard = new TaskCardControl(task, viewModel))
			{
				form.Controls.Add(taskCard);
				form.Show();

				AssertColorEquals(Color.BlanchedAlmond, taskCard.BackColor);

				task.P9_CardNote = ZString.Empty;

				using (var form2 = new ZForm())
				using (var taskCard2 = new TaskCardControl(task, viewModel))
				{
					form2.Controls.Add(taskCard);
					form2.Show();

					AssertEquals("Should reduce task card height when no note text present", taskCard2.Height, taskCard.Height);
				}
			}
		}

		public void TestCustomisedControl_ShouldSizeToFitParent()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = workflow.Parent.WorkflowItems.AddNew();

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var taskCard = new TaskCardControl(task, viewModel))
			{
				var customisedControl = taskCard.FindAll<ZUserControl>().First();
				AssertEquals(taskCard.Width, customisedControl.Width);

				taskCard.Width += 100;
				AssertEquals(taskCard.Width, customisedControl.Width);
			}
		}

#if !WINZOR  // Bitmap rendering is not used for Winzor, so the test does not apply.
		public void TestGetBitmapRender_RendersChildControlsInCorrectOrder()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG", "INQ");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var org = jobHeader.Parent as OrgHeader;

			AssertEquals(org.CompanyData.Company.GC_Code, "EDI");

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			var layout = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);

			var layoutSize = ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			var lineUpperLeftCorner = ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			var lineSize = ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			var colorTestPoint = ControlDpiScalingHelper.NewScaledPoint(10, 110, true);
			layout.Width = layoutSize.Width;
			layout.Height = layoutSize.Height;
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_Code, PropertyTypeList.Codes.Text, string.Empty, lineUpperLeftCorner.X, lineUpperLeftCorner.Y, lineSize.Width, lineSize.Height, Color.Red.Name, Color.Red.Name, 8, false, true, autoSize: false);
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_FullName, PropertyTypeList.Codes.Text, string.Empty, lineUpperLeftCorner.X, lineUpperLeftCorner.Y, lineSize.Width, lineSize.Height, Color.Green.Name, Color.Green.Name, 8, false, true, autoSize: false);

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(section.Board);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				Thread.Sleep(5000);

				var card = BMSGUITestCase.FindTaskCardControl(form, task);
				var bitmap = (Bitmap)card.BackgroundImage;
				var color = bitmap.GetPixel(colorTestPoint.X, colorTestPoint.Y);

				CombineAssertions("The second green line should be imposed over the first red one", () =>
				{
					AssertEquals("Green", Color.Green.G, color.G);
					AssertEquals("Red", (byte)0, color.R);
				});
			}
		}
#endif

		public void TestLoadCardContents_SectionComponent_ValidateSectionDuringRefresh()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;
			section.MS_FC_Component = ZGuid.Empty;
			section.SectionConfiguration.SectionNameIsOverridden = true;
			section.SectionConfiguration.SectionNameOverride = "Squanch";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var label = form.FindSingle<ZLabel>("LoadFailedLabel");
				AssertEquals("This section (Squanch) has validation errors and cannot be shown. Please open the board configuration form and fix any validation errors.", label.Text);
			}
		}

		public void TestLoadCardContents_SectionComponent_ShouldNotReportWhenNotNull()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;
			section.SectionConfiguration.SectionNameIsOverridden = true;
			section.SectionConfiguration.SectionNameOverride = "Squanch";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = form.FindSingle<TaskCardControl>();

				AssertNotNull(taskCard);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

#endregion

		#region Stack Reordering

		public void TestClickingTaskCard_ShouldReorderStack()
		{
			var system = CreateSystem("ORG");
			var component = BMSTestHelper.CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(component);
			section.MS_FC_Component = component.PK;
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "TheWorkflow", component, releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 30, description: "Task 1");
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 30, description: "Task 2");
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 30, description: "Task 3");
			var task4 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 30, description: "Task 4");

			Factory.Save();

			using (var form = TaskPanelTest.GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 200, height: 200))
			{
				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCards = taskPanel.Controls.OfType<TaskCardControl>().ToArray();
				AssertEquals(4, taskCards.Length);

				AssertEquals(task1.PK, taskCards[0].CardContent.TaskIdentifier);
				AssertEquals(task2.PK, taskCards[1].CardContent.TaskIdentifier);
				AssertEquals(task3.PK, taskCards[2].CardContent.TaskIdentifier);
				AssertEquals(task4.PK, taskCards[3].CardContent.TaskIdentifier);

				taskCards[2].OnTaskCardControlClicked();
				taskCards = taskPanel.Controls.OfType<TaskCardControl>().ToArray();

				AssertEquals(task3.PK, taskCards[0].CardContent.TaskIdentifier);
				AssertEquals(task4.PK, taskCards[1].CardContent.TaskIdentifier);
				AssertEquals(task1.PK, taskCards[2].CardContent.TaskIdentifier);
				AssertEquals(task2.PK, taskCards[3].CardContent.TaskIdentifier);
			}
		}

		#endregion

		#region Card border

		[ExpectNoExceptions]
		public void TestOnPaint_ForDeletedTask()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			var task = job.WorkflowItems.AddNew();

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(config.BucketSection, workflow);

			using (var form = new ZForm())
			using (var taskCard = new TaskCardControl(task, viewModel))
			{
				task.Delete();

				form.Controls.Add(taskCard);
				form.Show();
			}
		}

		#endregion

		#region DeletedTasks

		public void TestDeletedTasksShouldNotCauseExceptionsBeforeRefresh()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			var system = BMSTestHelper.CreateSystem(factory, "ORG");
			var bucket = system.Components.AddNew();
			bucket.FC_Name = "bucket";
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				var loadedTask1 = otherFactory.Load<ProcessTask>(task1.PK);

				AssertEquals(false, object.ReferenceEquals(task1, loadedTask1));

				using (var card = new TaskCardControl(new TaskCardContent(loadedTask1, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true))
				{
					form.Controls.Add(card);
					AssertNoExceptionThrown(() => card.OnTaskCardControlClicked());
					AssertNoExceptionThrown(() => card.OnTaskCardControlClicked()); //Toggle detailed task card off again.

					task1.Delete();
					factory.Save();
					AssertEquals(false, loadedTask1.IsDeleted);

					AssertNull(new BusinessObjectFactory().Load<ProcessTask>(task1.PK));
					AssertEquals(1, form.Controls.OfType<TaskCardControl>().Count());

					AssertNoExceptionThrown(() => card.OnTaskCardControlClicked());
					AssertEquals(0, form.Controls.OfType<TaskCardControl>().Count());
				}
			}
		}

		#endregion

		#region DeletedWorkflows

		public void TestDeletedWorkflowsShouldNotCauseNullExceptions()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			var section = config.BucketSection;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var newFactory1 = Factory.CreateNewFactory();
			var loadedTask1 = newFactory1.Load<ProcessTask>(task1.PK);

			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedWorkflow2 = newFactory2.Load<ProcessHeader>(workflow.PK);
			loadedWorkflow2.Delete();
			newFactory2.Save();

			AssertNull(loadedTask1.ProcessHeader);

			using (var card = new TaskCardControl(loadedTask1, viewModel))
			{
				card.TaskCardContextMenuStrip_OnOpening_ForTest();
				AssertEquals("This card's workflow has been deleted and will now be removed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Dispose

		public void TestDispose()
		{
			var system = BMBoardSectionTestHelper.CreateSystem(Factory);
			var bucket = BMBoardSectionTestHelper.CreateBucket(system);
			var section = BMBoardSectionTestHelper.CreateBoardSection(bucket);
			var jobHeader = BMBoardSectionTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMBoardSectionTestHelper.CreateWorkflow(jobHeader, "Completion statement");
			var task = BMBoardSectionTestHelper.CreateTask(workflow);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			WeakReference weakRef = GetWeakReference(task, viewModel);

			GC.Collect(); // This is a test so it is okay
			GC.WaitForFullGCComplete();

			Assert("TaskCardControl was constructed, then disposed and has not been Garbage Collected.", !weakRef.IsAlive);
		}

		static WeakReference GetWeakReference(ProcessTask task, BMBoardSectionViewModel viewModel)
		{
			var objectToTest = new TaskCardControl(new TaskCardContent(task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: true);
			objectToTest.Dispose();
			return new WeakReference(objectToTest);
		}

		[ExpectNoExceptions]
		public void TestHandlerDoesNotThrowNullReferenceOnDispose()
		{
			var system = BMBoardSectionTestHelper.CreateSystem(Factory);
			var bucket = BMBoardSectionTestHelper.CreateBucket(system);
			var section = BMBoardSectionTestHelper.CreateBoardSection(bucket);
			var jobHeader = BMBoardSectionTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMBoardSectionTestHelper.CreateWorkflow(jobHeader, "Completion statement");
			var task = BMBoardSectionTestHelper.CreateTask(workflow);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			var testObject = new TaskCardControl(task, viewModel);
			testObject.Dispose();
		}

		#endregion

#if !WINZOR
		#region Bitmaps

		[TestDate(2015, 7, 14)]
		public void TestSetBackgroundImage_ShouldKeepReferenceToCardBitmaps()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow);

			var section = config.BucketSection;

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(section))
			{
				TaskCardControl ticket = null;
				CardBitmaps bitmaps = null;

				void RetrieveCachedBitmaps()
				{
					var componentControl = form.FindSingle<BMComponentControl>();
					ticket = componentControl.FindSingle<TaskCardControl>();

					bitmaps = componentControl.ViewModel.GetCardBitmaps(ticket.CardContent);
				}

				RetrieveCachedBitmaps();

				AssertNotNull(bitmaps);
				AssertEquals("Should have cached the bitmaps produced when the ticket control finished loading", bitmaps, ticket.BackgroundBitmaps);
				AssertEquals("Ticket background image should be set from the cached bitmap", bitmaps.NormalBitmap, ticket.BackgroundImage);

				var previousBitmaps = bitmaps;

				TestDateAttribute.AddMinutes(1);

				form.RefreshBoard();
				RetrieveCachedBitmaps();

				AssertNotNull(bitmaps);
				AssertEquals("Should have re-used bitmaps since nothing has changed", previousBitmaps, bitmaps);

				previousBitmaps = bitmaps;

				TestDateAttribute.AddMinutes(1);
				task.P9_CardNote = "No one knew bitmaps were so hard";
				task.Factory.Save();

				Application.DoEvents();

				RetrieveCachedBitmaps();

				AssertEquals("Ticket should be updated by DataRefreshBus upon saving", "No one knew bitmaps were so hard", ticket.CardContent.NoteText);
				AssertNotEquals("Should have re-generated bitmaps since something has changed", previousBitmaps, bitmaps);
				AssertEquals("Should have cached the bitmaps produced when the ticket control finished loading", bitmaps, ticket.BackgroundBitmaps);
				AssertEquals("Ticket background image should be set from the cached bitmap", bitmaps.NormalBitmap, ticket.BackgroundImage);
				AssertEquals(true, previousBitmaps.IsDisposed);
			}
		}

		#endregion
#endif

		#region Other/Misc

		public void TestLabels_ShouldNotUseMnemonic()
		{
			var system = CreateSystem("ORG");
			var section = CreateBoardSection(CreateBucket(system));

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			task.P9_CardNote = "Something";

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new TaskCardControl(task, viewModel))
			{
				foreach (var label in control.FindAll<ZLabel>().Where(l => l != control.NoteTextLabel))
				{
					AssertEquals(false, label.UseMnemonic);
				}

				AssertEquals("Something", control.NoteTextLabel.Text);
				AssertEquals(false, control.NoteTextLabel.UseMnemonic);
			}
		}

		public void TestShowParentWorkflow_NullFormGenerated()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var task = BMSTestHelper.CreateTask(workflow);

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new TaskCardControl(task, viewModel))
			{
				AssertNoExceptionThrown("Should have no NullReferenceException thrown", () =>
				{
					control.ShowParentWorkflow();
				});

				var messageToAssert = $@"Unable to show the form due to null controller. Task details: Dummy Business Object Default
Task T00001000 Undefined - You can modify this in the System Reg (Type: UDF, Parent Table Code: Z0, Parent ID: {task.Parent.PK}, PK: {task.PK}). WorkflowProvider: Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow, WorkflowType: DUM.";

				AssertEquals(
					@"There could be some situations when a Parent Form cannot be shown, we should report developer exception and include more details so we can track it down better and do further investigations.
					However, trigger details are not included.",
					messageToAssert.StripTaskIds(), ErrorReporter.LastMessageReported.StripTaskIds());
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Caching Applicable Tags

		static ZGuid GetTagablePK(ProcessHeader workflow, ZString cardType)
		{
			return cardType == CardTypeList.Codes.Workflow ?
				workflow.PK :
				cardType == CardTypeList.Codes.JobLevelWorkflow ?
					workflow.JobHeader.PK :
					workflow.Tasks.First().PK;
		}

		void TestCachingApplicableTagsHelper(ZString cardType)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var tagR = BMSTestHelper.CreateTagMagnitude(tagGroup, "RED", color: Color.Red);
			var tagA = BMSTestHelper.CreateTagMagnitude(tagGroup, "ABL", color: Color.AliceBlue);
			var tagI = BMSTestHelper.CreateTagMagnitude(tagGroup, "IND", color: Color.Indigo);
			var tagN = BMSTestHelper.CreateTagMagnitude(tagGroup, "NAV", color: Color.Navy);
			var tagB = BMSTestHelper.CreateTagMagnitude(tagGroup, "BIS", color: Color.Bisque);
			var tagO = BMSTestHelper.CreateTagMagnitude(tagGroup, "ORN", color: Color.Orange);
			var tagW = BMSTestHelper.CreateTagMagnitude(tagGroup, "WHT", color: Color.Wheat);

			var job1 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job2 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job3 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job4 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job5 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job6 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job7 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job8 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job9 = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var job10 = BMSTestHelper.CreateJob<OrgHeader>(Factory);

			var jobHeader1 = BMSTestHelper.CreateJobHeader(job1, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader(job2, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader(job3, addDefaultProcessHeaderIfNone: false);
			var jobHeader4 = BMSTestHelper.CreateJobHeader(job4, addDefaultProcessHeaderIfNone: false);
			var jobHeader5 = BMSTestHelper.CreateJobHeader(job5, addDefaultProcessHeaderIfNone: false);
			var jobHeader6 = BMSTestHelper.CreateJobHeader(job6, addDefaultProcessHeaderIfNone: false);
			var jobHeader7 = BMSTestHelper.CreateJobHeader(job7, addDefaultProcessHeaderIfNone: false);
			var jobHeader8 = BMSTestHelper.CreateJobHeader(job8, addDefaultProcessHeaderIfNone: false);
			var jobHeader9 = BMSTestHelper.CreateJobHeader(job9, addDefaultProcessHeaderIfNone: false);
			var jobHeader10 = BMSTestHelper.CreateJobHeader(job10, addDefaultProcessHeaderIfNone: false);

			jobHeader1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader3.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader4.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader5.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader6.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader7.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader8.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader9.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			jobHeader10.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "Memory IS RAM"); // the other way around is more accurate, actually...
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "A Fire At A Sea Parks");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader3, "Hello IT");
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader4, "Have you tried turning it off");
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader5, "And on again");
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(jobHeader6, "What do you call");
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(jobHeader7, "Negative One");
			var workflow8 = BMSTestHelper.CreateWorkflowAndTask(jobHeader8, "Sitting alone");
			var workflow9 = BMSTestHelper.CreateWorkflowAndTask(jobHeader9, "In an empty room");
			var workflow10 = BMSTestHelper.CreateWorkflowAndTask(jobHeader10, "Overnumerousness");

			if (cardType == CardTypeList.Codes.JobLevelWorkflow)
			{
				jobHeader1.AddTag(tagR);
				jobHeader2.AddTag(tagA);
				jobHeader3.AddTag(tagI);
				jobHeader4.AddTag(tagN);
				jobHeader5.AddTag(tagB);
				jobHeader6.AddTag(tagO);
				jobHeader7.AddTag(tagW);
				jobHeader8.AddTag(tagR);

				jobHeader9.AddTag(tagA);
				jobHeader9.AddTag(tagI);
				jobHeader9.AddTag(tagN);
				jobHeader10.AddTag(tagA);
				jobHeader10.AddTag(tagI);
				jobHeader10.AddTag(tagN);
			}
			else
			{
				workflow1.AddTag(tagR);
				workflow2.AddTag(tagA);
				workflow3.AddTag(tagI);
				workflow4.AddTag(tagN);
				workflow5.AddTag(tagB);
				workflow6.AddTag(tagO);
				workflow7.AddTag(tagW);
				workflow8.AddTag(tagR);

				workflow9.AddTag(tagA);
				workflow9.AddTag(tagI);
				workflow9.AddTag(tagN);
				workflow10.AddTag(tagA);
				workflow10.AddTag(tagI);
				workflow10.AddTag(tagN);
			}

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item1;
			section.SectionConfiguration.CardType = cardType;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCards = taskPanel.FindAll<TaskCardControl>();
				AssertEquals(10, taskCards.Count());

				var taskCard1Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow1, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard2Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow2, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard3Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow3, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard4Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow4, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard5Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow5, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard6Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow6, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard7Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow7, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard8Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow8, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard9Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow9, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();
				var taskCard10Magnitudes = taskCards.Where(w => w.CardContent.Identifier == GetTagablePK(workflow10, cardType)).Select(c => c.CardContent.ApplicableTagMagnitudes).Single();

				CombineAssertions(() =>
				{
					AssertEquals(1, taskCard1Magnitudes.Count);
					AssertEquals(1, taskCard2Magnitudes.Count);
					AssertEquals(1, taskCard3Magnitudes.Count);
					AssertEquals(1, taskCard4Magnitudes.Count);
					AssertEquals(1, taskCard5Magnitudes.Count);
					AssertEquals(1, taskCard6Magnitudes.Count);
					AssertEquals(1, taskCard7Magnitudes.Count);
					AssertEquals(1, taskCard8Magnitudes.Count);
					AssertEquals(3, taskCard9Magnitudes.Count);
					AssertEquals(3, taskCard10Magnitudes.Count);
				});

				CombineAssertions(() =>
				{
					AssertContainsExactElementsInAnyOrder(new[] { tagR.PK }, taskCard1Magnitudes);
					AssertEquals("The cached tag sets for tagables 1 and 8 should have the same PK value", taskCard1Magnitudes.First(), taskCard8Magnitudes.First());
					Assert("The cached tag sets for tagables 1 and 8 should reference the same set in memory", Object.ReferenceEquals(taskCard1Magnitudes, taskCard8Magnitudes));

					AssertContainsExactElementsInAnyOrder(new[] { tagA.PK, tagI.PK, tagN.PK }, taskCard9Magnitudes);
					AssertContainsExactElementsInAnyOrder("The cached tag sets for tagables 9 and 10 should have the same PK values", taskCard9Magnitudes, taskCard10Magnitudes);
					Assert("The cached tag sets for tagables 9 and 10 should reference the same set in memory", Object.ReferenceEquals(taskCard9Magnitudes, taskCard10Magnitudes));
				});
			}
		}

		public void TestCachingApplicableTags_EnsureUniqueApplicableTagSet_ForWorkflows()
		{
			TestCachingApplicableTagsHelper(CardTypeList.Codes.Workflow);
		}

		public void TestCachingApplicableTags_EnsureUniqueApplicableTagSet_ForTasks()
		{
			TestCachingApplicableTagsHelper(CardTypeList.Codes.Task);
		}

		public void TestCachingApplicableTags_EnsureUniqueApplicableTagSet_ForJobs()
		{
			TestCachingApplicableTagsHelper(CardTypeList.Codes.JobLevelWorkflow);
		}

		#endregion

		#region Tag Tooltip

		public void TestTagTooltip()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(CreateBucket(system));
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			var definion = Factory.New<TagDefinition>();
			definion.TGD_Code = "DAD";
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definion, "MUM", "Help! Aliens are attacking my brain.");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definion, "SIM", "I know precisely what I mean,");
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definion, "TIM", "When I say its a Schpadoinkle Day!");

			workflow.AddTag(magnitude2);
			jobHeader.AddTag(magnitude3);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCard = taskPanel.FindAll<TaskCardControl>().Single();
				taskCard.OnMouseHover_ForTest();

				AssertMultilineASCIIEquals("",
@"Applied tags:
  SIM - I know precisely what I mean,
  TIM - When I say its a Schpadoinkle Day!", ToolTipService.GetToolTip(taskCard));
			}
		}

		public void TestTagTooltip_NoTagsMeansNoTooltip()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(CreateBucket(system));
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCard = taskPanel.FindAll<TaskCardControl>().Single();

				taskCard.OnMouseHover_ForTest();
				AssertEquals(string.Empty, ToolTipService.GetToolTip(taskCard));
			}
		}

		public void TestTagTooltip_DuplicateTagAtMultipleLevels()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(CreateBucket(system));
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			var definion = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude = BMSTestHelper.CreateTagMagnitude(definion, "DAT", "Dat TagMag");
			task.AddTag(magnitude);
			workflow.AddTag(magnitude);
			jobHeader.AddTag(magnitude);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCard = taskPanel.FindAll<TaskCardControl>().Single();

				taskCard.OnMouseHover_ForTest();
				AssertMultilineASCIIEquals("",
@"Applied tags:
  DAT - Dat TagMag", ToolTipService.GetToolTip(taskCard));
			}
		}

		public void TestToolTip_DoesntExplodeOnTagMagnitudeRaceCondition()
		{
			// Create our basic config.
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "PNK", color: Color.HotPink);
			tag.ApplyColorToBackground = true;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.AddTag(tag);
			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			// It's important to have an empty control customisation so we can be lazy about populating the viewModel cache.
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			customisation.FM_Name = "Mai Task Crad";
			customisation.BackgroundColor = ColorList.NameFromColor(Color.LimeGreen);

			var section = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 1, FlowDirectionList.Codes.Down, LastCellList.Codes.Right, ChannelTypeList.Codes.NotChanneled).Item1;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow1);
			var tagDefinitionCache = new TagDefinitionCache(Factory);

			var taskCardContent = new FactorylessCardContent(workflow1, task1, viewModel, new CustomisedControlDataCache(), tagDefinitionCache, new PopulateTaskCardStrategy());

			using (var task1Card = new TaskCardControl(taskCardContent, viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: false))
			{
				AssertEquals(Color.HotPink, task1Card.BackColor);
			}

			tagDefinitionCache.MagnitudesCache.Clear();

			AssertNoExceptionThrown("Now lets pretend the magnitude Cache is empty because of a race condition. We wouldn't want to crash.",
				() =>
				{
					using (var task1Card = new TaskCardControl(taskCardContent, viewModel, new CellContent(0, 0, CellContentType.Cards), canUseBitmapCache: false))
					{
						AssertEquals("We're the wrong colour, but at least nobody died.", Color.LimeGreen, task1Card.BackColor);
					}
				});
		}

		#endregion

		#region Menu Security

		public void TestMenu_TaskCard_TaskAndWorkflowTags_AddSecurity()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEA", "Dave East Reviewed this.");
			BMSTestHelper.CreateTagMagnitude(definition, "HOP", "I hope it's ok?");

			var system = CreateSystem("ORG");

			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			LinkComponents(bucket1, bucket2);

			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_CompletionStatement = "Moar peril";
			var workflow = CreateWorkflow(jobHeader, "Peril", bucket2, releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, "", 0);
			task1.P9_Description = "An Task";

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagAdd);

				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
				AddTagViaMenu(taskCard);

				Assert("Tag should not of been applied", !workflow.TagLinks.Any());
				AssertEquals(@"The selected item could not have the tag applied.

You do not have permission to perform this action. (Organization (XVBQP68SIYXQ) - Peril)", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenu_TaskCard_AddWorkQueue()
		{
			var system = CreateSystem("ORG");

			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			LinkComponents(bucket1, bucket2);

			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_CompletionStatement = "Moar peril";
			var workflow = CreateWorkflow(jobHeader, "Peril", bucket2, releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, "", 0);
			task1.P9_Description = "An Task";

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
				AddTagViaMenu(taskCard);

				using (var workQueueForm = Application.OpenForms.OfType<WorkQueueForm>().SingleOrDefault())
				{
					AssertNotNull(workQueueForm);
				}
			}
		}

		public static void AddTagViaMenu(TaskCardControl taskCard)
		{
			var addTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().First(x => x.Text == "Add Tag");
			addTagMenuItem.ShowDropDown();
			var workflowMenuItem = addTagMenuItem.DropDownItems.ToList<ToolStripMenuItem>().First();
			workflowMenuItem.ShowDropDown();
			var tagGroupMenuItem = workflowMenuItem.DropDownItems.ToList<ToolStripMenuItem>().First();
			tagGroupMenuItem.ShowDropDown();
			var tagMenuItem = tagGroupMenuItem.DropDownItems.ToList<ToolStripMenuItem>().First();
			tagGroupMenuItem.ShowDropDown();
			tagMenuItem.PerformClick();
		}

		public void TestMenu_TaskCard_TaskAndWorkflowTags_RemoveSecurity()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEA", "Dave East Reviewed this.");
			var tag = BMSTestHelper.CreateTagMagnitude(definition, "HOP", "I hope it's ok?");

			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			LinkComponents(bucket1, bucket2);

			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_CompletionStatement = "Moar peril";
			var workflow = CreateWorkflow(jobHeader, "Peril", bucket2, releaseGroupPK: group.PK);
			workflow.AddTag(tag);

			var task1 = CreateTask(workflow, "", 0);
			task1.P9_Description = "An Task";

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagRemove);

				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
				var addTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().First(x => x.Text == "Remove Tag");
				addTagMenuItem.ShowDropDown();
				var workflowMenuItem = addTagMenuItem.DropDownItems.ToList<ToolStripMenuItem>().First();
				workflowMenuItem.ShowDropDown();
				var tagGroupMenuItem = workflowMenuItem.DropDownItems.ToList<ToolStripMenuItem>().First();
				tagGroupMenuItem.ShowDropDown();
				var tagMenuItem = tagGroupMenuItem.DropDownItems.ToList<ToolStripMenuItem>().First();
				tagGroupMenuItem.ShowDropDown();
				tagMenuItem.PerformClick();

				Assert("Tag should not of been removed", workflow.TagLinks.Any());
				AssertEquals(@"The selected item could not have the tag removed.

You do not have permission to perform this action. (Organization (XVBQP68SIYXQ) - Peril)", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Context Menu

		public void TestMenuIsLazy()
		{
			TaskCardControl.AutoGenerateTaskMenuItems.Value = false;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			var component = BMSTestHelper.CreateBuffer(system);
			section.MS_FC_Component = component.PK;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var card = new TaskCardControl(task1, viewModel))
			{
				AssertEquals(1, card.ContextMenuStrip.Items.Count);
				card.TaskCardContextMenuStrip_OnOpening_ForTest();
				AssertNotEquals(1, card.ContextMenuStrip.Items.Count);
			}
		}

		public void TestMenuGracefullyHandlesDeletedTasks()
		{
			TaskCardControl.AutoGenerateTaskMenuItems.Value = false;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");

			var section = config.BufferSection;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var card = new TaskCardControl(task1, viewModel))
			{
				task1.Delete();
				Factory.Save();

				card.TaskCardContextMenuStrip_OnOpening_ForTest();

				Assert(card.IsDisposed);
				AssertEquals("This card's task has been deleted and will now be removed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Move to Component

		public void TestMoveToComponentContextMenu_ShouldRemoveFromSourceSectionAndMoveToNewSection()
		{
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var system = CreateSystem("ORG");
			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var bucket3 = CreateBucket(system, "bucket3");
			var bucket4 = CreateBucket(system, "bucket4");

			LinkComponents(bucket1, bucket2);
			LinkComponents(bucket2, bucket3);
			LinkComponents(bucket3, bucket4);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", releaseGroupPK: group.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var board = CreateBoard(system);
			var section1 = CreateBoardSection(bucket1, board, 0, 0, 50, 50);
			var section2 = CreateBoardSection(bucket2, board, 0, 1, 50, 50);
			var section3 = CreateBoardSection(bucket3, board, 1, 0, 50, 50);
			var section4 = CreateBoardSection(bucket4, board, 1, 1, 50, 50);

			section1.SectionConfiguration.ReleaseGroupPK = group.PK;
			section2.SectionConfiguration.ReleaseGroupPK = group.PK;
			section3.SectionConfiguration.ReleaseGroupPK = group.PK;
			section4.SectionConfiguration.ReleaseGroupPK = group.PK;

			section3.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = bucket2.PK;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl1 = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section1.PK);
				var sectionControl2 = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section2.PK);
				var sectionControl3 = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section3.PK);
				var sectionControl4 = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section4.PK);

				var taskCard = sectionControl1.FindAll<TaskCardControl>().Single();
				AssertEquals(0, sectionControl2.FindAll<TaskCardControl>().Count());
				AssertEquals(0, sectionControl3.FindAll<TaskCardControl>().Count());
				AssertEquals(0, sectionControl4.FindAll<TaskCardControl>().Count());

				taskCard.MoveToComponent_ForTest(bucket2);
				Application.DoEvents();

				AssertEquals(bucket2, workflow.CurrentComponent);

				var taskCardInSection2 = sectionControl2.FindAll<TaskCardControl>().SingleOrDefault();
				var taskCardInSection3 = sectionControl3.FindAll<TaskCardControl>().SingleOrDefault();

				CombineAssertions(() =>
				{
					AssertEquals("Ticket should be removed from initial section", 0, sectionControl1.FindAll<TaskCardControl>().Count());
					AssertEquals("Ticket should not be present in unrelated section", 0, sectionControl4.FindAll<TaskCardControl>().Count());

					AssertNotNull("Ticket should be in section for destination component", taskCardInSection2);
					AssertNotNull("Ticket should also be in section with destination component as an additional component", taskCardInSection3);
				});

				taskCardInSection3.MoveToComponent_ForTest(bucket1);
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertNotNull("Ticket should be in section for destination component", sectionControl1.FindAll<TaskCardControl>().SingleOrDefault());
					AssertEquals("Ticket should be removed from initial section2", 0, sectionControl2.FindAll<TaskCardControl>().Count());
					AssertEquals("Ticket should be removed from initial section3", 0, sectionControl3.FindAll<TaskCardControl>().Count());
					AssertEquals("Ticket should not be present in unrelated section", 0, sectionControl4.FindAll<TaskCardControl>().Count());
				});
			}
		}

		public void TestMoveToComponentContextMenu_ShouldNotRefreshCellsMultipleTimes()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = CreateBoard(config.System);
			var section1 = CreateBoardSection(config.Bucket, board, row: 0);
			var section2 = CreateBoardSection(config.Buffer, board, row: 1);
			section2.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl1 = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section1.PK);
				var sectionControl2 = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section2.PK);

				var section1Cell = sectionControl1.ViewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.Cards);
				var section2Cell = sectionControl2.ViewModel.ComponentGrid.Cells.First(c => c.ContentType == CellContentType.Cards);

				var section1RefreshCount = 0;
				var section2RefreshCount = 0;

				section1Cell.ContentRefreshed += (s, e) => section1RefreshCount++;
				section2Cell.ContentRefreshed += (s, e) => section2RefreshCount++;

				var taskCard = sectionControl1.FindAll<TaskCardControl>().Single();
				taskCard.MoveToComponent_ForTest(config.Buffer);
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertEquals("Should refresh section1 cell once to remove the ticket", 1, section1RefreshCount);
					AssertEquals("Should refresh section2 cell once to add the ticket", 1, section2RefreshCount);
				});

				AssertEquals(0, sectionControl1.FindAll<TaskCardControl>().Count());
				AssertNotNull(sectionControl2.FindAll<TaskCardControl>().SingleOrDefault());
			}
		}

		public void TestMoveToComponentContextMenu_ForJobCard_ShouldShowAllComponents()
		{
			var board = SetUpMoveToComponentTestObjects(CardTypeList.Codes.JobLevelWorkflow);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var tickets = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, tickets.Length);
				AssertMoveToComponentMenuItems("Job cards are showing, so all components should be listed, and yet...", new[] { "bucket", "buffer", "buffer 2" }, tickets[0]);
				AssertMoveToComponentMenuItems("Job cards are showing, so all components should be listed, and yet...", new[] { "bucket", "buffer", "buffer 2" }, tickets[1]);
			}
		}

		public void TestMoveToComponentContextMenu_ForWorkflowCard_ShouldNotShowCurrentComponent()
		{
			var board = SetUpMoveToComponentTestObjects(CardTypeList.Codes.Workflow);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var ticket = form.FindSingle<TaskCardControl>(x => x.CardContent.WorkflowType == "ORG");
				AssertMoveToComponentMenuItems("Workflow cards are showing, so the clicked workflow's component should not be listed, and yet...", new[] { "bucket", "buffer 2" }, ticket);

				ticket = form.FindSingle<TaskCardControl>(x => x.CardContent.WorkflowType == "INQ");
				AssertMoveToComponentMenuItems("Workflow cards are showing, so the clicked workflow's component should not be listed, and yet...", new[] { "bucket", "buffer" }, ticket);
			}
		}

		public void TestMoveToComponentContextMenu_ForTaskCard_ShouldNotShowCurrentComponent()
		{
			var board = SetUpMoveToComponentTestObjects(CardTypeList.Codes.Task);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var ticket = form.FindSingle<TaskCardControl>(x => x.CardContent.NoteText == "Task 1");
				AssertMoveToComponentMenuItems("Task cards are showing, so the clicked tasks's workflow's component should not be listed, and yet...", new[] { "bucket", "buffer 2" }, ticket);

				ticket = form.FindSingle<TaskCardControl>(x => x.CardContent.NoteText == "Task 2");
				AssertMoveToComponentMenuItems("Task cards are showing, so the clicked task's workflow's component should not be listed, and yet...", new[] { "bucket", "buffer" }, ticket);
			}
		}

		BMBoard SetUpMoveToComponentTestObjects(string cardType)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { "ORG", "INQ" });
			var buffer2 = BMSTestHelper.CreateBuffer(config.System, "buffer 2");
			var board = CreateBoard(config.System);
			var section = CreateBoardSection(config.Buffer, board);
			section.SectionConfiguration.CardType = cardType;
			BMSTestHelper.CreateAdditionalComponent(section, buffer2);

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader1, "workflow 1", config.Buffer);
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			task1.P9_CardNote = "Task 1";

			var jobHeader2 = CreateJobHeader<SalesEnquiry>(addDefaultProcessHeaderIfNone: false);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow 2", buffer2);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			task2.P9_CardNote = "Task 2";

			Factory.Save();

			return board;
		}

		void AssertMoveToComponentMenuItems(string message, string[] expectedItems, TaskCardControl ticket)
		{
			ticket.ShouldPreloadSubMenus_ForTest = true;
			ticket.TaskCardContextMenuStrip_OnOpening_ForTest();
			var strip = ticket.ContextMenuStrip;
			var item = (ZToolStripMenuItem)strip.Items.Find("MoveToComponentToolStripMenuItem", false).Single();
			var items = item.DropDownItems.ToList<ZToolStripMenuItem>().Select(x => x.Text);

			AssertContainsExactElementsInAnyOrder(message, expectedItems, items);
		}

		public void TestDeferJob_ShouldPassAllSectionComponentsToViewModel()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3");
			var bucket4 = BMSTestHelper.CreateBucket(system, "bucket4");
			BMSTestHelper.LinkComponents(bucket1, bucket2);
			BMSTestHelper.LinkComponents(bucket2, bucket3);
			BMSTestHelper.LinkComponents(bucket3, bucket4);

			var section = BMSTestHelper.CreateBoardSection(bucket2);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			BMSTestHelper.CreateAdditionalComponent(section, bucket3);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader.FH_GG_ReleaseGroup = group.PK;
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1", bucket2);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2", bucket2);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow3", bucket3);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow4", bucket4);

			workflow1.FH_GG_ReleaseGroup = group.PK;
			workflow2.FH_GG_ReleaseGroup = group.PK;
			workflow3.FH_GG_ReleaseGroup = group.PK;
			workflow4.FH_GG_ReleaseGroup = group.PK;

			Factory.Save();

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var card = form.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNotNull(card);

				IEnumerable<ZString> workflowsToDeferByDefault = Array.Empty<ZString>();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					workflowsToDeferByDefault = ((DeferWorkflowForm)dialog).DataSource.WorkflowsToDefer
						.Cast<WorkflowToDeferBusinessObject>()
						.Where(x => x.ActionToBeTaken == WorkflowDeferalActionList.Codes.Defer)
						.Select(x => x.ProcessHeader)
						.Select(x => x.FH_CompletionStatement);
				});

				card.ContextMenuStrip.Items.Find("DeferToolStripMenuItem", false)[0].PerformClick();

				AssertContainsExactElementsInAnyOrder("Only workflows from the source componnets of the section should have been selected for deferral by default, and yet...", new[] { "workflow1", "workflow2", "workflow3" }, workflowsToDeferByDefault);
			}
		}

		#endregion

		#region Tags

		public void TestMenu_TaskCard_TaskAndWorkflowTags()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEA", "Dave East Reviewed this.");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "HOP", "I hope it's ok?");

			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var link = LinkComponents(bucket1, bucket2);

			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_CompletionStatement = "Moar peril";
			var workflow = CreateWorkflow(jobHeader, "Peril", bucket2, releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, "", 0);
			task1.P9_Description = "An Task";

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
				AssertCollectionContains(taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>(), x => x.Text == "Add Tag");
				AssertCollectionContains(taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>(), x => x.Text == "Remove Tag");

				var addTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag");
				AssertCollectionContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Workflow [Peril]");
				AssertCollectionContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Job [Moar peril]");
				AssertCollectionContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Task [An Task]");

				var removeTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Remove Tag");
				AssertCollectionContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Workflow [Peril]");
				AssertCollectionContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Job [Moar peril]");
				AssertCollectionContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Task [An Task]");
			}
		}

		public void TestMenu_WorkflowCard_TaskAndWorkflowTags()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AE", "Eagle rhymes with Beagle");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "VAN", "Do nice thinks ok?");

			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var link = LinkComponents(bucket1, bucket2);

			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_CompletionStatement = "Moar peril";
			var workflow = CreateWorkflow(jobHeader, "Peril", bucket2, releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, "", 0);
			task1.P9_Description = "An Task";

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
				AssertCollectionContains(taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>(), x => x.Text == "Add Tag");
				AssertCollectionContains(taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>(), x => x.Text == "Remove Tag");

				var addTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag");
				AssertCollectionContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Workflow [Peril]");
				AssertCollectionContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Job [Moar peril]");
				AssertCollectionNotContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Task [An Task]");

				var removeTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Remove Tag");
				AssertCollectionContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Workflow [Peril]");
				AssertCollectionContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Job [Moar peril]");
				AssertCollectionNotContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Task [An Task]");
			}
		}

		public void TestMenu_JobWorkflowCard()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "TTT", "Test");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "TT1", "Test1");

			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var link = LinkComponents(bucket1, bucket2);

			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket2, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_CompletionStatement = "Moar peril";
			jobHeader.FH_GG_ReleaseGroup = group.PK;
			var workflow = CreateWorkflow(jobHeader, "Peril", bucket2, releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, "", 0);
			task1.P9_Description = "An Task";

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
				AssertCollectionContains(taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>(), x => x.Text == "Add Tag");
				AssertCollectionContains(taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>(), x => x.Text == "Remove Tag");

				var addTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag");
				AssertCollectionNotContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Workflow [Peril]");
				AssertCollectionContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Job [Moar peril]");
				AssertCollectionNotContains(addTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Task [An Task]");

				var removeTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Remove Tag");
				AssertCollectionNotContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Workflow [Peril]");
				AssertCollectionContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Job [Moar peril]");
				AssertCollectionNotContains(removeTagMenuItem.DropDownItems.OfType<TagToolStripMenuTree>(), x => x.Text == "Task [An Task]");

				var taskDetailsMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(x => x.Text == "Task Details");
				AssertNull(taskDetailsMenuItem);

				var transferMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(x => x.Text == "Transfer Diagnosis");
				AssertNull(transferMenuItem);
			}
		}

		public void TestMenu_AvailableTags_ShouldFilterInactiveTags()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEA", "IAmA Dave East. AMA!!!1");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "ANY", "And I mean ANYTHING");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "RLY", ";)", isActive: false);
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "NAH", "LOL JK", isActive: false);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			config.BucketSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Around about the same amount of peril, please.");
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = CreateWorkflow(jobHeader, "Ready for more Peril now");
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "An Task");

			workflow.AddTag(magnitude2);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BucketBoard)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();

				var addTagMenuItem = (TagToolStripMenuTree)taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag").DropDownItems[0];
				var removeTagMenuItem = (TagToolStripMenuTree)taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Remove Tag").DropDownItems[0];

				addTagMenuItem.OnDropDownOpening_ForTest();
				removeTagMenuItem.OnDropDownOpening_ForTest();

				var addMagnitudeMenuItem = (ZToolStripMenuItem)addTagMenuItem.DropDownItems[0];
				var removeMagnitudeMenuItem = (ZToolStripMenuItem)removeTagMenuItem.DropDownItems[0];

				AssertEquals("DEA - IAmA Dave East. AMA!!!1", addMagnitudeMenuItem.Text);
				AssertEquals("DEA - IAmA Dave East. AMA!!!1", removeMagnitudeMenuItem.Text);

				AssertEquals("ANY - And I mean ANYTHING", addMagnitudeMenuItem.DropDownItems[0].Text);
				AssertEquals("Should only contain active magnitudes", 1, addMagnitudeMenuItem.DropDownItems.Count);

				AssertEquals("", "RLY - ;)", removeMagnitudeMenuItem.DropDownItems[0].Text);
				AssertEquals("Should contain the magnitudes currently applied to the item - even if it's inactive", 1, removeMagnitudeMenuItem.DropDownItems.Count);
			}
		}

		public void TestMenu_AvailableTags_ShouldFilterInactiveTagsDynamically()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEA", "IAmA Dave East. AMA!!!1");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "ANY", "And I mean ANYTHING");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "RLY", "I'm just copying from previous tests", isActive: false);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			config.BucketSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Around about the same amount of peril, please.");
			var workflow = CreateWorkflow(jobHeader, "Ready for more Peril now", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "An Task");

			workflow.AddTag(magnitude2);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BucketBoard)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();

				var addTagMenuItem = (TagToolStripMenuTree)taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag").DropDownItems[0];
				addTagMenuItem.OnDropDownOpening_ForTest();

				var addMagnitudeMenuItem = (ZToolStripMenuItem)addTagMenuItem.DropDownItems[0];

				AssertEquals("DEA - IAmA Dave East. AMA!!!1", addMagnitudeMenuItem.Text);
				AssertEquals("Should only contain active magnitudes", 1, addMagnitudeMenuItem.DropDownItems.Count);
				AssertEquals("ANY - And I mean ANYTHING", addMagnitudeMenuItem.DropDownItems[0].Text);

				magnitude1.TGM_IsActive = false;
				magnitude2.TGM_IsActive = true;

				magnitude1.Factory.Save();

				addTagMenuItem.OnDropDownOpening_ForTest();
				addMagnitudeMenuItem = (ZToolStripMenuItem)addTagMenuItem.DropDownItems[0];

				AssertEquals("DEA - IAmA Dave East. AMA!!!1", addMagnitudeMenuItem.Text);
				AssertEquals("Should only contain active magnitudes", 1, addMagnitudeMenuItem.DropDownItems.Count);
				AssertEquals("RLY - I'm just copying from previous tests", addMagnitudeMenuItem.DropDownItems[0].Text);
			}
		}

		public void TestMenu_AvailableTags_ShouldShowUniqueMenus()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEA", "IAmA Dave East. AMA!!!1");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "ANY", "And I mean ANYTHING");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "RLY", "I'm just copying from previous tests", isActive: false);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			config.BucketSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Around about the same amount of peril, please.");
			var workflow = CreateWorkflow(jobHeader, "Ready for more Peril now", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "An Task");

			workflow.AddTag(magnitude2);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BucketBoard)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();

				var addTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag");
				var removeTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Remove Tag");

				AssertEquals(3, addTagMenuItem.DropDownItems.Count);
				AssertEquals("Workflow [Ready for more Peril now]", addTagMenuItem.DropDownItems[0].Text);
				AssertEquals("Job [Around about the same amoun...]", addTagMenuItem.DropDownItems[1].Text);
				AssertEquals("Task [An Task]", addTagMenuItem.DropDownItems[2].Text);

				AssertEquals(3, removeTagMenuItem.DropDownItems.Count);
				AssertEquals("Workflow [Ready for more Peril now]", removeTagMenuItem.DropDownItems[0].Text);
				AssertEquals("Job [Around about the same amoun...]", removeTagMenuItem.DropDownItems[1].Text);
				AssertEquals("Task [An Task]", removeTagMenuItem.DropDownItems[2].Text);

				taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();

				addTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag");
				removeTagMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Remove Tag");

				AssertEquals(3, addTagMenuItem.DropDownItems.Count);
				AssertEquals("Workflow [Ready for more Peril now]", addTagMenuItem.DropDownItems[0].Text);
				AssertEquals("Job [Around about the same amoun...]", addTagMenuItem.DropDownItems[1].Text);
				AssertEquals("Task [An Task]", addTagMenuItem.DropDownItems[2].Text);

				AssertEquals(3, removeTagMenuItem.DropDownItems.Count);
				AssertEquals("Workflow [Ready for more Peril now]", removeTagMenuItem.DropDownItems[0].Text);
				AssertEquals("Job [Around about the same amoun...]", removeTagMenuItem.DropDownItems[1].Text);
				AssertEquals("Task [An Task]", removeTagMenuItem.DropDownItems[2].Text);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestMenu_AvailableTags_ShouldFilterInactiveTagsDynamically_DBHits()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "DEF", "TagDefinition");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "TG1", "Tag1");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "TG2", "Tag2");
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "TG3", "Tag3");

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			config.BucketSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Around about the same amount of peril, please.");
			var workflow = CreateWorkflow(jobHeader, "Ready for more Peril now", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "An Task");

			Factory.Save();

			TaskCardControl.AutoGenerateTaskMenuItems.Value = false; // Force constructing the menu tree when we're actually testing its db hits.

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(config.BucketBoard)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();

				TagToolStripMenuTree addTagMenuItem;

				var hits = new Dictionary<string, int>
				{
					{ ProcessHeaderSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 }, // 1 hit for showing Assist With This Task and Add Assistance Task For menu items (AssistWithThisTaskHelper.IsEnabledForWorkflowType)
				};

				using (AssertDbHitsForAllFactories(hits, includeFactoryPredicate: f => f.NameForDebugging == "TaskCardControl.MenuItems" || f.NameForDebugging == "TagDefinitionCache"))
				{
					taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();

					addTagMenuItem = (TagToolStripMenuTree)taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Add Tag").DropDownItems[0];
					addTagMenuItem.OnDropDownOpening_ForTest();

					var addMagnitudeMenuItem = (ZToolStripMenuItem)addTagMenuItem.DropDownItems[0];

					AssertEquals("DEF - TagDefinition", addMagnitudeMenuItem.Text);
					AssertEquals("Should only contain active magnitudes", 3, addMagnitudeMenuItem.DropDownItems.Count);
					AssertEquals("TG1 - Tag1", addMagnitudeMenuItem.DropDownItems[0].Text);
					AssertEquals("TG2 - Tag2", addMagnitudeMenuItem.DropDownItems[1].Text);
					AssertEquals("TG3 - Tag3", addMagnitudeMenuItem.DropDownItems[2].Text);
				}

				magnitude1.TGM_IsActive = false;
				magnitude2.TGM_IsActive = true;

				Factory.Save();

				hits = new Dictionary<string, int>
				{
					{ ProcessHeaderSchema.Constants.TableName, 0 },
					{ ProcessTasksSchema.Constants.TableName, 0 },
				};

				using (AssertDbHitsForAllFactories(hits, includeFactoryPredicate: f => f.NameForDebugging == "TaskCardControl.MenuItems" || f.NameForDebugging == "TagDefinitionCache"))
				{
					addTagMenuItem.OnDropDownOpening_ForTest();
					var addMagnitudeMenuItem = (ZToolStripMenuItem)addTagMenuItem.DropDownItems[0];

					AssertEquals("DEF - TagDefinition", addMagnitudeMenuItem.Text);
					AssertEquals("Should only contain active magnitudes", 2, addMagnitudeMenuItem.DropDownItems.Count);
					AssertEquals("TG2 - Tag2", addMagnitudeMenuItem.DropDownItems[0].Text);
					AssertEquals("TG3 - Tag3", addMagnitudeMenuItem.DropDownItems[1].Text);
				}
			}
		}

		public void TestAddTagMenuItem_OnClick_FromNewThread_ShouldNotCauseThreadSentryException()
		{
			AssertTagMenuItem_OnClick_FromNewThread_ShouldNotCauseThreadSentryException(isRemovingTag: false);
		}

		public void TestRemoveTagMenuItem_OnClick_FromNewThread_ShouldNotCauseThreadSentryException()
		{
			AssertTagMenuItem_OnClick_FromNewThread_ShouldNotCauseThreadSentryException(isRemovingTag: true);
		}

		void AssertTagMenuItem_OnClick_FromNewThread_ShouldNotCauseThreadSentryException(bool isRemovingTag)
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var def = BMSTestHelper.CreateTagDefinition(Factory, "TVL", "It's not a ban");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "BAN", "That's your word!");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow);

			if (isRemovingTag)
			{
				workflow.AddTag(mag);
			}

			var section = config.BufferSection;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var cardContent = new TaskCardContent(task, viewModel);
			var grove = TaskCardMenuActionProvider.GetTagMenuItemTrees(Factory, workflow, null, cardContent, viewModel);
			var tree = grove[isRemovingTag ? 1 : 0];

			tree.OnDropDownOpening_ForTest();
			var defItem = tree.DropDownItems.OfType<ZToolStripMenuItem>().Single(x => x.Text == "TVL - It's not a ban");
			var tagItem = defItem.DropDownItems[0];

			Factory.RelinquishThreadOwnership();

			Task.Factory.StartNew(() =>
			{
				using (CargoWise.Data.Db.DisposableActionForDbConnection())
				{
					Factory.TakeThreadOwnership();
					tagItem.PerformClick();
					Factory.RelinquishThreadOwnership();
				}
			}).Wait();

			Application.DoEvents();
			Factory.TakeThreadOwnership();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var tags = loadedWorkflow.GetApplicableTags();
			var expected = isRemovingTag ? Array.Empty<ZString>() : new[] { mag.TGM_Description };
			AssertContainsExactElementsInAnyOrder("The menu item should have added/removed the tag, and yet...", expected, tags.Select(x => x.TGM_Description));
		}

		#endregion

		#region Copy Job ID/Hyperlink

		public void TestMenu_CopyJobLinkAndID()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket = CreateBucket(system);

			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket, board);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>();
			jobHeader.FH_CompletionStatement = "Moar peril";
			var workflow = CreateWorkflow(jobHeader, "Peril", bucket, releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, "", 0);
			task1.P9_Description = "An Task";

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().Single();
				AssertNotNull(taskCard);

				taskCard.ShowParentWorkflow();
				Application.DoEvents();

				var helper = new ClipboardTestHelper();
				using (helper.MockClipboard())
				using (var formCreated = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNotNull("formCreated", formCreated);

					var actionMenuItemsProvider = formCreated as IFileMenuItemsProvider;
					var actionsMenu = actionMenuItemsProvider.ActionsMenuItem;
					actionsMenu.OnPopup(EventArgs.Empty);
					var menuItem = actionsMenu.MenuItems.FindByName(ZFormMenuStrategy.CopyIdToClipboardName);
					AssertNotNull("menuItem", menuItem);
					menuItem.PerformClick();

					ZFormMenuStrategy.CopyHyperlinkToClipboard(formCreated);
					var hyperlink = helper.ClipboardData;
					AssertNotNull("hyperlink", hyperlink);
					var hyperlinkText = hyperlink.GetData(DataFormats.Text);
					var hyperlinkHtml = hyperlink.GetData(DataFormats.Html);
					var hyperlinkRtf = hyperlink.GetData(DataFormats.Rtf);

					// Copy Hyperlink
					taskCard.ContextMenuStrip.Show(new Point(0, 0));
					var copyHyperlinkMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Copy Job Hyperlink to Clipboard");
					AssertNotNull("copyHyperlinkMenuItem", copyHyperlinkMenuItem);
#if !WINZOR
					AssertEquals(true, copyHyperlinkMenuItem.Visible);
#endif

					copyHyperlinkMenuItem.PerformClick();
					var hyperlinkFromBoard = helper.ClipboardData;
					AssertNotNull("hyperlinkFromBoard", hyperlinkFromBoard);
					AssertEquals(hyperlinkText, hyperlinkFromBoard.GetData(DataFormats.Text));
					AssertEquals(hyperlinkHtml, hyperlinkFromBoard.GetData(DataFormats.Html));
					AssertEquals(hyperlinkRtf, hyperlinkFromBoard.GetData(DataFormats.Rtf));

					// Copy Id
					taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
					var copyIdMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Copy Job ID to Clipboard");
					AssertNotNull("copyIdMenuItem", copyIdMenuItem);

					copyIdMenuItem.PerformClick();
					var idFromBoard = helper.ClipboardData;
					AssertNotNull("idFromBoard", idFromBoard);

					var org = jobHeader.Parent as OrgHeader;
					AssertEquals(org.HumanReadableItemCode, idFromBoard.GetData(DataFormats.Text));

					// Copy Readable name
					taskCard.TaskCardContextMenuStrip_OnOpening_ForTest();
					var copyNameMenuItem = taskCard.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(x => x.Text == "Copy Job Name to Clipboard");
					AssertNotNull("copyNameMenuItem", copyNameMenuItem);

					copyNameMenuItem.PerformClick();
					var nameFromBoard = helper.ClipboardData;
					AssertNotNull("nameFromBoard", nameFromBoard);
					AssertEquals(org.HumanReadableName, nameFromBoard.GetData(DataFormats.Text));
				}
			}
		}

#endregion

#endregion

		#region Open Job

		public void TestNoExceptions_OpenJobWithoutSecurityRights()
		{
			var system = BMSTestHelper.GetOrCreateSystem(Factory, "ORG");

			var staff = Env.CurrentUser as GlbStaff;
			Env.Security.OrganisationView.IsAllowed = false;
			Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned.IsAllowed = false;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, 90);

			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			using (var control = new TaskCardControl(task, viewModel))
			{
				control.ShowParentWorkflow();
				AssertMultilineASCIIEquals("Task not shown reason", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> View", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowParentWorkflowFromTaskCard_WithTaskAndWorkflow_ButNoJob()
		{
			var system = BMSTestHelper.GetOrCreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			var staff = Env.CurrentUser as GlbStaff;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			Db.Connection.ExecuteNonQuery($"delete from dbo.OrgHeader where OH_PK = '{jobHeader.Parent.PK}'");

			using (var control = new TaskCardControl(task, viewModel))
			{
				control.ShowParentWorkflow();
				Application.DoEvents();

				using (var openedForm = Application.OpenForms.OfType<TaskManagementForm>().Single())
				{
					AssertNotNull(openedForm);

					var tabControl = openedForm.FindAll<ZTemplateTabControl>().First(t => t.Name == "MainTabControl");
					var tabPage = openedForm.FindAll<ZTabPage>().Single(t => t.Name == "MainTabPage");

					AssertEquals(tabPage, tabControl.SelectedTab);
				}
			}
		}

		public void TestShowParentWorkflowFromWorkflowCard_WithTaskAndWorkflow_ButNoJob()
		{
			var system = BMSTestHelper.GetOrCreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var staff = Env.CurrentUser as GlbStaff;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section, workflow);

			Db.Connection.ExecuteNonQuery($"delete from dbo.OrgHeader where OH_PK = '{jobHeader.Parent.PK}'");

			using (var control = new TaskCardControl(task, viewModel))
			{
				control.ShowParentWorkflow();
				Application.DoEvents();

				using (var openedForm = Application.OpenForms.OfType<TaskManagementForm>().Single())
				{
					AssertNotNull(openedForm);

					var tabControl = openedForm.FindAll<ZTemplateTabControl>().First(t => t.Name == "MainTabControl");
					var tabPage = openedForm.FindAll<ZTabPage>().Single(t => t.Name == "WorkflowDetailsTabPage");

					AssertEquals(tabPage, tabControl.SelectedTab);
				}
			}
		}

		public void TestDoubleClick_OnJobTicket_ShouldSelectJobWorkflow_SalesEnquiry()
		{
			AssertJobWorkflowSelected<SalesEnquiry, SalesEnquiryForm>(ticket => ticket.OnDoubleClick());
		}

		public void TestDoubleClick_OnJobTicket_ShouldSelectJobWorkflow_OrgHeader() // Because ZOrganizationsForm does it differently
		{
			AssertJobWorkflowSelected<OrgHeader, ZOrganisationsForm>(ticket => ticket.OnDoubleClick());
		}

		public void TestContextMenu_OpenJob_ForJobTicket_ShouldSelectJobWorkflow_SalesEnquiry()
		{
			AssertContextMenuOpenForm_JobWorkflowSelected<SalesEnquiry, SalesEnquiryForm>();
		}

		public void TestContextMenu_OpenJob_ForJobTicket_ShouldSelectJobWorkflow_OrgHeader() // Because ZOrganizationsForm does it differently
		{
			AssertContextMenuOpenForm_JobWorkflowSelected<OrgHeader, ZOrganisationsForm>();
		}

		void AssertContextMenuOpenForm_JobWorkflowSelected<TJobType, TFormType>()
			where TJobType : BusinessObject, IWorkflowProvider
			where TFormType : Form
		{
			AssertJobWorkflowSelected<TJobType, TFormType>(ticket =>
			{
				ticket.ShouldPreloadSubMenus_ForTest = true;
				ticket.TaskCardContextMenuStrip_OnOpening_ForTest();
				var strip = ticket.ContextMenuStrip;
				var item = (ZToolStripMenuItem)strip.Items.Find("OpenJobToolStripMenuItem", false).Single();
				item.PerformClick();
			});
		}

		void AssertJobWorkflowSelected<TJobType, TFormType>(Action<TaskCardControl> openJobAction)
			where TJobType : BusinessObject, IWorkflowProvider
			where TFormType : Form
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, new[] { "ORG", "INQ" });
			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var jobHeader = BMSTestHelper.CreateJobHeader<TJobType>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow", config.Bucket);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.BucketBoard)))
			{
				form.Show();
				Application.DoEvents();

				var ticket = form.FindSingle<TaskCardControl>();
				TFormType jobForm = null;

				try
				{
					openJobAction(ticket);
					Application.DoEvents();

					jobForm = BMSFormTestHelper.GetOpenForms<TFormType>().Single();
					var grid = jobForm.FindSingle<ZGrid>(x => x.Name == "WorkflowsGrid");
					var selected = (BusinessObject)grid.ListManager.Current;

					AssertEquals("The job header should have been selected instead of the workflow, and yet...", jobHeader.PK, selected.PK);
				}
				finally
				{
					jobForm?.Dispose();
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}

	#region NonTransactionedTestCase

	class TaskCardControlNonTransactionedTest : NonTransactionedTestCase
	{
		#region Task Details Form

		public void TestShowTaskDetailsForm_ShouldNotCauseThreadSentryExceptionWhenLoadingSecurityCheckpoint()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)shipment, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow", config.Buffer);
			var section = config.BufferSection;
			Factory.Save();

			AssertEquals("GIVEN task exist", 1, workflow.Tasks.Count());

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var task = workflow.Tasks.First();
				var taskCard = form.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == task.PK);
				AssertNotNull("GIVEN task card is shown", taskCard);
				AssertNull("GIVEN taskDetailsForm not shown", taskCard.taskDetailsForm);

				taskCard.ContextMenuStrip.Items.Find("TaskDetailsToolStripMenuItem", false)[0].PerformClick();
				form.AwaitAll();

				AssertNotNull("WHEN click task-detail THEN taskDetailsForm should be shown", taskCard.taskDetailsForm);

				taskCard.taskDetailsForm.Dispose();

				AssertNull("WHEN showing Task Details Form by clicking TaskDetailsToolStripMenuItem, THEN should not cause ThreadSentry Exception i.e. loading security checkpoint", ErrorReporter.LastExceptionReported);

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();
				AssertNull("WHEN refresh, THEN should not cause ThreadSentry Exception i.e. loading security checkpoint", ErrorReporter.LastExceptionReported);

				BMSFormTestHelper.PressHotkeys(form, Keys.F5 | Keys.Shift);
				form.AwaitAll();
				AssertNull("WHEN reload, THEN should not cause ThreadSentry Exception i.e. loading security checkpoint", ErrorReporter.LastExceptionReported);

				task.P9_Description = "updated-1x";
				Factory.Save();
				form.AwaitAll();

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();
				var bmComponentControl = form.FindAll<BMComponentControl>().Single();
				var cardControl = bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == task.PK);
				AssertEquals("WHEN update and refresh", "updated-1x", ((BusinessObject)cardControl.CardContent.Bindable)["TSK_P9_Description"]);
				AssertNull("THEN should not cause ThreadSentry Exception i.e. loading security checkpoint", ErrorReporter.LastExceptionReported);

				task.P9_Description = "updated-2x";
				Factory.Save();
				form.AwaitAll();

				BMSFormTestHelper.PressHotkeys(form, Keys.F5 | Keys.Shift);
				form.AwaitAll();
				bmComponentControl = form.FindAll<BMComponentControl>().Single();
				cardControl = bmComponentControl.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == task.PK);
				AssertEquals("WHEN update and reload", "updated-2x", ((BusinessObject)cardControl.CardContent.Bindable)["TSK_P9_Description"]);
				AssertNull("THEN should not cause ThreadSentry Exception i.e. loading security checkpoint", ErrorReporter.LastExceptionReported);

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var loadedTask = newFactory.Load<ProcessTask>(task.PK);
				loadedTask.Delete();
				newFactory.Save();
				form.AwaitAll();

				taskCard = form.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == task.PK);
				taskCard.ContextMenuStrip.Items.Find("TaskDetailsToolStripMenuItem", false)[0].PerformClick();
				form.AwaitAll();

				AssertEquals("WHEN task is deleted and click task-detail THEN error message should be shown", "Task 'updated-2x', has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestShowDetailedCardWhenSectionViewModelPropertyCacheIsEmpty_BoardRefreshed_ExpectNoExceptions()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bbb");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(Factory, "Workflow", releaseGroupPK: group.PK);
			var task = BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 30, description: "Task");

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCard = form.FindSingle<TaskCardControl>();
				AssertNotNull(taskCard);

				form.RefreshStarted += (sender, args) =>
				{
					var componentControl = form.FindSingle<BMComponentControl>();
					componentControl.ViewModel.Cache.Clear();
					taskCard.ShowDetailedCard();
				};

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();
			}
		}

		#region Defer

		public void TestDefer_EndToEnd()
		{
			AssertDefer_EndToEnd();
		}

		public void TestDefer_WithDataRefreshDisabled_ShouldStillRemoveTicket()
		{
			var originalRegistryValue = BMSRegistry.Instance.UpdateTicketsWithDataRefresh.Value;

			try
			{
				if (originalRegistryValue)
				{
					BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				}

				AssertDefer_EndToEnd();
			}
			finally
			{
				if (originalRegistryValue)
				{
					BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				}
			}
		}

		void AssertDefer_EndToEnd()
		{
			var board = SetUpObjectForDeferTests(out var task1Pk);

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var task1Card = taskCards.First(t => t.CardContent.TaskIdentifier == task1Pk);

				task1Card.ContextMenuStrip.Items.Find("DeferToolStripMenuItem", false)[0].PerformClick();
				form.AwaitAll();

				var deferForm = ZFormModaliser.LastFormShownDialogForTest as DeferWorkflowForm;
				AssertNotNull(deferForm);

				taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(1, taskCards.Length);
			}
		}

		BMBoard SetUpObjectForDeferTests(out ZGuid task1Pk)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";
			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket2.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_FC_CurrentComponent = bucket2.PK;
			workflow1.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
			workflow1.FH_GG_ReleaseGroup = group.PK;

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.FH_GG_ReleaseGroup = group.PK;

			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Type = "UDF";
			task1.P9_Description = "task1";
			task1Pk = task1.PK;

			var task2 = workflow2.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;
			task2.P9_Type = "UDF";
			task2.P9_Description = "task2";

			Factory.Save();

			return section.Board;
		}

		public void TestDefer_WithDataRefreshEnabled_ShouldOnlyRefreshSectionOnce()
		{
			var board = SetUpObjectForDeferTests(out var task1Pk);

			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				var refreshCount = 0;
				form.GetSectionControls().Single().SectionViewModel.PerformRefreshActionCompleted_ForTest += (_, x_) => refreshCount++;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var task1Card = taskCards.First(t => t.CardContent.TaskIdentifier == task1Pk);

				task1Card.ContextMenuStrip.Items.Find("DeferToolStripMenuItem", false)[0].PerformClick();

				var deferForm = ZFormModaliser.LastFormShownDialogForTest as DeferWorkflowForm;
				AssertNotNull(deferForm);

				form.AwaitAll();

				AssertEquals(1, refreshCount);
			}
		}

		public void TestDefer_WhenNoSecurityForCurrentComponent()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var section = BMSTestHelper.CreateBoardSection(bucket2);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_FC_CurrentComponent = bucket2.PK;
			workflow1.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
			workflow1.FH_GG_ReleaseGroup = group.PK;

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.FH_GG_ReleaseGroup = group.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 60, taskType: "UDF", description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 60, taskType: "UDF", description: "task2");

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;
				Env.Security.BMSystemDeferWorkflow.IsAllowed = true;

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var task1Card = taskCards.First(t => t.CardContent.TaskIdentifier == task1.PK);

				task1Card.ContextMenuStrip.Items["DeferToolStripMenuItem"].PerformClick();

				var deferForm = ZFormModaliser.LastFormShownDialogForTest as DeferWorkflowForm;
				AssertNotNull(deferForm);

				form.AwaitAll();

				taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(1, taskCards.Length);
			}
		}

		public void TestDefer_WhenNoSecurityForDefer()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var section = BMSTestHelper.CreateBoardSection(bucket2);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_FC_CurrentComponent = bucket2.PK;
			workflow1.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
			workflow1.FH_GG_ReleaseGroup = group.PK;

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.FH_GG_ReleaseGroup = group.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, taskType: "UDF", description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow2, string.Empty, 60, taskType: "UDF", description: "task2");

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = true;
				Env.Security.BMSystemDeferWorkflow.IsAllowed = false;

				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var task1Card = taskCards.First(t => t.CardContent.TaskIdentifier == task1.PK);

				task1Card.ContextMenuStrip.Items.Find("DeferToolStripMenuItem", false)[0].PerformClick();

				form.AwaitAll();

				var deferForm = ZFormModaliser.LastFormShownDialogForTest as DeferWorkflowForm;
				AssertNull(deferForm);

				taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCards.Length);
				AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Buffer Management Systems -> Defer Workflows", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Task Details

		public void TestShowTaskDetails()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "bucket2";
			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket2.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.NotChanneled;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_FC_CurrentComponent = bucket2.PK;
			workflow1.DoNotStartBeforeDateLocal = ZDateTime.Now.AddDays(1);
			workflow1.FH_GG_ReleaseGroup = group.PK;

			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Type = "UDF";
			task1.P9_Description = "task1";

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var taskCard = form.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == task1.PK);
				AssertNotNull(taskCard);

				taskCard.ContextMenuStrip.Items.Find("TaskDetailsToolStripMenuItem", false)[0].PerformClick();
				form.AwaitAll();

				taskCard.taskDetailsForm.Dispose();
			}
		}

		#endregion

		#region TestChangeTaskDetailsWhileFormOpened

		public void TestChangeTaskDetailsWhileFormOpened()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var board = BMSTestHelper.CreateBoard(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", bucket, releaseGroupPK: group.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "task");

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var taskCard = form.FindAll<TaskCardControl>().FirstOrDefault(t => t.CardContent.TaskIdentifier == task.PK);
				AssertNotNull(taskCard);
				taskCard.ContextMenuStrip.Items.Find("TaskDetailsToolStripMenuItem", false)[0].PerformClick();
				form.AwaitAll();

				AssertNotNull(taskCard.taskDetailsForm);
				taskCard.OnTaskCardControlClicked();
				Application.DoEvents();

				var detailedCards = form.FindAll<TaskCardDetailControl>().ToArray();
				AssertEquals("detail card is shown", 1, detailedCards.Length);

				detailedCards[0].ProcessTask.P9_Description = "task_changed";

				detailedCards[0].Save_ForTest();

				taskCard.taskDetailsForm.Dispose();
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestCaseWithFactory.EnableBMSInRegistry();
			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;
		}

		#endregion
	}

	#endregion

	#region DragDrop

	class TaskCardDraggingTest : BMSGUITestCase
	{
		public void TestDragDropEnabledOnStackedPanel()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "AAAA", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource1.GS_Code);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = FindTaskCardControl(form, task);
				AssertEquals(true, taskCard.DragDropEnabled);
			}
		}

		public void TestDragDropEnabledOnStaggedPanel()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = config.System.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "AAAA", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource1.GS_Code);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = FindTaskCardControl(form, task);
				AssertEquals(true, taskCard.DragDropEnabled);
			}
		}

		public void TestDragDropEnabledOnStackedPanel_ReleaseScheduler()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, releaseGroup: config.ReleaseGroup, board: board);

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			config.ReleaseGroup.Staff.Add(resource1);
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, false);

			var job = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "AAAA", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource1.GS_Code);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = FindTaskCardControl(form, task);
				AssertEquals(false, taskCard.DragDropEnabled);
			}
		}

		public void TestDragDropIntoSameCell_ShouldLeaveInsideTaskPanel()
		{
			var system = CreateSystem("ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = buffer.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task.P9_FH_ProcessHeader = header.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(taskCard.Location.X + 10, taskCard.Location.Y + 10);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals("Card should remain in same TaskPanel", startingTaskPanel, taskCard.Parent);
				AssertEquals(resource1, task.AssignedStaffMember);
				Assert("Should be no changes on task", !task.HasChanges);
			}
		}

		public void TestDragDropIntoSameCell_TotalTasksButtonOnTop()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var section = CreateBoardSection(bucket);
			section.MS_FC_Component = bucket.PK;
			section.SectionConfiguration.CellsPerSubsection = 1;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var staff = CreateStaffInCurrentBranchDept("NON", "NonBon the Tron");

			var workflow1 = CreateWorkflow(CreateJobHeader<OrgHeader>(false), "Katherine", releaseGroupPK: group.PK);
			var task1 = CreateTask(workflow1, staff.GS_Code, 60, description: "LOTR");

			Factory.Save();

			using (var menu = TaskCardControl.CreateMenuStrip())
			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = form.FindAll<TaskCardControl>().Single();

				taskCard.OnDragDropStarting();
				taskCard.Location = new Point(taskCard.Location.X + 10, taskCard.Location.Y + 10);
				taskCard.OnDragDropFinished();

				var tasksButton = form.FindAll<Control>().Single(p => p.Name == "TotalTasksButton");
				AssertNotNull(tasksButton);
			}
		}

		public void TestDragDropIntoDifferentCellOfSameChannel_ShouldMoveBackToOriginalTaskPanel()
		{
			var system = CreateSystem("ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders[0];
			header.FH_FC_CurrentComponent = buffer.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task.P9_FH_ProcessHeader = header.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(0, -taskCard.Height);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals("Card should remain in same TaskPanel", startingTaskPanel, taskCard.Parent);
				AssertEquals(resource1, task.AssignedStaffMember);
				Assert("Should be no changes on task", !task.HasChanges);
			}
		}

		public void TestRefreshDuringDragDrop_ShouldNotCreateDuplicateTaskCard()
		{
			var system = CreateSystem("ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders[0];
			header.FH_FC_CurrentComponent = buffer.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task.P9_FH_ProcessHeader = header.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(0, -taskCard.Height * 3);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				//Here we mimic the refresh by having the same taskCard appearing in the starting location
				form.RefreshBoard();
				Application.DoEvents();
				taskCard.OnDragDropFinished();

				taskCard = FindTaskCardControl(form, task);
				var startingPanel = taskCard.Parent as TaskPanel;

				AssertEquals("Should be still one task", 1, startingPanel.TaskCards.Count(x => x.CardContent.TaskIdentifier == taskCard.CardContent.TaskIdentifier));
			}
		}

		public void TestDragDropIntoNextChannel_ShouldMoveToOtherTaskPanel_WhenStatusIsASNAndOPN()
		{
			var system = CreateSystem("ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = buffer.PK;

			var task = job.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task2 = job.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_FH_ProcessHeader = header.PK;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section.PK);

				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, control, control.ViewModel.ComponentGrid.CardCells.First(c => c.Channel.EntityPK == resource2.PK));

				Assert("Original card should be disposed", taskCard.IsDisposed);
				AssertCollectionNotContains("Origional card should no longer ve in the starting panel", taskCard, startingTaskPanel.Controls);

				taskCard = FindTaskCardControl(form, task);
				AssertNotNull("Task card should be found in a BMSGUITestCase.CreateTaskPanel", taskCard);
				Assert(taskCard.Parent is TaskPanel);

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("Task should now be assigned to new channel", resource2.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
				AssertEquals("Task should still have the status of Assigned", ProcessTaskStatusCodeList.Codes.Assigned, loadedTask.P9_Status);

				taskCard = FindTaskCardControl(form, task2);
				startingTaskPanel = (TaskPanel)taskCard.Parent;

				control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, control, control.ViewModel.ComponentGrid.CardCells.First(c => c.Channel.EntityPK == resource2.PK));

				Assert("Original card should be disposed", taskCard.IsDisposed);
				AssertCollectionNotContains("Origional card should no longer ve in the starting panel", taskCard, startingTaskPanel.Controls);

				taskCard = FindTaskCardControl(form, task2);
				AssertNotNull("Task card should be found in a BMSGUITestCase.CreateTaskPanel", taskCard);
				Assert(taskCard.Parent is TaskPanel);

				loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task2.PK);
				AssertEquals("Task should now be assigned to new channel", resource2.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
				AssertEquals("Task should still have the status of Assigned", ProcessTaskStatusCodeList.Codes.Assigned, loadedTask.P9_Status);
			}
		}

		public void TestDragDropIntoNextChannel_ShouldNotMoveToOtherTaskPanel_WhenStatusIsWRKorSUS()
		{
			var system = CreateSystem("ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = buffer.PK;

			var task = job.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var task2 = job.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			task2.P9_FH_ProcessHeader = header.PK;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section.PK);

				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, control, control.ViewModel.ComponentGrid.CardCells.First(c => c.Channel.EntityPK == resource2.PK));

				Assert("Original card should not be disposed", !taskCard.IsDisposed);
				AssertCollectionContains(taskCard, startingTaskPanel.Controls);

				taskCard = FindTaskCardControl(form, task2);
				startingTaskPanel = (TaskPanel)taskCard.Parent;

				control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, control, control.ViewModel.ComponentGrid.CardCells.First(c => c.Channel.EntityPK == resource2.PK));

				Assert("Original card should not be disposed", !taskCard.IsDisposed);
				AssertCollectionContains(taskCard, startingTaskPanel.Controls);
			}
		}

		public void TestDragDropIntoNextChannel_SingleTaskInWorkflow_WhenSelectedTaskStatusIsOPN()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, "Buffer", 8 * 12 * 60); // 12 working days
			var board = CreateBoard(system);
			var section = CreateBoardSection(buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = CreateStaffInCurrentBranchDept("ST1", "Staff1");
			var resource2 = CreateStaffInCurrentBranchDept("ST2", "Staff2");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var header = CreateWorkflow(jobHeader, "Workflow1", buffer);
			var task = CreateTask(header, resource1.GS_Code, 60, taskStatus: "OPN");

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(form.Width * 2 / 3, startingTaskPanel.Top);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals(2, control.dialogWrapper.ButtonStripActions.Length);
				AssertEquals("Selected Task", control.dialogWrapper.ButtonStripActions[0].Text);
				AssertEquals("Cancel", control.dialogWrapper.ButtonStripActions[1].Text);
			}
		}

		public void TestDragDropIntoNextChannel_SingleTaskInWorkflow_WhenSelectedTaskStatusIsASN()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, "Buffer", 8 * 12 * 60); // 12 working days
			var board = CreateBoard(system);
			var section = CreateBoardSection(buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = CreateStaffInCurrentBranchDept("ST1", "Staff1");
			var resource2 = CreateStaffInCurrentBranchDept("ST2", "Staff2");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var header = CreateWorkflow(jobHeader, "Workflow1", buffer);
			var task = CreateTask(header, resource1.GS_Code, 60);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(form.Width * 2 / 3, startingTaskPanel.Top);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals(2, control.dialogWrapper.ButtonStripActions.Length);
				AssertEquals("Selected Task", control.dialogWrapper.ButtonStripActions[0].Text);
				AssertEquals("Cancel", control.dialogWrapper.ButtonStripActions[1].Text);
			}
		}

		public void TestDragDropIntoNextChannel_MultipleTasksInWorkflow_SUSandASN()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, "Buffer", 8 * 12 * 60); // 12 working days
			var board = CreateBoard(system);
			var section = CreateBoardSection(buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = CreateStaffInCurrentBranchDept("ST1", "Staff1");
			var resource2 = CreateStaffInCurrentBranchDept("ST2", "Staff2");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var header = CreateWorkflow(jobHeader, "Workflow1", buffer);
			var task1 = CreateTask(header, resource1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			var task2 = CreateTask(header, string.Empty, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = FindTaskCardControl(form, task1);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(form.Width * 2 / 3, startingTaskPanel.Top);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals(2, control.dialogWrapper.ButtonStripActions.Length);
				AssertEquals("Entire Workflow", control.dialogWrapper.ButtonStripActions[0].Text);
				AssertEquals("Cancel", control.dialogWrapper.ButtonStripActions[1].Text);
			}
		}

		public void TestDragDropIntoNextChannel_MultipleTasksInWorkflow()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, "Buffer", 8 * 12 * 60); // 12 working days
			var board = CreateBoard(system);
			var section = CreateBoardSection(buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = CreateStaffInCurrentBranchDept("ST1", "Staff1");
			var resource2 = CreateStaffInCurrentBranchDept("ST2", "Staff2");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var header = CreateWorkflow(jobHeader, "Workflow1", buffer);
			var task1 = CreateTask(header, resource1.GS_Code, 60);
			var task2 = CreateTask(header, string.Empty, 60);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = FindTaskCardControl(form, task1);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(form.Width * 2 / 3, startingTaskPanel.Top);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals(3, control.dialogWrapper.ButtonStripActions.Length);
				AssertEquals("Selected Task", control.dialogWrapper.ButtonStripActions[0].Text);
				AssertEquals("Entire Workflow", control.dialogWrapper.ButtonStripActions[1].Text);
				AssertEquals("Cancel", control.dialogWrapper.ButtonStripActions[2].Text);
			}
		}

		public void TestDragDropIntoNextChannel_MultipleTasksInWorkflowAndCapability()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, "Buffer", 8 * 12 * 60); // 12 working days
			var board = CreateBoard(system);
			var section = CreateBoardSection(buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var capability = CreateCapability("COD", "Coding");

			var resource1 = CreateStaffInCurrentBranchDept("ST1", "Staff1", capability);
			var resource2 = CreateStaffInCurrentBranchDept("ST2", "Staff2", capability);

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var header = CreateWorkflow(jobHeader, "Workflow1", buffer);
			var task1 = CreateTask(header, resource1.GS_Code, 60, capability: capability);
			var task2 = CreateTask(header, string.Empty, 60, capability: capability);
			var task3 = CreateTask(header, string.Empty, 60, capability: capability);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = FindTaskCardControl(form, task1);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(form.Width * 2 / 3, startingTaskPanel.Top);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals(4, control.dialogWrapper.ButtonStripActions.Length);
				AssertEquals("Selected Task", control.dialogWrapper.ButtonStripActions[0].Text);
				AssertEquals("Tasks Assigned to Capability", control.dialogWrapper.ButtonStripActions[1].Text);
				AssertEquals("Entire Workflow", control.dialogWrapper.ButtonStripActions[2].Text);
				AssertEquals("Cancel", control.dialogWrapper.ButtonStripActions[3].Text);
			}
		}

		public void TestDragDropIntoNextChannel_MultipleTasksInWorkflowAndResource()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system, "Buffer", 8 * 12 * 60); // 12 working days
			var board = CreateBoard(system);
			var section = CreateBoardSection(buffer, board);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = CreateStaffInCurrentBranchDept("ST1", "Staff1");
			var resource2 = CreateStaffInCurrentBranchDept("ST2", "Staff2");

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var header = CreateWorkflow(jobHeader, "Workflow1", buffer);
			var task1 = CreateTask(header, resource1.GS_Code, 60);
			var task2 = CreateTask(header, resource1.GS_Code, 60);
			var task3 = CreateTask(header, resource1.GS_Code, 60);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var taskCard = FindTaskCardControl(form, task1);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				taskCard.OnDragDropStarting();
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.Location = new Point(form.Width * 2 / 3, startingTaskPanel.Top);
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragDropFinished();

				AssertEquals(4, control.dialogWrapper.ButtonStripActions.Length);
				AssertEquals("Selected Task", control.dialogWrapper.ButtonStripActions[0].Text);
				AssertEquals("Tasks Assigned to Resource", control.dialogWrapper.ButtonStripActions[1].Text);
				AssertEquals("Entire Workflow", control.dialogWrapper.ButtonStripActions[2].Text);
				AssertEquals("Cancel", control.dialogWrapper.ButtonStripActions[3].Text);
			}
		}

		public void TestDragDropIntoNextBucket_ShouldSetCurrentComponent()
		{
			var system = CreateSystem("ORG");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucket2.FC_Name = "bucket2";

			var board = system.Boards.AddNew();
			var section1 = board.Sections.AddNew();
			section1.MS_FC_Component = bucket1.PK;
			section1.SectionConfiguration.OverrideChannels = true;
			section1.SectionConfiguration.ChannelBy = ZString.Empty;
			var section2 = board.Sections.AddNew();
			section2.MS_FC_Component = bucket2.PK;
			section2.Column = 1;
			section2.SectionConfiguration.OverrideChannels = true;
			section2.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = bucket1.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			Factory.Save();

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(board);
			var viewModel1 = BMSTestHelper.CreateViewModel(section1, boardViewModel);
			var viewModel2 = BMSTestHelper.CreateViewModel(section2, boardViewModel);

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var section1Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				section2Control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, section2Control, section2Control.ViewModel.ComponentGrid.CardCells.First());

				Assert("Original card should be disposed", taskCard.IsDisposed);
				AssertEquals("Should be no controls left in the original panel", 0, startingTaskPanel.Controls.Count);

				taskCard = FindTaskCardControl(form, task);
				AssertNotNull("Task card should be found in a BMSGUITestCase.CreateTaskPanel", taskCard);
				Assert(taskCard.Parent is TaskPanel);

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("ProcessHeader should be in second bucket now", bucket2.PK, loadedTask.ProcessHeader.FH_FC_CurrentComponent);
			}
		}

		public void TestDragDropIntoNextBucket_ShouldAssignToChannel_ChannelExistsOnManySections()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var board = system.Boards.AddNew();
			var section1 = board.Sections.AddNew();
			section1.MS_FC_Component = bucket.PK;
			section1.SectionConfiguration.OverrideChannels = true;
			var section2 = board.Sections.AddNew();
			section2.MS_FC_Component = bucket.PK;
			section2.Column = 1;
			section2.SectionConfiguration.OverrideChannels = true;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource2.PK);
			var channel3 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource2.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = CreateWorkflow(jobHeader, "Beep", bucket);
			var task = CreateTask(workflow, resource1.GS_Code, 60);

			Factory.Save();

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(board);
			var dataSource = boardViewModel.Build(board);

			var viewModel1 = (BMBoardSectionViewModel)dataSource.SectionsAndViewModels.Single(p => p.ViewModel.SectionPK == section1.PK).ViewModel;
			var viewModel2 = (BMBoardSectionViewModel)dataSource.SectionsAndViewModels.Single(p => p.ViewModel.SectionPK == section2.PK).ViewModel;
			AssertEquals(2, viewModel1.BoardViewModel.GetSections().Count());
			AssertEquals(2, viewModel2.BoardViewModel.GetSections().Count());

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var section1Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				section2Control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, section2Control, section2Control.ViewModel.ComponentGrid.CardCells.First());

				Assert("Original card should be disposed", taskCard.IsDisposed);
				AssertEquals("Should be no controls left in the original panel", 0, startingTaskPanel.Controls.Count);

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.CardContent.TaskIdentifier == task.PK).ToArray();

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("ProcessHeader should still be in original component", bucket.PK, loadedTask.ProcessHeader.FH_FC_CurrentComponent);
				AssertEquals(resource2.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(2, taskCards.Length);

				AssertEquals(true, taskCards.Select(c => c.Cell.Channel.ChannelEntityCode).All(c => resource2.GS_Code == c));
			}
		}

		public void TestDragDropIntoNextBucket_SameChannelInNextComponent_ShouldSetCurrentComponentAndNotChangeChannel()
		{
			var system = CreateSystem("ORG");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucket2.FC_Name = "bucket2";

			var board = system.Boards.AddNew();
			var section1 = board.Sections.AddNew();
			section1.MS_FC_Component = bucket1.PK;
			section1.SectionConfiguration.OverrideChannels = true;
			section1.SectionConfiguration.ChannelBy = ZString.Empty;
			var section2 = board.Sections.AddNew();
			section2.MS_FC_Component = bucket2.PK;
			section2.Column = 1;
			section2.SectionConfiguration.OverrideChannels = true;
			section2.SectionConfiguration.ChannelBy = ZString.Empty;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource1.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = bucket1.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			Factory.Save();

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(board);
			var viewModel1 = BMSTestHelper.CreateViewModel(section1, boardViewModel);
			var viewModel2 = BMSTestHelper.CreateViewModel(section2, boardViewModel);

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var section1Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				section2Control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, section2Control, section2Control.ViewModel.ComponentGrid.CardCells.First());

				Assert("Original card should be disposed", taskCard.IsDisposed);
				AssertEquals("Should be no controls left in the original panel", 0, startingTaskPanel.Controls.Count);

				taskCard = FindTaskCardControl(form, task);
				AssertNotNull("Task card should be found in a BMSGUITestCase.CreateTaskPanel", taskCard);
				Assert(taskCard.Parent is TaskPanel);

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("ProcessHeader should be in second bucket now", bucket2.PK, loadedTask.ProcessHeader.FH_FC_CurrentComponent);
				AssertEquals(resource1.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
			}
		}

		public void TestDragDropIntoComponentWithNoCells_ShouldNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var board = BMSTestHelper.CreateBoard(system);

			var section1 = BMSTestHelper.CreateBoardSection(bucket1, board);
			section1.SectionConfiguration.OverrideChannels = true;
			section1.SectionConfiguration.ChannelBy = ZString.Empty;

			var section2 = BMSTestHelper.CreateBoardSection(bucket2, board);
			section2.Row = 1;
			section2.SectionConfiguration.CellsPerSubsection = 0;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource1.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = bucket1.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			Factory.Save();

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(board);
			var viewModel1 = BMSTestHelper.CreateViewModel(section1, boardViewModel);
			var viewModel2 = BMSTestHelper.CreateViewModel(section2, boardViewModel);

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var section1Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				section2Control.dialogWrapper.ResponseToFireForTest = CrossChannelTaskAssignments.SelectedTask;
				BMSGUITestCase.DragTaskTicketToCell(taskCard, section2Control);

				AssertEquals("Original card should not be disposed", false, taskCard.IsDisposed);

				taskCard = FindTaskCardControl(form, task);
				AssertNotNull("Task card should be found in a BMSGUITestCase.CreateTaskPanel", taskCard);
				Assert(taskCard.Parent is TaskPanel);

				var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
				AssertEquals("ProcessHeader should still be in first bucket", bucket1.PK, loadedTask.ProcessHeader.FH_FC_CurrentComponent);
				AssertEquals(resource1.GS_Code, loadedTask.P9_GS_NKAssignedStaffMember);
			}
		}

		public void TestDragDropIntoNextBucket_WhenNoSecurityToChangeComponent_ShouldNotSetCurrentComponent()
		{
			var system = CreateSystem("ORG");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucket1.FC_Name = "bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucket2.FC_Name = "bucket2";

			var board = system.Boards.AddNew();
			var section1 = board.Sections.AddNew();
			section1.MS_FC_Component = bucket1.PK;
			var section2 = board.Sections.AddNew();
			section2.MS_FC_Component = bucket2.PK;
			section2.Column = 1;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = bucket1.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(board);
			var viewModel1 = BMSTestHelper.CreateViewModel(section1, boardViewModel);
			var viewModel2 = BMSTestHelper.CreateViewModel(section2, boardViewModel);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = GetAndShowVisualBoardForm(board))
			{
				var section1Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				AssertEquals(false, Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed);

				var taskCard = FindTaskCardControl(form, task);
				var startingTaskPanel = (TaskPanel)taskCard.Parent;

				BMSGUITestCase.DragTaskTicketToCell(taskCard, section2Control, section2Control.ViewModel.ComponentGrid.CardCells.First());

				AssertEquals(false, taskCard.IsDisposed);
				AssertEquals("Card should remain in same TaskPanel", startingTaskPanel, taskCard.Parent);
				AssertEquals("ProcessHeader should still be in first bucket", bucket1, task.ProcessHeader.CurrentComponent);
				Assert("Should be no changes on task", !task.HasChanges);
			}
		}

		public void TestOnDragging_ShouldHideDetailedCardIfPresent()
		{
			var system = CreateSystem("ORG");
			var bucket = system.Components.AddNew();
			bucket.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucket.FC_Name = "Bucket";
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_FC_CurrentComponent = bucket.PK;
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCard = FindTaskCardControl(form, task);
				AssertNotNull("Should be a task card on the board", taskCard);

				taskCard.OnDragDropStarting();
				taskCard.ShowDetailedCard();

				var detailedCard = form.Controls.OfType<TaskCardDetailControl>().FirstOrDefault();
				AssertNotNull(detailedCard);
				Assert(!detailedCard.IsDisposed);

				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				Assert("Should not close detailed card yet as the mouse hasn't moved", !detailedCard.IsDisposed);

				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 1, 0, 0));
				Assert("Should close detailed card when dragging", detailedCard.IsDisposed);
				AssertEquals("Should remove detailed card from parent controls", 0, form.Controls.OfType<TaskCardDetailControl>().Count());
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DisableAsyncBehaviour();
		}

		#endregion
	}

	#endregion
}
