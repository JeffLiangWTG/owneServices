using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.ESumA
{
	public class ESumAENSSTAMessageProcessor : ESumAMessageProcessor<AtlasInboundEDIMessage<IESumADataProvider>, IESumADataProvider>
	{
		public ESumAENSSTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("6fa386b9-dfd5-4723-913e-af53d0c6737e", "ESumA ENSSTA Message Processor");
	}
}
