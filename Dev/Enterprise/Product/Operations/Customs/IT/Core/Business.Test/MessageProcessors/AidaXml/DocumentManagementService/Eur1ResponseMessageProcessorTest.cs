using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Eur1ResponseMessageProcessorTest : XmlIncomingMessageProcessorTest<Eur1ResponseMessageProcessor>
{
	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "EU1" };

	readonly string documentName = "EUR1_24ITQ0B01AA28984A9.pdf";
	readonly string documentType = "COO";

	public void TestProcessNegativeResponseMessage()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_Eur1NegativeResponse.xml");

		(_,  var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: negativeResponse, messageType: "EU1");
		entryHeader.CH_EntryStatus = "XYZ";

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(entryHeader, 1);
		AssertEquals(nameof(entryHeader.CH_EntryStatus), "XYZ", entryHeader.CH_EntryStatus);
	}

	public void TestProcessPositiveResponseMessageResultCode200()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_Eur1PositiveResponseResultCode200_sample.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "EU1");
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
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_Eur1PositiveResponseResultCode199_sample.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "EU1");
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
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.Business.Testing.MessageProcessors.AidaXml.TestFiles.Export_Eur1PositiveResponseResultCode199_sample_with_EmptyContent.xml");

		(_, var entryHeader, _, var receivedMessage) = PrepareTestData(declarationType: "EXP", messageText: positiveResponse, messageType: "EU1");
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
		AssertEquals("Number of Response Messages", expectedAcknowledgement, messages.Count(x => x.EM_MessageType == "EU1"));
	}

	void AssertDocumentWasAdded(Business.Declaration.CusEntryHeader entryHeader)
	{
		var eDocs = entryHeader.DocManagerInfo().AllEDocs;
		var uniqueEdoc = eDocs[0];

		AssertEquals("eDocs count", 1, eDocs.Count);
		AssertNotNull("Document is not null", uniqueEdoc);
		AssertEquals("DocType is COO", documentType, uniqueEdoc.DocType);
		AssertEquals("Data is not empty", true, uniqueEdoc.ImageData.Length > 0);
		AssertEquals("FileName", documentName, uniqueEdoc.FileName);
	}

	protected override Eur1ResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new Eur1ResponseMessageProcessor(logger);
}

