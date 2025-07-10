using CargoWise.Customs.IE.MessageContracts.AIS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class AISMessageSender : MessageSender
	{
		public AISMessageSender(AISMessageSendingAction sendingAction) : base(sendingAction) { }

		protected new AISMessageSendingAction SendingAction => (AISMessageSendingAction)base.SendingAction;

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage outgoingMessage)
		{
			IXmlMessageBuilder result = null;
			var messageType = outgoingMessage.EM_MessageType;
			switch (messageType)
			{
				case AISOutgoingMessageTypeList.Codes.CustomsDeclaration:
					result = new IM415MessageBuilder(new IM415HeaderProvider(SendingAction));
					break;
				case AISOutgoingMessageTypeList.Codes.AmendmentRequest:
					result = new IM413MessageBuilder(new IM413HeaderProvider(SendingAction));
					break;
				case AISOutgoingMessageTypeList.Codes.InvalidationRequest:
					result = new IM414MessageBuilder(new IM414HeaderProvider(SendingAction));
					break;
				case AISOutgoingMessageTypeList.Codes.PresentationNotification:
					result = new IM432MessageBuilder(new IM432HeaderProvider(SendingAction));
					break;
				case AISOutgoingMessageTypeList.Codes.EntryIntoTheDeclarantRecords:
					result = new IM433MessageBuilder(new IM433HeaderProvider(SendingAction));
					break;
			}
			return result;
		}

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<AISOutboundEDIMessage>();

		protected override void LogEvents()
		{
			base.LogEvents();
			if (SendingAction.EntryHeader.Declaration is JobDeclaration declaration)
			{
				if (SendingAction.MessageType == AISOutgoingMessageTypeList.Codes.AmendmentRequest)
				{
					declaration.LogCustomsCommenced(AISOutgoingMessageTypeList.Descriptions.AmendmentRequest);
				}
			}
		}
	}
}
