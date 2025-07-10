using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5ASAmendmentSender : CusEntryHeaderAmendmentMessageSender<ExportAmendmentHeader, ExportEntryHeader, IExportEntryHeader>
	{
		public GOVCBR5ASAmendmentSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5AS;

		protected override IMessageBuilder GetMessageBuilder(ExportAmendmentHeader messageDataProvider, IAmendmentDetails amendmentDetails) => new GOVCBR5ASMessageBuilder(messageDataProvider, amendmentDetails, MessageFunctions.MessageFunctionCode.Amendment);
		protected override ExportEntryHeader GetCurrentDataProvider(CusEntryHeader parent) => new ExportEntryHeaderCreator().Create(parent);
		protected override ExportAmendmentHeader GetMessageDataProvider(CusEntryHeader parent, ExportEntryHeader currentSnapshot, AmendedItemCollection amendedItems) => new Export5ASHeaderCreator().Create(parent, amendedItems.Cast<AmendedItem>().ToArray());

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
		}
	}
}
