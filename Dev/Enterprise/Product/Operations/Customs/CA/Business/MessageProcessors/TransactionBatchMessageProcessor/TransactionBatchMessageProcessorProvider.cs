using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public static class TransactionBatchMessageProcessorProvider
	{
		public static ITransactionBatchMessageProcessor GetProcessor(Enterprise.Messaging.Business.EDIMessage message, TransactionBatch transactionBatch, IXmlSessionTracker logger)
		{
			ITransactionBatchMessageProcessor result = null;
			switch (transactionBatch.BatchType.Code)
			{
				case ARLMessageTypes.Codes.DailyNotice:
					result = new ARLDailyNoticeMessageProcessor(message, transactionBatch, logger);
					break;
				case ARLMessageTypes.Codes.StatementOfAccount:
					result = new ARLStatementOfAccountMessageProcessor(message, transactionBatch, logger);
					break;
#if DEBUG
				default:
					result = new ARLMessageProcessorForTesting(message, logger, transactionBatch.BatchType.Code.GetValueOrDefault());
					break;
#endif
			}

			return result;
		}
	}
}
