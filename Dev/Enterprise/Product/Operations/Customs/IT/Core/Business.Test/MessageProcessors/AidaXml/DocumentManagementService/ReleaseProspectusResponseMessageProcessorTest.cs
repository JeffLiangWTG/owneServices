using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ReleaseProspectusResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<ReleaseProspectusResponseMessageProcessor>
{
	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "SVI" };

	readonly string documentName = "SVI_24ITQYH4TAA11834R0.pdf";
	readonly string documentType = "CLR";

	public void TestProcessNegativeResponseMessage()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_ReleaseProspectusNegativeResponse.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: negativeResponse, messageType: "SVI");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "XYZ", entryHeader.CH_EntryStatus);
	}

	public void TestProcessPositiveResponseMessageResultCode200()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_ReleaseProspectusPositiveResponseResultCode200.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SVI");
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
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_ReleaseProspectusPositiveResponseResultCode199.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SVI");
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
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Import_ReleaseProspectusPositiveResponseResultCode199WithEmptyContent.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "IMP", messageText: positiveResponse, messageType: "SVI");
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
		AssertEquals("Number of Response Messages", expectedAcknowledgement, messages.Count(x => x.EM_MessageType == "SVI"));
	}

	void AssertDocumentWasAdded(Business.Declaration.CusEntryHeader entryHeader)
	{
		var eDocs = entryHeader.DocManagerInfo().AllEDocs;
		var uniqueEdoc = eDocs[0];

		AssertEquals("eDocs count", 1, eDocs.Count);
		AssertNotNull("Document is not null", uniqueEdoc);
		AssertEquals("DocType is CLR", documentType, uniqueEdoc.DocType);
		AssertEquals("Data is not empty", true, uniqueEdoc.ImageData.Length > 0);
		AssertEquals("FileName", documentName, uniqueEdoc.FileName);
	}

	protected override ReleaseProspectusResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new ReleaseProspectusResponseMessageProcessor(logger);
}
