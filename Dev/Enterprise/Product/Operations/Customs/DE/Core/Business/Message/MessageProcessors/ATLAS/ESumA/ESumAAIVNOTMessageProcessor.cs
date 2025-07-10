using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.ESumA
{
	public class ESumAAIVNOTMessageProcessor : ESumAMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>, IESumADataProvider>
	{
		public ESumAAIVNOTMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5a61601b-546e-451e-9c23-b9f9d1429118", "ESumA AIVNOT Message Processor");
	}
}
