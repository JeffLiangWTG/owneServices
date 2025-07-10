using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SadOutgoingMessageProcessorTest : ITOutgoingMessageProcessorBaseTest<SadOutgoingMessageProcessor>
{
	protected override string OutgoingMessageTypeToTest => SADConstants.CustomsInterchangeType.IdocR;

	protected override SadOutgoingMessageProcessor GetOutgoingMessageProcessor(LoggingInformation logger) => new SadOutgoingMessageProcessor(logger);
}
