using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsArrivalDeclarationResponseMessageProcessorTest : ESNCTSEDIResponseMessageProcessorTest<NctsArrivalDeclarationResponseMessageProcessor, INctsArrivalResponseMessageProvider>
	{
		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesTransactions()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			nctsHeader.LocalReferenceNumber = "AH3";
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";

			AddGuarantees(nctsHeader, nctsHeader.MovementReferenceNumber, nctsHeader.LocalReferenceNumber);

			var mockMessageProcessorData = GetOBSResponseProviderArrival(AdmissionDate);

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: message is processed as AWO", NctsTransitStatusList.Codes.GoodsWrittenOff, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);

				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith("Write-off")), 110m, new ZDateTime(2020, 11, 20, 18, 56, 00));

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith("Write-off")), 600m, new ZDateTime(2020, 11, 20, 18, 56, 00));
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesTransactions_PositiveAmount()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";

			SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.TRA, nctsHeader.MovementReferenceNumber, ApplicationReference, 500m, 1000, 1300, true, false);
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = GuaranteeReference;
			guarantee.PW_BondAmount = 1000m;

			var mockMessageProcessorData = GetOBSResponseProviderArrival(AdmissionDate);

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			var expectedError = "Reference 16ESAGL9990000096 has a positive balance of 440 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
			var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
			AssertContains("logger", expectedError, concatenatedUserLogStrings);
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesTransactions_DifferentJob()
		{
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
			nctsHeader.LocalReferenceNumber = "AZ";

			var nctsHeaderDeparture = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderDeparture.LocalReferenceNumber = "AH3";
			nctsHeaderDeparture.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";

			AddGuarantees(nctsHeaderDeparture, nctsHeaderDeparture.MovementReferenceNumber, nctsHeader.LocalReferenceNumber);

			var mockMessageProcessorData = GetOBSResponseProviderArrival(ZDateTime.Empty);

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("Prereq: message is processed as AWO", NctsTransitStatusList.Codes.GoodsWrittenOff, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);

				var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

				var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for first guarantee", 14, guarantee1Transactions.Count());
				AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith("Write-off")), 55m, new ZDateTime(2021, 10, 05, 09, 36, 0));

				var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
				AssertEquals("Original transactions + 1 new transaction for second guarantee", 14, guarantee2Transactions.Count());
				AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith("Write-off")), 300m, new ZDateTime(2021, 10, 05, 09, 36, 0));
			});
		}

		[TestDate(2021, 10, 05, 09, 36, 0)]
		public void TestAddGuaranteesTransactions_NoPendingAmount()
		{
			nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";

			var nctsHeaderDeparture = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderDeparture.LocalReferenceNumber = "AH3";
			nctsHeaderDeparture.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeaderDeparture.MovementReferenceEntryNumber.CE_EntryNum = "MRN123";
			nctsHeaderDeparture.UnloadingRemark.G9_UnloadingDate = ZDate.BrettsBirthday;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = GuaranteeReference;
			guaranteeHeader.CPH_OH_PermitHolder = org1.PK;
			guaranteeHeader.CPH_StartDate = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_EndDate = new ZDate(2022, 7, 15);
			guaranteeHeader.CPH_SystemCreateTimeUtc = new ZDate(2020, 7, 15);
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_RN_NKCountryCode = EsCode;
			guaranteeHeader.CPH_Balance = 0;

			AddOBLTransaction(guaranteeHeader, 1000m);
			AddTransaction(guaranteeHeader, "MRN123", ApplicationReference, "First-CON", 0, PermitTransactionStatusList.Codes.Confirmed);

			Factory.Save();
			var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = GuaranteeReference;
			guarantee.PW_BondAmount = 0m;

			var mockMessageProcessorData = GetOBSResponseProviderArrival(AdmissionDate);

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("3 Original transactions, no transactions added", 4, guarantee1Transactions.Count());
		}

		void AssertNewTransaction(string message, SharedCusPermitLineTransaction transaction, ZDecimal amount, ZDateTime acceptanceDate)
		{
			AssertEquals(message + ".CPL_Reference", "MRN123", transaction.CPL_Reference);
			AssertEquals(message + ".CPL_Comment", "Write-off NCTS Departure AH3", transaction.CPL_Comment);
			AssertEquals(message + ".CPL_TranValue", amount, transaction.CPL_TranValue);
			AssertEquals(message + ".CPL_TransactionDate", acceptanceDate, transaction.CPL_TransactionDate);
			AssertEquals(message + ".CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
		}

		MockResponseProviderArrival GetOBSResponseProviderArrival(ZDateTime admissionDate)
		{
			return new MockResponseProviderArrival()
			{
				MessageName = "OBS",
				AdmissionDate = admissionDate,
				MessageFunction = "4",
				PreviousSummaryDiscrepancy = "2",
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber
			};
		}

		public void TestProcessMessageAcceptedAVIGreenCircuitNotMocked()
		{
			SetSentInterchange(nctsHeader, InterchangeID);
			var messageText = ZString.Format("UNH+20185637975246+CUSRES:D:96B:UN:AVI006'BGM+AVI+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+1:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20185637975246'UNZ+1+20185637975246'");
			var responseMessage = CreateNewEDIMessage(ApplicationReference, messageText, InterchangeID, false);

			ProcessMessageForTest(responseMessage);

			var expectedMessageInterpretation = "<H3>Accepted Declaration AVI</H3>" +
					"<H4>Acceptance: 20-11-2020 18:56</H4>" +
					"<H4>Circuit: <strong><font color=\"#64AF00\">GREEN</font></strong></H4>" +
					"<H4>** No deviations between Transit and Previous Summary **</H4>" +
					"<H4>Summary Decl: 99980000521</H4>" +
					"<H4>MRN AVI: 20ES009998500102</H4>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ArrivalReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = responseMessage.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var querySummaryCode = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberSummary = responseMessage.Factory.Load<CusEntryNumber>(querySummaryCode).Single();

			AssertNCTSArrival(responseMessage, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberSummary: cusEntryNumberSummary, commonCustomsStatus: NctsTransitStatusList.Codes.UnloadingPermissionGranted, mrnIssueDate: AdmissionDate, summaryEntryNum: SummaryReferenceNumber, summaryEntryType: "SUM", mrnEntryNum: TransitReferenceNumber);
		}

		public void TestProcessMessageAcceptedAVIGreenCircuit()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "AVI",
				MessageFunction = "4",
				PreviousSummaryDiscrepancy = "1",
				AdmissionDate = AdmissionDate,
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration AVI</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#64AF00\">GREEN</font></strong></H4>" +
				"<H4>** No deviations between Transit and Previous Summary **</H4>" +
				"<H4>Summary Decl: 99980000521</H4>" +
				"<H4>MRN AVI: 20ES009998500102</H4>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ArrivalReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var querySummaryCode = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberSummary = message.Factory.Load<CusEntryNumber>(querySummaryCode).Single();

			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberSummary: cusEntryNumberSummary, commonCustomsStatus: NctsTransitStatusList.Codes.UnloadingPermissionGranted, mrnIssueDate: AdmissionDate, summaryEntryNum: SummaryReferenceNumber, summaryEntryType: "SUM", mrnEntryNum: TransitReferenceNumber);
		}

		public void TestProcessMessageAcceptedOBSGreenCircuitHolderAuthorized()
		{
			var mockMessageProcessorData = GetOBSResponseProviderArrival(AdmissionDate);

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);
			var expectedMessageInterpretation =
				"<H3>Accepted Declaration OBS</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#64AF00\">GREEN</font></strong></H4>" +
				"<H4>** Deviation in packages quantity between Transit and Previous Summary **</H4>" +
				"<H4>Summary Decl: 99980000521</H4>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ArrivalReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var querySummaryCode = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberSummary = message.Factory.Load<CusEntryNumber>(querySummaryCode).Single();

			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberSummary: cusEntryNumberSummary, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsWrittenOff, mrnIssueDate: AdmissionDate, summaryEntryNum: SummaryReferenceNumber, summaryEntryType: "SUM", mrnEntryNum: TransitReferenceNumber);
		}

		public void TestProcessMessageAcceptedOBSGreenCircuitHolderNotAuthorized_WrongType()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "OBS",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				PreviousSummaryDiscrepancy = "1",
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber,
				ErrorList = null
			};
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

			processor.ProcessMessage(message);
			var expectedMessageInterpretation =
				"<H3>Accepted Declaration OBS</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#64AF00\">GREEN</font></strong></H4>" +
				"<H4>** No deviations between Transit and Previous Summary **</H4>" +
				"<H4>Summary Decl: 99980000521</H4>";
			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: initialCustomsStatus);
		}

		public void TestProcessMessageAcceptedOBSGreenCircuitHolderNotAuthorized_WrongHolder()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "OBS",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				PreviousSummaryDiscrepancy = "1",
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			authorisationHeader.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;

			processor.ProcessMessage(message);
			var expectedMessageInterpretation =
				"<H3>Accepted Declaration OBS</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#64AF00\">GREEN</font></strong></H4>" +
				"<H4>** No deviations between Transit and Previous Summary **</H4>" +
				"<H4>Summary Decl: 99980000521</H4>";
			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: initialCustomsStatus);
		}

		public void TestProcessMessageAcceptedOBSGreenCircuitHolderNotAuthorized_NoAuthorizations()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "OBS",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				PreviousSummaryDiscrepancy = "1",
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			authorisationHeader.Delete();

			processor.ProcessMessage(message);
			var expectedMessageInterpretation =
				"<H3>Accepted Declaration OBS</H3>" +
					"<H4>Acceptance: 20-11-2020 18:56</H4>" +
					"<H4>Circuit: <strong><font color=\"#64AF00\">GREEN</font></strong></H4>" +
					"<H4>** No deviations between Transit and Previous Summary **</H4>" +
					"<H4>Summary Decl: 99980000521</H4>";
			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: initialCustomsStatus);
		}

		public void TestProcessMessageAcceptedAVIRedCircuit()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "AVI",
				AdmissionDate = AdmissionDate,
				MessageFunction = "5",
				PreviousSummaryDiscrepancy = "4",
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);
			var expectedMessageInterpretation =
				"<H3>Accepted Declaration AVI</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#F00000\">RED</font></strong></H4>" +
				"<H4>** Deviation in gross weight  and packages quantity between Transit and Previous Summary **</H4>" +
				"<H4>Summary Decl: 99980000521</H4>" +
				"<H4>MRN AVI: 20ES009998500102</H4>";

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ArrivalReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

			var querySummaryCode = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.SummaryEntryNumber);
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			querySummaryCode.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var cusEntryNumberSummary = message.Factory.Load<CusEntryNumber>(querySummaryCode).Single();

			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberSummary: cusEntryNumberSummary, commonCustomsStatus: NctsTransitStatusList.Codes.GoodsUnderCustomsControl, mrnEntryStatus: "5", mrnIssueDate: AdmissionDate, summaryEntryNum: SummaryReferenceNumber, summaryEntryType: "SUM", mrnEntryNum: TransitReferenceNumber);
		}

		public void TestProcessMessageAcceptedOBSRedCircuitNotAuthorized_WrongType()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "OBS",
				AdmissionDate = AdmissionDate,
				MessageFunction = "5",
				PreviousSummaryDiscrepancy = "3",
				TransitReferenceNumber = ZString.Empty,
				SummaryReferenceNumber = ZString.Empty,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;

			processor.ProcessMessage(message);
			var expectedMessageInterpretation =
				"<H3>Accepted Declaration OBS</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#F00000\">RED</font></strong></H4>" +
				"<H4>** Deviation in gross weight  between Transit and Previous Summary **</H4>";
			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: initialCustomsStatus);
		}

		public void TestProcessMessageAcceptedArrivalNotChangeMRN()
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999950012811";
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Before Arrival Processing", "20ES00999950012811", nctsHeader.MovementReferenceNumber);

				SetSentInterchange(nctsHeader, InterchangeID);
				var messageTextA = ZString.Format("UNH+20205637975246+CUSRES:D:96B:UN:AVI006'BGM+AVI+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+1:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500152'RFF+AFB:99980000521'UNT+8+20205637975246'UNZ+1+20205637975246'");
				var responseMessageA = CreateNewEDIMessage(ApplicationReference, messageTextA, InterchangeID, false);

				ProcessMessageForTest(responseMessageA);

				AssertEquals("After Arrival Processing", "20ES00999950012811", nctsHeader.MovementReferenceNumber);

				var queryMRND = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				queryMRND.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
				queryMRND.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
				var quantityD = responseMessageA.Factory.Load<CusEntryNumber>(queryMRND).Length;
				AssertEquals("Quantity of CusEntryNum with MRN type", 1, quantityD);
				var cusEntryNumberMRND = responseMessageA.Factory.Load<CusEntryNumber>(queryMRND).Single();

				AssertEquals("MRN CE_EntryNum", "20ES00999950012811", cusEntryNumberMRND.CE_EntryNum);

				var queryMRNA = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ArrivalReferenceNumber);
				queryMRNA.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
				queryMRNA.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
				var quantityA = responseMessageA.Factory.Load<CusEntryNumber>(queryMRNA).Length;
				AssertEquals("Quantity of CusEntryNum with ARN type", 1, quantityA);
				var cusEntryNumberMRNA = responseMessageA.Factory.Load<CusEntryNumber>(queryMRNA).Single();

				AssertEquals("ARN CE_EntryNum", "20ES009998500152", cusEntryNumberMRNA.CE_EntryNum);
			});
		}

		public void TestProcessMessageAcceptedOBSRedCircuitNotAuthorized_WrongHolder()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "OBS",
				AdmissionDate = AdmissionDate,
				MessageFunction = "5",
				PreviousSummaryDiscrepancy = "3",
				TransitReferenceNumber = ZString.Empty,
				SummaryReferenceNumber = ZString.Empty,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			authorisationHeader.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;

			processor.ProcessMessage(message);
			var expectedMessageInterpretation =
				"<H3>Accepted Declaration OBS</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#F00000\">RED</font></strong></H4>" +
				"<H4>** Deviation in gross weight  between Transit and Previous Summary **</H4>";
			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: initialCustomsStatus);
		}

		public void TestProcessMessageAcceptedOBSRedCircuitNotAuthorized_NoAuthorizations()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "OBS",
				AdmissionDate = AdmissionDate,
				MessageFunction = "5",
				PreviousSummaryDiscrepancy = "3",
				TransitReferenceNumber = ZString.Empty,
				SummaryReferenceNumber = ZString.Empty,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			authorisationHeader.Delete();

			processor.ProcessMessage(message);

			var expectedMessageInterpretation =
				"<H3>Accepted Declaration OBS</H3>" +
				"<H4>Acceptance: 20-11-2020 18:56</H4>" +
				"<H4>Circuit: <strong><font color=\"#F00000\">RED</font></strong></H4>" +
				"<H4>** Deviation in gross weight  between Transit and Previous Summary **</H4>";
			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: initialCustomsStatus);
		}

		public void TestMessageProcessingErrorHeaderNotArrival()
		{
			var nctsHeaderDeparture = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderDeparture.BH_HeaderType = NctsMovementType.Codes.Departure;

			message.EM_LinkedObject = nctsHeaderDeparture;

			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "AVI",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				PreviousSummaryDiscrepancy = "1",
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber,
				ErrorList = null
			};

			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));

			processor.ProcessMessage(message);

			var expectedMessageInterpretation = string.Format(
				"<H3>Processor Failure</H3><br>" +
				"<H4>Failure: Unable to read message text from message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to Failed.</H4>" +
				"<H4>Exception: Header type is D so can't process Arrival response message</H4>",
				message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference);

			AssertNCTSArrival(message, nctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, emStatus: "FAL", messageSubType: "AAA", messageStatus: "", arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: initialCustomsStatus);
			AssertContains("Log has error", "Header type is D so can't process Arrival response message", GetAllConcatenatedUserLogStrings());
		}

		public void TestProcessMessageAcceptedAVIAddNewCusEntryNum()
		{
			SetSentInterchange(nctsHeader, InterchangeID);
			var messageText = ZString.Format("UNH+20205637975246+CUSRES:D:96B:UN:AVI006'BGM+AVI+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+1:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20205637975246'UNZ+1+20205637975246'");
			var responseMessage = CreateNewEDIMessage(ApplicationReference, messageText, InterchangeID, false);

			ProcessMessageForTest(responseMessage);

			var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ArrivalReferenceNumber);
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
			queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
			var quantity = responseMessage.Factory.Load<CusEntryNumber>(queryMRN).Length;
			AssertEquals("Quantity of CusEntryNum with ARN type", 1, quantity);

			var cusEntryNumberMRN = responseMessage.Factory.Load<CusEntryNumber>(queryMRN).Single();
			AssertNCTSArrival(message, nctsHeader, messageSubType: "AAA", arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, emStatus: "QUE", cusEntryNumberMRN: cusEntryNumberMRN, commonCustomsStatus: NctsTransitStatusList.Codes.UnloadingPermissionGranted, mrnIssueDate: AdmissionDate, mrnEntryNum: TransitReferenceNumber);
		}

		public void TestProcessMessageRejectedAVI()
		{
			var error1 = SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text");

			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "963",
				AdmissionDate = ZDateTime.Empty,
				MessageFunction = "2",
				ErrorList = new List<ErrorMessage> { error1 }
			};
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));
			processor.ProcessMessage(message);

			AssertNCTSArrival(message, nctsHeader, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.DeclarationRejected, messageSubType: "REJ", messageStatus: NctsMessageStatusList.Codes.ArrivalNotificationRejected);
		}

		public void TestProcessMessageRejectedAVO()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.CombinedMessage = true;
			nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			var error1 = SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text");

			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "963",
				AdmissionDate = ZDateTime.Empty,
				MessageFunction = "2",
				ErrorList = new List<ErrorMessage> { error1 }
			};
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));
			processor.ProcessMessage(message);

			AssertNCTSArrival(message, nctsHeader, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.DeclarationRejected, messageSubType: "REJ", messageStatus: NctsMessageStatusList.Codes.ArrivalNotificationRejected);
		}

		public void TestProcessMessageRejectedTNA()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			var error1 = SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text");

			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "963",
				AdmissionDate = ZDateTime.Empty,
				MessageFunction = "2",
				ErrorList = new List<ErrorMessage> { error1 }
			};
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));
			processor.ProcessMessage(message);

			AssertNCTSArrival(message, nctsHeader, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.DeclarationRejected, messageSubType: "REJ", messageStatus: NctsMessageStatusList.Codes.ArrivalNotificationRejected, differentCommonCustomsStatus: true);
		}

		public void TestProcessMessageRejectedTAO()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			nctsHeader.CombinedMessage = true;
			nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			var error1 = SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text");

			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "963",
				AdmissionDate = ZDateTime.Empty,
				MessageFunction = "2",
				ErrorList = new List<ErrorMessage> { error1 }
			};
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));
			processor.ProcessMessage(message);

			AssertNCTSArrival(message, nctsHeader, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.DeclarationRejected, messageSubType: "REJ", messageStatus: NctsMessageStatusList.Codes.ArrivalNotificationRejected, differentCommonCustomsStatus: true);
		}

		public void TestProcessMessageRejectedOBSPreviousAVIAceptance()
		{
			var mockMessageProcessorData = new MockResponseProviderArrival()
			{
				MessageName = "AVI",
				AdmissionDate = AdmissionDate,
				MessageFunction = "4",
				PreviousSummaryDiscrepancy = "1",
				TransitReferenceNumber = TransitReferenceNumber,
				SummaryReferenceNumber = SummaryReferenceNumber,
				ErrorList = null
			};
			processor = GetMockedProcessor(GetMockedProvider(mockMessageProcessorData));
			processor.ProcessMessage(message);

			AssertNCTSArrival(message, nctsHeader, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, commonCustomsStatus: NctsTransitStatusList.Codes.UnloadingPermissionGranted);

			var error1 = SetUpError("55110", "CABECERA.Other Text", "El estado del Tránsito impide la operación..More Text");
			var error2 = SetUpError("55570", "CABECERA.TIR CARNET (CAS. 1).", "Cuaderno TIR incorrecto.");
			mockMessageProcessorData.MessageFunction = "2";
			mockMessageProcessorData.MessageName = "963";
			mockMessageProcessorData.AdmissionDate = new ZDateTime(2021, 07, 29, 18, 56, 00);
			mockMessageProcessorData.PreviousSummaryDiscrepancy = "3";
			mockMessageProcessorData.TransitReferenceNumber = ZString.Empty;
			mockMessageProcessorData.SummaryReferenceNumber = ZString.Empty;
			mockMessageProcessorData.ErrorList = new List<ErrorMessage> { error1, error2 };

			processor = GetMockedProcessor(GetMockedCUSRESMessageProvider(mockMessageProcessorData).Object);
			processor.ProcessMessage(message);

			AssertNCTSArrival(message, nctsHeader, messageSubType: "REJ", arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, emStatus: EDIMessage.Status.Received, messageStatus: NctsMessageStatusList.Codes.UnloadingRemarksRejected, commonCustomsStatus: NctsTransitStatusList.Codes.UnloadingPermissionGranted);
		}

		public void AssertNCTSArrival(TestEdiMessage message, NctsHeader nctsHeader, string emStatus = EDIMessage.Status.Received, string expectedMessageInterpretation = "", string messageSubType = "ACC", string messageStatus = EDIMessage.Status.Received, CusEntryNumber cusEntryNumberMRN = null, CusEntryNumber cusEntryNumberSummary = null, NctsArrivalMovementHeader arrivalMovementHeader = null, string commonCustomsStatus = null, string summaryEntryType = "", string summaryEntryNum = "", string mrnEntryStatus = "4", ZDateTime? mrnIssueDate = null, string mrnEntryNum = "", bool differentCommonCustomsStatus = false, string departureCustomStatus = "")
		{
			GenericCommonAssertProcessEntryDataNCTS(message, nctsHeader: nctsHeader, emStatus: emStatus, cusEntryNumberMRN: cusEntryNumberMRN, cusEntryNumberSummary: cusEntryNumberSummary, arrivalMovementHeader: arrivalMovementHeader, esNctsHeader: nctsHeader.ESNctsHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageSubType: messageSubType, messageStatus: messageStatus, commonCustomsStatus: commonCustomsStatus, summaryEntryNum: summaryEntryNum, summaryEntryType: summaryEntryType, mrnEntryStatus: mrnEntryStatus, mrnIssueDate: mrnIssueDate, mrnEntrynum: mrnEntryNum, differentCommonCustomsStatus: differentCommonCustomsStatus, departureCustomStatus: departureCustomStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = initialCustomsStatus;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.DestinationTrader.OrganisationPK = org.PK;

			authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			authorisationHeader.CPH_OH_PermitHolder = org.PK;
		}
		CusAuthorisationHeader authorisationHeader;

		readonly string initialCustomsStatus = "INI";

		INctsArrivalResponseMessageProvider GetMockedProvider(MockResponseProviderArrival providerData)
		{
			var mockTestHelper = new Mock<INctsArrivalResponseMessageProvider>();
			mockTestHelper.CallBase = true;
			mockTestHelper.Setup(m => m.DocumentMessageName).Returns(providerData.MessageName);
			mockTestHelper.Setup(m => m.AdmissionDate).Returns(providerData.AdmissionDate);
			mockTestHelper.Setup(m => m.MessageFunction).Returns(providerData.MessageFunction);
			mockTestHelper.Setup(m => m.FreeTextErrors).Returns(providerData.ErrorList);

			mockTestHelper.Setup(m => m.PreviousSummaryDiscrepancy).Returns(providerData.PreviousSummaryDiscrepancy);
			mockTestHelper.Setup(m => m.TransitReferenceNumber).Returns(providerData.TransitReferenceNumber);
			mockTestHelper.Setup(m => m.SummaryReferenceNumber).Returns(providerData.SummaryReferenceNumber);
			return mockTestHelper.Object;
		}

		class MockResponseProviderArrival : MockResponseProvider
		{
			public ZString PreviousSummaryDiscrepancy { get; set; }
			public ZString TransitReferenceNumber { get; set; }
			public ZString SummaryReferenceNumber { get; set; }
		}

		protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Arrival Declaration Message Processor";

		protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.NctsArrivalNotification,
																									DeclarationMessageTypeList.Codes.NctsUnloadingRemarks,
																									DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs,
																									DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi,
																									DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb };

		protected override NctsArrivalDeclarationResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new NctsArrivalDeclarationResponseMessageProcessor(logger);

		protected override ZString GetBM_CustomsStatus() => nctsHeader.ArrivalMovementHeader.BM_CustomsStatus;

		const string GuaranteeReference = "16ESAGL9990000096";
		const string OtherGuaranteeReference = "17ESAGL9990000097";

		void AddGuarantees(NctsHeader header, string mrnCode, string boReference)
		{
			SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.TRA, mrnCode, boReference, -50m, 1000, 750, true, true);
			var guarantee = header.GetEffectiveGuarantees().AddNew();
			guarantee.PW_BondNumber = GuaranteeReference;
			guarantee.PW_BondAmount = 1000m;

			SetUpGuarantee(OtherGuaranteeReference, EUGuaranteeTypeList.Codes.TRA, mrnCode, boReference, -540m, 1200, 460, true, true);
			var guarantee2 = header.GetEffectiveGuarantees().AddNew();
			guarantee2.PW_BondNumber = OtherGuaranteeReference;
			guarantee2.PW_BondAmount = 1200m;
		}

		protected override NctsArrivalDeclarationResponseMessageProcessor GetMockedProcessor(ICUSRESMessageProvider messageProvider)
		{
			var nctsArrivalDeclarationResponseMessageProcessorMock = new Mock<NctsArrivalDeclarationResponseMessageProcessorForMockTest>(logger) { CallBase = true };
			nctsArrivalDeclarationResponseMessageProcessorMock.Setup(c => c.GetMessageProviderForTest).Returns((INctsArrivalResponseMessageProvider)messageProvider);
			return nctsArrivalDeclarationResponseMessageProcessorMock.Object;
		}

		public class NctsArrivalDeclarationResponseMessageProcessorForMockTest : NctsArrivalDeclarationResponseMessageProcessor
		{
			public NctsArrivalDeclarationResponseMessageProcessorForMockTest(LoggingInformation logger) : base(logger)
			{
			}

			public virtual INctsArrivalResponseMessageProvider GetMessageProviderForTest
			{
				get;
			}
			protected override INctsArrivalResponseMessageProvider GetMessageProviderCore(EDIMessage message) => GetMessageProviderForTest;
		}
	}
}
