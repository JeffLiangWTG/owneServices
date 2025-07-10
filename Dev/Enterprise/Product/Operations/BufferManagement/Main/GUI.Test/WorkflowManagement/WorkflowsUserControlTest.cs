using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Security;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowsUserControlTest : BMSTestCaseWithFactory
	{
		#region Column Visibility

		public void TestDefaultAndInvisibleColumns()
		{
			using (var control = new WorkflowsUserControl())
			{
				var allColumns = control.WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"ProcessHeaderType",
					"Sequence",
					"FH_CompletionStatement",
					"CurrentStatus",
					"FH_GG_ReleaseGroup",
					"FH_IsActive",
					"DoNotStartBeforeDateLocal",
					"FH_StatusDescription",
					"AgreedDeliveryDateLocal",
					"FH_Category",
					"CategoryDescription",
				}, visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"TotalEstimatedHoursSummary",
					"CurrentComponentSystemPK",
					"FH_FC_CurrentComponent",
					"ReleaseGroup+GG_Desc",
					"IsOpen",
					"CompletionCriteria",
					"FH_IsCriticalHandover",
					"FH_AllowTaskAutoAssignment",
					"FH_DateAcceptability",
					"LastTransferDateLocal",
					"FH_IsStandby",
					"PrerequisiteStatusShortDescription",
					"ConstraintStatus",
					"FH_StaggeredReleaseDelayExpiry",
					"StaggeredReleaseDelayExpiryLocal",
					"PlannedDuration",
					"SynchroniseBufferPenetration",
					"EffectiveNudge",
					"FH_VoteUpDownAmount",
					"TotalActualHoursIncludingChildrenDateTime",
					"SourceTemplatePK",
					"FH_AgreedDeliveryDateDefaultsFrom",
					"FH_AgreedDeliveryDateDefaultHoursOffset",
					"FH_EarliestStartDateDefaultsFrom",
					"FH_EarliestStartDefaultHoursOffset",
					"LastTransferTypeDescription",
					"FH_TaskPenetrationResetDateTimeUtc",
					"FH_MilestoneCompletionPivotKey",
				}, invisibleColumns);
			}
		}

		public void TestDefaultAndInvisibleColumns_WhenReleaseSequenceModuleEnabled()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var control = new WorkflowsUserControl())
			{
				var allColumns = control.WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"ProcessHeaderType",
					"Sequence",
					"FH_CompletionStatement",
					"CurrentStatus",
					"FH_GG_ReleaseGroup",
					"FH_IsActive",
					"DoNotStartBeforeDateLocal",
					"FH_StatusDescription",
					"AgreedDeliveryDateLocal",
					"FH_Category",
					"CategoryDescription",
				}, visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"ReleaseSequenceName",
					"ReleaseSequencePosition",
					"ReleaseSequenceWorkflow",
					"ReleaseSequenceParentJob",
					"TotalEstimatedHoursSummary",
					"CurrentComponentSystemPK",
					"FH_FC_CurrentComponent",
					"ReleaseGroup+GG_Desc",
					"IsOpen",
					"CompletionCriteria",
					"FH_IsCriticalHandover",
					"FH_AllowTaskAutoAssignment",
					"FH_DateAcceptability",
					"LastTransferDateLocal",
					"FH_IsStandby",
					"PrerequisiteStatusShortDescription",
					"ConstraintStatus",
					"FH_StaggeredReleaseDelayExpiry",
					"StaggeredReleaseDelayExpiryLocal",
					"PlannedDuration",
					"SynchroniseBufferPenetration",
					"EffectiveNudge",
					"FH_VoteUpDownAmount",
					"TotalActualHoursIncludingChildrenDateTime",
					"SourceTemplatePK",
					"FH_AgreedDeliveryDateDefaultsFrom",
					"FH_AgreedDeliveryDateDefaultHoursOffset",
					"FH_EarliestStartDateDefaultsFrom",
					"FH_EarliestStartDefaultHoursOffset",
					"LastTransferTypeDescription",
					"FH_TaskPenetrationResetDateTimeUtc",
					"FH_MilestoneCompletionPivotKey",
				}, invisibleColumns);
			}
		}

		public void TestDefaultAndInvisibleColumns_WhenNewReleaseGateRegistryEnabled()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var control = new WorkflowsUserControl())
			{
				var allColumns = control.WorkflowsGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				var visibleColumns = allColumns.Where(c => c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"ProcessHeaderType",
					"Sequence",
					"FH_CompletionStatement",
					"CurrentStatus",
					"FH_GG_ReleaseGroup",
					"FH_IsActive",
					"DoNotStartBeforeDateLocal",
					"FH_StatusDescription",
					"AgreedDeliveryDateLocal",
					"FH_Category",
					"CategoryDescription",
					"FH_IsApproved",
					"FH_GB_Branch",
					"FH_GE_Department",
				}, visibleColumns);

				var invisibleColumns = allColumns.Where(c => !c.IsVisible).Select(c => c.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[] {
					"TotalEstimatedHoursSummary",
					"CurrentComponentSystemPK",
					"FH_FC_CurrentComponent",
					"ReleaseGroup+GG_Desc",
					"IsOpen",
					"CompletionCriteria",
					"FH_IsCriticalHandover",
					"FH_AllowTaskAutoAssignment",
					"FH_DateAcceptability",
					"LastTransferDateLocal",
					"FH_IsStandby",
					"PrerequisiteStatusShortDescription",
					"ConstraintStatus",
					"FH_StaggeredReleaseDelayExpiry",
					"StaggeredReleaseDelayExpiryLocal",
					"PlannedDuration",
					"SynchroniseBufferPenetration",
					"EffectiveNudge",
					"FH_EffectiveNudge",
					"FH_VoteUpDownAmount",
					"TotalActualHoursIncludingChildrenDateTime",
					"SourceTemplatePK",
					"FH_AgreedDeliveryDateDefaultsFrom",
					"FH_AgreedDeliveryDateDefaultHoursOffset",
					"FH_EarliestStartDateDefaultsFrom",
					"FH_EarliestStartDefaultHoursOffset",
					"LastTransferTypeDescription",
					"FH_TaskPenetrationResetDateTimeUtc",
					"FH_MilestoneCompletionPivotKey",
					"DedicatedBufferName",
					"FH_EffectiveAgreedDeliveryDateUtc",
					"FH_ReleaseSequenceSortDateUtc",
					"FH_DeadlineType",
					"EffectiveBranchCode",
					"EffectiveDepartmentCode",
					"FH_BMT_BufferTimespan",
					"EffectiveBufferDurationString",
					"LatestAcceptableReleaseDateUtcString"
				}, invisibleColumns);
			}
		}

		#endregion

		#region ConstraintStatus

		public void TestConstraintStatusColumn()
		{
			using (var control = new WorkflowsUserControl())
			{
				var grid = ((ZGrid)control.Controls.Find("WorkflowsGrid", true).Single());
				var constraintStatusColumn = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "ConstraintStatus");

				AssertNotNull("Constraint-Status column should exist", constraintStatusColumn);
				Assert("Constraint-Status column length should be able to show widest text i.e. Ready for Constraint", constraintStatusColumn.Width >= CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144));
			}
		}

		#endregion

		#region Tags

		public void TestTagSecurity_Delete()
		{
			Env.Security.TagAdd.IsAllowed = true;
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "TDA");
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var tag = BMSTestHelper.CreateTagMagnitude(tagDefinition, "TMA");
			workflow1.AddTag(tag);
			jobHeader.AddTag(tag);
			var viewModel = new WorkflowManagementViewModel(jobHeader);
			Factory.Save();

			using (Env.SetTemporarySecurityInstanceForTest(GetSecurityInstance()))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.TagRemove);

				using (var control = new WorkflowsUserControl())
				using (var form = new ZForm(viewModel))
				{
					form.Show();
					form.Controls.Add(control);
					var workflowGrid = ((ZGrid)control.Controls.Find("WorkflowsGrid", true).Single());
					workflowGrid.List.Add(workflow1);
					workflowGrid.List.Add(workflow2);
					workflowGrid.Select(0);
					var grid = ((TagGrid)control.Controls.Find("tagLinkGrid", true).Single());
					grid.Select(0);
					grid.DeleteMenuItem.PerformClick();

					Assert("The workflow tag should not have been removed", viewModel.AllProcessHeaders.Where(ph => ph.PK == workflow1.PK).Any(ph => ph.TagLinks.First().TagDefinitionPk == tagDefinition.PK));
					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Tag Groups -> Remove Tag -> Not Specified -> TMA - ", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		SecurityCore GetSecurityInstance()
		{
			var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			staff.GS_IsController = false;

			var securityCollection = new GlbSecurityCollection(Factory);
			securityCollection.Load();

			var collection = new GlbSecurityCollection(Factory);
			collection.Load();
			collection.RemoveAndDeleteAll();

			return BMSecurityTestHelper.CreateSecurity(securityCollection, staff);
		}

		public void TestTagGrid_WithWorkQueueTagLink_WhenQueueSequencesChangedByOtherUserCausingApparentDuplicate_AndValidationCalled_ShouldNotShowError()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "ABC", "ABC");

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow 1", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow 2");

			var link1 = (WorkQueueMembershipLink)workflow1.AddTag(queue).Link;
			var link2 = (WorkQueueMembershipLink)workflow2.AddTag(queue).Link;

			link1.TGL_Sequence = 1;
			link2.TGL_Sequence = 2;

			Factory.Save();

			var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy = newFactory1.Load<DummyWithWorkflow>(workflow1.Parent.PK);
			var loadedWorkflow1 = newFactory1.Load<ProcessHeader>(workflow1.PK);

			using (var form = new ZForm(dummy) { Size = ControlDpiScalingHelper.NewScaledSize(1000, 800) })
			using (var tabPage = new WorkflowManagementTabPage())
			using (var tabControl = new ZTabControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(tabPage);
				tabControl.Dock = DockStyle.Fill;
				form.Show();
				tabPage.SetDataBinding(dummy, "");

				var control = tabPage.Controls.OfType<WorkflowManagementUserControl>().Single();
				control.NavigateToWorkflowItem(loadedWorkflow1);

				Application.DoEvents();

				var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var loadedLink1 = newFactory2.Load<WorkQueueMembershipLink>(link1.PK);
				var loadedLink2 = newFactory2.Load<WorkQueueMembershipLink>(link2.PK);

				loadedLink1.TGL_Sequence = 2;
				loadedLink2.TGL_Sequence = 1;
				newFactory2.Save();

				var dummyFromForm = (DummyWithWorkflow)form.BusinessEntity;
				form.FireValidateAllForTest();

				AssertNoErrors(@"Because sequence numbers were changed by another user, the form might have been tricked into thinking that the shown tag link has a duplicate sequence.
But because we've disabled that validation for everywhere except the Work Queue form, the validation for sequence shouldn't have even run on this operational job form. SAD!", dummyFromForm);
			}
		}

		#endregion

		#region ResourceStrings

		public void TestResourceStrings()
		{
			using (var control = new WorkflowsUserControl())
			{
				var tab1 = (ZTabPage)control.Controls.Find("WorkflowsTabPage", true).Single();
				var tab2 = (ZTabPage)control.Controls.Find("NCNTabPage", true).Single();
				var grid = ((ZGrid)control.Controls.Find("WorkflowsGrid", true).Single());
				var actualHourscolumnName = nameof(ProcessHeader.TotalActualHoursIncludingChildrenDateTime);
				var totalHrsColumn = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(column => column.ColumnName == actualHourscolumnName);

				AssertEquals("Workflows", tab1.CaptionResourceString.Caption);
				AssertEquals("Workflow Relationship Designer", tab2.CaptionResourceString.Caption);
				AssertEquals("Actual Hours", totalHrsColumn.CaptionResourceString.Caption);
			}
		}

		#endregion

		#region Grid Interaction

		[ExpectNoExceptions]
		public void TestAddingWorkflowRowsDoesntExplode()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				control.WorkflowsGrid.ListManager.AddNew();
				Application.DoEvents();

				control.WorkflowsGrid[2, 2] = new ZString("Boop");
			}
		}

		public void TestAfterBind_ShouldSelectFirstOpenWorkflow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			var task1 = jobHeader.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = jobHeader.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;
			var task3 = jobHeader.Parent.WorkflowItems.AddNew();
			task3.P9_FH_ProcessHeader = workflow3.PK;

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(viewModel, string.Empty);

				AssertEquals("Second workflow (third in collection) is the first open workflow", 2, control.WorkflowsGrid.ListManager.Position);
			}
		}

#if !WINZOR
		public void TestPressCtrlShift_ValueDoesNotDisappear()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var viewModel = new WorkflowManagementViewModel(jobHeader);
			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.Controls.Add(control);
				form.Show();
				var workflowGrid = (ZGrid)control.Controls.Find("WorkflowsGrid", true).Single();

				var columnStyle = (ZTextBoxColumnStyle)workflowGrid.TableStyles[0].GridColumnStyles[2];
				workflowGrid.BeginEdit(columnStyle, 2);
				columnStyle.EditControl.Visible = true;
				Application.DoEvents();

				KeySender.PostKeyDown(columnStyle.EditControl, Keys.A);
				Application.DoEvents();
				AssertEquals("'a' is in textbox", "a", columnStyle.EditControl.Text);

				KeySender.PostKeyDown(workflowGrid, Keys.Control | Keys.Shift);
				Application.DoEvents();
				AssertEquals("Press Ctrl+Shift, 'a' does not disappear", "a", columnStyle.EditControl.Text);
			}
		}
#endif

		#endregion

		#region Network Interaction

#if !WINZOR
		public void TestShouldShowNetwork_WhenSwitchingToNCNTab()
		{
			BMSTestHelper.EnableBMSInRegistry();
			CreateSystem("INQ");

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1", releaseDateTime: ZDateTime.Now.AddDays(-3), description: "task 1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2", releaseDateTime: ZDateTime.Now.AddDays(-2), description: "task 2");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow3", releaseDateTime: ZDateTime.Now.AddDays(-2), description: "task 3");

			Factory.Save();

			using (var form = new SalesEnquiryForm(enquiry))
			{
				form.ControllerID = ControllerIDs.SalesEnquiry;
				form.Show();

				var workflowTabPage = form.FindAll<ZWorkflowTabPage>().Single();
				((TabControl)workflowTabPage.Parent).SelectTab(workflowTabPage);

				Application.DoEvents();

				var control = form.FindAll<WorkflowsUserControl>().First();
				AssertNotNull(control);

				control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed; // need to select the tab to host the NetworkUserControl
				Application.DoEvents(); // need to pump the queue now to let NetworkUserControl create a NetworkViewModel in SetDataContext() to get it ready for selection operations
				var networkViewModel = control.RelationshipDesignerUserControl.NetworkUserControl.ViewModel.NetworkViewModel;
				var network = networkViewModel.GetJobNetwork();
				AssertNotNull("SetDataContext() on NetworkUserControl should be invoked with JobNetwork passed through", network);

				AssertEquals("The network should have 3 entities", 3, network.Entities.Count);
				AssertEquals("The diagram should show 3 shapes", 3, networkViewModel.Nodes.Count());
			}
		}

		public void TestChangeToNCNTab_ShouldReloadNetwork()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();

			using (var form = new ZForm(viewModel))
			using (var control = new WorkflowsUserControl())
			{
				control.SetDataBinding(viewModel, string.Empty);
				using (new DisposableAction(() => form.Controls.Remove(control)))
				{
					form.Controls.Add(control);
					form.Show();

					control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed;

					AssertEquals(1, control.RelationshipDesignerUserControl.Network.Entities.Count);
					AssertEquals(0, control.RelationshipDesignerUserControl.Network.Entities[0].Links.Count());

					var workflow2 = jobHeader.ProcessHeaders.AddNew();
					var link = workflow1.GetOrCreateDependencyLink(workflow2);

					AssertEquals("New shape should be created when workflow collection count changes", 2, control.RelationshipDesignerUserControl.Network.Entities.Count);
					AssertEquals("New links are not created until network is reloaded", 0, control.RelationshipDesignerUserControl.Network.Entities[0].Links.Count());
					AssertEquals("New links are not created until network is reloaded", 0, control.RelationshipDesignerUserControl.Network.Entities[1].Links.Count());

					control.WorkflowNCNTabControl_Exposed.SelectedTab = control.WorkflowsTabPage_Exposed;
					control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed;

					AssertEquals(2, control.RelationshipDesignerUserControl.Network.Entities.Count);
					AssertEquals("New links should be created when network is reloaded on tab page navigation", 1, control.RelationshipDesignerUserControl.Network.Entities[0].Links.Count());
					AssertEquals("New links should be created when network is reloaded on tab page navigation", 1, control.RelationshipDesignerUserControl.Network.Entities[1].Links.Count());
				}
			}
		}

		public void TestJobNetwork_EntitySelected_ShouldUpdateWorkflowGridPosition()
		{
			BMSTestHelper.EnableBMSInRegistry();
			CreateSystem("INQ");

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = BMSTestHelper.CreateJobHeader(enquiry, addDefaultProcessHeaderIfNone: false);

			Factory.Save();

			using (var form = new SalesEnquiryForm(enquiry))
			{
				form.ControllerID = ControllerIDs.SalesEnquiry;
				form.Show();

				var workflowTabPage = form.FindAll<ZWorkflowTabPage>().Single();
				((TabControl)workflowTabPage.Parent).SelectTab(workflowTabPage);

				Application.DoEvents();

				var control = form.FindAll<WorkflowsUserControl>().First();
				AssertNotNull(control);

				control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed;

				Application.DoEvents(); // need to pump the queue now to let NetworkUserControl create a NetworkViewModel in SetDataContext() to get it ready for selection operations
				form.Show();
				var networkViewModel = control.RelationshipDesignerUserControl.NetworkUserControl.ViewModel.NetworkViewModel;
				var network = networkViewModel.GetJobNetwork();
				AssertNotNull("SetDataContext() on NetworkUserControl should be invoked with JobNetwork passed through", network);
				var diagram = network.DiagramEntity;

				var workflowShape1 = networkViewModel.CreateNewWorkflow(diagram);
				workflowShape1.ProcessHeader.FH_CompletionStatement = "workflow1";
				var workflowShape2 = networkViewModel.CreateNewWorkflow(diagram);
				workflowShape2.ProcessHeader.FH_CompletionStatement = "workflow2";
				var workflowShape3 = networkViewModel.CreateNewWorkflow(diagram);
				workflowShape3.ProcessHeader.FH_CompletionStatement = "workflow3";

				BMSTestHelper.CreateTask(workflowShape1.ProcessHeader, description: "task 1");
				BMSTestHelper.CreateTask(workflowShape2.ProcessHeader, description: "task 2");
				BMSTestHelper.CreateTask(workflowShape3.ProcessHeader, description: "task 3");

				AssertEquals("Workflows should be created with their nodes", 3, networkViewModel.Nodes.Count());

				var defaultDiagram = jobHeader.GetDefaultDiagram();

				CombineAssertions(@"
Given a Workflows User Control Diagram with 3 workflows,
When selecting workflow 3
Then the workflows grid should be selected and then task grid updated"
					, () =>
				{
					AssertNoExceptionThrown(() => networkViewModel.SelectSingleEntity(workflowShape3));
					AssertEquals(workflowShape3.ProcessHeader, control.WorkflowsGrid.ListManager.GetCurrent());
				});

				AssertNoExceptionThrown(@"
Given a Workflows User Control Diagram with 3 workflows one of which is selected,
When selecting workflow 2
Then the workflows grid should be selected and then task grid updated"
					, () => networkViewModel.SelectSingleEntity(workflowShape2));

				AssertEquals("The third (position 2) (JobWorkflow is 0) element in the workflows grid should be selected", workflowShape2.ProcessHeader, control.WorkflowsGrid.ListManager.GetCurrent());

				var tasksGrid = form.Controls.Find("TasksGrid", true)[0] as ZGrid;
				AssertNotNull(tasksGrid);

				AssertEquals("The task grid should only have task 2 in its list", "task 2", ((ProcessTask)tasksGrid.List[0]).P9_Description);
			}
		}

		public void TestJobNetwork_DeselectingAllShapes_ShouldSelectJobLevelWorkflowInTheGrid()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();

			using (var form = new ZForm(viewModel))
			using (var control = new WorkflowsUserControl())
			{
				using (new DisposableAction(() => form.Controls.Remove(control)))
				{
					form.Controls.Add(control);
					form.Show();

					control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed;

					Application.DoEvents();

					var networkViewModel = control.RelationshipDesignerUserControl.NetworkUserControl.ViewModel.NetworkViewModel;
					var network = networkViewModel.GetJobNetwork();

					var workflowShape1 = networkViewModel.CreateNewWorkflow(network.DiagramEntity);
					workflowShape1.ProcessHeader.FH_CompletionStatement = "workflow1";
					var workflowShape2 = networkViewModel.CreateNewWorkflow(network.DiagramEntity);
					workflowShape2.ProcessHeader.FH_CompletionStatement = "workflow2";
					var workflowShape3 = networkViewModel.CreateNewWorkflow(network.DiagramEntity);
					workflowShape3.ProcessHeader.FH_CompletionStatement = "workflow3";

					BMSTestHelper.CreateTask(workflowShape1.ProcessHeader, description: "task 1");
					BMSTestHelper.CreateTask(workflowShape2.ProcessHeader, description: "task 2");
					BMSTestHelper.CreateTask(workflowShape3.ProcessHeader, description: "task 3");
					AssertEquals(4, control.RelationshipDesignerUserControl.Network.Entities.Count);

					networkViewModel.SelectSingleEntity(workflowShape1);
					AssertEquals("Workflow 1 should be selected in the grid", workflowShape1.ProcessHeader, control.WorkflowsGrid.ListManager.GetCurrent());

					networkViewModel.SelectEntities(Enumerable.Empty<INetworkEntity>());
					Assert("No shapes should be selected", !networkViewModel.SelectedNodes.Any());
					AssertEquals("Deselecting all diagram shapes should result in the job-level workflow being selected in the workflows grid", jobHeader, control.WorkflowsGrid.ListManager.GetCurrent());

					networkViewModel.SelectSingleEntity(workflowShape2);
					AssertEquals("Workflow 2 should be selected in the grid", workflowShape2.ProcessHeader, control.WorkflowsGrid.ListManager.GetCurrent());
				}
			}
		}

		public void TestJobNetwork_DeselectingAllShapes_ShouldShowAllTasksInTheGrid()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();

			using (var form = new ZForm(viewModel) { ControllerID = ControllerIDs.Organisation })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var workflowsControl = form.FindSingle<WorkflowsUserControl>();

				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.NCNTabPage_Exposed;

				Application.DoEvents();

				var networkViewModel = workflowsControl.RelationshipDesignerUserControl.NetworkUserControl.ViewModel.NetworkViewModel;
				var network = networkViewModel.GetJobNetwork();

				var workflowShape1 = networkViewModel.CreateNewWorkflow(network.DiagramEntity);
				workflowShape1.ProcessHeader.FH_CompletionStatement = "workflow1";
				var workflowShape2 = networkViewModel.CreateNewWorkflow(network.DiagramEntity);
				workflowShape2.ProcessHeader.FH_CompletionStatement = "workflow2";
				var workflowShape3 = networkViewModel.CreateNewWorkflow(network.DiagramEntity);
				workflowShape3.ProcessHeader.FH_CompletionStatement = "workflow3";

				var task1 = BMSTestHelper.CreateTask(workflowShape1.ProcessHeader, description: "task 1");
				var task2 = BMSTestHelper.CreateTask(workflowShape2.ProcessHeader, description: "task 2");
				var task3 = BMSTestHelper.CreateTask(workflowShape3.ProcessHeader, description: "task 3");
				AssertEquals(4, workflowsControl.RelationshipDesignerUserControl.Network.Entities.Count);

				networkViewModel.SelectSingleEntity(workflowShape1);
				AssertContainsExactElementsInAnyOrder("Only task 1 should be shown in the task grid", new[] { task1 }, control.TasksGrid_ForTest.ListManager.List);

				networkViewModel.SelectEntities(Enumerable.Empty<INetworkEntity>());
				Assert("No shapes should be selected", !networkViewModel.SelectedNodes.Any());
				AssertContainsExactElementsInAnyOrder("Deselecting all diagram shapes should result in all tasks being displayed in the tasks grid (similar to what happens when we select the job-level workflow)", new[] { task1, task2, task3 }, control.TasksGrid_ForTest.ListManager.List);

				networkViewModel.SelectSingleEntity(workflowShape2);
				AssertContainsExactElementsInAnyOrder("Only task 2 should be shown in the task grid", new[] { task2 }, control.TasksGrid_ForTest.ListManager.List);
			}
		}

		public void TestWorkflowRelationshipDesigner_WhenJobLevelWorkflowDeleted_ShouldShowLabel_AndNotThrowExceptionsOrReportErrors()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var form = new ZForm(viewModel) { ControllerID = ControllerIDs.Organisation })
			using (var control = new WorkflowManagementUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				jobHeader.Delete();

				var workflowsControl = form.FindSingle<WorkflowsUserControl>();

				AssertNoExceptionThrown(() =>
				{
					workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.NCNTabPage_Exposed;
					Application.DoEvents();
				});

				var label = workflowsControl.FindSingleOrDefault<ZLabel>(x => x.Text == "The Workflow Relationship Designer cannot be shown since the network has an invalid relationship. Please correct this relationship and click this message to show the network.");
				AssertNotNull(label);
				AssertEquals(true, label.Visible);
			}
		}

#endif

		#endregion

		#region Universal Copy

		public void TestUniversalCopy_ForJobLevelWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Donary Clintump");

			Factory.Save();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var copyWorkflow = PerformUniversalCopy(control, workflow);

				AssertNotNull(copyWorkflow);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Caption);

				var copyJobHeader = PerformUniversalCopy(control, jobHeader);

				AssertNull(copyJobHeader);
				AssertEquals("Cannot perform Universal Copy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("",
@"Universal Copy cannot be performed for the following reason:
There can only be one job-level workflow per job.
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUniversalCopySchedules_ShouldBeAvailable()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Hillald Trunton");

			Factory.Save();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				var grid = (ZGrid)control.Controls.Find("WorkflowsGrid", true).FirstOrDefault();
				AssertNotNull(grid);

				grid.ListManager.Position = 0;
				Application.DoEvents();

				var ucMenuItem = grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);
				AssertNotNull(ucMenuItem);
				ucMenuItem.PerformSelect();

				var csMenuItem = ucMenuItem.MenuItems.FindByText("Copy Schedules");
				AssertNotNull("Copy Schedules menu item should be available as the job is not a workflow template", csMenuItem);
			}
		}

		public void TestUniversalCopySchedule_ForJobLevelWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Hillald Trunton");

			Factory.Save();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.Controls.Add(control);
				form.Show();

				SetupCopySchedule(control, workflow);

				AssertNotNull("Should show the copy schedules dialog", ZFormModaliser.LastFormShownDialogForTest);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				ZFormModaliser.LastFormShownDialogForTest = null;

				SetupCopySchedule(control, jobHeader);

				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("There can only be one job-level workflow per job.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Cannot create a copy schedule for this object", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		ProcessHeader PerformUniversalCopy(WorkflowsUserControl control, ProcessHeader processHeader)
		{
			var existingWorkflows = control.WorkflowsGrid.List.Cast<ProcessHeader>().ToArray();
			control.WorkflowsGrid.ListManager.Position = control.WorkflowsGrid.List.IndexOf(processHeader);

			AssertEquals(processHeader, control.WorkflowsGrid.GetCurrent());

			var menuItem = control.WorkflowsGrid.ContextMenu.MenuItems.FindByText("Universal Copy", true);
			menuItem.PerformSelect();

			var copyMenuItem = menuItem.MenuItems.Cast<MenuItem>().First();

			copyMenuItem.PerformClick();

			return control.WorkflowsGrid.List.Cast<ProcessHeader>().SingleOrDefault(w => !existingWorkflows.Contains(w));
		}

		void SetupCopySchedule(WorkflowsUserControl control, ProcessHeader processHeader)
		{
			control.WorkflowsGrid.ListManager.Position = control.WorkflowsGrid.List.IndexOf(processHeader);
			AssertEquals(processHeader, control.WorkflowsGrid.GetCurrent());

			control.WorkflowsGrid.UnSelectAll();
			control.WorkflowsGrid.Select(control.WorkflowsGrid.ListManager.Position);

			var menuItem = control.WorkflowsGrid.ContextMenu.MenuItems.FindByText("Universal Copy", true);
			menuItem.PerformSelect();

			var copySchedulesMenuItem = menuItem.MenuItems.FindByText("Copy Schedules");
			var createScheduleMenuItem = copySchedulesMenuItem.MenuItems.Cast<MenuItem>().FindByText("Create Copy Schedule");

			createScheduleMenuItem.PerformClick();
		}

		#endregion

		#region Promote

		public void TestPromoteWorkflowAction_NoBMSystem_DoesNotExplode()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow1.PK;

			var completionStatement = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();
				var contextMenu = control.WorkflowsGrid.ContextMenu;
				Application.DoEvents();

				var menuItem = contextMenu.MenuItems.FindByText("Promote to Job");

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertNoExceptionThrown(() => menuItem.PerformClick());
			}
		}

		public void TestPromoteWorkflowAction_OrgHeader_ProcessJobHeaderDoesNotPromote()
		{
			var system = CreateSystem("ORG");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow1.PK;

			var completionStatement = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();
				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Promote to Job");

				AssertNoExceptionThrown(() => menuItem.PerformClick());

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());
				Application.DoEvents();

				AssertType<ProcessJobHeader>(control.WorkflowsGrid.ListManager.GetCurrent());
				AssertEquals(false, Application.OpenForms.Cast<Form>().Any(f => f.GetType() == typeof(ZOrganisationsForm)));
			}
		}

		public void TestPromoteWorkflowAction_WorkItem_PromoteOpensForm()
		{
			var system = CreateSystem("WKI");
			var workItem = (BusinessObject)Factory.New<IWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow1.PK;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task.P9_Description = "Boop";

			AssertNoErrors(workItem);

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.SetDataBinding(workItem, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();
				control.WorkflowsGrid.ListManager.Position++;
				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Promote to Job");

				AssertNoExceptionThrown(() => menuItem.PerformClick());

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());
				Application.DoEvents();

				AssertEquals(false, control.WorkflowsGrid.ListManager.GetCurrent() is ProcessJobHeader);

				AssertEquals(false, workflow1.IsDeleted);

				var workItemForm = Application.OpenForms.OfType<ZTemplateForm>().FirstOrDefault(f => f.DataSource is IWorkItem);
				AssertNotNull(workItemForm);
				AssertSaved(workItemForm.FireSaveButton());
				AssertEquals(true, workflow1.IsDeleted);
			}
		}

		public void TestPromoteWorkflowAction_SalesEnquiry_PromoteOpensForm()
		{
			var system = CreateSystem("ORG", "INQ");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			BMSTestHelper.CreateTask(workflow1, staff.GS_Code);
			BMSTestHelper.CreateTask(workflow1, staff.GS_Code);

			AssertNoErrors(org);

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.SetDataBinding(org, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();
				control.WorkflowsGrid.ListManager.Position++;
				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var salesEnquiryMenuItem = contextMenu.MenuItems.FindByText("Promote to Job").MenuItems[1].MenuItems[0];

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertNoExceptionThrown(() => salesEnquiryMenuItem.PerformClick());
				Application.DoEvents();

				AssertEquals(false, control.WorkflowsGrid.ListManager.GetCurrent() is ProcessJobHeader);
				AssertEquals(false, workflow1.IsDeleted);

				var salesEnquiryForm = Application.OpenForms.OfType<SalesEnquiryForm>().FirstOrDefault();
				AssertNotNull(salesEnquiryForm);
				var salesEnquiry = ((SalesEnquiry)salesEnquiryForm.DataSource);
				salesEnquiry.FillWithValidTestData();

				AssertSaved(salesEnquiryForm.FireSaveButton());
				AssertEquals(true, workflow1.IsDeleted);
			}
		}

		public void TestPromoteWorkflowAction_MenuPopulatedFromSystems()
		{
			var system1 = CreateSystem("ORG", "BKN");
			var system2 = CreateSystem("CAM");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow1.PK;

			var completionStatement = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.SetDataBinding(org, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();
				control.WorkflowsGrid.ListManager.Position++;
				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Promote to Job");

				AssertEquals("Organization", menuItem.MenuItems[0].Text);
				AssertEquals("Other Job Types", menuItem.MenuItems[1].Text);

				AssertEquals(2, menuItem.MenuItems[1].MenuItems.Count);
				AssertEquals("Campaign", menuItem.MenuItems[1].MenuItems[0].Text);
				AssertEquals("Shipping Manager Booking", menuItem.MenuItems[1].MenuItems[1].Text);
			}
		}

		public void TestPromoteWorkflowAction_NotDeletePromotedTasksOnSave()
		{
			CreateSystem("WKI");
			var workItem = (BusinessObject)Factory.New<IWorkItem>();
			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow 1");
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			BMSTestHelper.CreateTask(workflow1, description: "Bleb11", taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);
			BMSTestHelper.CreateTask(workflow1, description: "Melm22", taskStatus: ProcessTaskStatusCodeList.Codes.Suspended);

			AssertNoErrors(workItem);
			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var origForm = new ZForm(viewModel))
			{
				origForm.SetDataBinding(workItem, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				origForm.Controls.Add(control);
				origForm.Show();
				Application.DoEvents();

				AssertEquals(3, control.WorkflowsGrid.List.Count); // job, default template, workflow1
				control.WorkflowsGrid.OnPopup_CallForTesting();
				control.WorkflowsGrid.ListManager.Position = 2; // workflow1 row

				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Promote to Job");
				AssertNoExceptionThrown(() => menuItem.PerformClick());

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());
				Application.DoEvents();

				AssertEquals(false, control.WorkflowsGrid.ListManager.GetCurrent() is ProcessJobHeader);
				AssertEquals(false, workflow1.IsDeleted);

				var promotedForm = Application.OpenForms.OfType<ZTemplateForm>().First(f => f.DataSource is IWorkItem);
				var promotedFormTabControl = promotedForm.FindAll<ZTemplateTabControl>().First();
				var promotedWorkflowTabPage = (ZTabPage)promotedFormTabControl.TabPages["WorkflowTabPage"];
				AssertNotNull(promotedWorkflowTabPage);
				promotedFormTabControl.SelectedTab = promotedWorkflowTabPage;
				Application.DoEvents();

				var promotedWorkflowManagementControl = promotedWorkflowTabPage.FindAll<WorkflowManagementUserControl>().First();
				AssertEquals("Should have 2 task rows before saving", 2, promotedWorkflowManagementControl.TasksGrid_ForTest.List.Count);
				AssertEquals(2, ((IWorkflowProvider)promotedForm.DataSource).WorkflowItems.Count);

				AssertSaved(promotedForm.FireSaveButton());
				Application.DoEvents();

				AssertEquals(true, workflow1.IsDeleted);
				AssertEquals("Should have 2 task rows after saving", 2, promotedWorkflowManagementControl.TasksGrid_ForTest.List.Count);
				AssertEquals(2, ((IWorkflowProvider)promotedForm.DataSource).WorkflowItems.Count);
			}
		}

		#endregion

		#region Demote

		public void TestDemoteWorkflowAction_OrgHeader_PromoteOpensForm()
		{
			var system = CreateSystem("ORG");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeaderToBeDemoted = ProcessJobHeader.GetForParent(org1, Factory);
			var consumingJobHeader = ProcessJobHeader.GetForParent(org2, Factory);
			var initialWorkflowCount = consumingJobHeader.ProcessHeaders.Count;

			var workflow1 = jobHeaderToBeDemoted.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);

			var viewModel = new WorkflowManagementViewModel(jobHeaderToBeDemoted);

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.SetDataBinding(org1, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();

				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Demote to Workflow").MenuItems.FindByText("Into Other Job");
				AssertNoExceptionThrown(() => menuItem.PerformClick());

				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(org2))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());
				}

				AssertEquals(true, control.WorkflowsGrid.ListManager.GetCurrent() is ProcessJobHeader);
				AssertEquals(true, workflow1.IsDeleted);
				AssertEquals(false, jobHeaderToBeDemoted.ProcessHeaders.Any());
				AssertEquals(initialWorkflowCount + 1, consumingJobHeader.ProcessHeaders.Count);
			}
		}

		public void TestDemoteWorkflowAction_DoesntExplodeWithWorkflowSelected()
		{
			var system1 = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.SetDataBinding(org, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.ListManager.Position++;
				control.WorkflowsGrid.OnPopup_CallForTesting();
				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Demote to Workflow")
					.MenuItems.FindByText("Into Other Job");

				AssertEquals(false, control.WorkflowsGrid.ListManager.GetCurrent() is ProcessJobHeader);
				AssertNoExceptionThrown(() => menuItem.MenuItems[0].PerformClick());
			}
		}

		public void TestDemoteWorkflowAction_MenuPopulatedFromSystems()
		{
			var system1 = CreateSystem("ORG", "BKN");
			var system2 = CreateSystem("CAM");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.SetDataBinding(org, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();
				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Demote to Workflow").MenuItems.FindByText("Into Other Job");

				AssertEquals("Organization", menuItem.MenuItems[0].Text);
				AssertEquals("Other Job Types", menuItem.MenuItems[1].Text);

				AssertEquals(2, menuItem.MenuItems[1].MenuItems.Count);
				AssertEquals("Campaign", menuItem.MenuItems[1].MenuItems[0].Text);
				AssertEquals("Shipping Manager Booking", menuItem.MenuItems[1].MenuItems[1].Text);
			}
		}

		public void TestDemoteWorkflowAction_DemoteToParent()
		{
			var system = CreateSystem("ORG");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeaderToBeDemoted = ProcessJobHeader.GetForParent(org1, Factory);
			var consumingJobHeader = ProcessJobHeader.GetForParent(org2, Factory);

			jobHeaderToBeDemoted.GetOrCreateLinkToParent(consumingJobHeader);

			var workflow1 = jobHeaderToBeDemoted.ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow1, string.Empty, 30);

			var viewModel = new WorkflowManagementViewModel(jobHeaderToBeDemoted);

			Factory.Save();

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				form.SetDataBinding(org1, string.Empty);
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.OnPopup_CallForTesting();

				var contextMenu = control.WorkflowsGrid.ContextMenu;
				var menuItem = contextMenu.MenuItems.FindByText("Demote to Workflow")
					.MenuItems.FindByText("Within Parent Job");
				var parentItem = menuItem.MenuItems[0];

				AssertNoExceptionThrown(() => menuItem.PerformClick());
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
				AssertNoExceptionThrown(() => parentItem.PerformClick());

				AssertEquals(true, control.WorkflowsGrid.ListManager.GetCurrent() is ProcessJobHeader);
				AssertEquals(true, workflow1.IsDeleted);
				AssertEquals(false, jobHeaderToBeDemoted.ProcessHeaders.Any());

				AssertEquals(task1.P9_ParentID, org2.PK);
				AssertEquals(task2.P9_ParentID, org2.PK);
			}
		}

		#endregion

		#region Clone

		public void TestCloneMultipleWorkflows_ShouldCloneLinksBetweenClonedWorkflows()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Coding 1";
			workflow2.FH_CompletionStatement = "Review 1";
			workflow3.FH_CompletionStatement = "Not Cloned";

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy })
			using (var managementControl = new WorkflowManagementUserControl())
			{
				form.Controls.Add(managementControl);
				form.Show();

				managementControl.SetDataBinding(viewModel, string.Empty);
				Application.DoEvents();

				var workflowsControl = managementControl.FindAll<WorkflowsUserControl>().Single();
				workflowsControl.WorkflowsGrid.Select(1);
				workflowsControl.WorkflowsGrid.Select(2);

				Application.DoEvents();

				workflowsControl.WorkflowsGrid.OnPopup_CallForTesting();
				var cloneMenuItem = workflowsControl.WorkflowsGrid.ContextMenu.MenuItems.FindByText("Clone");
				AssertNotNull(cloneMenuItem);

				cloneMenuItem.PerformClick();
				AssertEquals(5, jobHeader.ProcessHeaders.Count);

				var workflow4 = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "Coding 1 (1)");
				var workflow5 = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "Review 1 (1)");

				AssertEquals(1, workflow4.LinksFromMeToOthers.Count());
				AssertEquals(workflow5.PK, workflow4.LinksFromMeToOthers.Single().FP_FH_HeaderTo);
				AssertEquals(0, workflow4.LinksFromOthersToMe.Count());
			}
		}

		#endregion

		#region Delete Workflow

		public void TestDeleteJobHeader_ShouldNotAllow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.ListManager.Position = 0;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals("The Job-level workflow cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(3, control.WorkflowsGrid.List.Count);
			}
		}

		public void TestDeleteWorkflowWithNoTasks_ShouldAllow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(1);
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, control.WorkflowsGrid.List.Count);
			}
		}

		public void TestDeleteWorkflow_WhenPlanningMangementDisabled_ShouldNotReportError()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(1);
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, control.WorkflowsGrid.List.Count);

				Factory.Save();
			}
		}

		#endregion

		#region Network Refresh

#if !WINZOR
		[ExpectNoExceptions]
		public void TestSaveForm_ShouldNotThrowExceptionIfWorkflowsGridListIsNull()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task = workflow1.Parent.WorkflowItems.AddNew();
			task.P9_Type = "UDF";
			task.P9_Description = "task";

			AssertEquals(false, jobHeader.HasErrors);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var form = new ZForm(viewModel) { ControllerID = DummyControllerIDs.Dummy })
			using (var managementControl = new WorkflowManagementUserControl())
			{
				form.Controls.Add(managementControl);
				form.Show();
				managementControl.SetDataBinding(jobHeader, string.Empty);

				var workflowsControl = managementControl.FindAll<WorkflowsUserControl>().Single();
				workflowsControl.SetDataBinding(viewModel, string.Empty);

				viewModel.AllProcessHeaders.AddNew().FH_CompletionStatement = "Boop";

				workflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = workflowsControl.NCNTabPage_Exposed;
				Application.DoEvents();

				var refreshed = false;
				var onSavedCalled = false;

				form.Saved += (s, e) => onSavedCalled = true;
				workflowsControl.RelationshipDesignerUserControl.Network.Refreshed += (s, e) =>
				{
					managementControl.WorkflowsGrid.IsListManagerNotNull = false;
					Application.DoEvents();
					refreshed = true;
				};

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, ContinueWithSave.Yes, form.FireSaveButton());

				AssertEquals(true, onSavedCalled);
				AssertEquals(true, refreshed);
			}
		}

		public void TestSaveForm_ShouldRefreshNetwork()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task = workflow1.Parent.WorkflowItems.AddNew();
			task.P9_Type = "UDF";
			task.P9_Description = "task";

			AssertEquals(false, jobHeader.HasErrors);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy })
			using (var managementControl = new WorkflowManagementUserControl())
			{
				form.Controls.Add(managementControl);
				form.Show();
				managementControl.SetDataBinding(viewModel, string.Empty);

				var workflowsControl = managementControl.FindAll<WorkflowsUserControl>().Single();

				var refreshed = false;
				var onSavedCalled = false;

				form.Saved += (s, e) => onSavedCalled = true;
				workflowsControl.RelationshipDesignerUserControl.Network.Refreshed += (s, e) => refreshed = true;

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, ContinueWithSave.Yes, form.FireSaveButton());

				AssertEquals(true, onSavedCalled);
				AssertEquals(true, refreshed);
			}
		}

		public void TestSaveForm_WhenNetworkDoesNotExist_ShouldNotRefreshNetwork()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task = workflow1.Parent.WorkflowItems.AddNew();
			task.P9_Type = "UDF";
			task.P9_Description = "task";

			AssertEquals(false, jobHeader.HasErrors);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var form = new ZForm(jobHeader) { ControllerID = DummyControllerIDs.Dummy })
			using (var managementControl = new WorkflowManagementUserControl())
			{
				form.Controls.Add(managementControl);
				form.Show();
				managementControl.SetDataBinding(viewModel, string.Empty);

				var workflowsControl = managementControl.FindAll<WorkflowsUserControl>().Single();

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, ContinueWithSave.Yes, form.FireSaveButton());

				AssertEquals("The network should not have been initialized.", null, workflowsControl.RelationshipDesignerUserControl.NetworkForTest);
			}
		}
#endif

		[ExpectNoExceptions]
		public void TestNullNetworkOnSavingNewWorkflowType()
		{
			var system = CreateSystem("ORG");
			var orgheader = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZOrganisationsForm(orgheader))
			{
				form.Show();
				orgheader.OH_FullName = ZString.Empty;
				orgheader.RunPreSaveValidation();
				TabPageNotificationsExposer.ExposeTabPageNotificationsOnIdle(form, orgheader);
				Application.DoEvents();
			}
		}

#if !WINZOR
		public void TestDeleteWorkflow_ShouldRefreshNetwork()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var refreshed = false;
				control.RelationshipDesignerUserControl.Network.Refreshed += (s, e) => refreshed = true;

				AssertEquals(3, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(1);
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, control.WorkflowsGrid.List.Count);

				AssertEquals("Should have refreshed network when deleted on the grid", true, refreshed);
			}
		}

		public void TestAddNewWorkflow_ShouldRefreshNetwork()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var refreshed = false;
				control.RelationshipDesignerUserControl.Network.Refreshed += (s, e) => refreshed = true;

				AssertEquals(2, jobHeader.ProcessHeaders.Count);
				AssertEquals(2, control.RelationshipDesignerUserControl.Network.Entities.Count);
				var newWorkflow = viewModel.AllProcessHeaders.AddNew();

				AssertEquals(3, jobHeader.ProcessHeaders.Count);
				AssertEquals(3, control.RelationshipDesignerUserControl.Network.Entities.Count);

				AssertCollectionContains(control.RelationshipDesignerUserControl.Network.Shapes, e => e == newWorkflow.GetDefaultShape((BMNCNShapeDefaultDiagram)control.RelationshipDesignerUserControl.Network.DiagramShape));

				AssertEquals("Should have refreshed network when deleted on the grid", true, refreshed);
			}
		}
#endif

		#endregion

		#region CancelMenuItem

		public void TestCancelMenuItem_WholeRowNotSelected()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task1_1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task2_1 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var task3_1 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.WorkflowsGrid.CurrentRowIndex = 1;

				control.WorkflowsGrid.OnPopup_CallForTesting();
				var cancelMenuItem = control.WorkflowsGrid.ContextMenu.MenuItems.FindByText("Cancel");
				AssertNotNull(cancelMenuItem);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				cancelMenuItem.PerformClick();

				AssertEquals("ASN", task1_1.P9_Status);
				AssertEquals("CAN", task2_1.P9_Status);
				AssertEquals("ASN", task3_1.P9_Status);
			}
		}

		public void TestCancelMenuItem_WholeRowSelected()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task1_1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task2_1 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var task3_1 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.WorkflowsGrid.CurrentRowIndex = 1;
				control.WorkflowsGrid.Select(1);

				control.WorkflowsGrid.OnPopup_CallForTesting();
				var cancelMenuItem = control.WorkflowsGrid.ContextMenu.MenuItems.FindByText("Cancel");
				AssertNotNull(cancelMenuItem);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				cancelMenuItem.PerformClick();

				AssertEquals("ASN", task1_1.P9_Status);
				AssertEquals("CAN", task2_1.P9_Status);
				AssertEquals("ASN", task3_1.P9_Status);
			}
		}

		public void TestCancelMenuItem_MultipleRowsSelected()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task1_1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task2_1 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var task3_1 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.WorkflowsGrid.CurrentRowIndex = 1;
				control.WorkflowsGrid.Select(1);
				control.WorkflowsGrid.Select(2);

				control.WorkflowsGrid.OnPopup_CallForTesting();
				var cancelMenuItem = control.WorkflowsGrid.ContextMenu.MenuItems.FindByText("Cancel");
				AssertNotNull(cancelMenuItem);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				cancelMenuItem.PerformClick();

				AssertEquals("ASN", task1_1.P9_Status);
				AssertEquals("CAN", task2_1.P9_Status);
				AssertEquals("CAN", task3_1.P9_Status);
			}
		}

		void TestCancelWorkflowWithCopySchedulesUsingScheduleDeactivatorForm(string buttonToClick, string workflowFinalStatus, string taskFinalStatus, bool copyScheduleActiveStatus)
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var workflow_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var workflow_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			workflow_copySchedule.SUC_CopyObjectId = workflow.PK;
			workflow_copyScheduleTask.S5_ParentID = workflow_copySchedule.PK;

			var task_copyScheduleTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			var task_copySchedule = Factory.NewWithValidTestData<StmUniversalCopy>();
			task_copySchedule.SUC_CopyObjectId = task.PK;
			task_copyScheduleTask.S5_ParentID = task_copySchedule.PK;

			Factory.Save();

			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				form.Show();

				control.WorkflowsGrid.CurrentRowIndex = 1;
				control.WorkflowsGrid.Select(1);

				control.WorkflowsGrid.OnPopup_CallForTesting();
				var cancelMenuItem = control.WorkflowsGrid.ContextMenu.MenuItems.FindByText("Cancel");
				AssertNotNull(cancelMenuItem);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var cancelWorkflowForm = f as CancelWorkflowForm;

					cancelWorkflowForm.Shown += (o, x_) =>
					{
						ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(s =>
						{
							var scheduleDeactivatorForm = s as ZChildForm;
							AssertEquals("ScheduleDeactivatorInvocationForm implements IScheduleDeactivatorView, but we want to avoid explictly referencing the form here",
								true, s is IScheduleDeactivatorView);

							scheduleDeactivatorForm.Shown += (_, y_) =>
							{
								var button = scheduleDeactivatorForm.FindAll<ZButton>(b => b.Text == buttonToClick).Single();
								button.PerformClick();
								Application.DoEvents();
							};
						});

						var okButton = cancelWorkflowForm.FindAll<ZButton>(b => b.Text == "OK").Single();
						okButton.PerformClick();
						Application.DoEvents();
					};
				});

				cancelMenuItem.PerformClick();
				Application.DoEvents();

				Factory.Save();

				AssertEquals(taskFinalStatus, task.P9_Status);
				AssertEquals(workflowFinalStatus, workflow.FH_Status);
				AssertEquals(copyScheduleActiveStatus, workflow_copyScheduleTask.S5_IsActive);
				AssertEquals(copyScheduleActiveStatus, task_copyScheduleTask.S5_IsActive);
			}
		}

		public void TestCancelWorkflowWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelWorkflowWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and deactivate", WorkflowStatusList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled, false);
		}

		public void TestCancelWorkflowWithCopySchedules_WhenUserSelectsCancelAndDoNotDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelWorkflowWithCopySchedulesUsingScheduleDeactivatorForm("Cancel and do not deactivate", WorkflowStatusList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled, true);
		}

		public void TestDoNotCancelWorkflowWithCopySchedules_WhenUserSelectsCancelAndDeactivate_OnScheduleDeactivatorForm()
		{
			TestCancelWorkflowWithCopySchedulesUsingScheduleDeactivatorForm("Do not cancel or deactivate", WorkflowStatusList.Codes.Open, ProcessTaskStatusCodeList.Codes.Assigned, true);
		}

		#endregion

#if !WINZOR

		#region Ribbon
		public void TestShouldNotShowRibbon()
		{
			BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var viewModel = new WorkflowManagementViewModel(jobHeader);

			using (var control = new WorkflowsUserControl())
			using (var form = new ZForm(viewModel))
			{
				control.SetDataBinding(viewModel, string.Empty);
				form.Controls.Add(control);
				using (new DisposableAction(() => form.Controls.Remove(control)))
				{
					form.Show();

					control.WorkflowNCNTabControl_Exposed.SelectedTab = control.NCNTabPage_Exposed; // need to select the tab to host the NetworkUserControl
					Application.DoEvents(); // need to pump the queue now to let NetworkUserControl SetDataContext()
					AssertEquals(false, control.RelationshipDesignerUserControl.NetworkUserControl.RibbonControlIsVisible_ForTesting);
				}
			}
		}

		#endregion

		#region Search

		public void TestShouldResetSearchResultsAfterRefresherEvent()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.Name = "Workflow";

			Factory.Save();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();
				form.SwitchToDefaultDiagramTab();
				Application.DoEvents();

				var formFactory = control.RelationshipDesignerUserControl.NetworkForTest.DiagramEntity.Factory;
				var loadedJobHeader = formFactory.Load<ProcessJobHeader>(jobHeader.PK);

				var defaultDiagram = loadedJobHeader.GetDefaultDiagram();

				var networkUserControl = control.RelationshipDesignerUserControl.NetworkUserControl;
				var networkViewModel = networkUserControl.NetworkViewModel;
				AssertEquals(1, networkViewModel.Nodes.Count());
				var node = networkViewModel.Nodes.Single();
				AssertEquals(false, node.IsSelected);

				var searchForm = networkUserControl.OpenFinderForm();

				var entitiesForResfreshEvent = new ShapeNetworkEntity[] { (ShapeNetworkEntity)node.Entity };

				foreach (var refreshEvent in ((RefreshType[])Enum.GetValues(typeof(RefreshType))).Except(new RefreshType[] { RefreshType.None }))
				{
					searchForm.SetSearchBox_ForTest("Workflow");
					searchForm.Search_PerformClick_ForTest();
					AssertEquals(true, node.IsSelected);
					AssertEquals(true, searchForm.HasPerformedSearch_ForTest);

					networkViewModel.GetJobNetwork().Refresh(refreshEvent, entitiesForResfreshEvent);
					AssertEquals("Should reset search results", false, searchForm.HasPerformedSearch_ForTest);
				}
			}
		}

		public void TestShouldBeAbleToSearchAfterAddingNewWorkflow()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.Name = "Workflow1";

			Factory.Save();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();
				form.SwitchToDefaultDiagramTab();
				Application.DoEvents();

				var formFactory = control.RelationshipDesignerUserControl.NetworkForTest.DiagramEntity.Factory;
				var loadedJobHeader = formFactory.Load<ProcessJobHeader>(jobHeader.PK);

				var defaultDiagram = loadedJobHeader.GetDefaultDiagram();

				var networkUserControl = control.RelationshipDesignerUserControl.NetworkUserControl;
				var networkViewModel = networkUserControl.NetworkViewModel;
				AssertEquals(1, networkViewModel.Nodes.Count());
				var node1 = networkViewModel.Nodes.Single();
				AssertEquals(false, node1.IsSelected);

				var searchForm = networkUserControl.OpenFinderForm();
				searchForm.SetSearchBox_ForTest("Workflow");
				searchForm.Search_PerformClick_ForTest();
				AssertEquals(true, node1.IsSelected);

				var workflow2 = jobHeader.ProcessHeaders.AddNew();
				workflow2.Name = "Workflow2";
				Application.DoEvents();

				var newNetworkViewModel = networkUserControl.NetworkViewModel;
				AssertNotEquals("New NetworkViewModel should be created during refresh", networkViewModel, newNetworkViewModel);
				AssertEquals(2, newNetworkViewModel.Nodes.Count());
				var newNode1 = newNetworkViewModel.Nodes.First();
				var newNode2 = newNetworkViewModel.Nodes.Last();

				AssertEquals(false, newNode1.IsSelected);
				AssertEquals(false, newNode2.IsSelected);

				searchForm.Search_PerformClick_ForTest();
				AssertEquals(true, newNode1.IsSelected);
				AssertEquals(false, newNode2.IsSelected);

				searchForm.Search_PerformClick_ForTest();
				AssertEquals(false, newNode1.IsSelected);
				AssertEquals(true, newNode2.IsSelected);
			}
		}

		#endregion
#endif

		#region Hide NCN Tab When Planning Management Disabled

		public void TestTabControl_WhenPlanningManagementEnabled_ShouldIncludeNCNTab()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			AssertNCNTabShown("The Workflow Relationship Designer tab should be available because Planning Management is enabled.", true);
		}

		public void TestTabControl_WhenBufferManagementWorkflowModeEnabled_ShouldNotIncludeNCNTab()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertNCNTabShown("The Workflow Relationship Designer tab should not be available because Planning Management is not enabled.", false);
		}

		public void TestTabControl_WhenEnhancedWorkflowManagementEnabled_ShoulNotdIncludeNCNTab()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertNCNTabShown("The Workflow Relationship Designer tab should not be available because Planning Management is not enabled.", false);
		}

		void AssertNCNTabShown(string message, bool shouldBeShown)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				Application.DoEvents();

				var workflowsControl = form.FindSingle<WorkflowsUserControl>();
				var tabs = workflowsControl.WorkflowNCNTabControl_Exposed.AllTabPages.Select(x => x.Text);
				var expected = shouldBeShown
					? new[] { "Workflows", "Workflow Relationship Designer", }
					: new[] { "Workflows", };

				AssertContainsExactElementsInAnyOrder(message, expected, tabs);
			}
		}

		#endregion
	}

	#region Linked Shapes Test

	class WorkflowsUserControl_LinkedShapesTest : BMSGUITestCase
	{
		#region Deleting via workflows grid

		public void TestDeleteWorkflow_WhenNotLinkedToShapes_ShouldNotPromptToUnlink()
		{
			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(true, workflow2.IsDeleted);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);
			}
		}

		public void TestDeleteWorkflow_WhenLinkedToDefaultDiagramShape_ShouldNotPromptToUnlink()
		{
			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultShape = workflow2.GetDefaultShape(defaultDiagram);

			defaultShape.BNS_Name = "Fiddlesticks";

			Factory.Save();

			AssertEquals("The default shape should have been persisted", true, defaultShape.IsInDatabase);

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(true, workflow2.IsDeleted);
				AssertEquals(true, defaultShape.IsDeleted);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);
			}
		}

		public void TestDeleteJobLevelWorkflow_WhenLinkedToDefaultDiagram_ShouldNotPromptToUnlink()
		{
			var defaultDiagram = jobHeader.GetDefaultDiagram();

			defaultDiagram.BNS_Name = "Fiddlesticks";

			Factory.Save();

			AssertEquals("The default diagram should have been persisted", true, defaultDiagram.IsInDatabase);

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(0);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(false, jobHeader.IsDeleted);
				AssertEquals(false, defaultDiagram.IsDeleted);

				AssertButtonStripNotificationShown("It's not currently possible to delete job-level workflows. But if it becomes possible, this dialog should handle that somehow.", control.DeleteWorkflowDialogWrapper, null, null);
				AssertEquals("The Job-level workflow cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteWorkflow_WhenLinkedToShapes_ShouldPromptToUnlink_ChooseResponseToUnLinkShapes()
		{
			CreateDiagramsForWorkflows();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertEquals(workflow2, shape2_2.ProcessHeader);

				control.DeleteWorkflowDialogWrapper.ResponseToFireForTest = DeleteWorkflowOption.UnLinkShapes;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(true, workflow2.IsDeleted);
				AssertEquals(false, shape1_2.IsDeleted);
				AssertEquals(false, shape2_2.IsDeleted);

				AssertEquals(false, arrow1_1.IsDeleted);
				AssertEquals(false, arrow1_2.IsDeleted);
				AssertEquals(false, arrow2_1.IsDeleted);
				AssertEquals(false, arrow2_2.IsDeleted);

				AssertNull(shape1_2.ProcessHeader);
				AssertNull(shape2_2.ProcessHeader);

				AssertNull(arrow1_1.ProcessHeaderLink);
				AssertNull(arrow1_2.ProcessHeaderLink);
				AssertNull(arrow2_1.ProcessHeaderLink);
				AssertNull(arrow2_2.ProcessHeaderLink);

				AssertNoExceptionThrown(Factory.Save);
				AssertStandardLinkedShapesMessageShown(control, linkedToMultipleShapes: true);
			}
		}

		public void TestDeleteWorkflow_WhenLinkedToShapes_ShouldPromptToUnlink_NotInitialisingDialogWrapperInTest()
		{
			CreateDiagramsForWorkflows();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertEquals(workflow2, shape2_2.ProcessHeader);

				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(true, workflow2.IsDeleted);
				AssertEquals(false, shape1_2.IsDeleted);
				AssertEquals(false, shape2_2.IsDeleted);

				AssertEquals(false, arrow1_1.IsDeleted);
				AssertEquals(false, arrow1_2.IsDeleted);
				AssertEquals(false, arrow2_1.IsDeleted);
				AssertEquals(false, arrow2_2.IsDeleted);

				AssertNull(shape1_2.ProcessHeader);
				AssertNull(shape2_2.ProcessHeader);

				AssertNull(arrow1_1.ProcessHeaderLink);
				AssertNull(arrow1_2.ProcessHeaderLink);
				AssertNull(arrow2_1.ProcessHeaderLink);
				AssertNull(arrow2_2.ProcessHeaderLink);

				AssertNoExceptionThrown(Factory.Save);
				AssertStandardLinkedShapesMessageShown(control, linkedToMultipleShapes: true);
			}
		}

		public void TestDeleteWorkflow_WhenLinkedToOneShape_ShouldPromptToUnlink_ChooseResponseToUnLinkShapes()
		{
			CreateDiagramsForWorkflows();

			((IBMNCNShape)shape2_2).DisconnectRelatedEntity();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertNull(shape2_2.ProcessHeader);

				control.DeleteWorkflowDialogWrapper.ResponseToFireForTest = DeleteWorkflowOption.UnLinkShapes;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(true, workflow2.IsDeleted);
				AssertEquals(false, shape1_2.IsDeleted);
				AssertEquals(false, shape2_2.IsDeleted);

				AssertEquals(false, arrow1_1.IsDeleted);
				AssertEquals(false, arrow1_2.IsDeleted);
				AssertEquals(false, arrow2_1.IsDeleted);
				AssertEquals(false, arrow2_2.IsDeleted);

				AssertNull(shape1_2.ProcessHeader);
				AssertNull(shape2_2.ProcessHeader);

				AssertNull(arrow1_1.ProcessHeaderLink);
				AssertNull(arrow1_2.ProcessHeaderLink);
				AssertNull(arrow2_1.ProcessHeaderLink);
				AssertNull(arrow2_2.ProcessHeaderLink);

				AssertNoExceptionThrown(Factory.Save);
				AssertStandardLinkedShapesMessageShown(control, linkedToMultipleShapes: false);
			}
		}

		public void TestDeleteWorkflow_WhenLinkedToShapes_ShouldPromptToUnlink_ChooseResponseToDeleteShapes()
		{
			CreateDiagramsForWorkflows();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertEquals(workflow2, shape2_2.ProcessHeader);

				control.DeleteWorkflowDialogWrapper.ResponseToFireForTest = DeleteWorkflowOption.DeleteShapes;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(true, workflow2.IsDeleted);
				AssertEquals(true, shape1_2.IsDeleted);
				AssertEquals(true, shape2_2.IsDeleted);

				AssertEquals(true, arrow1_1.IsDeleted);
				AssertEquals(true, arrow1_2.IsDeleted);
				AssertEquals(true, arrow2_1.IsDeleted);
				AssertEquals(true, arrow2_2.IsDeleted);

				AssertStandardLinkedShapesMessageShown(control, linkedToMultipleShapes: true);
			}
		}

		public void TestDeleteWorkflow_WhenLinkedToShapes_ShouldPromptToUnlink_ChooseCancelResponse()
		{
			CreateDiagramsForWorkflows();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				var openFormCount = Application.OpenForms.Count;

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertEquals(workflow2, shape2_2.ProcessHeader);

				control.DeleteWorkflowDialogWrapper.ResponseToFireForTest = DeleteWorkflowOption.CancelDelete;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				AssertEquals(false, workflow2.IsDeleted);
				AssertEquals(false, shape1_2.IsDeleted);
				AssertEquals(false, shape2_2.IsDeleted);

				AssertEquals(false, arrow1_1.IsDeleted);
				AssertEquals(false, arrow1_2.IsDeleted);
				AssertEquals(false, arrow2_1.IsDeleted);
				AssertEquals(false, arrow2_2.IsDeleted);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertEquals(workflow2, shape2_2.ProcessHeader);

				AssertStandardLinkedShapesMessageShown(control, linkedToMultipleShapes: true);
				AssertEquals(openFormCount, Application.OpenForms.Count);
			}
		}

		public void TestDeleteWorkflow_WhenLinkedToShapes_ShouldPromptToUnlink_ChooseCancelAndOpenDiagramsResponse()
		{
			CreateDiagramsForWorkflows();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();

				AssertEquals(4, control.WorkflowsGrid.List.Count);
				control.WorkflowsGrid.Select(2);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertEquals(workflow2, shape2_2.ProcessHeader);

				control.DeleteWorkflowDialogWrapper.ResponseToFireForTest = DeleteWorkflowOption.OpenDiagramsAndCancelDelete;
				control.WorkflowsGrid.DeleteMenuItem.PerformClick();

				var diagramForms = Application.OpenForms.OfType<ZForm>().Where(f => f.BusinessEntity is IBMNCNShape).ToArray();

				try
				{
					AssertEquals(false, workflow2.IsDeleted);
					AssertEquals(false, shape1_2.IsDeleted);
					AssertEquals(false, shape2_2.IsDeleted);

					AssertEquals(false, arrow1_1.IsDeleted);
					AssertEquals(false, arrow1_2.IsDeleted);
					AssertEquals(false, arrow2_1.IsDeleted);
					AssertEquals(false, arrow2_2.IsDeleted);

					AssertEquals(workflow2, shape1_2.ProcessHeader);
					AssertEquals(workflow2, shape2_2.ProcessHeader);

					AssertStandardLinkedShapesMessageShown(control, linkedToMultipleShapes: true);

					AssertEquals(2, diagramForms.Length);
					Assert(diagramForms.Any(f => f.BusinessEntity.Identifier == diagram1.PK));
					Assert(diagramForms.Any(f => f.BusinessEntity.Identifier == diagram2.PK));
				}
				finally
				{
					diagramForms.ForEach(f => f.Dispose());
				}
			}
		}

		#endregion

		#region Deleting via default diagram

#if !WINZOR
		public void TestDeleteWorkflowViaDefaultDiagram_WhenNotLinkedToNonDefaultShapes_ShouldNotPromptToUnlink()
		{
			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();
				form.SwitchToDefaultDiagramTab();
				Application.DoEvents();

				var defaultDiagramShape = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, workflow2.PK).AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.DefaultWorkflow));

				AssertNotNull(defaultDiagramShape);
				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				form.Network.DeleteEntity(form.Network[workflow2.FH_CompletionStatement]);

				AssertEquals(true, workflow2.IsDeleted);
				AssertEquals(true, defaultDiagramShape.IsDeleted);

				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);
			}
		}

		public void TestDeleteWorkflowViaDefaultDiagram_WhenLinkedToNonDefaultShapes_ShouldPromptToUnlink_ChooseResponseToUnLinkShapes()
		{
			CreateDiagramsForWorkflows();

			using (var form = new DummyFormWithWorkflowsControl(jobHeader))
			{
				var control = form.WorkflowsControl;

				form.Show();
				form.SwitchToDefaultDiagramTab();
				Application.DoEvents();

				var defaultDiagramShape = Factory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, workflow2.PK).AddToFilter(BMNCNShapeSchema.BNS_ShapeType, ShapeTypeList.Codes.DefaultWorkflow));

				AssertNotNull(defaultDiagramShape);
				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, null, null);

				AssertEquals(workflow2, shape1_2.ProcessHeader);
				AssertEquals(workflow2, shape2_2.ProcessHeader);

				control.DeleteWorkflowDialogWrapper.ResponseToFireForTest = DeleteWorkflowOption.UnLinkShapes;
				form.Network.DeleteEntity(form.Network[workflow2.FH_CompletionStatement]);

				AssertEquals(true, workflow2.IsDeleted);
				AssertEquals(false, shape1_2.IsDeleted);
				AssertEquals(false, shape2_2.IsDeleted);
				AssertEquals(true, defaultDiagramShape.IsDeleted);

				AssertEquals(false, arrow1_1.IsDeleted);
				AssertEquals(false, arrow1_2.IsDeleted);
				AssertEquals(false, arrow2_1.IsDeleted);
				AssertEquals(false, arrow2_2.IsDeleted);

				AssertNull(shape1_2.ProcessHeader);
				AssertNull(shape2_2.ProcessHeader);

				AssertNull(arrow1_1.ProcessHeaderLink);
				AssertNull(arrow1_2.ProcessHeaderLink);
				AssertNull(arrow2_1.ProcessHeaderLink);
				AssertNull(arrow2_2.ProcessHeaderLink);

				AssertNoExceptionThrown(Factory.Save);
				AssertStandardLinkedShapesMessageShown(control, linkedToMultipleShapes: true);
			}
		}
#endif

		#endregion

		#region Implementation

		ProcessJobHeader jobHeader;
		ProcessHeader workflow1, workflow2, workflow3;
		BMNCNShape diagram1, diagram2;
		BMNCNShape shape1_1, shape1_2, shape1_3, shape2_1, shape2_2, shape2_3;
		BMNCNAttachment arrow1_1, arrow1_2, arrow2_1, arrow2_2;

		protected override void SetUp()
		{
			base.SetUp();

			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "TAKEN");
			workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "TAKEN 2");
			workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "TAKEN 3");

			Factory.Save();
		}

		void CreateDiagramsForWorkflows()
		{
			diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Things that were");

			shape1_1 = NetworkTestCase.CreateShape(workflow1, diagram1);
			shape1_2 = NetworkTestCase.CreateShape(workflow2, diagram1);
			shape1_3 = NetworkTestCase.CreateShape(workflow3, diagram1);

			arrow1_1 = shape1_1.MakeVisiblePrerequisiteOf(shape1_2, diagram1);
			arrow1_2 = shape1_2.MakeVisiblePrerequisiteOf(shape1_3, diagram1);

			diagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Things that are");

			shape2_1 = NetworkTestCase.CreateShape(workflow1, diagram2);
			shape2_2 = NetworkTestCase.CreateShape(workflow2, diagram2);
			shape2_3 = NetworkTestCase.CreateShape(workflow3, diagram2);

			arrow2_1 = shape2_1.MakeVisiblePrerequisiteOf(shape2_2, diagram2);
			arrow2_2 = shape2_2.MakeVisiblePrerequisiteOf(shape2_3, diagram2);

			Factory.Save();
		}

		static void AssertStandardLinkedShapesMessageShown(WorkflowsUserControl control, bool linkedToMultipleShapes)
		{
			if (linkedToMultipleShapes)
			{
				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, "The workflow [TAKEN 2] is linked to shapes on the diagrams listed below. How would you like to handle this?\r\n\r\n• Things that are\r\n• Things that were", "Workflow is linked to shapes",
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Un-link Shapes", ToolTip = (NoResString)"Un-link this workflow from the shapes that currently refer to it.", Response = DeleteWorkflowOption.UnLinkShapes },
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Delete Shapes", ToolTip = (NoResString)"Delete all shapes that are linked to this workflow. Note that this could also remove arrows and nested shapes.", Response = DeleteWorkflowOption.DeleteShapes },
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Open Diagrams and Cancel Delete", ToolTip = (NoResString)"Don't delete this workflow, and also open the diagrams on which there are shapes linked to this workflow.", Response = DeleteWorkflowOption.OpenDiagramsAndCancelDelete },
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Cancel", ToolTip = (NoResString)"Don't delete this workflow.", Response = DeleteWorkflowOption.CancelDelete }
					);
			}
			else
			{
				AssertButtonStripNotificationShown(control.DeleteWorkflowDialogWrapper, "The workflow [TAKEN 2] is linked to a shape on the diagram listed below. How would you like to handle this?\r\n\r\n• Things that were", "Workflow is linked to a shape",
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Un-link Shape", ToolTip = (NoResString)"Un-link this workflow from the shape that currently refers to it.", Response = DeleteWorkflowOption.UnLinkShapes },
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Delete Shape", ToolTip = (NoResString)"Delete the shape that is linked to this workflow. Note that this could also remove arrows and nested shapes.", Response = DeleteWorkflowOption.DeleteShapes },
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Open Diagram and Cancel Delete", ToolTip = (NoResString)"Don't delete this workflow, and also open the diagram on which there is a shape linked to this workflow.", Response = DeleteWorkflowOption.OpenDiagramsAndCancelDelete },
					new ButtonStripAction<DeleteWorkflowOption> { Text = "Cancel", ToolTip = (NoResString)"Don't delete this workflow.", Response = DeleteWorkflowOption.CancelDelete }
					);
			}
		}

		#endregion
	}

	class DummyFormWithWorkflowsControl : ZForm
	{
		internal DummyFormWithWorkflowsControl(ProcessJobHeader jobHeader)
			: this(new WorkflowManagementViewModel(jobHeader))
		{
		}

		DummyFormWithWorkflowsControl(WorkflowManagementViewModel viewModel)
			: base(viewModel)
		{
			WorkflowsControl = new WorkflowsUserControl();
			WorkflowsControl.SetDataBinding(viewModel, string.Empty);

			Controls.Add(WorkflowsControl);
		}

		internal WorkflowsUserControl WorkflowsControl { get; }

		internal void SwitchToDefaultDiagramTab()
		{
			WorkflowsControl.WorkflowNCNTabControl_Exposed.SelectedTab = WorkflowsControl.NCNTabPage_Exposed;
		}

#if !WINZOR
		internal JobNetwork Network => WorkflowsControl.RelationshipDesignerUserControl.Network;
#endif
	}

	#endregion
}
