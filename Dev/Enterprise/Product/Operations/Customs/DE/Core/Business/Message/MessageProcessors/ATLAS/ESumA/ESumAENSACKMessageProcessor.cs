using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.ESumA
{
	public class ESumAENSACKMessageProcessor : ESumAMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>, IESumADataProvider>
	{
		public ESumAENSACKMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("f22e8ac9-3680-474b-a2c1-1ba733242e2e", "ESumA ENSACK Message Processor");
	}
}
