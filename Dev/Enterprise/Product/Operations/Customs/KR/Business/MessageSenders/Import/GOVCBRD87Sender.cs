using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBRD87Sender : CusEntryHeaderOriginalMessageSender<ImportD87Header>
	{
		public GOVCBRD87Sender(IEnumerable<CusEntryHeader> parents, BusinessObjectFactory factory) : base(parents, factory)
		{
		}

		public override ZString MessageType => ElectronicDocumentTypeList.Codes._D87;

		protected override IMessageBuilder GetMessageBuilder(ImportD87Header messageDataProvider) => new GOVCBRD87MessageBuilder(messageDataProvider);

		protected override ImportD87Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new ImportD87HeaderCreator().Create(parent);

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
