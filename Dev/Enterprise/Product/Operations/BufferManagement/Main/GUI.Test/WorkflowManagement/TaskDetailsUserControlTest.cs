using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.RichEdit;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TaskDetailsUserControlTest : BMSTestCaseWithFactory
	{
		#region Sequence Numbers

		public void TestGridRowOrderingIsUpdatedAccordingToSequenceNumbersAfterFactorySave()
		{
			var system = CreateSystem("H");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task1 = workflow.Parent.WorkflowItems.AddNew();
			var task2 = workflow.Parent.WorkflowItems.AddNew();

			workflow.TaskCollection.Add(task1);
			workflow.TaskCollection.Add(task2);

			task1.P9_Sequence = 2;
			task2.P9_Sequence = 3;

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var grid = control.TasksGrid;
				AssertEquals("Grid row ordering should be according to sequence numbers initially", task1.P9_Sequence, grid[0, 0]);

				grid[1, 0] = (ZInt)1;
				AssertEquals("Grid row ordering should not change according to sequence number changes before saving", task1.P9_Sequence, grid[0, 0]);

				Factory.Save();
				AssertEquals("Grid row ordering should be fixed according to sequence number changes after saving", task2.P9_Sequence, grid[0, 0]);
			}
		}

		#endregion

		#region DefaultColumns

		public void TestTaskDetailsDefaultColumns()
		{
			var system = CreateSystem("H");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task1 = workflow.Parent.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var grid = ((ZGrid)control.Controls.Find("TasksGrid", true).Single());
				var allColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				string[] expectedVisibleColumns = {
						"P9_Sequence",
						"P9_Description",
						"P9_Type",
						"P9_Status",
						"P9_GS_NKAssignedStaffMember",
						"P9_G4_RequiredCapability",
						"P9_EstDuration",
						"P9_EstimateVariationFactor",
						"P9_ScheduledDateForBinding",
						"StandardEstimatedDuration",
						"P9_ActualDuration",
						"P9_FH_ProcessHeader",
						"P9_GG_AssignedGroup",
						"Iteration" };

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);

				AssertContainsExactElementsInAnyOrder(expectedVisibleColumns, visibleColumns);
			}
		}

		#endregion

		public void TestImportCollectionInformation()
		{
			using (var grid = new TasksCustomGridForTest())
			{
				AssertEquals("BMSTaskGrid", grid.GetInfoProvider_ForTest().ContextKey);
			}
		}

		class TasksCustomGridForTest : TaskDetailsUserControl.TaskCustomZGrid
		{
			public IImportCollectionInfoProvider GetInfoProvider_ForTest() => GetImportCollectionInfoProvider();
		}

		public void TestNotesBindingStillWorks()
		{
			var system = CreateSystem("DUM");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task1 = workflow.Parent.WorkflowItems.AddNew();
			var task2 = workflow.Parent.WorkflowItems.AddNew();

			task1.P9_NotesAsString = "Dat Note";
			task2.P9_NotesAsString = "Dis Note";

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertNotNull(control.TasksGrid.ListManager);
				AssertEquals(2, control.TasksGrid.ListManager.Count);

				var rtfBox = (ZRichTextBox)control.Controls.Find("NotesRichTextBox", true)[0];
#if !WINZOR
				AssertEquals("Dat Note", ORtfTextUtil.RtfToText(rtfBox.RtfZBlob));

				control.TasksGrid.ListManager.Position = 1;
				Application.DoEvents();

				AssertEquals("Dis Note", ORtfTextUtil.RtfToText(rtfBox.RtfZBlob));
#else
				AssertEquals("<p>Dat Note</p>", rtfBox.HtmlZBlob.ToUTF8());

				control.TasksGrid.ListManager.Position = 1;
				Application.DoEvents();

				AssertEquals("<p>Dis Note</p>", rtfBox.HtmlZBlob.ToUTF8());
#endif
			}
		}

		public void TestWorkflowColumn_ShouldBeIncludedInExport()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MAIFRISTORG";

			var workflow = ProcessJobHeader.GetForParent(org, Factory).ProcessHeaders[0];
			workflow.FH_CompletionStatement = "IAmA Workflow - Ask me anything!";
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var columnForExport = new ExcelExportGuidColumn(ProcessTasksSchema.P9_FH_ProcessHeader);
			var exportedValue = columnForExport.GetValueForExport(task);

			AssertEquals("IAmA Workflow - Ask me anything!", exportedValue);
		}

		public void TestTaskManagementForm_SaveButton()
		{
			var staff = BMSTestHelper.GetOrCreateStaff(Factory, "S01", "Staff 01");
			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "workflowX", description: "taskX");
			var task = workflow.Tasks.FirstOrDefault();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var link = Factory.New<IProcessTaskIterationLink>();
			link.P9I_P9_ContainmentBarrierTask = task.PK;
			link.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			link.P9I_SystemCreateTimeUtc = ZDateTime.UtcNow;
			link.P9I_GS_NKResourceUnderReview = staff.GS_Code;

			Factory.Save();

			task.IterationLinksViewModel.Build();
			AssertEquals("Precondition: task1 has 1 IterationLinksViewModel", 1, task.IterationLinksViewModel.Count);

			var loadedTask = Factory.Load<ProcessTask>(task.PK);

			using (var form = ZControllerFactory.Create(ControllerIDs.ProcessTasks).ShowEditForm(loadedTask) as ZForm)
			{
				form.Show();
				Application.DoEvents();
				AssertType<TaskManagementForm>(form);

				AssertEquals("task's IterationLinksViewModel SHOULD not cause duplication in loadedTask1", 1, loadedTask.IterationLinksViewModel.Count);

				var saveButton = form.FindAll<ZPostingButtonsUserControl>()?.FirstOrDefault()?.SaveAndCloseButton;
				AssertEquals("WHEN showing TaskManagementForm, THEN SaveAndCloseButton should has Enabled=false because nothing has changed", false, saveButton.Enabled);
			}
		}

		public void TestGridRowDoubleClick_ShowSavingDialog()
		{
			var system = CreateSystem("DUM");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task1 = workflow.Parent.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				task1.HasChanges = true;
				control.TasksGrid.CurrentRowIndex = 0;
				control.TasksGrid[0, 1] = new ZString("Blah");
				Application.DoEvents();

				Rectangle row1Rectangle = control.TasksGrid.GetRowNotificationRectangle(-1);
				var mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, row1Rectangle.X, row1Rectangle.Y, 0);
				control.TasksGrid_MouseDoubleClick(control.TasksGrid, mouseEvent);
				Application.DoEvents();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				row1Rectangle = control.TasksGrid.GetRowNotificationRectangle(0);
				mouseEvent = new MouseEventArgs(MouseButtons.Left, 2, row1Rectangle.X, row1Rectangle.Y, 0);
				control.TasksGrid_MouseDoubleClick(control.TasksGrid, mouseEvent);
				Application.DoEvents();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("You must save this form first. Would you like to save now?"));

				var taskManagementForm = Application.OpenForms.OfType<TaskManagementForm>().First();
				taskManagementForm.Close();
			}
		}

		public void TestCloneMenuItem_FirstRightClickShouldWork()
		{
			// Test that clone works when user right-clicks on a row-header in the clone menu item's grid when the grid
			// isn't focused.
			var system = CreateSystem("DUM");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			workflow.Parent.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var menuItem = control.TasksGrid.ContextMenu.MenuItems.FindByText("Clone");
				AssertNotNull(menuItem);

				control.TasksGrid.ListManager.Position = 0;
				var firstTask = control.TasksGrid.GetCurrent();

				// Unfocus from the taskgrid.
				var statusBar = form.MainStatusBar.Focus();

				AssertNotNull(control.TasksGrid.ListManager);
				AssertEquals(1, control.TasksGrid.ListManager.Count);

				// Emulate right click and clone on row header.
				menuItem.PerformClick();

				AssertEquals(2, control.TasksGrid.ListManager.Count);
			}
		}

		public void TestCloneMenuItem_NoElements()
		{
			var system = CreateSystem("DUM");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var menuItem = control.TasksGrid.ContextMenu.MenuItems.FindByText("Clone");
				control.TasksGrid.ListManager.RemoveAt(0);
				AssertNotNull(menuItem);
				AssertEquals(0, control.TasksGrid.ListManager.Count);

				AssertNoExceptionThrown("Grid with no elements should throw no exception when cloned.", () => menuItem.PerformClick());

				AssertEquals(0, control.TasksGrid.ListManager.Count);
			}
		}

		public void TestNotesMenuItem_ShouldShowNotesInNewForm()
		{
			var system = CreateSystem("DUM");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task = workflow.Parent.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertNotNull(control.TasksGrid.ListManager);
				AssertEquals(1, control.TasksGrid.ListManager.Count);

				control.TasksGrid.ListManager.Position = 0;

				var menuItem = control.TasksGrid.ContextMenu.MenuItems.FindByText("Notes");
				AssertNotNull(menuItem);

				AssertEquals(false, task.HasChanges);
				AssertEquals(string.Empty, task.P9_NotesAsString);

				menuItem.PerformClick();

				AssertEquals(true, task.HasChanges);

				AssertMatch(new Regex(@"\\viewkind4.* Ooga booga boo(\\fs24)?\\par", RegexOptions.Singleline), task.P9_NotesAsString);
			}
		}

		public void TestNotesMenuItem_NotChangingNotesText_ShouldNotSetHasChanges()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task = workflow.Parent.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertNotNull(control.TasksGrid.ListManager);
				AssertEquals(1, control.TasksGrid.ListManager.Count);

				control.TasksGrid.ListManager.Position = 0;

				var menuItem = control.TasksGrid.ContextMenu.MenuItems.FindByText("Notes");
				AssertNotNull(menuItem);

				AssertEquals(false, task.HasChanges);
				AssertEquals(string.Empty, task.P9_NotesAsString);

				menuItem.PerformClick();

				AssertEquals(false, task.HasChanges);
				AssertEquals(string.Empty, task.P9_NotesAsString);
			}
		}

		[UseSnapshotProtection(true)]
		public void TestCloseTaskNotAssignedToSelf_PromptForPassword()
		{
			Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed = false;

			var categorisedTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypeCollection.AddNew();
			categorisedTaskType.Code = "ORG";

			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "INV";
			taskType.CanCloseTaskNotAssignedToSelf = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypeCollection);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "FRO";

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow");
			var task = CreateTask(workflow, staff.GS_Code, 30, "INV");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert(Env.CurrentUser.Initials != task.P9_GS_NKAssignedStaffMember);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
				AssertEquals(typeof(TaskOwnerPasswordRequestForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		[UseSnapshotProtection(true)]
		public void TestCloseTaskNotAssignedToSelf_NoPromptForPasswordDuringDBTransaction()
		{
			Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed = false;

			var categorisedTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypeCollection.AddNew();
			categorisedTaskType.Code = "ORG";

			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "INV";
			taskType.CanCloseTaskNotAssignedToSelf = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypeCollection);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "FRO";

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow");
			var task = CreateTask(workflow, staff.GS_Code, 30, "INV");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControl())
			using (Db.Connection.BeginTransactionWithManager())
			{
				form.Controls.Add(control);
				form.Show();

				Assert(Env.CurrentUser.Initials != task.P9_GS_NKAssignedStaffMember);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
				AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestCloseTaskAssignedToSelf_NoPromptForPassword()
		{
			Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed = false;

			var categorisedTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypeCollection.AddNew();
			categorisedTaskType.Code = "ORG";

			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "INV";
			taskType.CanCloseTaskNotAssignedToSelf = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypeCollection);

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow");
			var task = CreateTask(workflow, Env.CurrentUser.Initials, 30, "INV");
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert(Env.CurrentUser.Initials == task.P9_GS_NKAssignedStaffMember);

				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
				AssertEquals(null, ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestTaskStatus_ShouldAlwaysBeUpperCase()
		{
			var system = CreateSystem("DUM");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertNotNull("Precondition", control.TasksGrid.ListManager);
				AssertEquals("Precondition - empty row", 1, control.TasksGrid.ListManager.Count);

				var statusColumn = control.TasksGrid.Columns.Single(c => c.ColumnName == "P9_Status");
				var statusColumnStyle = (ZDropEditColumnStyle)statusColumn.ColumnStyle;
				AssertEquals(statusColumnStyle.CharacterCasing, CharacterCasing.Upper);
			}
		}

		public void TestTaskType_ShouldAlwaysBeUpperCase()
		{
			var system = CreateSystem("DUM");
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertNotNull("Precondition", control.TasksGrid.ListManager);
				AssertEquals("Precondition - empty row", 1, control.TasksGrid.ListManager.Count);

				var statusColumn = control.TasksGrid.Columns.Single(c => c.ColumnName == "P9_Type");
				var statusColumnStyle = (ZDropEditColumnStyle)statusColumn.ColumnStyle;
				AssertEquals(statusColumnStyle.CharacterCasing, CharacterCasing.Upper);
			}
		}

		#region TaskChangeMode

		public void TestChangeStatusViaWorkFlowTaskGrid_ShouldSetTaskChangeModeToWTG()
		{
			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();
			processTaskStatusChangeModeTracker.Clear();

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, processTaskStatusChangeModeTracker.Current);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Application.DoEvents();

				var taskGrid = form.FindSingle<TaskDetailsUserControl>().TasksGrid;
				var statusColumn = taskGrid.Columns.Single(c => c.ColumnName == ProcessTasksSchema.P9_Status.Name);
				var statusColumnStyle = statusColumn.ColumnStyle as ZCustomControlColumnStyle;

				taskGrid.BeginEdit(statusColumnStyle, 0);
				var zdropEdit = statusColumnStyle.EditControl as ZDropEdit;
				zdropEdit.SelectItem(ProcessTaskStatusCodeList.Codes.Working);

				AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid, processTaskStatusChangeModeTracker.Current);

				taskGrid.EndEdit(statusColumn.ColumnStyle, 0, false);

				Application.DoEvents();
				Factory.Save();
			}

			var lastLogReference = task.Logs.MostRecentLogByEventTime(Events.StatusChange).SL_Reference;

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, processTaskStatusChangeModeTracker.Current);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid}", lastLogReference);
		}

		public void TestChangeStatusViaWorkFlowTaskGrid_WhenChangeAffectOtherTask_ShouldSetTaskChangeModeToWTG_AndOtherTaskToOTT()
		{
			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();
			processTaskStatusChangeModeTracker.Clear();

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, processTaskStatusChangeModeTracker.Current);

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var otherTask = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 1);
			var taskToChange = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended, sequence: 2);

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Application.DoEvents();

				var taskGrid = form.FindSingle<TaskDetailsUserControl>().TasksGrid;
				taskGrid.ListManager.Position = 1;
				var statusColumn = taskGrid.Columns.Single(c => c.ColumnName == ProcessTasksSchema.P9_Status.Name);
				var statusColumnStyle = statusColumn.ColumnStyle as ZCustomControlColumnStyle;

				taskGrid.BeginEdit(statusColumnStyle, 1);
				var zdropEdit = statusColumnStyle.EditControl as ZDropEdit;
				zdropEdit.SelectItem(ProcessTaskStatusCodeList.Codes.Working);

				AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid, processTaskStatusChangeModeTracker.Current);

				taskGrid.EndEdit(statusColumn.ColumnStyle, 1, false);

				taskGrid.ListManager.Position = 0;

				Application.DoEvents();
				Factory.Save();
			}

			var otherTaskLastLogReference = otherTask.Logs.MostRecentLogByEventTime(Events.StatusChange).SL_Reference;
			var taskToChangeLastLogReference = taskToChange.Logs.MostRecentLogByEventTime(Events.StatusChange).SL_Reference;

			AssertEquals(ProcessTaskStatusChangeModeCodeList.Codes.Other, processTaskStatusChangeModeTracker.Current);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, otherTask.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskToChange.P9_Status);

			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid}", taskToChangeLastLogReference);
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.OtherTask}", otherTaskLastLogReference);
		}

		#endregion

		public void TestTaskCompletedTime_ShouldBeDisplayedAsLocalTime()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Dummy Time!");
			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.P9_CompletedTime = ZDateTimeOffset.Now;

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				AssertEquals("Pre-conditon - Task exists in grid", 1, control.TasksGrid.List.Count);
				AssertNotNull("Completed Time Local Column exists in Tasks Grid", control.TasksGrid.Columns.Select(c => c.ColumnName == "CompletedTimeLocal"));

				control.TasksGrid.SetColumnVisible(true, "CompletedTimeLocal");
				Application.DoEvents();

				var completedTimeGridIndex = control.TasksGrid.Columns.Where(c => c.IsVisible).IndexOf(c => c.ColumnName == "CompletedTimeLocal");
				AssertEquals("Correct CompletedTimeLocal value should be displayed in the appropriate column", task.CompletedTimeLocal.ToString(), control.TasksGrid[0, completedTimeGridIndex].ToString());
			}
		}

		public void TestProcessTaskIteration_WithMultipleTasksOnJob_OpenJobForm_DbHits()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");

			for (var i = 0; i < 10; i++)
			{
				BMSTestHelper.CreateTask(workflow);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var expectedHits = new Dictionary<string, int> { { ProcessTaskIterationLinkPivotSchema.Constants.TableName, 1 } };
			const string failureMessage = @"We should be able to show the Iteration column with one db hit for all tasks in the job, not one per task. SAD!

A special, one-off note:

At the time of writing this message, it's 7:34pm, 20 January, 2021. This means this is the final unit test that I will write with my common assertion endings.
Four years ago I made a commitment to end all of my meaningful assertion messages in this way as long as a certain orange thing was still relevant.
I have now fulfilled my 'Oath of Office'.
I'd like to thank all WiseTech developers past, present, and future, for your patience in reading my assertion messages for this little game that I've played.
I will truly miss being able to blow off steam by writing my assertion messages in this way. It is indeed, truly SSSSSAAAAAADDDDDDDDDDD!!!";

			using (AssertDbHitsForAllFactories(failureMessage, expectedHits, ignoreUnspecified: true, thresholdForUnspecified: 50))
			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(loadedWorkflow))
			{
				Application.DoEvents();

				var taskGrid = form.FindSingle<ZGrid>("TasksGrid");
				var iterationColumnIndex = taskGrid.Columns.IndexOf(c => c.ColumnName == "Iteration");

				for (var i = taskGrid.Columns.Count - 1; i >= 0; i--)
				{
					if (i != iterationColumnIndex)
					{
						taskGrid.SetColumnVisible(false, taskGrid.Columns[i].ColumnName);
					}
				}

				Application.DoEvents();
			}
		}

		public void TestChangeTaskSequence_WhenTaskIsNew_ShouldStillAssignIterationIfApplicable()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "INQ");
			BMSTestHelper.CreateSystem(Factory, "INQ");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CB");

			BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var containmentBarrierIterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			containmentBarrierIterationTask.P9_Sequence = 10;
			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow))
			{
				Application.DoEvents();

				var taskGrid = form.FindSingle<ZGrid>("TasksGrid");
				var newTask = (ProcessTask)taskGrid.List.AddNew();
				newTask.P9_Sequence = 7;

				AssertEquals("The new task was placed between two tasks with the same iteration, so that iteration should be selected, even when it's a new task added using the grid.", "1", newTask.Iteration);
			}
		}

		public void TestAddTaskToQualityIterationWorkflow_ShouldAssignIterationAndSaveWithoutError()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "INQ");
			BMSTestHelper.CreateSystem(Factory, "INQ");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CB");

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(iterationWorkflow))
			{
				Application.DoEvents();

				var taskGrid = form.FindSingle<ZGrid>("TasksGrid");
				taskGrid.ListManager.AddNew();
				var newTask = (ProcessTask)taskGrid.GetCurrent();
				newTask.P9_Description = "Please save properly.";
				AssertEquals(false, newTask.IsInDatabase);

				taskGrid.ListManager.EndCurrentEdit();
				Application.DoEvents();

				AssertEquals("The iteration should be automatically assigned because all tasks in the workflow have that iteration.", "1", newTask.Iteration);
				AssertNoExceptionThrown("Saving a task added in this way should not have db constraint errors because we should be waiting until P9_ParentID and P9_ParentTableCode are set before adding the iteration pivot.", newTask.Factory.Save);
			}
		}

		public void TestRelevantEstimateHours_ShouldBeZDurationConvertsColumnStyle()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Dummy Time!");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var columnStyle = control.TasksGrid.Columns.FirstOrDefault(c => c.ColumnName == "RelevantEstimateHours");
				Assert(columnStyle.ColumnStyle is ZDurationConvertsColumnStyle);
			}
		}

		public void TestSpellChecker()
		{
			const string CheckSpellingMenuKey = "checkSpelling";
			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task = workflow.Parent.WorkflowItems.AddNew();

			Factory.Save();

			using (var form = new ZForm(workflow))
			using (var control = new TaskDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var notesRichTextBox = (ZRichTextBox)control.Controls.Find("NotesRichTextBox", true)[0];
				var richTextBox = notesRichTextBox.GetRichTextBoxForTest();

				AssertNotNull("Spell checker should be initialized", richTextBox.ContextMenuStrip?.Items[CheckSpellingMenuKey]);
			}
		}

		class TaskDetailsUserControlForTest : TaskDetailsUserControl
		{
			protected override void OnShowingNotesForm(ZRichTextBoxPopupForm form)
			{
				base.OnShowingNotesForm(form);

#if !WINZOR
				form.Rtf = "Ooga booga boo";
#else
				form.Html = "Ooga booga boo";
#endif
			}
		}
	}
}
