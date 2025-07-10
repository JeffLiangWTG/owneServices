using System;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public abstract class NCTSLinkedGuaranteeMessageProcessor<TEDIMessage, TDataProvider> : NCTSGuaranteeMessageProcessor<TEDIMessage, TDataProvider> where TEDIMessage : NCTSInboundEDIMessage
	{
		protected NCTSLinkedGuaranteeMessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}
	}
}
