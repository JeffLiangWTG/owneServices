using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business.Testing;

class CHCOutboundMessageProcessorTest : TestCaseWithFactory
{
	public void TestCreateInterchange()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_DeclarationReference = "B00183715";

		var entryHeaderCorrectXML = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderCorrectXML.CH_BGMReference = "CH000001";
		var correctMessageXML = Factory.CreateOutboundMessage(MessageTypeCodeList.Codes.Import, TestingData.MessageBodyText,
			applicationCode: ApplicationCodeList.Codes.CHCustomsEdec,
			direction: CHEDIMessage.Direction.Transmit,
			status: CHEDIMessage.Status.Queued);
		correctMessageXML.EM_LinkedObject = entryHeaderCorrectXML;

		var incorrectApplicationCodeMessage = Factory.CreateOutboundMessage(MessageTypeCodeList.Codes.Import, TestingData.MessageBodyText,
			applicationCode: "AAA",
			direction: CHEDIMessage.Direction.Transmit,
			status: CHEDIMessage.Status.Queued);
		var incorrectDirectionMessage = Factory.CreateOutboundMessage(MessageTypeCodeList.Codes.Import, TestingData.MessageBodyText,
			applicationCode: ApplicationCodeList.Codes.CHCustomsEdec,
			direction: CHEDIMessage.Direction.Receive,
			status: CHEDIMessage.Status.Queued);
		var incorrectStatusMessage = Factory.CreateOutboundMessage(MessageTypeCodeList.Codes.Import, TestingData.MessageBodyText,
			applicationCode: ApplicationCodeList.Codes.CHCustomsEdec,
			direction: CHEDIMessage.Direction.Transmit,
			status: CHEDIMessage.Status.Withdrawn);

		var nullLinkedObjectMessage = Factory.CreateOutboundMessage(MessageTypeCodeList.Codes.Import, TestingData.MessageBodyText,
			applicationCode: ApplicationCodeList.Codes.CHCustomsEdec,
			direction: CHEDIMessage.Direction.Transmit,
			status: CHEDIMessage.Status.Queued);

		var entryHeaderEmptyText = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderEmptyText.CH_BGMReference = "CH000004";
		var emptyTextMessage = Factory.CreateOutboundMessage(MessageTypeCodeList.Codes.Import, ZString.Empty,
			applicationCode: ApplicationCodeList.Codes.CHCustomsEdec,
			direction: CHEDIMessage.Direction.Transmit,
			status: CHEDIMessage.Status.Queued);
		emptyTextMessage.EM_LinkedObject = entryHeaderEmptyText;

		Factory.Save();

		CombineAssertions("PRE-CONDITION", () =>
		{
			AssertNull($"{nameof(correctMessageXML)} Message Interchange", correctMessageXML.Interchange);
			AssertNull($"{nameof(incorrectApplicationCodeMessage)} Message Interchange", incorrectApplicationCodeMessage.Interchange);
			AssertNull($"{nameof(incorrectDirectionMessage)} Message Interchange", incorrectDirectionMessage.Interchange);
			AssertNull($"{nameof(incorrectStatusMessage)} Message Interchange", incorrectStatusMessage.Interchange);
			AssertNull($"{nameof(nullLinkedObjectMessage)} Message Interchange", nullLinkedObjectMessage.Interchange);
			AssertNull($"{nameof(emptyTextMessage)} Message Interchange", emptyTextMessage.Interchange);
		});

		var logger = new LoggingInformation();
		var processor = new CHCOutboundMessageProcessor(logger);
		processor.ProcessMessage(CancellationToken.None);

		correctMessageXML.Reload();
		incorrectApplicationCodeMessage.Reload();
		incorrectDirectionMessage.Reload();
		incorrectStatusMessage.Reload();
		nullLinkedObjectMessage.Reload();
		emptyTextMessage.Reload();

		CombineAssertions("RESULT", () =>
		{
			AssertEquals($"{nameof(correctMessageXML)} Status", CHEDIMessage.Status.Sent, correctMessageXML.EM_Status);
			AssertEquals($"{nameof(incorrectApplicationCodeMessage)} Status", CHEDIMessage.Status.Queued, incorrectApplicationCodeMessage.EM_Status);
			AssertEquals($"{nameof(incorrectDirectionMessage)} Status", CHEDIMessage.Status.Queued, incorrectDirectionMessage.EM_Status);
			AssertEquals($"{nameof(incorrectStatusMessage)} Status", CHEDIMessage.Status.Withdrawn, incorrectStatusMessage.EM_Status);
			AssertEquals($"{nameof(nullLinkedObjectMessage)} Status", CHEDIMessage.Status.Failed, nullLinkedObjectMessage.EM_Status);
			AssertEquals($"{nameof(emptyTextMessage)} Status", CHEDIMessage.Status.Failed, emptyTextMessage.EM_Status);

			AssertNotNull($"{nameof(correctMessageXML)} Interchange", correctMessageXML.Interchange);
			AssertNull($"{nameof(incorrectApplicationCodeMessage)} Interchange", incorrectApplicationCodeMessage.Interchange);
			AssertNull($"{nameof(incorrectDirectionMessage)} Interchange", incorrectDirectionMessage.Interchange);
			AssertNull($"{nameof(incorrectStatusMessage)} Interchange", incorrectStatusMessage.Interchange);
			AssertNull($"{nameof(nullLinkedObjectMessage)} Interchange", nullLinkedObjectMessage.Interchange);
			AssertNull($"{nameof(emptyTextMessage)} Interchange", emptyTextMessage.Interchange);
		});
	}

	public void TestCreateInterchangeForDirectxT()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_DeclarationReference = "B00183715";

		var entryHeaderCorrectXML = declaration.CustomsEntryHeaders.AddNew();
		entryHeaderCorrectXML.CH_BGMReference = "CH000001";
		var correctMessageXML = Factory.CreateOutboundMessage(MessageTypeCodeList.Codes.Import, TestingData.MessageBodyText,
			applicationCode: ApplicationCodeList.Codes.CHCustomsEdec,
			direction: CHEDIMessage.Direction.Transmit,
			status: CHEDIMessage.Status.Queued);
		correctMessageXML.EM_LinkedObject = entryHeaderCorrectXML;

		Factory.Save();

		var logger = new LoggingInformation();
		var processor = new CHCOutboundMessageProcessor(logger);
		processor.ProcessMessage(CancellationToken.None);

		correctMessageXML.Reload();

		TestingData.AssertOutgoingInterchange("XML", correctMessageXML.Interchange,
			expectedInterchangeType: MessageTypeCodeList.Codes.Import,
			expectedDestination: MessagingConstants.CustomsDestinationCodes.CustomsEdecSoap,
			expectedTransportType: EDIInterchange.TransportType.xT,
			expectedStatus: EDIInterchange.Status.Queued,
			expectedBody: TestingData.SoapMessageText);
	}

	public void TestGetMessageFilterQuery() => CombineAssertions(() =>
	{
		var message1 = Factory.CreateOutboundMessage(applicationCode: ApplicationCodeList.Codes.CHCustomsEdec, messageType: MessageTypeCodeList.Codes.Export, messageText: ZString.Empty);
		var message2 = Factory.CreateOutboundMessage(applicationCode: ApplicationCodeList.Codes.CHCustomsPassar, messageType: MessageTypeCodeList.Codes.PassarNcts, messageText: ZString.Empty);
		var message3 = Factory.CreateOutboundMessage(applicationCode: ApplicationCodeList.Codes.CHCustomsCharteraOutput, messageType: MessageTypeCodeList.Codes.REQ, messageText: ZString.Empty);
		Factory.Save();

		var logger = new LoggingInformation();
		var processor = new CHCOutboundMessageProcessor(logger);
		processor.ProcessMessage(CancellationToken.None);

		message1.Reload();
		AssertNotEquals($"EM_ApplicationCode={message1.EM_ApplicationCode}", EDIMessageStatusList.Codes.Queued, message1.EM_Status);
		message2.Reload();
		AssertEquals($"EM_ApplicationCode={message2.EM_ApplicationCode}", EDIMessageStatusList.Codes.Queued, message2.EM_Status);
		message3.Reload();
		AssertEquals($"EM_ApplicationCode={message3.EM_ApplicationCode}", EDIMessageStatusList.Codes.Queued, message3.EM_Status);
	});
}
