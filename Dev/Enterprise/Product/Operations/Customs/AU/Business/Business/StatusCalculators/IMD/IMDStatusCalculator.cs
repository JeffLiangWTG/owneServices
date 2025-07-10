using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class IMDStatusCalculator : CMRStatusCalculatorBase<CusEntryHeader>
	{
		public IMDStatusCalculator(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected internal override ZString GetAcceptedResponseStatus(EDIMessage outgoingMessage)
		{
			ZString result = "";
			if (IsSACMessage(outgoingMessage))
			{
				if (!IsWithdrawal(outgoingMessage))
				{
					result = CustomsEntryStatus.ClearSAC.Code;
				}
				else
				{
					result = CustomsEntryStatus.ClearWithdrawal.Code;
				}
			}
			else if (IsPreLodgeMessage(outgoingMessage))
			{
				result = CustomsEntryStatus.ClearPreLodge.Code;
			}
			else if (IsAmendment(outgoingMessage))
			{
				result = CustomsEntryStatus.ClearAmendment.Code;
			}
			else if (IsWithdrawal(outgoingMessage))
			{
				result = CustomsEntryStatus.ClearWithdrawal.Code;
			}
			else if (IsFormalLodge(outgoingMessage))
			{
				result = CustomsEntryStatus.ClearFormalLodge.Code;
			}
			else if (IsPaymentMessage(outgoingMessage))
			{
				result = CustomsEntryStatus.ClearPayment.Code;
			}
			return result;
		}

		protected internal override ZString GetAwaitingResponseStatus(EDIMessage outgoingMessage)
		{
			ZString result = "";
			if (IsSACMessage(outgoingMessage))
			{
				if (!IsWithdrawal(outgoingMessage))
				{
					result = CustomsEntryStatus.AwaitingSAC.Code;
				}
				else
				{
					result = CustomsEntryStatus.AwaitingWithdrawal.Code;
				}
			}
			else if (IsPreLodgeMessage(outgoingMessage))
			{
				result = CustomsEntryStatus.AwaitingPreLodge.Code;
			}
			else if (IsAmendment(outgoingMessage))
			{
				result = CustomsEntryStatus.AwaitingAmendment.Code;
			}
			else if (IsWithdrawal(outgoingMessage))
			{
				result = CustomsEntryStatus.AwaitingWithdrawal.Code;
			}
			else if (IsFormalLodge(outgoingMessage))
			{
				result = CustomsEntryStatus.AwaitingFormalLodge.Code;
			}
			else if (IsPaymentMessage(outgoingMessage))
			{
				result = CustomsEntryStatus.AwaitingPayment.Code;
			}
			return result;
		}

		protected internal override ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage)
		{
			ZString result = "";
			if (IsSACMessage(outgoingMessage))
			{
				if (!IsWithdrawal(outgoingMessage))
				{
					result = CustomsEntryStatus.FailSAC.Code;
				}
				else
				{
					result = CustomsEntryStatus.FailWithdrawal.Code;
				}
			}
			else if (IsPreLodgeMessage(outgoingMessage))
			{
				result = CustomsEntryStatus.FailPreLodge.Code;
			}
			else if (IsAmendment(outgoingMessage))
			{
				result = CustomsEntryStatus.FailAmendment.Code;
			}
			else if (IsWithdrawal(outgoingMessage))
			{
				result = CustomsEntryStatus.FailWithdrawal.Code;
			}
			else if (IsFormalLodge(outgoingMessage))
			{
				result = CustomsEntryStatus.FailFormalLodge.Code;
			}
			else if (IsPaymentMessage(outgoingMessage))
			{
				result = CustomsEntryStatus.FailPayment.Code;
			}
			return result;
		}

		internal bool IsPreLodgeMessage(EDIMessage outgoingMessage)
		{
			if (outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.IMD)
			{
				CMRIMDMessage message = outgoingMessage as CMRIMDMessage;
				return message != null && message.IsPreLodgeMessage && message.IsOriginalMessage;
			}
			return false;
		}

		internal bool IsAmendment(EDIMessage outgoingMessage)
		{
			if (outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.IMD)
			{
				CMRIMDMessage message = outgoingMessage as CMRIMDMessage;
				return message != null && message.IsAmendmentMessage;
			}
			return false;
		}

		internal bool IsWithdrawal(EDIMessage outgoingMessage)
		{
			if ((outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.IMD) ||
				(outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.SAC))
			{
				CMRCUSDECMessage message = outgoingMessage as CMRCUSDECMessage;
				return message != null && message.IsWithdrawalMessage;
			}
			return false;
		}

		internal bool IsFormalLodge(EDIMessage outgoingMessage)
		{
			if (outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.IMD)
			{
				CMRIMDMessage message = outgoingMessage as CMRIMDMessage;
				return message != null && !message.IsPreLodgeMessage && message.IsOriginalMessage;
			}
			return false;
		}

		internal bool IsPaymentMessage(EDIMessage outgoingMessage) => outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.PAYSTD;

		internal bool IsSACMessage(EDIMessage outgoingMessage) => outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.SAC;

		protected internal override ZString[] InterestedMessageTypes
		{
			get
			{
				return new ZString[]
					{
						CMRMessage.CMRMessageTypes.ATD,
						CMRMessage.CMRMessageTypes.PAYINV,
						CMRMessage.CMRMessageTypes.PAYOUT,
						CMRMessage.CMRMessageTypes.IMD,
						CMRMessage.CMRMessageTypes.PAYSTD,
						CMRMessage.CMRMessageTypes.SAC,
						CMRMessage.CMRMessageTypes.PAYREC,
						CMRMessage.CMRMessageTypes.PAYEXC,
					};
			}
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.CH_StatusInfo;

		protected override CodeDescriptionPairList StatusList
		{
			get
			{
				if (fStatusList == null)
				{
					fStatusList = new LegacyCustomsEntryStatusList();
				}
				return fStatusList;
			}
		}
		CodeDescriptionPairList fStatusList;

		protected override void DeriveStatusCore()
		{
			base.DeriveStatusCore();

			#pragma warning disable IDE0001 // Prevent simplification to base class
			using (new MergeManager.ChangingMergedDeclarationInAWayThatDoesNotRequireReMerge(Parent.Declaration))
			#pragma warning restore IDE0001 // Prevent simplification to base class
			{
				Parent.Declaration.JE_MessageStatus = Parent.Declaration.SummaryEntryStatusCalculator.SummaryMessageStatus;
			}
		}

		protected override bool AutoDeriveStatusOnFactorySaving => Parent.ConsolidatedDeclaration == null;

		protected internal override ZString GetStatusFromInboundMessage(EDIMessage message) => message is CMRCUSRESMessage cmrCUSRESMessage
			? GetCargoStatusCodeFromDescription(cmrCUSRESMessage.GetCustomsStatusFromMessage())
			: base.GetStatusFromInboundMessage(message);
	}
}
