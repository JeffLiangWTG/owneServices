using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputGetMessageRejectionMessageProcessorTest : BaseGetMessageRejectionMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "Chartera Output Get Message Rejection Message Processor";

	protected override string ApplicationCode => ApplicationCodes.CHCustomsCharteraOutput;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new CharteraOutputGetMessageRejectionMessageProcessor(Logger);
}
