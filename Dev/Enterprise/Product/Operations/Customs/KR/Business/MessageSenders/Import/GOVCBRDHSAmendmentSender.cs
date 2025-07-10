using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBRDHSAmendmentSender : CusEntryHeaderAmendmentMessageSender<ImportFTAAmendmentHeader, ImportDHRHeader, IImportFTAHeader>
	{
		public GOVCBRDHSAmendmentSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory, ZString messageType)
			: base(messageSendingObjects, factory)
		{
			this.messageType = messageType;
		}

		public override ZString MessageType => messageType;

		readonly ZString messageType;

		protected override ImportDHRHeader GetCurrentDataProvider(CusEntryHeader parent) => new ImportDHRCreator().Create(parent);

		protected override IMessageBuilder GetMessageBuilder(ImportFTAAmendmentHeader messageDataProvider, IAmendmentDetails amendmentDetails) => new GOVCBRDHSMessageBuilder(messageDataProvider, (IDHSAmendmentDetails)amendmentDetails);

		protected override ImportFTAAmendmentHeader GetMessageDataProvider(CusEntryHeader parent, ImportDHRHeader currentSnapshot, AmendedItemCollection amendedItems) => new ImportFTAAmendmentHeaderCreator().Create(parent, amendedItems.Cast<AmendedItem>().ToArray());

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.GetOriginalType(MessageType));
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentSent;
		}
	}
}
