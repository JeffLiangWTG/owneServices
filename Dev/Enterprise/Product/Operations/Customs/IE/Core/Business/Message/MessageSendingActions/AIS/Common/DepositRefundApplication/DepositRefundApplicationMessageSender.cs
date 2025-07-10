using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.AIS;
using RD415UCC5MessageBuilder = CargoWise.Customs.IE.MessageContracts.AIS.UCC5.RD415MessageBuilder;
using RD415UCC6MessageBuilder = CargoWise.Customs.IE.MessageContracts.AIS.RD415MessageBuilder;

namespace Enterprise.Customs.IE.Business
{
	public class DepositRefundApplicationMessageSender : MessageSender
	{
		public DepositRefundApplicationMessageSender(DepositRefundApplicationMessageSendingAction sendingAction) : base(sendingAction)
		{
		}

		protected override IXmlMessageBuilder CreateMessageBuilder(OutboundEDIMessage outgoingMessage)
		{
			return SendingAction.EntryHeader.Declaration?.IsUCC5 ?? false ? new RD415UCC5MessageBuilder(new RD415Provider(SendingAction)) : new RD415UCC6MessageBuilder(new RD415Provider(SendingAction));
		}

		protected override OutboundEDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => SendingAction.EntryHeader.Declaration?.IsUCC5 ?? false ? factory.New<AISUCC5OutboundEDIMessage>() : factory.New<AISOutboundEDIMessage>();

		new DepositRefundApplicationMessageSendingAction SendingAction => (DepositRefundApplicationMessageSendingAction)base.SendingAction;
	}
}
