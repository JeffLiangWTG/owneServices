using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using RF415UCC5MessageBuilder = CargoWise.Customs.IE.MessageContracts.AIS.UCC5.RF415MessageBuilder;
using RF415UCC6MessageBuilder = CargoWise.Customs.IE.MessageContracts.AIS.RF415MessageBuilder;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationMessageSender : MessageSender
	{
		public RefundApplicationMessageSender(RefundApplicationMessageSendingAction sendingAction) : base(sendingAction)
		{
		}

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage outgoingMessage)
		{
			IXmlMessageBuilder result = null;
			var messageType = outgoingMessage.EM_MessageType;
			switch (messageType)
			{
				case AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15:
					result = SendingAction.EntryHeader.Declaration?.IsUCC5 ?? false ? new RF415UCC5MessageBuilder(new RF415HeaderProvider(SendingAction)) : new RF415UCC6MessageBuilder(new RF415HeaderProvider(SendingAction));
					break;
			}
			return result;
		}

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => SendingAction.EntryHeader.Declaration?.IsUCC5 ?? false ? factory.New<AISUCC5OutboundEDIMessage>() : factory.New<AISOutboundEDIMessage>();

		new RefundApplicationMessageSendingAction SendingAction => (RefundApplicationMessageSendingAction)base.SendingAction;
	}
}
