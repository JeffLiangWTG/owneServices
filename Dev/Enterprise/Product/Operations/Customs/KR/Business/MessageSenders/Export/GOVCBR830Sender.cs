using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR830Sender : CusEntryHeaderOriginalMessageSender<ExportEntryHeader>
	{
		public GOVCBR830Sender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}
		protected override IMessageBuilder GetMessageBuilder(ExportEntryHeader messageDataProvider) => new GOVCBR830MessageBuilder(messageDataProvider);

		protected override ExportEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new ExportEntryHeaderCreator().Create(parent);

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._830;

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			parent.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}

		protected override IEnumerable<EDIMessage> CreateMessages(CusEntryHeader parent)
		{
			var messages = base.CreateMessages(parent);
			var message = messages.First();
			message.RecordVersionNumberFromCH_VersionID(parent);
			return messages;
		}

		protected override void OnSentCore(CusEntryHeader parent)
		{
			base.OnSentCore(parent);
			parent.PopulateEntrySubmittedDateIfRequired();
		}
	}
}
