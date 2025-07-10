using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class CharteraOutputAcknowledgeMessageProcessorTest : TestCaseWithFactory
{
	string ExpectedFriendlyName => "Chartera Output Acknowledge Message Processor";

	string ApplicationCode => CusPollingTransaction.ApplicationCodes.CHCustomsCharteraOutput;

	CharteraOutputAcknowledgeMessageProcessor MessageProcessor => new CharteraOutputAcknowledgeMessageProcessor(Logger);

	LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestFriendlyName()
	{
		AssertEquals(ExpectedFriendlyName, MessageProcessor.MessageFriendlyName);
	}

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCode, MessageProcessor.ApplicationCode);
	}

	public void TestLinkedToCompany() => CombineAssertions(() =>
	{
		var (company, message) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, messageType: MessageTypeCodeList.Codes.REQ, messageSubType: MessageSubTypeCodeList.Codes.Acknowledged, messageResponse: TestingData.InputUniversalEventReq);
		var outgoingMessage = MessageProcessorTestHelper.GetOutgoingMessage(message);
		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		AssertSame("EM_LinkedObject expected same as outgoing message", outgoingMessage.EM_LinkedObject, message.EM_LinkedObject);
		AssertEquals(EDIMessage.Status.ProcessedOK, message.EM_Status);
	});
}
