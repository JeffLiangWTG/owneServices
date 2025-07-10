using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5SISender : CusEntryHeaderOriginalMessageSender<Import5SIHeader>
	{
		public GOVCBR5SISender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5SI;

		protected override IMessageBuilder GetMessageBuilder(Import5SIHeader messageDataProvider)
		{
			return new GOVCBR5SIMessageBuilder(messageDataProvider);
		}

		protected override Import5SIHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID)
		{
			return new Import5SIHeaderCreator().Create(parent);
		}

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			var cusEntryNum = parent.EntryNumbers.GetOrCreateCusEntryNum(MessageType);
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
	}
}
