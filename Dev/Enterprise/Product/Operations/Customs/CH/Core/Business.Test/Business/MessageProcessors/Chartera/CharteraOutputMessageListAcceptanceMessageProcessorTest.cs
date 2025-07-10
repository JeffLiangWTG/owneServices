using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputMessageListAcceptanceMessageProcessorTest : BaseMessageListAcceptanceMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "Chartera Output Message List Acceptance Message Processor";

	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsCharteraOutput;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor() => new CharteraOutputMessageListAcceptanceMessageProcessor(Logger);
}
