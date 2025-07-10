using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public abstract class ESNCTSResponseMessageProcessorTest<TResponseMessageProcessor, TResponseProvider> : ESCommonResponseMessageProcessorTest<TResponseMessageProcessor, NctsHeader, TResponseProvider>
	where TResponseMessageProcessor : ESNCTSResponseMessageProcessor<TResponseProvider>
	{
		protected override void AssertLoggerMessagesWhenProcessMessageWrongText(TestEdiMessage message, BusinessObject businessObject)
		{
			if (businessObject is NctsHeader nctsHeader1)
			{
				AssertEquals("EffectiveMessageStatus", "FAL", nctsHeader1.EffectiveMessageStatus);
			}

			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", "Unable to read message text from message ", concatenatedUserLogStrings);
			AssertContains("logger exception", "There is an error in XML document ", concatenatedUserLogStrings);
			AssertContains("EM_MessageInterpretation",
					string.Format("<H3>Processor Failure</H3><br>" +
					"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
					"<H4>Exception: There is an error in XML document ", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference)
					, message.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = ApplicationReference;
			nctsHeader.BH_HeaderType = TypeDeclaration == NctsMovementType.Codes.Departure
														? NctsMovementType.Codes.Departure
														: NctsMovementType.Codes.Arrival;

			message = CreateNewEDIMessage(ApplicationReference, "Message text", InterchangeID, false);
			message.EM_LinkedObject = nctsHeader;
		}
		protected NctsHeader nctsHeader;
		protected TestEdiMessage message;

		protected virtual ZString TypeDeclaration => NctsMovementType.Codes.Departure;

		public void AssertNCTS(TestEdiMessage message, NctsHeader nctsHeader, string emStatus = EDIMessage.Status.Received, string expectedMessageInterpretation = "", string messageSubType = "", string messageStatus = EDIMessage.Status.Received, string commonCustomsStatus = "")
		{
			GenericCommonAssertProcessEntryDataNCTS(message, nctsHeader: nctsHeader, emStatus: emStatus, esNctsHeader: nctsHeader.ESNctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, messageStatus: messageStatus, commonCustomsStatus: commonCustomsStatus);
		}

		protected const string TransitReferenceNumber = "20ES009998500102";
		protected const string SummaryReferenceNumber = "99980000521";
		protected const string ClearanceReferenceNumber = "A198543129CBF854";
		protected readonly ZDateTime ClearanceDate = new ZDateTime(2020, 11, 21, 00, 00, 00);
		protected readonly ZDateTime AdmissionDate = new ZDateTime(2020, 11, 20, 18, 56, 00);
		protected readonly ZDateTime ExpiryDate = new ZDateTime(2020, 4, 16);

		protected CusEntryNumber CreateOrUpdateCusEntryNumber(NctsHeader header, ZString entryType, ZString entryNum, ZString entryStatus)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(header, entryType, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = entryNum;
			newEntryNumber.CE_EntryStatus = entryStatus;
			newEntryNumber.CE_EntryIsSystemGenerated = true;

			return newEntryNumber;
		}

		protected override TestEdiMessage SetDataForCorrectPreProcessing(ZString interchangeTransportType)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.BH_JobReference = "TEST";

				var interchangeID = ZGuid.NewZGuid();

				SetSentInterchange(nctsHeader, interchangeID);

				var message = Factory.New<TestEdiMessage>();
				message.EM_ApplicationReference = "TEST";
				var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
				responseInterchange.ContainedMessages.Add(message);
				return message;
			}
		}

		public void GenericCommonAssertProcessEntryDataNCTS(TestEdiMessage responseMessage, NctsHeader nctsHeader, ZGuid? securityConsignorE2_OA_Address = null, ZGuid? securityConsigneeE2_OA_Address = null, ZGuid? headerOH_Carrier = null, CusEntryNumber cusEntryNumberMRN = null, CusEntryNumber cusEntryNumberClearance = null, CusEntryNumber cusEntryNumberSummary = null, string expectedMessageInterpretation = "", string messageStatus = EDIMessage.Status.Received, string emStatus = EDIMessage.Status.Received, string messageNum = "", string messageSubType = "", string commonCustomsStatus = "", string esNctsHeaderClearanceCriteria = "", string esNctsHeaderTADPrintProcedure = "", string movementReferenceNumber = "", string clearanceReferenceNumber = "", string mrnEntryStatus = "", ZDateTime? clearanceIssueDate = null, ZDateTime? mrnIssueDate = null, ZDateTime? clearanceExpiryDate = null, bool headerFTZMove = false, string commonBTAIndicator = "", string commonMethodOfPayment = "", string commonAdditionalText = "", string commonConveyanceNumber = "", string placeOfUnloadingCode = "", string headerUniqueVoyageIdentifier = "", int headerItineraty = 0, NctsDepartureMovementHeader departureMovementHeader = null, NctsArrivalMovementHeader arrivalMovementHeader = null, CusESNctsHeader esNctsHeader = null, string summaryEntryType = null, string mrnEntrynum = null, string summaryEntryNum = null, ZDateTime? clearanceDate = null, ZDateTime? arrivalLimit = null, bool differentCommonCustomsStatus = false, string departureCustomStatus = "")
		{
			var movementHeaderCustomsStatus = differentCommonCustomsStatus ? departureCustomStatus : commonCustomsStatus;
			GenericCommonAssertProcessResponseNctsHeader(responseMessage, nctsHeader: nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: emStatus, messageNum: messageNum, messageSubType: messageSubType, bhMessageStatus: messageStatus, clearanceReferenceNumber: clearanceReferenceNumber, clearanceCriteria: esNctsHeaderClearanceCriteria, movCustomsStatus: movementHeaderCustomsStatus, clearanceDate: clearanceDate, arrivalLimit: arrivalLimit);
			CombineAssertions(() =>
			{
				AssertEquals("MovementReferenceNumber", movementReferenceNumber, nctsHeader.MovementReferenceNumber);
				AssertEquals("BH_FTZMove", headerFTZMove, nctsHeader.BH_FTZMove);
				AssertEquals("PlaceOfUnloadingCode", placeOfUnloadingCode, nctsHeader.PlaceOfUnloadingCode);
				AssertEquals("BH_UniqueVoyageIdentifier", headerUniqueVoyageIdentifier, nctsHeader.BH_UniqueVoyageIdentifier);
				AssertEquals("SecurityConsignor.E2_OA_Address", securityConsignorE2_OA_Address == null ? ZGuid.Empty : securityConsignorE2_OA_Address, nctsHeader.SecurityConsignor.E2_OA_Address);
				AssertEquals("SecurityConsignee.E2_OA_Address", securityConsigneeE2_OA_Address == null ? ZGuid.Empty : securityConsigneeE2_OA_Address, nctsHeader.SecurityConsignee.E2_OA_Address);
				AssertEquals("BH_OH_Carrier", headerOH_Carrier == null ? ZGuid.Empty : headerOH_Carrier, nctsHeader.BH_OH_Carrier);
				AssertEquals("Itineraty", headerItineraty, nctsHeader.Itinerary.Count);
				if (cusEntryNumberMRN != null)
				{
					AssertEquals("cusEntryNumberMRN.CE_EntryStatus", mrnEntryStatus, cusEntryNumberMRN.CE_EntryStatus);
					AssertEquals("cusEntryNumberMRN.CE_IssueDate", mrnIssueDate == null ? ZDateTime.Empty : mrnIssueDate, cusEntryNumberMRN.CE_IssueDate);
					AssertEquals("cusEntryNumberMRN.CE_EntryNum", mrnEntrynum, cusEntryNumberMRN.CE_EntryNum);
				}
				if (cusEntryNumberClearance != null)
				{
					AssertEquals("cusEntryNumberClearance.CE_ExpiryDate", clearanceExpiryDate == null ? ZDateTime.Empty : clearanceExpiryDate, cusEntryNumberClearance.CE_ExpiryDate);
					AssertEquals("cusEntryNumberClearance.CE_IssueDate", clearanceIssueDate == null ? ZDateTime.Empty : clearanceIssueDate, cusEntryNumberClearance.CE_IssueDate);
				}
				if (cusEntryNumberSummary != null)
				{
					AssertEquals("cusEntryNumberSummary.CE_EntryType", summaryEntryType, cusEntryNumberSummary.CE_EntryType);
					AssertEquals("cusEntryNumberSummary.CE_EntryNum", summaryEntryNum, cusEntryNumberSummary.CE_EntryNum);
				}
				if (esNctsHeader != null)
				{
					AssertEquals("ESNctsHeader.CEN_TADPrintProcedure", esNctsHeaderTADPrintProcedure, esNctsHeader.CEN_TADPrintProcedure);
				}
				if (departureMovementHeader != null)
				{
					AssertEquals("Departure BM_BTAIndicator", commonBTAIndicator, departureMovementHeader.BM_BTAIndicator);
					AssertEquals("Departure BM_MethodOfPayment", commonMethodOfPayment, departureMovementHeader.BM_MethodOfPayment);
					AssertEquals("Departure BM_AdditionalText", commonAdditionalText, departureMovementHeader.BM_AdditionalText);
					AssertEquals("Departure BM_ConveyanceNumber", commonConveyanceNumber, departureMovementHeader.BM_ConveyanceNumber);
				}
				if (arrivalMovementHeader != null)
				{
					AssertEquals("Arrival MovementHeader.BM_CustomsStatus", commonCustomsStatus, arrivalMovementHeader.BM_CustomsStatus);
					AssertEquals("Arrival BM_BTAIndicator", commonBTAIndicator, arrivalMovementHeader.BM_BTAIndicator);
					AssertEquals("Arrival BM_MethodOfPayment", commonMethodOfPayment, arrivalMovementHeader.BM_MethodOfPayment);
					AssertEquals("Arrival BM_AdditionalText", commonAdditionalText, arrivalMovementHeader.BM_AdditionalText);
					AssertEquals("Arrival BM_ConveyanceNumber", commonConveyanceNumber, arrivalMovementHeader.BM_ConveyanceNumber);
				}
			});
		}

		protected void GenericCommonAssertProcessResponseNctsHeader(TestEdiMessage responseMessage, NctsHeader nctsHeader = null, string expectedMessageInterpretation = "", string emStatus = EDIMessage.Status.Failed, string messageNum = "", string messageSubType = "AAA", string bhMessageStatus = "", string loggerDesc = "", string movCustomsStatus = "", string clearanceReferenceNumber = "", ZDateTime? clearanceDate = null, string clearanceCriteria = "", ZDateTime? arrivalLimit = null)
		{
			CombineAssertions(() =>
			{
				GenericCommonAssertProcessResponseOthers(responseMessage, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: emStatus, messageNum: messageNum, messageSubType: messageSubType, loggerDesc: loggerDesc);
				AssertEquals("EffectiveMessageStatus", bhMessageStatus, nctsHeader.EffectiveMessageStatus);
				AssertEquals("ClearanceReferenceNumber", clearanceReferenceNumber, nctsHeader.ClearanceReferenceNumber);
				AssertEquals("ClearanceDate", clearanceDate == null ? ZDateTime.Empty : clearanceDate, nctsHeader.ClearanceDate);
				AssertEquals("ArrivalLimit", arrivalLimit == null ? ZDateTime.Empty : arrivalLimit, nctsHeader.ArrivalLimit);
				if (nctsHeader.ESNctsHeader != null)
				{
					AssertEquals("ESNctsHeader.CEN_ClearanceCriteria", clearanceCriteria, nctsHeader.ESNctsHeader.CEN_ClearanceCriteria);
				}
				if (nctsHeader.MovementHeader != null)
				{
					AssertEquals("MovementHeader.BM_CustomsStatus", movCustomsStatus, nctsHeader.MovementHeader.BM_CustomsStatus);
				}
			});
		}

		protected CusGuaranteeHeader SetUpGuaranteeTransactions()
		{
			var eoriCode1 = "123456789000";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriCode1, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;

			guaranteeHeader.AddTransaction(ApplicationReference, "First-CON", SentMessageNumber, "", 100m, 200m, PermitTransactionStatusList.Codes.Confirmed, isAggregated: true);
			guaranteeHeader.AddTransaction(ApplicationReference, "Second-PEN", SentMessageNumber, "", 200m, 200m, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			guaranteeHeader.AddTransaction(ApplicationReference, "Third-PEN", SentMessageNumber, "", 300m, 200m, PermitTransactionStatusList.Codes.Pending, isAggregated: true);
			Factory.Save();

			return guaranteeHeader;
		}

		protected ZQuery GetTransactionQuery(ZString comment)
		{
			var zQuery = new ZQuery(CusPermitLineTransactionSchema.CPL_Comment, comment);
			zQuery.AddToFilter(CusPermitLineTransactionSchema.CPL_AppId, SentMessageNumber);
			zQuery.AddToFilter(CusPermitLineTransactionSchema.CPL_TransactionCategory, PermitTransactionCategoryList.Codes.CUM);
			zQuery.AddToFilter(CusPermitLineTransactionSchema.CPL_TransactionType, PermitTransactionTypeList.Codes.TRA);
			return zQuery;
		}
	}

	sealed class ESNCTSResponseMessageProcessorDoNotInheritTest : TestCaseWithFactory
	{
		public void TestGetRelevantBusinessObjectFindQuery_EHub()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_JobReference = "TEST";

			var interchangeID = ZGuid.NewZGuid();

			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			var sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_LinkedObject = nctsHeader;
			sentInterchange.ContainedMessages.Add(sentMessage);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationReference = "TEST";
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { nctsHeader };

			var messageProcessor = new ESNCTSResponseMessageProcessorForTest(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, false);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct nctsHeader", nctsHeader, foudBusinessObject);
		}

		public void TestGetRelevantBusinessObjectFindQuery_DirectxT()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_JobReference = "TEST";

			var interchangeID = ZGuid.NewZGuid();

			var sentInterchange = Factory.New<EDIInterchange>();
			sentInterchange.EI_SessionGUID = interchangeID;
			sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			var sentMessage = Factory.New<TestEdiMessage>();
			sentMessage.EM_LinkedObject = nctsHeader;
			sentInterchange.ContainedMessages.Add(sentMessage);

			var responseInterchange = Factory.New<EDIInterchange>();
			responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
			responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			responseInterchange.EI_SessionGUID = interchangeID;
			responseInterchange.EI_TransportType = EDIInterchange.TransportType.xT;
			var message = Factory.New<TestEdiMessage>();
			responseInterchange.ContainedMessages.Add(message);

			var sentBusinessObjects = new BusinessObject[] { nctsHeader };

			var messageProcessor = new ESNCTSResponseMessageProcessorForTest(new LoggingInformation());
			var foudBusinessObject = messageProcessor.FindRelevantBusinessObjectCoreExposed(message, sentBusinessObjects, true);

			AssertEquals("FindRelevantBusinessObjectCore finds the correct nctsHeader", nctsHeader, foudBusinessObject);
		}
	}

	class ESNCTSResponseMessageProcessorForTest : ESNCTSResponseMessageProcessor<object>
	{
		public ESNCTSResponseMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { ZString.Empty };

		protected override object GetMessageProviderCore(EDIMessage message) => null;

		public NctsHeader FindRelevantBusinessObjectCoreExposed(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage) => FindRelevantBusinessObjectCore(message, sentBusinessObjects, isDirectxTMessage);

		protected override void ProcessMessageCore(EDIMessage message, NctsHeader linkedBusinessObject, object response)
		{
		}
	}
}
