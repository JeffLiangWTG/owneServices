using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5ULSender : CusEntryHeaderOriginalMessageSender<Import5ULHeader>
	{
		public GOVCBR5ULSender(IEnumerable<PenaltyRefundRequestMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header).Distinct(), factory)
		{
			PopulateRefundDetails(sendingObjects);
		}

		public GOVCBR5ULSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects.Select(x => x.Header), factory)
		{
			PopulateGOVCBR5ULDetailsPerEntry(messageSendingObjects);
		}

		void PopulateRefundDetails(IEnumerable<PenaltyRefundRequestMessageSendingObject> sendingObjects)
		{
			foreach (PenaltyRefundRequestMessageSendingObject sendingObject in sendingObjects)
			{
				if (sendingObject.ShouldSend)
				{
					RefundDetails.Add(sendingObject.CustomsDisbursementBillNumber, new GOVCBR5ULDetails(sendingObject));
				}
			}
		}

		void PopulateGOVCBR5ULDetailsPerEntry(IEnumerable<JobDeclarationAmendmentMessageSendingObject> amendmentMessageSendingObjects)
		{
			foreach (JobDeclarationAmendmentMessageSendingObject amendmentMessageSendingObject in amendmentMessageSendingObjects)
			{
				if (amendmentMessageSendingObject.ShouldSend)
				{
					RefundDetails.Add(amendmentMessageSendingObject.CustomsDisbursementBill, new GOVCBR5ULDetails(amendmentMessageSendingObject));
				}
			}
		}

		Dictionary<string, GOVCBR5ULDetails> RefundDetails => refundDetails ??= new Dictionary<string, GOVCBR5ULDetails>();
		Dictionary<string, GOVCBR5ULDetails> refundDetails;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5UL;
		protected override IMessageBuilder GetMessageBuilder(Import5ULHeader dataProvider) => new GOVCBR5ULMessageBuilder(dataProvider);
		protected override ZString GetMessageID(EDIMessage message)
		{
			return message.EM_ApplicationReference;
		}
		protected override Import5ULHeader GetMessageDataProvider(CusEntryHeader parent, ZString individualBillNumber)
		{
			return new Import5ULCreator().Create(parent, GetRefundDetails(individualBillNumber));
		}

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			foreach (GOVCBR5ULDetails refundDetails in RefundDetails.Values)
			{
				if (refundDetails.Entry == entry)
				{
					refundDetails.RefundEntryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
					refundDetails.RefundEntryNumber.CE_EntryLineReference = refundDetails.DisbursementBillNumber;
				}
			}
		}

		GOVCBR5ULDetails GetRefundDetails(ZString individualBillNumber)
		{
			RefundDetails.TryGetValue(individualBillNumber, out GOVCBR5ULDetails result);
			return result;
		}

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = new List<EDIMessage>();

			foreach (GOVCBR5ULDetails refundDetails in RefundDetails.Values)
			{
				if (refundDetails.Entry == parent)
				{
					var message = Factory.New<EDIMessage>();
					message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					message.EM_MessageType = MessageType;
					message.EM_ApplicationReference = refundDetails.DisbursementBillNumber;
					message.CustomsDisbursementBill = refundDetails.DisbursementBillNumber;
					message.EM_MessageSubType = refundDetails.MessageSubType;
					messages.Add(message);
				}
			}
			return messages;
		}

		protected override void OnSentCore(CusEntryHeader parent)
		{
			base.OnSentCore(parent);

			foreach (GOVCBR5ULDetails refundDetails in RefundDetails.Values)
			{
				if (refundDetails.Entry == parent && !refundDetails.DisbursementBillNumber.IsNullOrEmpty())
				{
					var refundSessionalData = parent.GetOrCreateRefundSessionalDataOriginalSendable(refundDetails.DisbursementBillNumber);
					if (refundSessionalData != null)
					{
						refundSessionalData.CSI_Value = refundDetails.RefundAmounts.Sum(x => x.Value);
						refundSessionalData.RefundType = refundDetails.RefundType;
						refundSessionalData.RefundCauseCode = refundDetails.RefundCause;
						refundSessionalData.RefundReasonCode = refundDetails.RefundReason;

						if (refundDetails.AmendmentSessionalData != null)
						{
							refundSessionalData.CSI_CSI_SupportingInfo = refundDetails.AmendmentSessionalData.PK;
						}
					}
				}
			}
		}
	}
}
