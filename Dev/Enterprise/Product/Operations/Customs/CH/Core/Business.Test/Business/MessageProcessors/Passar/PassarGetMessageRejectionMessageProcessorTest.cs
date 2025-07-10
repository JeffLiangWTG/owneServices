using static Enterprise.Messaging.Business.EDIInterchange;

namespace Enterprise.Customs.CH.Business.Testing;

class PassarGetMessageRejectionMessageProcessorTest : BaseGetMessageRejectionMessageProcessorTest
{
	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new PassarGetMessageRejectionMessageProcessor(Logger);

	protected override string ExpectedFriendlyName => "Passar Get Message Rejection Message Processor";

	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;
}
