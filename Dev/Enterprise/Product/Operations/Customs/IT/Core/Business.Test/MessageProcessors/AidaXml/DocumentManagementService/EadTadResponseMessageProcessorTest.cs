using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EadTadResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<EadTadResponseMessageProcessor>
{
	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "EAD", "TAD" };

	public void TestProcessEADNegativeResponseMessage()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_EadNegativeResponse.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: negativeResponse, messageType: "EAD");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "XYZ", entryHeader.CH_EntryStatus);
	}

	public void TestProcessEADPositiveResponseMessageResultCode200()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_EadPositiveResponseResultCode200.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "EAD");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertDocumentWasAdded(entryHeaderOnSeparateFactory);
	}

	public void TestProcessEADPositiveResponseMessageResultCode199()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_EadPositiveResponseResultCode199.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "EAD");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		var separateFactory = new BusinessObjectFactory();
		var entryHeaderOnSeparateFactory = separateFactory.Load<Business.Declaration.CusEntryHeader>(entryHeader.PK);

		AssertNumberOfResponseMessages(entryHeaderOnSeparateFactory, 1);
		AssertDocumentWasAdded(entryHeaderOnSeparateFactory);
	}

	public void TestProcessEADPositiveResponseWithEmptyFileContent()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_EadPositiveResponseResultCode199WithEmptyContent.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "EAD");
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
		AssertEquals("Number of Response Messages", expectedAcknowledgement, messages.Count(x => x.EM_MessageType == "EAD"));
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

	protected override EadTadResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new EadTadResponseMessageProcessor(logger);

	const string documentName = "EAD_24ITQ0B01AA28984A9.pdf";
	const string documentType = "CLR";
}

