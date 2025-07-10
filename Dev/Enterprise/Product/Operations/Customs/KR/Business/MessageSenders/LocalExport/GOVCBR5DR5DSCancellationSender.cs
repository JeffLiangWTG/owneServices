using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5DR5DSCancellationSender : MessageSender<CusEntryHeader, LocalExportAmendEntryHeader>
	{
		public GOVCBR5DR5DSCancellationSender(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, string messageType, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header), factory)
		{
			this.sendingObjects = sendingObjects;
			MessageType = messageType;
		}

		readonly IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects;

		public override ZString MessageType { get; }

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = base.CreateMessages(parent);
			var message = messages.First();
			message.EM_MessageSubType = LocalExportAmendmentTypeList.Codes.Cancellation;
			message.RecordVersionNumberFromCH_VersionID(parent);
			return messages;
		}

		protected override LocalExportAmendEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID)
		{
			return new LocalExportAmendmentHeaderCreator().Create(parent, Array.Empty<AmendedItem>());
		}
		protected override IMessageBuilder GetMessageBuilder(LocalExportAmendEntryHeader messageDataProvider)
		{
			var sendingObject = GetSendingObject(messageDataProvider.CustomsReceiptNumber);
			return MessageType == ElectronicDocumentTypeList.Codes._5DR ? new GOVCBR5DRMessageBuilder(messageDataProvider, sendingObject, MessageFunctions.MessageFunctionCode.Cancellation)
				: new GOVCBR5DSMessageBuilder(messageDataProvider, sendingObject, MessageFunctions.MessageFunctionCode.Cancellation);
		}
		JobDeclarationMiscMessageSendingObject GetSendingObject(ZString receiptNumber) => sendingObjects.First(x => x.CustomsReceiptNumber == MessageFunctions.GetCustomsReceiptNumber(receiptNumber));
		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
		}
	}
}
