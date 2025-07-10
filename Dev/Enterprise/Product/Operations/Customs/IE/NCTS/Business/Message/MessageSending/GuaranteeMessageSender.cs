using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeMessageSender : MessageSender
	{
		public GuaranteeMessageSender(IMessageSendingAction sendingAction) : base(sendingAction) { }

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage relatingMessage)
		{
			IXmlMessageBuilder result = null;
			var messageType = relatingMessage.EM_MessageType;
			switch (messageType)
			{
				case NCTSOutgoingMessageTypeList.Codes.GuaranteeVoucherSold:
					var guaranteeVoucherSoldSendingAction = (GuaranteeVoucherSoldSendingAction)SendingAction;
					result = new CargoWise.Customs.IE.MessageContracts.NCTS.IE224MessageBuilder(new IE224MessageProvider(guaranteeVoucherSoldSendingAction));
					break;
				default:
					break;
			}
			return result;
		}

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<NCTSOutboundEDIMessage>();
	}
}
