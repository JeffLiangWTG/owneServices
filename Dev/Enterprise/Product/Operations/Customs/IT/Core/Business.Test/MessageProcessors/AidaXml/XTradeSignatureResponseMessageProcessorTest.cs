using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XTradeSignatureResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<XtradeSignatureResponseMessageProcessor>
{
	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => ["SGN"];

	public void TestReceivedMessageSubTypeAndApplicationReferenceIsConsistentWithSentMessage_NewRequest()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeSignatureResponse.xml");
		var (_, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SGN");
		sentMessage.EM_MessageType = "NEW";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertMessageSubTypeAndApplicationReference(sentMessage, receivedMessage);
	}

	public void TestReceivedMessageSubTypeAndApplicationReferenceIsConsistentWithSentMessage_AmendmentRequest()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeSignatureResponse.xml");
		var (_, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SGN");
		sentMessage.EM_MessageType = "AMD";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertMessageSubTypeAndApplicationReference(sentMessage, receivedMessage);
	}

	public void TestReceivedMessageSubTypeAndApplicationReferenceIsConsistentWithSentMessage_CancellationRequest()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeSignatureResponse.xml");
		var (_, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SGN");
		sentMessage.EM_MessageType = "CAN";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertMessageSubTypeAndApplicationReference(sentMessage, receivedMessage);
	}

	public void TestReceivedMessageSubTypeAndApplicationReferenceIsConsistentWithSentMessage_IrildesRequest()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeSignatureResponse.xml");
		var (_, entryHeader, sentMessage, receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SGN");
		sentMessage.EM_MessageType = "IRR";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertEquals("EM_MessageSubType in sent and received message is the same", sentMessage.EM_MessageSubType, receivedMessage.EM_MessageSubType);
		AssertNullOrEmpty("EM_ApplicationReference in sent and received message is not the same", receivedMessage.EM_ApplicationReference);
	}

	void AssertMessageSubTypeAndApplicationReference(EDIMessage sentMessage, EDIMessage receivedMessage)
	{
		AssertEquals("EM_MessageNum in sent and received message is the same", sentMessage.EM_MessageNum, receivedMessage.EM_MessageNum);
		AssertEquals("EM_MessageSubType in sent and received message is the same", sentMessage.EM_MessageSubType, receivedMessage.EM_MessageSubType);
		AssertEquals("EM_ApplicationReference in sent and received message is the same", sentMessage.EM_ApplicationReference, receivedMessage.EM_ApplicationReference);
	}

	protected override XtradeSignatureResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new XtradeSignatureResponseMessageProcessor(logger);
}
