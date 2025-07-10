using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.IT.Business;

public class SadOutgoingMessageProcessor : ITOutgoingMessageProcessor
{
	public SadOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}
	protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new SadInterchangeProvider(readyMessages);

	protected override string OutgoingMessageType => SADConstants.CustomsInterchangeType.IdocR;
}
