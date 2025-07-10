using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public abstract class NctsGenericDepartureResponseMessageProcessorTest<TResponse> : ESNCTSEDIResponseMessageProcessorTest<TResponse, INctsDepartureAndTIRResponseMessageProvider>
		where TResponse : NctsGenericDepartureResponseMessageProcessor
	{
		public void TestProcessMessageAcceptedGreenCircuitNotMocked()
		{
			SetSentInterchange(nctsHeader, InterchangeID);
			var messageText = ZString.Format("UNH+10100507081242+CUSRES:1:921:UN:TEX011'BGM+962+NCD00000001+11'NAD+EX+ESA78587268:167:148'NAD+DT+ESA78587268:167:148'NAD+AF+ESA78587268:167:148'DTM+148:2011201856:201'DTM+268:20200416:102'GIS+4:117:148'GIS+24:116::A3'GIS+24:119::1'RFF+ABK:20ES009998500102'AUT+A198543129CBF854'DTM+204:2011201856:201'UNT+14+10100507081242'UNZ+1+10100507081242'");
			var responseMessage = CreateNewEDIMessage(ApplicationReference, messageText, InterchangeID, false);

			ProcessMessageForTest(responseMessage);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\">" +
				"<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES009998500102</td></tr>" +
				"</table>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
				"<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A3) Simplified Procedure</td></tr>" +
				"<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>Copy \"A\" of TAD</td></tr>" +
				"</table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, TransitReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = responseMessage.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryNum, ClearanceReferenceNumber);
			queryCLR.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberClearance = responseMessage.Factory.Load<CusEntryNumber>(queryCLR).Single();

			AssertNCTSDepartureGeneric(responseMessage, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, departureMovementHeader: nctsHeader.MovementHeader, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, mrnEntryStatus: "4", mrnIssueDate: AdmissionDate, clearanceIssueDate: AdmissionDate, mrnEntryNum: TransitReferenceNumber, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceExpiryDate: ExpiryDate, clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);
			AssertEquals("ValuationDate", nctsHeader.MovementReferenceEntryNumber.CE_IssueDate, nctsHeader.MovementHeader.BM_ValuationDate);
		}

		public void TestProcessMessageAcceptedGreenCircuit()
		{
			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\">" +
				"<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES009998500102</td></tr>" +
				"</table>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>A198543129CBF854</td></tr>" +
				"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
				"<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A3) Simplified Procedure</td></tr>" +
				"<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>Copy \"A\" of TAD</td></tr>" +
				"</table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, TransitReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryNum, ClearanceReferenceNumber);
			queryCLR.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberClearance = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

			AssertNCTSDepartureGeneric(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, departureMovementHeader: nctsHeader.MovementHeader, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, mrnEntryStatus: "4", mrnIssueDate: AdmissionDate, clearanceIssueDate: AdmissionDate, mrnEntryNum: TransitReferenceNumber, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceExpiryDate: ExpiryDate, clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);
		}

		public void TestProcessMessageAcceptedRedCircuit()
		{
			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "5",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ZString.Empty,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			var expectedMessageInterpretation = "<H3>Accepted Declaration</H3>" +
				"<table border=\"0\">" +
				"<tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
				"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>20ES009998500102</td></tr>" +
				"</table>" +
				"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F00000\">RED</font></strong></td></tr></table>" +
				"<br><table border=\"0\"><tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 18:56:00</td></tr>" +
				"<tr><td>Clearance Criteria:</td><td>&nbsp;&nbsp;</td><td>(A3) Simplified Procedure</td></tr>" +
				"<tr><td>Print Procedure:</td><td>&nbsp;&nbsp;</td><td>Copy \"A\" of TAD</td></tr>" +
				"</table>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, TransitReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			AssertNCTSDepartureGeneric(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, departureMovementHeader: nctsHeader.MovementHeader, cusEntryNumberMRN: cusEntryNumberMRN, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsNotReleasedForTransit, mrnIssueDate: AdmissionDate, clearanceReferenceNumber: "", mrnEntryNum: TransitReferenceNumber);
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
		{
			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			Factory.Save();

			ProcessMessageAndSave();
			AssertNCTSDepartureGeneric(message, nctsHeader, departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);

			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 1, docMessages.Length);
			AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (TransitReferenceNumber + "_NCTS_AEAT_TAD.pdf", TransitReferenceNumber) });
		}

		public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
		{
			var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], TransitReferenceNumber + "_NCTS_AEAT_TAD.pdf", "CAU");
			docManagerInfo.Save();

			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			Factory.Save();

			ProcessMessageAndSave();
			AssertNCTSDepartureGeneric(message, nctsHeader, departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);

			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		}

		public void TestMessageProcessingErrorHeaderNotDeparture()
		{
			var nctsHeaderArrival = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeaderArrival.BH_HeaderType = NctsMovementType.Codes.Arrival;

			message.EM_LinkedObject = nctsHeaderArrival;

			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);
			var expectedMessageInterpretation = string.Format("<H3>Processor Failure</H3><br>" +
						"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
						"<H4>Exception: Header type is A so can't process Departure response message</H4>", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference);

			AssertNCTSDepartureGeneric(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: "AAA", emStatus: "FAL", messageStatus: "", departureMovementHeader: nctsHeader.MovementHeader, esNctsHeaderTADPrintProcedure: "", esNctsHeaderClearanceCriteria: "", movementReferenceNumber: "", clearanceReferenceNumber: "", commonCustomsStatus: "");
			AssertContains("Log has error", "Header type is A so can't process Departure response message", GetAllConcatenatedUserLogStrings());
		}

		public void TestProcessMessageAcceptedGreenCircuit_PendingTransactionsAsConfirmed()
		{
			SetSentInterchange(nctsHeader, InterchangeID);

			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			var guaranteeHeader = SetUpGuaranteeTransactions();

			processor.ProcessMessage(message);
			AssertNCTSDepartureGeneric(message, nctsHeader, departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);

			CombineAssertions(() =>
			{
				var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
				AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
				AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

				var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
				AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
				AssertEquals("Second transaction's reference has been changed since it was pending", TransitReferenceNumber, secondTransaction.CPL_Reference);

				var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
				AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
				AssertEquals("Third transaction's reference has been changed since it was pending", TransitReferenceNumber, thirdTransaction.CPL_Reference);
			});
		}

		public void TestProcessMessageAcceptedRedCircuit_PendingTransactionsAsConfirmed()
		{
			SetSentInterchange(nctsHeader, InterchangeID);

			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "5",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ZString.Empty,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			var guaranteeHeader = SetUpGuaranteeTransactions();

			processor.ProcessMessage(message);
			AssertNCTSDepartureGeneric(message, nctsHeader, departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsNotReleasedForTransit, clearanceReferenceNumber: "");

			CombineAssertions(() =>
			{
				var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
				AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
				AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

				var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
				AssertEquals("Second transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, secondTransaction.CPL_TransactionStatus);
				AssertEquals("Second transaction's reference has been changed since it was pending", TransitReferenceNumber, secondTransaction.CPL_Reference);

				var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
				AssertEquals("Third transaction is now confirmed", PermitTransactionStatusList.Codes.Confirmed, thirdTransaction.CPL_TransactionStatus);
				AssertEquals("Third transaction's reference has been changed since it was pending", TransitReferenceNumber, thirdTransaction.CPL_Reference);
			});
		}

		public void TestProcessMessageRejectedDeclaration_PendingTransactionsAsDeleted()
		{
			SetSentInterchange(nctsHeader, InterchangeID);

			var mockMessageProcessorData = new MockResponseProvider()
			{
				MessageName = "963",
				AdmissionDate = ZDateTime.Empty,
				MessageFunction = "2",
				ErrorList = new List<ErrorMessage> { SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text") }
			};

			processor = GetMockedProcessor(GetMockedCUSRESMessageProvider(mockMessageProcessorData).Object);

			var guaranteeHeader = SetUpGuaranteeTransactions();

			processor.ProcessMessage(message);
			AssertNCTSDepartureGeneric(message, nctsHeader, messageSubType: "REJ", departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.DeclarationRejected, esNctsHeaderClearanceCriteria: "", esNctsHeaderTADPrintProcedure: "", movementReferenceNumber: "", clearanceReferenceNumber: "");

			CombineAssertions(() =>
			{
				var firstTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("First-CON"));
				AssertEquals("First transaction is still confirmed", PermitTransactionStatusList.Codes.Confirmed, firstTransaction.CPL_TransactionStatus);
				AssertEquals("First transaction's reference has not been changed since it was already confirmed", ApplicationReference, firstTransaction.CPL_Reference);

				var secondTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Second-PEN"));
				AssertEquals("Second transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, secondTransaction.CPL_TransactionStatus);
				AssertEquals("Second transaction's reference has not been changed since it was set to DEL", ApplicationReference, secondTransaction.CPL_Reference);

				var thirdTransaction = Factory.LoadTop1<BaseCusPermitLineTransaction>(GetTransactionQuery("Third-PEN"));
				AssertEquals("Third transaction is now deleted", PermitTransactionStatusList.Codes.Deleted, thirdTransaction.CPL_TransactionStatus);
				AssertEquals("Third transaction's reference has not been changed since it was set to DEL", ApplicationReference, thirdTransaction.CPL_Reference);
			});
		}

		public void TestProcessMessageAcceptedDeleteSecurityDataWhenCheckFalse()
		{
			nctsHeader.BH_FTZMove = false;
			nctsHeader.AddSecurityData();
			nctsHeader.Factory.Save();

			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);
			AssertNCTSDepartureGeneric(message, nctsHeader, departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);
		}

		public void TestProcessMessageAcceptedDeleteSecurityDataNullControlWhenCheckFalse()
		{
			nctsHeader.BH_FTZMove = false;
			nctsHeader.MovementHeader.BM_BTAIndicator = "S";
			nctsHeader.Factory.Save();

			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);
			AssertNCTSDepartureGeneric(message, nctsHeader, departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNumber: ClearanceReferenceNumber, clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);
		}

		public void TestProcessMessageAcceptedNotDeleteSecurityDataWhenCheckTrue()
		{
			nctsHeader.BH_FTZMove = true;
			nctsHeader.AddSecurityData();
			nctsHeader.Factory.Save();

			var mockMessageProcessorData = new MockResponseProviderDeparture()
			{
				MessageName = "962",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				CustomsClearanceCriteria = "A3",
				PrintActionRequired = "1",
				RegistrationNumber = TransitReferenceNumber,
				CSVReleaseCode = ClearanceReferenceNumber,
				CSVReleaseCreationDate = AdmissionDate,
				TransitMaxDate = ExpiryDate,
				ErrorList = null
			};

			SetSentInterchange(nctsHeader, InterchangeID);
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);
			AssertNCTSDepartureGeneric(message, nctsHeader, departureMovementHeader: nctsHeader.MovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture, clearanceReferenceNumber: ClearanceReferenceNumber, headerFTZMove: true, placeOfUnloadingCode: "UNLOD", headerUniqueVoyageIdentifier: "112233", securityConsignorE2_OA_Address: nctsHeader.SecurityConsignor.E2_OA_Address, securityConsigneeE2_OA_Address: nctsHeader.SecurityConsignee.E2_OA_Address, headerOH_Carrier: nctsHeader.BH_OH_Carrier, headerItineraty: 6, commonBTAIndicator: "S", commonMethodOfPayment: "P", commonAdditionalText: "7", commonConveyanceNumber: "S7", clearanceDate: AdmissionDate, arrivalLimit: ExpiryDate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		}

		NctsHeader ProcessMessageAndSave()
		{
			var newFactoryMessage = Factory.Load<TestEdiMessage>(message.PK);
			var newFactoryEntryHeader = Factory.Load<NctsHeader>(nctsHeader.PK);

			processor.ProcessMessage(newFactoryMessage);

			newFactoryEntryHeader.Factory.Save();

			nctsHeader.Messages.Reload(true);

			return newFactoryEntryHeader;
		}

		INctsDepartureAndTIRResponseMessageProvider GetMockedProvider(MockResponseProviderDeparture providerData)
		{
			var mockTestHelper = GetMockedCUSRESMessageProvider(providerData);
			mockTestHelper.Setup(m => m.CustomsClearanceCriteria).Returns(providerData.CustomsClearanceCriteria);
			mockTestHelper.Setup(m => m.PrintActionRequired).Returns(providerData.PrintActionRequired);
			mockTestHelper.Setup(m => m.RegistrationNumber).Returns(providerData.RegistrationNumber);
			mockTestHelper.Setup(m => m.CSVReleaseCode).Returns(providerData.CSVReleaseCode);
			mockTestHelper.Setup(m => m.CSVReleaseCreationDate).Returns(providerData.CSVReleaseCreationDate);
			mockTestHelper.Setup(m => m.TransitMaxDate).Returns(providerData.TransitMaxDate);
			return mockTestHelper.Object;
		}

		public void AssertNCTSDepartureGeneric(TestEdiMessage message, NctsHeader nctsHeader, string emStatus = EDIMessage.Status.Received, string expectedMessageInterpretation = "", string messageSubType = "ACC", string messageStatus = EDIMessage.Status.Received, CusEntryNumber cusEntryNumberMRN = null, CusEntryNumber cusEntryNumberClearance = null, string esNctsHeaderClearanceCriteria = "A3", string esNctsHeaderTADPrintProcedure = "1", NctsDepartureMovementHeader departureMovementHeader = null, string commonCustomsStatus = null, string mrnEntryStatus = "5", ZDateTime? mrnIssueDate = null, ZDateTime? clearanceIssueDate = null, string mrnEntryNum = "", string clearanceReferenceNumber = null, string movementReferenceNumber = TransitReferenceNumber, ZDateTime? clearanceExpiryDate = null, bool headerFTZMove = false, string placeOfUnloadingCode = "", string headerUniqueVoyageIdentifier = "", ZGuid? securityConsignorE2_OA_Address = null, ZGuid? securityConsigneeE2_OA_Address = null, ZGuid? headerOH_Carrier = null, int headerItineraty = 0, string commonBTAIndicator = "", string commonMethodOfPayment = "", string commonAdditionalText = "", string commonConveyanceNumber = "", ZDateTime? clearanceDate = null, ZDateTime? arrivalLimit = null)
		{
			GenericCommonAssertProcessEntryDataNCTS(message, nctsHeader: nctsHeader, securityConsignorE2_OA_Address: securityConsignorE2_OA_Address, securityConsigneeE2_OA_Address: securityConsigneeE2_OA_Address, headerOH_Carrier: headerOH_Carrier, emStatus: emStatus, esNctsHeaderClearanceCriteria: esNctsHeaderClearanceCriteria, esNctsHeaderTADPrintProcedure: esNctsHeaderTADPrintProcedure, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberClearance: cusEntryNumberClearance, departureMovementHeader: departureMovementHeader, esNctsHeader: nctsHeader.ESNctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, messageStatus: messageStatus, commonCustomsStatus: commonCustomsStatus, mrnEntryStatus: mrnEntryStatus, mrnIssueDate: mrnIssueDate, clearanceIssueDate: clearanceIssueDate, mrnEntrynum: mrnEntryNum, clearanceReferenceNumber: clearanceReferenceNumber, movementReferenceNumber: movementReferenceNumber, clearanceExpiryDate: clearanceExpiryDate, headerFTZMove: headerFTZMove, placeOfUnloadingCode: placeOfUnloadingCode, headerUniqueVoyageIdentifier: headerUniqueVoyageIdentifier, headerItineraty: headerItineraty, commonBTAIndicator: commonBTAIndicator, commonMethodOfPayment: commonMethodOfPayment, commonAdditionalText: commonAdditionalText, commonConveyanceNumber: commonConveyanceNumber, clearanceDate: clearanceDate, arrivalLimit: arrivalLimit);
		}

		class MockResponseProviderDeparture : MockResponseProvider
		{
			public ZString CustomsClearanceCriteria { get; set; }
			public ZString PrintActionRequired { get; set; }
			public ZString RegistrationNumber { get; set; }
			public ZString CSVReleaseCode { get; set; }
			public ZDateTime CSVReleaseCreationDate { get; set; }
			public ZDateTime TransitMaxDate { get; set; }
		}
	}
}
