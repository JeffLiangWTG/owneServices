using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class PassarGetMessageRequestSenderTest : BaseGetMessageRequestSenderTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string ExpectedDestination => MessagingConstants.CustomsDestinationCodes.CustomsPassar;

	protected override BasePassarCompanyMessageSender GetInstanceForTest(LoggingInformationForTesting loggingInformation) => new PassarGetMessageRequestSender(loggingInformation);
}
