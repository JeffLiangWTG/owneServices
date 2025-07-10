using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class BordereauErrorResponseMessageProcessorTest : TestCaseWithFactory
{
	string[] borSubTypes => [MessageSubTypeCodeList.Codes.XmlSchemaError, MessageSubTypeCodeList.Codes.RuleError, MessageSubTypeCodeList.Codes.Rejected];

	string ExpectedFriendlyName => "Bordereau Error Response Message Processor";

	string ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	BordereauErrorResponseMessageProcessor MessageProcessor => new BordereauErrorResponseMessageProcessor(Logger);

	LoggingInformationForTesting Logger => logger ??= new LoggingInformationForTesting();
	LoggingInformationForTesting logger;

	public void TestFriendlyName()
	{
		AssertEquals(ExpectedFriendlyName, MessageProcessor.MessageFriendlyName);
	}

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCode, MessageProcessor.ApplicationCode);
	}

	public void TestCanProcess()
	{
		CombineAssertions(() =>
		{
			foreach (var subType in borSubTypes)
			{
				var (entryHeader, receivedEdiMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, subType, ZString.Empty);

				Factory.Save();

				Assert(MessageProcessor.CanProcess(receivedEdiMessage));
			}
		});
	}

	public void TestBorPrsStatusForRexRerRej()
	{
		CombineAssertions(() =>
		{
			foreach (var subType in borSubTypes)
			{
				var (entryHeader, receivedEdiMessage) = MessageProcessorTestHelper.CreateCompanyMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.BOR, subType, ZString.Empty);

				Factory.Save();

				MessageProcessor.ProcessMessage(receivedEdiMessage);
				Factory.Save();

				AssertEquals($"MessageType:BOR, SubType:{subType} → EM_Status", EDIMessage.Status.ProcessedOK, receivedEdiMessage.EM_Status);
			}
		});
	}
}
