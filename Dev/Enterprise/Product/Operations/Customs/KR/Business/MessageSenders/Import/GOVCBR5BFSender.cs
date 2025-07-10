using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5BFSender : MessageSender<CusEntryHeader, Import5BFCancel>
	{
		public GOVCBR5BFSender(IEnumerable<CancellationMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header), factory)
		{
			this.sendingObjects = sendingObjects;
		}
		readonly IEnumerable<CancellationMessageSendingObject> sendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5BF;
		protected override Import5BFCancel GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new Import5BFCancelCreator().Create(parent, GetSendingObject(parent.EntryNumber));
		CancellationMessageSendingObject GetSendingObject(ZString entryNumber) => sendingObjects.First(x => x.EntryNumber == entryNumber);
		protected override IMessageBuilder GetMessageBuilder(Import5BFCancel messageDataProvider) => new GOVCBR5BFMessageBuilder(messageDataProvider);
		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
		}
	}
}
