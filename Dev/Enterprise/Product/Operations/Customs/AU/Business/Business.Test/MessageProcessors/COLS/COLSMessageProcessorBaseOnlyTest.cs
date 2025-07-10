using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSMessageProcessorBaseOnlyTest : COLSMessageProcessorAbstractTest
	{
		protected override COLSMessageProcessor GetMessageProcessor() => new COLSMessageProcessorForTest(new LoggingInformation(), "", "");

		sealed class COLSMessageProcessorForTest : COLSMessageProcessor
		{
			public COLSMessageProcessorForTest(LoggingInformation logger, string messageType, string messageFriendlyName)
				: base(logger, messageType, messageFriendlyName)
			{
			}

			protected override string DoProcessingReturningStatus(EDIMessage message) => string.Empty;
		}
	}
}
