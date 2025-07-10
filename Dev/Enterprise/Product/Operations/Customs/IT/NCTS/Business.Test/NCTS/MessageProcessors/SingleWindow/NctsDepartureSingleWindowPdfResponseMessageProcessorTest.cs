using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsDepartureSingleWindowPdfResponseMessageProcessorTest : NctsDepartureSingleWindowIncomingMessageProcessorTest<SingleWindowPdfResponseMessageProcessor>
{
	public void TestProcessResponse()
	{
		(var entryHeader, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ImageData, DocumentType);

		processor.ProcessMessage(receivedMessage);
		CombineAssertions(() =>
		{
			AssertProcessingResult(entryHeader, Filename, ImageData, DocumentType);
		});
	}

	public void TestProcessMessageEmptyDocumentType()
	{
		(_, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ImageData, ZString.Empty);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty document type");
	}

	public void TestProcessMessageEmptyFilename()
	{
		(_, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(ZString.Empty, ImageData, DocumentType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty filename");
	}

	public void TestProcessMessageEmptyImageData()
	{
		(_, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ZString.Empty, DocumentType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		AssertFailMessage(receivedMessage, receivedMessage.Interchange, "Unable to parse the message content.", "Empty image data");
	}

	public void TestEdocIsPersistedWithMainFactory()
	{
		(var nctsHeader, _, var receivedMessage) = PrepareTestDataPdfMessageProcessor(Filename, ImageData, DocumentType);

		var processor = GetMessageProcessor(logger);
		processor.ProcessMessage(receivedMessage);
		Factory.Save();

		CombineAssertions("Ensure calling main Factory.Save() saves NCTS departure declaration changes and added eDoc", () =>
		{
			var separateFactory = new BusinessObjectFactory();
			var nctsHeaderOnSeparateFactory = separateFactory.Load<NctsHeader>(nctsHeader.PK);
			AssertNotNull("NCST Declaration has been saved", nctsHeaderOnSeparateFactory);
			AssertProcessingResult(nctsHeaderOnSeparateFactory, Filename, ImageData, DocumentType);
		});
	}

	public void TestEdgeCases()
	{
		TestThrowsExceptionIfEmptyXml();
		TestThrowsExceptionIfInvalidXml();
	}

	protected override IReadOnlyList<ZString> ExpectedMessageTypesToInclude => new ZString[] { "PDF" };

	protected override SingleWindowPdfResponseMessageProcessor GetMessageProcessor(LoggingInformation logger) => new SingleWindowPdfResponseMessageProcessor(logger);

	(NctsHeader nctsHeader, EDIMessage sentMessage, EDIMessage receivedMessage) PrepareTestDataPdfMessageProcessor(ZString filename, ZString base64Data, ZString documentType)
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

	void AssertProcessingResult(NctsHeader nctsHeader, ZString filename, ZString imageData, ZString documentType)
	{
		AssertStatusUpdatedLog(nctsHeader, "PDF");

		var allEdocs = nctsHeader.DocManagerInfo.AllEDocs;
		AssertEquals("eDocs count", 1, allEdocs.Count);
		var uniqueEdoc = allEdocs[0];
		AssertEquals("FileName", filename, uniqueEdoc.FileName);
		AssertEquals("DocType", documentType, uniqueEdoc.DocType);
		AssertEquals("ImageData", new ZBlob(Convert.FromBase64String(imageData)), uniqueEdoc.ImageData);
	}

	const string Filename = "1 T40982021025100_Clearance.pdf";
	const string ImageData = "base64DataForPdf";
	const string DocumentType = "CLR";
}
