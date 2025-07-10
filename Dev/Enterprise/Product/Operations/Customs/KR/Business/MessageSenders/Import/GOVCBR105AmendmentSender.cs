using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR105AmendmentSender : CusEntryHeaderAmendmentMessageSender<ImportFTAAmendmentHeader, ImportFTAHeader, IImportFTAHeader>
	{
		public GOVCBR105AmendmentSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory, ZString messageType)
			: base(messageSendingObjects, factory)
		{
			this.messageType = messageType;
		}

		public override ZString MessageType => messageType;

		readonly ZString messageType;

		protected override ImportFTAHeader GetCurrentDataProvider(CusEntryHeader parent) => new ImportFTACreator().Create(parent);

		protected override IMessageBuilder GetMessageBuilder(ImportFTAAmendmentHeader messageDataProvider, IAmendmentDetails amendmentDetails) => new GOVCBR105MessageBuilder(messageDataProvider, amendmentDetails);

		protected override ImportFTAAmendmentHeader GetMessageDataProvider(CusEntryHeader parent, ImportFTAHeader currentSnapshot, AmendedItemCollection amendedItems) => new ImportFTAAmendmentHeaderCreator().Create(parent, amendedItems.Cast<AmendedItem>().ToArray());

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.GetOriginalType(MessageType));
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.AmendmentSent;
		}
	}
}
