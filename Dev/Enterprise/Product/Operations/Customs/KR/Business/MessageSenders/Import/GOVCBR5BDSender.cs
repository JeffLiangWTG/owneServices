using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5BDSender : CusEntryHeaderOriginalMessageSender<Import5BD>
	{
		public GOVCBR5BDSender(IEnumerable<EarlyReleaseMiscMessageSendingObject> sendingObjects, BusinessObjectFactory factory)
			: base(sendingObjects.Select(x => x.Header), factory)
		{
			this.sendingObjects = sendingObjects;
		}
		readonly IEnumerable<EarlyReleaseMiscMessageSendingObject> sendingObjects;

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5BD;
		protected override IMessageBuilder GetMessageBuilder(Import5BD dataProvider) => new GOVCBR5BDMessageBuilder(dataProvider);
		protected override Import5BD GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new Import5BDCreator().Create(parent, GetSendingObject(parent.EntryNumber));
		EarlyReleaseMiscMessageSendingObject GetSendingObject(ZString entryNumber) => sendingObjects.First(x => x.EntryNumber == entryNumber);

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(MessageType);
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
	}
}
