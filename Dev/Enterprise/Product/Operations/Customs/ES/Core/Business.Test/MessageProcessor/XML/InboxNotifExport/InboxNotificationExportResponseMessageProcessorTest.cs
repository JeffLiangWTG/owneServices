using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Export.NotifPreDUAV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class InboxNotificationExportResponseMessageProcessorTest : XMLResponseMessageProcessorTest<InboxNotificationExportResponseMessageProcessor, IMessagePrettyFormatter, NotifPreDuav1Sal>
	{
		public void TestProcessAdmittedNoClearanceMessage()
		{
			AddMessageProcessAndAssertResult_NoClearance();
		}

		public void TestProcessAdmittedClearedWithPendingMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileClearedWithPendingMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H4>Transaction Date = " + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</H4><br>";
			CommonInboxNotificactionExportResponse(message, entryStatusCode: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, expectedMessageInterpretationText: expectedMessageInterpretationText, circuit: CircuitCodeList.Codes.ORANGE, clearanceResult: ClearanceResultCodeList.Codes.A2, eadPrintProcedure: EADPrintProcedureCodeList.Codes._0NoEADPrint, acceptanceDate: acceptanceDate, entryReleaseDate: clearanceDate, csvClarance: CSVClearance);
		}

		public void TestProcessAdmittedClearedNotPendingMessage()
		{
			instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileClearedNotPendingMessage(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H4>Transaction Date = " + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</H4><br>";
			CommonInboxNotificactionExportResponse(message, entryStatusCode: EntryStatusCodes.Cleared, expectedMessageInterpretationText: expectedMessageInterpretationText, circuit: CircuitCodeList.Codes.GREEN, clearanceResult: ClearanceResultCodeList.Codes.A2, eadPrintProcedure: EADPrintProcedureCodeList.Codes._2EADCanBePrintedByDeclarant, acceptanceDate: acceptanceDate, entryReleaseDate: clearanceDate, csvClarance: CSVClearance, circuitCan: CircuitCodeList.Codes.GREEN, csvT2l: CSVT2L);
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_AllDocs()
		{
			declaration.ZG_CTStatusID = "T2LF";
			instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileClearedNotPendingMessage(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);
			var expectedMessageInterpretationText = "<H4>Transaction Date = " + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</H4><br>";
			CommonInboxNotificactionExportResponse(message, entryStatusCode: EntryStatusCodes.Cleared, expectedMessageInterpretationText: expectedMessageInterpretationText, circuit: CircuitCodeList.Codes.GREEN, clearanceResult: ClearanceResultCodeList.Codes.A2, eadPrintProcedure: EADPrintProcedureCodeList.Codes._2EADCanBePrintedByDeclarant, acceptanceDate: acceptanceDate, entryReleaseDate: clearanceDate, csvClarance: CSVClearance, circuitCan: CircuitCodeList.Codes.GREEN, csvT2l: CSVT2L);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 3, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCodeClearedNotPending + "_E_AEAT_CLR.pdf", "2ZZHTTTULQQDJSQ9"),
																													(MRNCodeClearedNotPending + "_E_AEAT_ead.pdf", MRNCodeClearedNotPending),
																													(MRNCodeClearedNotPending + "_E_AEAT_t2lf.pdf", CSVT2L) });
			});
		}

		public void TestCreateDocumentCaptureRequestEDIMessageWhenCSVClearance_Export_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_E_AEAT_ead.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeClearedNotPending + "_E_AEAT_t2lf.pdf", "CAU");
			docManagerInfo.Save();

			declaration.ZG_CTStatusID = "T2LF";
			
			instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileClearedNotPendingMessage(), InterchangeID);

			ProcessMessageForTest(message, entryHeader.Messages);
			var expectedMessageInterpretationText = "<H4>Transaction Date = " + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</H4><br>";
			CommonInboxNotificactionExportResponse(message, entryStatusCode: EntryStatusCodes.Cleared, expectedMessageInterpretationText: expectedMessageInterpretationText, circuit: CircuitCodeList.Codes.GREEN, clearanceResult: ClearanceResultCodeList.Codes.A2, eadPrintProcedure: EADPrintProcedureCodeList.Codes._2EADCanBePrintedByDeclarant, acceptanceDate: acceptanceDate, entryReleaseDate: clearanceDate, csvClarance: CSVClearance, circuitCan: CircuitCodeList.Codes.GREEN, csvT2l: CSVT2L);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		public void TestProcessRejectedMessage()
		{
			AddMessageProcessAndAssertResult_RejectedMessage();
		}

		public void TestProcessCancelledMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetCancelledTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H4>Transaction Date = 12-02-2020</H4><br>" +
					"<H3>Pre-SAD cancellation notification</H3><H3>Date: 12-02-2020</H3><H3>Motive: Anulacion Pre-DUA-Exportacion por caducidad</H3>";
			CommonInboxNotificactionExportResponse(message, entryStatusCode: EntryStatusCodes.Cancelled, expectedMessageInterpretationText: expectedMessageInterpretationText, messageSubType: "REJ", acceptanceDate: movementReferenceNumberIssueDate);
		}

		public void TestProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult_NoClearance);
		}

		public void TestProcessRejectedMessageAndRemoveCusPollingTransactionsWhenxT()
		{
			AssertProcessAcceptedMessageAndRemoveCusPollingTransactionsWhenxT(MRNCodeClearedNotPending, AddMessageProcessAndAssertResult_RejectedMessage);
		}

		void AddMessageProcessAndAssertResult_NoClearance()
		{
			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending, movementReferenceNumberIssueDate);

			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetAcceptanceTestFileNoClearance(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H4>Transaction Date = " + acceptanceDate.ToCustomsFormatDateStringddMMyyyyWithDash() + "</H4><br>";
			CommonInboxNotificactionExportResponse(message, entryStatusCode: EntryStatusCodes.CustomsDeclarationAccepted, expectedMessageInterpretationText: expectedMessageInterpretationText, circuit: CircuitCodeList.Codes.RED, clearanceResult: ClearanceResultCodeList.Codes.A1, eadPrintProcedure: EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities, acceptanceDate: acceptanceDate);
		}

		void AddMessageProcessAndAssertResult_RejectedMessage()
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);

			ProcessMessageForTest(message);
			var expectedMessageInterpretationText = "<H4>Transaction Date = 02-04-2020</H4><br>" +
					"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td><strong>Error</strong></td><td><strong>Location/Description</strong></td></tr>" +
					"<tr><td>14992</td><td>CABECERA.EXPORTADOR/EXPEDIDOR CABECERA (CAS 2). NIF NO IDENTIFICADO CON LOS PARAMETROS DE CONSULTA ENVIADOS.</td></tr>" +
					"<tr><td>25922</td><td>CABECERA.AUTORIZACION DESPACHO/TIPO AUT.(CAS14). AUTDESP NIU DEL EXPORTADOR SIN CUMPLIMENTAR. -VALOR 0-</td></tr></table>";
			CommonInboxNotificactionExportResponse(message, entryStatusCode: EntryStatusCodes.PreDeclarationAccepted, expectedMessageInterpretationText: expectedMessageInterpretationText, messageSubType: "REJ", acceptanceDate: movementReferenceNumberIssueDate);
		}

		protected void CommonInboxNotificactionExportResponse(TestEdiMessage message, string entryStatusCode = "", string messageSubType = "ACC", string expectedMessageInterpretationText = "", string circuit = "", string clearanceResult = "", string eadPrintProcedure = "", ZDateTime? entryReleaseDate = null, ZDateTime? acceptanceDate = null, string csvClarance = "", string csvT2l = "", string circuitCan = "")
		{
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretationText, messageSubType: messageSubType, entryStatusCode: entryStatusCode, circuit: circuit, clearanceResult: clearanceResult, eadPrintProcedure: eadPrintProcedure, acceptanceDate: acceptanceDate, movementReferenceNumber: MRNCodeClearedNotPending, csvClearance: csvClarance, entryReleaseDate: entryReleaseDate, csvT2L: csvT2l, messageNum: MessageNum, circuitCan: circuitCan);
		}

		protected override void SetUp()
		{
			base.SetUp();

			instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.B;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending, movementReferenceNumberIssueDate);

			SetSentInterchange(entryHeader, InterchangeID);

			logger = new LoggingInformation();
			processor = new InboxNotificationExportResponseMessageProcessor(logger);

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDA", "Rejected", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Cancelled", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "Cleared with Pending Complementary Declarations", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration Accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		CusEntryInstruction instruction;

		string GetAcceptanceTestFileNoClearance() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExportTestFilePath, "AcceptedNoClearanceMessage.txt");
		string GetAcceptanceTestFileClearedWithPendingMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExportTestFilePath, "AcceptedClearedWithPendingMessage.txt");
		string GetAcceptanceTestFileClearedNotPendingMessage() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExportTestFilePath, "AcceptedClearedNotPendingMessage.txt");
		string GetRejectedTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExportTestFilePath, "RejectedMessage.txt");
		string GetCancelledTestFile() => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExportTestFilePath, "CancelledMessage.txt");

		protected override string WrongXMLTestFile => ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExportTestFilePath, "WrongXML.txt");
		protected override string GetAcceptanceTestFileWithLongSegmentId() => string.Empty;

		const string MRNCodeClearedNotPending = "19ES00999910002776";
		const string MessageNum = "20190522140149757587";
		const string CSVClearance = "2ZZHTTTULQQDJSQ9";
		const string CSVT2L = "74GR23EW2QHWY955";
		readonly ZDateTime acceptanceDate = new ZDateTime(2019, 10, 22);
		readonly ZDateTime clearanceDate = new ZDateTime(2019, 05, 22, 14, 01, 00);
		readonly ZDateTime movementReferenceNumberIssueDate = new ZDateTime(2021, 01, 15, 05, 40, 55);

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCodeClearedNotPending, movementReferenceNumberIssueDate);

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFileNoClearance();

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override TestEdiMessage SetDataForIncorrectApplicationReferencePreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();

			var interchangeID = ZGuid.NewZGuid();

			SetSentInterchange(entryHeader, interchangeID);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = interchangeTransportType;

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = "TEST";
			message.EM_MessageText = GetAcceptanceTestFileNoClearance();

			responseInterchange.ContainedMessages.Add(message);
			return message;
		}

		protected override InboxNotificationExportResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new InboxNotificationExportResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Export inbox Notification Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForExport };

		protected override ZString CHStatusWhenWrongXMLOrMessageTextEmpty => ZString.Empty;
	}
}
