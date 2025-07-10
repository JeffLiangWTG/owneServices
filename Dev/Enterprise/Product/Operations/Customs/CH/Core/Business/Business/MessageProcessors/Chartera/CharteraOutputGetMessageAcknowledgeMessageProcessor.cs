using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public abstract class CharteraOutputGetMessageAcknowledgeMessageProcessor : BaseGetMessageInboundMessageProcessor
{
	protected CharteraOutputGetMessageAcknowledgeMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsCharteraOutput;

	protected internal override bool MustProcessInOrder => false;
}
