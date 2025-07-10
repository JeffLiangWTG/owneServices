using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class BaseMessageListRejectionMessageProcessorTest : TestCaseWithFactory
{
	protected abstract ApplicationTypeMessageProcessor GetMessageProcessor();

	protected abstract string ExpectedFriendlyName { get; }

	protected abstract string ApplicationCode { get; }

	protected LoggingInformationForTesting Logger => logger ?? (logger = new LoggingInformationForTesting());
	LoggingInformationForTesting logger;

	public void TestFriendlyName()
	{
		AssertEquals(ExpectedFriendlyName, GetMessageProcessor().MessageFriendlyName);
	}

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCode, GetMessageProcessor().ApplicationCode);
	}

	public void TestLinkedToCompany() => CombineAssertions(() =>
	{
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, TestingData.InputUniversalEvent);

		GetMessageProcessor().ProcessMessage(ediMessage);

		AssertEquals("EM_LinkTable", company.TableName, ediMessage.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", company.PK, ediMessage.EM_LinkUniqueID);
	});

	public void TestEventAdded()
	{
		var (company, ediMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCode, MessageTypeCodeList.Codes.PassarNcts, MessageSubTypeCodeList.Codes.Undefined, TestingData.InputUniversalEvent);

		GetMessageProcessor().ProcessMessage(ediMessage);

		var logEvent = company.Logs.MostRecentLogByEventTime(Events.MessageRejected);

		AssertNotNull("Event written", logEvent);
		AssertEquals("Event Reference", $"|ITN={ediMessage.Interchange.EI_InterchangeNum}|SRC={ApplicationCode}|TYP=MSL", logEvent.SL_Reference);
	}
}
