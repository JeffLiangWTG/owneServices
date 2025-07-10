using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SummaryProspectusResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<SummaryProspectusResponseMessageProcessor>
{
	public void TestProcessMessage()
	{
		var response = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusResponsePositiveResponseResultCode199.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: response, messageType: "SPR");
		sentMessage.EM_MessageSubType = "SPR";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertEquals("EM_MessageSubType", "SPR", receivedMessage.EM_MessageSubType);
		AssertSame("EM_LinkedObject", entryHeader, receivedMessage.EM_LinkedObject);
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => ["SPR"];

	protected override SummaryProspectusResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SummaryProspectusResponseMessageProcessor(logger);
}
