using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public static class HouseBillAndCloseMessageSendingExtension
	{
		static ZBool NeedToAutoSendWithdrawCloseMessage(CusCAeMHMaster master)
		{
			var result = master != null && (master.BP_MasterHouseCCNInfo.HasChanges || master.BP_CBSACarrierCodeInfo.HasChanges) && master.IsLodged;
			if (result)
			{
				var lastOutgoingWithdrawMessage = master.Messages.GetLastMessage(EDIMessage.ApplicationCodes.CAACI, MessageTypeList.Codes.ACIForwarderClose, EDIMessage.Direction.Transmit, ZString.Empty, MessageSubTypeCodes.Codes.Cancellation);
				if (lastOutgoingWithdrawMessage != null)
				{
					var lastIncomingWithdrawMessage = master.Messages.GetLastMessage(EDIMessage.ApplicationCodes.CAACI, MessageTypeList.Codes.ACIForwarderClose, EDIMessage.Direction.Receive, ZString.Empty, MessageSubTypeCodes.Codes.Cancellation);
					result = lastIncomingWithdrawMessage != null && lastIncomingWithdrawMessage.EM_SystemCreateTimeUtc > lastOutgoingWithdrawMessage.EM_SystemCreateTimeUtc;
				}
			}
			return result;
		}

		static ContinueWithSave AutoSendingWithdrawalMessageForMasterBill(CusCAeMHMaster master, ISendsMessagesToCustoms sender, AutoSendingWithdrawalMessageHelper helper)
		{
			var result = ContinueWithSave.Yes;
			if (master != null)
			{
				switch (Globals.Message.Show(CloseMessageHasBeenSubmittedMessage, CloseMessageHasBeenSubmittedCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
				{
					case DialogResult.Yes:
						GenerateWithdrawalMessages(master, sender, true, helper);
						break;
					case DialogResult.No:
						GenerateWithdrawalMessages(master, sender, false, helper);
						break;
					default:
						result = ContinueWithSave.No;
						break;
				}
			}
			return result;
		}

		static void GenerateWithdrawalMessages(CusCAeMHMaster master, ISendsMessagesToCustoms sender, bool needAutoResendCloseReport, AutoSendingWithdrawalMessageHelper helper)
		{
			var closeWrapper = new CusCAeMHMasterCloseWrapper(master);
			closeWrapper.UpdateIsShouldSend(o => true);
			var manager = new ACIForwarderCloseMessageManager(closeWrapper, null);
			var messages = manager.GenerateWithdrawalMessages(manager.BusinessObject);

			if (messages != null && messages.Any())
			{
				helper.MessageHasBeenGenerated = true;
				sender.NotifyUserOfASuccessfulSend(Res.GetString("7034c948-0337-4e51-a51d-5dcc1f2d56cf", "{0} cancel message(s) have been generated.", messages.Length));

				if (needAutoResendCloseReport)
				{
					foreach (var message in messages.OfType<ACIForwarderCloseMessage>())
					{
						message.NeedAutoCloseReport = needAutoResendCloseReport;
					}
				}
			}
		}

		internal static string CloseMessageHasBeenSubmittedCaption
		{
			get { return Res.GetString("5fb01741-296f-4f81-8910-0f601da4184b", "A close message has already been submitted"); }
		}

		internal static string CloseMessageHasBeenSubmittedMessage
		{
			get { return Res.GetString("aaacdb25-99a2-49c3-9b77-a8f89a0767bd", @"You are changing the Previous CCN or FF Carrier Code, a close message has already been submitted. You need to submit a cancel message for this close message, would you like to
	- Submit the Cancel Message and resubmit Close Message on acceptance of the Cancel message, click Yes
	- Submit the Cancel Message only, click No
	- Cancel, click Cancel"); }
		}

		static ZBool NeedToAutoSendAmendHouseBillMessages(CusCAeMHMaster master)
		{
			return master != null && (master.BP_PrimaryCCNInfo.HasChanges || master.BP_MasterHouseCCNInfo.HasChanges) && master.HouseBills.Any(housebill => housebill.IsLodged);
		}

		static ContinueWithSave AutoSendingWithdrawalMessageForAmendHouseBills(CusCAeMHMaster master, ISendsMessagesToCustoms sender, AutoSendingWithdrawalMessageHelper helper)
		{
			var result = ContinueWithSave.Yes;
			if (master != null)
			{
				if (!helper.MessageHasBeenGenerated)
				{
					if (Globals.Message.Show(HouseBillsHaveBeenAcceptedMessage, HouseBillsHaveBeenAcceptedCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
					{
						GenerateWithdrawalMessages(master, sender, false, helper);
					}
					else
					{
						result = ContinueWithSave.No;
					}
				}
			}
			return result;
		}

		public static ContinueWithSave ShowAutoSendingWithdrawalMessageDialog(this CusCAeMHMaster master, ISendsMessagesToCustoms sender)
		{
			var result = ContinueWithSave.Yes;
			var messageHasBeenGenarated = new AutoSendingWithdrawalMessageHelper();
			if (NeedToAutoSendWithdrawCloseMessage(master))
			{
				result = AutoSendingWithdrawalMessageForMasterBill(master, sender, messageHasBeenGenarated);
			}
			if (result == ContinueWithSave.Yes && NeedToAutoSendAmendHouseBillMessages(master))
			{
				result = AutoSendingWithdrawalMessageForAmendHouseBills(master, sender, messageHasBeenGenarated);
			}
			return result;
		}

		internal static string HouseBillsHaveBeenAcceptedCaption
		{
			get { return Res.GetString("2a6e1c90-bc97-4884-8d51-5e2bc6775cea", "Close Message needs to be withdrawn."); }
		}

		internal static string HouseBillsHaveBeenAcceptedMessage
		{
			get { return Res.GetString("6bc8be25-8d03-46b6-ba4d-479eb493691b", "House bill Message have to be resent if the Primary/Previous CCN is modified. Close Message needs to be withdrawn prior to resending House Bill Message. Do you want to withdraw Close message?"); }
		}

		class AutoSendingWithdrawalMessageHelper
		{
			public AutoSendingWithdrawalMessageHelper()
			{
				MessageHasBeenGenerated = false;
			}

			public bool MessageHasBeenGenerated { get; set; }
		}
	}
}
