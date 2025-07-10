using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IvistoResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<IvistoResponseMessageProcessor>
{
	public void TestProcessNegativeResponseMessage()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoNegativeResponse.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: negativeResponse, messageType: "IVR");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "XYZ", entryHeader.CH_EntryStatus);
	}

	public void TestProcessPositiveResponseMessageResultCode200()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode200.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "IVR");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "EXI", entryHeader.CH_EntryStatus);
		AssertEntryNumber(entryHeader.EntryNumbersProvider.Ivisto, "IVI", "", "CUS", "IT279100", new ZDateTime(2023, 04, 28), "EXC");
	}

	public void TestProcessPositiveResponseMessageResultCode199()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode199.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "IVR");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "EXI", entryHeader.CH_EntryStatus);
		AssertEntryNumber(entryHeader.EntryNumbersProvider.Ivisto, "IVI", "", "CUS", "IT279100", new ZDateTime(2023, 04, 28), "EXC");
	}

	public void TestProcessPositiveResponseMessageWithoutExitData()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseNoExitData.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "IVR");
		entryHeader.CH_EntryStatus = "";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "", entryHeader.CH_EntryStatus);
		AssertNull("IVISTO EntryNumber", entryHeader.EntryNumbersProvider.Ivisto);
	}

	public void TestProcessPositiveResponseMessageExitInfoAreNotUpdatedIfOlder()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode200.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "IVR");

		var ivistoEntryNumber = Factory.NewCusEntryNumber(entryHeader, "IVI", "", new ZDateTime(2023, 04, 29), "IT123456");
		ivistoEntryNumber.CE_EntryStatus = "EXR";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEntryNumber(entryHeader.EntryNumbersProvider.Ivisto, "IVI", "", "CUS", "IT123456", new ZDateTime(2023, 04, 29), "EXR");
	}

	public void TestProcessPositiveResponseMessageExitInfoAreUpdatedIfNewer()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_IvistoPositiveResponseResultCode200.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "IVR");

		var ivistoEntryNumber = Factory.NewCusEntryNumber(entryHeader, "IVI", "", new ZDateTime(2023, 04, 27), "IT123456");
		ivistoEntryNumber.CE_EntryStatus = "EXR";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEntryNumber(entryHeader.EntryNumbersProvider.Ivisto, "IVI", "", "CUS", "IT279100", new ZDateTime(2023, 04, 28), "EXC");
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "IVR" };

	protected override IvistoResponseMessageProcessor GetMessageProcessor(LoggingInformation logger)
		=> new IvistoResponseMessageProcessor(logger);

	void AssertNumberOfResponseMessages(CusEntryHeader entryHeader, int expectedAcknowledgement)
	{
		var messages = entryHeader.Messages.Cast<EDIMessage>();
		AssertEquals("Number of Response Messages", expectedAcknowledgement, messages.Count(x => x.EM_MessageType == "IVR"));
	}
}
