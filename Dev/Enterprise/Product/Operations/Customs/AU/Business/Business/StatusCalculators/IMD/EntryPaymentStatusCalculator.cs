using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EntryPaymentStatusCalculator : CMRStatusCalculatorBase<CusEntryHeader>
	{
		public EntryPaymentStatusCalculator(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		readonly CusEntryHeader entryHeader;

		protected override bool AutoDeriveStatusOnFactorySaving => false;

		protected internal override ZString GetStatusFromInboundMessage(EDIMessage incomingMessage)
		{
			var result = ZString.Empty;

			switch (incomingMessage.EM_MessageType)
			{
				case CMRMessage.CMRMessageTypes.IMD:
				case CMRMessage.CMRMessageTypes.SAC:
					if (incomingMessage is IOutstandingPaymentInfoProvider paymentProvider
						&& StatusNeedsRecalculationProvider.Messages.LastOutgoingMessage is IPaymentIncluded lastIMDOrSACMessage)
					{
						var retriever = new OutstandingAmountRetriever(paymentProvider);

						var outstandingAmount = retriever.OutstandingAmount;
						if (outstandingAmount > 0m)
						{
							if (!lastIMDOrSACMessage.IsPaymentIncluded)
							{
								result = CMREntryPaymentStatusList.Codes.PayPending;
							}
							else if (!entryHeader.IsCustomsChargePaid)
							{
								result = CMREntryPaymentStatusList.Codes.PayAckPending;
							}
						}
						else if (outstandingAmount == 0m
							&& incomingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.IMD
							&& entryHeader.IsDutyDeferred)
						{
							if (retriever.HasPaymentPendingProcessingIndicator)
							{
								result = CMREntryPaymentStatusList.Codes.PayPending;
							}
							else
							{
								result = CMREntryPaymentStatusList.Codes.Paid;
							}
						}
					}
					break;

				case CMRMessage.CMRMessageTypes.PAYREC:
					if (incomingMessage is CMRPAYRECMessage pAYREC
						&& pAYREC.PAYRECInfoProvider.TotalPayable > pAYREC.PAYRECInfoProvider.AQISServicePayment)
					{
						result = CMREntryPaymentStatusList.Codes.Paid;
					}
					break;

				case CMRMessage.CMRMessageTypes.PAYINV:
				case CMRMessage.CMRMessageTypes.PAYOUT:
				case CMRMessage.CMRMessageTypes.PAYEXC:
					result = CMREntryPaymentStatusList.Codes.PayRejected;
					break;

				case CMRMessage.CMRMessageTypes.REFACC:
					result = CMREntryPaymentStatusList.Codes.Refunded;
					break;

				case CMRMessage.CMRMessageTypes.REFREJ:
					result = CMREntryPaymentStatusList.Codes.RefundRejected;
					break;
			}

			return result;
		}

		protected internal override ZString GetAcceptedResponseStatus(EDIMessage outgoingMessage) => "";

		protected internal override ZString GetAwaitingResponseStatus(EDIMessage outgoingMessage)
		{
			ZString result = "";
			if (outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.PAYSTD)
			{
				CMRPAYSTDMessage pAYSTD = outgoingMessage as CMRPAYSTDMessage;
				if (pAYSTD != null && pAYSTD.HasCustomsPayment)
				{
					result = CMREntryPaymentStatusList.Codes.PayAckPending;
				}
			}
			else if (outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.IMD ||
				outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.SAC)
			{
				CusEntryHeader entryHeader = outgoingMessage.EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null && !entryHeader.IsSubjectToDutyAndTax)
				{
					result = CMREntryPaymentStatusList.Codes.NoAmountDue;
				}
			}
			return result;
		}

		protected internal override ZString GetRejectedResponseStatus(EDIMessage outgoingMessage, EDIMessage incomingMessage)
		{
			ZString result = "";
			if (incomingMessage != null)
			{
				if (incomingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.REFREJ)
				{
					result = CMREntryPaymentStatusList.Codes.RefundRejected;
				}
				else if (incomingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.PAYINV
					|| incomingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.PAYOUT
					|| incomingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.PAYEXC)
				{
					result = CMREntryPaymentStatusList.Codes.PayRejected;
				}
			}
			return result;
		}

		protected internal override ZString[] InterestedMessageTypes
		{
			get
			{
				return new ZString[]
					{
						CMRMessage.CMRMessageTypes.PAYREC,
						CMRMessage.CMRMessageTypes.PAYINV,
						CMRMessage.CMRMessageTypes.PAYOUT,
						CMRMessage.CMRMessageTypes.IMD,
						CMRMessage.CMRMessageTypes.SAC,
						CMRMessage.CMRMessageTypes.REFACC,
						CMRMessage.CMRMessageTypes.REFREJ,
						CMRMessage.CMRMessageTypes.PAYSTD,
						CMRMessage.CMRMessageTypes.PAYEXC
					};
			}
		}

		protected internal override ZPropertyInfo StatusInfo => Parent.AddInfo.ZA_PaymentStatus_HiddenInfo;

		protected override CodeDescriptionPairList StatusList
		{
			get
			{
				if (fStatusList == null)
				{
					fStatusList = new CMREntryPaymentStatusList();
				}
				return fStatusList;
			}
		}
		CodeDescriptionPairList fStatusList;
	}
}
