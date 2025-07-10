using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5ULStandAloneSender : MessageSender<CusReconDeclaration, Import5ULHeader>
	{
		public GOVCBR5ULStandAloneSender(IEnumerable<CusReconDeclaration> parents, BusinessObjectFactory factory) : base(parents, factory)
		{
		}
		public override ZString MessageType => ElectronicDocumentTypeList.Codes._5UL;
		protected override ZString GetMessageID(EDIMessage message)
		{
			return message.EM_ApplicationReference;
		}
		protected override IMessageBuilder GetMessageBuilder(Import5ULHeader messageDataProvider) => new GOVCBR5ULMessageBuilder(messageDataProvider);
		protected override void UpdateMessageStatus(CusReconDeclaration parent)
		{
			parent.CRD_MessageStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
		}
		protected override Import5ULHeader GetMessageDataProvider(CusReconDeclaration parent, ZString messageID)
		{
			return new Import5ULCreator().Create(parent);
		}
		protected override IEnumerable<EDIMessage> CreateMessages(CusReconDeclaration parent)
		{
			var messages = new List<EDIMessage>();
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageType;
			message.EM_MessageSubType = Constants.RefundRequestType.StandAlone;
			message.EM_ApplicationReference =  parent.RefundDeclarationNumber;
			messages.Add(message);
			return messages;
		}
	}
}
