using CargoWise.Customs.IE.MessageContracts.AIS.UCC5;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS.UCC5;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5MessageSender : MessageSender
	{
		public AISUCC5MessageSender(AISUCC5MessageSendingAction sendingAction) : base(sendingAction) { }

		protected new AISUCC5MessageSendingAction SendingAction => (AISUCC5MessageSendingAction)base.SendingAction;

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage outgoingMessage)
		{
			IXmlMessageBuilder result = null;
			var messageType = outgoingMessage.EM_MessageType;
			switch (messageType)
			{
				case AISOutgoingMessageTypeList.Codes.AmendmentRequest:
					result = new IM413MessageBuilder(new IM413HeaderProvider(SendingAction));
					break;
				case AISOutgoingMessageTypeList.Codes.CustomsDeclaration:
					result = new IM415MessageBuilder(new IM413AndIM415HeaderProvider(SendingAction));
					break;
				case AISOutgoingMessageTypeList.Codes.InvalidationRequest:
					result = new IM414MessageBuilder(new IM414HeaderProvider(SendingAction));
					break;
				case AISOutgoingMessageTypeList.Codes.PresentationNotification:
					result = new IM432MessageBuilder(new IM432HeaderProvider(SendingAction));
					break;
			}
			return result;
		}

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<AISUCC5OutboundEDIMessage>();
	}
}
