using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class EMCSMenuTest : TestCaseWithFactory
	{
		public void TestValidationSettings_Click()
		{
			using (var form = new ZForm(emcsDeclaration))
			using (var menu = new EMCSMenu(emcsDeclaration))
			{
				form.Menu.MenuItems.Add(menu);
				var menuItem = menu.MenuItems.FindByText("Validation Settings");

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ShowDialogsInTest = true;

				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (EMCSValidationSettingsForm)obj;
					dialog.FindSingle<ZCheckBox>("ExplanationOnReasonForShortageCheckBox").Checked = true;
					dialog.FindSingle<ZButton>("ConfirmButton").PerformClick();
				});

				menuItem.PerformClick();
				AssertEquals("ZG_ExplanationOnReasonForShortageValidation", true, emcsDeclaration.ZG_ExplanationOnReasonForShortageValidation);
			}
		}

		public void TestPreviewMessageEventConnected()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			var parent = new MinimalSendingActionParent(emcsDeclaration);

			using (var form = new EMCSMessageSendingForm<EMCSMessageSendingAction>(parent))
			{
				form.PreviewMessageCheckBox.Checked = true;
				form.Show();
				var button = form.FindSingle<ZButton>("SendButton");
				button.Enabled = true;
				button.PerformClick();
				var action = (EMCSMessageSendingAction)parent.SendingObjectsCollection.Single();
				ZFormModaliser.LastFormShownDialogForTest = null;
				action.MessageCreated("Some Message Text for Testing");
				AssertType<MessageEditForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestSubmitDraftMovementRequest_Click_SendMessage() => AssertSendMessage(EMCSEntryTypeList.Codes.Consignor, string.Empty, SubmitDraftMovementRequestMenuName, () =>
		{
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				var dialog = (EMCSMessageSendingForm<EMCSMessageSendingAction>)obj;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var sendWitheMessageErrorsCheckBox = dialog.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWitheMessageErrorsCheckBox.Checked = true;
				var sendButton = dialog.FindSingle<ZButton>("SendButton");
				sendButton.PerformClick();
			});
		});

		public void TestSubmitDraftMovementRequest_Click_IsActive()
		{
			AssertEquals("Precondition", true, emcsDeclaration.MessageSendingConfiguration.ShouldCheckCanSend);
			const string activeRuleMessage = "Active only if Declaration Type is '1' and Registration Status is empty or 'CAN' and Message Status is not 'SNT' or 'ACK'.";
			AssertIsActive(new[] { EMCSEntryTypeList.Codes.Consignor }, new[] { string.Empty, "CAN" }, new[] { EDIMessage.Status.Sent, EDIMessage.Status.Acknowledged }, activeRuleMessage, SubmitDraftMovementRequestMenuName);
		}

		public void TestCancellationOfEAD_Click_SendMessage()
		{
			AssertSendMessage(EMCSEntryTypeList.Codes.Consignor, EntryStatusList.Codes.REG, CancellationOfEADMenuName, () =>
			{
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (CancellationMessageSendingForm)obj;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var sendWitheMessageErrorsCheckBox = dialog.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
					sendWitheMessageErrorsCheckBox.Checked = true;
					var cancellation = (CancellationSendingAction)dialog.BusinessEntity.SendingObjectsCollection[0];
					cancellation.Reason = EMCSCancellationReasonList.Codes._0;
					cancellation.Information = "Information";
					dialog.FindSingle<ZButton>("SendButton").PerformClick();
				});
			});
		}

		public void TestCancellationOfEAD_Click_IsActive()
		{
			AssertEquals("Precondition", true, emcsDeclaration.MessageSendingConfiguration.ShouldCheckCanSend);
			const string activeRuleMessage = "Active only if Declaration Type is '1' and Registration Status is 'REG' and Message Status is not 'SNT' or 'ACK'.";
			AssertIsActive(new[] { EMCSEntryTypeList.Codes.Consignor }, new[] { EntryStatusList.Codes.REG }, new[] { EDIMessage.Status.Sent, EDIMessage.Status.Acknowledged }, activeRuleMessage, CancellationOfEADMenuName);
		}

		public void TestChangeOfDestination_Click_SendMessage() => AssertSendMessage(EMCSEntryTypeList.Codes.Consignor, EntryStatusList.Codes.REG, ChangeOfDestinationMenuName, () =>
		{
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				var dialog = (EMCSMessageSendingForm<EMCSMessageSendingAction>)obj;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var sendWitheMessageErrorsCheckBox = dialog.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWitheMessageErrorsCheckBox.Checked = true;
				var sendButton = dialog.FindSingle<ZButton>("SendButton");
				sendButton.PerformClick();
			});
		});

		public void TestChangeOfDestination_Click_IsActive()
		{
			AssertEquals("Precondition", true, emcsDeclaration.MessageSendingConfiguration.ShouldCheckCanSend);
			const string activeRuleMessage = "Active only if Declaration Type is '1' and Registration Status is 'REG', 'ALT', 'CHG', 'COM' or 'REM' and Message Status is not 'SNT' or 'ACK'.";
			AssertIsActive(new[] { EMCSEntryTypeList.Codes.Consignor }, new[] { EntryStatusList.Codes.REG, EntryStatusList.Codes.CHG, EntryStatusList.Codes.ALT, EntryStatusList.Codes.COM, EntryStatusList.Codes.REM }, new[] { EDIMessage.Status.Sent, EDIMessage.Status.Acknowledged }, activeRuleMessage, ChangeOfDestinationMenuName);
		}

		public void TestExplanationOnDelayForDelivery_Click_SendMessage()
		{
			AssertSendMessage(EMCSEntryTypeList.Codes.Consignor, EntryStatusList.Codes.REM, ExplanationOnDelayForDeliveryMenuName, () =>
			{
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (ExplanationOnDelayMessageSendingForm)obj;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var sendWitheMessageErrorsCheckBox = dialog.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
					sendWitheMessageErrorsCheckBox.Checked = true;
					var explanationOnDelay = (ExplanationOnDelaySendingAction)dialog.BusinessEntity.SendingObjectsCollection[0];
					explanationOnDelay.ExplanationCode = "6";
					explanationOnDelay.MessageRole = "1";
					dialog.FindSingle<ZButton>("SendButton").PerformClick();
				});
			});
		}

		public void TestExplanationOnDelayForDelivery_Click_IsActive()
		{
			AssertEquals("Precondition", true, emcsDeclaration.MessageSendingConfiguration.ShouldCheckCanSend);
			const string activeRuleMessage = "Active only if Registration Status is 'REM' and Message Status is not 'SNT' or 'ACK'.";
			AssertIsActive(new[] { EMCSEntryTypeList.Codes.Consignor, EMCSEntryTypeList.Codes.Consignee }, new[] { EntryStatusList.Codes.REM }, new[] { EDIMessage.Status.Sent, EDIMessage.Status.Acknowledged }, activeRuleMessage, ExplanationOnDelayForDeliveryMenuName);
		}

		public void TestExplanationOnReasonForShortage_Click_SendMessage()
		{
			emcsDeclaration.EMCSPackages.AddNew();
			CombineAssertions(() =>
			{
				AssertSendMessage(EMCSEntryTypeList.Codes.Consignor, EntryStatusList.Codes.COM, ExplanationOnReasonForShortageMenuName, () =>
				{
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						var dialog = (ReasonForShortageMessageSendingForm)obj;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						var sendWitheMessageErrorsCheckBox = dialog.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
						sendWitheMessageErrorsCheckBox.Checked = true;
						var reasonForShortage = (ReasonForShortageSendingAction)dialog.BusinessEntity.SendingObjectsCollection[0];
						reasonForShortage.GeneralExplanation = "General Explanation Text";
						dialog.FindSingle<ZButton>("SendButton").PerformClick();
					});
				});
				AssertEquals("ZG_ExplanationOnReasonForShortageValidation", true, emcsDeclaration.ZG_ExplanationOnReasonForShortageValidation);
			});
		}

		public void TestExplanationOnReasonForShortage_Click_IsActive()
		{
			AssertEquals("Precondition", true, emcsDeclaration.MessageSendingConfiguration.ShouldCheckCanSend);
			const string activeRuleMessage = "Active only if Registration Status is 'COM' and Message Status is not 'SNT' or 'ACK'.";
			AssertIsActive(new[] { EMCSEntryTypeList.Codes.Consignor, EMCSEntryTypeList.Codes.Consignee }, new[] { EntryStatusList.Codes.COM }, new[] { EDIMessage.Status.Sent, EDIMessage.Status.Acknowledged }, activeRuleMessage, ExplanationOnReasonForShortageMenuName);
		}

		public void TestAcceptOrRejectReportOfReceipt_Click_SendMessage()
		{
			AssertSendMessage(EMCSEntryTypeList.Codes.Consignee, EntryStatusList.Codes.REG, AcceptOrRejectReportOfReceiptMenuName, () =>
			{
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var dialog = (ReportOfReceiptMessageSendingForm)obj;
					dialog.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox").Checked = true;

					var reportOfReceipt = (ReportOfReceiptSendingAction)dialog.BusinessEntity.SendingObjectsCollection[0];
					reportOfReceipt.ArrivalDate = new ZDateTime(2020, 03, 24);
					reportOfReceipt.ReceiptResult = EMCSReceiptResultList.Codes.ReceiptRefused;
					reportOfReceipt.ComplementaryInformation = "Information";

					dialog.FindSingle<ZButton>("SendButton").PerformClick();
				});
			});
		}

		public void TestAcceptOrRejectReportOfReceipt_Click_IsActive()
		{
			AssertEquals("Precondition", true, emcsDeclaration.MessageSendingConfiguration.ShouldCheckCanSend);
			const string activeRuleMessage = "Active only if Declaration Type is '2' and Registration Status is 'REG', 'ALT', 'REM', 'EVT' or 'CHG' and Message Status is not 'SNT' or 'ACK'.";
			AssertIsActive(new[] { EMCSEntryTypeList.Codes.Consignee }, new[] { EntryStatusList.Codes.REG, EntryStatusList.Codes.ALT, EntryStatusList.Codes.REM, EntryStatusList.Codes.EVT, EntryStatusList.Codes.CHG }, new[] { EDIMessage.Status.Sent, EDIMessage.Status.Acknowledged }, activeRuleMessage, AcceptOrRejectReportOfReceiptMenuName);
		}

		public void TestAlertOrRejectEad_Click_SendMessage()
		{
			AssertSendMessage(EMCSEntryTypeList.Codes.Consignee, EntryStatusList.Codes.REG, AlertOrRejectMenuName, () =>
			{
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var dialog = (AlertOrRejectMessageSendingForm)obj;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var sendWithMessageErrorsCheckBox = dialog.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
					sendWithMessageErrorsCheckBox.Checked = true;
					var alertOrReject = (AlertOrRejectSendingAction)dialog.BusinessEntity.SendingObjectsCollection[0];
					alertOrReject.RejectedFlag = ZBool.True;
					alertOrReject.DateOfAlertOrRejection = ZDateTime.Today;
					var reason = alertOrReject.AlertOrRejectionReasons.AddNew();
					reason.Reason = EMCSAlertRejectionCodeList.Codes._0;
					reason.Information = "Information";
					dialog.FindSingle<ZButton>("SendButton").PerformClick();
				});
			});
		}

		public void TestAlertOrRejectEad_Click_IsActive()
		{
			AssertEquals("Precondition", true, emcsDeclaration.MessageSendingConfiguration.ShouldCheckCanSend);
			const string activeRuleMessage = "Active only if Declaration Type is '2' and Registration Status is 'REG', 'ALT', 'REM', 'EVT' or 'CHG' and Message Status is not 'SNT' or 'ACK'.";
			AssertIsActive(new[] { EMCSEntryTypeList.Codes.Consignee }, new[] { EntryStatusList.Codes.REG, EntryStatusList.Codes.ALT, EntryStatusList.Codes.REM, EntryStatusList.Codes.EVT, EntryStatusList.Codes.CHG }, new[] { EDIMessage.Status.Sent, EDIMessage.Status.Acknowledged }, activeRuleMessage, AlertOrRejectMenuName);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			var sendEMCSMessagesmock = new Mock<ISendEMCSMessages>();
			var mock = new Mock<ObjectHandle>();
			mock.Setup(x => x.GetObject(It.IsAny<EMCSJobDeclaration>())).Returns(sendEMCSMessagesmock.Object);

			var emcsMessageSenderProvider = new KeyObjectHandleDictionaryObject
			{
				{ Core.Constants.CountryCodes.Latvia, mock.Object }
			};
			ObjectFactory.Substitute("EMCSMessageSenderProvider", emcsMessageSenderProvider);
		}
		EMCSJobDeclaration emcsDeclaration;

		void AssertSendMessage(string validDeclarantType, string validEntryStatus, ZString menuItemName, Action zformModaliserAction)
		{
			var entryStatusList = new EntryStatusList();
			using (var form = new ZForm(emcsDeclaration))
			using (var menu = new EMCSMenu(emcsDeclaration))
			{
				form.Menu.MenuItems.Add(menu);
				var menuItem = menu.MenuItems.FindByText(menuItemName);
				emcsDeclaration.JE_DeclarantType = validDeclarantType;
				emcsDeclaration.JE_EntryStatus = validEntryStatus;
				emcsDeclaration.JE_MessageStatus = ZString.Empty;
				emcsDeclaration.EMCSPackages.AddNew();
				Factory.Save();

				Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				ZFormModaliser.ShowDialogsInTest = true;
				zformModaliserAction?.Invoke();
				menuItem.PerformClick();
				AssertEquals("Message has been send notification", "The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertIsActive(string[] validDeclarantTypes, string[] validEntryStatuses, string[] invalidMessageStatuses, ZString activeRuleMessage, ZString menuItemName)
		{
			var entryStatusList = new EntryStatusList();
			var validMessageStatus = ZString.Empty;
			CombineAssertions(() =>
			{
				using (var form = new ZForm(emcsDeclaration))
				using (var menu = new EMCSMenu(emcsDeclaration))
				{
					form.Menu.MenuItems.Add(menu);
					var menuItem = menu.MenuItems.FindByText(menuItemName);
					emcsDeclaration.JE_DeclarantType = validDeclarantTypes.FirstOrDefault();
					var firstValidEntryStatus = validEntryStatuses.FirstOrDefault();
					emcsDeclaration.JE_EntryStatus = firstValidEntryStatus;
					if (!entryStatusList.ContainsCode(firstValidEntryStatus))
					{
						Factory.Save();
					}

					foreach (var invalidMessageStatus in invalidMessageStatuses)
					{
						emcsDeclaration.JE_MessageStatus = invalidMessageStatus;
						UnitTestUserNotification.Instance.ClearMessages();
						menuItem.PerformClick();
						AssertEquals($"Invalid MessageStatus '{invalidMessageStatus}'", activeRuleMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}

					emcsDeclaration.JE_MessageStatus = validMessageStatus;
					if (!Array.Exists(validEntryStatuses, x => string.IsNullOrEmpty(x)))
					{
						emcsDeclaration.JE_EntryStatus = ZString.Empty;
						UnitTestUserNotification.Instance.ClearMessages();
						menuItem.PerformClick();
						AssertEquals("Invalid EntryStatus ''", activeRuleMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}

					foreach (var validEntryStatus in validEntryStatuses)
					{
						emcsDeclaration.JE_EntryStatus = validEntryStatus;
						if (!entryStatusList.ContainsCode(validEntryStatus))
						{
							Factory.Save();
						}
						UnitTestUserNotification.Instance.ClearMessages();
						menuItem.PerformClick();
						AssertNotEquals($"EntryStatus '{validEntryStatus}' and all valid", activeRuleMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}

					foreach (var validDeclarantType in validDeclarantTypes.Skip(1))
					{
						emcsDeclaration.JE_DeclarantType = validDeclarantType;
						UnitTestUserNotification.Instance.ClearMessages();
						menuItem.PerformClick();
						AssertNotEquals($"DeclarationType '{validDeclarantType}' and all valid", activeRuleMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			});
		}

		const string AlertOrRejectMenuName = "Alert or Rejection of EAD";
		const string AcceptOrRejectReportOfReceiptMenuName = "Accept or Reject Report of Receipt";
		const string ExplanationOnReasonForShortageMenuName = "Explanation On Reason for Shortage";
		const string ExplanationOnDelayForDeliveryMenuName = "Explanation On Delay for Delivery";
		const string ChangeOfDestinationMenuName = "Change of Destination";
		const string CancellationOfEADMenuName = "Cancellation of EAD";
		const string SubmitDraftMovementRequestMenuName = "Submit Draft Movement Request";
	}
}
