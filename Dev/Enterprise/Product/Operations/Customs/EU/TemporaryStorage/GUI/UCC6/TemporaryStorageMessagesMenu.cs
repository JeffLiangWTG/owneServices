using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TemporaryStorageMessagesMenu : ZMenuItem
	{
		public TemporaryStorageMessagesMenu(ZForm parentForm)
		{
			CaptionResourceString = Res.GetData("C4D438A4-A2AC-4901-89F4-6B2643282EF3", "&Messages");
			this.parentForm = parentForm;
			RefreshMenuItems();
		}

		readonly ZForm parentForm;

		public TemporaryStorageHeader Header => (TemporaryStorageHeader)parentForm.BusinessEntity;

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			RefreshMenuItems();
		}

		protected virtual void RefreshMenuItems()
		{
			MenuItems.Clear();
			MenuItems.AddRange(CreateMenuItems().ToArray());
			SetMenuItemVisibility(SetAsFailedFromTransmission, () => Header?.IsSent ?? false);
			SetMenuItemVisibility(TsRegisterManagementMenuItem, () => Header?.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible ?? false);
			SetMenuItemVisibility(TsRegisterManagementMenuItemLine, () => TsRegisterManagementMenuItem.Visible);
		}

		protected virtual IEnumerable<ZMenuItem> CreateMenuItems()
		{
			yield return SetAsFailedFromTransmission;
			yield return SendToCustomsMenuItem;
			yield return TsRegisterManagementMenuItemLine;
			yield return TsRegisterManagementMenuItem;
		}

		ZMenuItem SetAsFailedFromTransmission => setAsFailedFromTransmission ??= new ZMenuItem(ResString.GetMultilingualString("AB540183-0FC9-43AA-B954-AF326EB2DEBC", "Set Entry as Failed From Transmission"), SetAsFailedFromTransmissionClick);
		ZMenuItem setAsFailedFromTransmission;

		protected ZMenuItem SendToCustomsMenuItem => sendToCustomsMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("52B22C93-6626-4725-9F88-884F422772FF", "&Send To Customs"), SendMessageMenu_OnClick);
		protected ZMenuItem sendToCustomsMenuItem;

		ZMenuItem TsRegisterManagementMenuItemLine => tsRegisterManagementMenuItemLine ??= new ZMenuItem("-");
		ZMenuItem tsRegisterManagementMenuItemLine;

		ZMenuItem TsRegisterManagementMenuItem => tsRegisterManagementMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("A4D3E1F2-0C8B-4A7C-9F5E-6D1B2A0E5F7D", "TS Register Management"), [SelectInventoryMenuItem]);
		ZMenuItem tsRegisterManagementMenuItem;

		ZMenuItem SelectInventoryMenuItem => selectInventoryMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("1626B2DD-EA40-4471-94E1-89CC86EBB813", "Select Inventory"), SelectInventoryMenuItem_Click);
		ZMenuItem selectInventoryMenuItem;

		void SendMessageMenu_OnClick(object sender, EventArgs e)
		{
			if (SaveAndContinue())
			{
				var header = Header;
				var messageNumInCurrentFactory = header.Messages.Count;
				header.Messages.Reload(true);
				var messages = header.Messages;
				if (messages.Count != messageNumInCurrentFactory)
				{
					Globals.Message.ShowError(Res.GetString("D7F2E21B-63D7-4BFE-95AD-1F8DEBEAF559", "A new message has been attached to this temporary storage, please reopen the form before sending a message."));
				}
				else
				{
					SendToCustomsCore(messages, header);
				}
			}
		}

		protected virtual void SendToCustomsCore(EDIMessageCollection messages, TemporaryStorageHeader header)
		{
			if (messages.IsWaitingForAResponse)
			{
				var caption = Res.GetString("5531C8C3-EEA0-4056-9FC2-BADABD21D21A", "Resend to Customs");
				var lastOutgoingMessage = header.Messages.LastOutgoingMessage;
				var messageType = lastOutgoingMessage is IMessageTypeProvider provider ? provider.MessageType : lastOutgoingMessage?.EM_MessageSubType ?? ZString.Empty;
				var message = Res.GetString("5207A7AE-F8E0-4121-8D65-DE7FAC44D4E5", "The last outgoing {0} message is waiting for response, you can only resend another {0} message, do you want to resend this message?",
					messageType);

				var dialogResult = Globals.Message.ShowConfirmation(message, caption, Res.GetString("3BC2244E-2F1F-4EF4-BFF7-98B847120C90", "yes"), MessageBoxIcon.Question);
				if (dialogResult == DialogResult.OK)
				{
					if (header.MessagingProvider is TemporaryStorageMessagingProvider messagingProvider)
					{
						if (NeedToPreviewMessage)
						{
							var messageSendingObjectParent = header.MessageSendingConfiguration.GetNewMessageSendingObjectParent(header);
							var sendingObject = GetSendingObject(messageSendingObjectParent);
							sendingObject.MessageType = messageType;
							sendingObject.SetReadOnlyIncludingChildren(true);

							using (var messageSendingForm = GetNewMessageSendingForm(messageSendingObjectParent))
							{
								if (ZFormModaliser.ShowDialogAndDispose(messageSendingForm) == DialogResult.OK)
								{
									messagingProvider.SendMessage(sendingObject, new SendsMessagesToCustomsGUI(), Globals.Message);
								}
							}
						}
						else
						{
							var sendingObject = header.MessageSendingConfiguration.GetNewMessageSendingObject(header);
							sendingObject.MessageType = messageType;
							messagingProvider.SendMessage(sendingObject, new SendsMessagesToCustomsGUI(), Globals.Message);
						}
					}
					else
					{
						ReportPNTSMessagesNotSupportedError();
					}
				}
			}
			else
			{
				if (header.MessagingProvider is TemporaryStorageMessagingProvider messagingProvider)
				{
					var messageSendingObjectParent = header.MessageSendingConfiguration.GetNewMessageSendingObjectParent(header);
					using (var messageSendingForm = GetNewMessageSendingForm(messageSendingObjectParent))
					{
						if (ZFormModaliser.ShowDialogAndDispose(messageSendingForm) == DialogResult.OK)
						{
							var objectsToSend = GetSendingObject(messageSendingObjectParent);
							messagingProvider.SendMessage(objectsToSend, new SendsMessagesToCustomsGUI(), Globals.Message);
						}
					}
				}
				else
				{
					ReportPNTSMessagesNotSupportedError();
				}
			}

			static TemporaryStorageMessageSendingObject GetSendingObject(BaseMessageSendingObjectParent messageSendingObjectParent) => (TemporaryStorageMessageSendingObject)messageSendingObjectParent.SelectedSendingObjects.Single();
		}

		void SetAsFailedFromTransmissionClick(object sender, EventArgs ev)
		{
			var confirmationMessage = ResString.GetMultilingualString("3790DCA6-1A94-42FA-B50A-1A7E0A48FD93", "Are you sure you want to set this Entry as Failed from Transmission?");
			var caption = ResString.GetMultilingualString("39092307-2EED-4244-880D-84E6B4FC9B76", "Failed from Transmission");
			var confirmationPrompt = ResString.GetMultilingualString("5135D98A-7772-4055-8230-70A1CBBB5F33", "If you are absolutely sure you want to set this Entry as Failed From Transmission, please type:");
			var confirmationString = ResString.GetMultilingualString("1FC26519-4B31-4A08-B7A4-7E32C1F35FCA", "yes");

			if (Header.IsSent
						&& Globals.Message.ShowConfirmation(confirmationMessage, caption, confirmationPrompt, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK)
			{
				Header.AMA_MessageStatus = LogicalStatusList.Codes.Failed;
				Globals.Message.Show(ResString.GetMultilingualString("CCEE3137-C85C-4F44-88EF-12705B42214C", "Entry was set to Failed from Transmission"));
			}
		}

		void SelectInventoryMenuItem_Click(object sender, EventArgs e)
		{
			Globals.Message.ShowError((NoResString)"Is going to be implemented in 'Coding - Reqs 3 & 4' WF from 'WI00749380 - EU - TS - Trigger selection of Goods from G5 V1 Expedition'.");
		}

		protected virtual bool NeedToPreviewMessage => false;

		protected virtual MessageSendingFormWithValidationDetails GetNewMessageSendingForm(BaseMessageSendingObjectParent messageSendingObjectParent) => new MessageSendingFormWithValidationDetails(messageSendingObjectParent);

		void ReportPNTSMessagesNotSupportedError()
		{
			Globals.Message.ShowError(Res.GetString("4C644CDF-06AF-4041-9F7C-0A29755617C6", "PNTS messages are not supported in your country."));
		}

		protected bool SaveAndContinue()
		{
			return CustomsPlugIn.FormPreSaved(Header, parentForm);
		}

		protected void SetMenuItemVisibility(ZMenuItem menuItem, Func<bool> isVisible)
		{
			if (menuItem != null)
			{
				menuItem.Visible = isVisible();
			}
		}
	}
}
