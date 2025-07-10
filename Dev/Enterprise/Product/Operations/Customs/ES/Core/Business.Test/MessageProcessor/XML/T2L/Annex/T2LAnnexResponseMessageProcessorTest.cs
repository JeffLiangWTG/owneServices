using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LanexosV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LAnnexResponseMessageProcessorTest : T2LCommonResponseMessageProcessorTest<T2LAnnexResponseMessageProcessor, T2LanexosV1Sal>
	{
		public void TestProcessAcceptedNotLastExpeditionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForExpeditionTestFile(), InterchangeID2);

			ProcessMessageForTest(message);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation);
		}

		public void TestProcessAcceptedNotLastReceptionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			SetUpReceptionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForReceptionTestFile(), InterchangeID2);

			ProcessMessageForTest(message);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "AWR");
		}

		public void TestProcessAcceptedLastAndClearedExpeditionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForExpeditionGreenCircuitTestFile(), InterchangeID2);

			ProcessMessageForTest(message);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedLastForExpeditionGreenDeclarationInterpretation, entryStatusCode: EntryStatusCodes.Cleared, movementReferenceNumber: MRNCode, csvClearance: CsvClearance, circuit: CircuitCodeList.Codes.GREEN);
		}

		public void TestProcessAcceptedLastAndRedCircuitExpeditionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForExpeditionRedCircuitTestFile(), InterchangeID2);
			var expectedDeclarationInterpretation = "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento Anexado</td></tr></table>";

			ProcessMessageForTest(message);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: expectedDeclarationInterpretation, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.RED);
		}

		public void TestProcessAcceptedMessageWithPivot()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForExpeditionRedCircuitTestFile(), InterchangeID2);

			ProcessMessageForTest(message);

			AssertT2lAnnexMessage(message, entryHeader, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, circuit: CircuitCodeList.Codes.RED);

			CombineAssertions(() =>
			{
				AssertEquals("Sent message status is changed to received", EDIMessage.Status.Received, sentMessage.EM_Status);
				var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.CusStorageDocPivotEdiMessage);
				pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, message.PK);
				var genPivotResponseMessage = entryHeader.Factory.LoadTop1<GenPivot>(pivotQuery);
				AssertNotNull("Received message is related to sent annex through new GenPivot", genPivotResponseMessage);
			});
		}

		public void TestProcessAcceptedLastAndClearedReceptionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true);
			SetUpReceptionMessage(entryHeader, true);

			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForReceptionClearedTestFile(), InterchangeID2);

			ProcessMessageForTest(message);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedLastForReceptionDeclarationInterpretation, entryStatusCode: EntryStatusCodes.Cleared);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestNotTriggerAnnexSendingForAcceptedAndClearedExpeditionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			SetUpExpeditionMessage(entryHeader);

			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForExpeditionGreenCircuitTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedLastForExpeditionGreenDeclarationInterpretation, entryStatusCode: EntryStatusCodes.Cleared, movementReferenceNumber: MRNCode, csvClearance: CsvClearance, circuit: CircuitCodeList.Codes.GREEN);

			var msg = GetLastMessage(entryHeader);
			AssertNotEquals("Last EDIMessage is not transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTriggerAnnexSendingAllButTheLastAnnexExpeditionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, true);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForExpeditionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);
			Factory.Save();

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "AWR", movementReferenceNumber: MRNCode);

			CombineAssertions(() =>
			{
				var msg = GetLastMessage(entryHeader);
				AssertEquals("Last EDIMessage is transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to false because there are 2 annex documents declared but only one sent", "indicadorFinDeAnexado>N", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				AssertDocPivotMessage(entryHeader, msg);
			});
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTriggerAnnexSendingLastAnnexExpeditionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForExpeditionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);
			Factory.Save();

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "AWR", movementReferenceNumber: MRNCode);

			CombineAssertions(() =>
			{
				var msg = GetLastMessage(entryHeader);
				AssertEquals("Last EDIMessage is transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to true", "indicadorFinDeAnexado>S", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				AssertDocPivotMessage(entryHeader, msg);
			});
		}

		[TestDateIncremental(seconds: 1)]
		public void TestNotTriggerAnnexSendingForAcceptedAndClearedReceptionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true);

			SetUpReceptionMessage(entryHeader, true);

			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForReceptionClearedTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedLastForReceptionDeclarationInterpretation, entryStatusCode: EntryStatusCodes.Cleared);

			var msg = GetLastMessage(entryHeader);
			AssertNotEquals("Last EDIMessage is not transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTriggerAnnexSendingAllButTheLastAnnexReceptionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, false);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			SetUpReceptionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForReceptionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);
			Factory.Save();

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "AWR", movementReferenceNumber: MRNCode);

			CombineAssertions(() =>
			{
				var msg = GetLastMessage(entryHeader);
				AssertEquals("Last EDIMessage is transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to false because there are 2 annex documents declared but only one sent", "indicadorFinDeAnexado>N", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				AssertDocPivotMessage(entryHeader, msg);
			});
		}

		[TestDateIncremental(seconds: 1)]
		public void TestTriggerAnnexSendingLastAnnexReceptionMessage()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			SetUpReceptionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForReceptionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);
			Factory.Save();

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "AWR", movementReferenceNumber: MRNCode);

			CombineAssertions(() =>
			{
				var msg = GetLastMessage(entryHeader);
				AssertEquals("Last EDIMessage is transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
				AssertEquals("New EDIMessage is type T2A", DeclarationMessageTypeList.Codes.T2lAnnex, msg.EM_MessageType);
				AssertContains("New EDIMessage has last annex flag to true", "indicadorFinDeAnexado>S", msg.EM_MessageText);
				AssertContains("New EDIMessage has mrn tag", "numeroDeReferenciaDelT2L", msg.EM_MessageText);

				AssertDocPivotMessage(entryHeader, msg);
			});
		}

		[TestDateIncremental(seconds: 1)]
		public void TestProcessAcceptedNotLastExpeditionMessageNotLastResponseCHStatusEmpty()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, true);
			AddAnnexSentMessage(entryHeader);
			SetUpExpeditionMessage(entryHeader);

			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForExpeditionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: ZString.Empty);

			var msg = GetLastMessage(entryHeader);
			AssertEquals("Last EDIMessage is receive", EDIMessage.Direction.Receive, msg.EM_ReceiveTransmit);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestProcessAcceptedNotLastReceptionMessageNotLastResponseCHStatusEmpty()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, true);
			AddAnnexSentMessage(entryHeader);
			SetUpReceptionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForReceptionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: ZString.Empty);

			var msg = GetLastMessage(entryHeader);
			AssertEquals("Last EDIMessage is receive", EDIMessage.Direction.Receive, msg.EM_ReceiveTransmit);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestProcessAcceptedNotLastExpeditionMessageLastResponseCHStatusRejected()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, true);
			AddAnnexSentMessage(entryHeader);
			entryHeader.CH_Status = "REJ";
			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForExpeditionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "REJ");

			var msg = GetLastMessage(entryHeader);
			AssertEquals("Last EDIMessage is receive", EDIMessage.Direction.Receive, msg.EM_ReceiveTransmit);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestProcessAcceptedNotLastReceptionMessageLastResponseCHStatusRejected()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, true);
			AddAnnexSentMessage(entryHeader);
			entryHeader.CH_Status = "REJ";
			SetUpReceptionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForReceptionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "REJ");

			var msg = GetLastMessage(entryHeader);
			AssertEquals("Last EDIMessage is receive", EDIMessage.Direction.Receive, msg.EM_ReceiveTransmit);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestProcessAcceptedNotLastExpeditionMessageLastResponseCHStatusFailed()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, true);
			AddAnnexSentMessage(entryHeader);
			entryHeader.CH_Status = "FAL";
			SetUpExpeditionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForExpeditionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "FAL");

			var msg = GetLastMessage(entryHeader);
			AssertEquals("Last EDIMessage is receive", EDIMessage.Direction.Receive, msg.EM_ReceiveTransmit);
		}

		[TestDateIncremental(seconds: 1)]
		public void TestProcessAcceptedNotLastReceptionMessageLastResponseCHStatusFailed()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, true, true);
			AddAnnexSentMessage(entryHeader);
			entryHeader.CH_Status = "FAL";
			SetUpReceptionMessage(entryHeader);
			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedNotLastForReceptionTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedNotLastDeclarationInterpretation, chStatus: "FAL");

			var msg = GetLastMessage(entryHeader);
			AssertEquals("Last EDIMessage is receive", EDIMessage.Direction.Receive, msg.EM_ReceiveTransmit);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			SetUpExpeditionMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForExpeditionGreenCircuitTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedLastForExpeditionGreenDeclarationInterpretation, entryStatusCode: EntryStatusCodes.Cleared, movementReferenceNumber: MRNCode, csvClearance: CsvClearance, circuit: CircuitCodeList.Codes.GREEN);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_T2L_CLR.pdf", CsvClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var entryHeader = CreateT2LAnnexDeclaration(ApplicationReference2, InterchangeID2, false);
			SetUpExpeditionMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter(MRNCode);

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var message = CreateNewEDIMessage(ApplicationReference2, GetAcceptedLastForExpeditionGreenCircuitTestFile(), InterchangeID2);

			ProcessMessageForTest(message, entryHeader.Messages);

			AssertT2lAnnexMessage(message, entryHeader, expectedMessageInterpretation: acceptedLastForExpeditionGreenDeclarationInterpretation, entryStatusCode: EntryStatusCodes.Cleared, movementReferenceNumber: MRNCode, csvClearance: CsvClearance, circuit: CircuitCodeList.Codes.GREEN);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void AssertT2lAnnexMessage(TestEdiMessage message, CusEntryHeader entryHeader, string expectedMessageInterpretation = "", string entryStatusCode = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string circuit = "", string csvClearance = "", string movementReferenceNumber = "", string chStatus = "RCV")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: "ACC", chStatus: chStatus, entryStatusCode: entryStatusCode, circuit: circuit, csvClearance: csvClearance, movementReferenceNumber: movementReferenceNumber);
		}

		string GetAcceptedNotLastForExpeditionTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexTestFilePath, "AcceptedNotLastForExpeditionMessage.txt");
		string GetAcceptedNotLastForReceptionTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexTestFilePath, "AcceptedNotLastForReceptionMessage.txt");
		string GetAcceptedLastForExpeditionGreenCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexTestFilePath, "AcceptedLastForExpeditionGreenCircuitMessage.txt");
		string GetAcceptedLastForExpeditionRedCircuitTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexTestFilePath, "AcceptedLastForExpeditionRedCircuitMessage.txt");
		string GetAcceptedLastForReceptionClearedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexTestFilePath, "AcceptedLastForReceptionClearedMessage.txt");

		protected override string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexTestFilePath, "RejectedMessage.txt");

		protected override T2LAnnexResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new T2LAnnexResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L Annex Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lAnnex };

		protected override ZString RejectedMRN => ZString.Empty;

		protected override ZString RejectedMessageStatus => EDIMessage.Status.Rejected;

		protected override ZString RejectedMessageInterpretation => "<H3>Rejected Declaration</H3>" +
						"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
						"<tr><td><strong>Error</strong></td><td><strong>Description</strong></td></tr>" +
						"<tr><td>7002</td><td>El fichero que se anexa no tiene una extensión válida según se indica en tabla REGDFORM.</td></tr>" +
						"</table>";

		readonly ZString acceptedNotLastDeclarationInterpretation = "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
			"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento Anexado</td></tr></table>";

		readonly ZString acceptedLastForExpeditionGreenDeclarationInterpretation = "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
					"<table border=\"0\"><tr><td>C.S.V.:</td><td>&nbsp;&nbsp;</td><td>" + CsvClearance + "</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento Anexado</td></tr></table>";

		readonly ZString acceptedLastForReceptionDeclarationInterpretation = "<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = " + CsvElectronicDeclaration + "</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>07-05-2020, 14:50:32</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento Anexado</td></tr></table>";

		protected override ZString OriginalEntryStatus => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;

		void SetUpExpeditionMessage(CusEntryHeader entryHeader)
		{
			var messageExpedition = Factory.New<TestEdiMessage>();
			messageExpedition.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			messageExpedition.EM_MessageType = DeclarationMessageTypeList.Codes.T2lExpedition;
			messageExpedition.EM_Status = EDIMessage.Status.Received;
			entryHeader.Messages.Add(messageExpedition);

			var docPivot1 = entryHeader.EDocPivotCollection[0];
			var messagePivot = Factory.New<GenPivot>();
			messagePivot.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot.XX_Relation1ID = docPivot1.PK;
			messagePivot.XX_Relation1TableCode = docPivot1.TablePrefix;
			messagePivot.XX_Relation2ID = sentMessage.PK;
			messagePivot.XX_Relation2TableCode = sentMessage.TablePrefix;

			Factory.Save();
		}

		void SetUpReceptionMessage(CusEntryHeader entryHeader, bool isTestForLastAnnexCleared = false)
		{
			var messageReception = Factory.New<TestEdiMessage>();
			messageReception.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			messageReception.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReception;
			messageReception.EM_Status = EDIMessage.Status.Received;
			entryHeader.Messages.Add(messageReception);

			if (isTestForLastAnnexCleared)
			{
				var docPivot1 = entryHeader.EDocPivotCollection[0];
				var messagePivot1 = Factory.New<GenPivot>();
				messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot1.XX_Relation1ID = docPivot1.PK;
				messagePivot1.XX_Relation1TableCode = docPivot1.TablePrefix;
				messagePivot1.XX_Relation2ID = sentMessage.PK;
				messagePivot1.XX_Relation2TableCode = sentMessage.TablePrefix;

				var messageAnnex = Factory.New<TestEdiMessage>();
				messageAnnex.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
				messageAnnex.EM_MessageType = DeclarationMessageTypeList.Codes.T2lAnnex;
				messageAnnex.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				messageAnnex.EM_Status = EDIMessage.Status.Received;
				messageAnnex.EM_MessageSubType = DeclarationMessageSubTypeList.Codes.AcceptedResponse;
				entryHeader.Messages.Add(messageAnnex);

				var docPivot2 = entryHeader.EDocPivotCollection[1];
				var messagePivot2 = Factory.New<GenPivot>();
				messagePivot2.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
				messagePivot2.XX_Relation1ID = docPivot2.PK;
				messagePivot2.XX_Relation1TableCode = docPivot2.TablePrefix;
				messagePivot2.XX_Relation2ID = messageAnnex.PK;
				messagePivot2.XX_Relation2TableCode = messageAnnex.TablePrefix;
			}

			Factory.Save();
		}

		void AddAnnexSentMessage(CusEntryHeader entryHeader)
		{
			var messageAnnexSent = Factory.New<TestEdiMessage>();
			messageAnnexSent.EM_MessageNum = SentMessageNumber;
			messageAnnexSent.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			messageAnnexSent.EM_MessageType = DeclarationMessageTypeList.Codes.T2lAnnex;
			messageAnnexSent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageAnnexSent.EM_Status = EDIMessage.Status.Sent;
			entryHeader.Messages.Add(messageAnnexSent);

			var docPivot1 = entryHeader.EDocPivotCollection[0];
			var messagePivot1 = Factory.New<GenPivot>();
			messagePivot1.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot1.XX_Relation1ID = docPivot1.PK;
			messagePivot1.XX_Relation1TableCode = docPivot1.TablePrefix;
			messagePivot1.XX_Relation2ID = messageAnnexSent.PK;
			messagePivot1.XX_Relation2TableCode = messageAnnexSent.TablePrefix;

			var messageAnnexSent2 = Factory.New<TestEdiMessage>();
			messageAnnexSent2.EM_MessageNum = SentMessageNumber;
			messageAnnexSent2.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			messageAnnexSent2.EM_MessageType = DeclarationMessageTypeList.Codes.T2lAnnex;
			messageAnnexSent2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageAnnexSent2.EM_Status = EDIMessage.Status.Sent;
			entryHeader.Messages.Add(messageAnnexSent2);

			var docPivot2 = entryHeader.EDocPivotCollection[1];
			var messagePivot2 = Factory.New<GenPivot>();
			messagePivot2.XX_RelationType = GenPivotTypes.CusStorageDocPivotEdiMessage;
			messagePivot2.XX_Relation1ID = docPivot2.PK;
			messagePivot2.XX_Relation1TableCode = docPivot2.TablePrefix;
			messagePivot2.XX_Relation2ID = messageAnnexSent2.PK;
			messagePivot2.XX_Relation2TableCode = messageAnnexSent2.TablePrefix;

			Factory.Save();
		}

		EDIMessage GetLastMessage(CusEntryHeader entryHeader)
		{
			ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
			var messages = Factory.Load<EDIMessage>(messagesQuery).OrderByDescending(x => x.EM_SystemCreateTimeUtc);
			return messages.First();
		}

		void AssertDocPivotMessage(CusEntryHeader entryHeader, EDIMessage msg)
		{
			entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().ForEach(x => x.Messages.Reload(true));
			var msgPivot = entryHeader.EDocPivotCollection.Cast<CusStorageDocPivot>().FirstOrDefault(x => x.MessageStatus == "QUE").Message;
			AssertEquals("New EDIMessage is associated to the sent annex", msgPivot, msg);
		}

		protected override EDIInterchange CreateTestResponseInterchange(ZGuid interchangeID, ZString interchangeTransportType)
		{
			var interchange = base.CreateTestResponseInterchange(interchangeID, interchangeTransportType);
			interchange.EI_InterchangeType = DeclarationMessageTypeList.Codes.T2lAnnex;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			if (interchangeTransportType != EDIInterchange.TransportType.xT)
			{
				interchange.EI_HeaderText = ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>{0}</SentEDIMessageNumber>
</Headers>", SentMessageNumber);
			}
			return interchange;
		}
	}
}
