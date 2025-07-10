using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business.Testing;

class PassarMessageListAcceptanceMessageProcessorTest : BaseMessageListAcceptanceMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "Passar Message List Acceptance Message Processor";

	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override ApplicationTypeMessageProcessor GetMessageProcessor() => new PassarMessageListAcceptanceMessageProcessor(Logger);
}
