using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XTradeErrorResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<XTradeErrorResponseMessageProcessor>
{
	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForImport()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "ERR");
		sentMessage.Interchange.EI_InterchangeType = "IMP";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "FFT", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForExport()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "ERR");
		sentMessage.Interchange.EI_InterchangeType = "EXP";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "FFT", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForTransit()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "ERR");
		sentMessage.Interchange.EI_InterchangeType = "TRA";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "FFT", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasFailedForElectronicFolder()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml");
		(_, var entryHeader, var sentMessage, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "ERR");
		sentMessage.Interchange.EI_InterchangeType = "EFQ";
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsNotAwaitingResponseAndTransmissionHasFailed()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_EventTypeIRJ_TransmissionFailure.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "ERR");
		entryHeader.CH_Status = "ACO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "ACO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_WhenEntryIsAwaitingResponseAndTransmissionHasNotFailed()
	{
		var errorMessageContent = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.XTradeErrorFile_MultipleEvents_NotTransmissionFailure.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "", messageText: errorMessageContent, messageType: "ERR");
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	public void TestProcessMessage_CorruptedUniversalInterchange()
	{
		var corruptedXml = "<UniversalEven????";
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(messageText: corruptedXml, messageType: "ERR");
		entryHeader.CH_Status = "AWO";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfErrorMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_Status), "AWO", entryHeader.CH_Status);
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "ERR" };

	protected override XTradeErrorResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
		=> new XTradeErrorResponseMessageProcessor(logger);

	void AssertNumberOfErrorMessages(CusEntryHeader entryHeader, int expectedNumberOfErrorMessages)
	{
		var numberOfErrorMessages = entryHeader.Messages.Cast<EDIMessage>().Count(x => x.EM_MessageType == "ERR");
		AssertEquals("Number of Error Messages", expectedNumberOfErrorMessages, numberOfErrorMessages);
	}
}
