using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NctsTadResponseMessageProcessorTest : NctsResponseMessageProcessorAbstractTest<EadTadResponseMessageProcessor>
{
	public void TestProcessTADNegativeResponseMessage()
	{
		var negativeResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_TadNegativeResponse.xml");

		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageText: negativeResponse, responseMessageType: "TAD");
		var movementHeader = nctsHeader.MovementHeader;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);

		AssertNumberOfResponseMessages(movementHeader, 1);
		AssertEquals("File should not be added when the response is not valid", 0, nctsHeader.DocManagerInfo().AllEDocs.Count);
	}

	public void TestProcessTADPositiveResponseMessageResultCode200()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_TadPositiveResponseResultCode200.xml");

		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageText: positiveResponse, responseMessageType: "TAD");
		var movementHeader = nctsHeader.MovementHeader;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		AssertNumberOfResponseMessages(movementHeader, 1);
		AssertDocumentWasAdded(nctsHeader);
	}

	public void TestProcessTADPositiveResponseMessageResultCode199()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_TadPositiveResponseResultCode199.xml");

		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageText: positiveResponse, responseMessageType: "TAD");
		var movementHeader = nctsHeader.MovementHeader;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		AssertNumberOfResponseMessages(movementHeader, 1);
		AssertDocumentWasAdded(nctsHeader);
	}

	public void TestProcessTADPositiveResponseWithEmptyFileContent()
	{
		var positiveResponse = ManifestResourceHelper.ReadManifestResourceContent("Enterprise.Customs.IT.NCTS.Business.Testing.NCTS.MessageProcessors.Departure.AidaXml.TestFiles.Ncts_TadPositiveResponseResultCode199WithEmptyContent.xml");

		var (nctsHeader, _, receivedMessage) = PrepareTestData(messageText: positiveResponse, responseMessageType: "TAD");
		var movementHeader = nctsHeader.MovementHeader;

		var processor = GetMessageProcessor();
		processor.ProcessMessage(receivedMessage);

		Factory.Save();

		AssertNumberOfResponseMessages(movementHeader, 1);
		AssertEquals("File should not be added when content is empty", 0, nctsHeader.DocManagerInfo().AllEDocs.Count);
	}

	void AssertNumberOfResponseMessages(NctsDepartureMovementHeader movementHeader, int expectedAcknowledgement)
	{
		var messages = movementHeader.Messages.Cast<EDIMessage>();
		AssertEquals("Number of Response Messages", expectedAcknowledgement, messages.Count(x => x.EM_MessageType == "TAD"));
	}

	void AssertDocumentWasAdded(NctsHeader nctsHeader)
	{
		var eDocs = nctsHeader.DocManagerInfo().AllEDocs;
		var uniqueEdoc = eDocs[0];

		AssertEquals("eDocs count", 1, eDocs.Count);
		AssertNotNull("Document is not null", uniqueEdoc);
		AssertEquals("DocType is CLR", documentType, uniqueEdoc.DocType);
		AssertEquals("Data is not empty", true, uniqueEdoc.ImageData.Length > 0);
		AssertEquals("FileName", documentName, uniqueEdoc.FileName);
	}

	protected override EadTadResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new EadTadResponseMessageProcessor(logger);

	const string documentName = "TAD_24ITQTU08AA28957J9.pdf";
	const string documentType = "CLR";
}

