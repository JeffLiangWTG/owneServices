using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AnulaPreH7V1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class CancellationH7ResponseMessageProcessorTest : H7CommonResponseMessageProcessorTest<CancellationH7ResponseMessageProcessor, CancellationH7MessagePrettyFormatter, AnulaPreH7V1Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			SetupTestData();

			var expectedMessageInterpretationText = "<H3>Accepted Cancellation</H3>" +
				"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>1 H7 Canceled</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Message ID:</td><td>&nbsp;&nbsp;</td><td>20250117170000000000</td></tr></table>" +
				"<table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>17-01-2025, 17:00:00</td></tr></table>" +
				"<table border=\"0\"><tr><td>CSV ID:</td><td>&nbsp;&nbsp;</td><td>XIV2468101233333</td></tr></table>";

			var message = CreateNewEDIMessage(ApplicationReference, AcceptedMessage, InterchangeID, interchangeTransportType: EDIInterchangeTransportTypeList.Codes.xT);
			ProcessMessageForTest(message);

			CombineAssertions("Expected Process Result", () =>
			{
				AssertEquals(expectedMessageInterpretationText, message.EM_MessageInterpretation);
				AssertEquals(LogicalStatusList.Codes.Accepted, bill.ABL_MessageStatus);
				AssertEquals(AISEntryStatusList.Codes.Cancelled, bill.ABL_BillStatus);
			});
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "H7 Cancellation Response Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.H7Cancellation };

		protected override CancellationH7ResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new(logger);

		string AcceptedMessage => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.CancellationH7TestFilePath, "AcceptedMessage.xml");

		protected override string GetRejectedMessageWithMultipleErrors() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.CancellationH7TestFilePath, "RejectedMessageWithMultipleErrors.xml");

		protected override string GetRejectedMessageWithOneErrorTypeN() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.CancellationH7TestFilePath, "RejectedMessageWithOneErrorTypeN.xml");

		protected override string GetRejectedMessageWithOneErrorTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.CancellationH7TestFilePath, "RejectedMessageWithOneErrorTypeF.xml");

		protected override string GetRejectedMessageWithMultipleErrorsTypeF() => ESH7TestFileReader.GetEmbeddedFileText(H7MessageProcessorTestFileConstants.CancellationH7TestFilePath, "RejectedMessageWithMultipleErrorsTypeF.xml");
	}
}
