using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5DS5DRAmendmentSender : CusEntryHeaderAmendmentMessageSender<LocalExportAmendEntryHeader, LocalExportEntryHeader, ILocalExportEntryHeader>
	{
		public GOVCBR5DS5DRAmendmentSender(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, ZString messageType, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory)
		{
			this.localExportMessageType = messageType;
		}

		readonly ZString localExportMessageType;

		public override ZString MessageType => localExportMessageType;

		protected override LocalExportEntryHeader GetCurrentDataProvider(CusEntryHeader parent)
		{
			if (MessageType == ElectronicDocumentTypeList.Codes._5DR)
			{
				return new LocalExport5DPEntryHeaderCreator().Create(parent);
			}
			else
			{
				return new LocalExport5DQEntryHeaderCreator().Create(parent);
			}
		}

		protected override IMessageBuilder GetMessageBuilder(LocalExportAmendEntryHeader messageDataProvider, IAmendmentDetails amendmentDetails)
		{
			if (MessageType == ElectronicDocumentTypeList.Codes._5DR)
			{
				return new GOVCBR5DRMessageBuilder(messageDataProvider, amendmentDetails, MessageFunctions.MessageFunctionCode.Amendment);
			}
			else
			{
				return new GOVCBR5DSMessageBuilder(messageDataProvider, amendmentDetails, MessageFunctions.MessageFunctionCode.Amendment);
			}
		}

		protected override LocalExportAmendEntryHeader GetMessageDataProvider(CusEntryHeader parent, LocalExportEntryHeader currentSnapshot, AmendedItemCollection amendedItems) => new LocalExportAmendmentHeaderCreator().Create(parent, amendedItems.Cast<AmendedItem>().ToArray());

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			parent.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
		}
	}
}
