using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SummaryProspectusDownloadMessageProcessorTest : XmlIncomingMessageProcessorTest<SummaryProspectusDownloadMessageProcessor>
{
	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "SPD" };

	const string documentName = "SPD_25ITQYH7TAA00605R6.pdf";
	const string documentType = "MCD";

	public void TestProcessNegativeResponseMessage()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusDownloadNegativeResponse.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: negativeResponse, messageType: "SPD");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertEquals("File should not be added when response message is neagative", 0, entryHeader.DocManagerInfo().AllEDocs.Count);
	}

	public void TestProcessPositiveResponseMessageResultCode200()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusDownloadPositiveResponseResultCode200.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SPD");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertDocumentWasAdded(entryHeaderOnSeparateFactory);
	}

	public void TestProcessPositiveResponseMessageResultCode199()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusDownloadPositiveResponseResultCode199.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SPD");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertDocumentWasAdded(entryHeaderOnSeparateFactory);
	}

	public void TestProcessPositiveResponseWithEmptyFileContent()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_SummaryProspectusDownloadPositiveResponseResultCode199_sample_with_EmptyContent.xml");
		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SPD");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertEquals("File should not be added when content is empty", 0, entryHeader.DocManagerInfo().AllEDocs.Count);
	}

	void AssertNumberOfResponseMessages(Business.Declaration.CusEntryHeader entryHeader, int expectedAcknowledgement)
	{
		var messages = entryHeader.Messages.Cast<EDIMessage>();
		AssertEquals("Number of Response Messages", expectedAcknowledgement, messages.Count(x => x.EM_MessageType == "SPD"));
	}

	void AssertDocumentWasAdded(Business.Declaration.CusEntryHeader entryHeader)
	{
		var eDocs = entryHeader.DocManagerInfo().AllEDocs;
		var uniqueEdoc = eDocs[0];

		AssertEquals("eDocs Count", 1, eDocs.Count);
		AssertNotNull("Document is not null", uniqueEdoc);
		AssertEquals("DocType is MCD", documentType, uniqueEdoc.DocType);
		AssertEquals("Data is not empty", true, uniqueEdoc.ImageData.Length > 0);
		AssertEquals("FileName", documentName, uniqueEdoc.FileName);
	}

	protected override SummaryProspectusDownloadMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SummaryProspectusDownloadMessageProcessor(logger);
}

