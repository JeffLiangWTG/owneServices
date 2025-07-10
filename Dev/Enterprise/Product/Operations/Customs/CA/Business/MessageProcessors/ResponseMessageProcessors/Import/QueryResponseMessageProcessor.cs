using Enterprise.BatchProcessor;
using Enterprise.Customs.CA.Business.MessageManagers;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class QueryResponseMessageProcessor : ImportResponseMessageProcessor
	{
		public QueryResponseMessageProcessor(LoggingInformation logger)
			: base(logger, new B3ImportStatusCalculator(), MessageTypeList.Codes.Query, Res.GetString("bd966933-7eac-47b2-9012-c195d3c8f017", "Query Response"))
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			return EDIMessage.Status.Received;
		}
	}
}
