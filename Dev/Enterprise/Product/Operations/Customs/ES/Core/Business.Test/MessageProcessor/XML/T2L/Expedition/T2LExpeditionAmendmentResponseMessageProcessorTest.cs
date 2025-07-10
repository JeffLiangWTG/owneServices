using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionModificaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LExpeditionAmendmentResponseMessageProcessorTest : T2LCommonResponseMessageProcessorTest<T2LExpeditionAmendmentResponseMessageProcessor, T2LexpedicionModificaV1Sal>
	{
		[TestDate(2020, 05, 10, 14, 50, 32)]
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			var acceptedNotLastDeclarationInterpretation =
				"<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = 74GR23EW2QHWY955</H3>" +
				"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
				"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento T2L de Expedición modificado.</td></tr></table>";

			AssertT2lExpeditionAmendmentMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation);
		}

		public void TestProcessAcceptedMessageWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCodeExisting, AcceptanceDateExisting);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lExpeditionAmendmentMessage(message, entryHeader, movementReferenceNumber: MRNCodeExisting, movementReferenceNumberIssueDate: AcceptanceDateExisting);
		}

		public void TestProcessAcceptedAndClearedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedAndClearedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lExpeditionAmendmentMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, csvClearance: CsvClearance);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedAndClearedTestFile(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lExpeditionAmendmentMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, csvClearance: CsvClearance);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_T2L_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			entryHeader.SetCSVClearanceNum(CsvClearance);

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedAndClearedTestFile(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lExpeditionAmendmentMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, csvClearance: CsvClearance);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NewCSVClearance()
		{
			entryHeader.SetCSVClearanceNum("AAAAAAAAAAAAAAAA");

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedAndClearedTestFile(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lExpeditionAmendmentMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, csvClearance: CsvClearance);

			CombineAssertions(() =>
			{
				docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
				var eDocs = docManagerInfo.GetRelatedEDocs();
				AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

				AssertEquals("eDocs contains one document with the correct modified name", MRNCode + "_E_AEAT_T2L_CLR_OLD_AAAAAAAAAAAAAAAA.pdf", eDocs.First().FileName);

				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_T2L_CLR.pdf", CsvClearance) });
			});
		}

		void AssertT2lExpeditionAmendmentMessage(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string circuit = "", string csvClearance = "", string chStatus = "RCV", ZDateTime? movementReferenceNumberIssueDate = null, string movementReferenceNumber = MRNCode)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", chStatus: chStatus, entryStatusCode: "INI", circuit: circuit, acceptanceDate: movementReferenceNumberIssueDate, csvClearance: csvClearance, movementReferenceNumber: movementReferenceNumber);
		}

		string GetAcceptedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionAmendmentTestFilePath, "AcceptedMessage.txt");
		string GetAcceptedAndClearedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionAmendmentTestFilePath, "AcceptedAndClearedMessage.txt");
		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionAmendmentTestFilePath, "RejectedMessage.txt");

		protected override T2LExpeditionAmendmentResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new T2LExpeditionAmendmentResponseMessageProcessor(logger, new BranchCustomsMessageProcessorForTest());

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L Expedition Amendment Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lExpeditionAmendment };

		protected override ZString RejectedMRN => ZString.Empty;

		protected override ZString RejectedMessageInterpretation => "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>1095</td><td>El código de país de destino es incorrecto o no es un país de la U.E.</td></tr>" +
						"</table>";

		ZString AcceptedAndClearedMessageInterpretation => "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>C.S.V.:</td><td>&nbsp;&nbsp;</td><td>" + CsvClearance + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento T2L de Expedición modificado.</td></tr></table>";
	}
}

