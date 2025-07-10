using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LdatadoV2Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LClearanceResponseMessageProcessorTest : T2LCommonResponseMessageProcessorTest<T2LClearanceResponseMessageProcessor, T2LdatadoV2Sal>
	{
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lClearanceMessage(message, entryHeader);
		}

		public void TestProcessAcceptedMessageWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCodeExisting, AcceptanceDateExisting);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lClearanceMessage(message, entryHeader, movementReferenceNumber: MRNCodeExisting, movementReferenceNumberIssueDate: AcceptanceDateExisting);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lClearanceMessage(message, entryHeader);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MrnCodeT2C + "_I_AEAT_T2L_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MrnCodeT2C + "_I_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFile(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lClearanceMessage(message, entryHeader);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void AssertT2lClearanceMessage(TestEdiMessage message, CusEntryHeader entryHeader, ZDateTime? movementReferenceNumberIssueDate = null, string movementReferenceNumber = "")
		{
			AssertEquals("T2CMovementReferenceNumber", MrnCodeT2C, entryHeader.T2CMovementReferenceNumber);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", chStatus: "RCV", entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN, acceptanceDate: movementReferenceNumberIssueDate, csvClearance: CsvClearance, movementReferenceNumber: movementReferenceNumber);
		}

		const string MrnCodeT2C = "21ES009999M0000097";

		string GetAcceptanceTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ClearanceTestFilePath, "t2l_clearance_accepted.txt");
		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ClearanceTestFilePath, "t2l_clearance_rejected.txt");

		protected override T2LClearanceResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new T2LClearanceResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L Clearance Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lClearance };

		protected override ZString RejectedMRN => ZString.Empty;

		protected override ZString RejectedMessageInterpretation => "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>1181</td><td>Ausencia del Código de Ubicación de la Sumaria.</td></tr>" +
						"</table>";
		ZString AcceptedAndClearedMessageInterpretation => "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = 42JYQPT9XCWSBVHH</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>" + MrnCodeT2C + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
					"<table border=\"0\"><tr><td>C.S.V.:</td><td>&nbsp;&nbsp;</td><td>" + CsvClearance + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento JEC de Datado Admitido</td></tr></table>";
	}
}
