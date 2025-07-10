using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.Business.AES
{
	public abstract class ExitControlMessageProcessor<TEDIMessage, TDataProvider> : AESMessageProcessor<TEDIMessage, TDataProvider> where TEDIMessage : AESInboundEDIMessage
	{
		public ExitControlMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override IRegistryItem GetEmailGroupRegistryItem() => ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements;
	}
}
