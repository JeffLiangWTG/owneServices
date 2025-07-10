using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ExportClearanceEmailResponseMessageProcessorTest : ESResponseMessageProcessorTest<ExportClearanceEmailResponseMessageProcessor, IExportClearanceEmailProvider>
	{
		public void TestProcessEmailWithAllData()
		{
			var mailBody = GetMailBodyWithAllData();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ClearanceDate, arrivalLimit: ArrivalLimit);
		}

		public void TestProcessEmailWithAllDataWithOtherEntryInstruction()
		{
			var mailBody = GetMailBodyWithAllData();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, chEntryStatus: MessageProcessorConstants.EntryStatusCodes.Cleared, clearanceDate: ClearanceDate, arrivalLimit: ArrivalLimit);
		}

		public void TestProcessEmailWithoutLimitDateOfArrival()
		{
			var mailBody = GetMailBodyWithoutLimitDateOfArrival();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ClearanceDate);
		}

		public void TestProcessEmailWithoutReleaseDate()
		{
			var mailBody = GetMailBodyWithoutReleaseDate();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ZDateTime.Today, arrivalLimit: ArrivalLimit);
		}

		public void TestProcessEmailWithoutClearanceResult()
		{
			var mailBody = GetMailBodyWithoutClearanceResult();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ClearanceDate, arrivalLimit: ArrivalLimit, clearanceResult: ZString.Empty);
		}

		public void TestProcessEmailWithoutReleaseDateAndClearanceResult()
		{
			var mailBody = GetMailBodyWithoutReleaseDateAndClearanceResult();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ZDateTime.Today, arrivalLimit: ArrivalLimit, clearanceResult: ZString.Empty);
		}

		public void TestProcessEmailWithOnlyCSVClearance()
		{
			var mailBody = GetMailBodyWithOnlyCSVClearance();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ZDateTime.Today, clearanceResult: ZString.Empty);
		}
		
		public void TestCreateDocumentCaptureRequest_AllDocs()
		{
			var csvT2L = "CSVT2L";

			declaration.ZG_CTStatusID = "T2LF";
			entryHeader.EUH_EADPrintProcedure = "1";
			entryHeader.ZG_CSVT2L = csvT2L;

			var mailBody = GetMailBodyWithAllData();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ClearanceDate, arrivalLimit: ArrivalLimit);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 3, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_CLR.pdf", CSVClearance),
																													(MRNCode + "_E_AEAT_ead.pdf", MRNCode),
																													(MRNCode + "_E_AEAT_t2lf.pdf", csvT2L) });
			});
		}

		public void TestCreateDocumentCaptureRequest_NoDocs()
		{
			declaration.ZG_CTStatusID = "T2LF";
			entryHeader.EUH_EADPrintProcedure = "1";
			entryHeader.ZG_CSVT2L = "CSVT2L";

			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_CLR.pdf", "CLR");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_ead.pdf", "CAU");
			var eDoc3 = docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_t2lf.pdf", "CAU");
			docManagerInfo.Save();

			var mailBody = GetMailBodyWithAllData();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonExportClearanceEmailResponse(message, entryHeader: entryHeader, clearanceDate: ClearanceDate, arrivalLimit: ArrivalLimit);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		protected void CommonExportClearanceEmailResponse(TestEdiMessage message, CusEntryHeader entryHeader = null, string chEntryStatus = MessageProcessorConstants.EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, ZDateTime? clearanceDate = null, ZDateTime? arrivalLimit = null, string clearanceResult = "A2")
		{
			GenericCommonAssertProcessResponseEntryHeader(message, entryHeader: entryHeader, emStatus: EDIMessage.Status.Received, messageSubType: "ACC", chStatus: OriginalCHStatus, chEntryStatus: chEntryStatus, csvClearance: CSVClearance, clearanceDate: clearanceDate, arrivalLimit: arrivalLimit, clearanceResult: clearanceResult);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_Status = OriginalCHStatus;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);

			declaration.ActiveEntryHeaders.AddNew();

			AddPreviousResponseMessages();
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;

		void AddPreviousResponseMessages()
		{
			var lastAcceptedResponseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, "previous response", new Guid(), false);
			lastAcceptedResponseMessage.EM_Status = EDIMessage.Status.Received;
			lastAcceptedResponseMessage.EM_MessageSubType = "ACC";
			entryHeader.Messages.Add(lastAcceptedResponseMessage);

			Factory.Save();

			var lastRejectedResponseMessage = Factory.NewWithValidTestData<TestEdiMessage>();
			lastRejectedResponseMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			lastRejectedResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			lastRejectedResponseMessage.EM_MessageType = MessageType;
			lastRejectedResponseMessage.EM_Status = EDIMessage.Status.Received;
			lastRejectedResponseMessage.EM_MessageSubType = "REJ";
			var lastRejectedResponseInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			lastRejectedResponseInterchange.ContainedMessages.Add(lastRejectedResponseMessage);
			entryHeader.Messages.Add(lastRejectedResponseMessage);
		}

		const string MRNCode = "21ES00280120889150";
		const string CSVClearance = "5ZJV4LCUFX79CJY6";
		protected readonly ZDateTime ClearanceDate = new ZDateTime(2021, 06, 20);
		protected readonly ZDateTime ArrivalLimit = new ZDateTime(2021, 06, 29);
		ZString OriginalCHStatus => "INI";

		protected override ExportClearanceEmailResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ExportClearanceEmailResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Export Clearance Email Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceEmail };

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = MRNCode;
			message.EM_MessageText = GetMailBodyWithAllData();

			return message;
		}

		protected override void AssertLoggerMessagesWhenPreProcessMessageWrongSessionGUID()
		{
			AssertContains("logger", "Unable to find business object for message", logger.UserLogStrings[0]);
		}

		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "Email Body doesn't have the correct data", concatenatedUserLogStrings);
			AssertEquals("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: Email Body doesn't have the correct data</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		ZString GetMailBodyWithAllData()
		{
			return @"Su declaración de exportación con número 21ES00280120889150 ha sido despachada con el siguiente código  seguro de verificación (C.S.V.) del Justificante  de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): 5ZJV4LCUFX79CJY6 Fecha Máxima de Llegada: 29-06-2021 Fecha de Levante: 20-06-2021 Resultado al Despacho: A2
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN TALGO 250, WAGON, 2502N-01/2502N-02/2502N-03/2502N-04, ROTULADAS, ROTULADAS, ROTULADAS, ROTULADAS";
		}

		ZString GetMailBodyWithoutLimitDateOfArrival()
		{
			return @"Su declaración de exportación con número 21ES00280120889150 ha sido despachada con el siguiente código  seguro de verificación (C.S.V.) del Justificante  de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): 5ZJV4LCUFX79CJY6 Fecha de Levante: 20-06-2021 Resultado al Despacho: A2
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN TALGO 250, WAGON, 2502N-01/2502N-02/2502N-03/2502N-04, ROTULADAS, ROTULADAS, ROTULADAS, ROTULADAS";
		}

		ZString GetMailBodyWithoutReleaseDate()
		{
			return @"Su declaración de exportación con número 21ES00280120889150 ha sido despachada con el siguiente código  seguro de verificación (C.S.V.) del Justificante  de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): 5ZJV4LCUFX79CJY6 Fecha Máxima de Llegada: 29-06-2021 Resultado al Despacho: A2
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN TALGO 250, WAGON, 2502N-01/2502N-02/2502N-03/2502N-04, ROTULADAS, ROTULADAS, ROTULADAS, ROTULADAS";
		}

		ZString GetMailBodyWithoutClearanceResult()
		{
			return @"Su declaración de exportación con número 21ES00280120889150 ha sido despachada con el siguiente código  seguro de verificación (C.S.V.) del Justificante  de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): 5ZJV4LCUFX79CJY6 Fecha Máxima de Llegada: 29-06-2021 Fecha de Levante: 20-06-2021
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN TALGO 250, WAGON, 2502N-01/2502N-02/2502N-03/2502N-04, ROTULADAS, ROTULADAS, ROTULADAS, ROTULADAS";
		}

		ZString GetMailBodyWithoutReleaseDateAndClearanceResult()
		{
			return @"Su declaración de exportación con número 21ES00280120889150 ha sido despachada con el siguiente código  seguro de verificación (C.S.V.) del Justificante  de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): 5ZJV4LCUFX79CJY6 Fecha Máxima de Llegada: 29-06-2021
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN TALGO 250, WAGON, 2502N-01/2502N-02/2502N-03/2502N-04, ROTULADAS, ROTULADAS, ROTULADAS, ROTULADAS";
		}

		ZString GetMailBodyWithOnlyCSVClearance()
		{
			return @"Su declaración de exportación con número 21ES00280120889150 ha sido despachada con el siguiente código  seguro de verificación (C.S.V.) del Justificante  de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): 5ZJV4LCUFX79CJY6
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN TALGO 250, WAGON, 2502N-01/2502N-02/2502N-03/2502N-04, ROTULADAS, ROTULADAS, ROTULADAS, ROTULADAS";
		}
	}
}
