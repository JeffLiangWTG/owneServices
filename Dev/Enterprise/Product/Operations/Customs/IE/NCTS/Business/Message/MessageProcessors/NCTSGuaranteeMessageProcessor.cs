using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public abstract class NCTSGuaranteeMessageProcessor<TEDIMessage, TDataProvider> : NCTSDepartureMessageProcessor<TEDIMessage, TDataProvider> where TEDIMessage : NCTSInboundEDIMessage
	{
		public NCTSGuaranteeMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendGuaranteeNotifications;
	}
}
