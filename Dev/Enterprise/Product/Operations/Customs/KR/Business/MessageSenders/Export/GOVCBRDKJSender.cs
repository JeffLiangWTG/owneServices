using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBRDKJSender : MessageSender<CusEntryHeader, ExportCancellationHeader>
	{
		public GOVCBRDKJSender(IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header), factory)
		{
			this.sendingObjects = sendingObjects;
		}

		readonly IEnumerable<JobDeclarationMiscMessageSendingObject> sendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._DKJ;

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = base.CreateMessages(parent);
			var message = messages.First();
			message.EM_MessageSubType = _5ASAmendmentType.Codes.Cancellation;
			message.RecordVersionNumberFromCH_VersionID(parent);
			return messages;
		}

		protected override ExportCancellationHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID)
		{
			return new ExportCancellationHeaderCreator().Create(parent);
		}
		protected override IMessageBuilder GetMessageBuilder(ExportCancellationHeader messageDataProvider)
		{
			return new GOVCBRDKJMessageBuilder(messageDataProvider, GetSendingObject(messageDataProvider.ExportDeclarationNumber));
		}
		JobDeclarationMiscMessageSendingObject GetSendingObject(ZString entryNumber) => sendingObjects.First(x => x.EntryNumber == entryNumber);
		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
		}
	}
}
