using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5SMSender : CusEntryHeaderOriginalMessageSender<Import5SMHeader>
	{
		public GOVCBR5SMSender(IEnumerable<CusEntryHeader> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5SM;

		protected override IMessageBuilder GetMessageBuilder(Import5SMHeader messageDataProvider) => new GOVCBR5SMMessageBuilder(messageDataProvider);

		protected override Import5SMHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new Import5SMHeaderCreator().Create(parent);

		protected override void UpdateMessageStatus(CusEntryHeader parent)
		{
			parent.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
		protected override void OnSentCore(CusEntryHeader parent)
		{
			base.OnSentCore(parent);
			parent.PopulateEntrySubmittedDateIfRequired();
		}
	}
}
