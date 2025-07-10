using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5BASender : CusEntryHeaderOriginalMessageSender<Import5BAHeader>
	{
		public GOVCBR5BASender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5BA;
		protected override IMessageBuilder GetMessageBuilder(Import5BAHeader dataProvider) => new GOVCBR5BAMessageBuilder(dataProvider);
		protected override Import5BAHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new Import5BAHeaderCreator().Create(parent);

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = base.CreateMessages(parent);
			var message = messages.First();
			message.RecordVersionNumberFromCusEntryNum(parent, MessageType);
			return messages;
		}

		protected override void UpdateMessageStatus(CusEntryHeader entry)
		{
			var cusEntryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(MessageType);
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
	}
}

