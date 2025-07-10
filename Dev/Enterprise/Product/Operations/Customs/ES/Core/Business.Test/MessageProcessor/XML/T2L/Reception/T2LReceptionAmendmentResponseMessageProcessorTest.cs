using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionModificaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LReceptionAmendmentResponseMessageProcessorTest : T2LCommonResponseMessageProcessorTest<T2LReceptionAmendmentResponseMessageProcessor, T2LrecepcionModificaV1Sal>
	{
		[TestDate(2020, 05, 10, 14, 50, 32)]
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lReceptionAmendmentMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation);
		}

		public void TestProcessAcceptedMessageWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCodeExisting, AcceptanceDateExisting);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lReceptionAmendmentMessage(message, entryHeader, movementReferenceNumber: MRNCodeExisting, movementReferenceNumberIssueDate: AcceptanceDateExisting);
		}

		void AssertT2lReceptionAmendmentMessage(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", ZDateTime? movementReferenceNumberIssueDate = null, string movementReferenceNumber = MRNCode)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", chStatus: "RCV", entryStatusCode: "INI", movementReferenceNumber: movementReferenceNumber, acceptanceDate: movementReferenceNumberIssueDate);
		}

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionAmendmentTestFilePath, "AcceptedMessage.txt");
		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionAmendmentTestFilePath, "RejectedMessage.txt");

		protected override T2LReceptionAmendmentResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new T2LReceptionAmendmentResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L Reception Amendment Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lReceptionAmendment };

		protected override ZString RejectedMRN => "20ES00999930006184";
		protected override ZString RejectedMessageInterpretation => "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>3004</td><td>Ya existe un documento T2L con el nº de referencia indicado.</td></tr>" +
						"</table>";

		ZString AcceptedAndClearedMessageInterpretation => "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento de Alta Indirecta de T2L modificado.</td></tr></table>";
	}
}

