using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5FEAmendmentSender : CusEntryHeaderAmendmentMessageSender<Import5FEHeader, ImportEntryHeader, IImportEntryHeader>
	{
		public GOVCBR5FEAmendmentSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory)
		{
			this.messageSendingObjects = messageSendingObjects;
		}
		readonly IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5FE;

		protected override ImportEntryHeader GetCurrentDataProvider(CusEntryHeader parent) => new ImportEntryHeaderCreator().Create(parent);

		protected override IMessageBuilder GetMessageBuilder(Import5FEHeader messageDataProvider, IAmendmentDetails amendmentDetails) => new GOVCBR5FEMessageBuilder(messageDataProvider, amendmentDetails);

		protected override Import5FEHeader GetMessageDataProvider(CusEntryHeader parent, ImportEntryHeader currentSnapshot, AmendedItemCollection amendedItems)
		{
			return new Import5FECreator().Create(parent, GetSendingObject(parent.EntryNumber));
		}
		JobDeclarationAmendmentMessageSendingObject GetSendingObject(ZString entryNumber) => messageSendingObjects.First(x => x.EntryNumber == entryNumber);

		protected override EDIMessage GenerateMessage(CusEntryHeader entry)
		{
			var result = base.GenerateMessage(entry);
			var messageSendingObject = GetSendingObject(entry.EntryNumber);
			if (messageSendingObject.PenaltyExemptionIndicator == Constants.YesNo.Yes)
			{
				var versionNumber5UA = messageSendingObject.PenaltyExemptionReqSequence.ToString();
				result.EM_MessageOwner = versionNumber5UA;
			}
			if (messageSendingObject.RefundRequestSubmissionYN == Constants.YesNo.Yes)
			{
				result.CustomsDisbursementBill = messageSendingObject.CustomsDisbursementBill;
				var entryNum = messageSendingObject.EntryNumber5ULInTransaction;
				if (entryNum == null)
				{
					entryNum = entry.EntryNumbers.AddNew();
					entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
				}
				entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
				entryNum.CE_EntryLineReference = messageSendingObject.CustomsDisbursementBill;
			}
			return result;
		}

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			parent.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
			parent.SetChargesAndLineFeesVersion();

			var messageSendingObject = GetSendingObject(parent.EntryNumber);
			if (messageSendingObject.PenaltyExemptionIndicator == Constants.YesNo.Yes)
			{
				var versionNumber5UA = messageSendingObject.PenaltyExemptionReqSequence.ToString();

				var entryNum5UA = parent.EntryNumbers.GetCusEntryNumWithMatchingVersionNumber(ElectronicDocumentTypeList.Codes._5UA, versionNumber5UA);
				if (entryNum5UA == null)
				{
					entryNum5UA = parent.EntryNumbers.AddNew();
					entryNum5UA.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
					entryNum5UA.CE_EntryLineReference = versionNumber5UA;
				}
				entryNum5UA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			}
		}

		public override int Send()
		{
			var result = base.Send();

			var sendingObjectsForIncludedRefundRequest = messageSendingObjects.Where(x => x.RefundRequestSubmissionYN == Constants.YesNo.Yes);
			if (sendingObjectsForIncludedRefundRequest.Any())
			{
				foreach (JobDeclarationAmendmentMessageSendingObject amendmentMessageSendingObject in sendingObjectsForIncludedRefundRequest)
				{
					var amendmentSessionalData = amendmentMessageSendingObject.AmendmentSessionalData;
					if (amendmentSessionalData != null)
					{
						var refundSessionData = amendmentSessionalData.RefundSessionalData;
						if (refundSessionData == null)
						{
							var instruction = amendmentSessionalData.Parent;
							refundSessionData = instruction.RefundSessionalDataCollection.AddNew();
							refundSessionData.CSI_CSI_SupportingInfo = amendmentSessionalData.PK;
							amendmentSessionalData.RefundSessionalDataCollection.Reload(false);
						}
						refundSessionData.RefundRequestYN = Constants.YesNo.Yes;
						refundSessionData.CustomsDisbursementBill = amendmentMessageSendingObject.CustomsDisbursementBill;
					}
				}
				result += new GOVCBR5ULSender(sendingObjectsForIncludedRefundRequest, Factory).Send();
			}
			return result;
		}
	}
}
