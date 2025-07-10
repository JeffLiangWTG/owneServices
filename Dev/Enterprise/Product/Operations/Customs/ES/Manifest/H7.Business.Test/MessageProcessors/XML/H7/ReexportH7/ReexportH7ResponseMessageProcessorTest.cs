using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ReexportacionH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class ReexportH7ResponseMessageProcessorTest : H7CommonResponseMessageProcessorTest<ReexportH7ResponseMessageProcessor, ReexportH7MessagePrettyFormatter, ReexportacionH7V1Sal>
	{
		protected override ZString GetExpectedProcessorFriendlyName() => "Reexport H7 Response Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.H7ReExport };

		protected override ReexportH7ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ReexportH7ResponseMessageProcessor(logger);

		public void TestProcessAcceptedMessage_Type0_CsvId()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType0CsvId(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>0: Invalidate H7 (Re-Export through an EXS/ETD)</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV0000000002029</td></tr></table>";

			AssertReexportH7AcceptedResults(message, bill, expectedBillStatus: AcceptedBillStatus, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type2_CsvId()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType2CsvId(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>2: Annulation of a Re-Export through an EXS/ETD </td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV0000000002029</td></tr></table>";

			AssertReexportH7AcceptedResults(message, bill, expectedBillStatus: AcceptedBillStatus, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type3_CsvId()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType3CsvId(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>3: Invalidate H7 (Re-Export through a Transit procedure)</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV0000000002029</td></tr></table>";

			AssertReexportH7AcceptedResults(message, bill, expectedBillStatus: AcceptedBillStatus, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		public void TestProcessAcceptedMessage_Type4_CsvId()
		{
			SetupTestData();
			var message = CreateNewEDIMessage(ApplicationReference, GetAcceptedMessageWithType4CsvId(), InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H3>Accepted Reexport</H3>" +
				"<br><table border=\"0\"><tr><td>Declaration Type:</td><td>&nbsp;&nbsp;</td><td>4: Annulation of a Re-Export through a Transit procedure</td></tr></table>" +
				"<table border=\"0\"><tr><td>Response Code:</td><td>&nbsp;&nbsp;</td><td>A</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV0000000002029</td></tr></table>";

			AssertReexportH7AcceptedResults(message, bill, expectedBillStatus: AcceptedBillStatus, expectedMessageInterpretation: expectedMessageInterpretationText);
		}

		void AssertReexportH7AcceptedResults(TestEdiMessage responseMessage, AsycudaBill bill, string expectedBillStatus, string expectedMessageInterpretation = "")
		{
			CombineAssertions(() =>
			{
				AssertEquals("Bill Status", expectedBillStatus, bill.ABL_BillStatus);

				AssertEquals("Message Sub Type", DeclarationMessageSubTypeList.Codes.AcceptedResponse, responseMessage.EM_MessageSubType);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.Received, responseMessage.EM_Status);
				AssertEquals("Message Interpretation", expectedMessageInterpretation, responseMessage.EM_MessageInterpretation);
			});
		}

		string GetAcceptedMessageWithType0CsvId() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "AcceptedMessageWithType0CsvId.xml");

		string GetAcceptedMessageWithType2CsvId() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "AcceptedMessageWithType2CsvId.xml");

		string GetAcceptedMessageWithType3CsvId() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "AcceptedMessageWithType3CsvId.xml");

		string GetAcceptedMessageWithType4CsvId() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "AcceptedMessageWithType4CsvId.xml");

		protected override string GetRejectedMessageWithOneErrorTypeN() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "RejectedMessageWithOneErrorTypeN.xml");

		protected override string GetRejectedMessageWithOneErrorTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "RejectedMessageWithOneErrorTypeF.xml");

		protected override string GetRejectedMessageWithMultipleErrorsTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "RejectedMessageWithMultipleErrorsTypeF.xml");

		protected override string GetRejectedMessageWithMultipleErrors() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.ReexportH7TestFilePath, "RejectedMessageWithMultipleErrors.xml");

		const string AcceptedBillStatus = "INV";
	}
}

