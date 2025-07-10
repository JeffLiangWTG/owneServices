using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR008Sender : CusEntryHeaderOriginalMessageSender<Import008Header>
	{
		public GOVCBR008Sender(IEnumerable<CusEntryHeader> parents, BusinessObjectFactory factory) : base(parents, factory)
		{
		}
		public override ZString MessageType => ElectronicDocumentTypeList.Codes._008;
		protected override IMessageBuilder GetMessageBuilder(Import008Header messageDataProvider) => new GOVCBR008MessageBuilder(messageDataProvider);
		protected override Import008Header GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => new Import008Creator().Create(parent);
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
