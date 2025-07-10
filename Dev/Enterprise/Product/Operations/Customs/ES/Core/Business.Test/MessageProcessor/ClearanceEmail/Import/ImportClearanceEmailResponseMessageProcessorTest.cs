using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ImportClearanceEmailResponseMessageProcessorTest : ESResponseMessageProcessorTest<ImportClearanceEmailResponseMessageProcessor, IImportClearanceEmailProvider>
	{
		public void TestProcessEmailWithAllData()
		{
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, csvImportCertificate: CSVImportCertificate);
		}

		public void TestProcessEmailWithAllData_InstructionC()
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, csvImportCertificate: CSVImportCertificate);
		}

		public void TestProcessEmailWithAllData_InstructionZ()
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Z;
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, csvImportCertificate: CSVImportCertificate);
		}

		public void TestProcessEmailWithAllData_InstructionA()
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, chEntryStatus: EntryStatusCodes.Cleared, csvImportCertificate: CSVImportCertificate);
		}

		public void TestProcessEmailWithAllData_InstructionY()
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, chEntryStatus: EntryStatusCodes.Cleared, csvImportCertificate: CSVImportCertificate);
		}

		public void TestProcessEmailWithOnlyClearance()
		{
			var mailBody = GetMailBody(false);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader);
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public void TestTriggerIQUMessage()
		{
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, csvImportCertificate: CSVImportCertificate);

			CombineAssertions(() =>
			{
				var msg = entryHeader.Messages.LastMessage;
				AssertEquals("New EDIMessage is type IQU", DeclarationMessageTypeList.Codes.ImportQuery, msg.EM_MessageType);
				AssertEquals("New EDIMessage is Transmit", EDIMessage.Direction.Transmit, msg.EM_ReceiveTransmit);
				var randomSuffix = TransactionIdHelper.GetRandomSuffix(msg.EM_MessageText, "ES200109161323", 4);
				AssertEquals("New EDIMessage has correct text", ExpectedIQUMessageText(randomSuffix), msg.EM_MessageText);
			});
		}
		public void TestNoTriggerIQUMessage()
		{
			declaration.JE_CustomsProfile = ZString.Empty;

			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, chStatus: OriginalCHStatus, csvImportCertificate: CSVImportCertificate);

			CombineAssertions(() =>
			{
				var msg = entryHeader.Messages.LastMessage;
				AssertEquals("Last EDIMessage is type MIM", DeclarationMessageTypeList.Codes.ImportClearanceEmail, msg.EM_MessageType);
				AssertEquals("Last EDIMessage is Receive", EDIMessage.Direction.Receive, msg.EM_ReceiveTransmit);
				AssertContains("logger", "Can't trigger Import Query message, wrong broker or certificate in entry " + entryHeader.CH_BGMReference, logger.UserLogStrings[0]);
			});
		}

		public void TestDontCreateDocumentCaptureRequest()
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonImportClearanceEmailResponse(message, entryHeader: entryHeader, chEntryStatus: EntryStatusCodes.Cleared, csvImportCertificate: CSVImportCertificate);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
			});
		}

		protected void CommonImportClearanceEmailResponse(TestEdiMessage message, CusEntryHeader entryHeader = null, string chStatus = MessageStatusList.Codes.AwaitingResponse, string chEntryStatus = EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, string csvImportCertificate = "")
		{
			GenericCommonAssertProcessResponseEntryHeader(message, entryHeader: entryHeader, emStatus: EDIMessage.Status.Received, messageSubType: "ACC", chStatus: chStatus, chEntryStatus: chEntryStatus, csvClearance: CSVClearance, csvImportCertificate: csvImportCertificate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;

			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			declaration.JE_GS_NKCusAgent = staffWithCertificateHelperTest.Staff.GS_Code;
			declaration.JE_CustomsProfile = staffWithCertificateHelperTest.Certificate.CertificateName;

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

		const string OriginalCHStatus = "INI";
		const string MRNCode = "20ES00999830001277";
		const string CSVClearance = "52BD5D2SD67PG42H";
		const string CSVImportCertificate = "PBAGF96MUR2EXDER";

		protected override ImportClearanceEmailResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new ImportClearanceEmailResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Import Clearance Email Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.ImportClearanceEmail };

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
			message.EM_MessageText = GetMailBody(true);

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

		ZString GetMailBody(ZBool includeImportCertificate)
		{
			if (includeImportCertificate)
			{
				return @"A su declaración de importación con número 20ES00999830001277 se le ha asignado con el siguiente código seguro de verificación (C.S.V.)  del Justificante de Levante. Este C.S.V. le permitirá la consulta e impresión de su contenido en la Sede de la A.E.A.T. https://urldefense.com/v3/__https://www.agenciatributaria.gob.es__;!!Na5NE8kfbMIR6Ys!43cnxwoRROiqYk-EdpeT3qYT-p9zwPWIprBy6yPEMm8jWnsWz0-OCF8EznfLpn_CSg$ .
CSV Levante: 52BD5D2SD67PG42H
CSV Certificado de Importación: PBAGF96MUR2EXDER
BULTOS: 10
NUMERO REFERENCIA CASILLA 7:
CONTENEDORES: FSCU4762868,12312331231,31132312
MARCAS: RTDAS";
			}
			else
			{
				return @"A su declaración de importación con número 20ES00999830001277 se le ha asignado con el siguiente código seguro de verificación (C.S.V.)  del Justificante de Levante. Este C.S.V. le permitirá la consulta e impresión de su contenido en la Sede de la A.E.A.T. https://urldefense.com/v3/__https://www.agenciatributaria.gob.es__;!!Na5NE8kfbMIR6Ys!43cnxwoRROiqYk-EdpeT3qYT-p9zwPWIprBy6yPEMm8jWnsWz0-OCF8EznfLpn_CSg$ .
CSV Levante: 52BD5D2SD67PG42H
BULTOS: 10
NUMERO REFERENCIA CASILLA 7:
CONTENEDORES: FSCU4762868,12312331231,31132312
MARCAS: RTDAS";
			}
		}

		ZString ExpectedIQUMessageText(string randomSuffix) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent xmlns{XMLTestFileConstants.XmlElementNamespaceSuffix}=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/dit/adu/adip/ws/ConsultaImportacionV2Ent.xsd"">
  <SegmentosDeServicio Id=""ES200109161323{randomSuffix}"" fecha=""20200109"" hora=""161323"" />
  <NumeroDeReferencia>20ES00999830001277</NumeroDeReferencia>
</{XMLTestFileConstants.XmlElementNamespace}ConsultaImportacionV2Ent>
  </soapenv:Body>
</soapenv:Envelope>";
	}
}
