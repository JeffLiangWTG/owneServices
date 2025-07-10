using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CharteraOutputGetMessageRequestSenderTest : BaseGetMessageRequestSenderTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsCharteraOutput;

	protected override string ExpectedDestination => MessagingConstants.CustomsDestinationCodes.CustomsCharteraOutput;

	protected override BasePassarCompanyMessageSender GetInstanceForTest(LoggingInformationForTesting loggingInformation) => new CharteraOutputGetMessageRequestSender(loggingInformation);
}
