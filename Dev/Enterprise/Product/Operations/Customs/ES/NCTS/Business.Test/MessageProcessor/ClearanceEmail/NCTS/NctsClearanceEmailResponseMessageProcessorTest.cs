using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsClearanceEmailResponseMessageProcessorTest : ESNCTSResponseMessageProcessorTest<NctsClearanceEmailResponseMessageProcessor, INctsClearanceEmailProvider>
	{
		public void TestProcessEmailWithAllData()
		{
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(mrnCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonNCTSClearanceEmailResponse(message, nctsHeader: nctsHeader, movCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNunber: ClearanceReferenceNumber, clearanceDate: ClearanceDate, arrivalLimit: ArrivalLimit, clearanceCriteria: "A1");

			var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var clrEntryNumber = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

			CombineAssertions("CLR Entry Number", () =>
			{
				AssertEquals("CE_ExpiryDate", ArrivalLimit, clrEntryNumber.CE_ExpiryDate);
				AssertEquals("CE_IssueDate", ClearanceDate, clrEntryNumber.CE_IssueDate);
				AssertEquals("CE_EntryNum", ClearanceReferenceNumber, clrEntryNumber.CE_EntryNum);
			});
		}

		public void TestProcessEmailWithOnlyClearance()
		{
			var mailBody = GetMailBody(false);
			var message = CreateNewEDIMessage(mrnCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message);
			CommonNCTSClearanceEmailResponse(message, nctsHeader: nctsHeader, movCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNunber: ClearanceReferenceNumber, clearanceDate: ClearanceDate);

			var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var clrEntryNumber = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

			CombineAssertions("CLR Entry Number", () =>
			{
				AssertEquals("CE_ExpiryDate", ZDateTime.Empty, clrEntryNumber.CE_ExpiryDate);
				AssertEquals("CE_IssueDate", ClearanceDate, clrEntryNumber.CE_IssueDate);
				AssertEquals("CE_EntryNum", ClearanceReferenceNumber, clrEntryNumber.CE_EntryNum);
			});
		}

		public void TestCreateDocumentCaptureRequest()
		{
			var mailBody = GetMailBody(true);
			var message = CreateNewEDIMessage(mrnCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, nctsHeader.Messages);
			CommonNCTSClearanceEmailResponse(message, nctsHeader: nctsHeader, movCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNunber: ClearanceReferenceNumber, clearanceDate: ClearanceDate, arrivalLimit: ArrivalLimit, clearanceCriteria: "A1");

			CombineAssertions(() =>
			{
				var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
				AssertEquals("Doc Messages sent number is", 1, docMessages.Length);

				AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (mrnCode + "_NCTS_AEAT_TAD.pdf", mrnCode) });
			});
		}

		public void TestCreateDocumentCaptureRequest_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], mrnCode + "_NCTS_AEAT_TAD.pdf", "CAU");
			docManagerInfo.Save();

			var mailBody = GetMailBody(false);
			var message = CreateNewEDIMessage(mrnCode, mailBody, InterchangeID, false);

			ProcessMessageForTest(message, nctsHeader.Messages);
			CommonNCTSClearanceEmailResponse(message, nctsHeader: nctsHeader, movCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNunber: ClearanceReferenceNumber, clearanceDate: ClearanceDate);

			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
				AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		protected void CommonNCTSClearanceEmailResponse(TestEdiMessage message, NctsHeader nctsHeader = null, string movCustomsStatus = null, string clearanceReferenceNunber = null, ZDateTime? clearanceDate = null, string clearanceCriteria = "", ZDateTime? arrivalLimit = null)
		{
			GenericCommonAssertProcessResponseNctsHeader(message, nctsHeader: nctsHeader, emStatus: EDIMessage.Status.Received, messageSubType: "ACC", movCustomsStatus: movCustomsStatus, clearanceReferenceNumber: clearanceReferenceNunber, clearanceDate: clearanceDate, clearanceCriteria: clearanceCriteria, arrivalLimit: arrivalLimit, bhMessageStatus: OriginalMessageStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.EffectiveMessageStatus = OriginalMessageStatus;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			SetMRN(nctsHeader);

			AddPreviousResponseMessages();
		}

		ZString OriginalMessageStatus => "INI";
		ZString OriginalCHStatus => "INI";

		const string mrnCode = "20ES00999830001277";
		protected readonly ZDateTime ArrivalLimit = new ZDateTime(2021, 06, 17);

		protected override NctsClearanceEmailResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new NctsClearanceEmailResponseMessageProcessor(logger);

		protected override ZString GetExpectedProcessorFriendlyName() => "Ncts Clearance Email Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.NctsClearanceEmail };

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "TEST";
			SetMRN(nctsHeader);

			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageType;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageSubType = "AAA";
			message.EM_ApplicationReference = mrnCode;
			message.EM_MessageText = GetMailBody(true);

			Factory.Save();

			return message;
		}

		void AddPreviousResponseMessages()
		{
			var lastAcceptedResponseMessage = CreateNewEDIMessage(ApplicationReference, "previous response", new Guid(), false);
			lastAcceptedResponseMessage.EM_Status = EDIMessage.Status.Received;
			lastAcceptedResponseMessage.EM_MessageSubType = "ACC";
			nctsHeader.Messages.Add(lastAcceptedResponseMessage);

			Factory.Save();

			var lastRejectedResponseMessage = Factory.NewWithValidTestData<TestEdiMessage>();
			lastRejectedResponseMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			lastRejectedResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			lastRejectedResponseMessage.EM_MessageType = MessageType;
			lastRejectedResponseMessage.EM_Status = EDIMessage.Status.Received;
			lastRejectedResponseMessage.EM_MessageSubType = "REJ";
			var lastRejectedResponseInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			lastRejectedResponseInterchange.ContainedMessages.Add(lastRejectedResponseMessage);
			nctsHeader.Messages.Add(lastRejectedResponseMessage);
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

		ZString GetMailBody(ZBool includeAllData)
		{
			if (includeAllData)
			{
				return @"Su declaración de expedición de tránsito con número MRN: 21ES00359150119711 ha sido despachada con el siguiente Número de Autentificación de Levante. Ya puede/debe imprimir el Documento de Acompañamiento (D.A.T.) en la Sede Electrónica de la AEAT (puede cuando está autorizado a imprimir, y debe cuando no esté autorizado a imprimir).

Número de Autentificación de Levante: A198543129CBF854 Fecha de Levante: 21-11-2020 Fecha Máxima de Llegada: 17-06-2021 Resultado al Despacho: A1
BULTOS: 40
CONTENEDORES: 
MARCAS: RTDS, RTDS, RTDS, RTDS, RTDS, RTDS";
			}
			else
			{
				return @"Su declaración de expedición de tránsito con número MRN: 21ES00084152088719 ha sido despachada con el siguiente Número de Autentificación de Levante. Ya puede/debe imprimir el Documento de Acompañamiento (D.A.T.) en la Sede Electrónica de la AEAT (puede cuando está autorizado a imprimir, y debe cuando no esté autorizado a imprimir).

Número de Autentificación de Levante: A198543129CBF854
Fecha de Levante: 21-11-2020
BULTOS: 33
CONTENEDORES: CAIU4568990, CAIU4568990, CAIU4568990, CAIU4568990
MARCAS: PUS, PUS, PUS, PUS, PUS, PUS, PUS, PUS, PUS, PUS
";
			}
		}

		void SetMRN(NctsHeader nctsHeader)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = mrnCode;
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
		}
	}
}
