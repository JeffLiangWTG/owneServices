using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core;
using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroupConstants;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(IncidentManagementGroupDetailsUserControl))]
	public class IncidentManagementGroupDetailsUserControlTest : TestCaseWithFactory
	{
		#region Stages Grid

		void EnableIsReversibleForAllStages()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			foreach (var item in registryValue[0].IncidentGroupStatusConfigurations)
			{
				item.IsReversible = true;
			}
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
		}

		public void TestMinimumStatusShouldDisablePreviousButton()
		{
			EnableIsReversibleForAllStages();
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			var form = new FormForTest(Group);
			AssertGreaterThanOrEqualTo("Precondition: Need more than two stages for the button enabled assertions to be valid", Group.Stages.Count, 2);

			try
			{
				form.Show();
				AssertEquals(false, form.UserControl.PreviousButton.Enabled);
				AssertEquals(true, form.UserControl.NextButton.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.UserControl.NextButton.PerformClick();
				AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, Group.ING_Status);
				AssertEquals(false, form.UserControl.PreviousButton.Enabled);
				AssertEquals(true, form.UserControl.NextButton.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.NextButton.PerformClick();
				AssertNotEquals(IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, Group.ING_Status);
				AssertEquals(true, form.UserControl.PreviousButton.Enabled);
				AssertEquals(true, form.UserControl.NextButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				} 
			}
		}

		public void TestMaximumStatusShouldDisableNextButton()
		{
			EnableIsReversibleForAllStages();
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;

			var allMessages = Group.IncidentManagementGroupMessages;
			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";
			allMessages.Add(message);

			var form = new FormForTest(Group);
			AssertGreaterThanOrEqualTo("Precondition: Need more than two stages for the button enabled assertions to be valid", Group.Stages.Count, 2);

			try
			{
				form.Show();
				AssertEquals(true, form.UserControl.PreviousButton.Enabled);
				AssertEquals(false, form.UserControl.NextButton.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.UserControl.PreviousButton.PerformClick();
				AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code, Group.ING_Status);
				AssertEquals(true, form.UserControl.PreviousButton.Enabled);
				AssertEquals(false, form.UserControl.NextButton.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.PreviousButton.PerformClick();

				AssertNotEquals(IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code, Group.ING_Status);
				AssertEquals(true, form.UserControl.PreviousButton.Enabled);
				AssertEquals(true, form.UserControl.NextButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestPreviousShouldUpdateStatus()
		{
			EnableIsReversibleForAllStages();
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;

			var allMessages = Group.IncidentManagementGroupMessages;
			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";
			allMessages.Add(message);

			var form = new FormForTest(Group);
			AssertGreaterThanOrEqualTo("Precondition: Need more than two stages for the button enabled assertions to be valid", Group.Stages.Count, 2);

			try
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.UserControl.PreviousButton.PerformClick();
				AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code, Group.ING_Status);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.PreviousButton.PerformClick();
				AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code, Group.ING_Status);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestNextShouldUpdateStatus()
		{
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			var form = new FormForTest(Group);
			AssertGreaterThanOrEqualTo("Precondition: Need more than two stages for the button enabled assertions to be valid", Group.Stages.Count, 2);

			try
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.UserControl.NextButton.PerformClick();
				AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, Group.ING_Status);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.NextButton.PerformClick();
				AssertEquals(IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, Group.ING_Status);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestPreviousApprovalGate()
		{
			EnableIsReversibleForAllStages();
			var newCode1 = "OZZ";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var newStage = registryValue[0].IncidentGroupStatusConfigurations.AddNew(newCode1, "description", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, approvalGate: true);
			newStage.IsReversible = true;
			newStage.Sequence = 5;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			var form = new FormForTest(Group);
			AssertEquals("Precondition: Second stage should be new one with approval gate", newCode1, Group.Stages[1].Code);
			AssertEquals("Precondition: Third stage should be ESC", IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, Group.Stages[2].Code);

			try
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.UserControl.PreviousButton.PerformClick();
				AssertEquals("Should not update status if approval gate is rejected", IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, Group.ING_Status);
				AssertEquals("Should show approval gate message",
					FormattableString.Invariant($"Approval is required to change the group stage from {IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code} to {newCode1}. Do you wish to proceed with the change?"),
					UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.PreviousButton.PerformClick();
				AssertEquals("Should not update status if approval gate is accepted", newCode1, Group.ING_Status);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestNextApprovalGate()
		{
			var newCode1 = "OZZ";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(newCode1, "description", IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue, approvalGate: true);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code;

			var form = new FormForTest(Group);
			AssertEquals("Precondition: Maximum stage should be new one with approval gate", newCode1, Group.Stages[Group.Stages.Count - 1].Code);
			AssertEquals("Precondition: Should be no stages between RSV and new stage", IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code, Group.Stages[Group.Stages.Count - 2].Code);

			try
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.UserControl.NextButton.PerformClick();
				AssertEquals("Should not update status if approval gate is rejected", IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code, Group.ING_Status);
				AssertEquals("Should show approval gate message",
					FormattableString.Invariant($"Approval is required to change the group stage from {IncidentGroupStatusConfigurationConstants.ConfigurationRSV.Code} to {newCode1}. Do you wish to proceed with the change?"),
					UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.NextButton.PerformClick();
				AssertEquals("Should not update status if approval gate is accepted", newCode1, Group.ING_Status);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestButtonEnabledShouldBeUpdatedOnSave()
		{
			EnableIsReversibleForAllStages();
			Group.ING_Type = string.Empty;

			var form = new FormForTest(Group);
			try
			{
				form.Show();
				AssertEquals("Precondition: Buttons are disabled when the form is first created", false, form.UserControl.PreviousButton.Enabled);
				AssertEquals("Precondition: Buttons are disabled when the form is first created", false, form.UserControl.NextButton.Enabled);

				Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
				Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;
				AssertEquals("Precondition: Buttons should still be disabled until save", false, form.UserControl.PreviousButton.Enabled);
				AssertEquals("Precondition: Buttons should still be disabled until save", false, form.UserControl.NextButton.Enabled);

				form.FireSaveButton();
				AssertEquals("Button enabled should be updated", true, form.UserControl.PreviousButton.Enabled);
				AssertEquals("Button enabled should be updated", true, form.UserControl.NextButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestCannotMoveGroupToDeletedStage()
		{
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.Stages.RemoveAndDeleteAll();

			var code1 = "CO1";
			var code2 = "CO2";
			var description1 = "description1";
			var description2 = "description2";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.RemoveAll();
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code1, description1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code2, description2, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			
			Group.ING_Status = "CO1";
			Factory.Save();

			Group.RefreshStages();
			AssertEquals(2, Group.Stages.Count);
			var stages = Group.Stages.Cast<IncidentGroupStatusConfiguration>();

			Assert(stages.Any(x => x.Code.EqualsIgnoringCase(code1)));
			Assert(stages.Any(x => x.Code.EqualsIgnoringCase(code2)));

			var newRegistryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			newRegistryValue[0].IncidentGroupStatusConfigurations.RemoveAll();
			newRegistryValue[0].IncidentGroupStatusConfigurations.AddNew(code1, description1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegistryValue);
			Factory.Save();

			var form = new FormForTest(Group);
			try
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddOKAnswer();
				var nextButton = form.UserControl.NextButton;
				nextButton.PerformClick();

				var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains($"Stage has been deleted or disabled since opening this form.", dialog);
				AssertEquals("Group Status Should Not Be Changed", Group.ING_Status, code1);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
				TearDown();
			}
		}

		public void TestCannotMoveGroupToDisabledtage()
		{
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.Stages.RemoveAndDeleteAll();

			var code1 = "CO1";
			var code2 = "CO2";
			var description1 = "description1";
			var description2 = "description2";
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations.RemoveAll();
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code1, description1, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			registryValue[0].IncidentGroupStatusConfigurations.AddNew(code2, description2, IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Group.ING_Status = "CO1";
			Factory.Save();

			Group.RefreshStages();
			AssertEquals(2, Group.Stages.Count);
			var stages = Group.Stages.Cast<IncidentGroupStatusConfiguration>();

			Assert(stages.Any(x => x.Code.EqualsIgnoringCase(code1)));
			Assert(stages.Any(x => x.Code.EqualsIgnoringCase(code2)));

			var nextStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "CO2");
			nextStage.Enabled = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			var form = new FormForTest(Group);
			try
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddOKAnswer();
				var nextButton = form.UserControl.NextButton;
				nextButton.PerformClick();

				var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains($"Stage has been deleted or disabled since opening this form.", dialog);
				AssertEquals("Group Status Should Not Be Changed", Group.ING_Status, code1);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
				TearDown();
			}
		}

		#region Stage Change Prompts

		public void TestStageChangeToActiveIncidentUnderControl_MessageNotPublished_ShouldPromptPublishMessageConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.IncidentCompleted = false;
			escStage.GroupCompleted = false;

			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.IncidentCompleted = false;
			aciStage.GroupCompleted = false;
			aciStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";
			var messageTypeDesc = IncidentManagementGroupMessageTypePairList.Descriptions.Opening.ToString();

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains($"No {messageTypeDesc.ToLower()} broadcast message published", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestStageChangeToActiveIncidentNotUnderControl_MessageNotPublished_ShouldNotPromptPublishMessageConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.IncidentCompleted = false;
			escStage.GroupCompleted = false;

			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.IncidentCompleted = false;
			aciStage.GroupCompleted = false;
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";
			var messageTypeDesc = IncidentManagementGroupMessageTypePairList.Descriptions.Opening.ToString();

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertNotContains($"No {messageTypeDesc.ToLower()} broadcast message published", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestStageChangeToPostIncidentUnderControl_MessageNotPublished_ShouldPromptPublishMessageConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.IncidentCompleted = false;
			aciStage.GroupCompleted = false;

			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.IncidentCompleted = false;
			psiStage.GroupCompleted = false;
			psiStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";
			var messageTypeDesc = IncidentManagementGroupMessageTypePairList.Descriptions.Closing.ToString();

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains($"No {messageTypeDesc.ToLower()} broadcast message published", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestStageChangeToPostIncidentNotUnderControl_MessageNotPublished_ShouldNotPromptPublishMessageConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.IncidentCompleted = false;
			aciStage.GroupCompleted = false;

			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.IncidentCompleted = false;
			psiStage.GroupCompleted = false;
			psiStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = false;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";
			var messageTypeDesc = IncidentManagementGroupMessageTypePairList.Descriptions.Closing.ToString();

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertNotContains($"No {messageTypeDesc.ToLower()} broadcast message published", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestStageChangeToActiveIncidentUnderControl_ShouldPromptSendBroadcastConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.IncidentCompleted = false;
			aciStage.GroupCompleted = false;
			aciStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("Confirm broadcast of opening message", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestStageChangeToActiveIncidentNotUnderControl_ShouldNotPromptSendBroadcastConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var aciStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ACI");
			aciStage.IncidentCompleted = false;
			aciStage.GroupCompleted = false;
			aciStage.ControlIncidents = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			message.IGM_Message = "Test for Broadcast";

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertNotContains("Confirm broadcast of opening message", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestStageChangeToPostIncidentUnderControl_ShouldPromptSendBroadcastConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.IncidentCompleted = false;
			psiStage.GroupCompleted = false;
			psiStage.ControlIncidents = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("Confirm broadcast of closing message", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestStageChangeToPostIncidentNotUnderControl_ShouldNotPromptSendBroadcastConfirmation()
		{
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.Stages.RemoveAndDeleteAll();

			var groupStatusConfig1 = new IncidentGroupStatusConfiguration();
			groupStatusConfig1.Code = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;
			groupStatusConfig1.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Sequence;
			groupStatusConfig1.IncidentCompleted = false;
			groupStatusConfig1.GroupCompleted = false;
			groupStatusConfig1.IsSystem = true;
			Group.Stages.Add(groupStatusConfig1);

			var groupStatusConfig2 = new IncidentGroupStatusConfiguration();
			groupStatusConfig2.Code = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;
			groupStatusConfig2.Sequence = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Sequence;
			groupStatusConfig2.IncidentCompleted = false;
			groupStatusConfig2.GroupCompleted = false;
			groupStatusConfig2.ControlIncidents = false;
			groupStatusConfig2.IsSystem = true;
			Group.Stages.Add(groupStatusConfig2);

			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code;

			var allMessages = Group.IncidentManagementGroupMessages;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Closing;
			message.IGM_Message = "Test for Broadcast";

			allMessages.Add(message);

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertNotContains("Confirm broadcast of closing message", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestControlIncidentChangeToTrue_ShouldPromptEnactControlConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.ControlIncidents = false;
			psiStage.IncidentCompleted = false;
			psiStage.GroupCompleted = false;
			var rsvStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "RSV");
			rsvStage.ControlIncidents = true;
			rsvStage.IncidentCompleted = false;
			rsvStage.GroupCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Factory.Save();

			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("Confirm incident control enabled", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestControlIncidentChangeToFalse_ShouldPromptReleaseConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.ControlIncidents = true;
			psiStage.IncidentCompleted = false;
			psiStage.GroupCompleted = false;
			var rsvStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "RSV");
			rsvStage.ControlIncidents = false;
			rsvStage.IncidentCompleted = false;
			rsvStage.GroupCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Factory.Save();

			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("Confirm incident control disabled", dialog);

			form.Dispose();
			TearDown();
		}

		public void TestControlIncidentChangeToFalse_ShouldReopenTaskWithIncidentCompletedIsFalse()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var invStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "INV");
			invStage.ControlIncidents = true;
			invStage.IncidentCompleted = true;
			invStage.GroupCompleted = false;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.ControlIncidents = false;
			escStage.IncidentCompleted = false;
			escStage.GroupCompleted = false;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Factory.Save();

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			var task1 = incident.WorkflowItems.Tasks.AddNew();
			var task2 = incident.WorkflowItems.Tasks.AddNew();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			Factory.Save();

			var link = Group.LinkedIncidents.AddNew();
			link.INL_ING_Group = Group.PK;
			link.INL_IM_Incident = incident.PK;
			link.INL_IsGroupControlled = true;
			Factory.Save();

			AssertEquals("Precondition: link should be controlled", true, link.IsControlled);
			AssertEquals("Precondition: IncidentCompleted of NowStage should be true", true, Group.NowStage.IncidentCompleted);
			AssertEquals("Precondition: Task 1 should be cancelled after link is saved", ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals("Precondition: Task 2 should be cancelled after link is saved", ProcessTaskStatusCodeList.Codes.Cancelled, task2.P9_Status);

			using (var form = new FormForTest(Group))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddYesAnswer();
				var nextButton = form.UserControl.NextButton;

				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddYesAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();
				nextButton.PerformClick();

				AssertEquals("Precondition: link should not be controlled", false, link.IsControlled);
				AssertEquals("Precondition: IncidentCompleted of NowStage should not be true", false, Group.NowStage.IncidentCompleted);
				AssertEquals("Task 1 should be recovered to ASN", ProcessTaskStatusCodeList.Codes.Assigned, task1.P9_Status);
				AssertEquals("Task 2 should be recovered to OPN", ProcessTaskStatusCodeList.Codes.Open, task2.P9_Status);

				form.Dispose();
			}

			TearDown();
		}

		public void TestGroupCompleteChangeToTrue_AndServiceOutageActive_ShouldPromptGroupCompleteConfirmation()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var psiStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "PSI");
			psiStage.ControlIncidents = false;
			psiStage.IncidentCompleted = false;
			psiStage.GroupCompleted = false;
			var rsvStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "RSV");
			rsvStage.ControlIncidents = false;
			rsvStage.IncidentCompleted = false;
			rsvStage.GroupCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Factory.Save();

			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationPSI.Code;
			Group.ING_ServiceOutage = ServiceOutageCodes.Active;

			Factory.Save();

			var form = new FormForTest(Group);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddYesAnswer();

			var nextButton = form.UserControl.NextButton;

			form.Show();
			Application.DoEvents();
			nextButton.PerformClick();

			var dialog = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("Confirm group completion", dialog);

			form.Dispose();
			TearDown();
		}

		#endregion

		#endregion

		public void TestModuleDropEditVisibility()
		{
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			AssertEquals("Precondition", false, Group.ING_Product.IsEmpty);
			AssertEquals("Precondition", ModuleListType.MenuSection, Group.ModuleType);

			var form = new FormForTest(Group);

			try
			{
				form.Show();
				var menuSectionDropEdit = (ZDropEdit)form.UserControl.Controls.Find("menuSectionDropEdit", true).Single();
				var cr8ModuleDropEdit = (ZDropEdit)form.UserControl.Controls.Find("cr8ModuleDropEdit", true).Single();
				var cr9ModuleDropEdit = (ZDropEdit)form.UserControl.Controls.Find("cr9ModuleDropEdit", true).Single();

				AssertEquals("Should be visible with MenuSection module type and non-empty product", true, menuSectionDropEdit.Visible);
				AssertEquals(false, cr8ModuleDropEdit.Visible);
				AssertEquals(false, cr9ModuleDropEdit.Visible);

				Group.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				AssertEquals("Precondition", ModuleListType.Cr8, Group.ModuleType);

				AssertEquals(false, menuSectionDropEdit.Visible);
				AssertEquals("Should be visible with CR8 module type", true, cr8ModuleDropEdit.Visible);
				AssertEquals(false, cr9ModuleDropEdit.Visible);

				Group.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
				AssertEquals("Precondition", ModuleListType.Cr9, Group.ModuleType);

				AssertEquals(false, menuSectionDropEdit.Visible);
				AssertEquals(false, cr8ModuleDropEdit.Visible);
				AssertEquals("Should be visible with CR9 module type", true, cr9ModuleDropEdit.Visible);

				Group.ING_Priority = string.Empty;
				AssertEquals("Precondition", ModuleListType.Unspecified, Group.ModuleType);

				AssertEquals(false, menuSectionDropEdit.Visible);
				AssertEquals(false, cr8ModuleDropEdit.Visible);
				AssertEquals(false, cr9ModuleDropEdit.Visible);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestOverrideSourceModuleButtonVisibility()
		{
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			AssertEquals("Precondition", ModuleListType.MenuSection, Group.ModuleType);

			var form = new FormForTest(Group);

			try
			{
				form.Show();
				var overrideSourceModuleButton = (ZButton)form.UserControl.Controls.Find("OverrideSourceModuleButton", true).Single();

				AssertEquals("Should be visible with MenuSection module type", true, overrideSourceModuleButton.Visible);

				Group.ING_Priority = string.Empty;
				AssertEquals("Precondition", ModuleListType.Unspecified, Group.ModuleType);
				AssertEquals("Should be hidden with Unspecified module type", false, overrideSourceModuleButton.Visible);

				Group.ING_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement;
				AssertNotEquals("Precondition", ModuleListType.Unspecified, Group.ModuleType);
				AssertEquals("Should be visible with Cr8 module type", true, overrideSourceModuleButton.Visible);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestServiceType_Visibility()
		{
			var areas = new CodeDescriptionPairList();
			areas.AddPair("XRM", "XRM");
			areas.AddPair("ARC", "ARC");
			EDIDataRegistry.Instance.ProductAreas.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, areas);
			// setup for ServiceTypeProductAreaModuleMappingLookups.Modules
			var collection1 = new SystemProductCollection();
			var product1 = collection1.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			product1.ModuleMappings.AddNew("SAA", "XXX Description", "XRM", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);
			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "Enterprise", true);
			var module = product.ServiceTypeModuleMappings.AddNew("SAA", "Module XRM", "XRM", false);
			module.ServiceTypeMappings.AddNew("SIM");
			EDIDataRegistry.Instance.ServiceTypeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Group.ING_Product = ProductTypes.Codes.Enterprise;
			Group.ING_Priority = Core.Constants.CustomerService.CriticalityCodes.CR6_FeatureRequest;
			Group.ING_ProductArea = "XRM";
			Group.ING_Module = "SAA";
			Group.ING_ServiceType = "SIM";

			var form = new FormForTest(Group);

			try
			{
				form.Show();
				var serviceTypeCaption = (ZLabel)form.UserControl.Controls.Find("serviceTypeCaption", true).Single();
				var serviceTypeDropEdit = (ZDropEdit)form.UserControl.Controls.Find("serviceTypeDropEdit", true).Single();

				Assert(serviceTypeCaption.Visible);
				Assert(serviceTypeDropEdit.Visible);
				Assert(!Group.ING_ServiceType.IsEmpty);

				Group.ING_Module = "AAA";
				Assert(!serviceTypeCaption.Visible);
				Assert(!serviceTypeDropEdit.Visible);
				Assert(Group.ING_ServiceType.IsEmpty);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestServiceRestoredButton_ServiceOutageStart()
		{
			var form = new FormForTest(Group);

			try
			{
				form.Show();

				var serviceRestoredButtonItems = form.UserControl.ServiceRestoredButton.DropDownItems.Find("ServiceOutageStartMenuItem", true);
				AssertEquals(1, serviceRestoredButtonItems.Length);

				serviceRestoredButtonItems[0].PerformClick();

				var lastForm = ZFormModaliser.LastFormShownForTest as ZStmALogAddForm;
				AssertNotNull(lastForm);

				var stmLog = (BaseStmALog)lastForm.BusinessEntity;
				AssertEquals("SVS", stmLog.SL_SE_NKEvent);
				AssertEquals("|STA=OUT", stmLog.SL_Reference);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestServiceRestoredButton_ServiceOutageDowngraded()
		{
			var form = new FormForTest(Group);

			try
			{
				form.Show();

				var serviceRestoredButtonItems = form.UserControl.ServiceRestoredButton.DropDownItems.Find("ServiceOutageDowngradedMenuItem", true);
				AssertEquals(1, serviceRestoredButtonItems.Length);

				serviceRestoredButtonItems[0].PerformClick();

				var lastForm = ZFormModaliser.LastFormShownForTest as ZStmALogAddForm;
				AssertNotNull(lastForm);

				var stmLog = (BaseStmALog)lastForm.BusinessEntity;
				AssertEquals("SVM", stmLog.SL_SE_NKEvent);
				AssertEquals("|STA=DWN", stmLog.SL_Reference);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestServiceRestoredButton_ServiceRestored()
		{
			var form = new FormForTest(Group);

			try
			{
				form.Show();

				var serviceRestoredButtonItems = form.UserControl.ServiceRestoredButton.DropDownItems.Find("ServiceRestoredMenuItem", true);
				AssertEquals(1, serviceRestoredButtonItems.Length);

				serviceRestoredButtonItems[0].PerformClick();

				var lastForm = ZFormModaliser.LastFormShownForTest as ZStmALogAddForm;
				AssertNotNull(lastForm);

				var stmLog = (BaseStmALog)lastForm.BusinessEntity;
				AssertEquals("SVM", stmLog.SL_SE_NKEvent);
				AssertEquals("|STA=RES", stmLog.SL_Reference);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestServiceOutage_ForeColor()
		{
			Group.ING_ServiceOutage = ServiceOutageCodes.Investigating;

			var form = new FormForTest(Group);

			try
			{
				form.Show();
				var serviceOutageDropEdit = (ZDropEdit)form.UserControl.Controls.Find("serviceOutageDropEdit", true).Single();

				AssertEquals(Color.Black, serviceOutageDropEdit.ForeColor);

				Group.ING_ServiceOutage = ServiceOutageCodes.Active;
				AssertEquals(Color.Red, serviceOutageDropEdit.ForeColor);

				Group.ING_ServiceOutage = ServiceOutageCodes.Downgraded;
				AssertEquals(Color.Orange, serviceOutageDropEdit.ForeColor);

				Group.ING_ServiceOutage = ServiceOutageCodes.Restored;
				AssertEquals(Color.Green, serviceOutageDropEdit.ForeColor);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestNextPreviousButton_Reversible()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			registryValue[0].IncidentGroupStatusConfigurations[0].IsReversible = true;
			registryValue[0].IncidentGroupStatusConfigurations[1].IsReversible = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;
			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;
			var groupMessages = Group.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>(); // need call the collection to create default messages
			var autoReplay = groupMessages.First(m => m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.Opening);
			autoReplay.IGM_Message = "Test opening message";
			autoReplay.IGM_IsPublished = true;

			var form = new FormForTest(Group);

			try
			{
				form.Show();

				AssertEquals("Preconditions", IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code, Group.ING_Status);
				AssertEquals(true, Group.NowStage.IsReversible);
				AssertEquals("it's the first stage, there isn't previous stage to go", false, form.UserControl.PreviousButton.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.NextButton.PerformClick();
				AssertEquals("Preconditions", IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, Group.ING_Status);
				AssertEquals(true, Group.NowStage.IsReversible);
				AssertEquals("Enable when IsReversible is true", true, form.UserControl.PreviousButton.Enabled);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				form.UserControl.NextButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("This action is irreversible."));
				AssertEquals("Should not move to next stage when clicking 'No'", IncidentGroupStatusConfigurationConstants.ConfigurationESC.Code, Group.ING_Status);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.NextButton.PerformClick();
				AssertEquals("Preconditions", IncidentGroupStatusConfigurationConstants.ConfigurationACI.Code, Group.ING_Status);
				AssertEquals(false, Group.NowStage.IsReversible);
				AssertEquals("Disable when IsReversible is false", false, form.UserControl.PreviousButton.Enabled);
			}
			finally
			{
				if (form != null)
				{
					((IDisposable)form).Dispose();
				}
			}
		}

		public void TestCompleteIncidentsOnStageChange()
		{
			var registryValue = EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.Value;
			var escStage = registryValue[0].IncidentGroupStatusConfigurations.Cast<IncidentGroupStatusConfiguration>().FirstOrDefault(x => x.Code == "ESC");
			escStage.IncidentCompleted = true;
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			Factory.Save();

			Group.ING_IncidentGroupNumber = "ING000011";
			Group.ING_Type = IncidentGroupStatusConfigurationConstants.MajorIncidentCode;

			var message = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			message.IGM_ING_Group = Group.PK;
			message.IGM_IsPublished = true;
			message.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Interim;
			message.IGM_Message = "Test for Broadcast";

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_IncidentNumber = "WI00665511";
			incident1.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			incident1.RelatedWorkItems.Add(Factory.NewWithValidTestData<WorkItem>());
			var link1 = Group.LinkedIncidents.AddNew();
			link1.INL_ING_Group = Group.PK;
			link1.INL_IM_Incident = incident1.PK;
			link1.INL_IsGroupControlled = true;

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_IncidentNumber = "WI00665522";
			incident2.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.Closed.Other;
			var link2 = Group.LinkedIncidents.AddNew();
			link2.INL_ING_Group = Group.PK;
			link2.INL_IM_Incident = incident2.PK;
			link2.INL_IsGroupControlled = true;

			Group.ING_Status = IncidentGroupStatusConfigurationConstants.ConfigurationINV.Code;

			var allMessages = Group.IncidentManagementGroupMessages;
			var messageOpening = Factory.NewWithValidTestData<IncidentManagementGroupMessage>();
			messageOpening.IGM_ING_Group = Group.PK;
			messageOpening.IGM_IsPublished = true;
			messageOpening.IGM_Type = IncidentManagementGroupMessageTypePairList.Codes.Opening;
			messageOpening.IGM_Message = "Test for message";
			allMessages.Add(messageOpening);

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				Assert(!Group.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.IncidentClosed.Code));
				Assert(!incident1.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.IncidentClosed.Code));
				Assert(!incident2.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.IncidentClosed.Code));
				AssertNull(incident1.EConversation.LastAddedMessageForTest);
				AssertNull(incident1.EConversation.LastAddedMessageForTest);
			});

			var form = new FormForTest(Group);

			try
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				form.UserControl.NextButton.PerformClick();

				var lastMessagePostedTime = incident2.EConversation.LastAddedMessageForTest.JCM_PostedTimeUtc;

				CombineAssertions("Should close incidents and add logs when changing to stage with IncidentCompleted", () =>
				{
					AssertEquals(2, CountLogs(Group.Logs));
					AssertEquals(1, CountLogs(incident1.Logs));
					AssertEquals(1, CountLogs(incident2.Logs));
					AssertNull(incident1.EConversation.LastAddedMessageForTest);
					Assert(incident2.EConversation.GetTimeOrderedMessages().Any(x => x.Body == "Closed As Closed by Incident Group"));
					AssertEquals("The re-open rule for this incident at time of closure: ALW - Always Allow", incident2.EConversation.LastAddedMessageForTest.JCM_Body);
				});

				Group.ING_Description = "new description";
				form.FireSaveButton();

				CombineAssertions("Making changes in the form without closing or change stage should not try to close incidents again and duplicate logs/messages", () =>
				{
					Assert(Group.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.IncidentClosed.Code));
					Assert(incident1.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.IncidentClosed.Code));
					Assert(incident2.Logs.HasLogWith(x => x.SL_SE_NKEvent == AutoEvents.IncidentClosed.Code));
					AssertNull(incident1.EConversation.LastAddedMessageForTest);
					AssertEquals("Should not add new message", lastMessagePostedTime, incident2.EConversation.LastAddedMessageForTest.JCM_PostedTimeUtc);
				});

				int CountLogs(Logs logs)
				{
					return logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == AutoEvents.IncidentClosed.Code &&
								x.SL_Reference.Contains("closed by Incident Management Group"));
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

		IncidentManagementGroup PrepareHandleStageConcurrencyErrorEnvironment()
		{
			var registryValue = new IncidentGroupTypeCollection();
			var groupType = registryValue.AddNew();
			groupType.GroupType = "MIM";
			foreach (var stage in groupType.IncidentGroupStatusConfigurations)
			{
				stage.ControlIncidents = true;
				stage.CascadeCriticality = true;
				stage.CascadeProductDetails = true;
				stage.GroupCompleted = true;
				stage.IncidentCompleted = true;
				stage.IsReversible = true;
			}

			var tryUpdateStatus = typeof(IncidentManagementGroupDetailsUserControl).GetMethod("TryUpdateStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(tryUpdateStatus);
			EDIDataRegistry.Instance.StageAndDispositionsRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var mainFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var group = mainFactory.NewWithValidTestData<IncidentManagementGroup>();
			group.ING_IncidentGroupNumber = "INGConcurrencyOnly";
			group.ING_Priority = "CR3";
			group.ING_Module = "CAT";
			group.ING_Type = "MIM";
			group.ING_Status = "INV";
			group.ING_Product = ProductTypes.Codes.Enterprise;
			group.ING_Urgency = IncidentManagementGroupConstants.UrgencyCodes.Low;
			foreach (IncidentManagementGroupMessage message in group.IncidentManagementGroupMessages)
			{
				message.IGM_Message = message.IGM_Type;
				message.IGM_IsPublished = true;
			}
			group.RunPreSaveValidation();
			Assert($"The test data should not have errors\r\n{string.Join("\r\n", group.NotificationsIncludingChildren.Select(x => $"{x.Type.EnumValueName}-{x.Message}"))}", !group.HasErrors);
			mainFactory.Save();

			return group;
		}

		public void TestTryUpdateAndSaveStatus_HandleConcurrencyError_ValidateAndSaveBeforeChangingStage()
		{
			var group = PrepareHandleStageConcurrencyErrorEnvironment();

			FormForTest user1Form = null;
			FormForTest user2Form = null;

			try
			{
				var user1Factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var user2Factory = new BusinessObjectFactory() { RefreshEnabled = false };

				var user1GroupData = user1Factory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, group.PK));
				var user2GroupData = user2Factory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, group.PK));
				AssertNotNull(user1GroupData);
				AssertNotNull(user2GroupData);

				user1Form = new FormForTest(user1GroupData);
				user2Form = new FormForTest(user2GroupData);

				user1Form.Show();
				user2Form.Show();

				void ReloadData(string tag)
				{
					user1GroupData.Reload();
					user2GroupData.Reload();
					Assert($"[{tag}] user1's data should be reloaded", !user1GroupData.HasChanges);
					Assert($"[{tag}] user2's data should be reloaded", !user2GroupData.HasChanges);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				var user1Page = user1Form.UserControl;
				var user2Page = user2Form.UserControl;

				ReloadData("1");
				#region Case: [Has local changes]There are conflicts in fields other than the stage.

				user1GroupData.ING_Description = "ABCDEF";
				user1Form.FireSaveButton();

				var user2PreviousStage = user2GroupData.NowStage.Code;
				user2GroupData.ING_Description = "123456";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				user2Page.NextButton.PerformClick();

				Assert(user2GroupData.HasChanges);
				AssertEquals("Normal column should be merged with database", user1GroupData.ING_Description, user2GroupData.ING_Description);
				Assert("The merged fields should have warning message", user2GroupData.ING_DescriptionInfo.GetWarnings().Any(x => x.Message.Contains("has changed this field.")));
				AssertEquals($"Stage change aborted as conflicting changes require attention. Please review the form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Stage should not be changed if error occurred while saving", user2PreviousStage, user2GroupData.NowStage.Code);

				#endregion

				ReloadData("2");

				#region Case: [Has local changes]Other user updated the stage

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				user1Page.NextButton.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				user1Page.NextButton.PerformClick(); // user1 set group's stage to 3
				AssertEquals("user1 should update the group's stage to ACI", user1GroupData.ING_Status, "ACI");
				AssertEquals("user2's group stage should be INV", user2GroupData.ING_Status, "INV");

				user2PreviousStage = user2GroupData.NowStage.Code;
				user2GroupData.ING_RN_NKCountry = "AU";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				user2Page.NextButton.PerformClick();

				AssertContains($"has updated the stage of this Group to Active Incident. Please reload form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Stage should not be allowed merge", user2PreviousStage, user2GroupData.NowStage.Code);

				#endregion

				ReloadData("3");

				#region Case:[No Changes before updating stage]Other fields were changed by other session

				AssertEquals(user1GroupData.ING_Status, "ACI");
				AssertEquals(user2GroupData.ING_Status, "ACI");

				user1GroupData.ING_Description = "user1 updated the description";
				user1Form.FireSaveButton();
				Assert("user1's data should not have changes", !user1GroupData.HasChanges);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				user2Page.PreviousButton.Enabled = true;
				user2Page.PreviousButton.PerformClick();
				AssertEquals("user2's data should be reloaded when click button", "user1 updated the description", user2GroupData.ING_Description);
				AssertEquals("user2's stage should be updated", "ESC", user2GroupData.ING_Status);
				Assert("user2's data should not have changes", !user2GroupData.HasChanges);

				#endregion

				ReloadData("4");

				#region Case:[No Changes before updating stage]Stage was changed by other session

				AssertEquals("ESC", user1GroupData.ING_Status);
				AssertEquals("ESC", user2GroupData.ING_Status);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				user1Page.PreviousButton.Enabled = true;
				user1Page.PreviousButton.PerformClick();
				AssertEquals(user1GroupData.ING_Status, "INV");
				Assert(user1GroupData.IsInDatabase);
				Assert(!user2GroupData.HasChanges);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				user2Page.NextButton.PerformClick();
				AssertContains($"has updated the stage of this Group to {user1GroupData.NowStage.DescriptionOnGroup}. Please review the form.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("user2's group stage should not go to the next stage", "INV", user2GroupData.ING_Status);

				#endregion
			}
			finally
			{
				user1Form?.Dispose();
				user2Form?.Dispose();
			}
		}

		public void TestTryUpdateAndSaveStatus_HandleConcurrencyError_UpdateStage()
		{
			var group = PrepareHandleStageConcurrencyErrorEnvironment();

			FormForTest user1Form = null;
			FormForTest user2Form = null;

			try
			{
				var user1Factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var user2Factory = new BusinessObjectFactory() { RefreshEnabled = false };

				var user1GroupData = user1Factory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, group.PK));
				var user2GroupData = user2Factory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, group.PK));
				AssertNotNull(user1GroupData);
				AssertNotNull(user2GroupData);

				user1Form = new FormForTest(user1GroupData);
				user2Form = new FormForTest(user2GroupData);

				user1Form.Show();
				user2Form.Show();

				void ReloadData(string tag)
				{
					user1GroupData.Reload();
					user2GroupData.Reload();
					Assert($"[{tag}] user1's data should be reloaded", !user1GroupData.HasChanges);
					Assert($"[{tag}] user2's data should be reloaded", !user2GroupData.HasChanges);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				var user1Page = user1Form.UserControl;
				var user2Page = user2Form.UserControl;

				var tryUpdateStatusMethod = typeof(IncidentManagementGroupDetailsUserControl).GetMethod("TryUpdateStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var trySaveStatusChangesMethod = typeof(IncidentManagementGroupDetailsUserControl).GetMethod("TrySaveStatusChanges", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				AssertNotNull("Can not find the update method", tryUpdateStatusMethod);
				AssertNotNull("Can not find the save method", trySaveStatusChangesMethod);

				ReloadData("1");

				#region Another user updated the group's other fields

				var nextStatus = user2GroupData.GetNextStatus();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				tryUpdateStatusMethod.Invoke(user2Page, new object[] { nextStatus, "" });

				user1GroupData.ING_Description = "1";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				user1Form.FireSaveButton();

				trySaveStatusChangesMethod.Invoke(user2Page, Array.Empty<object>());
				AssertEquals("ING_Status cannot be allowed merge", nextStatus.Code, user2GroupData.ING_Status);
				AssertEquals("ING_Description should be allowed merge", "1", user2GroupData.ING_Description);

				user1Factory.Save();
				user2Factory.Save();

				#endregion

				ReloadData("2");
				#region Another user updated the stage

				nextStatus = user2GroupData.GetNextStatus();
				AssertEquals("ACI", nextStatus.Code);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				tryUpdateStatusMethod.Invoke(user2Page, new object[] { nextStatus, "" });

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				user1Page.PreviousButton.Enabled = true;
				user1Page.PreviousButton.PerformClick();
				Assert(user1GroupData.IsInDatabase);
				AssertEquals("INV", user1GroupData.ING_Status);

				trySaveStatusChangesMethod.Invoke(user2Page, null);
				AssertContains($"has updated the stage of this Group to Investigation. Please reload form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("ING_Status cannot be allowed merge", nextStatus.Code, user2GroupData.ING_Status);

				#endregion
			}
			finally
			{
				user1Form?.Dispose();
				user2Form?.Dispose();
			}
		}

		[TestDate(2022, 01, 05)]
		public void TestOutageDurationRefreshTimer()
		{
			GroupWithLog.ING_Description = "ING:DESC";
			GroupWithLog.ING_ServiceOutage = ServiceOutageCodes.Restored;
			using (var form = new FormForTest(GroupWithLog))
			{
				form.Show();
				var control = (IncidentManagementGroupDetailsUserControl)form.UserControl;
				var outageDurationTimeLabel = GetFieldValue<ZLabel>(control, "outageDurationTimeLabel");
				AssertEquals("", outageDurationTimeLabel.Text);

				using (GroupWithLog.GetValidationSuspender())
				using (GroupWithLog.SuspendOnValueChanged("ING_ServiceOutage"))
				using (GroupWithLog.SuspendSettingHasChangesIncludingChildren())
				{
					var serviceSuspendedTask = Factory.NewWithValidTestData<ProcessTask>();
					serviceSuspendedTask.P9_Type = Core.Constants.Workflow.MilestoneType;
					((ITriggerConditions)serviceSuspendedTask).TriggerEventCode = AutoEvents.ServiceSuspendedCode;
					serviceSuspendedTask.SetMilestoneActualDateForTest(new DateTime(2022, 01, 01, 22, 30, 20));
					GroupWithLog.Milestones.Add(serviceSuspendedTask);
					AssertEquals("Milestone Setup Error", GroupWithLog.OutageDuration);
				}

				AssertContains("OnElementChanged()", GroupWithLog.MethodLogs.ToString());
				GroupWithLog.MethodLogs.Clear();
				AssertEquals("", outageDurationTimeLabel.Text);
				CallMethod(control, "OutageDurationRefreshTimer_Tick", null, new EventArgs());
				AssertEquals("Milestone Setup Error", outageDurationTimeLabel.Text);
				AssertNotContains("OutageDurationRefreshTimer_Tick should not call OnElementChanged()", "OnElementChanged()", GroupWithLog.MethodLogs.ToString());
			}
		}

		public void TestTriageAssistButtonVisibility()
		{
			using (EDIDataRegistry.Instance.EnableTriageEngineModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new FormForTest(GroupWithLog))
				{
					form.Show();
					var control = (IncidentManagementGroupDetailsUserControl)form.UserControl;
					var triageAssistButton = control.Controls.Find("TriageAssistButton", true).FirstOrDefault();
					AssertEquals("Should show button since triage engine is enabled", true, triageAssistButton.Visible);
					form.Dispose();
				}
			}
		}

		public void TestTriageLabelVisibility()
		{
			var incidentManagementGroup = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			using (var form = new FormForTest(incidentManagementGroup))
			{
				form.Show();
				var control = (IncidentManagementGroupDetailsUserControl)form.UserControl;
				var triageAssistCaption = control.Controls.Find("triageAssistCaption", true).FirstOrDefault();
				var triageAssistLabel = control.Controls.Find("triageAssistLabel", true).FirstOrDefault();
				AssertEquals("Should show triageAssistCaption since triage engine is enabled", true, triageAssistCaption.Visible);
				AssertEquals("Should show triageAssistLabel since triage engine is enabled", true, triageAssistLabel.Visible);
			}
		}

		public void TestTriageAssistButton()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();

			using (var form = new FormForTest(incidentGroup))
			{
				form.Show();
				var control = (IncidentManagementGroupDetailsUserControl)form.UserControl;
				var triageAssistButton = (ZButton)form.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(control.TriageForm);
				triageAssistButton.PerformClick();
				AssertNotNull(control.TriageForm);
				AssertEquals(true, control.TriageForm.Visible);
				control.TriageForm.Close();
				form.Dispose();
			}
		}

		public void TestOnTriageAssistSaved_OverrideProductClassification()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();
			incidentGroup.ING_Module = "ADV";
			incidentGroup.ING_Priority = "CR7";
			incidentGroup.ING_Product = "ENT";
			incidentGroup.ING_ProductArea = "MDM";
			incidentGroup.ING_IMT_Triage = ZGuid.Empty;

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = "SPT";
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			Factory.Save();

			using (var incidentManagementForm = new FormForTest(incidentGroup))
			{
				incidentManagementForm.Show();
				var control = (IncidentManagementGroupDetailsUserControl)incidentManagementForm.UserControl;
				var triageAssistButton = (ZButton)control.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(control.TriageForm);
				triageAssistButton.PerformClick();
				using (var triageForm = control.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals(incidentGroup.ING_IMT_Triage, triage1.PK);
					AssertEquals(incidentGroup.ING_ProductArea, triage1.IMT_ProductArea);
					AssertEquals(incidentGroup.ING_Product, triage1.IMT_Product);
					AssertEquals(incidentGroup.ING_Module, triage1.IMT_Module);
				}
			}
		}

		public void TestOnTriageAssistSaved_OverrideLinkedIncidentsProductClassification()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();
			incidentGroup.ING_Module = "ADV";
			incidentGroup.ING_Priority = "CR7";
			incidentGroup.ING_Product = "ENT";
			incidentGroup.ING_ProductArea = "MDM";
			incidentGroup.ING_IMT_Triage = ZGuid.Empty;

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = "SPT";
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.IM_Module = "AAA";
			incident1.IM_Priority = "CR1";
			incident1.IM_Product = "AAA";

			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			incident2.IM_Module = "AAA";
			incident2.IM_Priority = "CR1";
			incident2.IM_Product = "AAA";

			var link1 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link1.INL_IM_Incident = incident1.PK;

			var link2 = Factory.NewWithValidTestData<IncidentManagementLink>();
			link2.INL_IM_Incident = incident2.PK;

			incidentGroup.LinkedIncidents.Add(link1);
			incidentGroup.LinkedIncidents.Add(link2);

			Factory.Save();

			using (var incidentManagementForm = new FormForTest(incidentGroup))
			{
				incidentManagementForm.Show();
				var control = (IncidentManagementGroupDetailsUserControl)incidentManagementForm.UserControl;
				var triageAssistButton = (ZButton)control.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(control.TriageForm);
				triageAssistButton.PerformClick();
				using (var triageForm = control.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals(incidentGroup.ING_IMT_Triage, triage1.PK);
					AssertEquals(incidentGroup.ING_ProductArea, triage1.IMT_ProductArea);
					AssertEquals(incidentGroup.ING_Product, triage1.IMT_Product);
					AssertEquals(incidentGroup.ING_Module, triage1.IMT_Module);

					foreach (IncidentManagementLink incidentManagementLink in incidentGroup.LinkedIncidents)
					{
						var incident = incidentManagementLink.SupportIncident;
						AssertEquals(incident.IM_IMT_Triage, triage1.PK);
						AssertEquals(incident.IM_Product, triage1.IMT_Product);
						AssertEquals(incident.IM_Module, triage1.IMT_Module);
					}
				}
			}
		}

		public void TestOnTriageAssistSaved_SetCriticalityAndStage_Support()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();
			incidentGroup.ING_Module = "ADV";
			incidentGroup.ING_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incidentGroup.ING_Product = "ENT";
			incidentGroup.ING_ProductArea = "MDM";
			incidentGroup.ING_IMT_Triage = ZGuid.Empty;

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			Factory.Save();

			using (var incidentManagementForm = new FormForTest(incidentGroup))
			{
				incidentManagementForm.Show();
				var control = (IncidentManagementGroupDetailsUserControl)incidentManagementForm.UserControl;
				var triageAssistButton = (ZButton)control.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(control.TriageForm);
				triageAssistButton.PerformClick();
				using (var triageForm = control.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals("Precondition: Triage PK is correctly set", incidentGroup.ING_IMT_Triage, triage1.PK);
					AssertEquals("Precondition: ProductArea is correctly set", incidentGroup.ING_ProductArea, triage1.IMT_ProductArea);
					AssertEquals("Precondition: Product PK is correctly set", incidentGroup.ING_Product, triage1.IMT_Product);
					AssertEquals("Precondition: Module PK is correctly set", incidentGroup.ING_Module, triage1.IMT_Module);
					AssertEquals(incidentGroup.ING_Priority, Constants.CustomerService.CriticalityCodes.CR5_Training);
					AssertEquals(incidentGroup.ING_Category, SupportIncidentCategoriesList.Codes.Support);
				}
			}
		}

		public void TestOnTriageAssistSaved_SetCriticalityAndStage_Compliance()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();
			incidentGroup.ING_Module = "ADV";
			incidentGroup.ING_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incidentGroup.ING_Product = "ENT";
			incidentGroup.ING_ProductArea = "MDM";
			incidentGroup.ING_IMT_Triage = ZGuid.Empty;

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Compliance;
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			Factory.Save();

			using (var incidentManagementForm = new FormForTest(incidentGroup))
			{
				incidentManagementForm.Show();
				var control = (IncidentManagementGroupDetailsUserControl)incidentManagementForm.UserControl;
				var triageAssistButton = (ZButton)control.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(control.TriageForm);
				triageAssistButton.PerformClick();
				using (var triageForm = control.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals("Precondition: Triage PK is correctly set", incidentGroup.ING_IMT_Triage, triage1.PK);
					AssertEquals("Precondition: ProductArea is correctly set", incidentGroup.ING_ProductArea, triage1.IMT_ProductArea);
					AssertEquals("Precondition: Product PK is correctly set", incidentGroup.ING_Product, triage1.IMT_Product);
					AssertEquals("Precondition: Module PK is correctly set", incidentGroup.ING_Module, triage1.IMT_Module);
					AssertEquals(incidentGroup.ING_Priority, Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement);
					AssertEquals(incidentGroup.ING_Category, SupportIncidentCategoriesList.Codes.Support);
				}
			}
		}

		public void TestOnTriageAssistSaved_SetCriticalityAndStage_Service()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();
			incidentGroup.ING_Module = "ADV";
			incidentGroup.ING_Priority = Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest;
			incidentGroup.ING_Product = "ENT";
			incidentGroup.ING_ProductArea = "MDM";
			incidentGroup.ING_IMT_Triage = ZGuid.Empty;

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Service;
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			Factory.Save();

			using (var incidentManagementForm = new FormForTest(incidentGroup))
			{
				incidentManagementForm.Show();
				var control = (IncidentManagementGroupDetailsUserControl)incidentManagementForm.UserControl;
				var triageAssistButton = (ZButton)control.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(control.TriageForm);
				triageAssistButton.PerformClick();
				using (var triageForm = control.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals("Precondition: Triage PK is correctly set", incidentGroup.ING_IMT_Triage, triage1.PK);
					AssertEquals("Precondition: ProductArea is correctly set", incidentGroup.ING_ProductArea, triage1.IMT_ProductArea);
					AssertEquals("Precondition: Product PK is correctly set", incidentGroup.ING_Product, triage1.IMT_Product);
					AssertEquals("Precondition: Module PK is correctly set", incidentGroup.ING_Module, triage1.IMT_Module);
					AssertEquals(incidentGroup.ING_Priority, Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest);
					AssertEquals(incidentGroup.ING_Category, SupportIncidentCategoriesList.Codes.CustomerServiceRequest);
				}
			}
		}

		public void TestOnTriageAssistSaved_DoesNotSetCriticalityAndStage_NonTriageOverridableCriticalities()
		{
			var incidentGroup = Factory.New<IncidentManagementGroup>();
			Factory.Save();
			incidentGroup.ING_Module = "ADV";
			incidentGroup.ING_Priority = Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incidentGroup.ING_Category = SupportIncidentCategoriesList.Codes.Defect;
			incidentGroup.ING_Product = "ENT";
			incidentGroup.ING_ProductArea = "MDM";
			incidentGroup.ING_IMT_Triage = ZGuid.Empty;

			var triage1 = Factory.NewWithValidTestData<IncidentTriage>();
			triage1.IMT_Type = IncidentTriageTypes.Codes.Support;
			triage1.IMT_Module = "ACO";
			triage1.IMT_Product = "ACO";
			triage1.IMT_ProductArea = "CUS";

			Factory.Save();

			using (var incidentManagementForm = new FormForTest(incidentGroup))
			{
				incidentManagementForm.Show();
				var control = (IncidentManagementGroupDetailsUserControl)incidentManagementForm.UserControl;
				var triageAssistButton = (ZButton)control.Controls.Find("TriageAssistButton", true).FirstOrDefault();
				AssertNull(control.TriageForm);
				triageAssistButton.PerformClick();
				using (var triageForm = control.TriageForm)
				{
					triageForm.Show();
					var bizObj = (TriageAssistBusinessObject)triageForm.BusinessEntity;
					bizObj.Parent.TriagePK = triage1.PK;
					bizObj.Factory.Save();

					AssertEquals(incidentGroup.ING_IMT_Triage, triage1.PK);
					AssertEquals(incidentGroup.ING_ProductArea, triage1.IMT_ProductArea);
					AssertEquals(incidentGroup.ING_Product, triage1.IMT_Product);
					AssertEquals(incidentGroup.ING_Module, triage1.IMT_Module);
					// Assert criticality and stage is not overriden set
					AssertEquals(incidentGroup.ING_Priority, Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround);
					AssertEquals(incidentGroup.ING_Category, SupportIncidentCategoriesList.Codes.Defect);
				}
			}
		}

		static T GetFieldValue<T>(object obj, string name)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			FieldInfo fi;
			var type = obj.GetType();

			while ((fi = type.GetField(name, bindingFlags)) == null && (type = type.BaseType) != null)
			{
			}

			return (T)fi.GetValue(obj);
		}

		static void CallMethod(object obj, string methodName, params object[] args)
		{
			var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			MethodInfo mi;
			var type = obj.GetType();

			while ((mi = type.GetMethod(methodName, bindingFlags)) == null && (type = type.BaseType) != null)
			{
			}

			mi.Invoke(obj, args);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var incidentApprovalLookups = new IncidentApprovalLookups(null);
			var validCriticalities = incidentApprovalLookups.CriticalityList.GetAllCodes();
			EDIDataRegistry.CreateProductsAndModulesForTest();

			var collection = new SystemProductCollection();
			var product = collection.AddNew(ProductTypes.Codes.Enterprise, "test", true);
			var validModule = "CAT";
			product.ModuleMappings.AddNew(validModule, "Category Module", ProductAreaList.Codes.ARC, true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			Group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Group.ING_Priority = validCriticalities[0];
			Group.ING_Module = validModule;

			GroupWithLog = Factory.NewWithValidTestData<IncidentManagementGroupForTest>();
			GroupWithLog.ING_Priority = validCriticalities[0];
			GroupWithLog.ING_Module = validModule;
		}

		IncidentManagementGroup Group { get; set; }
		IncidentManagementGroupForTest GroupWithLog { get; set; }

		protected override void TearDown()
		{
			base.TearDown();
			Group.Delete();
			GroupWithLog.Delete();
		}

		class FormForTest : ZForm
		{
			IncidentManagementGroupDetailsUserControlForTest userControl;

			public IncidentManagementGroupDetailsUserControlForTest UserControl => userControl ?? (userControl = new IncidentManagementGroupDetailsUserControlForTest());

			public FormForTest(object incidentManagementGroup)
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

		class IncidentManagementGroupDetailsUserControlForTest : IncidentManagementGroupDetailsUserControl
		{
			public new ZButton PreviousButton => base.PreviousButton;
			public new ZButton NextButton => base.NextButton;
			public new ZGrid StagesGrid => base.StagesGrid;
			public new ZToolStripDropDownButton ServiceRestoredButton => base.ServiceRestoredButton;
		}

		class IncidentManagementGroupForTest : IncidentManagementGroup
		{
			public IncidentManagementGroupForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void OnElementChanged()
			{
				base.OnElementChanged();
				MethodLogs.AppendLine("OnElementChanged()");
			}

			readonly public StringBuilder MethodLogs = new StringBuilder();
		}

		#endregion
	}
}
