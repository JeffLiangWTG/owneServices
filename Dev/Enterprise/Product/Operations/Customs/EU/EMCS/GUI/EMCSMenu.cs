using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ObjectHandle = CargoWise.Application.ObjectHandle;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public class EMCSMenu : ZMenuItem
	{
		public EMCSMenu(EMCSJobDeclaration declaration)
		{
			this.declaration = declaration;
			CaptionResourceString = TopLevelMenuCaptionResourceStringData;

			var messageSenderObjects = ObjectFactory.Get<Hashtable>("EMCSMessageSenderProvider");
			var messageSenderHandle = (ObjectHandle)messageSenderObjects[this.declaration.CountryCode.ToString()];

			if (messageSenderHandle != null)
			{
				messageSender = (ISendEMCSMessages)messageSenderHandle.GetObject(this.declaration);
				var validationSettings = new ZMenuItem(ResString.GetMultilingualString("2ECB6429-05AC-4751-A4AA-42DEAEE99BA8", "Validation Settings"), ValidationSettings_Click);
				var submitDraftMovementRequest = new ZMenuItem(ResString.GetMultilingualString("FB234718-A84F-456A-8DF4-3ACBB0F2EFE6", "Submit Draft Movement Request"), SubmitDraftMovementRequest_Click);
				var cancellationOfEad = new ZMenuItem(ResString.GetMultilingualString("15D70696-46B5-4701-9430-B4D9C2ECB123", "Cancellation of EAD"), CancellationOfEAD_Click);
				var changeOfDestination = new ZMenuItem(ResString.GetMultilingualString("5790D568-A286-47B0-A62F-D76FD501433F", "Change of Destination"), ChangeOfDestination_Click);
				var explanationOnDelayForDelivery = new ZMenuItem(ResString.GetMultilingualString("605A9939-7A89-446A-8251-97F7BE8C22B4", "Explanation On Delay for Delivery"), ExplanationOnDelayForDelivery_Click);
				var explanationOnReasonForShortage = new ZMenuItem(ResString.GetMultilingualString("5454F55D-B05C-43A4-98B7-911F5CD810E9", "Explanation On Reason for Shortage"), ExplanationOnReasonForShortage_Click);
				var acceptOrRejectReportOfReceipt = new ZMenuItem(ResString.GetMultilingualString("CDCB5AC7-CCAD-48A4-96B3-47462B7257CA", "Accept or Reject Report of Receipt"), AcceptOrRejectReportOfReceipt_Click);
				var alertOrRejectOfEad = new ZMenuItem(ResString.GetMultilingualString("5C0F763C-59D5-4C06-A87D-CDEB1EC1C261", "Alert or Rejection of EAD"), AlertOrRejectEad_Click);

				MenuItems.AddRange(new MenuItem[]
				{
					validationSettings,
					submitDraftMovementRequest,
					cancellationOfEad,
					changeOfDestination,
					explanationOnDelayForDelivery,
					explanationOnReasonForShortage,
					acceptOrRejectReportOfReceipt,
					alertOrRejectOfEad
				});
			}
		}
		readonly ISendEMCSMessages messageSender;

		internal static ResourceStringData TopLevelMenuCaptionResourceStringData => Res.GetData("5C6386B3-098B-4B2A-B6E1-E224287997BE", "&EMCS");

		void ValidationSettings_Click(object sender, EventArgs args)
		{
			ZFormModaliser.ShowDialogAndDispose(new EMCSValidationSettingsForm(declaration));
		}

		void SubmitDraftMovementRequest_Click(object sender, EventArgs args)
		{
			var canSend = true;

			if (declaration.MessageSendingConfiguration.ShouldCheckCanSend)
			{
				var inactiveInformationText = Res.GetString("0020F688-3687-4513-A7E1-FAD0AA38CBEC", "Active only if Declaration Type is '1' and Registration Status is empty or 'CAN' and Message Status is not 'SNT' or 'ACK'.");
				var registeredStatus = declaration.JE_EntryStatus;
				var activeCondition = IsEMCSConsignor && (registeredStatus.IsEmpty || registeredStatus == EntryStatusList.Codes.CAN) && !IsEMCSMessageStatusSNTOrACK;
				canSend = declaration.CanSend(MainForm, inactiveInformationText, () => activeCondition);
			}

			if (canSend)
			{
				var sendingParent = new MinimalSendingActionParent(declaration);
				using (var messageSendingForm = new EMCSMessageSendingForm<EMCSMessageSendingAction>(sendingParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						var action = (EMCSMessageSendingAction)sendingParent.SendingObjectsCollection.Single();
						Do(s => s.SendDraftMovementRequest(action));
					}
				}
			}
		}

		void CancellationOfEAD_Click(object sender, EventArgs args)
		{
			var canSend = true;

			if (declaration.MessageSendingConfiguration.ShouldCheckCanSend)
			{
				var inactiveInformationText = Res.GetString("232AA1A1-6362-4EF1-83CC-D4F1F3E7FAF4", "Active only if Declaration Type is '1' and Registration Status is 'REG' and Message Status is not 'SNT' or 'ACK'.");
				var activeCondition = IsEMCSConsignor && IsEMCSRegistered && !IsEMCSMessageStatusSNTOrACK;
				canSend = declaration.CanSend(MainForm, inactiveInformationText, () => activeCondition);
			}

			if (canSend)
			{
				var cancellation = new CancellationSendingActionParent(declaration);
				using (var messageSendingForm = new CancellationMessageSendingForm(cancellation))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						var action = (CancellationSendingAction)cancellation.SendingObjectsCollection.Single();
						Do(s => s.SendCancellation(action));
					}
				}
			}
		}

		void ChangeOfDestination_Click(object sender, EventArgs args)
		{
			var canSend = true;

			if (declaration.MessageSendingConfiguration.ShouldCheckCanSend)
			{
				var inactiveInformationText = Res.GetString("8BB3D234-497D-4D77-B8EA-EF73F7EA6286", "Active only if Declaration Type is '1' and Registration Status is 'REG', 'ALT', 'CHG', 'COM' or 'REM' and Message Status is not 'SNT' or 'ACK'.");
				var activeCondition = IsEMCSConsignor && IsEntryStatusInREG_ALT_CHG_COM_REM && !IsEMCSMessageStatusSNTOrACK;
				canSend = declaration.CanSend(MainForm, inactiveInformationText, () => activeCondition);
			}

			if (canSend)
			{
				var sendingParent = new MinimalSendingActionParent(declaration);
				using (var messageSendingForm = new EMCSMessageSendingForm<EMCSMessageSendingAction>(sendingParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						var action = (EMCSMessageSendingAction)sendingParent.SendingObjectsCollection.Single();
						Do(s => s.SendChangeOfDestination(action));
					}
				}
			}
		}

		void ExplanationOnDelayForDelivery_Click(object sender, EventArgs args)
		{
			var canSend = true;

			if (declaration.MessageSendingConfiguration.ShouldCheckCanSend)
			{
				var inactiveInformationText = Res.GetString("4E5FE100-7CC9-48FE-9C5B-E399F13F0D15", "Active only if Registration Status is 'REM' and Message Status is not 'SNT' or 'ACK'.");
				var activeCondition = IsEntryStatusInREM && !IsEMCSMessageStatusSNTOrACK;
				canSend = declaration.CanSend(MainForm, inactiveInformationText, () => activeCondition);
			}

			if (canSend)
			{
				var explanationOnDelay = new ExplanationOnDelaySendingActionParent(declaration);
				using (var messageSendingForm = new ExplanationOnDelayMessageSendingForm(explanationOnDelay))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						var action = (ExplanationOnDelaySendingAction)explanationOnDelay.SendingObjectsCollection.Single();
						Do(s => s.SendDeliveryDelayExplanation(action));
					}
				}
			}
		}

		void ExplanationOnReasonForShortage_Click(object sender, EventArgs args)
		{
			var canSend = true;

			if (declaration.MessageSendingConfiguration.ShouldCheckCanSend)
			{
				var inactiveInformationText = Res.GetString("D9BF3FD6-F771-43EC-8EAA-B6798A5F8625", "Active only if Registration Status is 'COM' and Message Status is not 'SNT' or 'ACK'.");
				var activeCondition = IsEMCSCompleted && !IsEMCSMessageStatusSNTOrACK;
				canSend = declaration.CanSend(MainForm, inactiveInformationText, () => activeCondition);
			}

			if (canSend)
			{
				declaration.ZG_ExplanationOnReasonForShortageValidation = true;
				var reasonForShortage = new ReasonForShortageSendingActionParent(declaration);
				using (var messageSendingForm = new ReasonForShortageMessageSendingForm(reasonForShortage))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						var action = (ReasonForShortageSendingAction)reasonForShortage.SendingObjectsCollection.Single();
						Do(s => s.SendReasonForShortageExplanation(action));
					}
				}
			}
		}

		void AcceptOrRejectReportOfReceipt_Click(object sender, EventArgs args)
		{
			var canSend = true;

			if (declaration.MessageSendingConfiguration.ShouldCheckCanSend)
			{
				var inactiveInformationText = Res.GetString("E2F90529-4F75-49F1-8616-CD75FE8AF2C9", "Active only if Declaration Type is '2' and Registration Status is 'REG', 'ALT', 'REM', 'EVT' or 'CHG' and Message Status is not 'SNT' or 'ACK'.");
				var activeCondition = IsEMCSConsignee && IsEntryStatusInREG_ALT_EVT_CHG_REM && !IsEMCSMessageStatusSNTOrACK;
				canSend = declaration.CanSend(MainForm, inactiveInformationText, () => activeCondition);
			}

			if (canSend)
			{
				var actionParent = new ReportOfReceiptSendingActionParent(declaration);
				using (var messageSendingForm = new ReportOfReceiptMessageSendingForm(actionParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						var action = (ReportOfReceiptSendingAction)actionParent.SendingObjectsCollection.Single();
						Do(s => s.SendReportOfReceipt(action));
					}
				}
			}
		}

		void AlertOrRejectEad_Click(object sender, EventArgs args)
		{
			var canSend = true;

			if (declaration.MessageSendingConfiguration.ShouldCheckCanSend)
			{
				var inactiveInformationText = Res.GetString("FD1B5586-52CE-47AC-97FC-007105CFF6FE", "Active only if Declaration Type is '2' and Registration Status is 'REG', 'ALT', 'REM', 'EVT' or 'CHG' and Message Status is not 'SNT' or 'ACK'.");
				var activeCondition = IsEMCSConsignee && IsEntryStatusInREG_ALT_EVT_CHG_REM && !IsEMCSMessageStatusSNTOrACK;
				canSend = declaration.CanSend(MainForm, inactiveInformationText, () => activeCondition);
			}

			if (canSend)
			{
				var alertOrReject = new AlertOrRejectSendingActionParent(declaration);
				using (var messageSendingForm = new AlertOrRejectMessageSendingForm(alertOrReject))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
					{
						var action = (AlertOrRejectSendingAction)alertOrReject.SendingObjectsCollection.Single();
						Do(s => s.SendAlertOrRejectEad(action));
					}
				}
			}
		}

		void Do(Action<ISendEMCSMessages> send)
		{
			try
			{
				send(messageSender);
				declaration.Factory.Save();
				Globals.Message.Show(Res.GetString("88C895D0-07E5-4259-99D2-EECE84B6E67C", "The message has been sent."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		ZBool IsEMCSRegistered => declaration.JE_EntryStatus == EntryStatusList.Codes.REG;

		ZBool IsEntryStatusInREM => declaration.JE_EntryStatus == EntryStatusList.Codes.REM;

		ZBool IsEntryStatusInREG_ALT_CHG_COM_REM => declaration.JE_EntryStatus == EntryStatusList.Codes.REG || declaration.JE_EntryStatus == EntryStatusList.Codes.ALT || declaration.JE_EntryStatus == EntryStatusList.Codes.CHG
			|| declaration.JE_EntryStatus == EntryStatusList.Codes.COM || declaration.JE_EntryStatus == EntryStatusList.Codes.REM;

		ZBool IsEntryStatusInREG_ALT_EVT_CHG_REM => declaration.JE_EntryStatus == EntryStatusList.Codes.REG || declaration.JE_EntryStatus == EntryStatusList.Codes.ALT || declaration.JE_EntryStatus == EntryStatusList.Codes.CHG
			|| declaration.JE_EntryStatus == EntryStatusList.Codes.EVT || declaration.JE_EntryStatus == EntryStatusList.Codes.REM;

		ZBool IsEMCSCompleted => declaration.JE_EntryStatus == EntryStatusList.Codes.COM;

		ZBool IsEMCSConsignor => declaration.JE_DeclarantType == EMCSEntryTypeList.Codes.Consignor;

		ZBool IsEMCSConsignee => declaration.JE_DeclarantType == EMCSEntryTypeList.Codes.Consignee;

		ZBool IsEMCSMessageStatusSNTOrACK => declaration.IsMessageStatusSentOrAcknowledged;

		readonly EMCSJobDeclaration declaration;

		ZForm MainForm => (ZForm)GetMainMenu()?.GetForm();
	}
}
