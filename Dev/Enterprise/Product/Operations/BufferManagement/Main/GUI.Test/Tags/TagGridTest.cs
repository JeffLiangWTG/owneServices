using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TagGridTest : BMSTestCaseWithFactory
	{
		#region Tag Management Form

		public void TestDoubleClickColumnHeader_WithInvalidTag_ShouldNotShowForm()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Liam Edwards-Playne", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				form.Show();
				Application.DoEvents();

				var tagGrid = form.FindAll<TagGrid>().Single();
				var formJob = (OrgHeader)form.BusinessEntity;

				tagGrid.ListManager.AddNew();

				AssertEquals("An uncommitted tag link row should be present", 1, tagGrid.ListManager.Count);
				AssertEquals(false, formJob.HasChanges);

				DoubleClickTagGridRow(form, -1);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoubleClickColumnHeader_WithValidTag_ShouldNotShowForm()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Liam Edwards-Playne", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			workflow.AddTag(config.PrincessCelestiaTag);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				form.Show();
				Application.DoEvents();

				var tagGrid = form.FindAll<TagGrid>().Single();
				var formJob = (OrgHeader)form.BusinessEntity;

				formJob.OH_Code = "LIAM!!!";

				AssertEquals(true, formJob.HasChanges);
				AssertEquals("A committed tag link row should be present", 1, tagGrid.ListManager.Count);

				DoubleClickTagGridRow(form, -1);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoubleClick_WhenNoTagRowsPresent_ShouldNotExplode()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Liam Edwards-Playne", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				form.Show();
				Application.DoEvents();

				var tagGrid = form.FindAll<TagGrid>().Single();

				AssertEquals("No uncommitted tag link row should be present", 0, tagGrid.ListManager.Count);

				DoubleClickTagGridRow(form, 0);

				AssertEquals("Please select a valid row.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickDeleteMenu_AfterFocusingOnOtherElement_DoesNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: true);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Test Completion Statement", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "TST", "I like tests...");

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				form.Show();
				Application.DoEvents();

				var controls = form.FindAll<Control>();
				var tagGrid = form.FindAll<TagGrid>().Single();

				tagGrid.ListManager.AddNew();
				SetTagGroupInGrid(tagGrid, 0, BMConstants.WorkQueuesTagGroupCode);
				SetTagMagnitudeInGrid(tagGrid, 0, "TST");

				AssertEquals("Tag group should be the work queue code", (string)tagGrid[0, 0], BMConstants.WorkQueuesTagGroupCode);
				AssertEquals("Tag magnitude should be the work queue.", (string)tagGrid[0, 1], "TST");
				AssertEquals("1 uncommitted row should exist.", tagGrid.ListManager.Count, 1);

				var taskGrid = form.FindAll<TaskDetailsUserControl>().First();
				taskGrid.Focus();
				tagGrid.Focus();

				AssertNoExceptionThrown("Deleting empty row should do neither do anything or throw anything", () => tagGrid.DeleteMenuItem.PerformClick());
				AssertEquals("Row should have been deleted.", tagGrid.ListManager.Count, 0);
			}
		}

		public void TestClickDeleteMenu_OnEmptyTagRow_DoesNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "MTH", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				form.Show();
				Application.DoEvents();

				var tagGrid = form.FindAll<TagGrid>().Single();

				AssertNoExceptionThrown("Deleting empty row should do neither do anything or throw anything", () => tagGrid.DeleteMenuItem.PerformClick());
			}
		}

		public void TestDoubleClick_WhenNoTagRowsPresent_ChangesExistOnJob_ShouldNotExplode()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Liam Edwards-Playne", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				form.Show();
				Application.DoEvents();

				var tagGrid = form.FindAll<TagGrid>().Single();
				var formJob = (OrgHeader)form.BusinessEntity;

				formJob.OH_Code = "LIAM!!!";

				AssertEquals(true, formJob.HasChanges);
				AssertEquals("No uncommitted tag link row should be present", 0, tagGrid.ListManager.Count);

				DoubleClickTagGridRow(form, 0);
				AssertEquals("Please select a valid row.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoubleClick_WhenInvalidTagRowPresent_ShouldNotExplode()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Liam Edwards-Playne", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			using (var form = (ZForm)WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(task))
			{
				form.Show();
				Application.DoEvents();

				var tagGrid = form.FindAll<TagGrid>().Single();

				tagGrid.ListManager.AddNew();

				AssertEquals("An uncommitted tag link row should be present", 1, tagGrid.ListManager.Count);

				DoubleClickTagGridRow(form, 0);
				AssertEquals("There are errors that need to be corrected before this Tag can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestTagManagementForm_DoesntOpenWithUnknownTagFields()
		{
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			var unknownTagName = "UNK";
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var tagGrid = form.TagGrid;
				SetTagMagnitudeInGrid(tagGrid, 0, unknownTagName);

				DoubleClickTagGridRow(form, 0);
				Assert("Tag form shouldn't open for unknown tag magnitude", Application.OpenForms.OfType<TagDefinitionForm>().ToArray().Length == 0);

				SetTagMagnitudeInGrid(tagGrid, 0, "");
				SetTagGroupInGrid(tagGrid, 0, unknownTagName);
				DoubleClickTagGridRow(form, 0);
				Assert("Tag form shouldn't open for unknown tag group", Application.OpenForms.OfType<TagDefinitionForm>().ToArray().Length == 0);
			}
		}

		public void TestTagManagementForm_DoesntOpenWithEmptyTagMagnitude()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var tagGrid = form.TagGrid;
				SetTagGroupInGrid(tagGrid, 0, tagGroup.TGD_Code);
				Assert("Tag group should be set", (string)tagGrid[0, 0] == tagGroup.TGD_Code);
				Assert(tagGrid[0, 1] != null);
				Assert("Tag magnitude should be empty", string.IsNullOrEmpty((string)tagGrid[0, 1]));
				Application.DoEvents();

				DoubleClickTagGridRow(form, 0);
				Assert("Tag form shouldn't open for tag group with no magnitude", Application.OpenForms.OfType<TagDefinitionForm>().ToArray().Length == 0);
			}
		}

		public void TestTagManagementForm_DoesntOpenIfDialogResultIsNotYes()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var tagGrid = form.TagGrid;
				SetTagMagnitudeInGrid(tagGrid, 0, tagMagnitude.TGM_Code);
				Assert("Tag magnitude should be set", (string)tagGrid[0, 1] == tagMagnitude.TGM_Code);

				DoubleClickTagGridRow(form, 0);
				Assert("Tag management form shouldn't have been opened", Application.OpenForms.OfType<TagDefinitionForm>().ToArray().Length == 0);
			}
		}

		public void TestTagManagementForm_InfersTagGroupFromTagMagnitudeAndOpensForm()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var tagGrid = form.TagGrid;
				SetTagMagnitudeInGrid(tagGrid, 0, tagMagnitude.TGM_Code);
				SetTagGroupInGrid(tagGrid, 0, string.Empty);
				Assert("Tag magnitude should be set", (string)tagGrid[0, 1] == tagMagnitude.TGM_Code);

				DoubleClickTagGridRow(form, 0);
				Assert("Tag group should be inferred automatically from the tag magnitude", (string)tagGrid[0, 0] == tagGroup.TGD_Code);
				var tagDefinitionForm = Application.OpenForms.OfType<TagDefinitionForm>().First();
				AssertNotNull("Tag management form should be successfully opened", tagDefinitionForm);

				tagDefinitionForm.Close();
			}
		}

		public void TestTagManagementForm_OnlyShowsOneRowAsSelected()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tag1 = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var tag2 = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "TIM", "Tim Dog", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			workflow.AddTag(tag1);
			workflow.AddTag(tag2);
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var tagGrid = form.TagGrid;
				DoubleClickTagGridRow(form, tagGrid.ListManager.List.IndexOf(workflow.TagLinks_ForBinding.Single(l => l.TagMagnitude == tag1)));
				DoubleClickTagGridRow(form, tagGrid.ListManager.List.IndexOf(workflow.TagLinks_ForBinding.Single(l => l.TagMagnitude == tag2)));

				var tagDefinitionForm = Application.OpenForms.OfType<TagDefinitionForm>().First();
				Assert("Only one tag form was opened for the tag group", Application.OpenForms.OfType<TagDefinitionForm>().ToArray().Length == 1);

				var tagGridInDefinitionForm = (tagDefinitionForm.Controls.Find("tagDefinitionControl1", true)[0] as TagDefinitionControl).TagMagnitudeGrid;
				Assert("Only one row in the tag grid should be selected", tagGridInDefinitionForm.SelectedRowCount == 1);
				AssertEquals("The last clicked tag should be selected in the tag grid", (tagGridInDefinitionForm.ListManager.GetCurrent() as TagMagnitude).PK, tag2.PK);

				tagDefinitionForm.Close();
			}
		}

		public void TestTagManagementForm_OpensOnGridRowDoubleClick()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tagMagnitude1 = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var tagMagnitude2 = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "TIM", "Tim Dog", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			workflow.AddTag(tagMagnitude1);
			workflow.AddTag(tagMagnitude2);
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();

				var tagGrid = form.TagGrid;
				DoubleClickTagGridRow(form, tagGrid.ListManager.List.IndexOf(workflow.TagLinks_ForBinding.Single(l => l.TagMagnitude == tagMagnitude2)));
				var tagDefinitionForm = Application.OpenForms.OfType<TagDefinitionForm>().First();
				AssertNotNull("Tag should be double-clicked and editing form opened successfully", tagDefinitionForm);

				AssertEquals("Row in tag grid should still be selected, even after editing form is opened.", 1, tagGrid.SelectedRowCount);

				var tagGridInDefinitionForm = (tagDefinitionForm.Controls.Find("tagDefinitionControl1", true)[0] as TagDefinitionControl).TagMagnitudeGrid;
				AssertEquals("The clicked/opened tag should be selected in the tag grid", (tagGridInDefinitionForm.ListManager.GetCurrent() as TagMagnitude).PK, tagMagnitude2.PK);

				tagDefinitionForm.Close();
			}
		}

		public void TestTagManagementForm_EditSecurity()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tagMagnitude1 = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			workflow.AddTag(tagMagnitude1);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Nick Beith";
			staff.GS_Code = "NIC";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.TagDefinitionEdit.IsAllowed = false;

				DoubleClickTagGridRow(form, 1);
				var tagDefinitionForm = Application.OpenForms.OfType<TagDefinitionForm>().First();
				AssertNotNull("Tag should be double-clicked and editing form opened successfully", tagDefinitionForm);
				AssertEquals("Form should not be editable due to security controls", ODisplayMode.ReadOnly, tagDefinitionForm.DisplayMode);

				tagDefinitionForm.Close();
			}
		}

		public void TestTagManagementForm_ViewSecurity()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tagMagnitude1 = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			workflow.AddTag(tagMagnitude1);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Nick Beith";
			staff.GS_Code = "NIC";

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.TagDefinitionEdit.IsAllowed = false;
				Env.Security.TagDefinitionView.IsAllowed = false;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				DoubleClickTagGridRow(form, 1);
				AssertEquals("Access Denied: Tag Groups -> View", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestTagManagementForm_ShowsFeedbackWhenValidationFailsAfterRowDoubleClick()
		{
			var tagGroup = VisualBoardsTestHelper.CreateTagDefinition(Factory, "DOG", "For canine-related hip hop artists");
			var tagMagnitude = VisualBoardsTestHelper.CreateTagMagnitude(tagGroup, "SNP", "Snoop Dogg", nudge: 20);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				DoubleClickTagGridRow(form, 0);
				AssertEquals("Should show a message when the entity has a validation error, asking user to correct that before opening the form", "There are errors that need to be corrected before this Tag can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Tag management form shouldn't have been opened", Application.OpenForms.OfType<TagDefinitionForm>().ToArray().Length == 0);
			}
		}

		#endregion

		#region Work Queue Form

		public void TestWorkQueueForm_DoesntOpenWithUnknownTagFields()
		{
			var workflow = VisualBoardsTestHelper.CreateWorkflow(VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory), "Completion statement");
			var unknownTagName = "UNK";

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var tagGrid = form.TagGrid;
				SetTagGroupInGrid(tagGrid, 0, BMConstants.WorkQueuesTagGroupCode);
				SetTagMagnitudeInGrid(tagGrid, 0, unknownTagName);

				DoubleClickTagGridRow(form, 0);
				Assert("Work queue form shouldn't open for unknown tag magnitude", Application.OpenForms.OfType<WorkQueueForm>().ToArray().Length == 0);

				SetTagMagnitudeInGrid(tagGrid, 0, "");
				SetTagGroupInGrid(tagGrid, 0, unknownTagName);

				DoubleClickTagGridRow(form, 0);
				Assert("Work queue form shouldn't open for unknown tag group", Application.OpenForms.OfType<WorkQueueForm>().ToArray().Length == 0);
			}
		}

		public void TestWorkQueueForm_DoesntOpenWithEmptyTagMagnitude()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Description");
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var tagGrid = form.TagGrid;
				SetTagGroupInGrid(tagGrid, 0, BMConstants.WorkQueuesTagGroupCode);
				Assert("Tag group should be the work queue code", (string)tagGrid[0, 0] == BMConstants.WorkQueuesTagGroupCode);
				Assert(tagGrid[0, 1] != null); 
				Assert("Tag magnitude should be empty", string.IsNullOrEmpty((string)tagGrid[0, 1]));
				DoubleClickTagGridRow(form, 0);
				Assert("Work queue form shouldn't open for tag group with no magnitude", Application.OpenForms.OfType<WorkQueueForm>().ToArray().Length == 0);
			}
		}

		public void TestWorkQueueForm_DoesntOpenIfDialogResultIsNotYes()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Description");
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var tagGrid = form.TagGrid;
				SetTagMagnitudeInGrid(tagGrid, 0, queue.TGM_Code);
				Assert("Tag magnitude should be set", (string)tagGrid[0, 1] == queue.TGM_Code);

				DoubleClickTagGridRow(form, 0);
				Assert("Work queue form shouldn't have been opened", Application.OpenForms.OfType<WorkQueueForm>().ToArray().Length == 0);
			}
		}

		public void TestWorkQueueForm_InfersTagGroupFromTagMagnitudeAndOpensForm()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Description");
			workflow.AddTag(queue);
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var tagGrid = form.TagGrid;
				SetTagMagnitudeInGrid(tagGrid, 0, queue.TGM_Code);
				Assert("Tag magnitude should be set", (string)tagGrid[0, 1] == queue.TGM_Code);

				DoubleClickTagGridRow(form, 0);
				Assert("Work queues tag group should be inferred automatically from the tag magnitude", (string)tagGrid[0, 0] == BMConstants.WorkQueuesTagGroupCode);
				var workQueueForm = Application.OpenForms.OfType<WorkQueueForm>().First();
				AssertNotNull("Work queue form should be successfully opened", workQueueForm);

				workQueueForm.Close();
			}
		}

		public void TestWorkQueueForm_OpensOnGridRowDoubleClick()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Description");
			queue.AddMember(workflow);
			workflow.AddTag(queue);
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();

				DoubleClickTagGridRow(form, 0);
				var workQueueForm = Application.OpenForms.OfType<WorkQueueForm>().First();
				AssertNotNull("Tag should be double-clicked and editing form opened successfully", workQueueForm);

				workQueueForm.Close();
			}
		}

		public void TestWorkQueueForm_Security()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Description");
			queue.AddMember(workflow);
			workflow.AddTag(queue);
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();

				Env.Security.WorkQueuesEdit.IsAllowed = false;
				Env.Security.TagDefinitionEdit.IsAllowed = false;

				DoubleClickTagGridRow(form, 0);
				var workQueueForm = Application.OpenForms.OfType<WorkQueueForm>().First();
				AssertNotNull("Tag should be double-clicked and work queue form opened successfully", workQueueForm);
				AssertEquals("Form should not be editable due to security controls", ODisplayMode.ReadOnly, workQueueForm.DisplayMode);

				workQueueForm.Close();
			}
		}

		public void TestWorkQueueForm_ShowsFeedbackWhenValidationFailsAfterRowDoubleClick()
		{
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "workflow");
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Description");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "Description");
			Factory.Save();

			using (var form = new TagGridFormForTesting(workflow.TagLinks_ForBinding))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var tagGrid = form.TagGrid;
				SetTagGroupInGrid(tagGrid, 0, BMConstants.WorkQueuesTagGroupCode);
				Assert("Tag group should be the work queue code", (string)tagGrid[0, 0] == BMConstants.WorkQueuesTagGroupCode);
				Assert(tagGrid[0, 1] != null);
				Assert("Tag magnitude should be empty", string.IsNullOrEmpty((string)tagGrid[0, 1]));
				DoubleClickTagGridRow(form, 0);
				AssertEquals("Should show a message when the entity has a validation error, asking user to correct that before opening the form", "There are errors that need to be corrected before this Tag can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Work queue form shouldn't have been opened", Application.OpenForms.OfType<WorkQueueForm>().ToArray().Length == 0);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestCaseWithFactory.EnableBMSInRegistry();
		}

		static void DoubleClickTagGridRow(ZForm form, int selectedRowIndex)
		{
			var tagGrid = form.FindAll<TagGrid>().Single();
			tagGrid.CurrentRowIndex = selectedRowIndex;
			tagGrid.PerformMouseDoubleClickForTest(selectedRowIndex, 0);
		}

		static void SetTagGroupInGrid(TagGrid tagGrid, int row, string tagGroupValue)
		{
			var tagGroupDropEdit = (ZGuidDropEditColumnStyle)tagGrid.TableStyles[0].GridColumnStyles[0];
			tagGrid.BeginEdit(tagGroupDropEdit, row);
			tagGroupDropEdit.EditControl.Text = tagGroupValue;
			tagGrid.EndEdit(tagGroupDropEdit, row, false);
		}

		static void SetTagMagnitudeInGrid(TagGrid tagGrid, int row, string tagMagnitudeValue)
		{
			var tagMagnitudeDropEdit = (ZGuidDropEditColumnStyle)tagGrid.TableStyles[0].GridColumnStyles[1];
			tagGrid.BeginEdit(tagMagnitudeDropEdit, row);
			tagMagnitudeDropEdit.EditControl.Text = tagMagnitudeValue;
			tagGrid.EndEdit(tagMagnitudeDropEdit, row, false);
		}

		#endregion
	}
}
