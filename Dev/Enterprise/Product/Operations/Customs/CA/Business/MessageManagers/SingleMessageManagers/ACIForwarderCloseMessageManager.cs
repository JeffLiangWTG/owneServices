//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.Business.MessageManagers;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Customs.CA.Business.MessageProcessors;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Customs.Common.Shared;
	using Enterprise.Messaging.MessageBuilders;

	public class ACIForwarderCloseMessageManager : CAEManifestForwarderMessageManager
	{
		public ACIForwarderCloseMessageManager(IACIForwarderCloseProvider closeMessageProvider, IUserNotification notification, bool isForced = false)
			: base(closeMessageProvider, new ACIEManifestForwaderStatusCalculator(MessageTypeList.Descriptions.ACIForwarderClose), notification)
		{
			IsForced = isForced;
		}

		#region Overrides

		public override void RefreshDetails()
		{
			base.RefreshDetails();
			var masterBill = DataWrapper.TopLevelBusinessObject as CusCAeMHMaster;
			if (masterBill != null)
			{
				masterBill.EnableAndSynchronise(true);
			}
		}

		public override string MessageFriendlyName
		{
			get { return Res.GetString("B9AB6D28-57D9-4D2F-A06A-DD97C77542AF", "ACI eManifest Forwarder Manifest Close Report for Previous CCN {0}", DataWrapper.PreviousCCN); }
		}

		protected override bool ShouldSendWithAmendment
		{
			get { return !DataWrapper.AmendReasonCode.IsEmpty && !DataWrapper.ATA.IsEmpty; }
		}

		#region CanSendThisMessage

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			if (ActionPurpose == ActionPurpose.Change)
			{
				actionCode = MessageSubTypes.Request;
			}
			_ = base.CanSendThisMessage(actionCode, out messageText);
			if (messageText.IsEmpty && actionCode == MessageSubTypes.Create && !DataWrapper.ReadyToClose)
			{
				messageText = Res.GetString("D902BDD8-C83B-43F5-AF06-72029B529FC6", "not all the house bills have been accepted.");
			}

			if (messageText.IsEmpty)
			{
				if (actionCode != MessageSubTypes.Withdraw && !DataWrapper.RelatedCCNs.Any())
				{
					messageText = Res.GetString("6C1B8FC5-27FA-4361-A77E-1224CFFCAFC6", "there are no valid related house CCNs entered for this job, please check the house bills have been reported.");
				}
				else if (actionCode == MessageSubTypes.Withdraw && !CanSendWithdrawal)
				{
					messageText = Res.GetString("EA67906C-BFA1-437B-BD48-16E3BEDD5389", "the Manifest has not been reported yet.");
				}

				if (actionCode == MessageSubTypes.Request && (DataWrapper.AmendReasonCode.IsEmpty || DataWrapper.ATA.IsEmpty))
				{
					messageText = Res.GetString("07F0E29B-7E82-46AC-8766-BA091826EA73", "the Amendment reason and ATA are required for Amend message, please check them have been entered.");
				}
			}
			return messageText.IsEmpty;
		}

		#endregion

		bool HasAcceptedMessage()
		{
			return DataWrapper.Messages.GetMatchingMessages(EDIMessage.ApplicationCodes.CAACI, new ZString[] { MessageTypeList.Codes.ACIForwarderClose }, EDIMessage.Direction.Receive)
				.Cast<ACIForwarderCloseMessage>().Any(message => message.EM_MessageSubType != MessageSubTypeCodes.Codes.Cancellation && new EManifestResponseWrapper(message).IsAccepted);
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAWithdrawal()
		{
			var result = base.GetNotificationsForSendingAWithdrawal();

			if (IsForced && !HasAcceptedMessage())
			{
				result.AddWarning(Res.GetString("B346875F-6264-4281-96A0-4E663AA3AB42", "No Close message has been accepted yet, we strongly recommend not to sent a cancel message now."));
			}

			return result;
		}

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new ACIForwarderCloseMessageBuilder(DataWrapper, actionCode);
		}

		new internal IACIForwarderCloseProvider DataWrapper
		{
			get { return (IACIForwarderCloseProvider)base.DataWrapper; }
		}

		public IEnumerable<HouseCCNInstruction> AllWrappedCCN
		{
			get { return DataWrapper.AllCCNs; }
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAnOriginal()
		{
			var result = base.GetNotificationsForSendingAnOriginal();

			DataWrapper.RelatedCCNs.ForEach(ccn =>
			{
				if (!IsHouseStatusEligable(ccn) && !(isHouseBillIncludedToSend != null && isHouseBillIncludedToSend(ccn)))
				{
					result.AddWarning(Res.GetString("14aa08c6-6814-464f-8aee-1614a34f5178", "The House Bill CCN: {0} is not in an active customs status nor included in this sending batch, are you sure you want to include this CCN in the close report?", ccn));
				}
			}
			);

			return result;
		}

		internal delegate bool CheckHouseBillSendingStatus(string houseCCN);
		internal CheckHouseBillSendingStatus isHouseBillIncludedToSend;

		bool IsHouseStatusEligable(string houseCCN)
		{
			var result = false;

			var masterBill = DataWrapper.TopLevelBusinessObject as CusCAeMHMaster;
			if (masterBill != null)
			{
				result = masterBill.HouseBills.Any(houseBill => houseBill.BW_HouseCCN == houseCCN && houseBill.IsCustomsStatusActive);
			}

			return result;
		}

		protected override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			var result = base.GetCommonNotificationsForSending();

			if (DataWrapper.RelatedCCNs.Any(x => IsHouseWeightIsZero(x)))
			{
				result.AddError(CusCAeMHHouseValidation.WeightIsMandatory);
			}

			return result;
		}

		bool IsHouseWeightIsZero(string houseCCN)
		{
			var result = false;

			var masterBill = DataWrapper.TopLevelBusinessObject as CusCAeMHMaster;
			if (masterBill != null)
			{
				result = masterBill.HouseBills.Any(houseBill => houseBill.BW_HouseCCN == houseCCN && houseBill.BW_Weight.IsEmpty);
			}

			return result;
		}

		public ActionPurpose ActionPurpose { get; set; }

		bool IsForced { get; set; }

		protected override bool AllowSendingMessageWhenAwaitingReply => IsForced;

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			var master = bizo as CusCAeMHMaster;
			if (master != null)
			{
				foreach (var message in master.Messages.Find(message => message.EM_MessageType == MessageTypeList.Codes.ACIForwarderClose && message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit))
				{
					var closeMessage = message as ACIForwarderCloseMessage;
					if (closeMessage != null && closeMessage.NeedAutoCloseReport)
					{
						closeMessage.NeedAutoCloseReport = false;
					}
				}
			}

			return base.GenerateOriginalMessagesCore(bizo);
		}

		#endregion
	}
}
