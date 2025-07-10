using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputGetMessageRejectionMessageProcessor : BaseGetMessageRejectionMessageProcessor
{
	public CharteraOutputGetMessageRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Get Message Rejection Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsCharteraOutput;
}
