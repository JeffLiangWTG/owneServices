using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public abstract class NCTSDepartureMessageProcessor<TEDIMessage, TDataProvider> : MessageAttacheeMessageProcessor<TEDIMessage, TDataProvider> where TEDIMessage : NCTSInboundEDIMessage
	{
		public NCTSDepartureMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.IECustomsNCTS;

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsAcknowledgements;

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;
	}
}
