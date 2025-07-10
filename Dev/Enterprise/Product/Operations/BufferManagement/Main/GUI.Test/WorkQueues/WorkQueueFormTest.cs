using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkQueueForm))]
	class WorkQueueFormTest : ZFormBasherTest
	{
		#region RemoveMember

		public void TestRemoveMember_WhenNoItemSelected()
		{
			queue.AddMember(jobHeader);

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				AssertEquals(0, control.SelectedMembers.Count);

				control.RemoveSelectedMembers_ForTest();
				AssertEquals("Please select items from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRemoveMember_ShouldRemoveSelectedRows()
		{
			var link = queue.AddMember(jobHeader).Link;

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				var grid = control.FindAll<ZGrid>().Single();
				grid.Select(0);

				AssertEquals(1, control.SelectedMembers.Count);
				AssertEquals(link, control.SelectedMembers.FirstOrDefault());

				control.RemoveSelectedMembers_ForTest();
				AssertEquals("Remove the selected items from this queue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, link.IsDeleted);
			}
		}

		#endregion

		#region AddMember

		public void TestAddMember_RightLocation()
		{
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			queue.AddMember(workflow3);
			Factory.Save();

			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow3, workflow2, workflow1))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				((ZButton)control.Controls.Find("AddMemberButton", true)[0]).PerformClick();
				var listManager = form.FindAll<ZGrid>().First().ListManager;

				AssertEquals("Member should be in the correct location in the list", listManager.List[0], queue.Members.Single(m => m.TGL_Sequence == 3));
				AssertEquals("Member should be in the correct location in the list", listManager.List[1], queue.Members.Single(m => m.TGL_Sequence == 2));
				AssertEquals("Member should be in the correct location in the list", listManager.List[2], queue.Members.Single(m => m.TGL_Sequence == 1));
			}
		}

		public void TestAddMember_Security()
		{
			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesAddToQueue);

				using (var form = new WorkQueueForm(queue))
				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow1, workflow2))
				{
					form.Show();
					var control = form.FindAll<WorkQueueUserControl>().Single();
					((ZButton)control.Controls.Find("AddMemberButton", true)[0]).PerformClick();

					Assert("Member should not be in the list", !queue.ContainsMember(workflow1));
					Assert("Member should not be in the list", !queue.ContainsMember(workflow2));
					AssertStartsWith("Should start with correct message", "The selected items could not be added to the queue.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("You do not have permission to perform this action. (Organization (MAIORGSYD) - workflow1)", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("You do not have permission to perform this action. (Organization (MAIORGSYD) - workflow2)", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestRemoveMemberButton_Security()
		{
			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesRemoveFromQueue);

				using (var form = new WorkQueueForm(queue))
				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow1))
				{
					form.Show();
					var control = form.FindAll<WorkQueueUserControl>().Single();
					var grid = form.FindAll<ZGrid>().First();
					((ZButton)control.Controls.Find("AddMemberButton", true)[0]).PerformClick();
					grid.Select(0);

					Assert("Member should be in the list", queue.ContainsMember(workflow1));

					((ZButton)control.Controls.Find("RemoveMemberButton", true)[0]).PerformClick();

					Assert("Member should have been removed from the list", queue.ContainsMember(workflow1));
					AssertEquals(@"The selected item could not be removed from the queue.

You do not have permission to perform this action. (Organization (MAIORGSYD) - workflow1)", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestReSequencingClick()
		{
			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();

				using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow1, workflow2))
				{
					((ZButton)control.Controls.Find("AddMemberButton", true)[0]).PerformClick();
				}
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, queue.ContainsMember(jobHeader));
				AssertEquals(true, queue.ContainsMember(workflow1));
				AssertEquals(true, queue.ContainsMember(workflow2));

				var membersGrid = control.FindSingle<TagGrid>();
				var reSequenceMenuItem = (ZMenuItem)membersGrid.ContextMenu.MenuItems.FindByText("Re-Sequence");
				membersGrid.ContextMenu.ShowPopupMenu();
				AssertEquals(true, reSequenceMenuItem.Visible);
				AssertEquals(0, membersGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownDialogForTest = null;
				reSequenceMenuItem.PerformClick();
				AssertEquals("Please select two or more rows to re-sequence.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				membersGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownDialogForTest = null;
				reSequenceMenuItem.PerformClick();
				AssertType<ReSequencerForm>(ZFormModaliser.LastFormShownDialogForTest);

				using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
				{
					Env.Security.FindCheckPoint("triggerSecurityLoading");
					BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesResequenceQueue);
					membersGrid.ContextMenu.ShowPopupMenu();
					AssertEquals(false, reSequenceMenuItem.Visible);
				}
			}

			queue.SetReadOnlyIncludingChildren(true);
			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow1, workflow2))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				var membersGrid = control.FindSingle<TagGrid>();
				var reSequenceMenuItem = (ZMenuItem)membersGrid.ContextMenu.MenuItems.FindByText("Re-Sequence");
				membersGrid.ContextMenu.ShowPopupMenu();
				AssertEquals(false, reSequenceMenuItem.Visible);
			}
		}

		public void TestAddMember_EndToEnd()
		{
			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow1, workflow2))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();

				((ZButton)control.Controls.Find("AddMemberButton", true)[0]).PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, queue.ContainsMember(jobHeader));
				AssertEquals(true, queue.ContainsMember(workflow1));
				AssertEquals(true, queue.ContainsMember(workflow2));
			}
		}

		public void TestAddMember()
		{
			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(workflow1, workflow2))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();

				control.AddMembers_ForTest();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, queue.ContainsMember(jobHeader));
				AssertEquals(true, queue.ContainsMember(workflow1));
				AssertEquals(true, queue.ContainsMember(workflow2));
			}
		}

		public void TestAddMember_OneIsUnsuccessful()
		{
			queue.AddMember(workflow1);

			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(jobHeader))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();

				control.AddMembers_ForTest();
				AssertMultilineASCIIEquals("",
@"The selected item could not be added to the queue.

Cannot add the job of a workflow already in the queue. The following workflows are already present in this queue:
	workflow1 (Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.)
", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, queue.ContainsMember(jobHeader));
				AssertEquals(true, queue.ContainsMember(workflow1));
				AssertEquals(false, queue.ContainsMember(workflow2));
			}
		}

		public void TestAddMember_OneOfTwoIsUnsuccessful()
		{
			queue.AddMember(workflow1);

			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(jobHeader, workflow2))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();

				control.AddMembers_ForTest();
				AssertMultilineASCIIEquals("",
@"Some items could not be added to the queue.

Cannot add the job of a workflow already in the queue. The following workflows are already present in this queue:
	workflow1 (Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.)
", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(false, queue.ContainsMember(jobHeader));
				AssertEquals(true, queue.ContainsMember(workflow1));
				AssertEquals(true, queue.ContainsMember(workflow2));
			}
		}

		public void TestAddMember_SecondOfTwoIsUnsuccessfulDueToFirst()
		{
			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(jobHeader, workflow2))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();

				Application.DoEvents();

				control.AddMembers_ForTest();
				AssertMultilineASCIIEquals("",
@"Some items could not be added to the queue.

Cannot add a workflow of a job already in the queue. (Organization (MAIORGSYD) - workflow2)
", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(true, queue.ContainsMember(jobHeader));
				AssertEquals(false, queue.ContainsMember(workflow1));
				AssertEquals(false, queue.ContainsMember(workflow2));
			}
		}

		public void TestAddMember_AllAreUnsuccessful()
		{
			queue.AddMember(jobHeader);

			using (var form = new WorkQueueForm(queue))
			using (BusinessObjectModulePickerTest.SelectRecordsOnDialogShown(jobHeader, workflow2))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();

				control.AddMembers_ForTest();
				AssertMultilineASCIIEquals("",
@"The selected items could not be added to the queue.

Cannot add an item to same queue more than once. (Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.)
Cannot add a workflow of a job already in the queue. (Organization (MAIORGSYD) - workflow2)
", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(true, queue.ContainsMember(jobHeader));
				AssertEquals(false, queue.ContainsMember(workflow1));
				AssertEquals(false, queue.ContainsMember(workflow2));
			}
		}

		public void TestAddMember_BatchTagAdder()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory, addDefaultProcessHeaderIfNone: false);

			var wf1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var wf2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			string failureMessage;

			// Simulate add workflows to queue via TagAdder when WorkQueue form is not yet opened
			BatchTagOperator.TryAddTag(queue, new[] { wf1, wf2 }, out failureMessage);

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.WorkQueues).ShowEditForm(queue))
			{
				var wf3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
				var wf4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");

				// Simulate add workflows to queue via TagAdder when WorkQueue form is already opened
				BatchTagOperator.TryAddTag(queue, new[] { wf3, wf4 }, out failureMessage);

				Factory.Save();

				var formQueue = (WorkQueue)form.BusinessEntity;

				AssertEquals(4, formQueue.Members.Count);

				AssertNotNull(formQueue.Members.Single(m => m.TGL_Sequence == 1));
				AssertNotNull(formQueue.Members.Single(m => m.TGL_Sequence == 2));
				AssertNotNull(formQueue.Members.Single(m => m.TGL_Sequence == 3));
				AssertNotNull(formQueue.Members.Single(m => m.TGL_Sequence == 4));
			}
		}

		#endregion

		#region Form Caption

		public void TestFormCaption_New()
		{
			using (var form = new WorkQueueForm(Factory.New<WorkQueue>()))
			{
				form.Show();
				AssertEquals("Work Queue", form.FormCaption);
			}
		}

		public void TestFormCaption_Edit()
		{
			Factory.Save();

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				AssertEquals("Work Queue aaa", form.FormCaption);
			}
		}

		#endregion

		#region Opening grid items

		public void TestDoubleClickMembersGrid_ShouldOpenJobForm()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			queue.AddMember(workflow);

			Factory.Save();

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				var grid = control.FindAll<ZGrid>().Single();

				grid.Select(0);
				control.MembersGrid_DoubleClick(null, new MouseEventArgs(MouseButtons.Left, 2, 10, 30, 0));

				using (var openForm = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNotNull(openForm);
				}
			}
		}

		public void TestDoubleClickMembersGrid_WhenNoRowSelected()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			queue.AddMember(workflow);

			Factory.Save();

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				var grid = control.FindAll<ZGrid>().Single();

				control.MembersGrid_DoubleClick(null, new MouseEventArgs(MouseButtons.Left, 2, 10, 30, 0));
				using (var openForm = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNull(openForm);
				}
			}
		}

		public void TestPressF3MembersGrid_ShouldOpenJobForm()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			queue.AddMember(workflow);

			Factory.Save();

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				var grid = control.FindAll<ZGrid>().Single();

				grid.Select(0);
				var jobNumberColumnStyle = grid.Columns.FirstOrDefault(x => x.ColumnName == "Parent+ProviderJobNumber").ColumnStyle;
				grid.BeginEdit(jobNumberColumnStyle, 0);

				KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.F3);
				Application.DoEvents();

				using (var openForm = Application.OpenForms.OfType<ZOrganisationsForm>().SingleOrDefault())
				{
					AssertNotNull(openForm);
				}
			}
		}

		public void TestMembersGridVisibility()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			queue.AddMember(workflow);
			Factory.Save();

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				var control = form.FindAll<WorkQueueUserControl>().Single();
				var grid = control.FindAll<ZGrid>().Single();
				var columnNameList = new List<string>(new string[] { "StaffAssignedToNextStartableTask", "CapabilityAssignedToNextStartableTask", "JobCreatedBy", "JobCreatedDate", "JobCriteria1", "JobCriteria2", "JobCriteria3", "JobCriteria4", "JobCriteria5", "DescriptionOfTheFirstOpenWorkflowInTheJob", "CurrentComponentForTheFirstOpenWorkflowInTheJob" });
				foreach (var columnName in columnNameList)
				{
					var columnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(s => s.ColumnName == columnName);
					AssertNotNull($"The column name {columnInfo.ColumnName} cannot be null", columnInfo);
					AssertEquals($"The column name {columnInfo.ColumnName} should be not visible", false, columnInfo.IsVisible);
					AssertEquals($"The column name {columnInfo.ColumnName} should be available", false, columnInfo.IsUnavailable);
				}
			}
		}
		#endregion

		#region Validation

		public void TestLinks_WhenShownOnWorkQueueForm_ShouldEnableSequenceValidation()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "ABC", "ABC");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var link1 = (TagLink)workflow1.AddTag(queue).Link;
			var link2 = (TagLink)workflow2.AddTag(queue).Link;

			var queueLink1 = Factory.Load<WorkQueueMembershipLink>(link1.PK);
			var queueLink2 = Factory.Load<WorkQueueMembershipLink>(link2.PK);

			queueLink1.TGL_Sequence = 0;
			queueLink2.TGL_Sequence = 0;

			AssertEquals("The link is not yet shown on the Work Queue form, so sequence validation should not be enabled. SAD!", false, queueLink2.ShouldValidateSequenceUniqueness);
			AssertNoErrors("Sequence validation hasn't been enabled because the links aren't being shown on the Work Queue form, so there should be no errors even though the sequence is not unique. SAD!", queueLink2);

			using (var form = new WorkQueueForm(queue))
			{
				form.Show();
				Application.DoEvents();

				var member1 = form.DataSource.Members[0];
				var member2 = form.DataSource.Members[1];

				AssertEquals("The link is shown on the Work Queue form, so sequence validation should be enabled. SAD!", true, member1.ShouldValidateSequenceUniqueness);
				AssertEquals("The link is shown on the Work Queue form, so sequence validation should be enabled. SAD!", true, member2.ShouldValidateSequenceUniqueness);

				form.FireValidateAllForTest();

				AssertHasError("The link is shown on the Work Queue form, so it should validate its sequence number. SAD!", member1.TGL_SequenceInfo, "The Sequence has been duplicated and must be unique for each item in a Work Queue.");
				AssertHasError("The link is shown on the Work Queue form, so it should validate its sequence number. SAD!", member2.TGL_SequenceInfo, "The Sequence has been duplicated and must be unique for each item in a Work Queue.");

				member2.TGL_Sequence = 1;
				AssertNoErrors(member2);
			}
		}

		#endregion

		#region Db Hits

		public void TestOpeningWorkQueueWithWorkflows_DbHits()
		{
			var queue = Factory.NewWithValidTestData<WorkQueue>();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "job header 1");
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "workflow 1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "job header 2");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "workflow 2");

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "job header 3");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader3, "workflow 3");

			queue.AddMember(workflow1);
			queue.AddMember(workflow2);
			queue.AddMember(workflow3);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedQueue = newFactory.Load<WorkQueue>(queue.PK);

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessHeaderSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, thresholdForUnspecified: 99))
			using (var form = new WorkQueueForm(loadedQueue))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestOpeningWorkQueueWithJobHeader_DbHits()
		{
			var queue = Factory.NewWithValidTestData<WorkQueue>();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "job header 1");
			var workflow1_1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "workflow 1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "workflow 1_2");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "job header 2");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "workflow 2");

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "job header 3");
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader3, "workflow 3");

			queue.AddMember(jobHeader1);
			queue.AddMember(workflow2);
			queue.AddMember(workflow3);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedQueue = newFactory.Load<WorkQueue>(queue.PK);

			var expectedHits = new Dictionary<string, int>
			{
				{ ProcessHeaderSchema.Constants.TableName, 3 },
			};

			using (AssertDbHitsForAllFactories(expectedHits, ignoreUnspecified: true, thresholdForUnspecified: 99))
			using (var form = new WorkQueueForm(loadedQueue))
			{
				form.Show();
				Application.DoEvents();
			}
		}

		#endregion

		#region Implementation

		WorkQueue queue;
		ProcessJobHeader jobHeader;
		ProcessHeader workflow1, workflow2;

		protected override void SetUp()
		{
			base.SetUp();

			var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "MAIORGSYD");
			if (org == null) // Because form basher explicitly calls this method twice on different instances. :'(
			{
				org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "MAIORGSYD";

				queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

				var otherFactory = Factory.CreateNewFactory();
				jobHeader = ProcessJobHeader.GetForParent(org, otherFactory, addDefaultProcessHeaderIfNone: false);
				workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
				workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

				otherFactory.Save();
				Factory.Save();
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new WorkQueueForm(Factory.New<WorkQueue>());
		}

		#endregion
	}
}
