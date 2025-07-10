using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AsycudaManifestHeader = Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
			manifestHeader = (AsycudaManifestHeader)header;
		}

		AsycudaManifestHeader manifestHeader { get; }

		EUManifestMessageSender EUManifestMessageSender => eUManifestMessageSender ?? (eUManifestMessageSender = new EUManifestMessageSender(manifestHeader));
		EUManifestMessageSender eUManifestMessageSender;

		public override ResourceString MenuCaption => ResString.GetMultilingualString("aee762aa-f28a-46b4-92c8-3f8f382301dc", "EU ICS2 Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				AddFilingOrAmendmentMessageMenuItem(mainForm, menuItems);
				AddCancelManifestMenuItem(mainForm, menuItems);
				AddHRCMScreeningResponseMenuItem(mainForm, menuItems);
				AddArrivalNotificationMenuItem(mainForm, menuItems);
				AddAdditionalInformationReplyMenuItem(mainForm, menuItems);
				AddStatusRequestMenuItem(mainForm, menuItems);
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		bool HasRegistrationInfo => !manifestHeader.RegistrationDate.IsEmpty && !manifestHeader.RegistrationNumber.IsEmpty;

		void AddCancelManifestMenuItem(ZForm mainForm, List<ZMenuItem> menuItems)
		{
			if (HasRegistrationInfo)
			{
				var caption = ResString.GetMultilingualString("34566fe5-cbaa-4f34-8768-09cc853981c0", "Cancel Manifest");
				var cancelManifestMenuItem = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, manifestHeader, () =>
				{
					var confirmationMessage = ResString.GetMultilingualString("57E10E09-3095-459D-8A2F-858334EC08B6", "Are you sure you want to cancel this Manifest?");
					var confirmationString = ResString.GetMultilingualString("558FA73E-4F41-457E-AE6C-03587E70FEF2", "yes");
					if (Globals.Message.ShowConfirmation(confirmationMessage, caption, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK)
					{
						SendMessage(() => EUManifestMessageSender.SendMessage(MessageTypes.Codes.Q04));
					}
				});
				cancelManifestMenuItem.Enabled = manifestHeader.RegistrationStatus != EUICS2CustomsStatusList.Codes.CAN;
			}
		}

		void AddArrivalNotificationMenuItem(ZForm mainForm, List<ZMenuItem> menuItems)
		{
			if (manifestHeader.IsCarrierManifest)
			{
				var caption = ResString.GetMultilingualString("62132357-959f-4025-8209-2a186d9174ee", "Arrival Notification");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, manifestHeader, () => ArrivalNotificationMenuItem_Click());
			}
		}

		void AddHRCMScreeningResponseMenuItem(ZForm mainForm, List<ZMenuItem> menuItems)
		{
			if (HasRegistrationInfo &&
				manifestHeader.Messages.Any(message => ((EDIMessage)message).EM_MessageType == MessageTypes.Codes.Q03))
			{
				var caption = ResString.GetMultilingualString("7f9d1a80-d723-49d5-ba8a-e454aa87e860", "HRCM Screening Response");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, manifestHeader, () =>
				{
					var amendedItemsHeader = new ICS2AmendedItemsHeader(MessageTypes.Codes.R03, manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFS);

					if (IsAmendedItemSelectedOrSkipped(amendedItemsHeader, caption))
					{
						SendMessage(() => EUManifestMessageSender.SendAmendmentMessage(amendedItemsHeader));
					}
				});
			}
		}

		void ArrivalNotificationMenuItem_Click()
		{
			if (manifestHeader.AMA_A_ARV.IsEmpty)
			{
				Globals.Message.Show(ResString.GetMultilingualString("7b09d4be-2277-4c30-8309-684b62938d59", "There is No Actual Arrival Date Entered."), ResString.GetMultilingualString("69e756c1-665c-4dcf-8431-95c428755187", "Unable to complete your request"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				SendMessage(() => EUManifestMessageSender.SendMessage(MessageTypes.Codes.N06));
			}
		}

		void AddFilingOrAmendmentMessageMenuItem(ZForm mainForm, List<ZMenuItem> menuItems)
		{
			if (!HasRegistrationInfo)
			{
				var caption = ResString.GetMultilingualString("34F4E9D4-912B-4CC7-8211-0CC42D58799A", "Send &Manifest");
				var filingMenuItem = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, manifestHeader, () => SendMessage(() => EUManifestMessageSender.SendFilingMessage()));
				filingMenuItem.Enabled = EnableFilingMenuItem();
			}
			else
			{
				var caption = ResString.GetMultilingualString("D3B3C550-5162-45F5-BF62-DA088B6A133F", "&Amend Manifest");
				var amendMenuItem = MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, manifestHeader, () =>
				{
					var amendedItemsHeader = new ICS2AmendedItemsHeader(manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);

					try
					{
						if (amendedItemsHeader.HasAmendedItems())
						{
							using (var dialog = new EUICS2AmendedItemsSelectionDialog(amendedItemsHeader, caption))
							{
								ZFormModaliser.ShowDialogWithoutDispose(dialog, mainForm);

								if (dialog.DialogResult == DialogResult.OK)
								{
									SendMessage(() => EUManifestMessageSender.SendAmendmentMessage(amendedItemsHeader));
								}
							}
						}
						else
						{
							SendMessage(() => EUManifestMessageSender.SendMessage(amendedItemsHeader.MessageType));
						}
					}
					finally
					{
						amendedItemsHeader.Delete();
					}
				});

				amendMenuItem.Enabled = EnableAmendMenuItem();
			}
		}

		void AddAdditionalInformationReplyMenuItem(ZForm mainForm, List<ZMenuItem> menuItems)
		{
			if (HasRegistrationInfo && manifestHeader.Messages.Any(message => ((EDIMessage)message).EM_MessageType == MessageTypes.Codes.Q02))
			{
				var caption = ResString.GetMultilingualString("E984E733-8D98-4934-9A83-5C90BB9527B1", "Additional Information Reply");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, manifestHeader, () =>
				{
					var amendedItemsHeader = new ICS2AmendedItemsHeader(MessageTypes.Codes.R02, manifestHeader, EUICS2ReferralRequestTypeList.Codes.CL735_RFI, EUICS2ReferralRequestTypeList.Codes.CL735_AMD);

					if (IsAmendedItemSelectedOrSkipped(amendedItemsHeader, caption))
					{
						SendMessage(() => EUManifestMessageSender.SendAmendmentMessage(amendedItemsHeader));
					}
				});
			}
		}

		void AddStatusRequestMenuItem(ZForm mainForm, List<ZMenuItem> menuItems)
		{
			if (HasRegistrationInfo)
			{
				var caption = ResString.GetMultilingualString("ED4823DB-236A-40B5-BD60-717265A1A841", "Status Request");
				var q05Sender = new Q05MessageSender(manifestHeader);
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, manifestHeader, () =>
				{
					SendMessage(() => q05Sender.SendMessage());
				}, validateManifest: false);
			}
		}

		bool IsAmendedItemSelectedOrSkipped(ICS2AmendedItemsHeader header, string caption)
		{
			var result = false;

			if (header.HasAmendedItems())
			{
				using (var dialog = new EUICS2AmendedItemsSelectionDialog(header, caption))
				{
					ZFormModaliser.ShowDialogWithoutDispose(dialog, mainForm);
					result = dialog.DialogResult == DialogResult.OK;
				}
			}
			else
			{
				result = true;
			}

			return result;
		}

		bool EnableFilingMenuItem() => manifestHeader.SpecificCircumstanceIndicator.ToString() switch
		{
			_ when manifestHeader.RegistrationStatus == EUICS2CustomsStatusList.Codes.CAN => false,

			EUICS2SpecificCircumstanceList.Codes.F10 => true,
			EUICS2SpecificCircumstanceList.Codes.F13 => true,
			EUICS2SpecificCircumstanceList.Codes.F14 => true,
			EUICS2SpecificCircumstanceList.Codes.F15 => true,
			EUICS2SpecificCircumstanceList.Codes.F16 => true,
			EUICS2SpecificCircumstanceList.Codes.F17 => true,
			EUICS2SpecificCircumstanceList.Codes.F22 => true,
			EUICS2SpecificCircumstanceList.Codes.F23 => true,
			EUICS2SpecificCircumstanceList.Codes.F24 => true,
			EUICS2SpecificCircumstanceList.Codes.F25 => true,
			EUICS2SpecificCircumstanceList.Codes.F26 => true,
			EUICS2SpecificCircumstanceList.Codes.F40 => true,
			EUICS2SpecificCircumstanceList.Codes.F41 => true,
			EUICS2SpecificCircumstanceList.Codes.F43 => true,
			EUICS2SpecificCircumstanceList.Codes.F44 => true,
			EUICS2SpecificCircumstanceList.Codes.F50 => true,
			EUICS2SpecificCircumstanceList.Codes.F51 => true,

			_ => false,
		};

		bool EnableAmendMenuItem() => manifestHeader.SpecificCircumstanceIndicator.ToString() switch
		{
			_ when manifestHeader.RegistrationStatus == EUICS2CustomsStatusList.Codes.CAN => false,

			EUICS2SpecificCircumstanceList.Codes.F10 => true,
			EUICS2SpecificCircumstanceList.Codes.F13 => true,
			EUICS2SpecificCircumstanceList.Codes.F14 => true,
			EUICS2SpecificCircumstanceList.Codes.F15 => true,
			EUICS2SpecificCircumstanceList.Codes.F16 => true,
			EUICS2SpecificCircumstanceList.Codes.F17 => true,
			EUICS2SpecificCircumstanceList.Codes.F22 => true,
			EUICS2SpecificCircumstanceList.Codes.F23 => true,
			EUICS2SpecificCircumstanceList.Codes.F24 => true,
			EUICS2SpecificCircumstanceList.Codes.F26 => true,
			EUICS2SpecificCircumstanceList.Codes.F40 => true,
			EUICS2SpecificCircumstanceList.Codes.F41 => true,
			EUICS2SpecificCircumstanceList.Codes.F50 => true,
			EUICS2SpecificCircumstanceList.Codes.F51 => true,

			_ => false,
		};

		void SendMessage(Func<string> sendMessage)
		{
			var result = sendMessage();
			Globals.Message.Show(result);
		}
	}
}
