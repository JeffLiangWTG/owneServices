using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormDataRefreshBusTest : BMSGUITestCase
	{
		#region Standard Data Refresh

		public void TestSaveDetailedTicket_ForTaskTicket_ShouldRefreshSummaryTicketWithoutExecutingSectionQueryTwice()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			FilterStripsTestHelper.AddCustomSQLFilterStrip(section.WorkflowFilter, "69=69");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Richard Morcroft", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "ABC");

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				detailedTicket.ProcessTask.P9_CardNote = "ABC News";

				using (Db.Connection.TrackExecutedCommands())
				{
					detailedTicket.Save_ForTest();

					var updatedTicket = form.FindSingle<TaskCardControl>();

					AssertEquals("ABC News", updatedTicket.CardContent.NoteText);

					var commands = Db.Connection.ExecutedCommands.Where(c => c.Contains("69=69")).ToArray();
					var message = "Should only execute section query once. Queries executed:" + System.Environment.NewLine + string.Join(System.Environment.NewLine, commands);

					AssertEquals(message, 1, commands.Length);
				}
			}
		}

		public void TestSaveDetailedTicket_ShouldNotRecreateControlsMultipleTimes()
		{
			AssertEquals("Precondition: this registry defaults to true", true, BMSRegistry.Instance.UpdateTicketsWithDataRefresh.Value);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			FilterStripsTestHelper.AddCustomSQLFilterStrip(section.WorkflowFilter, "'Your words'='Your words'");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Kim Jong-un", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "Smart Cookie");

			workflow.RunPreSaveValidation();

			AssertNoErrors("Validation errors prevent detailed tickets being saved.", workflow);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			using (KUserControl.TrackInstantiatedControls_ForTest())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Should be one for the actual ticket, and another for the 'template' ticket used for rendering optimisation", 2, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);
				AssertEquals("Should have executed section query once + once for validation of section upon initial load", 2, Db.Connection.ExecutedCommands.Count(c => c.Contains("Your words")));

				var ticket = form.FindSingle<TaskCardControl>();
				ticket.ShowDetailedCard();

				var detailedTicket = form.FindSingle<TaskCardDetailControl>();
				detailedTicket.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				detailedTicket.Save_ForTest(doEvents: false);

				AssertEquals("DataRefreshBus hasn't kicked in yet, so should not have re-drawn the ticket yet", 2, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);
				AssertEquals("Should not have executed the section query again yet", 2, Db.Connection.ExecutedCommands.Count(c => c.Contains("Your words")));

				Application.DoEvents();

				AssertEquals("Saving a detailed ticket should update the summary ticket via DataRefreshBus, rather than manually poking it and also having DataRefreshBus redraw it again. An extra TaskCardControl is again required for the 'template' ticket.", 4, KUserControl.InstantiatedControls_ForTest[typeof(TaskCardControl)]);
				AssertEquals("Should have executed the section query again", 3, Db.Connection.ExecutedCommands.Count(c => c.Contains("Your words")));
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestSaveDetailedTicket_WhenUpdatedExternallyToBoard_AndCellTaskListVisible_ShouldNotRecreateControlsMultipleTimes()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Twax", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "Do");

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks((OrgHeader)workflow.Parent);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskPanel = form.FindSingle<TaskPanel>(t => t.Controls.Count > 0);
				taskPanel.FindAndClickButton("TotalTasksButton");

				var cellTaskList = form.FindSingle<CellTasksControl>();
				var ticket = cellTaskList.FindSingle<TaskCardControl>();

				AssertEquals("", ticket.CardContent.NoteText);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

				ticket.ShowParentWorkflow();

				using (KUserControl.TrackInstantiatedControls_ForTest())
				using (var organisationForm = Application.OpenForms.OfType<ZOrganisationsForm>().Single())
				{
					var taskInOtherFactory = ((IWorkflowProvider)organisationForm.BusinessEntity).WorkflowItems.Tasks.Cast<ProcessTask>().Single();

					taskInOtherFactory.P9_CardNote = "Mmm";

					AssertSaved(organisationForm.FireSaveButton());

					Application.DoEvents();

					var newTicket = cellTaskList.FindSingle<TaskCardControl>();

					AssertNotEquals("Ticket should have been re-created by data refresh bus", ticket, newTicket);
					AssertEquals("Mmm", newTicket.CardContent.NoteText);
					AssertEquals(1, KUserControl.InstantiatedControls_ForTest[newTicket.GetType()]);
				}
			}
		}

		public void TestDataRefreshBus()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = CreateBoardSection(config.Buffer);

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Arnold", config.Buffer);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 25);

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var taskCardControl = form.FindAll<TaskCardControl>().Single();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, taskCardControl.CardContent.TaskOrderable.Status);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				Application.DoEvents();
				taskCardControl = form.FindAll<TaskCardControl>().Single();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskCardControl.CardContent.TaskOrderable.Status);
			}
		}

		public void TestDataRefreshBus_JobWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Test 1", config.Buffer);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 25);
			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var taskCardControl = form.FindAll<TaskCardControl>().Single();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, taskCardControl.CardContent.TaskOrderable.Status);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				Application.DoEvents();
				taskCardControl = form.FindAll<TaskCardControl>().Single();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskCardControl.CardContent.TaskOrderable.Status);
			}
		}

		public void TestDataRefreshBus_WhenTurnedOff()
		{
			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = CreateBoardSection(config.Buffer);

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Arnold", config.Buffer);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 25);

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var taskCardControl = form.FindAll<TaskCardControl>().Single();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, taskCardControl.CardContent.TaskOrderable.Status);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				Application.DoEvents();
				taskCardControl = form.FindAll<TaskCardControl>().Single();
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, taskCardControl.CardContent.TaskOrderable.Status);
			}
		}

		public void TestRefreshSection_ShouldDetectStartMonitoring_ShouldNotRecursivelyEndMonitoring()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "PLG", "Darth Plagueis the Wise", capability);
			var resource2 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "SID", "Darth Sidious", capability);

			config.ReleaseGroup.Staff.AddRange(resource1, resource2);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow, string.Empty, 60, description: "task1", capability: capability);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			var stats = new DummyPerformanceStatsCollector();

			Factory.Save();

			using (ObjectFactory.Substitute<IPerformanceStatisticsCollector>(stats))
			using (var form = GetAndShowVisualBoardForm(section))
			{
				PerformanceStatisticsCollector.ResetInstance();
				PlayTask(task1, resource1, form);
				form.RefreshNow_ForTest();
			}

			var relevantStats = stats.CollectedStats.Where(s => s.Name.Contains("BoardDataRefreshBusMonitor")).ToArray();
			AssertEquals(1, relevantStats.Length);
			Assert(relevantStats.All(s => s.WasCompleted));
		}

		#endregion

		#region Edge Case Handling

		[TestDate(2019, 1, 1)]
		public void TestSaveTaskOffTheBoard_WhenSectionDeletedInConfigForm_ShouldReloadBoard()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Bucket, board);

			section1.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section2.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section2.Column = 1;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Factoreéeéeé", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var section1Control = form.FindSingleOrDefault<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingleOrDefault<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				AssertNotNull(section1Control);
				AssertNotNull(section2Control);

				var section1TaskTicket = FindTaskCardControl(section1Control, task);
				var section2TaskTicket = FindTaskCardControl(section2Control, task);

				AssertNotNull(section1TaskTicket);
				AssertNotNull(section2TaskTicket);

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				using (var editForm = Application.OpenForms.OfType<BMBoardForm>().Single())
				{
					var editFormBoard = (BMBoard)editForm.BusinessEntity;

					task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
					Factory.Save();
					Application.DoEvents();

					CombineAssertions("Updating the task should update it on both sections via data refresh bus.", () =>
					{
						AssertEquals("IsDisposed flag for ticket on section 1", true, section1TaskTicket.IsDisposed);
						AssertEquals("IsDisposed flag for ticket on section 2", true, section2TaskTicket.IsDisposed);

						AssertEquals("IsDisposed flag for section 1 itself", false, section1Control.IsDisposed);
						AssertEquals("IsDisposed flag for section 2 itself", false, section2Control.IsDisposed);
					});

					section1TaskTicket = FindTaskCardControl(section1Control, task);
					section2TaskTicket = FindTaskCardControl(section2Control, task);

					AssertNotNull(section1TaskTicket);
					AssertNotNull(section2TaskTicket);
					AssertEquals(false, section1TaskTicket.IsDisposed);
					AssertEquals(false, section2TaskTicket.IsDisposed);

					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

					// Now delete one of the sections before updating the task again.
					editFormBoard.Sections.Single(s => s.PK == section2.PK).Delete();
					editFormBoard.RunPreSaveValidation();

					AssertNoErrors(editFormBoard);

					AssertSaved(editForm.FireSaveButton());

					CombineAssertions("Pre-condition: deleting a section but not yet closing the config form does not affect the board", () =>
					{
						AssertEquals("IsDisposed flag for ticket on section 1", false, section1TaskTicket.IsDisposed);
						AssertEquals("IsDisposed flag for ticket on section 2", false, section2TaskTicket.IsDisposed);
					});

					task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
					Factory.Save();
					Application.DoEvents();

					CombineAssertions("Updating the task should prompt the board to reload now that one of its sections has been deleted.", () =>
					{
						AssertEquals("IsDisposed flag for ticket on section 1", true, section1TaskTicket.IsDisposed);
						AssertEquals("IsDisposed flag for ticket on section 2", true, section2TaskTicket.IsDisposed);

						AssertEquals("IsDisposed flag for section 1 itself", true, section1Control.IsDisposed);
						AssertEquals("IsDisposed flag for section 2 itself", true, section2Control.IsDisposed);
					});

					AssertEquals("Only one section remains on the board", 1, form.FindAll<BMComponentControl>().Count());

					section1Control = form.FindSingleOrDefault<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
					AssertNotNull(section1Control);
					AssertEquals(false, section1Control.IsDisposed);

					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

					editForm.Close();
				}

				AssertEquals("The edit form has now been closed, but there's no need to reload the form again since we have already reloaded it to capture recent changes", false, section1Control.IsDisposed);
			}
		}

		#endregion

		#region Implementation

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}

	[TestDate(2015, 7, 14)]
	class VisualBoardFormDataRefreshBusWithFiltersTest : BMSGUITestCase
	{
		#region Refresh Considers Filters

		public void TestTaskUpdatedByDataRefreshBus_ShouldRespectSectionWorkflowFilters()
		{
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var tickets = sectionControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);
			}

			FilterStripsTestHelper.AddStartsWithFilter(section1.SectionConfiguration.WorkflowFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Adam");
			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);

				StartTaskTwoAndAssertJustTaskOnePresent(sectionControl);
			}
		}

		public void TestTaskUpdatedByDataRefreshBus_ShouldRespectSectionTaskFilters()
		{
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var tickets = sectionControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);
			}

			FilterStripsTestHelper.AddStartsWithFilter(section1.SectionConfiguration.TaskFilter, "Description", "Adam");
			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);

				StartTaskTwoAndAssertJustTaskOnePresent(sectionControl);
			}
		}

		public void TestTaskUpdatedByDataRefreshBus_ShouldRespectTemporarySectionWorkflowFilters()
		{
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var tickets = sectionControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);

				SetRuntimeFilterStrips(sectionControl, true, new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Adam",
				});

				Application.DoEvents();

				StartTaskTwoAndAssertJustTaskOnePresent(sectionControl);
			}
		}

		public void TestTaskUpdatedByDataRefreshBus_ShouldRespectTemporarySectionTaskFilters()
		{
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var tickets = sectionControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);

				SetRuntimeFilterStrips(sectionControl, true, new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Description",
					FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Adam",
				});

				Application.DoEvents();

				StartTaskTwoAndAssertJustTaskOnePresent(sectionControl);
			}
		}

		public void TestTaskUpdatedByDataRefreshBus_ShouldRespectReleaseGateLinkFilters()
		{
			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var tickets = sectionControl.FindAll<TaskCardControl>().ToArray();

				AssertEquals(2, tickets.Length);
			}

			FilterStripsTestHelper.AddStartsWithFilter(config.ComponentLink.FilterRule, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Adam");
			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);

				StartTaskTwoAndAssertJustTaskOnePresent(sectionControl);
			}
		}

		#endregion

		#region Implementation

		protected override bool ShouldDisableAsyncBehaviour => true;

		ConstrainedSchematicTestConfig config;
		ProcessTask task1, task2;
		BMBoard board;
		BMBoardSection section1, section2;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Adam");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Durkee");

			task1 = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, description: "Adam");
			task2 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, description: "Durkee");

			board = BMSTestHelper.CreateBoard(config.System);
			section1 = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);
			section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			section2.Row = 1;

			Factory.Save();
		}

		#endregion

		#region Assertions

		void StartTaskTwoAndAssertJustTaskOnePresent(BMComponentControl control)
		{
			var tickets = control.FindAll<TaskCardControl>().ToArray();

			AssertEquals(1, tickets.Length);
			AssertEquals(task1.PK, tickets[0].CardContent.TaskIdentifier);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			Application.DoEvents();

			tickets = control.FindAll<TaskCardControl>().ToArray();

			AssertEquals(1, tickets.Length);
			AssertEquals(task1.PK, tickets[0].CardContent.TaskIdentifier);
		}

		#endregion
	}

	#region Non-Transactioned Test

	class VisualBoardFormDataRefreshBusTest_BackgroundBoard : NonTransactionedTestCase
	{
		#region Saving Detailed Ticket

		[TestDate(2018, 1, 1, 9, 0, 0)]
		public void TestSaveDetailedTicket_WhenUpdatedExternallyToBoard_AndCellTaskListVisible_ShouldNotRecreateControlsMultipleTimes()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow X", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "task X");

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks((OrgHeader)workflow.Parent);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskPanel = form.FindAll<TaskPanel>().Single(t => t.Controls.Count > 0);
				taskPanel.Invoke(new Action(() => taskPanel.FindAndClickButton("TotalTasksButton")));

				var cellTaskList = form.FindAll<CellTasksControl>().Single();
				var ticket = cellTaskList.FindSingle<TaskCardControl>();

				AssertNullOrEmpty("Precondition: note", ticket.CardContent.NoteText);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

				ticket.Invoke(new Action(() => ticket.ShowParentWorkflow()));

				Application.DoEvents();

				using (KUserControl.TrackInstantiatedControls_ForTest())
				using (var organisationForm = Application.OpenForms.OfType<ZOrganisationsForm>().Single())
				{
					var taskInOtherFactory = ((IWorkflowProvider)organisationForm.BusinessEntity).WorkflowItems.Tasks.Cast<ProcessTask>().Single();

					taskInOtherFactory.P9_CardNote = "Updating ...";

					BMSGUITestCase.AssertSaved(organisationForm.FireSaveButton());

					form.AwaitAll();

					var newTicket = cellTaskList.FindSingle<TaskCardControl>();

					CombineAssertions("Ticket should have been re-created by data refresh bus", () =>
					{
						AssertNotEquals("Ticket", ticket, newTicket);
						AssertEquals("Note", "Updating ...", newTicket.CardContent.NoteText);
						AssertEquals("InstantiatedControls", 1, KUserControl.InstantiatedControls_ForTest[newTicket.GetType()]);
					});
				}
			}
		}

		#endregion

#region Saving Outside the Board

#if !WINZOR  // Bitmap rendering is not used for Winzor, so the test does not apply.
		public void TestDataRefreshUpdatingTickets_PlayTaskOutsideBoard_ShouldUpdateTaskBitmap()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				var bitmap = taskCard.BackgroundImage;

				form.RefreshBoardAndWait();

				taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, taskCard.CardContent.TaskOrderable.Status);
				AssertEquals("Nothing has changed, so should use the same background image", bitmap, taskCard.BackgroundImage);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				form.AwaitAll();

				taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				AssertNotEquals("Task has changed, so should regenerate background image", bitmap, taskCard.BackgroundImage);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskCard.CardContent.TaskOrderable.Status);
			}
		}
#endif

		public void TestDataRefreshUpdatingTickets_ReassignTaskOutsideBoard_ShouldMoveChannels()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ALX", "Alex The Phallex");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "The Dan");
			config.ReleaseGroup.Staff.AddRange(resource1, resource2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.CellsPerSubsection = 13;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				var cell = taskCard.Cell;
				AssertEquals(cell.Column, 2);

				form.RefreshBoardAndWait();

				taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				AssertEquals("Nothing has changed, so should be in the same column", 2, cell.Column);

				task.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
				Factory.Save();

				form.AwaitAll();

				taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				AssertEquals("Task has changed, so should move to next column", 3, taskCard.Cell.Column);
			}
		}

		public void TestDataRefreshUpdatingTickets_ReassignCapabilityTaskToResource_ShouldRemoveFromOtherChannel()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ALX", "Alex The Phallex");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "The Dan");
			config.ReleaseGroup.Staff.AddRange(resource1, resource2);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Saying Boo-urns";
			capability.ResourcesWithCapability.AddRange(resource1, resource2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 60, capability: capability);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.CellsPerSubsection = 13;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCards = BMSGUITestCase.FindTaskCardControls(form, task);
				AssertEquals(2, taskCards.Length);
				AssertCollectionContains(taskCards, t => t.Cell.Column == 2);
				AssertCollectionContains(taskCards, t => t.Cell.Column == 3);

				form.RefreshBoardAndWait();

				taskCards = BMSGUITestCase.FindTaskCardControls(form, task);
				AssertEquals("Nothing has changed, so should be in both cells still", 2, taskCards.Length);
				AssertCollectionContains(taskCards, t => t.Cell.Column == 2);
				AssertCollectionContains(taskCards, t => t.Cell.Column == 3);

				task.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
				Factory.Save();

				form.RefreshBoardAndWait();

				taskCards = BMSGUITestCase.FindTaskCardControls(form, task);
				AssertEquals("Task has been assigned to a resource, so should remove from other resource channel", 1, taskCards.Length);
				AssertEquals(3, taskCards[0].Cell.Column);
			}
		}

#endregion

		#region Registry

		public void TestDataRefreshUpdatingTickets_WhenSwitchedOffInRegistry_PlayTaskOutsideBoard_ShouldNotUpdateTaskBitmap()
		{
			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new MockAsyncStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				var bitmap = taskCard.BackgroundImage;

				form.RefreshBoardAndWait();

				taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				AssertEquals("Nothing has changed, so should use the same background image", bitmap, taskCard.BackgroundImage);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				form.AwaitAll();

				taskCard = BMSGUITestCase.FindTaskCardControl(form, task);
				AssertEquals("Task has changed, but registry option is off, so should not regenerate background image", bitmap, taskCard.BackgroundImage);
			}
		}

		#endregion

		#region Channel Headings

		[TestDate(2016, 10, 10)]
		public void TestHeadingStatusShouldUpdateWhenChannelStatusIsChanged()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Complete!");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(section.Board);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				var header = form.FindAll<ChannelHeaderControl>().Single();

				AssertEquals("Zone 3, Working", header.ChannelStatus);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				Factory.Save();
				Application.DoEvents();

				AssertEquals("Zone 3, Idle", header.ChannelStatus);

				BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();

				AssertEquals("Zone 3, Idle", header.ChannelStatus);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestCloseTaskOutsideTheBoard_WhenNoTicketsRemain_ShouldUpdateChannel()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "OTM", "Otm Shank");
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource.PK);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Strangelove", config.Buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(config.BufferSection.Board))
			{
				form.AwaitAll();
				void AssertTicketShown(bool shouldTicketBeShown)
				{
					var ticket = form.FindAll<TaskCardControl>().SingleOrDefault();

					if (shouldTicketBeShown)
					{
						AssertNotNull(ticket);
					}
					else
					{
						AssertNull(ticket);
					}
				}

				void AssertChannelHeadingStatus(string expectedStatusText)
				{
					AssertEquals(expectedStatusText, form.FindSingle<ChannelHeaderControl>().ChannelStatus);
				}

				AssertTicketShown(true);
				AssertChannelHeadingStatus("Idle");

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();
				form.AwaitAll();

				AssertTicketShown(true);
				AssertChannelHeadingStatus("Working");

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
				form.AwaitAll();

				AssertTicketShown(false);
				AssertChannelHeadingStatus("Idle");
			}
		}

		#endregion

		#region Edge-Case Handling

		[TestDate(2019, 1, 1)]
		public void TestSaveTaskOffTheBoard_WhenSectionDeletedInAnotherSession_ShouldReloadBoard()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Bucket, board);

			section1.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section2.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section2.Column = 1;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Factoreéeéeé", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var section1Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section2.PK);

				var section1TaskTicket = BMSGUITestCase.FindTaskCardControl(section1Control, task);
				var section2TaskTicket = BMSGUITestCase.FindTaskCardControl(section2Control, task);

				AssertNotNull(section1TaskTicket);
				AssertNotNull(section2TaskTicket);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				Factory.Save();
				form.AwaitAll();

				CombineAssertions("Updating the task should update it on both sections via data refresh bus.", () =>
				{
					AssertEquals("IsDisposed flag for ticket on section 1", true, section1TaskTicket.IsDisposed);
					AssertEquals("IsDisposed flag for ticket on section 2", true, section2TaskTicket.IsDisposed);

					AssertEquals("IsDisposed flag for section 1 itself", false, section1Control.IsDisposed);
					AssertEquals("IsDisposed flag for section 2 itself", false, section2Control.IsDisposed);
				});

				section1TaskTicket = BMSGUITestCase.FindTaskCardControl(section1Control, task);
				section2TaskTicket = BMSGUITestCase.FindTaskCardControl(section2Control, task);

				AssertNotNull(section1TaskTicket);
				AssertNotNull(section2TaskTicket);
				AssertEquals(false, section1TaskTicket.IsDisposed);
				AssertEquals(false, section2TaskTicket.IsDisposed);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

				// Now delete one of the sections in another session before updating the task again.
				var newFactory = Factory.CreateNewFactory();
				newFactory.RefreshEnabled = false;
				var loadedSection = newFactory.Load<BMBoardSection>(section2.PK);
				loadedSection.Delete();
				newFactory.Save();

				CombineAssertions("Pre-condition: deleting a section in another factory does not affect the board", () =>
				{
					AssertEquals("IsDisposed flag for ticket on section 1", false, section1TaskTicket.IsDisposed);
					AssertEquals("IsDisposed flag for ticket on section 2", false, section2TaskTicket.IsDisposed);
				});

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				Factory.Save();
				form.AwaitAll();

				CombineAssertions("Updating the task should prompt the board to reload now that one of its sections has been deleted.", () =>
				{
					AssertEquals("IsDisposed flag for ticket on section 1", true, section1TaskTicket.IsDisposed);
					AssertEquals("IsDisposed flag for ticket on section 2", true, section2TaskTicket.IsDisposed);

					AssertEquals("IsDisposed flag for section 1 itself", true, section1Control.IsDisposed);
					AssertEquals("IsDisposed flag for section 2 itself", true, section2Control.IsDisposed);
				});

				AssertEquals("Only one section remains on the board", 1, form.FindAll<BMComponentControl>().Count());

				AssertNoExceptionThrown(() => form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK));
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			BMSTestCaseWithFactory.SetupAndClearTables();

			base.SetUp();
		}

		#endregion
	}

#endregion
}
