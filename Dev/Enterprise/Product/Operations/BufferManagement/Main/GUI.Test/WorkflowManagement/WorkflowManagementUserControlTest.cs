using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ProcessJobHeaderProvider = Enterprise.MasterFiles.Business.ProcessJobHeaderProvider;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowManagementUserControlTest : BMSTestCaseWithFactory
	{
		[TestDateIncremental(seconds: 1)]
		public void TestConcurrency_WhenModifyingTaskStatusInOtherFactoryAndSaveForm_ThenHasChangesShouldBeFalse()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE1", "Resource 1");
			var preWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, completionStatement: "pre workflow", staffCode: resource1.GS_Code, description: "pre task");
			var postWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, completionStatement: "post workflow", staffCode: resource1.GS_Code, description: "post task");
			var postTask = postWorkflow.Tasks.First(t => t.P9_Description == "post task");
			var preTask = preWorkflow.Tasks.First(t => t.P9_Description == "pre task");

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks((OrgHeader)jobHeader.Parent);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(preTask))
			{
				Application.DoEvents();

				var control = form.FindSingle<WorkflowManagementUserControl>();

				var tasksGrid = control.TasksGrid_ForTest;
				var taskStatusIndex = tasksGrid.Columns.IndexOf(column => column.ColumnName == ProcessTasksSchema.P9_Status.Name);
				var workflowsGrid = control.WorkflowsGrid;

				// update for saving and cache StmALog in RowFactory.QueryCache
				tasksGrid[0, taskStatusIndex] = new ZString("CAN");
				Application.DoEvents();

				var currentWorkflow = ((ProcessHeader)workflowsGrid.ListManager.GetCurrent());
				AssertEquals("workflow description", "pre workflow", currentWorkflow.FH_CompletionStatement);
				AssertEquals("workflow status", WorkflowStatusList.Codes.Closed, currentWorkflow.FH_Status);

				SimulateClosingTaskInOtherFactory(postWorkflow);

				AssertSaved(form.FireSaveButton());

				var bizO = (IBusiness)form.DataSource;
				AssertEquals("bizO.HasChanges should be false after saving", false, bizO.HasChanges);
			}
		}

		void SimulateClosingTaskInOtherFactory(ProcessHeader workflow)
		{
			var sql = $@"
INSERT INTO [StmALog]
           ([SL_PK]
           ,[SL_Table]
           ,[SL_Parent]
           ,[SL_IsEstimate]
           ,[SL_IsCancelled]
           ,[SL_Reference]
           ,[SL_PostedTimeUtc]
           ,[SL_EventTime]
           ,[SL_GS_NKUser]
           ,[SL_SE_NKEvent]
           ,[SL_GB_NKBranch]
           ,[SL_GE_NKDepartment]
           ,[SL_FireWorkflow])
     VALUES
           (NEWID() -- <SL_PK, uniqueidentifier,>
           ,'ProcessHeader' -- <SL_Table, varchar(35),>
           ,'{workflow.PK}' -- <SL_Parent, uniqueidentifier,>
           ,'N' -- <SL_IsEstimate, char(1),>
           ,'N' -- <SL_IsCancelled, char(1),>
           ,'Task status modification in other factory' -- <SL_Reference, varchar(1024),>
           ,'{ZDateTime.UtcNow.SqlFormat}' -- <SL_PostedTimeUtc, datetime,>
           ,'{ZDateTime.Now.SqlFormat}' -- <SL_EventTime, datetime,>
           ,'E' -- <SL_GS_NKUser, char(3),>
           ,'JCL' -- <SL_SE_NKEvent, char(3),>
           ,'BNE' -- <SL_GB_NKBranch, varchar(3),>
           ,'BRN' -- <SL_GE_NKDepartment, varchar(3),>
           ,'0') -- <SL_FireWorkflow, bit,>)";

			Db.Connection.ExecuteNonQuery(sql);
		}

		#region Workflow Notes

		public void TestWorkflowNotes()
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			workflow2.WorkflowNote.ST_NoteDataAsText = "Blah";

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Factory.Save();
				AssertEquals(false, jobHeader.HasChanges);

				var notesTabPage = control.FindAll<WorkflowNotesTabPage>().Single();
				AssertEquals("Job Notes", notesTabPage.Text);
				AssertEquals("Empty note on jobHeader, so should not be an image", -1, notesTabPage.ImageIndex);

				control.WorkflowsGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals("Workflow Notes", notesTabPage.Text);
				AssertEquals("Empty note on workflow1, so should not be an image", -1, notesTabPage.ImageIndex);

				control.WorkflowsGrid.ListManager.Position = 2;
				Application.DoEvents();
				AssertEquals("Workflow Notes", notesTabPage.Text);
				AssertEquals("There is a note on workflow2, so should be an image", NotesImageIndex, notesTabPage.ImageIndex);

				AssertEquals(false, jobHeader.HasChanges);

				workflow1.WorkflowNote.ST_NoteDataAsText = "Booo";
				AssertEquals(false, jobHeader.HasChanges);

				control.WorkflowsGrid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals("Workflow Notes", notesTabPage.Text);
				AssertEquals("There is now a note on workflow2, so should be an image", NotesImageIndex, notesTabPage.ImageIndex);

				AssertNoErrors(jobHeader);
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		static int NotesImageIndex
		{
			get { return Icons.GetImageIndex(IconTypes.StmNote); }
		}

		public void TestCreateWorkflowAndSave_ThenDeleteAndSaveAgain_ShouldNotDevelopMergeConflictWithWorkflowNote()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "DUM");

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, taskType: "QCB");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Factory.Save();
				AssertEquals(false, jobHeader.HasChanges);

				Application.DoEvents();
				var grid = control.WorkflowsGrid;

				grid.ListManager.AddNew();
				Application.DoEvents();

				grid[2, 2] = new ZString("Xoop");
				grid.ListManager.Position = 1; // Switch to the second row, causing the list to resequence

				Factory.Save();

				AssertEquals(2, grid.ListManager.List.Cast<ProcessHeader>().IndexOf(w => w.FH_CompletionStatement == "Xoop"));
				grid.ListManager.RemoveAt(2);
				AssertNoExceptionThrown("We should be able to save without any concurrency errors. If you're getting concurrency errors on WorkflowNote, check if you're trying to delete one that was never actually saved to begin with.", () => Factory.Save());
			}
		}

		public void TestCreateNewWorkflowInGrid_EnsureCorrectTaskSequenceEnumeration()
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>();
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, taskType: "UDF");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1200, 800, true) })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Factory.Save();
				AssertEquals(false, jobHeader.HasChanges);

				Application.DoEvents();

				var workflowGrid = control.WorkflowsGrid;
				workflowGrid.Focus();

				var workDescriptionColumnIndex = workflowGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);
				workflowGrid.ListManager.AddNew();
				workflowGrid[4, workDescriptionColumnIndex] = new ZString("new workflow");

				var taskGrid = control.TasksGrid_ForTest;
				var taskDescriptionColumnIndex = taskGrid.Columns.IndexOf(x => x.ColumnName == ProcessTasksSchema.P9_Description.Name);
				taskGrid.ListManager.AddNew();
				taskGrid[0, taskDescriptionColumnIndex] = new ZString("new task");

				AssertEquals("We should still have correctly have the 'current workflow' be the new workflow, which we renamed", "new workflow", ((ProcessHeader)workflowGrid.ListManager.Current).FH_CompletionStatement);
				AssertEquals("A new task in the new workflow should be created", 1, taskGrid.ListManager.Count);
				AssertEquals("The sequence numbering should start anew and not continue from a previous workflow", (ZInt)1, taskGrid[0, 0]);
			}
		}

		public void TestCreateNewWorkflowInGrid_EnsureCorrectWorkflowSelected()
		{
			var jobHeader = CreateJobHeader<DummyWithWorkflow>();
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, taskType: "UDF");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1200, 800, true) })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Factory.Save();
				AssertEquals(false, jobHeader.HasChanges);

				Application.DoEvents();

				var workGrid = control.WorkflowsGrid;
				workGrid.Focus();

				var workDescriptionColumnIndex = workGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);
				workGrid.ListManager.AddNew();
				workGrid[4, workDescriptionColumnIndex] = new ZString("Flo Worqa!");

				var taskGrid = control.TasksGrid_ForTest;
				var taskDescriptionColumnIndex = taskGrid.Columns.IndexOf(x => x.ColumnName == ProcessTasksSchema.P9_Description.Name);

				taskGrid.ListManager.List.Add(Factory.New<ProcessTask>());
				taskGrid[0, 0] = new ZInt(1);
				taskGrid[0, taskDescriptionColumnIndex] = new ZString("Des");

				taskGrid.ListManager.List.Add(Factory.New<ProcessTask>());
				taskGrid.ListManager.Position = 1;
				taskGrid[1, 0] = new ZInt(2);
				taskGrid[1, taskDescriptionColumnIndex] = new ZString("Crip");

				taskGrid.ListManager.List.Add(Factory.New<ProcessTask>());
				taskGrid.ListManager.Position = 2;
				taskGrid[2, 0] = new ZInt(3);
				taskGrid[2, taskDescriptionColumnIndex] = new ZString("Tion");
				Application.DoEvents();

				workGrid.Focus();
				workGrid.ListManager.Position = 3;
				Application.DoEvents();

				AssertEquals("workflow2", workGrid[3, workDescriptionColumnIndex]);
				AssertNotEquals("Task list changes when different workflow is selected", "Des", taskGrid[0, taskDescriptionColumnIndex]);
				AssertNotEquals("Task list changes when different workflow is selected", "Crip", taskGrid[1, taskDescriptionColumnIndex]);

				workGrid.ListManager.Position = 4;
				Application.DoEvents();

				AssertEquals("Flo Worqa!", workGrid[4, workDescriptionColumnIndex]);
				AssertEquals("Des", taskGrid[0, taskDescriptionColumnIndex]);
				AssertEquals("Crip", taskGrid[1, taskDescriptionColumnIndex]);
				AssertEquals("Tion", taskGrid[2, taskDescriptionColumnIndex]);
			}
		}

		public void TestCreateQualityIterationWorkflowAndSave_ThenDeleteAndSaveAgain_ShouldNotDevelopMergeConflictWithWorkflowNote()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "DUM");

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, taskType: "QCB");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Factory.Save();
				AssertEquals(false, jobHeader.HasChanges);

				var qualityIterationWorkflow = ((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(workflow1);

				var link1 = (IProcessTaskIterationLink)task2.IterationLinks.AddNew();
				link1.P9I_FH_IterationWorkflow = qualityIterationWorkflow.PK;
				link1.P9I_P9_IterationTask = task1.PK;

				Application.DoEvents();
				var grid = control.WorkflowsGrid;

				Factory.Save();

				AssertEquals(2, grid.ListManager.List.Cast<ProcessHeader>().IndexOf(w => w.FH_CompletionStatement == "workflow1 (Quality Iteration 1)"));
				grid.ListManager.RemoveAt(2);
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		#endregion

		#region Performance Regression

		void TestDeleteWorkflow_DbHits(Dictionary<string, int> expectedHitCounts)
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(loadedJobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.WorkflowsGrid.ListManager.Position = 1;
				Application.DoEvents();

				AssertEquals(3, loadedJobHeader.ProcessHeaders.Count);

				newFactory.ResetDatabaseLoadCount();

				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(2, loadedJobHeader.ProcessHeaders.Count);
				AssertDbHits(expectedHitCounts, newFactory);
			}
		}

		public void TestDeleteWorkflow_DbHits_PlanningManagementMode()
		{
			AssertEquals(WorkflowManagementModes.Codes.PlanningManagement, BMSRegistry.Instance.WorkflowManagementMode.Value);

			TestDeleteWorkflow_DbHits(new Dictionary<string, int>
			{
				{ BMNCNAttachmentSchema.Constants.TableName, 3 },
				{ BMNCNShapeSchema.Constants.TableName, 3 }, // 1 for the diagram itself, 1 for its descendents, and 1 for their direct children.
				{ ProcessTaskIterationLinkSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ StmUniversalCopySchema.Constants.TableName, 2 },
				{ StmDocDataOverrideSchema.Constants.TableName, 4 },
			});
		}

		public void TestDeleteWorkflow_DbHits_NotInPlanningManagementMode()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			TestDeleteWorkflow_DbHits(new Dictionary<string, int> // NB: we still want to hit BMNCN-related tables even when not in planning management mode, since we don't want orphaned shapes and attachments
			{
				{ BMNCNAttachmentSchema.Constants.TableName, 1 },
				{ BMNCNShapeSchema.Constants.TableName, 1 },
				{ ProcessTaskIterationLinkSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ StmUniversalCopySchema.Constants.TableName, 2 },
				{ StmDocDataOverrideSchema.Constants.TableName, 4 },
			});
		}

		void TestDatabaseHitsDuringLoad(Dictionary<string, int> hits)
		{
			var capability = CreateCapability("INF", "Individuality");
			var staff1 = CreateStaffInCurrentBranchDept("STI", "Stinky McBill");
			var staff2 = CreateStaffInCurrentBranchDept("STD", "Standard Deviant");

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "Showers can be uncomfortable");
			var task1_1 = CreateTask(workflow1, staff1.GS_Code, 25);
			var task1_2 = CreateTask(workflow1, staff2.GS_Code, 25);

			var workflow2 = CreateWorkflow(jobHeader, "Especially outdoors.");
			var task2_1 = CreateTask(workflow2, staff1.GS_Code, 25);
			var task2_2 = CreateTask(workflow2, staff2.GS_Code, 25);

			var workflow3 = CreateWorkflow(jobHeader, "Eagles mum.");
			var task3_1 = CreateTask(workflow3, staff1.GS_Code, 25);
			var task3_2 = CreateTask(workflow3, staff2.GS_Code, 25);

			var defaultdiagram = jobHeader.GetDefaultDiagram();
			NetworkTestCase.CreateNetwork(defaultdiagram);
			Factory.Save();
			RowFactory.ResetCacheAfterDbUpgrade();

			var cleanFactory = Factory.CreateNewFactory();
			var freshJobHeader = cleanFactory.Load<ProcessHeader>(jobHeader.PK);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(freshJobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindAll<ZTabControl>().Single(c => c.Name == "WorkflowNCNTabControl");
				tabControl.SelectedIndex = 1;

				Application.DoEvents();

				AssertDbHits(hits, cleanFactory);
				form.Controls.Remove(control);
			}
		}

		[TestDate(2017, 10, 11)]
		public void TestDatabaseHitsDuringLoad_PlanningManagementMode()
		{
			AssertEquals(WorkflowManagementModes.Codes.PlanningManagement, BMSRegistry.Instance.WorkflowManagementMode.Value);

			TestDatabaseHitsDuringLoad(new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessHeaderSchema.Constants.TableName, 2 }, // 1 for loading freshJobHeader, others for loading the various collections needed. This is not O(N) per workflow, but EntityFramework is not able to perform more-specific queries in-memory when FH_ParentTableCode is part of the predicate.
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ BMNCNShapeSchema.Constants.TableName, 3 }, // 1 for the diagram itself, 1 for its descendents, and 1 for their direct children.
				{ BMNCNAttachmentSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ GlbHolidaySchema.Constants.TableName, 1 },
				{ GlbWorkTimeSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbDepartmentSchema.Constants.TableName, 1 },
			});
		}

		[TestDate(2017, 10, 11)]
		public void TestDatabaseHitsDuringLoad_NotInPlanningManagementMode()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			TestDatabaseHitsDuringLoad(new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessHeaderSchema.Constants.TableName, 2 }, // 1 for loading freshJobHeader, others for loading the various collections needed. This is not O(N) per workflow, but EntityFramework is not able to perform more-specific queries in-memory when FH_ParentTableCode is part of the predicate.
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ BMNCNShapeSchema.Constants.TableName, 0 }, // no need to hit shapes when not in planning mode.
				{ BMNCNAttachmentSchema.Constants.TableName, 0 }, // no need to hit attachments
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ GlbHolidaySchema.Constants.TableName, 0 },
				{ GlbWorkTimeSchema.Constants.TableName, 0 },
				{ RefTimeZoneSchema.Constants.TableName, 0 },
				{ RefTimeZoneSetSchema.Constants.TableName, 0 },
				{ RefUNLOCOSchema.Constants.TableName, 0 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 0 },
				{ GlbDepartmentSchema.Constants.TableName, 0 },
			});
		}

		void TestDatabaseHitsDuringLoad_Extended(Dictionary<string, int> hits)
		{
			var capability = CreateCapability("HON", "Honour");
			var staff1 = CreateStaffInCurrentBranchDept("INT", "Integrity MacDoople");
			var staff2 = CreateStaffInCurrentBranchDept("RAI", "Rainbow Socks");

			var jobHeader = CreateJobHeader<OrgHeader>(false);

			ProcessHeader lastWorkflow = null;
			for (var i = 0; i < 20; i++)
			{
				var workflow = CreateWorkflow(jobHeader, "Justice" + i);
				var task1 = CreateTask(workflow, staff1.GS_Code, 25);
				var task2 = CreateTask(workflow, staff2.GS_Code, 25);

				if (lastWorkflow != null)
				{
					lastWorkflow.MakePrerequisiteOf(workflow);
				}

				lastWorkflow = workflow;
			}

			var defaultdiagram = jobHeader.GetDefaultDiagram();
			NetworkTestCase.CreateNetwork(defaultdiagram);
			Factory.Save();
			RowFactory.ResetCacheAfterDbUpgrade();

			var cleanFactory = Factory.CreateNewFactory();
			var freshJobHeader = cleanFactory.Load<ProcessHeader>(jobHeader.PK);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(freshJobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var tabControl = form.FindAll<ZTabControl>().Single(c => c.Name == "WorkflowNCNTabControl");
				tabControl.SelectedIndex = 1;

				Application.DoEvents();

				AssertDbHits(hits, cleanFactory);
			}
		}

		[TestDate(2017, 10, 11)]
		public void TestDatabaseHitsDuringLoad_Extended_PlanningManagementMode()
		{
			AssertEquals(WorkflowManagementModes.Codes.PlanningManagement, BMSRegistry.Instance.WorkflowManagementMode.Value);

			TestDatabaseHitsDuringLoad_Extended(new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessHeaderSchema.Constants.TableName, 2 }, // 1 for loading freshJobHeader, others for loading the various collections needed. This is not O(N) per workflow, but EntityFramework is not able to perform more-specific queries in-memory when FH_ParentTableCode is part of the predicate.
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ BMNCNShapeSchema.Constants.TableName, 3 }, // 1 for the diagram itself, 1 for its descendents, and 1 for their direct children.
				{ BMNCNAttachmentSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ GlbHolidaySchema.Constants.TableName, 1 },
				{ GlbWorkTimeSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbDepartmentSchema.Constants.TableName, 1 },
			});
		}

		[TestDate(2017, 10, 11)]
		public void TestDatabaseHitsDuringLoad_Extended_NotInPlanningManagementMode()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			TestDatabaseHitsDuringLoad_Extended(new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessHeaderSchema.Constants.TableName, 2 }, // 1 for loading freshJobHeader, others for loading the various collections needed. This is not O(N) per workflow, but EntityFramework is not able to perform more-specific queries in-memory when FH_ParentTableCode is part of the predicate.
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ BMNCNShapeSchema.Constants.TableName, 0 }, // no need to hit shapes when not in planning mode.
				{ BMNCNAttachmentSchema.Constants.TableName, 0 }, // no need to hit attachments
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ GlbHolidaySchema.Constants.TableName, 0 },
				{ GlbWorkTimeSchema.Constants.TableName, 0 },
				{ RefTimeZoneSchema.Constants.TableName, 0 },
				{ RefTimeZoneSetSchema.Constants.TableName, 0 },
				{ RefUNLOCOSchema.Constants.TableName, 0 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 0 },
				{ GlbDepartmentSchema.Constants.TableName, 0 },
			});
		}

		void TestManyPrereqs_DbHits(Dictionary<string, int> hitsBeforeReset)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var component = BMSTestHelper.CreateBucket(config.System, "new Component");

			const int numWorkflows = 20;

			var workflows = new ProcessHeader[numWorkflows];
			ProcessHeader previousWorkflow = null;

			for (var i = 0; i < numWorkflows; i++)
			{
				var workflow = workflows[i] = BMSTestHelper.CreateWorkflow(jobHeader, "workflow" + i, currentComponent: component);
				BMSTestHelper.CreateTask(workflow);

				if (i > 0)
				{
					previousWorkflow.GetOrCreateDependencyLink(workflow);
				}

				previousWorkflow = workflow;
			}

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflows[0].Tasks.Single()))
			{
				Application.DoEvents();

				var workflowsUserControl = form.FindAll<WorkflowManagementUserControl>().Single();

				workflowsUserControl.WorkflowsGrid.ListManager.Position = 0;
				Application.DoEvents();

				AssertDbHitsWithUsefulQueryInformation(hitsBeforeReset, form.BusinessEntity.Factory);

				form.BusinessEntity.Factory.ResetDatabaseLoadCount();

				for (var i = 1; i < numWorkflows; i++)
				{
					workflowsUserControl.WorkflowsGrid.ListManager.Position = i;
					Application.DoEvents();
				}

				AssertDbHits(new Dictionary<string, int> // after reset -- no further hits.
				{
					{ BMNCNShapeSchema.Constants.TableName, 0 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 0 },
					{ StmNoteSchema.Constants.TableName, 0 },
					{ TagLinkSchema.Constants.TableName, 0 },
				}, form.BusinessEntity.Factory);
			}
		}

		public void TestManyPrereqs_DbHits_PlanningManagementMode()
		{
			AssertEquals(WorkflowManagementModes.Codes.PlanningManagement, BMSRegistry.Instance.WorkflowManagementMode.Value);

			TestManyPrereqs_DbHits(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMNCNShapeSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 2 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgOpportunitySchema.Constants.TableName, 1 },
				{ OrgWebURLSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ RefComplianceListSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ TagLinkSchema.Constants.TableName, 1 }
			});
		}

		public void TestManyPrereqs_DbHits_NotInPlanningManagementMode()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			TestManyPrereqs_DbHits(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMNCNShapeSchema.Constants.TableName, 0 }, // no need to hit shapes
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 2 },
				{ OrgCountryDataSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgOpportunitySchema.Constants.TableName, 1 },
				{ OrgWebURLSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 2 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ RefComplianceListSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ TagLinkSchema.Constants.TableName, 1 }
			});
		}

		public void TestLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "Workflow1", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow2", config.Bucket);
			BMSTestHelper.MakeChildOf(workflow2, workflow1);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");
				using (WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow1))
				{
					Application.DoEvents();
					var relevantQueries = Db.Connection.ExecutedCommands.Where(x => x.Contains(ProcessHeaderLinkSchema.Constants.FP_FH_HeaderFrom + " in") || x.Contains(ProcessHeaderLinkSchema.Constants.FP_FH_HeaderTo + " in"));

					CombineAssertions("The following queries use an OR operator for HeaderFrom and HeaderTo queries. These should be executed separately. They may be caused by fetch hints. The OR is very bad for query performance. SAD! You should fix this.", () =>
					{
						foreach (var query in relevantQueries)
						{
							AssertNotContains("or (FP_FH_HeaderTo in", query, ignoreCase: true);
							AssertNotContains("or (FP_FH_HeaderFrom in", query, ignoreCase: true);
							AssertContains("Table valued parameters should be used. SAD!", "SELECT Value FROM @CWO", query, ignoreCase: true);
						}
					});
				}
			}
		}

		#endregion

		#region NCN tab

#if !WINZOR
		public void TestDefaultDiagramDeletedInAnotherFactory_AndNetworkReloaded_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks((OrgHeader)workflow.Parent);

			var defaultDiagram = workflow.JobHeader.GetDefaultDiagram();
			defaultDiagram.Name += " diddly";

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var workflowsControl = form.FindAll<WorkflowsUserControl>().Single();
				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.NCNTabPage_Exposed;
				Application.DoEvents();

				workflowsControl.RelationshipDesignerUserControl.Refresh(new RefreshArgs(RefreshType.RedrawDiagram));

				form.BusinessEntity.Factory.Load<BMNCNShape>(defaultDiagram.PK).Delete();
				form.Dispose();

				AssertNoExceptionThrown("NCN tab page can be refreshed before the diagram is deleted and a form is disposed, probably because the job was deleted. This shouldn't cause the NCN tab page to explode.", () => Application.DoEvents());
			}
		}

		public void TestChangeToNCNTab_ShouldNotHaveChangesUntilUserDoesSomething()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			jobHeader.FH_CompletionStatement = "jobHeader";
			workflow1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			workflow2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks((OrgHeader)jobHeader.Parent);

			Factory.Save();

			var shapeCount = Factory.GetDatabaseCount(typeof(BMNCNShape));
			var attachmentCount = Factory.GetDatabaseCount(typeof(BMNCNAttachment));

			using (var form = new ZOrganisationsForm((OrgHeader)jobHeader.Parent) { Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			{
				form.Show();
				form.OrganisationsTabControl.SelectedTab = form.OrganisationsTabControl.TabPages.OfType<ZWorkflowTabPage>().Single();
				Application.DoEvents();

				var workflowsControl = form.FindAll<WorkflowsUserControl>().Single();
				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.NCNTabPage_Exposed;
				Application.DoEvents();

				var diagram = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, jobHeader.PK));
				AssertNotNull("Diagram should be created when accessing NCN tab", diagram);

				var childShape1 = diagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow1.PK);
				var childShape2 = diagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow2.PK);

				NetworkTestCase.AssertShapeOffset(childShape1.AsEntity(workflowsControl.RelationshipDesignerUserControl.Network), diagram.AsEntity(workflowsControl.RelationshipDesignerUserControl.Network), 0.0, 0.0);
				NetworkTestCase.AssertShapeOffset(childShape2.AsEntity(workflowsControl.RelationshipDesignerUserControl.Network), diagram.AsEntity(workflowsControl.RelationshipDesignerUserControl.Network), 345.0, 0.0);

				AssertEquals(false, jobHeader.HasChanges);
				AssertEquals(false, workflow1.HasChanges);
				AssertEquals(false, workflow2.HasChanges);
				AssertEquals(false, diagram.HasChanges);
				AssertEquals(false, childShape1.HasChanges);
				AssertEquals(false, childShape2.HasChanges);

				var postingButtonsProvider = (IPostingButtonsProvider)form;
				AssertEquals(false, postingButtonsProvider.CommandButtonPost.Enabled);
				AssertEquals(true, postingButtonsProvider.CommandButtonCancel.Enabled);

				AssertSaved(form.FireSaveButton());

				AssertEquals("Should be no new shapes in the DB", shapeCount, Factory.GetDatabaseCount(typeof(BMNCNShape)));
				AssertEquals("Should be no new attachments in the DB", attachmentCount, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

				AssertEquals(false, diagram.IsDeleted);
				AssertEquals(false, diagram.IsInDatabase);

				AssertEquals("Should use same shape instance", diagram, jobHeader.GetDefaultDiagram());
				AssertEquals("Should use same shape instance", childShape1, diagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow1.PK));
				AssertEquals("Should use same shape instance", childShape2, diagram.ChildShapes.Single(s => s.BNS_RelatedEntityID == workflow2.PK));

				workflow1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;

				AssertEquals(false, jobHeader.HasChanges);
				AssertEquals(true, workflow1.HasChanges);
				AssertEquals(false, workflow2.HasChanges);
				AssertEquals(false, diagram.HasChanges);
				AssertEquals(false, childShape1.HasChanges);
				AssertEquals(false, childShape2.HasChanges);

				workflow1.FH_CompletionStatement = "This shouldn't cause shapes to have changes";

				AssertEquals(false, jobHeader.HasChanges);
				AssertEquals(true, workflow1.HasChanges);
				AssertEquals(false, workflow2.HasChanges);
				AssertEquals(false, diagram.HasChanges);
				AssertEquals(false, childShape1.HasChanges);
				AssertEquals(false, childShape2.HasChanges);

				AssertSaved(form.FireSaveButton());

				AssertEquals("Should be no new shapes in the DB", shapeCount, Factory.GetDatabaseCount(typeof(BMNCNShape)));
				AssertEquals("Should be no new attachments in the DB", attachmentCount, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

				AssertEquals(false, diagram.IsDeleted);
				AssertEquals(false, diagram.IsInDatabase);

				childShape1.BNS_Name = "sprugle";
				childShape2.BNS_Name = "splartt";

				AssertSaved(form.FireSaveButton());

				AssertEquals("New shapes should be saved now that there's a real change", shapeCount + 3, Factory.GetDatabaseCount(typeof(BMNCNShape)));
				AssertEquals("New attachment should be saved now that there's a real change", attachmentCount + 1, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

				AssertEquals(true, diagram.IsInDatabase);
			}
		}
#endif

		public void TestTabControl_WhenPlanningManagementEnabled_ShouldIncludeRelatedDiagramsTab()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertRelatedDiagramsTabShown("The Related Diagrams tab should be available because Planning Management is enabled.", true);
		}

		public void TestTabControl_WhenBufferManagementWorkflowModeEnabled_ShouldNotIncludeRelatedDiagramsTab()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertRelatedDiagramsTabShown("The Related Diagrams tab should not be available because Planning Management is not enabled.", false);
		}

		public void TestTabControl_WhenEnhancedWorkflowManagementEnabled_ShoulNotdIncludeRelatedDiagramsTab()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertRelatedDiagramsTabShown("The Related Diagrams tab should not be available because Planning Management is not enabled.", false);
		}

		void AssertRelatedDiagramsTabShown(string message, bool shouldBeShown)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var workflowsControl = form.FindSingle<WorkflowManagementUserControl>();
				var tabs = workflowsControl.WorkflowTasksTabPage.GetParent<ZTabControl>().AllTabPages.Select(x => x.Text);
				var expected = shouldBeShown
					? new[] { "Tasks", "Workflow Notes", "Related Diagrams", }
					: new[] { "Tasks", "Workflow Notes", };

				AssertContainsExactElementsInAnyOrder(message, expected, tabs);
			}
		}

		#endregion

		#region Workflow Grid

		public void TestJobWorkflow_ReleaseGroupShouldNeverBeReadOnly()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "DUM");

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, taskType: "QCB");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1200, 1000) })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				Factory.Save();

				var jobLevelWorkflowPosition = 0;
				var workflowPosition = 1;

				control.WorkflowsGrid.ListManager.Position = jobLevelWorkflowPosition;
				Application.DoEvents();

				var testControl1 = form.FindAll<ZGuidDropEdit>().Single(c => c.Name == "ComponentDropEdit");
				AssertEquals("Compoonent should be read-only on a job level workflow", true, testControl1.ReadOnly);

				var testControl2 = form.FindAll<ZGuidFindBox>().Single(c => c.Name == "ReleaseGroupFindBox");
				AssertEquals("Release group should not be read-only on a job level workflow", false, testControl2.ReadOnly);

				control.WorkflowsGrid.ListManager.Position = workflowPosition;
				Application.DoEvents();

				testControl1 = form.FindAll<ZGuidDropEdit>().Single(c => c.Name == "ComponentDropEdit");
				AssertEquals("Compoonent should not be read-only on a workflow", false, testControl1.ReadOnly);

				testControl2 = form.FindAll<ZGuidFindBox>().Single(c => c.Name == "ReleaseGroupFindBox");
				AssertEquals("Release group should not be read-only on a workflow", false, testControl2.ReadOnly);
			}
		}

		public void TestCurrentComponentDropEditWidth_ShouldMatchSimilarControls()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");

			var jobHeader = CreateJobHeader<SalesEnquiry>(addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "It's at Bill's house, and Fred's house");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var control = form.FindSingle<WorkflowDetailsUserControl>();
				var componentDropEdit = control.FindSingle<ZDropEdit>("ComponentDropEdit");
				var releaseGroupFindBox = control.FindSingle<ZCodeFindBox>("ReleaseGroupFindBox");

				AssertEquals(releaseGroupFindBox.Width, componentDropEdit.Width);
			}
		}

		public void TestCreateNewRowAndThenDelete_ShouldDeleteRowWithoutPrompt()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var workflowGrid = control.WorkflowsGrid;
				workflowGrid.ListManager.AddNew();
				Application.DoEvents();
				form.Show();

				var workDescriptionColumnIndex = workflowGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);

				workflowGrid[3, workDescriptionColumnIndex] = new ZString("Boop");
				workflowGrid.ListManager.Position = 1; // Switch to the second row, causing the list to resequence

				workflowGrid.Select(0);
				workflowGrid.Select(1);
				workflowGrid.UnSelectAll(); // Touch up the grid a bit

				workflowGrid.Select(3);
				Application.DoEvents(); // The new workflow's row is selected
				form.Show();

				workflowGrid.DeleteMenuItem.PerformClick(); // Delete the row
				Application.DoEvents();
				form.Show();

				AssertEquals("The second row should contain the real workflow, and the old workflow should be deleted", "workflow1", workflowGrid[1, 2]);
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeleteRow_ShouldDeleteWorkflowWithPrompt()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var grid = control.WorkflowsGrid;

				grid.Select(1);
				Application.DoEvents();

				grid.DeleteMenuItem.PerformClick();
				Application.DoEvents();

				Factory.Save();
			}

			AssertEquals(true, workflow1.IsDeleted);
			AssertEquals(false, workflow2.IsDeleted);

			AssertEquals("Delete workflow 'workflow1' and all associated tasks?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectNoExceptions]
		public void TestNavigateToWorkflowItemWithTwoProcessHeaders()
		{
			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow1");
			var task = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			task.P9_FH_ProcessHeader = ZGuid.Empty;

			using (var form = new ZOrganisationsForm(org))
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task);
			}
		}

		public void TestDeletingWorkflowDoesNotCrashTheForm()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Original Workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Don't do it");

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var control = form.FindSingle<WorkflowManagementUserControl>();
				var workflowsGrid = control.WorkflowsGrid;
				var tasksGrid = control.TaskDetailsControl.TasksGrid;
				var taskDescriptionIndex = tasksGrid.Columns.IndexOf(x => x.ColumnName == ProcessTasksSchema.P9_Description.Name);
				var workflowCompletionIndex = workflowsGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);

				AssertEquals("Original Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);

				Application.DoEvents();

				EditDescription(workflowsGrid, 2, "0");

				workflowsGrid.ListManager.Position = 1;
				Application.DoEvents();
				workflowsGrid.Select(1);
				Application.DoEvents();
				workflowsGrid.OnDeleteKeyPressed();

				Application.DoEvents();
				EditDescription(workflowsGrid, 2, "w3");

				workflowsGrid.ListManager.EndCurrentEdit();
				workflowsGrid.ListManager.Position = 1;
				Application.DoEvents();
			}
		}

		static void EditDescription(ZGrid workflowsGrid, int index, string value)
		{
			var descriptionColumn = workflowsGrid.TableStyles[0].GridColumnStyles[2];
			workflowsGrid.BeginEdit(descriptionColumn, index);
			Application.DoEvents();
			AssertEquals(index, workflowsGrid.CurrentRowIndex);
			((ZTextBoxColumnStyle)descriptionColumn).EditControl.Focus();
			Application.DoEvents();
			((ZTextBoxColumnStyle)descriptionColumn).EditControl.Text = value;
			Application.DoEvents();
			var parent = ((ZTextBoxColumnStyle)descriptionColumn).EditControl.Parent;
			parent.Focus();
			Application.DoEvents();
			Assert(parent.Focused);
		}

		public void TestAfterAddingNewWorkflow_ThenSave_ShouldKeepNewWorkflowSelected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Original Workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Don't do it");

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var control = form.FindSingle<WorkflowManagementUserControl>();
				var workflowsGrid = control.WorkflowsGrid;
				var tasksGrid = control.TaskDetailsControl.TasksGrid;
				var taskDescriptionIndex = tasksGrid.Columns.IndexOf(x => x.ColumnName == ProcessTasksSchema.P9_Description.Name);
				var workflowCompletionIndex = workflowsGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);

				AssertEquals("Original Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);

				var newWorkflow = ((WorkflowManagementViewModel)workflowsGrid.DataSource).AllProcessHeaders.AddNew();
				Application.DoEvents();
				workflowsGrid[2, workflowCompletionIndex] = new ZString("New Workflow");
				Application.DoEvents();
				AssertNoErrors(newWorkflow);

				var newAndNowSelectedWorkflow = ((WorkflowManagementViewModel)tasksGrid.DataSource).Workflows.First(wf => wf.PK == newWorkflow.PK);
				AssertEquals("PRE: We can find a workflow through a viewmodel", newWorkflow, newAndNowSelectedWorkflow);

				var newTask = newAndNowSelectedWorkflow.TaskCollection.AddNew();
				Application.DoEvents();
				tasksGrid[0, taskDescriptionIndex] = new ZString("Do it");
				Application.DoEvents();
				AssertNoErrors(newTask);

				AssertEquals("Should remain on the new workflow and not switch back to the existing workflow. SAD!", "New Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);

				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals("Should remain on the new workflow and not switch back to the existing workflow. SAD!", "New Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);

				var job = new BusinessObjectFactory().Load<SalesEnquiry>(jobHeader.FH_ParentId);
				var tasks = job.WorkflowItems.Tasks.Cast<ProcessTask>();
				AssertContainsExactElementsInAnyOrder(new[] { "Don't do it", "Do it" }, tasks.Select(x => x.P9_Description));
			}
		}

		public void TestAfterAddingNewWorkflow_SwitchingToAndModifyingExistingWorkflow_ThenSave_ShouldNotSelectNewWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Original Workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Task");

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var control = form.FindSingle<WorkflowManagementUserControl>();
				var workflowsGrid = control.WorkflowsGrid;
				var workflowCompletionIndex = workflowsGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);

				AssertEquals("Original Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);

				var newWorkflow = ((WorkflowManagementViewModel)workflowsGrid.DataSource).AllProcessHeaders.AddNew();
				Application.DoEvents();
				workflowsGrid[2, workflowCompletionIndex] = new ZString("New Workflow");
				Application.DoEvents();
				AssertNoErrors(newWorkflow);

				var tasksGrid = control.TaskDetailsControl.TasksGrid;
				var taskDescriptionIndex = tasksGrid.Columns.IndexOf(x => x.ColumnName == ProcessTasksSchema.P9_Description.Name);
				var taskStatusIndex = tasksGrid.Columns.IndexOf(x => x.ColumnName == ProcessTasksSchema.P9_Status.Name);

				var newAndNowSelectedWorkflow = ((WorkflowManagementViewModel)tasksGrid.DataSource).Workflows.First(wf => wf.PK == newWorkflow.PK);
				AssertEquals("PRE: We can find a workflow through a viewmodel", newWorkflow, newAndNowSelectedWorkflow);

				var newTask = newAndNowSelectedWorkflow.TaskCollection.AddNew();
				Application.DoEvents();
				tasksGrid[0, taskDescriptionIndex] = new ZString("Bip");
				Application.DoEvents();
				AssertNoErrors(newTask);

				workflowsGrid.ListManager.Position = 1; // Go back to existing workflow and update its description on its task
				AssertEquals("Original Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);
				Application.DoEvents();
				tasksGrid[0, taskDescriptionIndex] = new ZString("Bop");
				Application.DoEvents();

				CombineAssertions("Before first form save", () =>
				{
					AssertEquals("Should be on the existing workflow", "Original Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);
					AssertEquals(1, workflowsGrid.ListManager.Position);
					AssertEquals("Bop", ((ProcessTask)tasksGrid.ListManager.GetCurrent()).P9_Description);
				});

				form.FireSaveButton();

				CombineAssertions("After first form save", () =>
				{
					AssertEquals("Should remain on the workflow with changes and not swap back to the new workflow", "Original Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);
					AssertEquals("The workflow list stayed sorted", 1, workflowsGrid.ListManager.Position);
					AssertEquals("Bop", ((ProcessTask)tasksGrid.ListManager.GetCurrent()).P9_Description);
				});

				var job = new BusinessObjectFactory().Load<SalesEnquiry>(jobHeader.FH_ParentId);
				var tasks = job.WorkflowItems.Tasks.Cast<ProcessTask>();
				AssertContainsExactElementsInAnyOrder(new[] { "Bip", "Bop" }, tasks.Select(x => x.P9_Description));

				workflowsGrid.ListManager.Position = 2; // Switch back to new workflow and update its task
				Application.DoEvents();
				AssertEquals("New Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);

				tasksGrid[0, taskDescriptionIndex] = new ZString("Bap");
				Application.DoEvents();

				CombineAssertions("Before second form save", () =>
				{
					AssertEquals("Should be on the new workflow", "New Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);
					AssertEquals(2, workflowsGrid.ListManager.Position);
					AssertEquals("Bap", ((ProcessTask)tasksGrid.ListManager.GetCurrent()).P9_Description);
				});

				form.FireSaveButton();
				Application.DoEvents();

				CombineAssertions("After second form save", () =>
				{
					AssertEquals("Should remain on the new workflow", "New Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);
					AssertEquals(2, workflowsGrid.ListManager.Position);
					AssertEquals("Bap", ((ProcessTask)tasksGrid.ListManager.GetCurrent()).P9_Description);
				});

				// Set the status on its task
				tasksGrid[0, taskStatusIndex] = new ZString("WRK");
				Application.DoEvents();
				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals("Should remain on the current workflow and not switch to job workflow", "New Workflow", ((ProcessHeader)workflowsGrid.ListManager.GetCurrent()).FH_CompletionStatement);
			}
		}

		public void TestUpdatingChildWorkflow_OnFormSave_ShouldNotCauseParentWorkflowToBeSelected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code);

			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow (Quality Iteration 1)", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var taskInChildWorkflow = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);

			Factory.Save();

			AssertEquals(parentWorkflow, childWorkflow.WorkflowParent);

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(taskInChildWorkflow))
			{
				Application.DoEvents();

				var workflowGrid = form.FindSingle<WorkflowManagementUserControl>().WorkflowsGrid;
				AssertEquals(childWorkflow.FH_CompletionStatement, ((ProcessHeader)workflowGrid.ListManager.Current).FH_CompletionStatement);

				var taskGrid = form.FindSingle<TaskDetailsUserControl>().TasksGrid;
				var taskInGrid = (ProcessTask)taskGrid.ListManager.Current;
				taskInGrid.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals("The list should not have selected the parent workflow. SAD!", childWorkflow.FH_CompletionStatement, ((ProcessHeader)workflowGrid.ListManager.Current).FH_CompletionStatement);
			}

			var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(taskInChildWorkflow.PK);
			AssertEquals("The task should have actually been saved. If it wasn't, then we aren't really testing the main issue that this test is meant to address. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
		}

		public void TestUpdatingChildWorkflow_OnFormSave_WhenParentWorkflowSelected_ShouldNotCauseChildWorkflowToBeSelected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Parent Task", sequence: 1);

			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow (Quality Iteration 1)", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var taskInChildWorkflow = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Child Task", sequence: 2);
			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);

			Factory.Save();

			AssertEquals(parentWorkflow, childWorkflow.WorkflowParent);

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(parentWorkflow))
			{
				Application.DoEvents();

				var workflowGrid = form.FindSingle<WorkflowManagementUserControl>().WorkflowsGrid;
				AssertEquals(parentWorkflow.FH_CompletionStatement, ((ProcessHeader)workflowGrid.ListManager.Current).FH_CompletionStatement);

				var taskGrid = form.FindSingle<TaskDetailsUserControl>().TasksGrid;
				taskGrid.ListManager.Position = 1;
				var taskInGrid = (ProcessTask)taskGrid.ListManager.Current;
				AssertEquals(taskInChildWorkflow.P9_Description, taskInGrid.P9_Description);

				taskInGrid.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals("The list should not have selected the child workflow. SAD!", parentWorkflow.FH_CompletionStatement, ((ProcessHeader)workflowGrid.ListManager.Current).FH_CompletionStatement);
			}

			var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(taskInChildWorkflow.PK);
			AssertEquals("The task should have actually been saved. If it wasn't, then we aren't really testing the main issue that this test is meant to address. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
		}

		public void TestSelectWorkflowAndTask_OnFormSave_ShouldKeepThePreviouslySelectedTaskSelected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var taskInWorkflow1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, description: "Task in workflow 1", sequence: 1);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, description: "Task 1", sequence: 2);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, description: "Task 2", sequence: 3);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, description: "Task 3", sequence: 4);

			Factory.Save();

			void AssertTaskDoesntMove(string taskDescription, int taskPosition)
			{
				using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(taskInWorkflow1))
				{
					Application.DoEvents();

					var workflowGrid = form.FindSingle<WorkflowManagementUserControl>().WorkflowsGrid;
					AssertEquals("Workflow 1", ((ProcessHeader)workflowGrid.ListManager.Current).FH_CompletionStatement);

					workflowGrid.ListManager.Position = 2;
					Application.DoEvents();
					AssertEquals("Workflow 2", ((ProcessHeader)workflowGrid.ListManager.Current).FH_CompletionStatement);

					var taskGrid = form.FindSingle<TaskDetailsUserControl>().TasksGrid;
					taskGrid.ListManager.Position = taskPosition;
					var selectedTask = (ProcessTask)taskGrid.ListManager.GetCurrent();
					AssertEquals(taskDescription, selectedTask.P9_Description);

					selectedTask.P9_Status = "SUS";
					form.FireSaveButton();
					Application.DoEvents();
					AssertEquals("The selection should not have changed when the form was saved. SAD!", taskDescription, ((ProcessTask)taskGrid.ListManager.GetCurrent()).P9_Description);
				}
			}

			AssertTaskDoesntMove("Task 1", 0);
			AssertTaskDoesntMove("Task 2", 1);
			AssertTaskDoesntMove("Task 3", 2);
		}

		public void TestSelectChildWorkflowAndTask_OnFormSave_ShouldKeepThePreviouslySelectedTaskSelected()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Parent Task", sequence: 1);

			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow (Quality Iteration 1)", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var taskInChildWorkflow1 = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Child Task 1", sequence: 2);
			var taskInChildWorkflow2 = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, description: "Child Task 2", sequence: 3);
			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);

			Factory.Save();

			AssertEquals(parentWorkflow, childWorkflow.WorkflowParent);

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(taskInChildWorkflow2))
			{
				Application.DoEvents();

				var taskGrid = form.FindSingle<TaskDetailsUserControl>().TasksGrid;
				var taskInGrid = (ProcessTask)taskGrid.ListManager.Current;
				AssertEquals(taskInChildWorkflow2.P9_Description, taskInGrid.P9_Description);

				taskInGrid.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				form.FireSaveButton();
				Application.DoEvents();

				AssertEquals("The list should have kept the second task in the child workflow selected.. SAD!", "Child Task 2", ((ProcessTask)taskGrid.ListManager.Current).P9_Description);
			}

			var loadedTask = new BusinessObjectFactory().Load<ProcessTask>(taskInChildWorkflow2.PK);
			AssertEquals("The task should have actually been saved. If it wasn't, then we aren't really testing the main issue that this test is meant to address. SAD!", ProcessTaskStatusCodeList.Codes.Working, loadedTask.P9_Status);
		}

		public void TestChangeSystemAndCurrentComponent()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system1.FS_Name = "Fast";
			var bucketOrg1 = BMSTestHelper.CreateBucket(system1, "bucketOrg1");
			var bucketOrg2 = BMSTestHelper.CreateBucket(system1, "bucketOrg2");
			BMSTestHelper.LinkComponents(bucketOrg1, bucketOrg2);

			var system2 = BMSTestHelper.CreateSystem(Factory, "INQ");
			system2.FS_Name = "Slow";
			var bucketInq1 = BMSTestHelper.CreateBucket(system2, "bucketInq1");
			var bucketInq2 = BMSTestHelper.CreateBucket(system2, "bucketInq2");
			BMSTestHelper.LinkComponents(bucketInq1, bucketInq2);

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow1");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			workflow.FH_FC_CurrentComponent = bucketOrg1.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Frodo Baggins";
			staff.GS_Code = "FRO";

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(jobHeader))
			{
				Application.DoEvents();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					var workflowManagementControl = form.FindSingle<WorkflowManagementUserControl>();
					var workflowDetailControl = workflowManagementControl.FindSingle<WorkflowDetailsUserControl>();
					var systemNameFindBox = workflowDetailControl.FindSingle<ZGuidFindBox>(c => c.Name == "SystemNameFindBox");
					AssertEquals("User does not have permission and it is a job-level workflow", true, systemNameFindBox.ReadOnly);

					var workflowsGrid = workflowManagementControl.WorkflowsGrid;
					workflowsGrid.ListManager.Position = 1;
					Application.DoEvents();

					AssertEquals("User does not have permission", true, systemNameFindBox.ReadOnly);

					workflowsGrid.ListManager.Position = 2;
					Application.DoEvents();

					AssertEquals("User still does not have permission, even when we select a row for a new workflow", true, systemNameFindBox.ReadOnly);
				}
			}

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(jobHeader))
			{
				Application.DoEvents();

				var workflowManagementControl = form.FindSingle<WorkflowManagementUserControl>();
				var workflowDetailControl = workflowManagementControl.FindSingle<WorkflowDetailsUserControl>();
				var systemNameFindBox = workflowDetailControl.FindSingle<ZGuidFindBox>(c => c.Name == "SystemNameFindBox");
				AssertEquals("User has permission, but this is a job-level workflow, so the find box should still be readonly", true, systemNameFindBox.ReadOnly);

				var componentDropEdit = workflowDetailControl.FindSingle<ZGuidDropEdit>(c => c.Name == "ComponentDropEdit");
				AssertEquals(string.Empty, componentDropEdit.Text);
				AssertEquals(true, componentDropEdit.ReadOnly);
			}

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow))
			{
				Application.DoEvents();

				var workflowManagementControl = form.FindSingle<WorkflowManagementUserControl>();
				var workflowDetailControl = workflowManagementControl.FindSingle<WorkflowDetailsUserControl>();
				var systemNameFindBox = workflowDetailControl.FindSingle<ZGuidFindBox>(c => c.Name == "SystemNameFindBox");
				AssertEquals("User has permission, and this is a workflow, so the find box should not be readonly", false, systemNameFindBox.ReadOnly);

				var componentDropEdit = workflowDetailControl.FindSingle<ZGuidDropEdit>(c => c.Name == "ComponentDropEdit");
				AssertEquals(false, componentDropEdit.ReadOnly);
				AssertEquals("bucketOrg1", componentDropEdit.Text);

				workflow.CurrentComponentSystemPK = system2.PK;
				Factory.Save();

				Application.DoEvents();
				AssertEquals("bucketInq1", componentDropEdit.Text);
			}
		}

		public void TestShowWorkflowTab_ForJobNotYetInBufferManagementSystem_ShouldNotStackOverflow()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();

			AssertNull(ProcessJobHeaderProvider.GetForParent(job, Factory));

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var system = BMSTestHelper.CreateSystem(newFactory, "INQ");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			newFactory.Save();

			using (var form = (ZTemplateForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowEditForm(newFactory.Load<SalesEnquiry>(job.PK)))
			{
				AssertNull(ProcessJobHeader.GetForParentWithoutCreation(job, form.BusinessEntity.Factory));

				var mainTabControl = form.FindAll<ZTemplateTabControl>().First();
				var workflowTabPage = mainTabControl.FindAll<ZWorkflowTabPage>().First();

				mainTabControl.SelectedTab = workflowTabPage;
				Application.DoEvents();

				AssertNotNull(ProcessJobHeader.GetForParentWithoutCreation(job, form.BusinessEntity.Factory));
			}
		}

		public void TestShowWorkflowTab_AndDeleteWorkflow_ForJobNotYetInBufferManagementSystem_ShouldNotStackOverflow()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var task1 = BMSTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(job, GlbStaff.CurrentUser.GS_Code);

			AssertNull(ProcessJobHeaderProvider.GetForParent(job, Factory));
			AssertEquals(ZGuid.Empty, task1.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, task2.P9_FH_ProcessHeader);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var config = TestConfigsHelper.CreateSchematicTestConfig(newFactory, "INQ");

			newFactory.Save();

			using (var form = (ZTemplateForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowEditForm(newFactory.Load<SalesEnquiry>(job.PK)))
			{
				AssertNull(ProcessJobHeader.GetForParentWithoutCreation(job, form.BusinessEntity.Factory));

				var mainTabControl = form.FindAll<ZTemplateTabControl>().First();
				var workflowTabPage = mainTabControl.FindAll<ZWorkflowTabPage>().First();

				mainTabControl.SelectedTab = workflowTabPage;
				Application.DoEvents();

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, form.BusinessEntity.Factory);

				AssertNotNull(jobHeader);
				AssertEquals(1, jobHeader.ProcessHeaders.Count);

				var workflow = jobHeader.ProcessHeaders[0];
				AssertEquals(BMGlobalConstants.DefaultWorkflowCompletionStatement, workflow.FH_CompletionStatement);
				AssertEquals(2, workflow.Tasks.Count());

				var managementControl = workflowTabPage.FindAll<WorkflowManagementUserControl>().Single();

				managementControl.WorkflowsGrid.Select(1);
				managementControl.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(true, workflow.IsDeleted);
				AssertNoExceptionThrown(() => form.FireSaveButton());
			}
		}

		public void TestWorkflowGrid_WorkflowGridListChanged_ShouldNotThrowExceptionsOrReportErrors()
		{
			var job = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_CompletionStatement = "job";
			var workflow = CreateWorkflow(jobHeader, "workflow1");
			workflow.FH_CompletionStatement = "work";
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10, description: "I miss Ryan and his whimsy");

			Factory.Save();

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(900, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var workflowsGrid = control.WorkflowsGrid;
				var workflowCompletionIndex = workflowsGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);
				workflowsGrid.ListManager.AddNew();

				workflowsGrid[2, workflowCompletionIndex] = new ZString("BoopyBooper");
				workflowsGrid.ListManager.ListChanged += control.WorkflowsGrid_ListChanged_ForTest;

				AssertNoExceptionThrown(() => control.WorkflowsGrid_ListChanged_ForTest(this, new ListChangedEventArgs(ListChangedType.ItemAdded, -1)));
				AssertEquals(0, ErrorReporter.TotalErrorCount);

				workflowsGrid[2, workflowCompletionIndex] = new ZString("BooperBoopy");
				AssertNoExceptionThrown(() => control.WorkflowsGrid_ListChanged_ForTest(this, new ListChangedEventArgs(ListChangedType.ItemAdded, 3)));
				AssertEquals(0, ErrorReporter.TotalErrorCount);

				workflowsGrid[2, workflowCompletionIndex] = new ZString("BooperyBoopy");
				AssertNoExceptionThrown(() => control.WorkflowsGrid_ListChanged_ForTest(this, new ListChangedEventArgs(ListChangedType.ItemAdded, 2)));

				AssertEquals(0, ErrorReporter.TotalErrorCount);

				workflowsGrid.ListManager.Position = 0;
				var tasksGrid = control.TaskDetailsControl.TasksGrid;
				var currentTask = (ProcessTask)tasksGrid.ListManager.Current;
				currentTask.P9_Status = "CLS";

				workflowsGrid.Focus();
				workflowsGrid[2, workflowCompletionIndex] = new ZString("BoopyBooperBoo");
				control.SetLastSelectedWorkflow_ForTest(workflow);

				AssertNoExceptionThrown(() => control.WorkflowsGrid_ListChanged_ForTest(this, new ListChangedEventArgs(ListChangedType.ItemAdded, -1)));

				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestWorkflowGrid_TasksReorderOnWorkflowDisplay()
		{
			SetupWorkflowSamplerBox(out ProcessJobHeader jobHeader, out ProcessHeader workflow1, out ProcessHeader workflow2, out ProcessTask task1, out ProcessTask task2, out ProcessTask task3, out ProcessTask task4);

			Factory.Save();

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(900, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var workflowGrid = control.WorkflowsGrid;
				workflowGrid.ListManager.Position = 1; // select workflow1

				var taskGrid = control.TaskDetailsControl.TasksGrid;
				taskGrid.ExposeAllColumns();
				taskGrid.ListManager.Position = 1; // select task1

				var taskGridDescriptionIndex = taskGrid.Columns.IndexOf(tg => tg.ColumnName == ProcessTasksSchema.P9_Description.Name);

				AssertEquals("PRE: Workflow1 should have two tasks; a master, and an apprentice.", taskGrid[0, taskGridDescriptionIndex], (task1.DescriptionWithReference));
				AssertEquals("PRE: Workflow1 should have two tasks; a master, and an apprentice.", taskGrid[1, taskGridDescriptionIndex], (task2.DescriptionWithReference));

				var taskGridSequenceIndex = taskGrid.Columns.IndexOf(tg => tg.ColumnName == ProcessTasksSchema.P9_Sequence.Name);
				taskGrid[0, taskGridSequenceIndex] = task2.NextSequenceNumber; // change the sequence of task1 to be 21, 1 more than the sequence of task2

				workflowGrid.ListManager.Position = 2; // click workflow2 in the workflow grid
				Application.DoEvents(); // let the program catch up on us selecting a new workflow in the workflow grid

				workflowGrid.ListManager.Position = 1; // click workflow1 in the workflow grid
				Application.DoEvents(); // do all events. Do them. Why not "PlayEvents", or "ContinueEvents" or something

				taskGrid.ListManager.Position = 1; // select task1 again, as it has the smollest task sequence

				AssertEquals("By changing task1 to to have a sequence of task2.Sequence + 1, the taskgrid of workflow1 should now be reordered, but instead...", taskGrid[0, taskGridDescriptionIndex], (task2.DescriptionWithReference));
				AssertEquals("By changing task1 to to have a sequence of task2.Sequence + 1, the taskgrid of workflow1 should now be reordered, but instead...", taskGrid[1, taskGridDescriptionIndex], (task1.DescriptionWithReference));
			}
		}

		public void TestWorkflowGrid_AddMultipleWorkflows_ThenLoseFocus()
		{
			var job = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_CompletionStatement = "job";

			Factory.Save();

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(900, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var workflowsGrid = control.WorkflowsGrid;
				var workflowCompletionColumnIndex = workflowsGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);
				var tasksGrid = control.TaskDetailsControl.TasksGrid;

				AssertEquals("PRE: The workflow grid should initialise with one item being the job", 1, workflowsGrid.ListManager.Count);

				workflowsGrid.CurrentCell = new DataGridCell(1, workflowCompletionColumnIndex);
				KeySender.PostKeyDown(workflowsGrid.LastFocusedColumn.EditControl, workflowsGrid.LastFocusedColumn.EditControl.Handle, Keys.A);
				Application.DoEvents();

				AssertEquals("The workflow grid should now have two items (job + one workflow)", 2, workflowsGrid.ListManager.Count);

				workflowsGrid.CurrentCell = new DataGridCell(2, workflowCompletionColumnIndex);
				KeySender.PostKeyDown(workflowsGrid.LastFocusedColumn.EditControl, workflowsGrid.LastFocusedColumn.EditControl.Handle, Keys.B);
				Application.DoEvents();

				AssertEquals("The workflow grid should now have three items (job + two workflows)", 3, workflowsGrid.ListManager.Count);

				workflowsGrid.CurrentCell = new DataGridCell(3, workflowCompletionColumnIndex);
				KeySender.PostKeyDown(workflowsGrid.LastFocusedColumn.EditControl, workflowsGrid.LastFocusedColumn.EditControl.Handle, Keys.C);
				Application.DoEvents();

				AssertEquals("The workflow grid should now have four items (job + three workflows)", 4, workflowsGrid.ListManager.Count);

				tasksGrid.Focus();
				Application.DoEvents();

				AssertEquals("The workflow grid should still have four items when focus is lost (job + three workflows)", 4, workflowsGrid.ListManager.Count);
			}
		}

		public void TestCreateQualityIterationWorkflow_OnGridSortedByCategoryDescription_ShouldNotThrowException()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "DUM");

			var jobHeader = CreateJobHeader<DummyWithWorkflow>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskType: "QCB");

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);
				var viewModel = (WorkflowManagementViewModel)control.BindingSource.DataSource;
				viewModel.AllProcessHeaders.ApplySort("CategoryDescription", ListSortDirection.Descending);

				AssertNoExceptionThrown(() => ((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(workflow));
			}
		}

		#endregion

		#region Save

		public void TestSaveForm_ShouldUpdatePlannedDuration()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(org);

			var task = org.WorkflowItems.Tasks.AddNew();
			task.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task.P9_Description = "Dat Task";
			task.P9_Type = "UDF";

			ProcessHeader workflow;

			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task);
				Application.DoEvents();

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
				AssertNotNull(jobHeader);
				AssertEquals(1, jobHeader.ProcessHeaders.Count);

				workflow = jobHeader.ProcessHeaders[0];
				workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

				AssertEquals(0, workflow.FH_PlannedDurationInMinutes);
				AssertSaved(form.FireSaveButton());
			}

			var reloadedWorkflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);
			AssertEquals(90, reloadedWorkflow.FH_PlannedDurationInMinutes);
		}

		#endregion

		#region Approved Schedule

		public void TestChangeSelectedWorkflow_ShouldUpdateWhetherApprovedScheduleIsDisplayed()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var nonApprovedShape = Factory.New<IBMNCNShape>();
			var approvedShape = Factory.New<IBMNCNShape>();
			approvedShape.Approve(GlbStaff.CurrentUser.GS_Code);

			approvedShape.BNS_RelatedEntityID = workflow1.PK;
			nonApprovedShape.BNS_RelatedEntityID = workflow2.PK;

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				form.SetDataBinding(jobHeader, string.Empty);

				var workflowsControl = control.FindAll<WorkflowsUserControl>().Single();

				AssertEquals("No approved shape exists for the job header", false, control.ApprovedShapeDetailsControl.Visible);

				workflowsControl.WorkflowsGrid.ListManager.Position++;
				AssertEquals("An approved shape exists for workflow1", true, control.ApprovedShapeDetailsControl.Visible);

				workflowsControl.WorkflowsGrid.ListManager.Position++;
				AssertEquals("No approved shape exists for workflow2", false, control.ApprovedShapeDetailsControl.Visible);
			}
		}

		#endregion

		#region Misc

		public void TestControlBindingSetup_ShouldNotCauseDuplicateStatusChangeEvents()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				form.SetDataBinding(jobHeader, string.Empty);

				BMSTestHelper.FillWithValidTestDataSoFormSaveWorks((OrgHeader)jobHeader.Parent);
				var result = form.FireSaveButton();
				AssertSaved(result);
			}

			var jobStatusEvents = ProcessHeaderTest.GetStatusChangeLogs(jobHeader);
			var workflowStatusEvents = ProcessHeaderTest.GetStatusChangeLogs(workflow);

			AssertEquals(1, jobStatusEvents.Length);
			AssertEquals(1, workflowStatusEvents.Length);

			AssertEquals(Events.JobOpenCode, jobStatusEvents[0].SL_SE_NKEvent);
			AssertEquals(Events.JobOpenCode, workflowStatusEvents[0].SL_SE_NKEvent);
		}

		[ExpectNoExceptions]
		public void TestMinimiseWhilstShowingManagementControl_ShouldNotResizeSplitterOutsideValidRange()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var processJobHeader = ProcessJobHeader.GetForParent(dummy, Factory);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowManagementUserControl { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				form.WindowState = FormWindowState.Minimized;
			}
		}

		[ExpectNoExceptions]
		public void TestLoadControl_WhenWorkflowHasNoFH_FH_ParentHeaderSet_ShouldShowErrorMessage_AndNotThrowExceptionsOrReportErrors()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow with missing FK");
			var jobLevelWorkflow = workflow.JobHeader;
			Factory.Save();

			workflow.FH_FH_ParentHeader = ZGuid.Empty;

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowManagementUserControl { Dock = DockStyle.Fill })
			{
				control.SetDataBinding(jobLevelWorkflow, "");
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				AssertEquals("At least one workflow does not have its Parent Header property set. Please ensure that the process that created this job's workflows sets all required properties correctly.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Remove Job Level Workflow links
		public void TestRemoveLinkFromJobLevelWorkflow_DependentWorkflowsMenuItem()
		{
			TestRemoveLinkFromJobLevelWorkflow((a, b) => a.GetOrCreateDependencyLink(b), "Dependent Workflows");

			AssertMultilineASCIIEquals("", @"Would you like to delete this link?

From:	Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.
To:	Organization (MAIORGSYD1) - Workflow2", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRemoveLinkFromJobLevelWorkflow_PreRequisiteWorkflowsMenuItem()
		{
			TestRemoveLinkFromJobLevelWorkflow((a, b) => b.GetOrCreateDependencyLink(a), "Pre-requisite Workflows");

			AssertMultilineASCIIEquals("", @"Would you like to delete this link?

From:	Organization (MAIORGSYD1) - Workflow2
To:	Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRemoveLinkFromJobLevelWorkflow_ParentWorkflowsMenuItem()
		{
			TestRemoveLinkFromJobLevelWorkflow((a, b) => a.GetOrCreateLinkToParent(b), "Parent Workflows");

			AssertMultilineASCIIEquals("", @"Would you like to delete this link?

From:	Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.
To:	Organization (MAIORGSYD1) - Workflow2", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRemoveLinkFromJobLevelWorkflow_ChildWorkflowsMenuItem()
		{
			TestRemoveLinkFromJobLevelWorkflow((a, b) => b.GetOrCreateLinkToParent(a), "Child Workflows");

			AssertMultilineASCIIEquals("", @"Would you like to delete this link?

From:	Organization (MAIORGSYD1) - Workflow2
To:	Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void TestRemoveLinkFromJobLevelWorkflow(Func<ProcessHeader, ProcessHeader, ProcessHeaderLink> createLinkAction, string menuItemName)
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org1 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);

			var org2 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			var jobToWorkflowLink = createLinkAction(jobHeader1, workflow2);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(jobHeader1))
			{
				Application.DoEvents();

				var parentPostingButtons = form as IPostingButtonsProvider;
				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");

				var control = form.FindAll<WorkflowManagementUserControl>().First();

				control.WorkflowsGrid.ListManager.Position = control.WorkflowsGrid.List.IndexOf(jobHeader1);
				Application.DoEvents();

				var workflowsControl = control.FindAll<WorkflowsUserControl>().Single();
				workflowsControl.ContextMenu_Popup(workflowsControl.WorkflowsGrid.ContextMenu, EventArgs.Empty);

				var menuItem = (ZMenuItem)workflowsControl.WorkflowsGrid.ContextMenu.MenuItems.Cast<MenuItem>().Single(m => m.Text == menuItemName);
				menuItem.OnPopup_Exposed();
				var toolStripMenuItem = (ZToolStripMenuItem)MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem);

				var removeMenuItem = (ZToolStripMenuItem)toolStripMenuItem.DropDownItems[2]; // Remove

				removeMenuItem.DropDownItems[0].PerformClick();
				Application.DoEvents();

				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: true, cancelEnabled: true,
					applyText: "&Save", postText: "S&ave && Close", cancelText: "&Cancel");

				form.FireSaveButton();
				AssertEquals(true, jobToWorkflowLink.IsDeleted);
			}
		}
		#endregion

		#region Workflow Relationships Form

		public void TestOpenWorkflowRelationshipsForm()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow1))
			{
				Application.DoEvents();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task1);
				Application.DoEvents();
				form.BusinessEntity.Factory.Save();

				var managementControl = form.FindAll<WorkflowManagementUserControl>().First();
				var button = (ZButton)managementControl.Controls.Find("WorkflowRelationshipsButton", true)[0];
				AssertNotNull(button);

				button.PerformClick();

				AssertEquals("1 WorkflowRelationshipsForm should have been open", 1, Application.OpenForms.OfType<WorkflowRelationshipsForm>().Count());
				Application.OpenForms.OfType<WorkflowRelationshipsForm>().First().Close();

				AssertEquals("0 WorkflowRelationshipsForm should be open", 0, Application.OpenForms.OfType<WorkflowRelationshipsForm>().Count());
			}
		}

		public void TestOpenWorkflowRelationshipsForm_WhenWorkflowDeletedInAnotherSession_ShouldShowErrorMesage()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "That's it ✋🏼✋🏼✋🏼");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var newFactory = Factory.CreateNewFactory();
				newFactory.RefreshEnabled = false;
				newFactory.Load<ProcessHeader>(workflow.PK).Delete();
				newFactory.Save();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				form.FindAndClickButton("WorkflowRelationshipsButton");

				AssertEquals("Cannot show the Workflow Relationship Navigator because this workflow has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWorkflowRelationshipsForm_ModalityDoesntLockSystem()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow1))
			{
				Application.DoEvents();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task1);
				Application.DoEvents();
				form.BusinessEntity.Factory.Save();

				var managementControl = form.FindAll<WorkflowManagementUserControl>().First();
				var button = (ZButton)managementControl.Controls.Find("WorkflowRelationshipsButton", true)[0];
				AssertNotNull(button);

				button.PerformClick();

				using (var relationshipForm = Application.OpenForms.OfType<WorkflowRelationshipsForm>().Single())
				{
					var addPrereqButton = relationshipForm.FindAll<ZButton>().Single(b => b.Name == "addPrerequisiteButton");

					addPrereqButton.PerformClick();

					using (var popup = Application.OpenForms.OfType<EmbeddedModulePopup>().Single())
					{
						var filter = popup.FindAll<ZFilterStripControl>().Single();
						filter.FirePerformSearch();

						filter.FilteredGrid.Select(1);
						var edit = filter.FilteredGrid.ContextMenu.MenuItems.OfType<ZMenuItem>().Single(s => s.Text == "&Edit");

						AssertNoExceptionThrown(() => edit.PerformClick());
					}
				}
			}
		}

		public void TestOpenWorkflowRelationshipsForm_WhenNoWorkflowIsSelected()
		{
			using (var form = new ZForm())
			using (var control = new WorkflowDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var button = (ZButton)control.Controls.Find("WorkflowRelationshipsButton", true)[0];

				AssertEquals(false, button.Enabled);
			}
		}

		public void TestOpenWorkflowRelationshipsForm_WhenWorkflowAddedAndFormSaved_ParentFormSaveButtonShouldNotBeActive()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org1 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);

			var org2 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow1))
			{
				Application.DoEvents();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task1);
				Application.DoEvents();
				form.BusinessEntity.Factory.Save();

				var parentPostingButtons = form as IPostingButtonsProvider;
				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");

				var managementControl = form.FindAll<WorkflowManagementUserControl>().First();
				var button = (ZButton)managementControl.Controls.Find("WorkflowRelationshipsButton", true)[0];
				AssertNotNull(button);

				button.PerformClick();

				using (var relationshipForm = Application.OpenForms.OfType<WorkflowRelationshipsForm>().Single())
				{
					var preReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "prerequisitesGrid");
					var postReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "postrequisitesGrid");

					AssertEquals("No pre-reqs", 0, preReqGrid.ListManager.Count);
					AssertEquals("No post-reqs", 0, postReqGrid.ListManager.Count);

					var addPostReqButton = relationshipForm.FindAll<ZButton>().Single(b => b.Name == "addPostrequisiteButton");
					addPostReqButton.PerformClick();

					using (var popup = Application.OpenForms.OfType<EmbeddedModulePopup>().Single())
					{
						var filter = popup.FindAll<ZFilterStripControl>().Single();
						var strip = filter.AddNewFilterStrip();
						strip.CurrentDataItem.FilterDescription = "Completion Statement";
						((ModuleTextFilter)strip.CurrentDataItem.CurrentModuleFilter).Property = "Workflow2";
						filter.FirePerformSearch();
						filter.FilteredGrid.Select(0);
						popup.ExposedOKButtonForTesting.PerformClick();
					}

					AssertEquals("One post-req", 1, postReqGrid.ListManager.Count);

					var relationshipPostingButtons = relationshipForm as IPostingButtonsProvider;
					ZFormTestHelper.AssertCommandButtons(relationshipPostingButtons,
						applyEnabled: true, postEnabled: true, cancelEnabled: true,
						applyText: "&Save", postText: "S&ave && Close", cancelText: "&Cancel");

					relationshipForm.FireSaveButton();

					AssertNoErrors(((ProcessHeaderLink)(postReqGrid.List[0])).FP_FH_HeaderToInfo);

					ZFormTestHelper.AssertCommandButtons(relationshipPostingButtons,
						applyEnabled: false, postEnabled: false, cancelEnabled: true,
						applyText: "&Save", postText: "S&ave && Close", cancelText: "&Close");
				}

				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");
			}
		}

		public void TestOpenWorkflowRelationshipsForm_WhenWorkflowRemovedAndFormSaved_ParentFormSaveButtonShouldNotBeActive()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org1 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);

			var org2 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow1))
			{
				Application.DoEvents();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task1);
				Application.DoEvents();
				form.BusinessEntity.Factory.Save();

				var parentPostingButtons = form as IPostingButtonsProvider;
				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");

				var managementControl = form.FindAll<WorkflowManagementUserControl>().First();
				var button = (ZButton)managementControl.Controls.Find("WorkflowRelationshipsButton", true)[0];
				AssertNotNull(button);

				button.PerformClick();

				using (var relationshipForm = Application.OpenForms.OfType<WorkflowRelationshipsForm>().Single())
				{
					var preReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "prerequisitesGrid");
					var postReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "postrequisitesGrid");

					AssertEquals("No pre-reqs", 0, preReqGrid.ListManager.Count);
					AssertEquals("One post-req", 1, postReqGrid.ListManager.Count);

					postReqGrid.SelectAllElements();
					Application.DoEvents();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					var removePostReqButton = relationshipForm.FindAll<ZButton>().Single(b => b.Name == "removePostrequisiteButton");
					removePostReqButton.PerformClick();

					AssertEquals("Are you sure you want to remove the selected links?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("No post-reqs", 0, postReqGrid.ListManager.Count);

					var relationshipPostingButtons = relationshipForm as IPostingButtonsProvider;

					ZFormTestHelper.AssertCommandButtons(relationshipPostingButtons,
						applyEnabled: true, postEnabled: true, cancelEnabled: true,
						applyText: "&Save", postText: "S&ave && Close", cancelText: "&Cancel");

					relationshipForm.FireSaveButton();
				}

				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");
			}
		}

		public void TestOpenWorkflowRelationshipsForm_WhenWorkflowRemovedAndFormSaved_ParentFormSaveButtonShouldNotBeActive_WithRelatedAttachment()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var org1 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);

			var org2 = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			var link = workflow2.GetOrCreateLinkToParent(workflow1);
			var shape = Factory.New<IBMNCNShape>();
			var fromShape = Factory.New<IBMNCNShape>();
			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_FP_ProcessHeaderLink = link.PK;
			attachment.BNA_BNS_Owner = attachment.BNA_BNS_ToShape = shape.Identifier;
			attachment.BNA_BNS_FromShape = fromShape.Identifier;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow1))
			{
				Application.DoEvents();
				WorkflowParentFormFactory.NavigateToWorkflowItem(form, task1);
				Application.DoEvents();
				form.BusinessEntity.Factory.Save();

				var parentPostingButtons = form as IPostingButtonsProvider;

				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");

				var managementControl = form.FindAll<WorkflowManagementUserControl>().First();
				var button = (ZButton)managementControl.Controls.Find("WorkflowRelationshipsButton", true)[0];
				AssertNotNull(button);

				button.PerformClick();

				using (var relationshipForm = Application.OpenForms.OfType<WorkflowRelationshipsForm>().Single())
				{
					var parentGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "parentsGrid");
					var childGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "childrenGrid");

					AssertEquals("No parent", 0, parentGrid.ListManager.Count);
					AssertEquals("One child", 1, childGrid.ListManager.Count);

					childGrid.SelectAllElements();
					Application.DoEvents();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					var removePostReqButton = relationshipForm.FindAll<ZButton>().Single(b => b.Name == "removeChildButton");
					removePostReqButton.PerformClick();

					AssertEquals("Are you sure you want to remove the selected links?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("No children", 0, childGrid.ListManager.Count);

					var relationshipPostingButtons = relationshipForm as IPostingButtonsProvider;

					ZFormTestHelper.AssertCommandButtons(relationshipPostingButtons,
						applyEnabled: true, postEnabled: true, cancelEnabled: true,
						applyText: "&Save", postText: "S&ave && Close", cancelText: "&Cancel");

					relationshipForm.FireSaveButton();
				}

				ZFormTestHelper.AssertCommandButtons(parentPostingButtons,
					applyEnabled: true, postEnabled: false, cancelEnabled: true,
					applyText: "&New", postText: "S&ave && Close", cancelText: "&Close");
			}
		}

		public void TestAddingWorkflowPrerequisites_ShouldUpdateWorkflowOpenPrereqsCount()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";

			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();

			AssertEquals("Open", workflow1.FH_StatusDescription);

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task1))
			{
				Application.DoEvents();

				form.BusinessEntity.Factory.Save();

				var managementControl = form.FindAll<WorkflowManagementUserControl>().First();
				var button = (ZButton)managementControl.Controls.Find("WorkflowRelationshipsButton", true).FirstOrDefault();
				AssertNotNull(button);

				var details = managementControl.FindSingle<WorkflowDetailsUserControl>();
				AssertNotNull(details);
				var calcEditControl = details.Controls.Find("OpenPrereqsCalcEdit", true).SingleOrDefault();
				CombineAssertions("Prereq control initial state", () =>
				{
					AssertEquals("0", calcEditControl.Text);
					AssertEquals(SystemColors.Control, calcEditControl.BackColor);
				});

				button.PerformClick();

				using (var relationshipForm = Application.OpenForms.OfType<WorkflowRelationshipsForm>().Single())
				{
					var preReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "prerequisitesGrid");
					var postReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "postrequisitesGrid");

					AssertEquals("No pre-reqs", 0, preReqGrid.ListManager.Count);
					AssertEquals("No post-reqs", 0, postReqGrid.ListManager.Count);

					var addPreReqButton = relationshipForm.FindAll<ZButton>().Single(b => b.Name == "addPrerequisiteButton");
					addPreReqButton.PerformClick();

					using (var popup = Application.OpenForms.OfType<EmbeddedModulePopup>().Single())
					{
						var filter = popup.FindAll<ZFilterStripControl>().Single();
						var strip = filter.AddNewFilterStrip();
						strip.CurrentDataItem.FilterDescription = "Completion Statement";
						((ModuleTextFilter)strip.CurrentDataItem.CurrentModuleFilter).Property = "Workflow2";
						filter.FirePerformSearch();
						filter.FilteredGrid.Select(0);
						popup.ExposedOKButtonForTesting.PerformClick();
					}

					AssertEquals("One pre-req. Seriously, why aren't they called preqs?", 1, preReqGrid.ListManager.Count);
					AssertEquals("No post-reqs. To be sure, to be sure...", 0, postReqGrid.ListManager.Count);

					relationshipForm.FireSaveButton();
					relationshipForm.Close();
				}

				var pinkBackColor = Color.FromArgb(255, 202, 213);
				CombineAssertions("Prereq control changes", () =>
				{
					AssertEquals("Prereq control text should update", "1", calcEditControl.Text);
					AssertEquals("Prereq control background colour should update", pinkBackColor, calcEditControl.BackColor);
				});
			}

			CombineAssertions("List of things we should have", () =>
			{
				AssertEquals("We should have properly detected the prereq we just added, howmst'ever...", 1, workflow1.NumberOfOpenPrerequisitesUpTheTree);
				AssertEquals("We should have made Workflow1 a prereq of Workflow2, instead...", true, workflow1.Prerequisites().Contains(workflow2));
				AssertEquals("We should have made Workflow2 a postreq of Workflow1, alas...", true, workflow2.Postrequisites().Contains(workflow1));
				AssertEquals("We should have a workflow status change, but yet...", "Blocked", workflow1.FH_StatusDescription);
			});
		}
		public void TestRemovingWorkflowPrerequisites_ShouldUpdateWorkflowOpenPrereqsCount()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);

			var org = BMSTestHelper.CreateValidOrgHeader(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Workflow2";

			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			workflow2.MakePrerequisiteOf(workflow1);

			Factory.Save();

			AssertEquals("Blocked", workflow1.FH_StatusDescription);

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task1))
			{
				Application.DoEvents();

				var managementControl = form.FindAll<WorkflowManagementUserControl>().First();
				var button = (ZButton)managementControl.Controls.Find("WorkflowRelationshipsButton", true).FirstOrDefault();
				AssertNotNull(button);

				var details = managementControl.FindSingle<WorkflowDetailsUserControl>();
				AssertNotNull(details);
				var calcEditControl = details.Controls.Find("OpenPrereqsCalcEdit", true).SingleOrDefault();
				var pinkBackColor = Color.FromArgb(255, 202, 213);
				CombineAssertions("Prereq control initial state", () =>
				{
					AssertEquals("1", calcEditControl.Text);
					AssertEquals(pinkBackColor, calcEditControl.BackColor);
				});

				button.PerformClick();

				using (var relationshipForm = Application.OpenForms.OfType<WorkflowRelationshipsForm>().Single())
				{
					var preReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "prerequisitesGrid");
					var postReqGrid = relationshipForm.FindAll<ZGrid>().Single(g => g.Name == "postrequisitesGrid");

					CombineAssertions("List of reqs", () =>
					{
						AssertEquals("One pre-req", 1, preReqGrid.ListManager.Count);
						AssertEquals("No post-reqs", 0, postReqGrid.ListManager.Count);
					});

					preReqGrid.SelectAllElements();
					Application.DoEvents();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

					var removePreReqButton = relationshipForm.FindAll<ZButton>().Single(b => b.Name == "removePrerequisiteButton");
					removePreReqButton.PerformClick();

					AssertEquals("Are you sure you want to remove the selected links?", UnitTestUserNotification.Instance.LastMessage.Text);

					CombineAssertions("Another list of reqs", () =>
					{
						AssertEquals("There is no pre-req. Only Zuul.", 0, preReqGrid.ListManager.Count);
						AssertEquals("No post-reqs. To be sure, to be sure...", 0, postReqGrid.ListManager.Count);
					});

					relationshipForm.FireSaveButton();
					relationshipForm.Close();
				}

				CombineAssertions("Prereq control changes", () =>
				{
					AssertEquals("Prereq control text should update", "0", calcEditControl.Text);
					AssertEquals("Prereq control background colour should update", SystemColors.Control, calcEditControl.BackColor);
				});
			}

			CombineAssertions("List of things we should have", () =>
			{
				AssertEquals("We should have properly detected the absence of the prereq we just removed, howmst'ever...", 0, workflow1.NumberOfOpenPrerequisitesUpTheTree);
				AssertEquals("We should have unmade Workflow1 a prereq of Workflow2, instead...", false, workflow1.Prerequisites().Contains(workflow2));
				AssertEquals("We should have unmade Workflow2 a postreq of Workflow1, alas...", false, workflow2.Postrequisites().Contains(workflow1));
				AssertEquals("We should have a workflow status change, but yet...", "Open", workflow1.FH_StatusDescription);
			});
		}

		public void TestRelationshipNavigatorButton_WhenJobHasChanges_ShouldPromptUserToSaveJobFirst()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow))
			{
				Application.DoEvents();
				AssertEquals(ODisplayMode.Browse, form.DisplayMode);

				var jobLevelWorkflow = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent((SalesEnquiry)form.BusinessEntity, form.BusinessEntity.Factory, addDefaultProcessHeaderIfNone: false);
				BMSTestHelper.CreateWorkflow(jobLevelWorkflow, "Additional workflow");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var button = form.FindSingle<ZButton>("WorkflowRelationshipsButton");
				button.PerformClick();
				Application.DoEvents();

				AssertEquals("You must save this form before trying to edit this workflow's relationships. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);

				using (var relationshipForm = BMSFormTestHelper.GetOpenForms<WorkflowRelationshipsForm>().SingleOrDefault())
				{
					AssertNull("The form shouldn't have opened because the user selected No.", relationshipForm);
				}

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				button.PerformClick();
				Application.DoEvents();

				AssertEquals("You must save this form before trying to edit this workflow's relationships. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);

				using (var relationshipForm = BMSFormTestHelper.GetOpenForms<WorkflowRelationshipsForm>().SingleOrDefault())
				{
					AssertNotNull("The form should have opened because the user selected Yes.", relationshipForm);
					AssertEquals("The parent job wasn't read only so neither should the relationship form be.", ODisplayMode.Browse, relationshipForm.DisplayMode);
					AssertEquals("We should use a read/write factory, so the user can make changes.", nameof(BusinessObjectFactory), relationshipForm.BusinessEntity.Factory.GetType().Name);
				}
			}
		}

		public void TestRelationshipNavigatorButton_WhenJobIsReadOnly_AndJobHasChanges_ShouldPromptUserToReloadJobFirst()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow, isEditAllowed: false))
			{
				Application.DoEvents();
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);

				var button = form.FindSingle<ZButton>("WorkflowRelationshipsButton");
				AssertEquals("Viewing relationships should be allowed when the job is read-only", true, button.Enabled);

				var control = form.FindSingle<WorkflowManagementUserControl>();
				var workflowGrid = control.WorkflowsGrid;
				workflowGrid.Focus();

				var workDescriptionColumnIndex = workflowGrid.Columns.IndexOf(x => x.ColumnName == ProcessHeaderSchema.FH_CompletionStatement.Name);
				workflowGrid.ListManager.AddNew();
				workflowGrid[1, workDescriptionColumnIndex] = new ZString("This shouldn't be possible but there was a loophole that allowed it.");
				Application.DoEvents();

				button.PerformClick();
				Application.DoEvents();

				AssertEquals("Changes have been made to a read-only job. Please reload this job before viewing Workflow Relationships.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (var relationshipForm = BMSFormTestHelper.GetOpenForms<WorkflowRelationshipsForm>().SingleOrDefault())
				{
					AssertNull("The form shouldn't have opened.", relationshipForm);
				}
			}
		}

		public void TestRelationshipForm_WhenOpenedFromReadOnlyJob_ShouldAlsoBeReadOnly()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow, isEditAllowed: false))
			{
				Application.DoEvents();
				AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);

				var button = form.FindSingle<ZButton>("WorkflowRelationshipsButton");
				AssertEquals("Viewing relationships should be allowed when the job is read-only", true, button.Enabled);

				button.PerformClick();
				Application.DoEvents();

				AssertEquals("The job has no changes so no message should have been shown.", null, UnitTestUserNotification.Instance.LastMessage.Text);

				using (var relationshipForm = BMSFormTestHelper.GetOpenForms<WorkflowRelationshipsForm>().SingleOrDefault())
				{
					AssertNotNull("The form should have opened.", relationshipForm);
					AssertEquals("The relationship form should also be read only.", ODisplayMode.ReadOnly, relationshipForm.DisplayMode);
					AssertEquals("What factory are we using?", nameof(WorkflowRelationshipsForm), relationshipForm.BusinessEntity.Factory.NameForDebugging);
					AssertEquals("We should use a read-only factory, just in case.", nameof(ReadOnlyBusinessObjectFactory), relationshipForm.BusinessEntity.Factory.GetType().Name);
				}
			}
		}

		#endregion

		#region WorkflowTaskGrid

		public void TestTaskGrid_SelectedRowDoesntJumpWhenSaving()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new WorkflowManagementUserControl() { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();

				form.SetDataBinding(jobHeader, string.Empty);
				BMSTestHelper.FillWithValidTestDataSoFormSaveWorks((OrgHeader)jobHeader.Parent);
				Application.DoEvents();

				var grid = control.TaskDetailsControl.TasksGrid;

				AssertNotNull(grid.ListManager);
				AssertEquals(3, grid.ListManager.Count);
				AssertEquals(0, grid.CurrentRowIndex);

				var rowRectangle = grid.GetRowNotificationRectangle(1);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rowRectangle.X + 30, rowRectangle.Y + 3, 0) });
				typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, rowRectangle.X + 30, rowRectangle.Y + 3, 0) });
				Application.DoEvents();

				AssertEquals(0, grid.CurrentCell.ColumnNumber);
				AssertEquals(1, grid.CurrentCell.RowNumber);

				((ZTextBoxColumnStyle)grid.TableStyles[0].GridColumnStyles[0]).EditControl.Text = "3";
				Application.DoEvents();
				AssertEquals(1, grid.CurrentRowIndex);

				var result = form.FireSaveButton();
				Application.DoEvents();
				AssertEquals(1, grid.CurrentRowIndex);
				AssertSaved(result);
			}
		}

		public void TestGridEnterWithNoDefaultWorkflow()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>(false);

			// sorted Descending
			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new WorkflowManagementUserControl() { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();
				form.SetDataBinding(jobHeader, string.Empty);
				var viewModel = (WorkflowManagementViewModel)control.BindingSource.DataSource;
				viewModel.AllProcessHeaders.ApplySort(ProcessHeaderSchema.Constants.FH_CompletionStatement, ListSortDirection.Descending);
				Application.DoEvents();

				var bindingList = (IBindingList)control.WorkflowsGrid.ListManager.List;
				AssertEquals("precondition", 0, jobHeader.ProcessHeaders.Count);

				control.TasksGrid_ForTest.Focus();

				// default process header added
				AssertEquals(1, jobHeader.ProcessHeaders.Count);
				AssertEquals(jobHeader.ProcessHeaders[0], viewModel.AllProcessHeaders[0]);
			}

			// sorted Ascending
			jobHeader = CreateJobHeader<OrgHeader>(false);
			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new WorkflowManagementUserControl() { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();
				form.SetDataBinding(jobHeader, string.Empty);
				var viewModel = (WorkflowManagementViewModel)control.BindingSource.DataSource;
				viewModel.AllProcessHeaders.ApplySort(ProcessHeaderSchema.Constants.FH_CompletionStatement, ListSortDirection.Ascending);
				Application.DoEvents();

				var bindingList = (IBindingList)control.WorkflowsGrid.ListManager.List;
				AssertEquals("precondition", 0, jobHeader.ProcessHeaders.Count);

				control.TasksGrid_ForTest.Focus();

				// default process header added
				AssertEquals(1, jobHeader.ProcessHeaders.Count);
				AssertEquals(jobHeader.ProcessHeaders[0], viewModel.AllProcessHeaders[1]);
			}
		}

		public void TestOpenGrid_WhenNoWorkflowPreviouslyPresent_AndListChangedEventsSuspendedForSomeReason_ShouldNotCreateDuplicatedWorkflows()
		{
			var job = BMSTestHelper.CreateJob<SalesEnquiry>(Factory);
			var task1 = BMSTestHelper.CreateTask(job);
			var task2 = BMSTestHelper.CreateTask(job);
			var task3 = BMSTestHelper.CreateTask(job);

			Factory.Save();

			AssertEquals(ZGuid.Empty, task1.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, task2.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, task3.P9_FH_ProcessHeader);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);
			ProcessHeader workflow;

			AssertEquals(0, jobHeader.ProcessHeaders.Count);

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new WorkflowManagementUserControl() { Dock = DockStyle.Fill })
			{
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory)) // A number of things can cause this to happen inside ZArchitecture. Let's just ensure our code doesn't break when this happens.
				{
					form.Controls.Add(control);
					form.Show();
					form.SetDataBinding(jobHeader, string.Empty);

					Application.DoEvents();

					workflow = Factory.LoadTop1<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeader.PK));

					AssertNotNull("Default workflow should have been created", workflow);
					AssertEquals("Job Workflow", workflow.FH_CompletionStatement);

					AssertEquals("ListChanged events are suspended, so the new workflow shouldn't be found in the collection yet", 0, jobHeader.ProcessHeaders.Count);

					AssertEquals(workflow.PK, task1.P9_FH_ProcessHeader);
					AssertEquals(workflow.PK, task2.P9_FH_ProcessHeader);
					AssertEquals(workflow.PK, task3.P9_FH_ProcessHeader);
				}

				AssertEquals(1, jobHeader.ProcessHeaders.Count);

				AssertEquals(workflow.PK, task1.P9_FH_ProcessHeader);
				AssertEquals(workflow.PK, task2.P9_FH_ProcessHeader);
				AssertEquals(workflow.PK, task3.P9_FH_ProcessHeader);
			}
		}

		public void TestTaskGrid_MovedTasksDisappear_WhenFocusReevaluated()
		{
			AssertTaskMovedCorrectlyWhenChosenActionExecuted(false);
		}

		public void TestTaskGrid_MovedTasksDisappear_WhenSaving()
		{
			AssertTaskMovedCorrectlyWhenChosenActionExecuted(true);
		}

		void AssertTaskMovedCorrectlyWhenChosenActionExecuted(bool saveForm)
		{
			SetupWorkflowSamplerBox(out ProcessJobHeader jobHeader, out ProcessHeader workflow1, out ProcessHeader workflow2, out ProcessTask task1, out ProcessTask task2, out ProcessTask task3, out ProcessTask task4);

			Factory.Save();

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(900, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var workflowsGrid = control.WorkflowsGrid;
				workflowsGrid.ListManager.Position = 1; // select workflow1

				var tasksGrid = control.TaskDetailsControl.TasksGrid;
				tasksGrid.ExposeAllColumns();
				tasksGrid.ListManager.Position = 1; // select task1

				var taskGridWorkflowIndex = tasksGrid.Columns.IndexOf(tg => tg.ColumnName == ProcessTasksSchema.P9_FH_ProcessHeader.Name);
				tasksGrid[0, taskGridWorkflowIndex] = workflow2.PK; // change the associated workflow of task1 to be 'workflow2'

				if (saveForm)
				{
					form.FireSaveButton();
				}
				else
				{
					Application.DoEvents();
				}

				var taskGridDescriptionIndex = tasksGrid.Columns.IndexOf(tg => tg.ColumnName == ProcessTasksSchema.P9_Description.Name);
				AssertEquals("By changing task1 to be associated with workflow2, the taskgrid of workflow1 should only have task2 inside it, but instead...", tasksGrid[0, taskGridDescriptionIndex], task2.DescriptionWithReference);
				AssertEquals("By changing task1 to be associated with workflow2, the taskgrid of workflow1 should only have task2 inside it, but instead...", tasksGrid[1, taskGridDescriptionIndex], ZString.Empty);

				workflowsGrid.ListManager.Position = 2;
				Application.DoEvents(); // let the program catch up on us selecting a new workflow in the workflow grid

				tasksGrid.ListManager.Position = 1; // select task1 again, as it has the smollest task sequence

				AssertEquals("By changing task1 to be associated with workflow2, the taskgrid of workflow2 now have task1 inside it, but instead...", tasksGrid[0, taskGridDescriptionIndex], (task1.DescriptionWithReference));
			}
		}

		public void TestTaskGrid_WhenThereAreTasksInOtherCompanies_ProcessTypeUsesCompanySpecificTasks_ShouldShowHintLabel()
		{
			var startingBranch = GlbBranch.GetCurrentBranch(Factory);
			var otherCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			var otherCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch1 = otherCompany1.Branches.AddNew();
			var otherBranch2 = otherCompany2.Branches.AddNew();

			otherBranch1.FillWithValidTestData();
			otherBranch2.FillWithValidTestData();

			Factory.Save();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, DummyWorkflowDescriptor.Instance.Code);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.Z0_Description = "Wheremy?";

			var jobHeader = BMSTestHelper.CreateJobHeader(job);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Theremy!");
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			var task3 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			task1.P9_GC = Env.CurrentCompanyPK;
			task2.P9_GC = otherCompany1.PK;
			task3.P9_GC = otherCompany2.PK;

			// Other workflow item types shouldn't cause false-positives.
			job.WorkflowItems.Triggers.AddNew();
			job.WorkflowItems.Milestones.AddNew();
			job.WorkflowItems.Exceptions.AddNew();

			Factory.Save();

			AssertEquals(true, job.IsInDatabase);
			AssertNotEquals(task1.P9_TaskID, task2.P9_TaskID);
			AssertEquals(false, DummyWorkflowDescriptor.Instance.AreTasksCompanySpecific);

			ShowWorkflowManagementControlAndAssertTasksShown("Should only show all tasks in the job since we're not filtering by company", startingBranch, null, workflow.JobHeader, false, task1, task2, task3);
			ShowWorkflowManagementControlAndAssertTasksShown("Should only show all tasks in the job since we're not filtering by company", otherBranch1, null, workflow.JobHeader, false, task1, task2, task3);
			ShowWorkflowManagementControlAndAssertTasksShown("Should only show all tasks in the job since we're not filtering by company", otherBranch2, null, workflow.JobHeader, false, task1, task2, task3);

			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);

			ShowWorkflowManagementControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list all companies which have tasks", startingBranch, new[] { otherCompany1, otherCompany2 }, workflow.JobHeader, true, task1);
			ShowWorkflowManagementControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list all companies which have tasks", otherBranch1, new[] { startingBranch.Company, otherCompany2 }, workflow.JobHeader, true, task2);
			ShowWorkflowManagementControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list all companies which have tasks", otherBranch2, new[] { startingBranch.Company, otherCompany1 }, workflow.JobHeader, true, task3);

			task3.P9_GC = task2.P9_GC;
			Factory.Save();

			ShowWorkflowManagementControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list the single company which has a task", startingBranch, new[] { otherCompany1 }, workflow.JobHeader, true, task1);
			ShowWorkflowManagementControlAndAssertTasksShown("Should only show task that is part of the currently logged-in company, and list the single company which has a task", otherBranch1, new[] { startingBranch.Company }, workflow.JobHeader, true, task2, task3);
			ShowWorkflowManagementControlAndAssertTasksShown("Should show no tasks since all tasks are part of another company", otherBranch2, new[] { startingBranch.Company, otherCompany1 }, workflow.JobHeader, true);
		}

		#region Add Assistance Tasks For Menu Item

		public void TestAddAssistanceTaskForMenuItem_ShouldBeVisible_WhenLoggedInUserAndTaskAssignedUserAreTheSame()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" }, { "QCB", "Barrier" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");

			var currentUser = Env.CurrentUser.Initials;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, currentUser, 60, "INV", sequence: 4, description: "Just a little task, y'know", estVariationFactor: 5);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Just a little task, y'know", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();

				var assistMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().SingleOrDefault();
				AssertNull("One cannot render assistance unto oneself.", assistMenuItem);

				var addForMenuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("Although, one should be able to seek assistance from others for their task at all times.", addForMenuItem);

				var staffSubMenu = addForMenuItem.MenuItems.OfType<AddAssistanceTaskForStaffSubMenu>().SingleOrDefault();
				AssertNotNull("The conditions should be right for the Staff sub menu item to appear.", staffSubMenu);

				var capabilitySubMenu = addForMenuItem.MenuItems.OfType<AddAssistanceTaskForCapabilitySubMenu>().SingleOrDefault();
				AssertNotNull("The conditions should be right for the Capabilities sub menu item to appear.", capabilitySubMenu);
			}
		}

		public void TestAddAssistanceTaskForMenuItem_StaffSubMenu_OnClick()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" }, { "QCB", "Barrier" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");

			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var anotherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4, description: "Other user's task", estVariationFactor: 5);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser, 10, "QCB", sequence: 5, description: "Review", estVariationFactor: 2);
			var anotherTask = BMSTestHelper.CreateTask(workflow, anotherUser, 10, "INV", sequence: 6, description: "Another task", estVariationFactor: 2);

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2);
			var task3 = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Other user's task");
			var task4 = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Review");

			AssertEquals(6, task3.P9_Sequence);

			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task3))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForStaffSubMenu>().SingleOrDefault();
				staffSubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = staffSubMenu.MenuItems.OfType<AddAssistanceTaskForStaffMenuItem>().SingleOrDefault(m => m.Caption.ToString().Contains(anotherUser));
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("A new task should be added.", 4, grid.VisibleRowCount);

				var assistTask = grid.List.Cast<ProcessTask>().SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(task3.P9_ParentID, assistTask.P9_ParentID);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(6, assistTask.P9_Sequence);
				AssertEquals(anotherUser, assistTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertEquals("Workflow (Quality Iteration 1)", assistTask.ProcessHeader.FH_CompletionStatement);
				AssertEquals("The assist task should be in the same iteration as the selected task (if any).", task3.IterationPivot.Iteration.PK, assistTask.IterationPivot?.Iteration.PK);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);

				grid.ContextMenu.DoPopup();
				menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				staffSubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForStaffSubMenu>().SingleOrDefault();
				staffSubMenu.ShowPopupMenu();

				Application.DoEvents();

				addAssistanceMenuItem = staffSubMenu.MenuItems.OfType<AddAssistanceTaskForStaffMenuItem>().SingleOrDefault(m => m.Caption.ToString().Contains(anotherUser));
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("An appropriate assistance task for this user already exists.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestAddAssistanceTaskForMenuItem_StaffSubMenu_OnClick_ShouldCreateTaskEvenWhenACapabilityAssistTaskExists()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" }, { "QCB", "Barrier" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var anotherUser = Factory.NewWithValidTestData<GlbStaff>();
			var currentUser = Env.CurrentUser.Initials;

			var capability = BMSTestHelper.CreateCapability(Factory, "CAP");
			var pivot = anotherUser.CapabilityPivots.AddNew();
			pivot.G5_G4_Capability = capability.PK;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4, description: "Other user's task", estVariationFactor: 5, capability: capability);
			var existingAssistTask = BMSTestHelper.CreateTask(workflow, string.Empty, 10, "AST", sequence: 4, description: "Assist", estVariationFactor: 2, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser, 10, "QCB", sequence: 5, description: "Review", estVariationFactor: 2, capability: capability);
			var anotherTask = BMSTestHelper.CreateTask(workflow, anotherUser.GS_Code, 10, "INV", sequence: 6, description: "Another task", estVariationFactor: 2);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task1))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var staffSubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForStaffSubMenu>().SingleOrDefault();
				staffSubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = staffSubMenu.MenuItems.OfType<AddAssistanceTaskForStaffMenuItem>().SingleOrDefault(m => m.Caption.ToString().Contains(anotherUser.GS_Code));
				addAssistanceMenuItem.PerformClick();

				Application.DoEvents();

				AssertEquals("A new task should be added.", 6, grid.VisibleRowCount);

				var assistTask = grid.List.Cast<ProcessTask>().SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(task1.P9_ParentID, assistTask.P9_ParentID);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(4, assistTask.P9_Sequence);
				AssertEquals(ZGuid.Empty, assistTask.P9_G4_RequiredCapability);
				AssertEquals(anotherUser.GS_Code, assistTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertEquals("Workflow", assistTask.ProcessHeader.FH_CompletionStatement);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddAssistanceTaskForMenuItem_CapabilitySubMenu_OnClick()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" }, { "QCB", "Barrier" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var capability = BMSTestHelper.CreateCapability(Factory, "CAP");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4, description: "Other user's task", estVariationFactor: 5, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser, 10, "QCB", sequence: 5, description: "Review", estVariationFactor: 2, capability: capability);

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2);
			var task3 = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Other user's task");
			task3.P9_G4_RequiredCapability = capability.PK;
			var task4 = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Review");
			task4.P9_G4_RequiredCapability = capability.PK;

			AssertEquals(6, task3.P9_Sequence);

			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task3))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				var capabilitySubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForCapabilitySubMenu>().SingleOrDefault();
				capabilitySubMenu.ShowPopupMenu();

				Application.DoEvents();

				var addAssistanceMenuItem = capabilitySubMenu.MenuItems.OfType<AddAssistanceTaskForCapabilityMenuItem>().SingleOrDefault();
				addAssistanceMenuItem.PerformClick();

				AssertEquals("A new task should be added.", 4, grid.VisibleRowCount);

				var assistTask = grid.List.Cast<ProcessTask>().SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(task3.P9_ParentID, assistTask.P9_ParentID);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(6, assistTask.P9_Sequence);
				AssertEquals(capability.PK, assistTask.P9_G4_RequiredCapability);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertEquals("Workflow (Quality Iteration 1)", assistTask.ProcessHeader.FH_CompletionStatement);
				AssertEquals("The assist task should be in the same iteration as the selected task (if any).", task3.IterationPivot.Iteration.PK, assistTask.IterationPivot?.Iteration.PK);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);

				grid.ContextMenu.DoPopup();
				menuItem = grid.ContextMenu.MenuItems.OfType<AddAssistanceTaskForMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				capabilitySubMenu = menuItem.MenuItems.OfType<AddAssistanceTaskForCapabilitySubMenu>().SingleOrDefault();
				capabilitySubMenu.ShowPopupMenu();

				Application.DoEvents();

				addAssistanceMenuItem = capabilitySubMenu.MenuItems.OfType<AddAssistanceTaskForCapabilityMenuItem>().SingleOrDefault();
				addAssistanceMenuItem.PerformClick();

				AssertEquals("An appropriate assistance task for this capability already exists.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		#endregion

		#region Assist With This Task

		public void TestAssistWithThisTaskMenuItem_OnClick()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" }, { "QCB", "Barrier" } });
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4, description: "Other user's task", estVariationFactor: 5);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser, 10, "QCB", sequence: 5, description: "Review", estVariationFactor: 2);

			var iterationWorkflow = BMSTestHelper.CreateQualityIteration(task1, task2);
			var task3 = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Other user's task");
			var task4 = iterationWorkflow.Tasks.Single(x => x.P9_Description == "Review");

			AssertEquals(6, task3.P9_Sequence);

			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task3))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().SingleOrDefault();
				AssertNotNull("The conditions should be right for this menu item to appear.", menuItem);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("A new task should be added.", 4, grid.VisibleRowCount);

				var assistTask = grid.List.Cast<ProcessTask>().SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(assistTask);

				AssertEquals(task3.P9_ParentID, assistTask.P9_ParentID);
				AssertEquals("OH", assistTask.P9_ParentTableCode);
				AssertEquals("AST", assistTask.P9_Type);
				AssertEquals(20, (ZInt)assistTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(4m, assistTask.P9_EstimateVariationFactor);
				AssertEquals(6, assistTask.P9_Sequence);
				AssertEquals(currentUser, assistTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, assistTask.P9_Status);
				AssertEquals("Assist", assistTask.P9_Description);
				AssertEquals("Workflow (Quality Iteration 1)", assistTask.ProcessHeader.FH_CompletionStatement);
				AssertEquals("The assist task should be in the same iteration as the selected task (if any).", task3.IterationPivot.Iteration.PK, assistTask.IterationPivot?.Iteration.PK);
				AssertEquals("We should let the user save the form when they're ready.", false, assistTask.IsInDatabase);
			}
		}

		public void TestAssistWithThisTaskMenuItem_WhenWorkflowTypeNotConfiguredInRegistry_ShouldNotAppearInMenu()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.AddTaskTypesToRegistry("WKI", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("WKI", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4, description: "Other user's task", estVariationFactor: 5);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var hasMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				AssertEquals("Assist With This Task isn't set up in the registry for the ORG workflow type, so this menu item shouldn't appear for this task.", false, hasMenuItem);
			}
		}

		public void TestAssistWithThisTaskMenuItem_ShouldNotAppearOnRowsForTasksAssignedToTheCurrentUser()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4, description: "Other user's task", estVariationFactor: 5);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser, 10, "QCB", sequence: 5, description: "Task assigned to me", estVariationFactor: 2);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task2))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Task assigned to me", selectedTask.P9_Description);

				grid.ContextMenu.DoPopup();
				var hasMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				AssertEquals("Assist With This Task shouldn't appear on tasks that are assigned to the current user, despite the often-quoted mantra of 'How can you help others if you can't even help yourself?'", false, hasMenuItem);
			}
		}

		public void TestAssistWithThisTaskMenuItem_ShouldNotAppearWhenMultipleRowsAreSelected()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);
			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 4, description: "Other user's task", estVariationFactor: 5);
			var task2 = BMSTestHelper.CreateTask(workflow, currentUser, 10, "QCB", sequence: 5, description: "Task assigned to me", estVariationFactor: 2);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task1))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				grid.SelectAllElements(x => x != null);
				AssertEquals(2, grid.SelectedRowCount);

				grid.ContextMenu.DoPopup();
				var hasMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				AssertEquals("Assist With This Task doesn't make sense when multiple rows are selected. It says 'Assist with this TASK'... we're only allowed to have one!", false, hasMenuItem);
			}
		}

		public void TestAssistWithThisTaskMenuItem_ShouldNotAppearWhenActualTaskIsNotSelected()
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", "INV", "AST");
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", 20, 4);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(orgHeader);

			using (var form = (ZOrganisationsForm)controller.ShowEditForm(orgHeader))
			{
				Application.DoEvents();

				var tabPage = form.FindSingle<ZWorkflowTabPage>();
				var zTabControl = (ZTabControl)tabPage.Parent;
				zTabControl.SelectedTab = tabPage;
				Application.DoEvents();

				var managementControl = form.FindSingle<WorkflowManagementUserControl>();
				var taskControl = managementControl.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				AssertNull("Precondition: The new row is selected, not an actual task", grid.GetCurrent());

				grid.ContextMenu.DoPopup();
				var hasMenuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().Any();
				AssertEquals("Assist With This Task can obviously only be used on a real task.", false, hasMenuItem);
			}
		}

		#region Avoid Duplication of Assistance Tasks When Using Assist With This Task

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusWRK_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "WRK";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusSUS_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "SUS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusASN_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}
		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithStatusOPN_AndSameSequenceNumber()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "OPN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsClosed()
		{
			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "CLS";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsCancelled()
		{
			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "CAN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_WithDifferentSequenceNumberButStartable()
		{
			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 1;
				Assert("Precondition", assistanceTask.P9_Sequence < sourceTask.P9_Sequence);
				Assert("Precondition", assistanceTask.IsStartable());
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsNotStartable_AndHasDifferentSequenceNumber()
		{
			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				assistanceTask.P9_GS_NKAssignedStaffMember = Env.CurrentUser.Initials;
				assistanceTask.P9_Status = "ASN";
				assistanceTask.P9_Sequence = 100;
				Assert("Precondition", assistanceTask.P9_Sequence > sourceTask.P9_Sequence);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGlobalCapabilityCurrentUserPossesses()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: false);
			capability.ResourcesWithCapability.Add(currentUser);

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			Assert("Precondition", !group.Staff.Select(s => s.PK).Contains(currentUser.PK));

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGlobalCapabilityCurrentUserDoesNotPossess()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: false);
			Assert("Precondition", !capability.ResourcesWithCapability.Select(s => s.PK).Contains(currentUser.PK));

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			group.Staff.Add(currentUser);

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", workflow.FH_GG_ReleaseGroup.IsEmpty);

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGroupCapabilityCurrentUserPossesses_WhenTaskIsAssignedToUserGroup()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			group.Staff.Add(currentUser);

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGroupCapabilityCurrentUserPossesses_ButTaskGroupDoesNotContainCurrentUser()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			Assert("Precondition", !group.Staff.Select(s => s.PK).Contains(currentUser.PK));

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGroupCapabilityCurrentUserPossesses_WhenWorkflowIsAssignedToUserReleaseGroup()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "RG1", "Release group");
			releaseGroup.Staff.Add(currentUser);

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				Assert("Precondition", assistanceTask.P9_GG_AssignedGroup.IsEmpty);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGroupCapabilityCurrentUserPossesses_ButWorkflowReleaseGroupDoesNotContainCurrentUser()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "RG1", "Release group");
			Assert("Precondition", !releaseGroup.Staff.Select(s => s.PK).Contains(currentUser.PK));

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				workflow.FH_GG_ReleaseGroup = releaseGroup.PK;

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				Assert("Precondition", assistanceTask.P9_GG_AssignedGroup.IsEmpty);
			});
		}

		public void TestAssistWithThisTask_ShouldNotDuplicateExistingAssistanceTask_AssignedToGroupCapabilityCurrentUserPossesses_WhenThereIsNeitherTaskNorReleaseGroup()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			capability.ResourcesWithCapability.Add(currentUser);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "RG1", "Release group");
			releaseGroup.Staff.Add(currentUser);

			AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", workflow.FH_GG_ReleaseGroup.IsEmpty);

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				Assert("Precondition", assistanceTask.P9_GG_AssignedGroup.IsEmpty);
			});
		}

		public void TestAssistWithThisTask_ShouldCreateNewAssistanceTask_WhenExistingAssistanceTaskIsAssignedToGroupCapabilityCurrentUserDoesNotPossess()
		{
			var currentUser = (GlbStaff)Env.CurrentUser;

			var capability = BMSTestHelper.CreateCapability(Factory, "CA1", "Capability1", isGroupScope: true);
			Assert("Precondition", !capability.ResourcesWithCapability.Select(s => s.PK).Contains(currentUser.PK));

			var group = BMSTestHelper.CreateGroup(Factory, "GR1", "Task group");
			group.Staff.Add(currentUser);

			AssertAssistWithThisTask_CreatesNewAssistanceTask((workflow, sourceTask, assistanceTask) =>
			{
				Assert("Precondition", workflow.FH_GG_ReleaseGroup.IsEmpty);

				Assert("Precondition", assistanceTask.P9_GS_NKAssignedStaffMember.IsEmpty);
				assistanceTask.P9_Status = "ASN";
				AssertEquals("Precondition", sourceTask.P9_Sequence, assistanceTask.P9_Sequence);

				assistanceTask.P9_G4_RequiredCapability = capability.PK;
				assistanceTask.P9_GG_AssignedGroup = group.PK;
			});
		}

		void AssertAssistWithThisTask_DoesNotDuplicateExistingAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			var assistanceTaskInitialSequence = assistanceTask.P9_Sequence;

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(sourceTask))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				AssertEquals("Precondition: should be two tasks in the grid", 2, grid.List.Count);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().SingleOrDefault();
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("The number of tasks should stay the same.", 2, grid.List.Count);

				var existingTask = grid.List.Cast<ProcessTask>().SingleOrDefault(x => x.P9_Description == "Existing assistance task");
				AssertNotNull(existingTask);

				AssertEquals(sourceTask.P9_ParentID, existingTask.P9_ParentID);
				AssertEquals("OH", existingTask.P9_ParentTableCode);
				AssertEquals("AST", existingTask.P9_Type);
				AssertEquals(15, (ZInt)existingTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals(3m, existingTask.P9_EstimateVariationFactor);
				AssertEquals(assistanceTaskInitialSequence, existingTask.P9_Sequence);
				AssertEquals(currentUser, existingTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, existingTask.P9_Status);
				AssertEquals("Existing assistance task", existingTask.P9_Description);
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void AssertAssistWithThisTask_CreatesNewAssistanceTask(Action<ProcessHeader, ProcessTask, ProcessTask> assistanceTaskSetup)
		{
			ZFormModaliser.ShowDialogsInTest = true;
			MasterFilesTestHelper.AddTaskTypesToRegistry("ORG", new Dictionary<string, string> { { "INV", "Investigation" }, { "AST", "Assist" } });
			const int defaultAssistanceTaskEstimate = 20;
			const int defaultAssistanceTaskVariationFactor = 4;
			MasterFilesTestHelper.SetUpAssistWithThisTaskInRegistry("ORG", "AST", defaultAssistanceTaskEstimate, defaultAssistanceTaskVariationFactor);

			var otherUser = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var currentUser = Env.CurrentUser.Initials;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var sourceTask = BMSTestHelper.CreateTask(workflow, otherUser, 60, "INV", sequence: 10, description: "Other user's task", estVariationFactor: 5);

			var assistanceTask = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 15, taskType: "AST", sequence: 10, description: "Existing assistance task", estVariationFactor: 3);
			AssertNotEquals("Precondition", defaultAssistanceTaskEstimate, (int)assistanceTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
			AssertNotEquals("Precondition", (ZDecimal)defaultAssistanceTaskVariationFactor, assistanceTask.P9_EstimateVariationFactor);

			assistanceTaskSetup.Invoke(workflow, sourceTask, assistanceTask);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(sourceTask))
			{
				Application.DoEvents();

				var taskControl = form.FindSingle<TaskDetailsUserControl>();
				var grid = taskControl.TasksGrid;
				var selectedTask = (ProcessTask)grid.GetCurrent();
				AssertEquals("Other user's task", selectedTask.P9_Description);

				AssertEquals("Precondition: should be two tasks in the grid", 2, grid.List.Count);

				grid.ContextMenu.DoPopup();
				var menuItem = grid.ContextMenu.MenuItems.OfType<AssistWithThisTaskMenuItem>().SingleOrDefault();
				AssertNotNull(menuItem);

				menuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("A new task should be added.", 3, grid.List.Count);

				var newTask = grid.List.Cast<ProcessTask>().SingleOrDefault(x => !x.IsInDatabase);
				AssertNotNull(newTask);

				AssertEquals(sourceTask.P9_ParentID, newTask.P9_ParentID);
				AssertEquals("OH", newTask.P9_ParentTableCode);
				AssertEquals("AST", newTask.P9_Type);
				AssertEquals(defaultAssistanceTaskEstimate, (int)newTask.P9_EstDuration.GetMinutesFromDateTimeSpan());
				AssertEquals((ZDecimal)defaultAssistanceTaskVariationFactor, newTask.P9_EstimateVariationFactor);
				AssertEquals(10, newTask.P9_Sequence);
				AssertEquals(currentUser, newTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Working, newTask.P9_Status);
				AssertEquals("Assist", newTask.P9_Description);
			}

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#endregion

		#endregion

		#region WorkflowDetailsAndSchedulePanel

		public void TestWorkflowDetailsAndSchedulePanel_ShowsScrollBar()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader = CreateJobHeader<OrgHeader>();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new WorkflowManagementUserControl() { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				ControlDpiScalingHelper.SetHeight(form, 2000, true);
				Application.DoEvents();
				AssertEquals("Height is greater than 800, therefore scroll bar should not be visible", false, control.WorkflowDetailsAndSchedulePanel.VerticalScroll.Visible);

				ControlDpiScalingHelper.SetHeight(form, 500, true);
				Application.DoEvents();
				AssertEquals("Height is less than 800, therefore scroll bar should be visible", true, control.WorkflowDetailsAndSchedulePanel.VerticalScroll.Visible);
			}
		}

		public void TestSystemNameFindBoxChanges_WhenBMSystemChanges()
		{
			var system = CreateSystem();
			system.FS_Name = "Bolag";
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			BMSTestHelper.LinkComponents(bucket, bucket2);

			var templateSystem = CreateSystem("ORG");
			templateSystem.FS_Name = "Beer";
			var bucketTemplate = BMSTestHelper.CreateBucket(templateSystem, "bucketTemplate");
			var bucketTemplate2 = BMSTestHelper.CreateBucket(templateSystem, "bucketTemplate2");
			BMSTestHelper.LinkComponents(bucketTemplate, bucketTemplate2);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Grain";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			template.P0_FS_BufferManagementSystem = templateSystem.PK;

			Factory.Save();

			AssertEquals(template.PK, templateWorkflow.FH_P0_Template);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.ApplyWorkflowTemplates();
			AssertEquals("Workflow was applied to the job", 1, job.WorkflowItems.Count);

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new WorkflowManagementUserControl() { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				var workflowsGrid = control.WorkflowsGrid;
				workflowsGrid.ListManager.Position = 0;
				Application.DoEvents();

				AssertEquals("Should show system created from template", "BEER", control.WorkflowDetailsControl.SystemNameFindBox.Text);

				workflowsGrid.ListManager.Position = 1;
				Application.DoEvents();

				AssertEquals("BEER", control.WorkflowDetailsControl.SystemNameFindBox.Text);
			}

			workflow.CurrentComponentSystemPK = system.PK;
			Factory.Save();

			using (var form = new ZForm { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var control = new WorkflowManagementUserControl() { Dock = DockStyle.Fill })
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(jobHeader, string.Empty);

				var workflowsGrid = control.WorkflowsGrid;
				workflowsGrid.ListManager.Position = 0;
				Application.DoEvents();

				AssertEquals("Should show system created from template", "BEER", control.WorkflowDetailsControl.SystemNameFindBox.Text);

				workflowsGrid.ListManager.Position = 1;
				Application.DoEvents();

				AssertEquals("Should show new system", "BOLAG", control.WorkflowDetailsControl.SystemNameFindBox.Text);
			}
		}

		#endregion

		#region Date Acceptability

		public void TestChangeDateAcceptability_ShouldUpdateGraphic()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Mariam Kassir");
			var task = BMSTestHelper.CreateTask(workflow);

			jobHeader.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.SharpStartExtendedFinish;

			Factory.Save();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var form = new ZForm(viewModel) { ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = control.WorkflowsGrid;

				AssertEquals(0, grid.ListManager.Position);

				var dateAcceptabilityControl = control.FindAll<DateAcceptabilityControl>().Single();
				var pictureBox = dateAcceptabilityControl.FindAll<PictureBox>().Single();
				var selectedImage = pictureBox.Image;

				AssertNotNull(selectedImage);

				grid.ListManager.Position++;

				AssertNotNull(pictureBox.Image);
				AssertNotEquals(pictureBox.Image, selectedImage);
			}
		}

		#endregion

		#region Prereqs List

		public void TestPrereqsListButton_WhenNoSecurity_ShouldShowError()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			using (var form = new ZFormForPrereqsListTest(jobHeader))
			{
				form.Show();

				Env.Security.WorkflowDependencies.IsAllowed = false;

				form.FindAndClickButton("PrereqListButton");

				AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Workflow & Process -> Job Workflows -> Workflow Dependencies", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrereqsListButton_WhenWorkflowHasNoPrereqs_ShouldShowMessage()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Slack Almond");

			Factory.Save();

			using (var form = new ZFormForPrereqsListTest(jobHeader))
			{
				form.Show();

				form.WorkflowsGrid.ListManager.Position = 1;
				AssertEquals(workflow, form.WorkflowsGrid.ListManager.GetCurrent());
				form.FindAndClickButton("PrereqListButton");

				AssertEquals("The selected workflow has no prerequisites.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPrereqsListButton_WhenHasChanges_ShouldShowMessage()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Slack Almond", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Enthusiastic Pecan", releaseGroupPK: config.ReleaseGroup.PK);

			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);

			workflow1.GetOrCreateDependencyLink(workflow2);

			using (var form = new ZFormForPrereqsListTest(jobHeader))
			{
				form.Show();

				AssertEquals(true, jobHeader.HasChanges);
				AssertNoErrors((BusinessObject)form.BusinessEntity);

				form.WorkflowsGrid.ListManager.Position = 2;
				AssertEquals(workflow2, form.WorkflowsGrid.ListManager.GetCurrent());
				form.FindAndClickButton("PrereqListButton");

				AssertEquals("Please save the form before viewing prerequisites. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, jobHeader.HasChanges);
			}
		}

		public void TestPrereqsListButton_WhenHasChanges_ShouldShowMessage_AnsweringNo()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Slack Almond");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Enthusiastic Pecan");

			BMSTestHelper.CreateTask(workflow1);

			workflow1.GetOrCreateDependencyLink(workflow2);

			using (var form = new ZFormForPrereqsListTest(jobHeader))
			{
				form.Show();

				AssertEquals(true, jobHeader.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.WorkflowsGrid.ListManager.Position = 2;
				AssertEquals(workflow2, form.WorkflowsGrid.ListManager.GetCurrent());
				form.FindAndClickButton("PrereqListButton");

				AssertEquals("Please save the form before viewing prerequisites. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, jobHeader.HasChanges);
			}
		}

		public void TestPrereqsListButton_WhenWorkflowHasPrereqs_ShouldShowModulePopup()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Slack Almond");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Enthusiastic Pecan");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Night cheese");

			BMSTestHelper.CreateTask(workflow1);

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			Factory.Save();

			using (var form = new ZFormForPrereqsListTest(jobHeader))
			{
				form.Show();

				form.WorkflowsGrid.ListManager.Position = form.WorkflowsGrid.List.IndexOf(workflow3);
				AssertEquals(workflow3, form.WorkflowsGrid.ListManager.GetCurrent());

				var shownLinks = ShowPrereqsListAndReturnLinksShown(form.Control);

				AssertNotNull(shownLinks.FirstOrDefault(l => l.PK == link1_2.PK));
				AssertNotNull(shownLinks.FirstOrDefault(l => l.PK == link2_3.PK));
			}
		}

		public void TestPrereqsListButton_WhenWorkflowHasPrereqs_ShouldShowModulePopup_ShouldIncludeAllUpstreamPrereqs()
		{
			ZFormModaliser.ShowDialogsInTest = true;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Ein");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Zwei");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Drei");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Vier");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "Fünf");
			var workflow6 = BMSTestHelper.CreateWorkflow(jobHeader, "Sechs");

			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2);
			BMSTestHelper.CreateTask(workflow3);
			BMSTestHelper.CreateTask(workflow4);
			BMSTestHelper.CreateTask(workflow5);
			BMSTestHelper.CreateTask(workflow6);

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var link3_4 = workflow3.GetOrCreateDependencyLink(workflow4);
			var link5_6 = workflow5.GetOrCreateDependencyLink(workflow6);

			// 1 -> 2 -> 3 -> 4
			// 5 -> 6

			Factory.Save();

			using (var form = new ZFormForPrereqsListTest(jobHeader))
			{
				form.Show();

				form.WorkflowsGrid.ListManager.Position = form.WorkflowsGrid.ListManager.List.IndexOf(workflow4);
				AssertEquals(workflow4, form.WorkflowsGrid.ListManager.GetCurrent());

				var shownLinks = ShowPrereqsListAndReturnLinksShown(form.Control);

				AssertEquals("Should show all the links in an upstream direction from workflow4", 3, shownLinks.Length);

				AssertNotNull(shownLinks.FirstOrDefault(l => l.PK == link1_2.PK));
				AssertNotNull(shownLinks.FirstOrDefault(l => l.PK == link2_3.PK));
				AssertNotNull(shownLinks.FirstOrDefault(l => l.PK == link3_4.PK));
			}
		}

		static ProcessHeaderLink[] ShowPrereqsListAndReturnLinksShown(WorkflowManagementUserControl control)
		{
			var result = Array.Empty<ProcessHeaderLink>();

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				var popup = (EmbeddedModulePopup)form;
				popup.Shown += (s, e) =>
				{
					result = popup.Module_ForTest.GridCollection.Cast<ProcessHeaderLink>().ToArray();
				};
			});

			control.FindAndClickButton("PrereqListButton");

			return result;
		}

		class ZFormForPrereqsListTest : ZForm
		{
			internal ZFormForPrereqsListTest(ProcessJobHeader jobLevelWorkflow)
				: base(jobLevelWorkflow)
			{
				ControllerID = DummyControllerIDs.Dummy;
				Size = ControlDpiScalingHelper.NewScaledSize(600, 400);

				Control = new WorkflowManagementUserControl();
				Controls.Add(Control);
			}

			internal WorkflowManagementUserControl Control { get; }

			internal ZGrid WorkflowsGrid => Control.WorkflowsGrid;
		}

		#endregion

		#region Tasks

		static void ShowWorkflowManagementControlAndAssertTasksShown(string message, GlbBranch branchToLogInto, GlbCompany[] otherCompanies, ProcessJobHeader jobHeader, bool areSomeTasksHidden, params ProcessTask[] expectedTasksToBeShown)
		{
			var factory = jobHeader.Factory.CreateNewFactory();
			jobHeader = factory.Load<ProcessJobHeader>(jobHeader.PK);

			AssertNotNull(jobHeader.Parent);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchToLogInto.PK.ToGuid(), Env.CurrentDepartmentPK))
			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy, Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = new WorkflowManagementUserControl())
			{
				control.SetDataBinding(jobHeader, string.Empty);
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder(message, GetTaskIDs(expectedTasksToBeShown), GetTaskIDs(control.TasksGrid_ForTest.List));

				var splitContainer = control.WorkflowTasksTabPage.FindSingle<KSplitContainer>(c => c.Name == "TasksHintSplitContainer");

				AssertEquals("SplitContainer is simply for hiding/showing the hint label, so shouldn't be able to adjust the slider", true, splitContainer.IsSplitterFixed);

				if (areSomeTasksHidden)
				{
					AssertEquals("Some tasks are not shown since they are specific to another company. We should show thie hint label.", false, splitContainer.Panel1Collapsed);

					var label = splitContainer.Panel1.Controls.OfType<ZLabel>().Single();

					if (otherCompanies.Length == 1)
					{
						AssertEquals($"There are tasks in this job that are shown only when logged into {otherCompanies.Single().GC_Code} company.", label.Text);
					}
					else
					{
						var list = string.Join(", ", otherCompanies.Select(c => c.GC_Code));
						AssertEquals($"There are tasks in this job that are shown only when logged into {list} companies.", label.Text);
					}
				}
				else
				{
					AssertEquals("There's no need to show the hint label since all tasks are visible and everything is fines.", true, splitContainer.Panel1Collapsed);
				}
			}
		}

		static string[] GetTaskIDs(IEnumerable tasks)
		{
			return tasks.Cast<ProcessTask>().Select(t => t.P9_TaskID.ToString()).ToArray();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		public void SetupWorkflowSamplerBox(out ProcessJobHeader jobHeader, out ProcessHeader workflow1, out ProcessHeader workflow2, out ProcessTask task1, out ProcessTask task2, out ProcessTask task3, out ProcessTask task4)
		{
			var job = BMSTestHelper.CreateJob<OrgHeader>(Factory);
			jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_CompletionStatement = "job";

			workflow1 = CreateWorkflow(jobHeader, "workflow1");
			task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10, description: "task1");
			task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, sequence: 20, description: "task2");

			workflow2 = CreateWorkflow(jobHeader, "workflow2");
			task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60, sequence: 30, description: "task3");
			task4 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60, sequence: 40, description: "task4");
		}

		#endregion
	}
}
