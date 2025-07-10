using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SingleWindowOutgoingMessageProcessorTest : ITOutgoingMessageProcessorBaseTest<SingleWindowOutgoingMessageProcessor>
{
	protected override string OutgoingMessageTypeToTest => MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;

	protected override SingleWindowOutgoingMessageProcessor GetOutgoingMessageProcessor(LoggingInformation logger) => new SingleWindowOutgoingMessageProcessor(logger);
}
