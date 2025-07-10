using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionV2Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LExpeditionResponseMessageProcessorTest : T2LCommonResponseMessageProcessorTest<T2LExpeditionResponseMessageProcessor, T2LexpedicionV2Sal>
	{
		[TestDate(2020, 05, 10, 14, 50, 32)]
		public void TestProcessAcceptedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lExpeditionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedMessageInterpretation, acceptanceDate: AcceptanceDate);
		}

		public void TestProcessAcceptedMessageWithExistingMRN()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCodeExisting, MovementReferenceNumberIssueDateExisting);
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lExpeditionMessage(message, entryHeader, movementReferenceNumber: MRNCodeExisting, acceptanceDate: AcceptanceDate);
		}

		public void TestProcessAcceptedMessageNotValidDate()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedTestFileNotValidDate(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lExpeditionMessage(message, entryHeader, acceptanceDate: ZDateTime.Empty, messageNum: "AA200428180533305039");
		}

		public void TestProcessAcceptedAndClearedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedAndClearedTestFile(), InterchangeID);

			ProcessMessageForTest(message);

			AssertT2lExpeditionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN, csvClearance: CsvClearance, acceptanceDate: AcceptanceDate);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestNotTriggerAnnexSendingForAcceptedAndClearedMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedAndClearedTestFile(), InterchangeID2);

			ProcessMessageForTest(message);

			AssertT2lExpeditionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN, csvClearance: CsvClearance, acceptanceDate: AcceptanceDate);

			CombineAssertions(() =>
			{
				ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
				var messages = Factory.Load<EDIMessage>(messagesQuery).OrderByDescending(x => x.EM_SystemCreateTimeUtc);
				var msg = messages.First();
				AssertNotEquals("Last sent EDIMessage is not type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertEquals("Only 1 T2A EDIMessage in entryHeader.Messages", 1, messages.Count(x => x.EM_MessageType == DeclarationMessageTypeList.Codes.T2lAnnex));
			});
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTriggerAnnexSendingAllButTheLastAnnexMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lExpeditionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedMessageInterpretation, chStatus: "AWR", acceptanceDate: AcceptanceDate);

			CombineAssertions(() =>
			{
				var msg = entryHeader.Messages.LastMessage;
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to false because there are 2 annex documents declared but only one sent", "indicadorFinDeAnexado>N", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ForEach(x => x.Messages.Reload(true));
				var msgPivot = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault(x => x.Message != null).Message;
				AssertEquals("New EDIMessage is associated to the sent annex", msgPivot, msg);
			});
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTriggerAnnexSendingLastAnnexMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lExpeditionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedMessageInterpretation, chStatus: "AWR", acceptanceDate: AcceptanceDate);

			CombineAssertions(() =>
			{
				var msg = entryHeader.Messages.LastMessage;
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to true", "indicadorFinDeAnexado>S", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ForEach(x => x.Messages.Reload(true));
				var msgPivot = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault(x => x.Message != null).Message;
				AssertEquals("New EDIMessage is associated to the sent annex", msgPivot, msg);
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedAndClearedTestFile(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lExpeditionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN, csvClearance: CsvClearance, acceptanceDate: AcceptanceDate);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_T2L_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptedAndClearedTestFile(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lExpeditionMessage(message, entryHeader, expectedMessageInterpretation: AcceptedAndClearedMessageInterpretation, entryStatusCode: EntryStatusCodes.Cleared, circuit: CircuitCodeList.Codes.GREEN, csvClearance: CsvClearance, acceptanceDate: AcceptanceDate);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void AssertT2lExpeditionMessage(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = "", string csvClearance = "", string chStatus = "RCV", string movementReferenceNumber = MRNCode, ZDateTime? acceptanceDate = null, string messageNum = MessageNum)
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: messageNum, messageSubType: "ACC", chStatus: chStatus, entryStatusCode: entryStatusCode, circuit: circuit, csvClearance: csvClearance, movementReferenceNumber: movementReferenceNumber, acceptanceDate: acceptanceDate);
		}

		string GetAcceptedTestFileNotValidDate() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionTestFilePath, "InvalidAcceptedMessage.txt");
		string GetAcceptedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionTestFilePath, "AcceptedMessage.txt");
		string GetAcceptedAndClearedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionTestFilePath, "AcceptedAndClearedMessage.txt");

		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionTestFilePath, "RejectedMessage.txt");

		protected override T2LExpeditionResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new T2LExpeditionResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L Expedition Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lExpedition };

		protected override ZString RejectedMRN => ZString.Empty;
		protected readonly ZDateTime MovementReferenceNumberIssueDateExisting = new ZDateTime(2020, 05, 10);

		protected override ZString RejectedMessageInterpretation => "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>1095</td><td>El código de país de destino es incorrecto o no es un país de la U.E.</td></tr>" +
						"</table>";

		ZString AcceptedMessageInterpretation => "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento T2L de Expedición admitido.</td></tr></table>";

		ZString AcceptedAndClearedMessageInterpretation => "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<table border=\"0\"><tr><td>MRN:</td><td>&nbsp;&nbsp;</td><td>" + MRNCode + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
					"<table border=\"0\"><tr><td>C.S.V.:</td><td>&nbsp;&nbsp;</td><td>" + CsvClearance + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento T2L de Expedición admitido.</td></tr></table>";
	}
}
