using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AccountingSummaryResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<AccountingSummaryResponseMessageProcessor>
{
	public void TestProcessResponse()
	{
		var response = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_AccountingSummaryResponsePositiveResponseResultCode199.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: response, messageType: "PRR");
		sentMessage.EM_MessageSubType = "PRR";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertEquals("EM_MessageSubType", "PRR", receivedMessage.EM_MessageSubType);
		AssertSame("EM_LinkedObject", entryHeader, receivedMessage.EM_LinkedObject);
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => ["PRR"];

	protected override AccountingSummaryResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new AccountingSummaryResponseMessageProcessor(logger);
}
