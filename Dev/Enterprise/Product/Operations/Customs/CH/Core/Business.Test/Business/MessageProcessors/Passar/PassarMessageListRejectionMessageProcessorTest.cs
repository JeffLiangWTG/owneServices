using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business.Testing;

class PassarMessageListRejectionMessageProcessorTest : BaseMessageListRejectionMessageProcessorTest
{
	protected override ApplicationTypeMessageProcessor GetMessageProcessor() => new PassarMessageListRejectionMessageProcessor(Logger);

	protected override string ExpectedFriendlyName => "Passar Message List Rejection Message Processor";

	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsPassar;
}
