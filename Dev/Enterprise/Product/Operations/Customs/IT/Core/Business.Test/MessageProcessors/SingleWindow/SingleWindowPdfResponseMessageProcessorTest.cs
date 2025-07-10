using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SingleWindowPdfResponseMessageProcessorTest : SingleWindowIncomingMessageProcessorTest<SingleWindowPdfResponseMessageProcessor>
{
	public void TestProcessMessage()
	{
		(_, var entryHeader, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ImageData, DocumentType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		CombineAssertions(() =>
		{
			AssertProcessingResult(entryHeader, Filename, ImageData, DocumentType);
		});
	}

	public void TestProcessMessageEmptyDocumentType()
	{
		(_, _, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ImageData, ZString.Empty);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty document type");
	}

	public void TestProcessMessageEmptyFilename()
	{
		(_, _, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(ZString.Empty, ImageData, DocumentType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty filename");
	}

	public void TestProcessMessageEmptyImageData()
	{
		(_, _, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ZString.Empty, DocumentType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty image data");
	}

	public void TestEdocIsPersistedWithMainFactory()
	{
		(var declaration, _, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ImageData, DocumentType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions("Ensure calling main Factory.Save() saves declaration/entry changes and added eDoc", () =>
		{
			var separateFactory = new BusinessObjectFactory();
			var declarationOnSeparateFactory = separateFactory.Load<JobDeclaration>(declaration.PK);
			AssertNotNull("Declaration has been saved", declarationOnSeparateFactory);
			var entryHeaderOnSeparateFactory = declaration.CustomsEntryHeaders?[0];
			AssertNotNull("EntryHeader has been saved", entryHeaderOnSeparateFactory);
			AssertProcessingResult(entryHeaderOnSeparateFactory, Filename, ImageData, DocumentType);
		});
	}

	public void TestEdgeCases()
	{
		TestThrowsExceptionIfEmptyXml();
		TestThrowsExceptionIfInvalidXml();
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "PDF" };

	protected override SingleWindowPdfResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SingleWindowPdfResponseMessageProcessor(logger);

	(JobDeclaration declaration, CusEntryHeader entryHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestDataPdfMessageProcessor(ZString filename, ZString base64Data, ZString documentType)
	{
		var messageText =
				$@"<AttachedDocumentCollection> 
						<AttachedDocument> 
							<Filename>{filename}</Filename> 
							<ImageData>{base64Data}</ImageData> 
							<Type> 
								<Code>{documentType}</Code> 
								<Description>Clearance Document</Description>
							</Type> 
						</AttachedDocument> 
					</AttachedDocumentCollection>";

		return PrepareTestData(messageText: messageText);
	}

	void AssertProcessingResult(CusEntryHeader entryHeader, ZString filename, ZString imageData, ZString documentType)
	{
		AssertStatusUpdatedLog(entryHeader, "PDF");

		var allEdocs = entryHeader.Declaration.DocManagerInfo.AllEDocs;
		AssertEquals("eDocs count", 1, allEdocs.Count);
		var uniqueEdoc = allEdocs[0];
		AssertEquals("FileName", filename, uniqueEdoc.FileName);
		AssertEquals("DocType", documentType, uniqueEdoc.DocType);
		AssertEquals("ImageData", new ZBlob(Convert.FromBase64String(imageData)), uniqueEdoc.ImageData);
	}

	const string Filename = "Filename_Clearance.pdf";
	const string ImageData = "base64DataForPdf";
	const string DocumentType = "CLR";
}
