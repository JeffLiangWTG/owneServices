using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5ULDetails
	{
		public GOVCBR5ULDetails(PenaltyRefundRequestMessageSendingObject sendingObject5UL)
		{
			DisbursementBillNumber = sendingObject5UL.CustomsDisbursementBillNumber;
			Entry = sendingObject5UL.Header;
			RefundRequestNumber = sendingObject5UL.RefundEntryNumber.CE_EntryNum.IsEmpty ? EDIMessage.RefundEntryNumberPlaceHolder : sendingObject5UL.RefundEntryNumber.CE_EntryNum;
			RefundEntryNumber = sendingObject5UL.RefundEntryNumber;
			var penaltiesToRefund = new Dictionary<ZString, ZDecimal>
			{
				{ EntryTaxTypeList.Codes.CUD, sendingObject5UL.DutyPenaltyToRefund },
				{ EntryTaxTypeList.Codes._5AB, sendingObject5UL.EDTPenaltyToRefund },
				{ EntryTaxTypeList.Codes.CAP, sendingObject5UL.AGTPenaltyToRefund },
				{ EntryTaxTypeList.Codes.VAT, sendingObject5UL.VATPenaltyToRefund },
				{ EntryTaxTypeList.Codes.ACT, sendingObject5UL.LQTPenaltyToRefund },
				{ EntryTaxTypeList.Codes.IND, sendingObject5UL.SCTPenaltyToRefund },
				{ EntryTaxTypeList.Codes.ENV, sendingObject5UL.TRTPenaltyToRefund },
			};
			PenaltiesToRefund = penaltiesToRefund;
			var refundAmounts = new Dictionary<ZString, ZDecimal>
			{
				{ EntryTaxTypeList.Codes.CUD, sendingObject5UL.DutyRefundAmount },
				{ EntryTaxTypeList.Codes._5AB, sendingObject5UL.EDTRefundAmount },
				{ EntryTaxTypeList.Codes.CAP, sendingObject5UL.AGTRefundAmount },
				{ EntryTaxTypeList.Codes.VAT, sendingObject5UL.VATRefundAmount },
				{ EntryTaxTypeList.Codes.ACT, sendingObject5UL.LQTRefundAmount },
				{ EntryTaxTypeList.Codes.IND, sendingObject5UL.SCTRefundAmount },
				{ EntryTaxTypeList.Codes.ENV, sendingObject5UL.TRTRefundAmount },
				{ EntryTaxTypeList.Codes._5CQ, sendingObject5UL.ValueForVATRefundAmount },
				{ EntryTaxTypeList.Codes._5CR, sendingObject5UL.VATExemptionValueRefundAmount },
				{ EntryTaxTypeList.Codes._5AC, sendingObject5UL.PenaltyForLateDeclarationRefundAmount },
				{ EntryTaxTypeList.Codes._5AY, sendingObject5UL.PenaltyForMissedDeclarationRefundAmount },
				{ EntryTaxTypeList.Codes._5CT, sendingObject5UL.LatePaymentRefundAmount },
				{ EntryTaxTypeList.Codes._5CS, sendingObject5UL.NonDutyTaxRefundAmount }
			};
			RefundAmounts = refundAmounts;
			RefundType = sendingObject5UL.RefundType;
			RefundCause = sendingObject5UL.RefundCause;
			RefundReason = sendingObject5UL.RefundReason;
			Amendment5WNVersionNumber = sendingObject5UL.Amendment5WNNumber;
			AmendmentSessionalData = sendingObject5UL.AmendmentSessionalData;

			var are5FE_5ULToBeSentTogether = GetAre5FE_5ULToBeSentTogether(AmendmentSessionalData);
			MessageSubType = are5FE_5ULToBeSentTogether ? Constants.RefundRequestType.Simultaneous : Constants.RefundRequestType.StandAlone;
			Are5FE_5ULToBeSentTogether = are5FE_5ULToBeSentTogether;
		}

		public GOVCBR5ULDetails(JobDeclarationAmendmentMessageSendingObject amendmentMessageSendingObject)
		{
			DisbursementBillNumber = amendmentMessageSendingObject.CustomsDisbursementBill;
			Entry = amendmentMessageSendingObject.Header;

			var isOriginalMessageAllowed = (CusEntryNumber entryNumber) => CustomsMessageStatusTypeList.IsOriginalMessageAllowed(entryNumber.CE_EntryStatus);
			RefundEntryNumber = Entry.EntryNumbers.GetOrCreateCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UL, DisbursementBillNumber, isOriginalMessageAllowed);
			RefundRequestNumber = RefundEntryNumber.CE_EntryNum.IsEmpty ? EDIMessage.RefundEntryNumberPlaceHolder : RefundEntryNumber.CE_EntryNum;

			var penaltiesToRefund = new Dictionary<ZString, ZDecimal>();
			PenaltiesToRefund = penaltiesToRefund;

			var refundAmounts = new Dictionary<ZString, ZDecimal>
			{
				{ EntryTaxTypeList.Codes.CUD, amendmentMessageSendingObject.RefundAmountOfDutyAmount },
				{ EntryTaxTypeList.Codes._5AB, amendmentMessageSendingObject.RefundAmountOfEducationTax },
				{ EntryTaxTypeList.Codes.CAP, amendmentMessageSendingObject.RefundAmountOfAgricultureTax },
				{ EntryTaxTypeList.Codes.VAT, amendmentMessageSendingObject.RefundAmountOfVAT },
				{ EntryTaxTypeList.Codes.ACT, amendmentMessageSendingObject.RefundAmountOfLiquorTax },
				{ EntryTaxTypeList.Codes.IND, amendmentMessageSendingObject.RefundAmountOfSpecialConsumptionTax },
				{ EntryTaxTypeList.Codes.ENV, amendmentMessageSendingObject.RefundAmountOfTransportationTax },
				{ EntryTaxTypeList.Codes._5CQ, amendmentMessageSendingObject.RefundAmountOfValueForVAT },
				{ EntryTaxTypeList.Codes._5CR, amendmentMessageSendingObject.RefundAmountOfVATExemptionValue },
				{ EntryTaxTypeList.Codes._5AC, amendmentMessageSendingObject.RefundAmountOfPenaltyForLateDeclaration },
				{ EntryTaxTypeList.Codes._5AY, amendmentMessageSendingObject.RefundAmountOfPenaltyForMissedDeclaration },
				{ EntryTaxTypeList.Codes._5CT, amendmentMessageSendingObject.RefundAmountOfPenaltyForLatePayment },
				{ EntryTaxTypeList.Codes._5CS, amendmentMessageSendingObject.RefundAmountOfNonDutyTaxRevenue }
			};
			RefundAmounts = refundAmounts;
			RefundType = amendmentMessageSendingObject.RefundType;
			RefundCause = amendmentMessageSendingObject.RefundCause;
			RefundReason = amendmentMessageSendingObject.RefundReason;
			Amendment5WNVersionNumber = amendmentMessageSendingObject.AmendSeqNo5WN;
			AmendmentSessionalData = amendmentMessageSendingObject.AmendmentSessionalData;

			var are5FE_5ULToBeSentTogether = GetAre5FE_5ULToBeSentTogether(AmendmentSessionalData);
			MessageSubType = are5FE_5ULToBeSentTogether ? Constants.RefundRequestType.Simultaneous : Constants.RefundRequestType.StandAlone;
			Are5FE_5ULToBeSentTogether = are5FE_5ULToBeSentTogether;
		}

		static bool GetAre5FE_5ULToBeSentTogether(AmendmentSessionalData amendmentSessionalData)
		{
			return (amendmentSessionalData?.RefundSessionalData?.CSI_Code ?? ZString.Empty) == YesNoList.Codes.Yes;
		}

		public string RefundRequestNumber { get; private set; }
		public string DisbursementBillNumber { get; private set; }
		public bool Are5FE_5ULToBeSentTogether { get; private set; }
		public short Amendment5WNVersionNumber { get; private set; }
		public string MessageSubType { get; private set; }
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> PenaltiesToRefund { get; private set; }
		public IEnumerable<KeyValuePair<ZString, ZDecimal>> RefundAmounts { get; private set; }
		public CusEntryHeader Entry { get; private set; }
		public CusEntryNumber RefundEntryNumber { get; private set; }
		public string RefundType { get; private set; }
		public string RefundCause { get; private set; }
		public string RefundReason { get; private set; }
		public AmendmentSessionalData AmendmentSessionalData { get; private set; }
	}
}
