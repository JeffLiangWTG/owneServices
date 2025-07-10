using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.IT.Business;

public class SingleWindowOutgoingMessageProcessor : ITOutgoingMessageProcessor
{
	public SingleWindowOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new SingleWindowInterchangeProvider(readyMessages);

	protected override string OutgoingMessageType => MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;
}
