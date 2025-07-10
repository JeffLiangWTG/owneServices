using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputMessageListRejectionMessageProcessorTest : BaseMessageListRejectionMessageProcessorTest
{
	protected override ApplicationTypeMessageProcessor GetMessageProcessor() => new CharteraOutputMessageListRejectionMessageProcessor(Logger);

	protected override string ExpectedFriendlyName => "Chartera Output Message List Rejection Message Processor";

	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsCharteraOutput;
}
