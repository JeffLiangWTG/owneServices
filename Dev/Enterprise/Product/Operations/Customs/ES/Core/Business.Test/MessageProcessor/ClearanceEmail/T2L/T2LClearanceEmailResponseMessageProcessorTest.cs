using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LClearanceEmailResponseMessageProcessorTest : ESCommonResponseMessageProcessorTest<T2LClearanceEmailResponseMessageProcessor, CusEntryHeader, IT2LClearanceEmailProvider>
	{
		public void TestProcessEmailCSV_T2L()
		{
			var mailBody = GetMailBody();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonT2lClearanceEmailResponse(message, entryHeader: entryHeader);
		}

		public void TestProcessEmailCSV_T2L_With2EntriesWithSameMRN()
		{
			entryHeader.Delete();

			var declarationImport = Factory.NewWithValidTestData<JobDeclaration>();
			declarationImport.JE_MessageType = MessageTypeList.Codes.Import;
			var entryHeaderImport = (CusEntryHeader)declarationImport.ActiveEntryHeaders.AddNew();
			entryHeaderImport.CH_Status = OriginalCHStatus;
			entryHeaderImport.CH_EntryStatus = OriginalEntryStatus;
			entryHeaderImport.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);
			declarationImport.ActiveEntryHeaders.AddNew();

			var declarationExport = Factory.NewWithValidTestData<JobDeclaration>();
			declarationExport.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeaderExport = (CusEntryHeader)declarationExport.ActiveEntryHeaders.AddNew();
			entryHeaderExport.CH_Status = OriginalCHStatus;
			entryHeaderExport.CH_EntryStatus = OriginalEntryStatus;
			entryHeaderExport.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);
			declarationExport.ActiveEntryHeaders.AddNew();

			var mailBody = GetMailBody();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonT2lClearanceEmailResponse(message, entryHeader: entryHeaderExport);
		}

		public void TestCreateDocumentCaptureRequest_AllDocs_T2LExpedition()
		{
			var mailBody = GetMailBody();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonT2lClearanceEmailResponse(message, entryHeader: entryHeader);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (MRNCode + "_E_AEAT_T2L_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequest_NoDocs_T2LExpedition()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCode + "_E_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var mailBody = GetMailBody();
			var message = CreateNewEDIMessage(MRNCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonT2lClearanceEmailResponse(message, entryHeader: entryHeader);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		void CreateT2CEntryNumber(CusEntryHeader entryHeader)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = MRNCodeT2C;
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
		}

		public void TestProcessEmailCSV_T2C()
		{
			var mailBody = GetMailBody();
			var message = CreateNewEDIMessage(MRNCodeT2C, mailBody, InterchangeID, false);
			message.EM_MessageType = DeclarationMessageTypeList.Codes.T2cClearanceEmail;

			CreateT2CEntryNumber(entryHeader);

			Factory.Save();

			ProcessMessageForTest(message);
			CommonT2lClearanceEmailResponse(message, entryHeader: entryHeader);
		}

		public void TestCreateDocumentCaptureRequest_AllDocs_T2LClearance()
		{
			var mailBody = GetMailBody();
			var message = CreateNewEDIMessage(MRNCodeT2C, mailBody, InterchangeID, false);
			message.EM_MessageType = DeclarationMessageTypeList.Codes.T2cClearanceEmail;

			CreateT2CEntryNumber(entryHeader);

			Factory.Save();

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonT2lClearanceEmailResponse(message, entryHeader: entryHeader);

			CombineAssertions(() =>
			{
				var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { ("21ES009999M0000707_I_AEAT_T2L_CLR.pdf", CSVClearance) });
			});
		}

		public void TestCreateDocumentCaptureRequest_NoDocs_T2LClearance()
		{
			var docManagerInfo = ((IDocManagerSupport)entryHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], MRNCodeT2C + "_I_AEAT_T2L_CLR.pdf", "CLR");
			docManagerInfo.Save();

			var mailBody = GetMailBody();
			var message = CreateNewEDIMessage(MRNCodeT2C, mailBody, InterchangeID, false);
			message.EM_MessageType = DeclarationMessageTypeList.Codes.T2cClearanceEmail;

			CreateT2CEntryNumber(entryHeader);

			Factory.Save();

			ProcessMessageForTest(message, entryHeader.Messages);
			CommonT2lClearanceEmailResponse(message, entryHeader: entryHeader);

			var docMessages = entryHeader.Messages.GetMatchingMessages("ESC", ["DOC"], "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		protected void CommonT2lClearanceEmailResponse(TestEdiMessage message, CusEntryHeader entryHeader = null)
		{
			GenericCommonAssertProcessResponseEntryHeader(message, entryHeader: entryHeader, emStatus: EDIMessage.Status.Received, messageSubType: "ACC", chStatus: OriginalCHStatus, chEntryStatus: OriginalEntryStatus, csvClearance: CSVClearance);
		}

		protected override T2LClearanceEmailResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new T2LClearanceEmailResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "T2L Clearance Email Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail, DeclarationMessageTypeList.Codes.T2cClearanceEmail };

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

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = MRNCode;
			message.EM_MessageText = GetMailBody();

			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = OriginalCHStatus;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryHeader.MovementReferenceNumberSetter(MRNCode, ZDateTime.Today);
			declaration.ActiveEntryHeaders.AddNew();

			AddPreviousResponseMessages();
		}
		CusEntryHeader entryHeader;

		ZString GetMailBody()
		{
			return @"Su declaración de T2L con número 21ES009999L0002748 ha sido despachado con el siguiente código seguro de verificación (C.S.V.) del justificante de Levante. Este C.S.V. le permitirá la consulta e impresión de su contenido en la Sede de la A.E.A.T.

CSV: 34YEUXEP8E7GE9XD
BULTOS: 22
CONTENEDORES: CONTENEDOR1, CONTENEDOR2
MARCAS: RTDASÑ
";
		}

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

		const string MRNCode = "21ES009999L0002748";
		const string MRNCodeT2C = "21ES009999M0000707";
		const string CSVClearance = "34YEUXEP8E7GE9XD";
		ZString OriginalCHStatus => "INI";
	}
}
