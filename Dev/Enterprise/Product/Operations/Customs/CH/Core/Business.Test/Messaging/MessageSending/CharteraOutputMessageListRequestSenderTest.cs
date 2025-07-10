using Enterprise.BatchProcessor;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

class CharteraOutputMessageListRequestSenderTest : BaseMessageListRequestSenderTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsCharteraOutput;

	protected override string FriendlyName => "Chartera Output Message List Request";

	protected override string CustomsDestinationCode => MessagingConstants.CustomsDestinationCodes.CustomsCharteraOutput;

	protected override BaseMessageListRequestSender CreateMessageListRequestSender(LoggingInformation logger) => new CharteraOutputMessageListRequestSender(logger);
}
