using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class EDIWorkTaskRelatedItemUserControlTest : ProcessManagement.GUI.Test.WorkTaskRelatedItemUserControlTest
	{
		#region EDIFormForTest

		class EDIFormForTest : FormForTest
		{
			public EDIFormForTest(object workItem, bool isViewOrDeleteMode) : base(workItem, isViewOrDeleteMode)
			{
			}

			protected override IWorkTaskRelatedItemUserControlForTest GetWorkTaskRelatedItemUserControlForTest() => new EDIWorkTaskRelatedItemUserControlForTest(SetToViewOrDeleteMode);

			public new EDIWorkTaskRelatedItemUserControlForTest WorkTaskRelatedItemUserControl => (EDIWorkTaskRelatedItemUserControlForTest)UserControl;
		}

		class EDIWorkTaskRelatedItemUserControlForTest : EDIWorkTaskRelatedItemUserControl, IWorkTaskRelatedItemUserControlForTest
		{
			public EDIWorkTaskRelatedItemUserControlForTest(bool isViewOrDeleteMode) : base(isViewOrDeleteMode)
			{
			}

			public new ZGroupBox RelatedItemGroupBox => base.RelatedItemGroupBox;
			public new ZGrid RelatedItemGrid => base.RelatedItemGrid;
			public new ZButton DetachButton => base.DetachButton;
			public new ZButton EditButton => base.EditButton;
			public new ZButton AttachButton => base.AttachButton;
			public new ZButton NewButton => base.NewButton;

			public new ZGroupBox NetworkDiagramGroupBox => base.NetworkDiagramGroupBox;
			public new ZGroupBox ParentWorkflowGroupBox => base.ParentWorkflowGroupBox;
			public new ZGroupBox ChildWorkflowGroupBox => base.ChildWorkflowGroupBox;

			public new ZButton CreateIncidentsButton => base.CreateIncidentsButton;
			public ToolStripMenuItem AttachIncidentMenuItem => FindAttachMenuItem("Incident");
			public ToolStripMenuItem NewIncidentMenuItem => FindNewMenuItem("Incident");
			public ToolStripMenuItem NewWorkItemMenuItem => FindNewMenuItem("Work Item");

			public void AttachProjectButton_Click()
			{
				FindMenuItem("Project").PerformClick();
			}

			public bool NewButton_Click(string text)
			{
				foreach (ToolStripMenuItem item in menuStripNew.Items)
				{
					if (item.Text == text)
					{
						item.PerformClick();
						return true;
					}
				}

				return false;
			}

			ToolStripMenuItem FindAttachMenuItem(string text)
			{
				ToolStripMenuItem result = null;
				foreach (ToolStripMenuItem item in menuStripAttach.Items)
				{
					if (item.Text == text)
					{
						result = item;
						break;
					}
				}

				return result;
			}

			ToolStripMenuItem FindNewMenuItem(string text)
			{
				ToolStripMenuItem result = null;
				foreach (ToolStripMenuItem item in menuStripNew.Items)
				{
					if (item.Text == text)
					{
						result = item;
						break;
					}
				}

				return result;
			}

			ToolStripMenuItem FindMenuItem(string text)
			{
				ToolStripMenuItem result = null;
				foreach (ToolStripMenuItem item in menuStripAttach.Items)
				{
					if (item.Text == text)
					{
						result = item;
						break;
					}
				}
				return result;
			}
		}

		#endregion

		public void TestCreateIncidents()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CreateAndLoadLicenceForOrg();
			org1.LicCompany.LicEnterprise.LE_EnterpriseCode = "AAA";
			org1.LicCompany.LC_CompanyCode = "111";
			var database1 = org1.LicCompany.LicDatabases.AddNew();
			var licence1 = org1.LicCompany.GetHeader(database1);
			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_LD = database1.PK;
			clientCompany1.LCC_OH = org1.PK;
			org2.CreateAndLoadLicenceForOrg();
			org1.LicCompany.LicEnterprise.LE_EnterpriseCode = "BBB";
			org1.LicCompany.LC_CompanyCode = "222";
			var database2 = org2.LicCompany.LicDatabases.AddNew();
			var licence2 = org2.LicCompany.GetHeader(database2);
			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_LD = database2.PK;
			clientCompany2.LCC_OH = org2.PK;
			org1.OH_FullName = "Foo";
			org2.OH_FullName = "Bar";
			Factory.Save();
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			using (var form = GetEDIFormForTest(workItem, false))
			{
				form.Show();
				var issue1 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
				var issue2 = Factory.NewWithValidTestData<EdiHelpErrorLog>();
				issue1.Occurrences.AddNew().HO_LD = licence1.LA_LD;
				issue2.Occurrences.AddNew().HO_LD = licence2.LA_LD;
				workItem.RelatedItems.AddRange(issue1, issue2);
				AssertEquals(2, workItem.RelatedItems.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.WorkTaskRelatedItemUserControl.CreateIncidentsButton.PerformClick();
				AssertEquals("The confirmation message should be shown.", "This operation may create lots of incidents, do you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, workItem.RelatedItems.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.WorkTaskRelatedItemUserControl.CreateIncidentsButton.PerformClick();
				AssertEquals("The confirmation message should be shown.", "This operation may create lots of incidents, do you want to continue?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals(4, workItem.RelatedItems.Count);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("The following incidents have been created:"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.WorkTaskRelatedItemUserControl.CreateIncidentsButton.PerformClick();
				AssertEquals("No new incidents were created. Either there are no attached issues or all the client databases that the issues belong to already have a related work item attached to the issues.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(4, workItem.RelatedItems.Count);
			}
		}

		public void TestAttachIncident()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			using (var form = GetEDIFormForTest(incident, false))
			{
				form.Show();
				var button = form.WorkTaskRelatedItemUserControl.AttachIncidentMenuItem;
				AssertNotNull("Should have Attach Incident button", button);
				button.PerformClick();
				var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				AssertEquals(typeof(SupportIncidentModule), modulePopup.Module_ForTest.GetType());
				AssertEquals("Incidents", modulePopup.FormCaption);
			}
		}

		public void TestAttachIncidentFromIncidentGroupRelatedItems()
		{
			var incident = Factory.New<SupportIncident>();
			var group = Factory.New<IncidentManagementGroup>();

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00FMT01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			group.AutoCascadeRelatedItems.Add(workItemCascade);
			Factory.Save();

			using (var form = GetEDIFormForTest(group, false))
			{
				form.Show();
				var button = form.WorkTaskRelatedItemUserControl.AttachIncidentMenuItem;
				AssertNotNull("Should have Attach Incident button", button);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				button.PerformClick();
				var modulePopup = (EmbeddedModulePopup)ZFormModaliser.LastFormShownForTest;
				AssertEquals(typeof(SupportIncidentModule), modulePopup.Module_ForTest.GetType());
				AssertEquals("Incidents", modulePopup.FormCaption);
				modulePopup.Module_ForTest.PerformSearch_ForTest();
				modulePopup.Module_ForTest.DisplayGrid.SelectAllElements();
				AssertEquals(1, modulePopup.Module_ForTest.DisplayGrid.SelectedRowCount);
				modulePopup.ExposedOKButtonForTesting.PerformClick();
				form.FireSaveButton();
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SetAllColumnsVisible(true);
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SelectAllElements();
				group.Reload();
				incident.Reload();
				AssertEquals(0, group.LinkedIncidents.Count);
				AssertEquals(2, group.RelatedItems.Count);
				AssertCollectionNotContains("Should not add WI in incident", workItemCascade, incident.RelatedItems);

				group.SendBroadcastMessage(incident.IncidentManagementLink, message);

				Factory.Save();
				AssertEquals(0, incident.EConversation.Conversation.Messages.Count);
			}
		}

		public void TestNewIncidentFromIncidentGroupRelatedItems()
		{
			var incident = Factory.New<SupportIncident>();
			var group = Factory.New<IncidentManagementGroup>();

			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			workItemCascade.WKI_WorkItemNumber = "WI00FMT01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();

			group.AutoCascadeRelatedItems.Add(workItemCascade);
			Factory.Save();

			using (var form = GetEDIFormForTest(group, false))
			{
				form.Show();
				var menuItem = form.WorkTaskRelatedItemUserControl.NewIncidentMenuItem;
				AssertNotNull("Should have New Incident button", menuItem);
				var subMenuItems = menuItem.DropDownItems.Cast<ZToolStripMenuItem>().ToArray();
				var newMenuItem = subMenuItems[0];
				AssertNotNull("Should have New -> Incident -> New menu item", newMenuItem);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				newMenuItem.PerformClick();
				AssertNotNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertEquals(typeof(SupportIncidentController), form.WorkTaskRelatedItemUserControl.LastController.GetType());
				AssertEquals(typeof(SupportIncidentForm), form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.GetType());
				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.BusinessEntityForPersistingForm = incident;
				form.WorkTaskRelatedItemUserControl.LastController.Factory.Save();
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SetAllColumnsVisible(true);
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SelectAllElements();
				group.Reload();
				incident.Reload();
				AssertEquals(0, group.LinkedIncidents.Count);
				AssertEquals(2, group.RelatedItems.Count);
				AssertCollectionNotContains("Should not add WI in incident", workItemCascade, incident.RelatedItems);

				group.SendBroadcastMessage(incident.IncidentManagementLink, message);

				Factory.Save();
				AssertEquals(0, incident.EConversation.Conversation.Messages.Count);

				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.Dispose();
			}
		}

		public void TestShouldNotDetachIncidentGroupFromIncidentRelatedItems()
		{
			var incident = Factory.New<SupportIncident>();
			var group = Factory.New<IncidentManagementGroup>();
			Factory.Save();

			var findBoxList = new IncidentManagementLinkCollection(Factory);
			var attacher = new LinkedIncidentsGridAttacherForTest(incident.RelatedItems, findBoxList, ClientModuleRegistration.IncidentManagementGroup, group);
			attacher.TestAttachCore(incident, new List<BusinessObject>());
			Factory.Save();
			Assert("incident.RelatedItems should contain group", incident.RelatedItems.Contains(group));

			using (var form = GetEDIFormForTest(incident, false))
			{
				form.Show();
				var button = form.WorkTaskRelatedItemUserControl.DetachButton;
				AssertNotNull("Should have Detach Incident button", button);
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SetAllColumnsVisible(true);
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SelectAllElements();

				button.PerformClick();
				AssertContains("Links between Incidents and Incident Management Groups can only be modified in the Incident Group", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldNotDetachIncidentFromIncidentGroupRelatedItems()
		{
			var incident = Factory.New<SupportIncident>();
			var group = Factory.New<IncidentManagementGroup>();
			Factory.Save();

			var findBoxList = new IncidentManagementLinkCollection(Factory);
			var attacher = new LinkedIncidentsGridAttacherForTest(incident.RelatedItems, findBoxList, ClientModuleRegistration.IncidentManagementGroup, group);
			attacher.TestAttachCore(incident, new List<BusinessObject>());
			Factory.Save();

			Assert("IncidentManagementGroup.RelatedItems should contain incident", group.RelatedItems.Contains(incident));
			group.LinkedIncidents.Reload(true);
			using (var form = GetEDIFormForTest(group, false))
			{
				form.Show();
				var button = form.WorkTaskRelatedItemUserControl.DetachButton;
				AssertNotNull("Should have Detach Incident button", button);
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SetAllColumnsVisible(true);
				form.WorkTaskRelatedItemUserControl.RelatedItemGrid.SelectAllElements();

				button.PerformClick();
				AssertContains("Links between Incidents and Incident Management Groups can only be modified from the Incident Control Center tab.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateNewIncidentMenuItem()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			using (var form = GetEDIFormForTest(incident, false))
			{
				form.Show();
				var menuItem = form.WorkTaskRelatedItemUserControl.NewIncidentMenuItem;
				AssertNotNull("Should have New -> Incident menu item", menuItem);
				var subMenuItems = menuItem.DropDownItems.Cast<ZToolStripMenuItem>().ToArray();
				var newMenuItem = subMenuItems[0];
				AssertNotNull("Should have New -> Incident -> New menu item", newMenuItem);
				newMenuItem.PerformClick();
				AssertNotNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(SupportIncidentController), form.WorkTaskRelatedItemUserControl.LastController.GetType());
				AssertEquals(typeof(SupportIncidentForm), form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.GetType());
				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.Dispose();
			}

			UnitTestUserNotification.Instance.ClearMessages();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			using (var form = GetEDIFormForTest(incident, false))
			{
				form.Show();
				var menuItem = form.WorkTaskRelatedItemUserControl.NewIncidentMenuItem;
				AssertNotNull("Should have New -> Incident menu item", menuItem);
				var subMenuItems = menuItem.DropDownItems.Cast<ZToolStripMenuItem>().ToArray();
				var newMenuItem = subMenuItems[0];
				AssertNotNull("Should have New -> Incident -> New menu item", newMenuItem);
				newMenuItem.PerformClick();
				AssertNotNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(SupportIncidentController), form.WorkTaskRelatedItemUserControl.LastController.GetType());
				AssertEquals(typeof(SupportIncidentForm), form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.GetType());
				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.Dispose();
			}
		}

		public void TestCreateNewWorkItemInIncident()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			using (var form = GetEDIFormForTest(incident, false))
			{
				form.Show();
				var button = form.WorkTaskRelatedItemUserControl.NewWorkItemMenuItem;
				AssertNotNull("Should have New -> Work Item menu item", button);
				button.PerformClick();
				AssertNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertEquals(ModuleSelectionControl.YouCannotCreateWorkItem, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			incident.Escalate(SupportIncidentCategoriesList.Codes.Defect, "");
			using (var form = GetEDIFormForTest(incident, false))
			{
				form.Show();
				var button = form.WorkTaskRelatedItemUserControl.NewWorkItemMenuItem;
				AssertNotNull("Should have New -> Work Item menu item", button);
				button.PerformClick();
				AssertNotNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(EDIWorkItemController), form.WorkTaskRelatedItemUserControl.LastController.GetType());
				AssertEquals(typeof(NewWorkItemForm), form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.GetType());
				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.Dispose();
			}
		}

		protected override FormForTest GetFormForTest(object workItem, bool isViewOrDeleteMode) => GetEDIFormForTest(workItem, isViewOrDeleteMode);

		EDIFormForTest GetEDIFormForTest(object workItem, bool isViewOrDeleteMode) => new EDIFormForTest(workItem, isViewOrDeleteMode);
	}
}
