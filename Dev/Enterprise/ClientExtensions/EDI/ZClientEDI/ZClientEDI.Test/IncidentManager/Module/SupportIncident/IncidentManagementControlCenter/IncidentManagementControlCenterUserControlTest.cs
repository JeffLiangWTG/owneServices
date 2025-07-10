using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Tools.SpellCheck.TestFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class IncidentManagementControlCenterUserControlTest : TestCaseWithFactory
	{
		public void TestLinkedIncidentsGrid_Refresh()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_GS_NKResponder = "~UK";

			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_GS_NKResponder = "~UK";

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();
				var status = form.UserControl.timer.Enabled;
				AssertEquals(status, true);
				AssertEquals(2, form.UserControl.LinkedIncidentsGrid.InnerGrid.List.Count);

				form.UserControl.CommunicationArea.SetDataBinding(incident1, "");
				form.UserControl.CommunicationArea.Visible = true;
				form.UserControl.RefreshButton.Visible = true;

				var countdownLabel1 = form.UserControl.Controls.Find("countdownLabel", true).First() as ZLabel;
				countdownLabel1.Text = "00:00";
				form.UserControl.RefreshButton.PerformClick();
				AssertNotEquals("00:00", countdownLabel1.Text);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestLinkedIncidentsGrid_Detach()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_GS_NKResponder = "~UK";

			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_GS_NKResponder = "~UK";

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();

				AssertEquals(2, form.UserControl.LinkedIncidentsGrid.InnerGrid.List.Count);
				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);
				form.UserControl.CommunicationArea.Visible = true;

				var selectedView = form.UserControl.LinkedIncidentsGrid.InnerGrid.GetCurrent();
				var remainingView = form.UserControl.LinkedIncidentsGrid.InnerGrid.List[1];

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.LinkedIncidentsGrid.DetachButtonForTest.PerformClick();
				AssertEquals(false, form.UserControl.CommunicationArea.Visible);
				AssertEquals(1, form.UserControl.LinkedIncidentsGrid.InnerGrid.List.Count);

				Assert(!form.UserControl.LinkedIncidentsGrid.InnerGrid.List.Contains(selectedView));
				Assert(form.UserControl.LinkedIncidentsGrid.InnerGrid.List.Contains(remainingView));
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestLinkedIncidentsGrid_Attach_CascadeWorkItem()
		{
			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_WorkItemNumber = "WI00PYN01";
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.NowStage.ControlIncidents = true;

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.AttachButton_Click(form.UserControl, EventArgs.Empty);
				using (var popup = form.UserControl.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { workItemCascade });
				}

				AssertEquals(0, group.LinkedIncidents.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.LinkedIncidentsGrid.AttachButtonForTest.PerformClick();
				using (var popup = form.UserControl.LinkedIncidentsGrid.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { incident });
				}

				Factory.Save();

				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				var selectedLink = form.UserControl.LinkedIncidentsGrid.InnerGrid.GetCurrent() as IncidentManagementLink;

				Assert(selectedLink.INL_IsGroupControlled);
				AssertCollectionContains(workItemCascade, selectedLink.SupportIncident.RelatedItems);
				Assert(HasCascadeLog(group.Logs));
				Assert(HasCascadeLog(selectedLink.SupportIncident.Logs));

				bool HasCascadeLog(Logs logs)
				{
					return logs.GetAllLogs().Cast<StmALog>().Any(l =>
															l.SL_Reference.Contains("ARG=" + workItemCascade.WKI_WorkItemNumber) &&
															l.SL_Reference.Contains("RFN=" + selectedLink.SupportIncident.IM_IncidentNumber) &&
															l.SL_Reference.Contains("JOB=" + group.ING_IncidentGroupNumber));
				}
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestAttachIncidentToGroupNoDuplicateIncident()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident1.PK;
			link.INL_ING_Group = group.PK;
			link.INL_GS_NKResponder = "~UK";

			Factory.Save();

			var form = new FormForTest(group);
			{
				try
				{
					form.Show();

					AssertEquals(1, group.LinkedIncidents.Count);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.UserControl.LinkedIncidentsGrid.AttachButtonForTest.PerformClick();
					using (var popup = form.UserControl.LinkedIncidentsGrid.LastShownAttachPopupForTesting)
					{
						popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { incident1 });
					}
					group.LinkedIncidents.Load();
					AssertEquals(1, group.LinkedIncidents.Count);
				}
				finally
				{
					if (form != null)
					{
						((IDisposable)form).Dispose();
					}
				}
			}
		}

		public void TestCreateNewDraftButton()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();
				form.UserControl.MessageInventoryGrid.InnerGrid.Select(0);
				var selectedItem = form.UserControl.MessageInventoryGridCurrentElement;

				AssertEquals(4, group.IncidentManagementGroupMessages.Count);
				AssertEquals(false, selectedItem.IGM_IsPublished);

				form.UserControl.CreateNewDraftButton.PerformClick();

				AssertEquals("Should not create new message if message is not published", 4, group.IncidentManagementGroupMessages.Count);
				AssertEquals("Should select row for previous message", 0, form.UserControl.MessageInventoryGrid.InnerGrid.ListManager.Position);

				selectedItem.IGM_IsPublished = true;
				form.UserControl.CreateNewDraftButton.PerformClick();

				AssertEquals(5, group.IncidentManagementGroupMessages.Count);
				AssertEquals("Should select row for new message", 4, form.UserControl.MessageInventoryGrid.InnerGrid.ListManager.Position);

				form.UserControl.CreateNewDraftButton.PerformClick();

				AssertEquals("Should not create new message if there are 2 messages with the same message type", 5, group.IncidentManagementGroupMessages.Count);
				AssertEquals("Should select row for previous message", 4, form.UserControl.MessageInventoryGrid.InnerGrid.ListManager.Position);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestRevertToDraftButton()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();
				form.UserControl.MessageInventoryGrid.InnerGrid.Select(0);
				var selectedItem = form.UserControl.MessageInventoryGridCurrentElement;
				selectedItem.IGM_Message = "Test published message";
				selectedItem.IGM_IsPublished = true;

				AssertEquals(IncidentManagementGroupMessageTypePairList.Codes.AutoReply, selectedItem.IGM_Type);
				AssertEquals(true, selectedItem.IGM_IsPublished);
				AssertEquals(false, form.UserControl.PublishButton.Enabled);
				AssertEquals(true, form.UserControl.RevertToDraftButton.Enabled);

				form.UserControl.RevertToDraftButton.PerformClick();

				selectedItem = form.UserControl.MessageInventoryGridCurrentElement;

				AssertEquals(IncidentManagementGroupMessageTypePairList.Codes.AutoReply, selectedItem.IGM_Type);
				AssertEquals(false, selectedItem.IGM_IsPublished);
				AssertEquals(true, form.UserControl.PublishButton.Enabled);
				AssertEquals(false, form.UserControl.RevertToDraftButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestPublishButton()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();
				form.UserControl.MessageInventoryGrid.InnerGrid.Select(0);
				var selectedItem = form.UserControl.MessageInventoryGridCurrentElement;

				AssertEquals(IncidentManagementGroupMessageTypePairList.Codes.AutoReply, selectedItem.IGM_Type);

				AssertEquals(false, selectedItem.IGM_IsPublished);
				AssertEquals(false, form.UserControl.RevertToDraftButton.Enabled);
				AssertEquals("Should be disable when the message is empty", false, form.UserControl.PublishButton.Enabled);

				form.UserControl.PublishButton.PerformClick();

				selectedItem = form.UserControl.MessageInventoryGridCurrentElement;
				AssertEquals("Should not publish empty message", false, selectedItem.IGM_IsPublished);

				selectedItem.IGM_Message = "published message";
				form.UserControl.PublishButton.PerformClick();

				selectedItem = form.UserControl.MessageInventoryGridCurrentElement;

				AssertEquals(IncidentManagementGroupMessageTypePairList.Codes.AutoReply, selectedItem.IGM_Type);
				AssertEquals(true, selectedItem.IGM_IsPublished);
				AssertEquals(false, form.UserControl.PublishButton.Enabled);
				AssertEquals(true, form.UserControl.RevertToDraftButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestCreateNewDraftEventAfterClicking()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				var publishButton = form.UserControl.PublishButton;
				var revertToDraftButton = form.UserControl.RevertToDraftButton;
				var createNewDraftButton = form.UserControl.CreateNewDraftButton;
				var messageGrid = form.UserControl.MessageInventoryGrid;

				messageGrid.InnerGrid.ListManager.List.Clear();
				AssertEquals(0, messageGrid.InnerGrid.VisibleRowCount);

				var oriMessage = group.IncidentManagementGroupMessages.AddNew();
				oriMessage.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
				oriMessage.IGM_Message = ":)";
				messageGrid.InnerGrid.Refresh();

				Assert(oriMessage.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage));
				AssertEquals(1, messageGrid.InnerGrid.VisibleRowCount);
				messageGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddYesAnswer();
				publishButton.PerformClick();
				Assert(oriMessage.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage));

				UnitTestUserNotification.Instance.AddYesAnswer();
				createNewDraftButton.PerformClick();
				var newDraft = group.IncidentManagementGroupMessages.Find(x => !x.IGM_IsPublished).FirstOrDefault();
				Assert(oriMessage.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.NewBroadcastMessage));

				form.UserControl.MessageInventoryGrid.InnerGrid.ListManager.Position = 0;
				UnitTestUserNotification.Instance.AddYesAnswer();
				revertToDraftButton.PerformClick();
				Assert(!oriMessage.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.PublishBroadcastMessage));
				Assert(!oriMessage.GetEventFlagByCode(IncidentManagementGroupMessage.EventType.RevertBroadcastMessage));
			}
		}

		public void TestIsGroupControlledChangeOnSaving_AfterAllowButtonClicked()
		{
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			Factory.Save();

			var workItem = Factory.New<WorkItem>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			group.AutoCascadeRelatedItems.Add(workItem);
			group.ING_Priority = "CR3";
			group.ING_Product = "PRO";
			group.ING_ProductArea = "ARE";
			group.ING_Module = "MOD";
			group.ING_ServiceType = "TMP";
			group.ING_SourceModuleId = "MID";
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;

			group.NowStage.CascadeProductDetails = true;
			group.NowStage.ControlIncidents = true;

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = true;
			var task = link1.SupportIncident.WorkflowItems.Tasks.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_IsGroupControlled = true;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearUserResponses();
			using (var form = new FormForTest(group))
			{
				form.Show();

				group.ING_ServiceType = "TMP";
				var controlCenter = form.UserControl;
				controlCenter.LinkedIncidentsGrid.InnerGrid.SelectAllElements();
				AssertEquals(2, controlCenter.LinkedIncidentsGrid.InnerGrid.SelectedRowCount);
				var allowButton = controlCenter.AllowControlButton;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				allowButton.PerformClick();
				AssertStartsWith("if all selected incidents are already INL_IsGroupControlled enabled display", "Allow Control cannot be executed if the selected incident(s) status is already enabled", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();

				link1.INL_IsGroupControlled = true;
				link2.INL_IsGroupControlled = false;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				allowButton.PerformClick();
				AssertStartsWith("Main message missed", "Allowing Control will apply all controlled operations on the selected incident(s)", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				Assert("(1)The status of IsGroupControlled should not be changed, if no action", link1.INL_IsGroupControlled);
				Assert("(2)The status of IsGroupControlled should not be changed, if no action", !link2.INL_IsGroupControlled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				allowButton.PerformClick();
				AssertContains("Message should be popped up when a part of links are controlled", "Allow Control status is already enabled for the selected Incident(s) below", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(link1.SupportIncident.Number, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Link2 should not be edited", !UnitTestUserNotification.Instance.LastMessage.Text.Contains($"{link2.SupportIncident.Number}"));
				Assert("Link2 should be controlled", link2.INL_IsGroupControlled);

				Factory.Save();

				AssertEquals("All tasks should be cancelled", 0, link1.SupportIncident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
				Assert("Controlled incidents should contain group's cascade WI", link2.SupportIncident.RelatedItems.FindByPK(workItem.PK) != null);

				link1.Delete();
				link2.Delete();
				Factory.Save();
			}
			UnitTestUserNotification.Instance.ClearUserResponses();

			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_ServiceType = "NTZ";
			Factory.Save();

			var link3 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link3.INL_IM_Incident = incident3.PK;
			link3.INL_ING_Group = group.PK;
			link3.INL_IsGroupControlled = false;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			group.NowStage.CascadeProductDetails = true;
			group.NowStage.ControlIncidents = true;

			var broadcastMessage = group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>().FirstOrDefault(x => x.IGM_Type.Equals(IncidentManagementGroupMessageTypePairList.Codes.Interim));
			broadcastMessage.IGM_IsPublished = true;
			broadcastMessage.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			broadcastMessage.IGM_Message = "NTZ DisallowButton Test 990725";

			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				group.ING_ServiceType = "TMP";

				var controlCenter = form.UserControl;
				controlCenter.LinkedIncidentsGrid.InnerGrid.SelectAllElements();
				AssertEquals(1, controlCenter.LinkedIncidentsGrid.InnerGrid.SelectedRowCount);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				controlCenter.AllowControlButton.PerformClick();
				Factory.Save();

				AssertEquals(true, link3.INL_IsGroupControlled);
				AssertEquals("All tasks should be cancelled", 0, link3.SupportIncident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().Count(x => x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled));
				AssertNotNull("Controlled incidents should contain group's cascade WI", link3.SupportIncident.RelatedItems.FindByPK(workItem.PK));

				AssertEquals("(1)The product details should be same with group", group.ING_Priority, link3.SupportIncident.IM_Priority);
				AssertEquals("(2)The product details should be same with group", group.ING_Product, link3.SupportIncident.IM_Product);
				AssertEquals("(3)The product details should be same with group", group.ING_ProductArea, link3.SupportIncident.IM_ProgramArea);
				AssertEquals("(4)The product details should be same with group", group.ING_Module, link3.SupportIncident.IM_Module);
				AssertEquals("(5)The product details should be same with group", group.ING_ServiceType, link3.SupportIncident.IM_ServiceType);
				AssertEquals("(6)The product details should be same with group", group.ING_SourceModuleId, link3.SupportIncident.IM_SourceModuleId);

				AssertContains("There should be a prompt to ask whether to send a interim broadcast", $"The Incident Group stage has an active {IncidentManagementGroupMessageTypePairList.Descriptions.Interim}", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Should send interim message", link3.SupportIncident.EConversation.Conversation.Messages.Any(x => x.Body.Contains(broadcastMessage.IGM_Message)));

				broadcastMessage.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
				link3.INL_IsGroupControlled = false;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				controlCenter.AllowControlButton.PerformClick();
				AssertContains("There should be a prompt to ask whether to send a Opening broadcast", $"The Incident Group stage has an active {IncidentManagementGroupMessageTypePairList.Descriptions.Opening}", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			UnitTestUserNotification.Instance.ClearUserResponses();
			ErrorReporter.Clear();
		}

		public void TestIsGroupControlledChangeOnSaving_AfterDisallowButtonClicked()
		{
			var workItem = Factory.New<WorkItem>();
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var task = incident1.WorkflowItems.Tasks.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = group.PK;
			link1.INL_IsGroupControlled = false;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = group.PK;
			link2.INL_IsGroupControlled = false;

			Factory.Save();

			UnitTestUserNotification.Instance.ClearUserResponses();
			using (var form = new FormForTest(group))
			{
				form.Show();
				var controlCenter = form.UserControl;
				controlCenter.LinkedIncidentsGrid.InnerGrid.SelectAllElements();
				AssertEquals(2, controlCenter.LinkedIncidentsGrid.InnerGrid.SelectedRowCount);
				var disallowButton = controlCenter.DisallowControlButton;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				disallowButton.PerformClick();
				AssertStartsWith("if all selected incidents are already INL_IsGroupControlled disabled should pop up message", "Disallow Control cannot be executed if the selected incident status is already disabled.", UnitTestUserNotification.Instance.LastMessage.Text);

				link1.INL_IsGroupControlled = false;
				link2.INL_IsGroupControlled = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				disallowButton.PerformClick();
				AssertStartsWith("Main message missed", "Canceled tasks for the selected incident(s) will be re-opened", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				disallowButton.PerformClick();
				AssertContains("Message should be popped up when a part of links are released", "Disallow Control status is already disabled for the selected Incident", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(link1.SupportIncident.Number, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Link1 should not be edited", !UnitTestUserNotification.Instance.LastMessage.Text.Contains($"{link2.SupportIncident.Number}"));
				Assert("Link2 should be controlled", !link2.INL_IsGroupControlled);
			}
			UnitTestUserNotification.Instance.ClearUserResponses();
		}

		public void TestMessageContentTextBox_Visibility()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();

				form.UserControl.MessageInventoryGrid.InnerGrid.Select(0);
				var selectedItem1 = form.UserControl.MessageInventoryGridCurrentElement;
				selectedItem1.IGM_IsPublished = true;
				AssertEquals(true, selectedItem1.IGM_IsPublished);
				AssertEquals(false, form.UserControl.MessageContentTextBox.Enabled);

				form.UserControl.MessageInventoryGrid.InnerGrid.PerformMouseDownForTest(1, 1);
				var selectedItem2 = form.UserControl.MessageInventoryGridCurrentElement;
				AssertEquals(false, selectedItem2.IGM_IsPublished);
				AssertEquals(true, form.UserControl.MessageContentTextBox.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestControlButton()
		{
			var workItemCascade = Factory.NewWithValidTestData<WorkItem>();
			workItemCascade.WKI_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			workItemCascade.WKI_WorkItemNumber = "WI00PYN01";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org1.PK;
			incident1.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org2.PK;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			group.NowStage.ControlIncidents = true;

			var link = Factory.New<IncidentManagementLink>();
			link.INL_IM_Incident = incident1.PK;
			link.INL_ING_Group = group.PK;
			link.INL_GS_NKResponder = "~UK";
			link.INL_IsGroupControlled = false;

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.AttachButton_Click(form.UserControl, EventArgs.Empty);
				using (var popup = form.UserControl.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { workItemCascade });
				}

				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				var selectedView = form.UserControl.LinkedIncidentsGrid.InnerGrid.GetCurrent() as IncidentManagementLink;
				Assert(!selectedView.INL_IsGroupControlled);

				form.UserControl.AllowControlButton.PerformClick();
				Factory.Save();

				AssertCollectionContains(workItemCascade, selectedView.SupportIncident.RelatedItems);
				var groupLog = group.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l =>
															l.SL_Reference.Contains(workItemCascade.WKI_WorkItemNumber) &&
															l.SL_Reference.Contains(selectedView.SupportIncident.IM_IncidentNumber) &&
															l.SL_Reference.Contains(group.ING_IncidentGroupNumber));
				AssertNotNull(groupLog);

				var incidentLog = selectedView.SupportIncident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l =>
															l.SL_Reference.Contains(workItemCascade.WKI_WorkItemNumber) &&
															l.SL_Reference.Contains(selectedView.SupportIncident.IM_IncidentNumber) &&
															l.SL_Reference.Contains(group.ING_IncidentGroupNumber));
				AssertNotNull(incidentLog);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestAttachIncident_CompletedIncidentDialog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;
			incident.IM_Status = IncidentMainLookups.Status.Closed;

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org.PK;
			incident2.IM_Status = IncidentMainLookups.Status.Closed;

			var incident3 = Factory.NewWithValidTestData<SupportIncident>();
			incident3.IM_OH_Client = org.PK;
			incident3.IM_Status = IncidentMainLookups.Status.Closed;

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();

				// Dialog = Yes
				form.UserControl.LinkedIncidentsGrid.AttachButtonForTest.PerformClick();
				using (var popup = form.UserControl.LinkedIncidentsGrid.LastShownAttachPopupForTesting)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { incident });
				}

				group.LinkedIncidents.Load();

				Assert("Incident should be controlled", group.LinkedIncidents.Cast<IncidentManagementLink>().First(l => l.INL_IM_Incident == incident.PK).INL_IsGroupControlled);
				Assert(!group.IncidentCompleted);
				Assert(incident.IM_Status.Equals(IncidentMainLookups.Status.Closed));

				// Dialog = No
				form.UserControl.LinkedIncidentsGrid.AttachButtonForTest.PerformClick();
				using (var popup = form.UserControl.LinkedIncidentsGrid.LastShownAttachPopupForTesting)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { incident2 });
				}

				group.LinkedIncidents.Load();

				Assert("Incident should not be controlled", !group.LinkedIncidents.Cast<IncidentManagementLink>().First(l => l.INL_IM_Incident == incident2.PK).INL_IsGroupControlled);
				Assert(!group.IncidentCompleted);
				Assert(incident2.IM_Status.Equals(IncidentMainLookups.Status.Closed));

				// Dialog = Cancel
				form.UserControl.LinkedIncidentsGrid.AttachButtonForTest.PerformClick();
				using (var popup = form.UserControl.LinkedIncidentsGrid.LastShownAttachPopupForTesting)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { incident3 });
				}

				group.LinkedIncidents.Load();

				Assert("Should not attach the incident", !group.LinkedIncidents.Cast<IncidentManagementLink>().Any(l => l.INL_IM_Incident == incident3.PK));
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestInterimMessageButton_NoPublishedInterimMessage_ShouldButtonDisabled()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			message.IGM_Message = "Test for Broadcast";

			group.IncidentManagementGroupMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();
				AssertEquals(false, form.UserControl.BroadcastInterimButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestInterimMessageButton_ExistPublishedInterimMessage_ShouldButtonEnabled()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			message.IGM_Message = "Test for Broadcast";

			group.IncidentManagementGroupMessages.Add(message);

			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				AssertEquals(true, form.UserControl.BroadcastInterimButton.Enabled);
			}
		}

		public void TestInterimMessageButton_NoBroadcastDate_ShouldSendInterimMessageToIncidentAndGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			message.IGM_Message = "Test for Broadcast";
			group.IncidentManagementGroupMessages.Add(message);
			AssertEquals(ZDateTime.Empty, message.IGM_BroadcastDateUtc);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;
			group.LinkedIncidents.Add(link);

			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.BroadcastInterimButton.PerformClick();

				Factory.Save();

				AssertEquals(incident.EConversation.Conversation.Messages.Count, 1);
				var newMessage = incident.EConversation.Conversation.Messages[0];
				AssertEquals(newMessage.JCM_Body, message.IGM_Message);
				AssertEquals("TOM", newMessage.SenderCode);

				var currentUserCode = ((IConversationParticipant)EnvProxy.Instance.CurrentUser).Code;
				AssertEquals(group.EConversation.Conversation.Messages.Count, 1);
				var newGroupMessage = group.EConversation.Conversation.Messages[0];
				AssertEquals(newGroupMessage.JCM_Body, message.IGM_Message);
				AssertEquals(currentUserCode, newGroupMessage.SenderCode);

				var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
				var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;

				var messageType = message.IGM_Type;
				var description = FormattableString.Invariant(
					$"{messageType} Broadcast Message sent to {incident.Number}"
					);

				var pair1 = new KeyValuePair<string, string>("MST", messageType);
				var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
				var pair3 = new KeyValuePair<string, string>("DES", description);

				Assert(incidentLogParameters.Contains(pair1));
				Assert(incidentLogParameters.Contains(pair2));
				Assert(incidentLogParameters.Contains(pair3));

				Assert(groupLogParameters.Contains(pair1));
				Assert(groupLogParameters.Contains(pair2));
				Assert(groupLogParameters.Contains(pair3));
			}
		}

		public void TestInterimMessageButton_ExistBroadcastDate_ShouldSendInterimMessageToIncidentOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			message.IGM_Message = "Test for Broadcast";
			message.IGM_BroadcastDateUtc = DateTime.UtcNow;
			group.IncidentManagementGroupMessages.Add(message);
			AssertNotEquals(ZDateTime.Empty, message.IGM_BroadcastDateUtc);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;
			group.LinkedIncidents.Add(link);

			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.BroadcastInterimButton.PerformClick();

				Factory.Save();

				AssertEquals(group.EConversation.Conversation.Messages.Count, 0);
				AssertEquals(incident.EConversation.Conversation.Messages.Count, 1);
				var newMessage = incident.EConversation.Conversation.Messages[0];
				AssertEquals(newMessage.JCM_Body, message.IGM_Message);
				AssertEquals("TOM", newMessage.SenderCode);
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
				var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;

				var messageType = message.IGM_Type;
				var description = FormattableString.Invariant(
					$"{messageType} Broadcast Message sent to {incident.Number}"
					);

				var pair1 = new KeyValuePair<string, string>("MST", messageType);
				var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
				var pair3 = new KeyValuePair<string, string>("DES", description);

				Assert(incidentLogParameters.Contains(pair1));
				Assert(incidentLogParameters.Contains(pair2));
				Assert(incidentLogParameters.Contains(pair3));

				Assert(groupLogParameters.Contains(pair1));
				Assert(groupLogParameters.Contains(pair2));
				Assert(groupLogParameters.Contains(pair3));
			}
		}

		public void TestAutoReplyButton_NoPublishedAutoReplyMessage_ShouldButtonDisabled()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			message.IGM_Message = "Test for Broadcast";

			group.IncidentManagementGroupMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();
				AssertEquals(false, form.UserControl.SendAutoReplyButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestAutoReplyButton_ExistPublishedAutoReplyMessage_ShouldButtonEnabled()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			message.IGM_Message = "Test for Broadcast";

			group.IncidentManagementGroupMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(group);

			try
			{
				form.Show();
				AssertEquals(true, form.UserControl.SendAutoReplyButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestAutoReplyButton_NoBroadcastDate_ShouldSendAutoReplyMessageToIncidentAndGroup()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			message.IGM_Message = "Test for Broadcast";
			group.IncidentManagementGroupMessages.Add(message);
			AssertEquals(ZDateTime.Empty, message.IGM_BroadcastDateUtc);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;
			group.LinkedIncidents.Add(link);

			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.SendAutoReplyButton.PerformClick();

				Factory.Save();

				AssertEquals(incident.EConversation.Conversation.Messages.Count, 1);
				var newMessage = incident.EConversation.Conversation.Messages[0];
				AssertEquals(newMessage.JCM_Body, message.IGM_Message);
				AssertEquals("TOM", newMessage.SenderCode);

				var currentUserCode = ((IConversationParticipant)EnvProxy.Instance.CurrentUser).Code;
				AssertEquals(group.EConversation.Conversation.Messages.Count, 1);
				var newGroupMessage = group.EConversation.Conversation.Messages[0];
				AssertEquals(newGroupMessage.JCM_Body, message.IGM_Message);
				AssertEquals(currentUserCode, newGroupMessage.SenderCode);

				var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
				var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;

				var messageType = message.IGM_Type;
				var description = FormattableString.Invariant(
					$"{messageType} Broadcast Message sent to {incident.Number}"
					);

				var pair1 = new KeyValuePair<string, string>("MST", messageType);
				var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
				var pair3 = new KeyValuePair<string, string>("DES", description);

				Assert(incidentLogParameters.Contains(pair1));
				Assert(incidentLogParameters.Contains(pair2));
				Assert(incidentLogParameters.Contains(pair3));

				Assert(groupLogParameters.Contains(pair1));
				Assert(groupLogParameters.Contains(pair2));
				Assert(groupLogParameters.Contains(pair3));
			}
		}

		public void TestAutoReplyButton_ExistBroadcastDate_ShouldSendAutoReplyMessageToIncidentOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			message.IGM_Message = "Test for Broadcast";
			message.IGM_BroadcastDateUtc = DateTime.UtcNow;
			group.IncidentManagementGroupMessages.Add(message);
			AssertNotEquals(ZDateTime.Empty, message.IGM_BroadcastDateUtc);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;
			group.LinkedIncidents.Add(link);

			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.SendAutoReplyButton.PerformClick();

				Factory.Save();

				AssertEquals(group.EConversation.Conversation.Messages.Count, 0);
				AssertEquals(incident.EConversation.Conversation.Messages.Count, 1);
				var newMessage = incident.EConversation.Conversation.Messages[0];
				AssertEquals(newMessage.JCM_Body, message.IGM_Message);
				AssertEquals("TOM", newMessage.SenderCode);

				var incidentLogParameters = incident.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;
				var groupLogParameters = group.Logs.MostRecentLogByEventTime(AutoEvents.BroadcastMessage).Parameters;

				var messageType = message.IGM_Type;
				var description = FormattableString.Invariant(
					$"{messageType} Broadcast Message sent to {incident.Number}"
					);

				var pair1 = new KeyValuePair<string, string>("MST", messageType);
				var pair2 = new KeyValuePair<string, string>("RFN", incident.Number);
				var pair3 = new KeyValuePair<string, string>("DES", description);

				Assert(incidentLogParameters.Contains(pair1));
				Assert(incidentLogParameters.Contains(pair2));
				Assert(incidentLogParameters.Contains(pair3));

				Assert(groupLogParameters.Contains(pair1));
				Assert(groupLogParameters.Contains(pair2));
				Assert(groupLogParameters.Contains(pair3));
			}
		}

		public void TestGridLoad_DbHits()
		{
			var wi1 = Factory.NewWithValidTestData<WorkItem>();
			wi1.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.DefectFix;
			wi1.WKI_WorkItemNumber = "WI00NTZ01";
			var task1 = wi1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task1.P9_Sequence = 1;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_OH_Client = org.PK;
			incident1.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_OH_Client = org.PK;
			incident2.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;

			var incidentGroup = Factory.New<IncidentManagementGroup>();
			incidentGroup.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			incidentGroup.NowStage.ControlIncidents = true;
			incidentGroup.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround;
			incidentGroup.AutoCascadeRelatedItems.Add(wi1);

			Factory.Save();

			var link1 = Factory.New<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_ING_Group = incidentGroup.PK;
			link1.INL_IsGroupControlled = true;
			var link2 = Factory.New<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_ING_Group = incidentGroup.PK;
			link2.INL_IsGroupControlled = true;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incidentGroupReloaded = newFactory.Load<IncidentManagementGroup>(incidentGroup.PK);

			using (var form = new FormForTest(incidentGroupReloaded))
			{
				newFactory.ResetDatabaseLoadCount();

				using (AssertDbHitsWithUsefulQueryInformation(new Dictionary<string, int>
				{
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ JobConversationSchema.Constants.TableName, 5 },
					{ JobConversationMessageSchema.Constants.TableName, 2 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ IncidentManagementGroupMessageSchema.Constants.TableName, 1 },
					{ IncidentManagementLinkSchema.Constants.TableName, 1 },
					{ IncidentMainSchema.Constants.TableName, 2 },
					{ IncidentRequestSchema.Constants.TableName, 1 },
					{ JobConversationParticipantSchema.Constants.TableName, 1 },
					{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ GenPivotSchema.Constants.TableName, 5 },
					{ WorkItemSchema.Constants.TableName, 1 },
					{ LicenceCompanySchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
				}, newFactory))
				{
					incidentGroupReloaded.LinkedIncidents.Load();
					form.Show();

					var selectedView = form.UserControl.LinkedIncidentsGrid.InnerGrid.GetCurrent() as IncidentManagementLink;
					AssertEquals("Precondition", 2, form.UserControl.LinkedIncidentsGrid.InnerGrid.List.Count);
					form.UserControl.LinkedIncidentsGrid.InnerGrid.SetAllColumnsVisible(true);
					var waitingTime = selectedView.CustomerWaitingTime;
					Application.DoEvents();
				}
			}
		}

		public void TestPublishButtonClick_ShouldPopUpSpellCheckForm()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "TOM";
			staff.GS_Code = "TOM";

			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_GS_NKGroupOwner = staff.GS_Code;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = group.PK;
			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.AutoReply;
			message.IGM_Message = "wrongg";
			group.IncidentManagementGroupMessages.Add(message);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_OH_Client = org.PK;

			var link = Factory.NewWithValidTestData<IncidentManagementLink>();
			link.INL_IM_Incident = incident.PK;
			link.INL_ING_Group = group.PK;
			link.INL_IsGroupControlled = true;
			group.LinkedIncidents.Add(link);

			Factory.Save();

			using (var form = new FormForTest(group))
			{
				form.Show();
				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessages();

				form.UserControl.MessageContentTextBox.Text = "correctly";

				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					form.UserControl.PublishButton.PerformClick();
					AssertEquals(0, spellCheckTester.DisplayCount);
				}
			}

			message.IGM_IsPublished = false;

			using (var form = new FormForTest(group))
			{
				form.Show();
				form.UserControl.LinkedIncidentsGrid.InnerGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessages();

				form.UserControl.MessageContentTextBox.Text = "wrongg";

				using (var spellCheckTester = new SpellCheckFormTestHelper(Change))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					form.UserControl.PublishButton.PerformClick();
					AssertEquals(1, spellCheckTester.DisplayCount);
				}
			}
		}

		SpellCheckFormAction Change(ISpellCheckerForm form)
		{
			return new SpellCheckFormAction(SpellCheckerFormResult.Change, null);
		}

		#region Implementation

		class FormForTest : IncidentManagementGroupForm
		{
			IncidentManagementControlCenterUserControlForTest userControl;

			public IncidentManagementControlCenterUserControlForTest UserControl => userControl ?? (userControl = new IncidentManagementControlCenterUserControlForTest());

			public FormForTest(IncidentManagementGroup incidentManagementGroup)
				: base(incidentManagementGroup)
			{
				base.Controls.Add(UserControl);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && userControl != null)
				{
					userControl.Dispose();
				}

				base.Dispose(disposing);
			}
		}

		class IncidentManagementControlCenterUserControlForTest : IncidentManagementControlCenterUserControl
		{
			public new LinkedIncidentsModuleButtonGrid LinkedIncidentsGrid => base.LinkedIncidentsGrid;
			public new MessageInventoryModuleButtonGrid MessageInventoryGrid => base.MessageInventoryGrid;
			public new IncidentManagementGroupMessage MessageInventoryGridCurrentElement => base.MessageInventoryGridCurrentElement;
			public new ZButton CreateNewDraftButton => base.CreateNewDraftButton;
			public new ZButton RevertToDraftButton => base.RevertToDraftButton;
			public new ZButton PublishButton => base.PublishButton;
			public ZButton BroadcastInterimButton => broadcastInterimButton;
			public ZButton SendAutoReplyButton => sendAutoReplyButton;
			public new ZTextBox MessageContentTextBox => base.MessageContentTextBox;
			public ZButton AllowControlButton => base.allowControlButton;
			public ZButton DisallowControlButton => base.disallowControlButton;
			public ZButton RefreshButton => refreshButton;
			public new IncidentManagementGroupControlCenterCommunicationAreaUserControl CommunicationArea => base.CommunicationArea;

			public new Timer timer => base.timer;
		}

		#endregion
	}
}
