using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR929Sender : CusEntryHeaderOriginalMessageSender<ImportEntryHeader>
	{
		public GOVCBR929Sender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}
		protected override IMessageBuilder GetMessageBuilder(ImportEntryHeader messageDataProvider) => new GOVCBR929MessageBuilder(messageDataProvider);

		protected override ImportEntryHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new ImportEntryHeaderCreator().Create(parent);

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._929;

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			parent.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			parent.SetChargesAndLineFeesVersion();
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
