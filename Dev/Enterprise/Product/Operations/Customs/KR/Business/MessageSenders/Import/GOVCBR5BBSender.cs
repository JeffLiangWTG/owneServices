using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5BBSender : CusEntryHeaderAmendmentMessageSender<Import5BBHeader, Import5BAHeader, IImport5BAHeader>
	{
		public GOVCBR5BBSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5BB;

		protected override IMessageBuilder GetMessageBuilder(Import5BBHeader messageDataProvider, IAmendmentDetails amendmentDetails) => new GOVCBR5BBMessageBuilder(messageDataProvider, amendmentDetails);
		protected override Import5BAHeader GetCurrentDataProvider(CusEntryHeader parent) => new Import5BAHeaderCreator().Create(parent);
		protected override Import5BBHeader GetMessageDataProvider(CusEntryHeader parent, Import5BAHeader currentSnapshot, AmendedItemCollection amendedItems) => new Import5BBHeaderCreator().Create(parent, currentSnapshot, amendedItems);

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			entry.EntryNumbers.UpdateCusEntryNumIfExists(OriginalMessageType, CusEntryNumber.Schema.CE_EntryStatus, (ZString)CustomsMessageStatusTypeList.Codes.AmendmentSent);
		}
	}
}
