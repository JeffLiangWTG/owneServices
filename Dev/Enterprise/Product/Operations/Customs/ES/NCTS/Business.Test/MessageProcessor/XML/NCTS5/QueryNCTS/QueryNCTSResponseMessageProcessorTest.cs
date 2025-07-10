using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTRAC_v515.CCTRACV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

public class QueryNCTSResponseMessageProcessorTest : NCTS5CommonResponseMessageProcessorTest<QueryNCTSResponseMessageProcessor, QueryNCTSMessagePrettyFormatter, Cctracv1Sal>
{
	public void TestProcessAcceptedMessageDeparture_StatusPA() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PA - Pre-Declaration", GuaranteeDataNotWrittenOff), GetAcceptanceTestFilePA(), ESNCTS5DepartureCustomsStatusList.Codes.PreLodged);

	public void TestProcessAcceptedMessageDeparture_StatusPG() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PG - Pending Guarantee", GuaranteeDataNotWrittenOff), GetAcceptanceTestFilePG(), ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance);

	public void TestProcessAcceptedMessageDeparture_StatusPD() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PD - Pending Dispatch", GuaranteeDataNotWrittenOff), GetAcceptanceTestFilePD(), ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl);

	public void TestProcessAcceptedMessageDeparture_StatusDE() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("DE - Dispatch", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileDE(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusCA() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("CA - Declaration Canceled", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileCA(), ESNCTS5DepartureCustomsStatusList.Codes.Cancelled);

	public void TestProcessAcceptedMessageDeparture_StatusPI() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("PI - Pre-Declaration Invalidated", GuaranteeDataNotWrittenOff), GetAcceptanceTestFilePI(), ESNCTS5DepartureCustomsStatusList.Codes.Invalidated);

	public void TestProcessAcceptedMessageDeparture_StatusIG() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("IG - Invalidated by Guarantee", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileIG(), ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid);

	public void TestProcessAcceptedMessageDeparture_StatusIV() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("IV - Invalidated", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileIV(), ESNCTS5DepartureCustomsStatusList.Codes.Invalidated);

	public void TestProcessAcceptedMessageDeparture_StatusNL() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("NL - Not Cleared", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileNL(), ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusRQ() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RQ - Requesting to the country of departure", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileRQ(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusRZ() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RZ - Deviation Rejected By UE", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileRZ(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusRE() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RE - Received", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileRE(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusLI() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("LI - Liquidation Initiated", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileLI(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusUC() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UC - Ultimated For Payment", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileUC(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusRD() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RD - Pending Resolution Of Discrepancy", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileRD(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageDeparture_StatusUL() => ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UL - Completed", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileUL(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

	public void TestProcessAcceptedMessageArrival_StatusPA() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PA - Pre-Declaration"), GetAcceptanceTestFilePA());

	public void TestProcessAcceptedMessageArrival_StatusPG() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PG - Pending Guarantee"), GetAcceptanceTestFilePG());

	public void TestProcessAcceptedMessageArrival_StatusPD() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PD - Pending Dispatch"), GetAcceptanceTestFilePD(), ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl);

	public void TestProcessAcceptedMessageArrival_StatusDE() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("DE - Dispatch"), GetAcceptanceTestFileDE());

	public void TestProcessAcceptedMessageArrival_StatusCA() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("CA - Declaration Canceled"), GetAcceptanceTestFileCA());

	public void TestProcessAcceptedMessageArrival_StatusPI() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("PI - Pre-Declaration Invalidated"), GetAcceptanceTestFilePI());

	public void TestProcessAcceptedMessageArrival_StatusIG() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("IG - Invalidated by Guarantee"), GetAcceptanceTestFileIG());

	public void TestProcessAcceptedMessageArrival_StatusIV() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("IV - Invalidated"), GetAcceptanceTestFileIV());

	public void TestProcessAcceptedMessageArrival_StatusNL() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("NL - Not Cleared"), GetAcceptanceTestFileNL());

	public void TestProcessAcceptedMessageArrival_StatusRQ() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RQ - Requesting to the country of departure"), GetAcceptanceTestFileRQ());

	public void TestProcessAcceptedMessageArrival_StatusRZ() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RZ - Deviation Rejected By UE"), GetAcceptanceTestFileRZ());

	public void TestProcessAcceptedMessageArrival_StatusRE() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RE - Received"), GetAcceptanceTestFileRE(), ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, shouldHaveDataChanged: true);

	public void TestProcessAcceptedMessageArrival_StatusLI() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("LI - Liquidation Initiated"), GetAcceptanceTestFileLI(), ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, shouldHaveDataChanged: true);

	public void TestProcessAcceptedMessageArrival_StatusUC() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UC - Ultimated For Payment"), GetAcceptanceTestFileUC(), ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease);

	public void TestProcessAcceptedMessageArrival_StatusUC_LoadData() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UC - Ultimated For Payment"), GetAcceptanceTestFileUC(), ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, shouldHaveDataChanged: true, shouldAddBillsToArrival: false);

	public void TestProcessAcceptedMessageArrival_StatusRD() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RD - Pending Resolution Of Discrepancy"), GetAcceptanceTestFileRD(), ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease);

	public void TestProcessAcceptedMessageArrival_StatusRD_LoadData() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RD - Pending Resolution Of Discrepancy"), GetAcceptanceTestFileRD(), ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease, shouldHaveDataChanged: true, shouldAddBillsToArrival: false);

	public void TestProcessAcceptedMessageArrival_StatusUL() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UL - Completed"), GetAcceptanceTestFileUL(), ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease);

	public void TestProcessAcceptedMessageArrival_StatusRE_And_BM_NoChangesToReport_True() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RE - Received"), GetAcceptanceTestFileRE(), ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, shouldHaveDataChanged: true, noChangesToReport: true);

	public void TestProcessAcceptedMessageArrival_StatusUL_WithoutReceptionCircuitOrDate()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = mrnEntryNumber;
		nctsHeader.MovementReferenceEntryNumber.CE_EntryStatus = CircuitCodeList.Codes.RED;
		var mrnExistingIssueDate = new ZDateTime(2023, 01, 01, 02, 01, 01);
		nctsHeader.MovementReferenceEntryNumber.CE_IssueDate = mrnExistingIssueDate;

		var expectedMessageInterpretation = "<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>NCTS5 (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>UL - Completed</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 00:00:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + mrnEntryNumber + "</td></tr></table>" +
			"<br><br><H4>Departure</H4>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>" + CsvClearance + "</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>21-11-2020</td></tr></table>" +
			"<br><br><H4>Arrival</H4>" +
			"<br><table border=\"0\"><tr><td>Location:</td><td>&nbsp;&nbsp;</td><td>nacUbicac1</td></tr></table>" +
			"<br><br><H4>Unloading</H4>" +
			"<br><table border=\"0\"><tr><td>Unloading Result Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Ultimate Date:</td><td>&nbsp;&nbsp;</td><td>02-07-2023</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Result:</td><td>&nbsp;&nbsp;</td><td>Major discrepancies in unloading but unlocked guarantee (B11)</td></tr></table>";

		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFileULWithoutReceptionCircuitOrDate(), InterchangeID);
		ProcessMessageForTest(message);

		AssertNCTSDeclarationQuery(message, expectedMessageInterpretation, ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, mrnExistingIssueDate, CircuitCodeList.Codes.RED, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
	}

	public void TestProcessAcceptedMessageArrival_StatusRE_GrossWeightWithPreviousDocChange() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RE - Received"), GetAcceptanceTestFileRE_MrnESWithPreviousDocuments(), ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, shouldHaveDataChanged: true, shouldHaveGrossWeightFromPrevDocument: true);

	public void TestProcessAcceptedMessageArrival_StatusRE_GrossWeightWithMrnNoES() => ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RE - Received", isMrnEntryNumberNoES: true), GetAcceptanceTestFileRE_MrnNoESWithPreviousDocuments(), ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, shouldHaveDataChanged: true, isMrnEntryNumberNoES: true);

	public void TestProcessAcceptedMessageArrival_StatusRE_WithoutCommodityCodeAndGoodsMeasure()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		AddUnloadingDataToArrival(false);

		var message = CreateNewEDIMessage(ApplicationReference, GetAcceptanceTestFilREWithoutCommodityCodeAndGoodsMeasure(), InterchangeID);
		ProcessMessageForTest(message);

		AssertNCTSDeclarationQuery(message, GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RE - Received"), ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted, receptionDate, CircuitCodeList.Codes.ORANGE, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader);
		var bills = nctsHeader.Bills;

		var bill1GoodsItems = bills.First(x => x.MovementDetail.B9_SeqNo == "2").ArrivalGoodsItems;
		AssertContainsExactElementsInAnyOrder("bill1GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
												new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
												{
													(2, 1, "DEC", ZString.Empty, ZString.Empty, 0, "KG", 0, "KG", "KFZ Teile (hier: Blechteile für die industrielle Montage von Karosserien)"),
													(3, 2, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
												}, bill1GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));
	}

	public void TestSetReleaseDate()
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;

		var testFile = GetAcceptanceTestFileUL();
		var message = CreateNewEDIMessage(ApplicationReference, testFile, InterchangeID);
		ProcessMessageForTest(message);

		CombineAssertions(() =>
		{
			AssertEquals("BM_CustomsStatus is CL1", ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			AssertEquals("ReleaseDate is set", new ZDateTime(2023, 07, 02, 0, 0, 0), nctsHeader.ReleaseDate);

			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			nctsHeader.SummaryEntryNumber.CE_IssueDate = ZDateTime.Empty;
			testFile = GetAcceptanceTestFileUL().Replace("<fechaUltimacionCompleta>2023-07-02</fechaUltimacionCompleta>", string.Empty);
			message = CreateNewEDIMessage(ApplicationReference, testFile, InterchangeID);
			ProcessMessageForTest(message);

			AssertEquals("BM_CustomsStatus is CL1 when fechaUltimacionCompleta is empty", ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
			AssertEquals("ReleaseDate is empty when fechaUltimacionCompleta is empty", ZDateTime.Empty, nctsHeader.ReleaseDate);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_AllDocs()
	{
		ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("DE - Dispatch", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileDE(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

		CombineAssertions(() =>
		{
			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 1, docMessages.Length);
			AssertDocumentRequestEDIMessages(docMessages, new List<(ZString fileName, ZString urlParameter)>() { (mrnEntryNumber + "_NCTS_AEAT_DAT.pdf", CsvClearance) });
		});
	}

	public void TestCreateDocumentCaptureRequestWhenCSVClearance_NoDocs()
	{
		var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
		docManagerInfo.AddFileOrDocument(new byte[1], mrnEntryNumber + "_NCTS_AEAT_DAT.pdf", "CAU");
		docManagerInfo.Save();

		ProcessAcceptedMessageDeparture_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("DE - Dispatch", GuaranteeDataNotWrittenOff), GetAcceptanceTestFileDE(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

		CombineAssertions(() =>
		{
			var eDocs = docManagerInfo.GetRelatedEDocs();
			AssertEquals("Number of eDocs is correct", 1, eDocs.Count());

			var eDocNames = new List<ZString>() { mrnEntryNumber + "_NCTS_AEAT_DAT.pdf" };
			AssertContainsExactElementsInAnyOrder("eDocs contains all documents with original names", eDocNames, eDocs.Cast<IeDoc>().Select(x => x.FileName).ToList());

			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	public void TestCreateDocumentCaptureRequestWhenArrivalAndCSVClearance_AllDocs()
	{
		ProcessAcceptedMessageArrival_Status(GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("DE - Dispatch"), GetAcceptanceTestFileDE());

		CombineAssertions(() =>
		{
			var docManagerInfo = ((IDocManagerSupport)nctsHeader).DocManagerInfo;
			var eDocs = docManagerInfo.GetRelatedEDocs();
			AssertEquals("Number of eDocs is correct", 0, eDocs.Count());

			nctsHeader.Messages.Reload(true);
			var docMessages = nctsHeader.Messages.GetMatchingMessages("ESC", new ZString[] { "DOC" }, "TRX");
			AssertEquals("Doc Messages sent number is", 0, docMessages.Length);
		});
	}

	[TestDate(2021, 10, 05, 09, 36, 0)]
	public void TestProcessAcceptedMessageDeparture_StatusUL_AddGuaranteesTransactions()
	{
		var extraGuaranteeReference = "16ESAGL9990000098";

		AddGuarantees(addTransactions: true, addOBLTransaction: true);

		SetUpGuarantee(extraGuaranteeReference, EUGuaranteeTypeList.Codes.IMP, mrnEntryNumber, ApplicationReference, 0m, 2000, 1300, true, true);
		var extraGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		extraGuarantee.PW_BondNumber = extraGuaranteeReference;
		extraGuarantee.PW_BondAmount = 1200m;
		AssertEquals(3, nctsHeader.MovementHeader.Guarantees.Count);

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UL - Completed", GuaranteeDataULWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFileUL(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for first guarantee (write off)", 14, guarantee1Transactions.Count());
			AssertNewTransaction("First guarantee's transaction", guarantee1Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 110m, new ZDateTime(2023, 07, 02, 0, 0, 0));

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions + 1 new transaction for second guarantee (write off)", 14, guarantee2Transactions.Count());
			AssertNewTransaction("Second guarantee's transaction", guarantee2Transactions.First(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)), 600m, new ZDateTime(2023, 07, 02, 0, 0, 0));

			var guarantee5Transactions = guaranteeHeaderList.First(x => x.CPH_Number == extraGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions, no transactions added for fifth guarantee", 13, guarantee5Transactions.Count());
			AssertEquals("No new TRA Write off transaction for fifth guarantee", false, guarantee5Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));
		});
	}

	public void TestProcessAcceptedMessageDeparture_StatusUL_AddGuaranteesTransactions_PositiveAmount()
	{
		SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.TRA, mrnEntryNumber, ApplicationReference, 500m, 1000, 1300, true, true);
		var extraGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		extraGuarantee.PW_BondNumber = GuaranteeReference;
		extraGuarantee.PW_BondAmount = 1000m;
		AssertEquals(1, nctsHeader.MovementHeader.Guarantees.Count);

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UL - Completed", GuaranteeDataULNotWrittenOffPositiveBalance);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFileUL(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

		var expectedError = "Reference 16ESAGL9990000096 has a positive balance of 440 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.";
		var concatenatedUserLogStrings = GetAllConcatenatedUserLogStrings();
		AssertContains("logger", expectedError, concatenatedUserLogStrings);
	}

	public void TestProcessAcceptedMessageDeparture_StatusNotUL_AddGuaranteesTransactions()
	{
		AddGuarantees(addTransactions: true, addOBLTransaction: true);

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PA - Pre-Declaration", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFilePA(), ESNCTS5DepartureCustomsStatusList.Codes.PreLodged);

		CombineAssertions(() =>
		{
			var guaranteeHeaderList = LoadCusGuaranteeHeaderList();

			var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
			AssertEquals("Original transactions", 13, guarantee1Transactions.Count());
			AssertEquals("No new TRA Write off transaction for first guarantee", false, guarantee1Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));

			var guarantee2Transactions = guaranteeHeaderList.First(x => x.CPH_Number == OtherGuaranteeReference).GetTransactions();
			AssertEquals("Original transactions", 13, guarantee2Transactions.Count());
			AssertEquals("No new TRA Write off transaction for second guarantee", false, guarantee2Transactions.Any(x => x.CPL_Comment.StartsWith(WriteOffTransactionCommentPrefix)));
		});
	}

	public void TestProcessAcceptedMessageDeparture_StatusUL_AddGuaranteesTransactions_NoPendingAmount()
	{
		SetUpGuarantee(GuaranteeReference, EUGuaranteeTypeList.Codes.TRA, mrnEntryNumber, ApplicationReference, 0, 880, 0, false, false);
		var extraGuarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		extraGuarantee.PW_BondNumber = GuaranteeReference;
		extraGuarantee.PW_BondAmount = 0m;
		AssertEquals(1, nctsHeader.MovementHeader.Guarantees.Count);

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("UL - Completed", GuaranteeDataULExcluded);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFileUL(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

		var guaranteeHeaderList = LoadCusGuaranteeHeaderList();
		var guarantee1Transactions = guaranteeHeaderList.First(x => x.CPH_Number == GuaranteeReference).GetTransactions();
		AssertEquals("Guarantee has no transactions", false, guarantee1Transactions.Any());
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithAllData_WithMultipleHouseConsignments()
	{
		ProcessPreDeclarationAndAssertData(true);
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithData_WithOneHouseConsignment_ShouldNotOverrideAllItems()
	{
		AddPreDeclarationDataToDeparture(addSecondGoodsItem: true);
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PG - Pending Guarantee", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFilePG(), ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance, placeOfUnloadingCode: "ESMADRID");

		CombineAssertions(() =>
		{
			var movementHeader = nctsHeader.MovementHeader;

			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertEquals("BM_InBondEntryType", "T1", movementHeader.BM_InBondEntryType);
			AssertEquals("TirCarnetNumber", ZString.Empty, movementHeader.TirCarnetNumber);
			AssertEquals("BM_ReducedDatasetIndicator", false, movementHeader.BM_ReducedDatasetIndicator);
			AssertEquals("BM_SpecificCircumstance", ZString.Empty, movementHeader.BM_SpecificCircumstance);
			AssertEquals("BM_RN_NKCountryOfDispatch", "FR", movementHeader.BM_RN_NKCountryOfDispatch);
			AssertEquals("BM_RL_NKDestinationPort", "IT", movementHeader.BM_RL_NKDestinationPort);
			AssertEquals("InlandTransportModeAtDeparture", "2", movementHeader.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", "30", movementHeader.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture", "5678MFB", movementHeader.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", "ES", movementHeader.TransportCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", "2131RTY", movementHeader.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", "ES", movementHeader.Trailer1NationalityAtDeparture);
			AssertEquals("BM_ExportTransportMode", "8", movementHeader.BM_ExportTransportMode);
			AssertEquals("BM_CustomsOfficeAtBorder", "CH006251", movementHeader.BM_CustomsOfficeAtBorder);
			AssertEquals("BM_ActiveBorderIdentificationType", "21", movementHeader.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID", "A_854/ 17-797//124-125", movementHeader.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality", "ES", movementHeader.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber", ZString.Empty, movementHeader.BM_ConveyanceNumber);
			AssertEquals("BM_GrossWeight", 51000m, movementHeader.BM_GrossWeight);
			AssertEquals("BM_UniqueConsignmentReference", ZString.Empty, movementHeader.BM_UniqueConsignmentReference);
			AssertEquals("GoodsLocation.CGL_AdditionalIdentifier", "ES00010101GENE", movementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("BM_PlaceOfLoading", "DEBER", movementHeader.BM_PlaceOfLoading);
			AssertEquals("BM_PlaceOfUnloading", "ESMADRID", movementHeader.BM_PlaceOfUnloading);
			AssertEquals("BM_MethodOfPayment", "A", movementHeader.BM_MethodOfPayment);
			AssertEquals("nctsHeader.PreviousDocuments", 0, nctsHeader.PreviousDocuments.Count);
			AssertEquals("nctsHeader.SupportingDocuments", 0, nctsHeader.MovementHeader.SupportingDocuments.Count);
			AssertEquals("nctsHeader.AdditionalDocuments with SubType TRA", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType REF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType INF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));

			AssertEquals("CusAuthorizationUsages", 0, nctsHeader.MovementHeader.CusAuthorizationUsages.Count);
			AssertEquals("CustomsOffices", 0, nctsHeader.MovementHeader.CustomsOffices.Count);
			AssertEquals("Guarantees", 0, nctsHeader.MovementHeader.Guarantees.Count);
			AssertEquals("CusSupplyChainActors", 0, nctsHeader.MovementHeader.CusSupplyChainActors.Count);
			AssertEquals("CountriesOfRouting", 0, nctsHeader.CountriesOfRouting.Count);

			AssertContainsExactElementsInAnyOrder("DepartureHeaderContainers (BC_SequenceNumber, BC_ContainerNum, BC_Mode, BC_Seal1, BC_Seal2)",
													new (ZShort, ZString, ZString, ZString, ZString)[]
														{
																(1, "CSQU3054383", "CNT","SEAL", ZString.Empty),
																(2, "CSQU3054384", "CNT",ZString.Empty, ZString.Empty),
																(3, "", "NCT","SEAL1", ZString.Empty)
														}, nctsHeader.DepartureHeaderContainers.Select(x => (x.BC_SequenceNumber, x.BC_ContainerNum, x.BC_Mode, x.BC_Seal1, x.BC_Seal2)));
			var containerWithSeq1 = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
			AssertEquals("Container with seq 1, has no seals", 0, containerWithSeq1.AdditionalSeals.Count);
			var containerWithSeq2 = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
			AssertEquals("Container with seq 2, has no seals", 0, containerWithSeq2.AdditionalSeals.Count);
			var containerWithSeq3 = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 3);
			AssertEquals("Container with seq 3, has no seals", 0, containerWithSeq3.AdditionalSeals.Count);

			AssertContainsExactElementsInAnyOrder("Bills (MovementDetail.B9_SeqNo, B0_Weight, B0_WeightUQ, B0_TransportPaymentMethod, B0_RN_NKCountryOfExport)",
													new (ZString, ZDecimal, ZString, ZString, ZString)[]
													{
															("1", 25000m, "KG", "S", "ES")
													}, nctsHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ, x.B0_TransportPaymentMethod, x.B0_RN_NKCountryOfExport)));

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");

			AssertEquals("bill1.CusSupplyChainActorReferences", 0, bill1.CusSupplyChainActorReferences.Count);
			AssertEquals("bill1.PreviousDocuments", 0, bill1.PreviousDocuments.Count);
			AssertEquals("bill1.SupportingDocuments", 0, bill1.SupportingDocuments.Count);

			AssertContainsExactElementsInAnyOrder("bill1.AdditionalDocuments with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "N740", "eshouse", ZString.Empty),
															(2, "N741", "ESHEADER", ZString.Empty),
															(3, "N743", "ESHEADER3", ZString.Empty)
													}, bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertEquals("bill1.AdditionalInfos with SubType REF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("bill1.AdditionalInfos with SubType INF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));

			AssertContainsExactElementsInAnyOrder("bill1.GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_Type,BY_RN_NKCountryOfDispatch, BY_RN_NKCountryOfDestination, BY_CommercialReferenceNumber, BY_Description" +
																				", BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_CustomsSecondQuantity, BY_CustomsSecondUnitQty, IsVehicles)",
													new (ZShort, ZInt, ZString, ZString, ZString, ZString, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZBool)[]
													{
															(1, 1, "T1", "DE", "IT", "reference2", "Of fir (Abies spp.) and spruce (Picea spp.)-A description", "CUSCODE1", "4407122011", 10000, "KG", 7000, "KG", 6, "LT", true),
															(2, 2, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, "KG", ZDecimal.Zero, "KG", ZDecimal.Zero, ZString.Empty, false)
													}, bill1.GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_Type, x.BY_RN_NKCountryOfDispatch, x.BY_RN_NKCountryOfDestination, x.BY_CommercialReferenceNumber, x.BY_Description
																									, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, x.IsVehicles)));
			var bill1GoodsItem1 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 1);
			AssertEquals("bill1GoodsItem1.CusSupplyChainActorReferences", 0, bill1GoodsItem1.CusSupplyChainActorReferences.Count);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.UNDGs (UNDGSubstance.DG_Code)",
													new ZString[] { "1002", "1003" }, bill1GoodsItem1.UNDGs.Select(x => x.UNDGSubstance.DG_Code));
			var bill1GoodsItem1Packages = bill1GoodsItem1.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages (B5_SequenceNumber, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
													new (ZShort, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "FR", ZLong.Zero, ZString.Empty, "VINCODE1", "BRAND1", "MODEL1"),
															(2, "FR", ZLong.Zero, ZString.Empty, "VINCODE2", ZString.Empty, ZString.Empty)
													}, bill1GoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var bill1GoodsItem1Packages1Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages1Containers.Container", new[] { containerWithSeq1, containerWithSeq2 }, bill1GoodsItem1Packages1Containers);
			var bill1GoodsItem1Packages2Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages2Containers.Container", new[] { containerWithSeq1, containerWithSeq2 }, bill1GoodsItem1Packages2Containers);

			AssertEquals("bill1.PreviousDocuments", 0, bill1.PreviousDocuments.Count);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_PackType, CSI_PackQty, CSI_UnitOfQuantity, CSI_Quantity, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString, ZInt, ZString, ZDecimal, ZString)[]
													{
															(1, "A001", "PrevDoc1", 5, "BX", 3, "LT", 600, "ExtraInfo"),
															(2, "A002", "PrevDoc2", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty)
													}, bill1GoodsItem1.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_PackType, x.CSI_PackQty, x.CSI_UnitOfQuantity, x.CSI_Quantity, x.CSI_ReferenceNumber2)));

			AssertEquals("bill1GoodsItem21.SupportingDocuments", 0, bill1GoodsItem1.SupportingDocuments.Count);
			AssertEquals("bill1GoodsItem1.AdditionalInfos with SubType TRA", 0, bill1GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));

			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.AdditionalInfos with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y066", "ESHOUSE", ZString.Empty),
															(2, "Y015", "ESHOUSE2", ZString.Empty)
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertEquals("bill1.AdditionalInfos with SubType INF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.AdditionalInfos with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y067", ZString.Empty, "ESHOUSE3"),
															(2, "Y016", ZString.Empty, "ESHOUSE4")
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			var bill1GoodsItem2 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 2);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2.CusSupplyChainActorReferences (CFR_Code, CFR_Reference, CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("CS", "actorId5", ZGuid.Empty),
															("FW", "actorId6", ZGuid.Empty)
													}, bill1GoodsItem2.CusSupplyChainActorReferences.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));
			AssertEquals("bill1GoodsItem2.UNDGs", 0, bill1GoodsItem2.UNDGs.Count);
			var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
													new (ZShort, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "BX", 15, "MARCA2", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "NE", 5, "MARCA3", ZString.Empty, ZString.Empty, ZString.Empty)
													}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var bill1GoodsItem2Packages1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages1Containers.Container", new[] { containerWithSeq1, containerWithSeq2, containerWithSeq3 }, bill1GoodsItem2Packages1Containers);
			var bill1GoodsItem2Packages2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages2Containers.Container", new[] { containerWithSeq1, containerWithSeq2, containerWithSeq3 }, bill1GoodsItem2Packages2Containers);

			AssertEquals("bill1GoodsItem2.PreviousDocuments", 0, bill1GoodsItem2.PreviousDocuments.Count);

			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(1, "N380", "M/144IC CV 7732/2"),
															(2, "N381", "ESHEADER")
													}, bill1GoodsItem2.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));

			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType TRA", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType REF", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType INF", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "INF"));
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithData_WithOneHouseConsignment_ShouldOverrideAllItems()
	{
		AddPreDeclarationDataToDeparture(addSecondGoodsItem: true, secondGoodsItemDeclarationGoodsItemNumber1: 0);
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PG - Pending Guarantee", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFilePG(), ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance, placeOfUnloadingCode: "ESMADRID");

		CombineAssertions(() =>
		{
			var movementHeader = nctsHeader.MovementHeader;

			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertEquals("BM_InBondEntryType", "T1", movementHeader.BM_InBondEntryType);
			AssertEquals("TirCarnetNumber", ZString.Empty, movementHeader.TirCarnetNumber);
			AssertEquals("BM_ReducedDatasetIndicator", false, movementHeader.BM_ReducedDatasetIndicator);
			AssertEquals("BM_SpecificCircumstance", ZString.Empty, movementHeader.BM_SpecificCircumstance);
			AssertEquals("BM_RN_NKCountryOfDispatch", "FR", movementHeader.BM_RN_NKCountryOfDispatch);
			AssertEquals("BM_RL_NKDestinationPort", "IT", movementHeader.BM_RL_NKDestinationPort);
			AssertEquals("InlandTransportModeAtDeparture", "2", movementHeader.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", "30", movementHeader.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture", "5678MFB", movementHeader.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", "ES", movementHeader.TransportCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", "2131RTY", movementHeader.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", "ES", movementHeader.Trailer1NationalityAtDeparture);
			AssertEquals("BM_ExportTransportMode", "8", movementHeader.BM_ExportTransportMode);
			AssertEquals("BM_CustomsOfficeAtBorder", "CH006251", movementHeader.BM_CustomsOfficeAtBorder);
			AssertEquals("BM_ActiveBorderIdentificationType", "21", movementHeader.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID", "A_854/ 17-797//124-125", movementHeader.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality", "ES", movementHeader.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber", ZString.Empty, movementHeader.BM_ConveyanceNumber);
			AssertEquals("BM_GrossWeight", 51000m, movementHeader.BM_GrossWeight);
			AssertEquals("BM_UniqueConsignmentReference", ZString.Empty, movementHeader.BM_UniqueConsignmentReference);
			AssertEquals("GoodsLocation.CGL_AdditionalIdentifier", "ES00010101GENE", movementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("BM_PlaceOfLoading", "DEBER", movementHeader.BM_PlaceOfLoading);
			AssertEquals("BM_PlaceOfUnloading", "ESMADRID", movementHeader.BM_PlaceOfUnloading);
			AssertEquals("BM_MethodOfPayment", "A", movementHeader.BM_MethodOfPayment);
			AssertEquals("nctsHeader.PreviousDocuments", 0, nctsHeader.PreviousDocuments.Count);
			AssertEquals("nctsHeader.SupportingDocuments", 0, nctsHeader.MovementHeader.SupportingDocuments.Count);
			AssertEquals("nctsHeader.AdditionalDocuments with SubType TRA", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType REF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType INF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));

			AssertEquals("CusAuthorizationUsages", 0, nctsHeader.MovementHeader.CusAuthorizationUsages.Count);
			AssertEquals("CustomsOffices", 0, nctsHeader.MovementHeader.CustomsOffices.Count);
			AssertEquals("Guarantees", 0, nctsHeader.MovementHeader.Guarantees.Count);
			AssertEquals("CusSupplyChainActors", 0, nctsHeader.MovementHeader.CusSupplyChainActors.Count);
			AssertEquals("CountriesOfRouting", 0, nctsHeader.CountriesOfRouting.Count);

			AssertContainsExactElementsInAnyOrder("DepartureHeaderContainers (BC_SequenceNumber, BC_ContainerNum, BC_Mode, BC_Seal1, BC_Seal2)",
													new (ZShort, ZString, ZString, ZString, ZString)[]
														{
																(1, "CSQU3054383", "CNT","SEAL", ZString.Empty),
																(2, "CSQU3054384", "CNT",ZString.Empty, ZString.Empty),
																(3, "", "NCT","SEAL1", ZString.Empty)
														}, nctsHeader.DepartureHeaderContainers.Select(x => (x.BC_SequenceNumber, x.BC_ContainerNum, x.BC_Mode, x.BC_Seal1, x.BC_Seal2)));
			var containerWithSeq1 = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
			AssertEquals("Container with seq 1, has no seals", 0, containerWithSeq1.AdditionalSeals.Count);
			var containerWithSeq2 = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
			AssertEquals("Container with seq 2, has no seals", 0, containerWithSeq2.AdditionalSeals.Count);
			var containerWithSeq3 = nctsHeader.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 3);
			AssertEquals("Container with seq 3, has no seals", 0, containerWithSeq3.AdditionalSeals.Count);

			AssertContainsExactElementsInAnyOrder("Bills (MovementDetail.B9_SeqNo, B0_Weight, B0_WeightUQ, B0_TransportPaymentMethod, B0_RN_NKCountryOfExport)",
													new (ZString, ZDecimal, ZString, ZString, ZString)[]
													{
															("1", 25000m, "KG", "S", "ES")
													}, nctsHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ, x.B0_TransportPaymentMethod, x.B0_RN_NKCountryOfExport)));

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");

			AssertEquals("bill1.CusSupplyChainActorReferences", 0, bill1.CusSupplyChainActorReferences.Count);
			AssertEquals("bill1.PreviousDocuments", 0, bill1.PreviousDocuments.Count);
			AssertEquals("bill1.SupportingDocuments", 0, bill1.SupportingDocuments.Count);

			AssertContainsExactElementsInAnyOrder("bill1.AdditionalDocuments with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "N740", "eshouse", ZString.Empty),
															(2, "N741", "ESHEADER", ZString.Empty),
															(3, "N743", "ESHEADER3", ZString.Empty)
													}, bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertEquals("bill1.AdditionalInfos with SubType REF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("bill1.AdditionalInfos with SubType INF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));

			AssertContainsExactElementsInAnyOrder("bill1.GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_Type,BY_RN_NKCountryOfDispatch, BY_RN_NKCountryOfDestination, BY_CommercialReferenceNumber, BY_Description" +
																				", BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_CustomsSecondQuantity, BY_CustomsSecondUnitQty, IsVehicles)",
													new (ZShort, ZInt, ZString, ZString, ZString, ZString, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZBool)[]
													{
															(1, 1, "T1", ZString.Empty, ZString.Empty, ZString.Empty, "Of fir (Abies spp.) and spruce (Picea spp.)-A description", "CUSCODE1", "44071220", 10000, "KG", 7000, "KG", ZDecimal.Zero, ZString.Empty, true),
															(2, 2, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, "KG", ZDecimal.Zero, "KG", ZDecimal.Zero, ZString.Empty, false)
													}, bill1.GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_Type, x.BY_RN_NKCountryOfDispatch, x.BY_RN_NKCountryOfDestination, x.BY_CommercialReferenceNumber, x.BY_Description
																									, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, x.IsVehicles)));
			var bill1GoodsItem1 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 1);
			AssertEquals("bill1GoodsItem1.CusSupplyChainActorReferences", 0, bill1GoodsItem1.CusSupplyChainActorReferences.Count);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.UNDGs (UNDGSubstance.DG_Code)",
													new ZString[] { "1002", "1003" }, bill1GoodsItem1.UNDGs.Select(x => x.UNDGSubstance.DG_Code));
			var bill1GoodsItem1Packages = bill1GoodsItem1.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages (B5_SequenceNumber, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
													new (ZShort, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "FR", ZLong.Zero, ZString.Empty, "VINCODE1", "BRAND1", "MODEL1"),
															(2, "FR", ZLong.Zero, ZString.Empty, "VINCODE2", ZString.Empty, ZString.Empty)
													}, bill1GoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var bill1GoodsItem1Packages1Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages1Containers.Container", new[] { containerWithSeq1, containerWithSeq2 }, bill1GoodsItem1Packages1Containers);
			var bill1GoodsItem1Packages2Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages2Containers.Container", new[] { containerWithSeq1, containerWithSeq2 }, bill1GoodsItem1Packages2Containers);

			AssertEquals("bill1.PreviousDocuments", 0, bill1.PreviousDocuments.Count);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_PackType, CSI_PackQty, CSI_UnitOfQuantity, CSI_Quantity, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString, ZInt, ZString, ZDecimal, ZString)[]
													{
															(1, "A001", "PrevDoc1", 5, "BX", 3, "LT", 600, "ExtraInfo"),
															(2, "A002", "PrevDoc2", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty)
													}, bill1GoodsItem1.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_PackType, x.CSI_PackQty, x.CSI_UnitOfQuantity, x.CSI_Quantity, x.CSI_ReferenceNumber2)));

			AssertEquals("bill1GoodsItem21.SupportingDocuments", 0, bill1GoodsItem1.SupportingDocuments.Count);
			AssertEquals("bill1GoodsItem1.AdditionalInfos with SubType TRA", 0, bill1GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));

			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.AdditionalInfos with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y066", "ESHOUSE", ZString.Empty),
															(2, "Y015", "ESHOUSE2", ZString.Empty)
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertEquals("bill1.AdditionalInfos with SubType INF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.AdditionalInfos with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y067", ZString.Empty, "ESHOUSE3"),
															(2, "Y016", ZString.Empty, "ESHOUSE4")
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			var bill1GoodsItem2 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 2);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2.CusSupplyChainActorReferences (CFR_Code, CFR_Reference, CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("CS", "actorId5", ZGuid.Empty),
															("FW", "actorId6", ZGuid.Empty)
													}, bill1GoodsItem2.CusSupplyChainActorReferences.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));
			AssertEquals("bill1GoodsItem2.UNDGs", 0, bill1GoodsItem2.UNDGs.Count);
			var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
													new (ZShort, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "BX", 15, "MARCA2", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "NE", 5, "MARCA3", ZString.Empty, ZString.Empty, ZString.Empty)
													}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var bill1GoodsItem2Packages1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages1Containers.Container", new[] { containerWithSeq1, containerWithSeq2, containerWithSeq3 }, bill1GoodsItem2Packages1Containers);
			var bill1GoodsItem2Packages2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages2Containers.Container", new[] { containerWithSeq1, containerWithSeq2, containerWithSeq3 }, bill1GoodsItem2Packages2Containers);

			AssertEquals("bill1GoodsItem2.PreviousDocuments", 0, bill1GoodsItem2.PreviousDocuments.Count);

			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(1, "N380", "M/144IC CV 7732/2"),
															(2, "N381", "ESHEADER")
													}, bill1GoodsItem2.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));

			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType TRA", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType REF", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType INF", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "INF"));
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithNoData()
	{
		AddPreDeclarationDataToDeparture();
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("PI - Pre-Declaration Invalidated", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFilePI(), ESNCTS5DepartureCustomsStatusList.Codes.Invalidated, placeOfUnloadingCode: "FRPAR");

		CombineAssertions(() =>
		{
			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("BM_InBondEntryType", ZString.Empty, movementHeader.BM_InBondEntryType);
			AssertEquals("TirCarnetNumber", ZString.Empty, movementHeader.TirCarnetNumber);
			AssertEquals("BM_ReducedDatasetIndicator", false, movementHeader.BM_ReducedDatasetIndicator);
			AssertEquals("BM_SpecificCircumstance", ZString.Empty, movementHeader.BM_SpecificCircumstance);
			AssertEquals("BM_RN_NKCountryOfDispatch", "FR", movementHeader.BM_RN_NKCountryOfDispatch);
			AssertEquals("BM_RL_NKDestinationPort", "IT", movementHeader.BM_RL_NKDestinationPort);
			AssertEquals("InlandTransportModeAtDeparture", "2", movementHeader.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", ZString.Empty, movementHeader.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture", ZString.Empty, movementHeader.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", ZString.Empty, movementHeader.TransportCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", ZString.Empty, movementHeader.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", ZString.Empty, movementHeader.Trailer1NationalityAtDeparture);
			AssertEquals("BM_ExportTransportMode", "8", movementHeader.BM_ExportTransportMode);
			AssertEquals("BM_CustomsOfficeAtBorder", ZString.Empty, movementHeader.BM_CustomsOfficeAtBorder);
			AssertEquals("BM_ActiveBorderIdentificationType", ZString.Empty, movementHeader.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID", ZString.Empty, movementHeader.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality", ZString.Empty, movementHeader.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber", ZString.Empty, movementHeader.BM_ConveyanceNumber);
			AssertEquals("BM_GrossWeight", ZDecimal.Zero, movementHeader.BM_GrossWeight);
			AssertEquals("BM_UniqueConsignmentReference", ZString.Empty, movementHeader.BM_UniqueConsignmentReference);
			AssertEquals("GoodsLocation.CGL_AdditionalIdentifier", ZString.Empty, movementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("BM_PlaceOfLoading", "ESBCN", movementHeader.BM_PlaceOfLoading);
			AssertEquals("BM_PlaceOfUnloading", "FRPAR", movementHeader.BM_PlaceOfUnloading);
			AssertEquals("BM_MethodOfPayment", "A", movementHeader.BM_MethodOfPayment);
			AssertEquals("nctsHeader.PreviousDocuments", 0, nctsHeader.PreviousDocuments.Count);
			AssertEquals("nctsHeader.SupportingDocuments", 0, movementHeader.SupportingDocuments.Count);
			AssertEquals("nctsHeader.AdditionalDocuments with SubType TRA", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType REF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType INF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));
			AssertEquals("nctsHeader.CusAuthorizationUsages", 0, nctsHeader.MovementHeader.CusAuthorizationUsages.Count);
			AssertEquals("CustomsOffices", 0, movementHeader.CustomsOffices.Count);
			AssertEquals("Guarantees", 0, nctsHeader.MovementHeader.Guarantees.Count);
			AssertEquals("nctsHeader.CusSupplyChainActors", 0, nctsHeader.MovementHeader.CusSupplyChainActors.Count);
			AssertEquals("DepartureHeaderContainers", 0, nctsHeader.DepartureHeaderContainers.Count);
			AssertEquals("CountriesOfRouting", 0, nctsHeader.CountriesOfRouting.Count);
			AssertEquals("Bills", 0, nctsHeader.Bills.Count);
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithDocDataForFinalPeriod()
	{
		AddPreDeclarationDataToDeparture();
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PD - Pending Dispatch", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFilePD(), ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl, placeOfUnloadingCode: "FRPAR");

		CombineAssertions(() =>
		{
			var movementHeader = nctsHeader.MovementHeader;

			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			AssertEquals("BM_InBondEntryType", "T1", movementHeader.BM_InBondEntryType);
			AssertEquals("TirCarnetNumber", ZString.Empty, movementHeader.TirCarnetNumber);
			AssertEquals("BM_ReducedDatasetIndicator", false, movementHeader.BM_ReducedDatasetIndicator);
			AssertEquals("BM_SpecificCircumstance", ZString.Empty, movementHeader.BM_SpecificCircumstance);
			AssertEquals("BM_RN_NKCountryOfDispatch", "FR", movementHeader.BM_RN_NKCountryOfDispatch);
			AssertEquals("BM_RL_NKDestinationPort", "ES", movementHeader.BM_RL_NKDestinationPort);
			AssertEquals("InlandTransportModeAtDeparture", "2", movementHeader.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", "30", movementHeader.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture", "5678MFB", movementHeader.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", "ES", movementHeader.TransportCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", "2131RTY", movementHeader.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", "ES", movementHeader.Trailer1NationalityAtDeparture);
			AssertEquals("BM_ExportTransportMode", "2", movementHeader.BM_ExportTransportMode);
			AssertEquals("BM_CustomsOfficeAtBorder", "CH006251", movementHeader.BM_CustomsOfficeAtBorder);
			AssertEquals("BM_ActiveBorderIdentificationType", "21", movementHeader.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID", "A_854/ 17-797//124-125", movementHeader.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality", "ES", movementHeader.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber", ZString.Empty, movementHeader.BM_ConveyanceNumber);
			AssertEquals("BM_GrossWeight", 51000m, movementHeader.BM_GrossWeight);
			AssertEquals("BM_UniqueConsignmentReference", ZString.Empty, movementHeader.BM_UniqueConsignmentReference);
			AssertEquals("GoodsLocation.CGL_AdditionalIdentifier", "ES00010101GENE", movementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("BM_PlaceOfLoading", "ESBCN", movementHeader.BM_PlaceOfLoading);
			AssertEquals("BM_PlaceOfUnloading", "FRPAR", movementHeader.BM_PlaceOfUnloading);
			AssertEquals("BM_MethodOfPayment", "A", movementHeader.BM_MethodOfPayment);

			AssertContainsExactElementsInAnyOrder("nctsHeader.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_PackType, CSI_PackQty, CSI_UnitOfQuantity, CSI_Quantity, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString, ZInt, ZString, ZDecimal, ZString)[]
													{
															(1, "A001", "PrevDoc1", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, "ExtraInfo"),
															(2, "A002", "PrevDoc2", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty)
													}, nctsHeader.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_PackType, x.CSI_PackQty, x.CSI_UnitOfQuantity, x.CSI_Quantity, x.CSI_ReferenceNumber2)));

			AssertContainsExactElementsInAnyOrder("nctsHeader.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString)[]
													{
															(1, "N380", "M/144IC CV 7732/2", 5, "OtherExtraInfo"),
															(2, "N381", "ESHEADER", ZInt.Zero, ZString.Empty)
													}, movementHeader.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_ReferenceNumber2)));

			AssertContainsExactElementsInAnyOrder("nctsHeader.AdditionalDocuments with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "N740", "eshouse", ZString.Empty),
															(2, "N741", "ESHEADER", ZString.Empty)
													}, nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("nctsHeader.AdditionalDocuments with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y066", "ESHOUSE", ZString.Empty),
															(2, "Y015", "ESHOUSE2", ZString.Empty)
													}, nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("nctsHeader.AdditionalDocuments with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y067", ZString.Empty, "ESHOUSE3"),
															(2, "Y016", ZString.Empty, "ESHOUSE4")
													}, nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertEquals("CusAuthorizationUsages", 0, nctsHeader.MovementHeader.CusAuthorizationUsages.Count);
			AssertEquals("CustomsOffices", 0, nctsHeader.MovementHeader.CustomsOffices.Count);
			AssertEquals("Guarantees", 0, nctsHeader.MovementHeader.Guarantees.Count);
			AssertEquals("CusSupplyChainActors", 0, nctsHeader.MovementHeader.CusSupplyChainActors.Count);
			AssertEquals("DepartureHeaderContainers", 0, nctsHeader.DepartureHeaderContainers.Count);
			AssertEquals("CountriesOfRouting", 0, nctsHeader.CountriesOfRouting.Count);

			AssertContainsExactElementsInAnyOrder("Bills (MovementDetail.B9_SeqNo, B0_Weight, B0_WeightUQ, B0_TransportPaymentMethod, B0_RN_NKCountryOfExport)",
													new (ZString, ZDecimal, ZString, ZString, ZString)[]
													{
															("1", 25000m, "KG", "S", "ES")
													}, nctsHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ, x.B0_TransportPaymentMethod, x.B0_RN_NKCountryOfExport)));

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");

			AssertEquals("bill1.CusSupplyChainActorReferences", 0, bill1.CusSupplyChainActorReferences.Count);

			AssertContainsExactElementsInAnyOrder("bill1.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_PackType, CSI_PackQty, CSI_UnitOfQuantity, CSI_Quantity, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString, ZInt, ZString, ZDecimal, ZString)[]
													{
															(1, "A003", "PrevDoc3", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, "ExtraInfo3"),
															(2, "A004", "PrevDoc4", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty)
													}, bill1.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_PackType, x.CSI_PackQty, x.CSI_UnitOfQuantity, x.CSI_Quantity, x.CSI_ReferenceNumber2)));

			AssertContainsExactElementsInAnyOrder("bill1.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString)[]
													{
															(1, "N383", "M/144IC CV 7732/3", 3, "OtherExtraInfo3"),
															(2, "N384", "ESHEADER4", ZInt.Zero, ZString.Empty)
													}, bill1.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_ReferenceNumber2)));

			AssertContainsExactElementsInAnyOrder("bill2.AdditionalDocuments with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "N743", "eshouse3", ZString.Empty),
															(2, "N744", "ESHEADER4", ZString.Empty)
													}, bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("bill1.AdditionalDocuments with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y063", "ESHOUSE3", ZString.Empty),
															(2, "Y014", "ESHOUSE4", ZString.Empty)
													}, bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("bill1.AdditionalDocuments with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y065", ZString.Empty, "ESHOUSE5"),
															(2, "Y017", ZString.Empty, "ESHOUSE6")
													}, bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("bill2.GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_Type,BY_RN_NKCountryOfDispatch, BY_RN_NKCountryOfDestination, BY_CommercialReferenceNumber, BY_Description" +
																				", BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, IsVehicles)",
													new (ZShort, ZInt, ZString, ZString, ZString, ZString, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZBool)[]
													{
															(1, 1, ZString.Empty, "DE", "IT", "reference2", "Of fir (Abies spp.) and spruce (Picea spp.)-A description", "CUSCODE1", "55071220", 10000, "KG", 7000, "KG", 6, ZString.Empty, false)
													}, bill1.GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_Type, x.BY_RN_NKCountryOfDispatch, x.BY_RN_NKCountryOfDestination, x.BY_CommercialReferenceNumber, x.BY_Description
																									, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, x.IsVehicles)));

			var bill1GoodsItem1 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 1);
			AssertEquals("bill1GoodsItem1.CusSupplyChainActorReferences", 0, bill1GoodsItem1.CusSupplyChainActorReferences.Count);
			AssertEquals("bill1GoodsItem1.UNDGs", 0, bill1GoodsItem1.UNDGs.Count);
			AssertEquals("bill1GoodsItem1.Packages", 0, bill1GoodsItem1.Packages.Count);

			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_PackType, CSI_PackQty, CSI_UnitOfQuantity, CSI_Quantity, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString, ZInt, ZString, ZDecimal, ZString)[]
													{
															(1, "A005", "PrevDoc5", 5, "BX", 3, "LT", 600, "ExtraInfo5"),
															(2, "A006", "PrevDoc6", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty)
													}, bill1GoodsItem1.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_PackType, x.CSI_PackQty, x.CSI_UnitOfQuantity, x.CSI_Quantity, x.CSI_ReferenceNumber2)));

			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(1, "N385", "M/144IC CV 7732/5"),
															(2, "N386", "ESHEADER6")
													}, bill1GoodsItem1.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));

			AssertEquals("bill1GoodsItem1.AdditionalInfos with SubType TRA", 0, bill1GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));

			AssertContainsExactElementsInAnyOrder("bill2GoodsItem1.AdditionalInfos with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y068", "ESHOUSE5", ZString.Empty),
															(2, "Y019", "ESHOUSE6", ZString.Empty)
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("bill2GoodsItem1.AdditionalInfos with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y069", ZString.Empty, "ESHOUSE7"),
															(2, "Y011", ZString.Empty, "ESHOUSE8")
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_StatusRE_WithoutCommodityCodeAndGoodsMeasure()
	{
		AddPreDeclarationDataToDeparture(addSecondGoodsItem: true, secondGoodsItemDeclarationGoodsItemNumber2: 0);
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("RE - Received", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFilREWithoutCommodityCodeAndGoodsMeasure(), ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, placeOfUnloadingCode: "FRPAR");

		CombineAssertions(() =>
		{
			var movementHeader = nctsHeader.MovementHeader;

			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "2");

			AssertContainsExactElementsInAnyOrder("bill2.GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_Type,BY_RN_NKCountryOfDispatch, BY_RN_NKCountryOfDestination, BY_CommercialReferenceNumber, BY_Description" +
																				", BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_CustomsSecondQuantity, BY_CustomsSecondUnitQty, IsVehicles)",
													new (ZShort, ZInt, ZString, ZString, ZString, ZString, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZBool)[]
													{
															(2, 1, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "KFZ Teile (hier: Blechteile für die industrielle Montage von Karosserien)", ZString.Empty, ZString.Empty, 0, "KG", 0, "KG", ZDecimal.Zero, ZString.Empty, false)
													}, bill1.GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_Type, x.BY_RN_NKCountryOfDispatch, x.BY_RN_NKCountryOfDestination, x.BY_CommercialReferenceNumber, x.BY_Description
																									, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, x.IsVehicles)));
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithSomeData_WithOneHouseConsignment_GuaranteesShouldHaveOverrideTrue_AmountNot0()
	{
		EU.NCTS.Business.Testing.NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		SetupLiabilityRates();
		AddPreDeclarationDataToDeparture(addSecondGoodsItem: true);
		SetUpGuarantee("Number", EUGuaranteeTypeList.Codes.TRA, mrnEntryNumber, ApplicationReference, 1000m, 1000m, 20m, nctsHeader.DeclarantOrgPK, subType: "6");
		SetUpGuarantee("Number2", EUGuaranteeTypeList.Codes.TRA, mrnEntryNumber, ApplicationReference, 1000m, 1000m, 20m, nctsHeader.DeclarantOrgPK, subType: "6");
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("IG - Invalidated by Guarantee", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFileIG(), ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, placeOfUnloadingCode: "FRPAR");

		CombineAssertions(() =>
		{
			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");
			var bill1GoodsItem1 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 1);
			AssertEquals("First GoodsItem has 3 fees", 3, bill1GoodsItem1.Fees.Count);
			AssertEquals("First GoodsItem has 3 fees which the sum of is 19.20", 19.20m, bill1GoodsItem1.Fees.Sum(x => x.BFE_ChargeAmount));

			var bill1GoodsItem2 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 2);
			AssertEquals("Second GoodsItem has 0 fees", 0, bill1GoodsItem2.Fees.Count);

			AssertContainsExactElementsInAnyOrder("Guarantees (PW_BondType, PW_BondNumber, PW_BondAmount, PW_Password, PW_Override)",
													new (ZString, ZString, ZDecimal, ZString, ZBool)[]
													{
															("6", "Number", 32, ZString.Empty, true),
															("6", "Number2", 2, "pass", true)
													}, nctsHeader.MovementHeader.Guarantees.Select(x => (x.PW_BondType, x.PW_BondNumber, x.PW_BondAmount, x.PW_Password, x.PW_Override)));
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithData_WithOneHouseConsignment_GuaranteesShouldHaveOverrideTrue_Amount0()
	{
		SetupLiabilityRates();
		AddPreDeclarationDataToDeparture();
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("CA - Declaration Canceled", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFileCA(), ESNCTS5DepartureCustomsStatusList.Codes.Cancelled, placeOfUnloadingCode: "FRPAR");

		CombineAssertions(() =>
		{
			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");
			var bill1GoodsItem1 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 1);
			AssertEquals("First GoodsItem has 3 fees", 3, bill1GoodsItem1.Fees.Count);
			AssertEquals("First GoodsItem has 3 fees which the sum of is 19.20", 19.20m, bill1GoodsItem1.Fees.Sum(x => x.BFE_ChargeAmount));

			var bill1GoodsItem2 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 2);
			AssertEquals("Second GoodsItem has 0 fees", 0, bill1GoodsItem2.Fees.Count);

			AssertContainsExactElementsInAnyOrder("Guarantees (PW_BondType, PW_BondNumber, PW_BondAmount, PW_Password, PW_Override)",
													new (ZString, ZString, ZDecimal, ZString, ZBool)[]
													{
															("6", "Number", 0, ZString.Empty, true)
													}, nctsHeader.MovementHeader.Guarantees.Select(x => (x.PW_BondType, x.PW_BondNumber, x.PW_BondAmount, x.PW_Password, x.PW_Override)));
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationTrue_WithData_WithOneHouseConsignment_GuaranteesShouldHaveOverrideFalse()
	{
		SetupLiabilityRates();
		AddPreDeclarationDataToDeparture(addSecondGoodsItem: true);
		nctsHeader.UpdatePreDeclaration = true;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV("IV - Invalidated", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFileIV(), ESNCTS5DepartureCustomsStatusList.Codes.Invalidated, placeOfUnloadingCode: "FRPAR");

		CombineAssertions(() =>
		{
			AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
			AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");
			var bill1GoodsItem1 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 1);
			AssertEquals("First GoodsItem has 3 fees", 3, bill1GoodsItem1.Fees.Count);
			AssertEquals("First GoodsItem has 3 fees which the sum of is 19.20", 19.20m, bill1GoodsItem1.Fees.Sum(x => x.BFE_ChargeAmount));

			var bill1GoodsItem2 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 2);
			AssertEquals("Second GoodsItem has 0 fees", 0, bill1GoodsItem2.Fees.Count);

			AssertContainsExactElementsInAnyOrder("Guarantees are recalculated when override is set to false (PW_BondType, PW_BondNumber, PW_BondAmount, PW_Password, PW_Override)",
													new (ZString, ZString, ZDecimal, ZString, ZBool)[]
													{
															("6", "Number", 9.60m, ZString.Empty, false),
															("6", "Number2", 9.60m, "pass", false)
													}, nctsHeader.MovementHeader.Guarantees.Select(x => (x.PW_BondType, x.PW_BondNumber, x.PW_BondAmount, x.PW_Password, x.PW_Override)));
		});
	}

	public void TestProcessAcceptedMessageDeparture_UpdatePreDeclarationFalse()
	{
		ProcessPreDeclarationAndAssertData(false);
	}

	public void TestSetEntryIssueDate()
	{
		var testFile = GetAcceptanceTestFileUL();
		CombineAssertions("when departure", () =>
		{
			AssertEquals("MovementReferenceEntryNumber.CE_IssueDate", ZDateTime.Empty, nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);
			var message = CreateNewEDIMessage(ApplicationReference, testFile, InterchangeID);
			ProcessMessageForTest(message);
			AssertEquals("MovementReferenceEntryNumber.CE_IssueDate", admissionDate, nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);

			nctsHeader.MovementReferenceEntryNumber.CE_IssueDate = admissionDate.AddHours(2);
			ProcessMessageForTest(message);
			AssertEquals("MovementReferenceEntryNumber.CE_IssueDate", admissionDate.AddHours(2), nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);
		});

		CombineAssertions("when arrival", () =>
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
			nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;

			var message = CreateNewEDIMessage(ApplicationReference, testFile, InterchangeID);
			ProcessMessageForTest(message);
			AssertEquals("MovementReferenceEntryNumber.CE_IssueDate", receptionDate, nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);

			nctsHeader.MovementReferenceEntryNumber.CE_IssueDate = receptionDate.AddHours(2);
			ProcessMessageForTest(message);
			AssertEquals("MovementReferenceEntryNumber.CE_IssueDate", receptionDate.AddHours(2), nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);
		});
	}

	void ProcessAcceptedMessageArrival_Status(ZString expectedMessageInterpretationText, ZString file, string status = "", bool shouldHaveDataChanged = false, bool noChangesToReport = false, bool shouldHaveGrossWeightFromPrevDocument = false, bool isMrnEntryNumberNoES = false, bool shouldAddBillsToArrival = true)
	{
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		nctsHeader.ArrivalMovementHeader.BM_Phase = InitialPhaseStatus;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = OriginalEntryStatus;
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = noChangesToReport;
		AddUnloadingDataToArrival(noChangesToReport, shouldAddBillsToArrival);

		var message = CreateNewEDIMessage(ApplicationReference, file, InterchangeID);
		ProcessMessageForTest(message);

		AssertNCTSDeclarationQuery(message, expectedMessageInterpretationText, status, receptionDate, CircuitCodeList.Codes.ORANGE, arrivalMovementHeader: nctsHeader.ArrivalMovementHeader, isMrnEntryNumberNoES: isMrnEntryNumberNoES);

		AssertUnloadingDataAfterProcessing(shouldHaveDataChanged, shouldHaveGrossWeightFromPrevDocument, shouldAddBillsToArrival);
	}

	void ProcessAcceptedMessageDeparture_Status(ZString expectedMessageInterpretationText, ZString file, ZString status, string placeOfUnloadingCode = "")
	{
		var message = CreateNewEDIMessage(ApplicationReference, file, InterchangeID);
		ProcessMessageForTest(message);

		var queryCLR = new ZQuery(CusEntryNumSchema.CE_EntryNum, CsvClearance);
		queryCLR.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Spain.ClearanceCSV);
		queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryCLR.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberClearance = message.Factory.Load<CusEntryNumber>(queryCLR).Single();

		AssertNCTSDeclarationQuery(message, expectedMessageInterpretationText, status, admissionDate, CircuitCodeList.Codes.GREEN, cusEntryNumberClearance: cusEntryNumberClearance, clearanceReferenceNumber: CsvClearance, clearanceIssueDate: ClearanceDate, clearanceExpiryDate: ExpiryDate, placeOfUnloadingCode: placeOfUnloadingCode);
	}

	void AssertNCTSDeclarationQuery(TestEdiMessage message, ZString expectedMessageInterpretationText, ZString status, ZDateTime mrnIssueDate, ZString mrnEntryStatus, NctsArrivalMovementHeader arrivalMovementHeader = null, CusEntryNumber cusEntryNumberClearance = null, string clearanceReferenceNumber = "", ZDateTime? clearanceIssueDate = null, ZDateTime? clearanceExpiryDate = null, string placeOfUnloadingCode = "", bool isMrnEntryNumberNoES = false)
	{
		var mrnEntryNum = isMrnEntryNumberNoES ? mrnEntryNumberNoES : mrnEntryNumber;
		var queryMRN = new ZQuery(CusEntryNumSchema.CE_EntryNum, mrnEntryNum);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentTable, nameof(CusInBondHeader));
		queryMRN.AddToFilter(CusEntryNumSchema.CE_ParentID, nctsHeader.PK);
		var cusEntryNumberMRN = message.Factory.Load<CusEntryNumber>(queryMRN).Single();

		AssertNCTSDeclaration(message, messageSubType: "ACC", expectedMessageInterpretation: expectedMessageInterpretationText, emStatus: EDIMessageStatusList.Codes.Received, mrnEntrynum: mrnEntryNum, mrnIssueDate: mrnIssueDate, commonCustomsStatus: status, clearanceReferenceNumber: clearanceReferenceNumber, clearanceIssueDate: clearanceIssueDate, clearanceExpiryDate: clearanceExpiryDate, cusEntryNumberClearance: cusEntryNumberClearance, cusEntryNumberMRN: cusEntryNumberMRN, mrnEntryStatus: mrnEntryStatus, arrivalMovementHeader: arrivalMovementHeader, placeOfUnloadingCode: placeOfUnloadingCode);
	}

	void AssertUnloadingDataAfterProcessing(bool shouldHaveDataChanged, bool shouldHaveGrossWeightFromPrevDocument, bool shouldAddBillsToArrival = true)
	{
		CombineAssertions(() =>
		{
			var containers = nctsHeader.ArrivalHeaderContainers;
			NctsArrivalHeaderContainer containerWithSeq1 = null;
			var containerWithSeq2 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
			var containerWithSeq3 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 3);
			var transportInfos = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos;
			var bills = nctsHeader.Bills;

			if (shouldHaveDataChanged)
			{
				AssertEquals("nctsHeader.ArrivalMovementHeader.BM_GrossWeight", 51000m, nctsHeader.ArrivalMovementHeader.BM_GrossWeight);
				AssertEquals("nctsHeader.ArrivalMovementHeader.BM_InlandTransportMode", "2", nctsHeader.ArrivalMovementHeader.BM_InlandTransportMode);

				AssertContainsExactElementsInAnyOrder("containers (BC_SequenceNumber, BC_UnloadedState, BC_ContainerNum, BC_Mode)",
														new (ZShort, ZString, ZString, ZString)[]
															{
																(1, "DEC", "CSQU3054383", "CNT"),
																(2, "DEC", "", "NCT"),
																(3, "NEW", "CONTAINER3", "CNT")
															}, containers.Select(x => (x.BC_SequenceNumber, x.BC_UnloadedState, x.BC_ContainerNum, x.BC_Mode)));
				containerWithSeq1 = containers.Cast<NctsArrivalHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
				AssertEquals("Container with seq 1, has no seals", 0, containerWithSeq1.Seals.Count);
				var container2Seals = containerWithSeq2.Seals;
				AssertEquals("Container with seq 2, has 3 seals", 3, container2Seals.Count);
				AssertContainsExactElementsInAnyOrder("container2Seals (BK_SequenceNumber, BK_UnloadingState, BK_SealNumber)",
														new (ZShort, ZString, ZString)[]
														{
															(1, "DEC", "SEAL1"),
															(2, "DEC", "SEAL2"),
															(3, "NEW", "SEAL12")
														}, container2Seals.Select(x => (x.BK_SequenceNumber, x.BK_UnloadingState, x.BK_SealNumber)));
				var container3Seal = containerWithSeq3.Seals[0];
				AssertEquals("Container with seq 3, only seal's BK_SequenceNumber", (ZShort)1, container3Seal.BK_SequenceNumber);
				AssertEquals("Container with seq 3, only seal's BK_UnloadingState", "NEW", container3Seal.BK_UnloadingState);
				AssertEquals("Container with seq 3, only seal's BK_SealNumber", "SEAL22", container3Seal.BK_SealNumber);

				var movementHeaderSupportingDocuments = nctsHeader.ArrivalMovementHeader.SupportingDocuments;
				var movementHeaderTransportDocuments = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				var movementHeaderReferenceDocuments = nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
				var bill1 = bills.First(x => x.MovementDetail.B9_SeqNo == "1");
				var bill1SupportingDocuments = bill1.SupportingDocuments;
				var bill1TransportDocuments = bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				var bill1ReferenceDocuments = bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
				var bill1GoodsItems = bill1.ArrivalGoodsItems;
				if (shouldAddBillsToArrival)
				{
					AssertContainsExactElementsInAnyOrder("ArrivalMovementHeader transportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
														new (ZShort, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "30", "5678MFB", "ES"),
															(2, "DEC", "31", "2131RTY", "GB"),
															(3, "NEW", "11", "transport2", "DE")
														}, transportInfos.Select(x => (x.TPM_SequenceNumber, x.TPM_TransportState, x.TPM_TypeOfIdentification, x.TPM_IdentificationNumber, x.TPM_RN_NKTransportNationality)));
					AssertContainsExactElementsInAnyOrder("movementHeaderSupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "N111", "M/111IC CV 111/2"),
															(2, "DEC", "N222", "ESHEADER222"),
															(3, "NEW", "4321", "REFERENCE2")
														}, movementHeaderSupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("movementHeaderTransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "N333", "eshouse333"),
															(2, "DEC", "N444", "ESHEADER444"),
															(3, "NEW", "A001", "TRA2")
														}, movementHeaderTransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("movementHeaderReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "Y555", "ESHOUSE555"),
															(2, "DEC", "Y666", "ESHOUSE2666"),
															(3, "NEW", "A003", "REF2")
														}, movementHeaderReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
														new (ZString, ZString, ZDecimal, ZString)[]
														{
															("1", "DEC", 25000m, "KG"),
															("2", "DEC", 26000m, "KG"),
															("3", "NEW", 15m, "T")
														}, bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));
					AssertContainsExactElementsInAnyOrder("bill1.ArrivalTransportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
														new (ZShort, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "20", "5678AAA", "ES"),
															(2, "DEC", "21", "2131BBB", "GB")
														}, bill1.ArrivalTransportInfos.Select(x => (x.TPM_SequenceNumber, x.TPM_TransportState, x.TPM_TypeOfIdentification, x.TPM_IdentificationNumber, x.TPM_RN_NKTransportNationality)));
					AssertContainsExactElementsInAnyOrder("bill1SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "N777", "M/7777C CV 777/2"),
															(2, "DEC", "N888", "ESHEADER888")
														}, bill1SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill1TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "N999", "eshouse999"),
															(2, "DEC", "N101", "ESHEADER101")
														}, bill1TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "Y102", "ESHOUSE102"),
															(2, "DEC", "Y103", "ESHOUSE2103")
															}, bill1ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill1GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
															new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
															{
															(1, 1, "DEC", "CUSCODE1", "44071220", 10000, "KG", 7000, "KG", "Of fir (Abies spp.) and spruce (Picea spp.)-A description"),
															(2, 2, "DEC", "CUSCODE2", "44071190", 15000, "KG", 0, "KG", "Coniferous-Of pine (Pinus spp.)-Other-A description of the goods is added in this element")
															}, bill1GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("ArrivalMovementHeader transportInfos load data (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
														new (ZShort, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "30", "5678MFB", "ES"),
															(2, "DEC", "31", "2131RTY", "ES"),
															(3, "NEW", "11", "transport2", "DE")
														}, transportInfos.Select(x => (x.TPM_SequenceNumber, x.TPM_TransportState, x.TPM_TypeOfIdentification, x.TPM_IdentificationNumber, x.TPM_RN_NKTransportNationality)));
					AssertContainsExactElementsInAnyOrder("movementHeaderSupportingDocuments load data (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(2, "NEW", "1234", "REFERENCE1"),
															(3, "NEW", "4321", "REFERENCE2")
														}, movementHeaderSupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("movementHeaderTransportDocuments load data (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(2, "NEW", "A000", "TRA1"),
															(3, "NEW", "A001", "TRA2")
														}, movementHeaderTransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("movementHeaderReferenceDocuments load data (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(2, "NEW", "A002", "REF1"),
															(3, "NEW", "A003", "REF2")
														}, movementHeaderReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

					AssertContainsExactElementsInAnyOrder("bills load data (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
														new (ZString, ZString, ZDecimal, ZString)[]
														{
															("1", "DEC", 25000m, "KG"),
															("2", "DEC", 26000m, "KG")
														}, bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));
					AssertContainsExactElementsInAnyOrder("bill1GoodsItems load data (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
															new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
															{
															(1, 1, "DEC", "CUSCODE1", "44071220", 10000, "KG", 7000, "KG", "Of fir (Abies spp.) and spruce (Picea spp.)-A description"),
															(2, 2, "DEC", "CUSCODE2", "44071190", 15000, "KG", 11000, "KG", "Coniferous-Of pine (Pinus spp.)-Other-A description of the goods is added in this element")
															}, bill1GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));
				}

				var bill1GoodsItem1 = bill1GoodsItems.First(x => x.BY_LineNo == 1);

				AssertEquals("bill1GoodsItem1.Packages has only one element", 1, bill1GoodsItem1.Packages.Count);
				var bill1GoodsItem1Package = bill1GoodsItem1.Packages[0];
				AssertEquals("bill1GoodsItem1Packages.B5_SequenceNumber", (ZShort)1, bill1GoodsItem1Package.B5_SequenceNumber);
				AssertEquals("bill1GoodsItem1Packages.B5_TypeOfDifference", "DEC", bill1GoodsItem1Package.B5_TypeOfDifference);
				AssertEquals("bill1GoodsItem1Packages.B5_UnitType", "NE", bill1GoodsItem1Package.B5_UnitType);
				AssertEquals("bill1GoodsItem1Packages.B5_UnitCount", 1600, bill1GoodsItem1Package.B5_UnitCount);
				AssertEquals("bill1GoodsItem1Packages.B5_MarksAndNumbers", "MARCA1", bill1GoodsItem1Package.B5_MarksAndNumbers);

				AssertEquals("bill1GoodsItem1Package.ContainersPivot.Count", 0, bill1GoodsItem1Package.ContainersPivot.Count);

				AssertEquals("bill1GoodsItem1.SupportingDocuments is empty", 0, bill1GoodsItem1.SupportingDocuments.Count);
				AssertEquals("bill1GoodsItem1.AdditionalInfos with TRA is empty", 0, bill1GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));
				AssertEquals("bill1GoodsItem1.AdditionalInfos with REF is empty", 0, bill1GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "REF"));

				var bill1GoodsItem2 = bill1GoodsItems.First(x => x.BY_LineNo == 2);

				var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "BX", 15, "MARCA2", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "FR", 1, ZString.Empty, "ford", "mondeo", "12345")
														}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

				var bill1GoodsItem2Package1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package1ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem2Package1Containers);
				var bill1GoodsItem2Package2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq1 }, bill1GoodsItem2Package2Containers);

				var bill1GoodsItem2SupportingDocuments = bill1GoodsItem2.SupportingDocuments;
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "N380", "M/144IC CV 7732/2"),
															(2, "DEC", "N381", "ESHEADER")
														}, bill1GoodsItem2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2TransportDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "N740", "eshouse"),
															(2, "DEC", "N741", "ESHEADER")
														}, bill1GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill1GoodsItem2ReferenceDocuments = bill1GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
				AssertContainsExactElementsInAnyOrder("bill1GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "Y066", "ESHOUSE"),
															(2, "DEC", "Y015", "ESHOUSE2")
														}, bill1GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));

				var bill2 = bills.First(x => x.MovementDetail.B9_SeqNo == "2");
				var bill2SupportingDocuments = bill2.SupportingDocuments;
				var bill2TransportDocuments = bill2.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA");
				var bill2ReferenceDocuments = bill2.AdditionalDocuments.Where(x => x.CSI_SubType == "REF");
				var bill2GoodsItems = bills.First(x => x.MovementDetail.B9_SeqNo == "2").ArrivalGoodsItems;
				if (shouldAddBillsToArrival)
				{
					AssertContainsExactElementsInAnyOrder("bill2.ArrivalTransportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
														new (ZShort, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "20", "5678AAA", "ES"),
															(2, "DEC", "21", "2131BBB", "GB"),
															(3, "NEW", "11", "transport2", "DE")
														}, bill2.ArrivalTransportInfos.Select(x => (x.TPM_SequenceNumber, x.TPM_TransportState, x.TPM_TypeOfIdentification, x.TPM_IdentificationNumber, x.TPM_RN_NKTransportNationality)));
					AssertContainsExactElementsInAnyOrder("bill2SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "N777", "M/7777C CV 777/2"),
															(2, "DEC", "N888", "ESHEADER888"),
															(3, "NEW", "4321", "REFERENCE2")
															}, bill2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "N999", "eshouse999"),
															(2, "DEC", "N101", "ESHEADER101"),
															(3, "NEW", "A001", "TRA2")
															}, bill2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill1ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "Y102", "ESHOUSE102"),
															(2, "DEC", "Y103", "ESHOUSE2103"),
															(3, "NEW", "A003", "REF2")
															}, bill2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill2GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
														new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
														{
															(1, 3, "DEC", "CUSCODE3", "44071221", 11000, "KG", 7100, "KG", "Description3"),
															(2, 4, "DEC", "CUSCODE4", "44071191", shouldHaveGrossWeightFromPrevDocument ? 1 : 15001, "KG", 11001, "KG", "Description4"),
															(3, 2, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
														}, bill2GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("bill2GoodsItems load data (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
														new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
														{
															(1, 3, "DEC", "CUSCODE3", "44071221", 11000, "KG", 7100, "KG", "Description3"),
															(2, 4, "DEC", "CUSCODE4", "44071191", shouldHaveGrossWeightFromPrevDocument ? 1 : 15001, "KG", 11001, "KG", "Description4")
														}, bill2GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));
				}

				var bill2GoodsItem1 = bill2GoodsItems.First(x => x.BY_LineNo == 1);

				AssertEquals("bill2GoodsItem1.Packages has only one element", 1, bill2GoodsItem1.Packages.Count);
				var bill2GoodsItem1Package = bill2GoodsItem1.Packages[0];
				AssertEquals("bill2GoodsItem1Packages.B5_SequenceNumber", (ZShort)1, bill2GoodsItem1Package.B5_SequenceNumber);
				AssertEquals("bill2GoodsItem1Packages.B5_TypeOfDifference", "DEC", bill2GoodsItem1Package.B5_TypeOfDifference);
				AssertEquals("bill2GoodsItem1Packages.B5_UnitType", "NE", bill2GoodsItem1Package.B5_UnitType);
				AssertEquals("bill2GoodsItem1Packages.B5_UnitCount", 1100, bill2GoodsItem1Package.B5_UnitCount);
				AssertEquals("bill2GoodsItem1Packages.B5_MarksAndNumbers", "MARCA3", bill2GoodsItem1Package.B5_MarksAndNumbers);

				AssertEquals("bill2GoodsItem1Package.ContainersPivot.Count", 0, bill2GoodsItem1Package.ContainersPivot.Count);

				AssertEquals("bill2GoodsItem1.SupportingDocuments is empty", 0, bill2GoodsItem1.SupportingDocuments.Count);
				AssertEquals("bill2GoodsItem1.AdditionalInfos with TRA is empty", 0, bill2GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));
				AssertEquals("bill2GoodsItem1.AdditionalInfos with REF is empty", 0, bill2GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "REF"));

				var bill2GoodsItem2 = bill2GoodsItems.First(x => x.BY_LineNo == 2);

				var bill2GoodsItem2Packages = bill2GoodsItem2.Packages;
				if (shouldAddBillsToArrival)
				{
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "BX", 13, "MARCA4", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "FR", 1, ZString.Empty, "ford-mondeo-54321", ZString.Empty, ZString.Empty),
															(3, "NEW", "BX", 2, "marks2", ZString.Empty, ZString.Empty, ZString.Empty)
														}, bill2GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2Packages (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(1, "DEC", "BX", 13, "MARCA4", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "DEC", "FR", 1, ZString.Empty, "ford-mondeo-54321", ZString.Empty, ZString.Empty)
														}, bill2GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));
				}

				var bill2GoodsItem2Package1Containers = bill2GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill2GoodsItem2Package1ContainersPivots.Container", new[] { containerWithSeq2 }, bill2GoodsItem2Package1Containers);
				var bill2GoodsItem2Package2Containers = bill2GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
				AssertContainsExactElementsInAnyOrder("bill2GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq2 }, bill2GoodsItem2Package2Containers);

				var bill2GoodsItem2SupportingDocuments = bill2GoodsItem2.SupportingDocuments;
				var bill2GoodsItem2TransportDocuments = bill2GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "TRA");
				var bill2GoodsItem2ReferenceDocuments = bill2GoodsItem2.AdditionalInfos.Where(x => x.CSI_SubType == "REF");
				if (shouldAddBillsToArrival)
				{
					var bill2GoodsItem2Package3Containers = bill2GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 3).ContainersPivot.Containers;
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2Package2ContainersPivots.Container", new[] { containerWithSeq3 }, bill2GoodsItem2Package3Containers);
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2SupportingDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "N380", "M/144IC CV 7732/2"),
															(2, "DEC", "N381", "ESHEADER"),
															(3, "NEW", "4321", "REFERENCE2")
															}, bill2GoodsItem2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2TransportDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "N740", "eshouse"),
															(2, "DEC", "N741", "ESHEADER"),
															(3, "NEW", "A001", "TRA2")
															}, bill2GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2ReferenceDocuments (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "Y066", "ESHOUSE"),
															(2, "DEC", "Y015", "ESHOUSE2"),
															(3, "NEW", "A003", "REF2")
														}, bill2GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					var bill2GoodsItem3 = bill2GoodsItems.First(x => x.BY_LineNo == 3);
					AssertPackagesWithoutChanges(bill2GoodsItem3, "bill2GoodsItem3Packages", containerWithSeq2, containerWithSeq3, containerWithSeq1);
					AssertSupportingDocWithoutChanges("bill2GoodsItem3SupportingDocuments", bill2GoodsItem3.SupportingDocuments);
					AssertTransportDocWithoutChanges("bill2GoodsItem3TransportDocuments", bill2GoodsItem3.AdditionalInfos);
					AssertReferenceDocWithoutChanges("bill2GoodsItem3ReferenceDocuments", bill2GoodsItem3.AdditionalInfos);
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2SupportingDocuments load data (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "N380", "M/144IC CV 7732/2"),
															(2, "DEC", "N381", "ESHEADER")
															}, bill2GoodsItem2SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2TransportDocuments load data (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
															new (ZInt, ZString, ZString, ZString)[]
															{
															(1, "DEC", "N740", "eshouse"),
															(2, "DEC", "N741", "ESHEADER")
															}, bill2GoodsItem2TransportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
					AssertContainsExactElementsInAnyOrder("bill2GoodsItem2ReferenceDocuments load data (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(1, "DEC", "Y066", "ESHOUSE"),
															(2, "DEC", "Y015", "ESHOUSE2")
														}, bill2GoodsItem2ReferenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
				}
			}
			else
			{
				AssertEquals("nctsHeader.ArrivalMovementHeader.BM_GrossWeight", 20m, nctsHeader.ArrivalMovementHeader.BM_GrossWeight);

				AssertContainsExactElementsInAnyOrder("containers (BC_SequenceNumber, BC_UnloadedState, BC_ContainerNum, BC_Mode)",
														new (ZShort, ZString, ZString, ZString)[]
															{
																(2, "NEW", "CONTAINER2", "CNT"),
																(3, "NEW", "CONTAINER3", "CNT")
															}, containers.Select(x => (x.BC_SequenceNumber, x.BC_UnloadedState, x.BC_ContainerNum, x.BC_Mode)));
				var container1Seal = containerWithSeq2.Seals[0];
				AssertEquals("Container with seq 2, only seal's BK_SequenceNumber", (ZShort)2, container1Seal.BK_SequenceNumber);
				AssertEquals("Container with seq 2, only seal's BK_SealNumber", "SEAL11", container1Seal.BK_SealNumber);
				var container2Seal = containerWithSeq3.Seals[0];
				AssertEquals("Container with seq 3, only seal's BK_SequenceNumber", (ZShort)1, container2Seal.BK_SequenceNumber);
				AssertEquals("Container with seq 3, only seal's BK_SealNumber", "SEAL22", container2Seal.BK_SealNumber);

				AssertArrivalTransportInfosWithoutChanges("ArrivalMovementHeader transportInfos", transportInfos);
				AssertSupportingDocWithoutChanges("ArrivalMovementHeader", nctsHeader.ArrivalMovementHeader.SupportingDocuments);
				AssertTransportDocWithoutChanges("ArrivalMovementHeader", nctsHeader.ArrivalMovementHeader.AdditionalDocuments);
				AssertReferenceDocWithoutChanges("ArrivalMovementHeader", nctsHeader.ArrivalMovementHeader.AdditionalDocuments);
				AssertReferenceDocWithoutChanges("ArrivalMovementHeader", nctsHeader.ArrivalMovementHeader.AdditionalDocuments);

				AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, MovementDetail.B9_UnloadedState, B0_Weight)",
														new (ZString, ZString, ZDecimal, ZString)[]
														{
															("2", "NEW", 10m, "LT"),
															("3", "NEW", 15m, "T")
														}, bills.Select(x => (x.MovementDetail.B9_SeqNo, x.MovementDetail.B9_UnloadedState, x.B0_Weight, x.B0_WeightUQ)));

				var bill2 = bills.First(x => x.MovementDetail.B9_SeqNo == "2");
				AssertArrivalTransportInfosWithoutChanges("bill2", bill2.ArrivalTransportInfos);
				AssertSupportingDocWithoutChanges("bill2", bill2.SupportingDocuments);
				AssertTransportDocWithoutChanges("bill2", docCollectionBill: bill2.AdditionalDocuments);
				AssertReferenceDocWithoutChanges("bill2", docCollectionBill: bill2.AdditionalDocuments);

				AssertGoodsItemsWithoutChanges(bill2, "bill2", containerWithSeq2, containerWithSeq3);
			}

			if (shouldAddBillsToArrival)
			{
				var bill3 = bills.First(x => x.MovementDetail.B9_SeqNo == "3");
				AssertArrivalTransportInfosWithoutChanges("bill3", bill3.ArrivalTransportInfos);
				AssertSupportingDocWithoutChanges("bill3", bill3.SupportingDocuments);
				AssertTransportDocWithoutChanges("bill3", docCollectionBill: bill3.AdditionalDocuments);
				AssertReferenceDocWithoutChanges("bill3", docCollectionBill: bill3.AdditionalDocuments);

				AssertGoodsItemsWithoutChanges(bill3, "bill3", containerWithSeq2, containerWithSeq3, containerWithSeq1);
			}
		});

		void AssertArrivalTransportInfosWithoutChanges(ZString objectName, IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> transportInfos)
		{
			AssertContainsExactElementsInAnyOrder(objectName + " transportInfos (TPM_SequenceNumber, TPM_TransportState, TPM_TypeOfIdentification, TPM_IdentificationNumber, TPM_RN_NKTransportNationality)",
														new (ZShort, ZString, ZString, ZString, ZString)[]
														{
															(2, "NEW", "10", "transport", "FR"),
															(3, "NEW", "11", "transport2", "DE")
														}, transportInfos.Select(x => (x.TPM_SequenceNumber, x.TPM_TransportState, x.TPM_TypeOfIdentification, x.TPM_IdentificationNumber, x.TPM_RN_NKTransportNationality)));
		}

		void AssertGoodsItemsWithoutChanges(NctsBill bill, ZString billName, NctsArrivalHeaderContainer containerWithSeq2, NctsArrivalHeaderContainer containerWithSeq3, NctsArrivalHeaderContainer containerWithSeqExtra = null)
		{
			var billGoodsItems = bill.ArrivalGoodsItems;
			AssertContainsExactElementsInAnyOrder(billName + ".GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_UnloadedState, BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, BY_Description)",
														new (ZShort, ZInt, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZString)[]
														{
															(2, 1, "NEW", "AAA", "11111111", 4, "HG", 6, "G", "somedescription"),
															(3, 2, "NEW", "BBB", "22222222", 3, "T", 7, "LT", "somedescription2")
														}, billGoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_UnloadedState, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_Description)));

			var bill3GoodsItem1 = billGoodsItems.First(x => x.BY_LineNo == 2);
			AssertPackagesWithoutChanges(bill3GoodsItem1, billName + "GoodsItem1Packages", containerWithSeq2, containerWithSeq3, containerWithSeqExtra);
			AssertSupportingDocWithoutChanges(billName + "GoodsItem1SupportingDocuments", bill3GoodsItem1.SupportingDocuments);
			AssertTransportDocWithoutChanges(billName + "GoodsItem1TransportDocuments", bill3GoodsItem1.AdditionalInfos);
			AssertReferenceDocWithoutChanges(billName + "GoodsItem1ReferenceDocuments", bill3GoodsItem1.AdditionalInfos);

			var bill3GoodsItem2 = billGoodsItems.First(x => x.BY_LineNo == 3);
			AssertPackagesWithoutChanges(bill3GoodsItem2, billName + "GoodsItem2Packages", containerWithSeq2, containerWithSeq3, containerWithSeqExtra);
			AssertSupportingDocWithoutChanges(billName + "GoodsItem2SupportingDocuments", bill3GoodsItem2.SupportingDocuments);
			AssertTransportDocWithoutChanges(billName + "GoodsItem2TransportDocuments", bill3GoodsItem2.AdditionalInfos);
			AssertReferenceDocWithoutChanges(billName + "GoodsItem2ReferenceDocuments", bill3GoodsItem2.AdditionalInfos);
		}

		void AssertPackagesWithoutChanges(NctsArrivalCargoDesc goodsItem, ZString goodsItemName, NctsArrivalHeaderContainer containerWithSeq2, NctsArrivalHeaderContainer containerWithSeq3, NctsArrivalHeaderContainer containerWithSeqExtra = null)
		{
			var packages = goodsItem.Packages;
			AssertContainsExactElementsInAnyOrder(goodsItemName + " (B5_SequenceNumber, B5_TypeOfDifference, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
														new (ZShort, ZString, ZString, ZLong, ZString, ZString, ZString, ZString)[]
														{
															(2, "NEW", "FR", 3, ZString.Empty, "vin1", "brand1", "model1"),
															(3, "NEW", "BX", 2, "marks2", ZString.Empty, ZString.Empty, ZString.Empty)
														}, packages.Select(x => (x.B5_SequenceNumber, x.B5_TypeOfDifference, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var package1Containers = packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			var package2Containers = packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 3).ContainersPivot.Containers;

			if (containerWithSeqExtra != null)
			{
				AssertContainsExactElementsInAnyOrder(goodsItemName + ".Package1ContainersPivots.Container", new[] { containerWithSeq3 }, package1Containers);
				AssertContainsExactElementsInAnyOrder(goodsItemName + ".Package2ContainersPivots.Container", new[] { containerWithSeq3 }, package2Containers);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(goodsItemName + ".Package1ContainersPivots.Container", new[] { containerWithSeq3 }, package1Containers);
				AssertContainsExactElementsInAnyOrder(goodsItemName + ".Package2ContainersPivots.Container", new[] { containerWithSeq3 }, package2Containers);
			}
		}

		void AssertSupportingDocWithoutChanges(ZString objectName, INctsSupportingDocumentCollection<NctsSupportingDocument> docCollection)
		{
			AssertContainsExactElementsInAnyOrder(objectName + " (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
														new (ZInt, ZString, ZString, ZString)[]
														{
															(2, "NEW", "1234", "REFERENCE1"),
															(3, "NEW", "4321", "REFERENCE2")
														}, docCollection.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
		}

		void AssertTransportDocWithoutChanges(ZString objectName, INctsAdditionalInfoCollection<NctsAdditionalInfo> docCollection = null, INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> docCollectionBill = null)
		{
			var transportDocuments = docCollectionBill != null ? docCollectionBill.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == "TRA") : docCollection.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == "TRA");
			AssertContainsExactElementsInAnyOrder(objectName + " (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(2, "NEW", "A000", "TRA1"),
															(3, "NEW", "A001", "TRA2")
													}, transportDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
		}

		void AssertReferenceDocWithoutChanges(ZString objectName, INctsAdditionalInfoCollection<NctsAdditionalInfo> docCollection = null, INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> docCollectionBill = null)
		{
			var referenceDocuments = docCollectionBill != null ? docCollectionBill.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == "REF") : docCollection.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == "REF");
			AssertContainsExactElementsInAnyOrder(objectName + " (CSI_LineNo, CSI_Status, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(2, "NEW", "A002", "REF1"),
															(3, "NEW", "A003", "REF2")
													}, referenceDocuments.Select(x => (x.CSI_LineNo, x.CSI_Status, x.CSI_Code, x.CSI_ReferenceNumber)));
		}
	}

	void ProcessPreDeclarationAndAssertData(bool isUpdatePreDeclaration)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType("AUTH", "Authorisation");
		helper.CreateNewOrGetExistingCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateCusMapType("EUNAU", "INW", "Authorisation Codes to Document Type Code", true);
		helper.CreateCusMap("EUNAU", "ACR", "C521", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
		helper.CreateCusMap("EUNAU", "AAA", "C523", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
		helper.CreateCusMap("EUNAU", "SSE", "C523", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, EsCode);
		Factory.Save();

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "123456";
		authorizationHeader.CPH_Type = "ACR";
		authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

		var orgAddressForActor = Factory.New<OrgAddress>();
		orgAddressForActor.OA_Address1 = "Address";
		orgAddressForActor.OA_OH = orgHeader.PK;

		AddPreDeclarationDataToDeparture(orgAddressForActor: orgAddressForActor, shouldSetDifferentItemType: isUpdatePreDeclaration);
		nctsHeader.UpdatePreDeclaration = isUpdatePreDeclaration;

		var expectedMessageInterpretation = GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus("PA - Pre-Declaration", GuaranteeDataNotWrittenOff);

		ProcessAcceptedMessageDeparture_Status(expectedMessageInterpretation, GetAcceptanceTestFilePA(), ESNCTS5DepartureCustomsStatusList.Codes.PreLodged, placeOfUnloadingCode: isUpdatePreDeclaration ? "ESMAD" : "FRPAR");

		CombineAssertions(() =>
		{
			var movementHeader = nctsHeader.MovementHeader;
			var containers = nctsHeader.DepartureHeaderContainers;

			if (isUpdatePreDeclaration)
			{
				AssertEquals("UpdatePreDeclaration was reset to false", false, nctsHeader.UpdatePreDeclaration);
				AssertEquals("BH_ReleaseStatus was reset to empty", ZString.Empty, nctsHeader.BH_ReleaseStatus);
				AssertNewData(movementHeader, containers);
			}
			else
			{
				AssertEquals("UpdatePreDeclaration was left as it was", false, nctsHeader.UpdatePreDeclaration);
				AssertEquals("BH_ReleaseStatus was left as it was", "1", nctsHeader.BH_ReleaseStatus);
				AssertExistingData(movementHeader, containers);
			}
		});

		void AssertNewData(NctsDepartureMovementHeader movementHeader, INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> containers)
		{
			AssertEquals("BM_InBondEntryType", "T1", movementHeader.BM_InBondEntryType);
			AssertEquals("TirCarnetNumber", "TIRNumber", movementHeader.TirCarnetNumber);
			AssertEquals("BM_ReducedDatasetIndicator", false, movementHeader.BM_ReducedDatasetIndicator);
			AssertEquals("BM_SpecificCircumstance", "A20", movementHeader.BM_SpecificCircumstance);
			AssertEquals("BM_RN_NKCountryOfDispatch", "DE", movementHeader.BM_RN_NKCountryOfDispatch);
			AssertEquals("BM_RL_NKDestinationPort", "ES", movementHeader.BM_RL_NKDestinationPort);
			AssertEquals("InlandTransportModeAtDeparture", "3", movementHeader.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", "30", movementHeader.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture", "5678MFB", movementHeader.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", "ES", movementHeader.TransportCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", "2131RTY", movementHeader.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", "FR", movementHeader.Trailer1NationalityAtDeparture);
			AssertEquals("BM_ExportTransportMode", "1", movementHeader.BM_ExportTransportMode);
			AssertEquals("BM_CustomsOfficeAtBorder", "CH006251", movementHeader.BM_CustomsOfficeAtBorder);
			AssertEquals("BM_ActiveBorderIdentificationType", "11", movementHeader.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID", "A_854/ 17-797//124-125", movementHeader.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality", "ES", movementHeader.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber", "ConveyanceRef", movementHeader.BM_ConveyanceNumber);
			AssertEquals("BM_GrossWeight", 51000m, movementHeader.BM_GrossWeight);
			AssertEquals("BM_UniqueConsignmentReference", "UCRCode", movementHeader.BM_UniqueConsignmentReference);
			AssertEquals("GoodsLocation.CGL_AdditionalIdentifier", "ES00010101GENE", movementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("BM_PlaceOfLoading", "FRLOCATION", movementHeader.BM_PlaceOfLoading);
			AssertEquals("BM_PlaceOfUnloading", "ESMAD", movementHeader.BM_PlaceOfUnloading);
			AssertEquals("BM_MethodOfPayment", "S", movementHeader.BM_MethodOfPayment);
			AssertEquals("nctsHeader.PreviousDocuments", 0, nctsHeader.PreviousDocuments.Count);
			AssertEquals("nctsHeader.SupportingDocuments", 0, movementHeader.SupportingDocuments.Count);
			AssertEquals("nctsHeader.AdditionalDocuments with SubType TRA", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType REF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("nctsHeader.AdditionalDocuments with SubType INF", 0, nctsHeader.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));

			AssertContainsExactElementsInAnyOrder("CusAuthorizationUsages (AGC_Code, AGC_Number, AGC_OH_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("ACR", "123456", orgHeader.PK),
															("SSE", "987654", ZGuid.Empty)
													}, nctsHeader.MovementHeader.CusAuthorizationUsages.Select(x => (x.AGC_Code, x.AGC_Number, x.AGC_OH_Owner)));

			AssertEquals("CustomsOffices.DEP", "ES000101", movementHeader.CustomsOffices.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).FirstOrDefault().CY_Data);
			AssertEquals("CustomsOffices.DES", "CH006251", movementHeader.CustomsOffices.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination).FirstOrDefault().CY_Data);
			AssertContainsExactElementsInAnyOrder("CustomsOffices.TRA (CY_Data, CY_Order)",
													new (ZString, ZShort)[]
													{
															("CH004321", 1),
															("CH001234", 3)
													}, movementHeader.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Select(x => (x.CY_Data, x.CY_Order)));
			AssertContainsExactElementsInAnyOrder("CustomsOffices.TXT (CY_Data, CY_Order)",
													new (ZString, ZShort)[]
													{
															("DE001234", 1),
															("DE004321", 4)
													}, movementHeader.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit).Select(x => (x.CY_Data, x.CY_Order)));
			AssertContainsExactElementsInAnyOrder("Guarantees (PW_BondType, PW_BondNumber, PW_BondAmount, PW_Password, PW_Override)",
													new (ZString, ZString, ZDecimal, ZString, ZBool)[]
													{
															("6", ZString.Empty, 0, ZString.Empty, true),
															("6", "Number", 5, "pass", true),
															("4", "RefNum", 10, "pas2", true)
													}, nctsHeader.MovementHeader.Guarantees.Select(x => (x.PW_BondType, x.PW_BondNumber, x.PW_BondAmount, x.PW_Password, x.PW_Override)));
			AssertContainsExactElementsInAnyOrder("CusSupplyChainActors (CFR_Code, CFR_Reference, CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("CS", "actorId1", orgAddressForActor.PK),
															("FW", "actorId2", ZGuid.Empty)
													}, nctsHeader.MovementHeader.CusSupplyChainActors.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));
			AssertContainsExactElementsInAnyOrder("DepartureHeaderContainers (BC_SequenceNumber, BC_ContainerNum, BC_Mode, BC_Seal1, BC_Seal2)",
													new (ZShort, ZString, ZString, ZString, ZString)[]
														{
																(1, "CSQU3054383", "CNT","SEAL", ZString.Empty),
																(2, "", "NCT","SEAL1", "SEAL2")
														}, containers.Select(x => (x.BC_SequenceNumber, x.BC_ContainerNum, x.BC_Mode, x.BC_Seal1, x.BC_Seal2)));
			var containerWithSeq1 = containers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
			AssertEquals("Container with seq 1, has no seals", 0, containerWithSeq1.AdditionalSeals.Count);
			var containerWithSeq2 = containers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 2);
			AssertContainsExactElementsInAnyOrder("containerWithSeq2.AdditionalSeals (BK_SequenceNumber, BK_SealNumber)",
													new (ZShort, ZString)[]
													{
															(3, "SEAL3"),
															(4, "SEAL4")
													}, containerWithSeq2.AdditionalSeals.Select(x => (x.BK_SequenceNumber, x.BK_SealNumber)));
			AssertContainsExactElementsInAnyOrder("CountriesOfRouting (CY_Order, CY_Data)",
													new (ZShort, ZString)[]
													{
															(1, "ES"),
															(2, "FR")
													}, nctsHeader.CountriesOfRouting.Select(x => (x.CY_Order, x.CY_Data)));

			AssertContainsExactElementsInAnyOrder("Bills (MovementDetail.B9_SeqNo, B0_Weight, B0_WeightUQ, B0_TransportPaymentMethod, B0_RN_NKCountryOfExport)",
													new (ZString, ZDecimal, ZString, ZString, ZString)[]
													{
															("1", 25000m, "KG", "A", "FR"),
															("2", 26000m, "KG", ZString.Empty, ZString.Empty)
													}, nctsHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ, x.B0_TransportPaymentMethod, x.B0_RN_NKCountryOfExport)));

			var bill1 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");
			AssertContainsExactElementsInAnyOrder("bill1.CusSupplyChainActorReferences (CFR_Code, CFR_Reference, CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("CS", "actorId3", ZGuid.Empty),
															("FW", "actorId4", ZGuid.Empty)
													}, bill1.CusSupplyChainActorReferences.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));

			AssertContainsExactElementsInAnyOrder("bill2.GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_Type,BY_RN_NKCountryOfDispatch, BY_RN_NKCountryOfDestination, BY_CommercialReferenceNumber, BY_Description" +
																				", BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, IsVehicles)",
													new (ZShort, ZInt, ZString, ZString, ZString, ZString, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZBool)[]
													{
															(1, 1, "T1", "ES", "FR", "UCRCode2", "Of fir (Abies spp.) and spruce (Picea spp.)-A description", "CUSCODE1", "4407122011", 10000, "KG", 7000, "KG", 600, "LT", true),
															(2, 2, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZDecimal.Zero, "KG", ZDecimal.Zero, "KG", ZDecimal.Zero, ZString.Empty, false)
													}, bill1.GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_Type, x.BY_RN_NKCountryOfDispatch, x.BY_RN_NKCountryOfDestination, x.BY_CommercialReferenceNumber, x.BY_Description
																									, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, x.IsVehicles)));
			var bill1GoodsItem1 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 1);
			AssertEquals("bill1GoodsItem1.CusSupplyChainActorReferences", 0, bill1GoodsItem1.CusSupplyChainActorReferences.Count);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.UNDGs (UNDGSubstance.DG_Code)",
													new ZString[] { "1002", "1003" }, bill1GoodsItem1.UNDGs.Select(x => x.UNDGSubstance.DG_Code));
			var bill1GoodsItem1Packages = bill1GoodsItem1.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages (B5_SequenceNumber, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
													new (ZShort, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "FR", ZLong.Zero, ZString.Empty, "VINCODE1", "BRAND1", "MODEL1"),
															(2, "FR", ZLong.Zero, ZString.Empty, "VINCODE2", ZString.Empty, ZString.Empty)
													}, bill1GoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var bill1GoodsItem1Packages1Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages1Containers.Container", new[] { containerWithSeq2 }, bill1GoodsItem1Packages1Containers);
			var bill1GoodsItem1Packages2Containers = bill1GoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1Packages2Containers.Container", new[] { containerWithSeq2 }, bill1GoodsItem1Packages2Containers);

			AssertEquals("bill1.PreviousDocuments", 0, bill1.PreviousDocuments.Count);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem1.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_PackType, CSI_PackQty, CSI_UnitOfQuantity, CSI_Quantity, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString, ZInt, ZString, ZDecimal, ZString)[]
													{
															(1, "A001", "PrevDoc1", 5, "BX", 3, "LT", 600, "ExtraInfo"),
															(2, "A002", "PrevDoc2", ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty)
													}, bill1GoodsItem1.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_PackType, x.CSI_PackQty, x.CSI_UnitOfQuantity, x.CSI_Quantity, x.CSI_ReferenceNumber2)));

			AssertEquals("bill1.SupportingDocuments", 0, bill1.SupportingDocuments.Count);
			AssertEquals("bill1GoodsItem21.SupportingDocuments", 0, bill1GoodsItem1.SupportingDocuments.Count);

			AssertContainsExactElementsInAnyOrder("bill1.AdditionalDocuments with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "N740", "eshouse", ZString.Empty),
															(2, "N741", "ESHEADER", ZString.Empty),
															(3, "N743", "ESHEADER3", ZString.Empty)
													}, bill1.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertEquals("bill1GoodsItem1.AdditionalInfos with SubType TRA", 0, bill1GoodsItem1.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));

			AssertEquals("bill1.AdditionalInfos with SubType REF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertContainsExactElementsInAnyOrder("bill2GoodsItem1.AdditionalInfos with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y066", "ESHOUSE", ZString.Empty),
															(2, "Y015", "ESHOUSE2", ZString.Empty)
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertEquals("bill1.AdditionalInfos with SubType INF", 0, bill1.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));
			AssertContainsExactElementsInAnyOrder("bill2GoodsItem1.AdditionalInfos with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "Y067", ZString.Empty, "ESHOUSE3"),
															(2, "Y016", ZString.Empty, "ESHOUSE4")
													}, bill1GoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			var bill1GoodsItem2 = bill1.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == 2);
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2.CusSupplyChainActorReferences (CFR_Code, CFR_Reference, CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("CS", "actorId5", ZGuid.Empty),
															("FW", "actorId6", ZGuid.Empty)
													}, bill1GoodsItem2.CusSupplyChainActorReferences.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));
			AssertEquals("bill1GoodsItem2.UNDGs", 0, bill1GoodsItem2.UNDGs.Count);
			var bill1GoodsItem2Packages = bill1GoodsItem2.Packages;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages (B5_SequenceNumber, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers, B5_PackageID, B5_Brand, B5_Model)",
													new (ZShort, ZString, ZLong, ZString, ZString, ZString, ZString)[]
													{
															(1, "BX", 15, "MARCA2", ZString.Empty, ZString.Empty, ZString.Empty),
															(2, "NE", 5, "MARCA3", ZString.Empty, ZString.Empty, ZString.Empty)
													}, bill1GoodsItem2Packages.Select(x => (x.B5_SequenceNumber, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers, x.B5_PackageID, x.B5_Brand, x.B5_Model)));

			var bill1GoodsItem2Packages1Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 1).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages1Containers.Container", new[] { containerWithSeq1, containerWithSeq2 }, bill1GoodsItem2Packages1Containers);
			var bill1GoodsItem2Packages2Containers = bill1GoodsItem2Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 2).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2Packages2Containers.Container", new[] { containerWithSeq1, containerWithSeq2 }, bill1GoodsItem2Packages2Containers);

			AssertEquals("bill1GoodsItem2.PreviousDocuments", 0, bill1GoodsItem2.PreviousDocuments.Count);

			AssertContainsExactElementsInAnyOrder("bill1GoodsItem2.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(1, "N380", "M/144IC CV 7732/2"),
															(2, "N381", "ESHEADER")
													}, bill1GoodsItem2.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));

			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType TRA", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType REF", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("bill1GoodsItem2.AdditionalInfos with SubType INF", 0, bill1GoodsItem2.AdditionalInfos.Count(x => x.CSI_SubType == "INF"));

			var bill2 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "2");
			AssertEquals("bill2.CusSupplyChainActorReferences", 0, bill2.CusSupplyChainActorReferences.Count);
			AssertEquals("bill2.GoodsItems", 0, bill2.GoodsItems.Count);
			AssertEquals("bill2.PreviousDocuments", 0, bill2.PreviousDocuments.Count);
			AssertEquals("bill2.SupportingDocuments", 0, bill2.SupportingDocuments.Count);
			AssertEquals("bill2.AdditionalInfos with SubType TRA", 0, bill2.AdditionalDocuments.Count(x => x.CSI_SubType == "TRA"));
			AssertEquals("bill2.AdditionalInfos with SubType REF", 0, bill2.AdditionalDocuments.Count(x => x.CSI_SubType == "REF"));
			AssertEquals("bill2.AdditionalInfos with SubType INF", 0, bill2.AdditionalDocuments.Count(x => x.CSI_SubType == "INF"));
		}

		void AssertExistingData(NctsDepartureMovementHeader movementHeader, INctsDepartureHeaderContainerCollection<NctsDepartureHeaderContainer, NctsHeader> containers)
		{
			AssertEquals("BM_InBondEntryType", "T2F", movementHeader.BM_InBondEntryType);
			AssertEquals("TirCarnetNumber", "CarnetNumber", movementHeader.TirCarnetNumber);
			AssertEquals("BM_ReducedDatasetIndicator", true, movementHeader.BM_ReducedDatasetIndicator);
			AssertEquals("BM_SpecificCircumstance", "XXX", movementHeader.BM_SpecificCircumstance);
			AssertEquals("BM_RN_NKCountryOfDispatch", "FR", movementHeader.BM_RN_NKCountryOfDispatch);
			AssertEquals("BM_RL_NKDestinationPort", "IT", movementHeader.BM_RL_NKDestinationPort);
			AssertEquals("InlandTransportModeAtDeparture", "2", movementHeader.InlandTransportModeAtDeparture);
			AssertEquals("TransportTypeAtDeparture", "20", movementHeader.TransportTypeAtDeparture);
			AssertEquals("TransportAtDeparture", "wagon", movementHeader.TransportAtDeparture);
			AssertEquals("TransportCountryAtDeparture", "GB", movementHeader.TransportCountryAtDeparture);
			AssertEquals("Trailer1IDAtDeparture", ZString.Empty, movementHeader.Trailer1IDAtDeparture);
			AssertEquals("Trailer1NationalityAtDeparture", ZString.Empty, movementHeader.Trailer1NationalityAtDeparture);
			AssertEquals("BM_ExportTransportMode", "8", movementHeader.BM_ExportTransportMode);
			AssertEquals("BM_CustomsOfficeAtBorder", "ES009999", movementHeader.BM_CustomsOfficeAtBorder);
			AssertEquals("BM_ActiveBorderIdentificationType", "80", movementHeader.BM_ActiveBorderIdentificationType);
			AssertEquals("BM_TOLCarrierID", "VESSEL", movementHeader.BM_TOLCarrierID);
			AssertEquals("BM_RN_NKTOLCarrierNationality", "PL", movementHeader.BM_RN_NKTOLCarrierNationality);
			AssertEquals("BM_ConveyanceNumber", "Conveyance", movementHeader.BM_ConveyanceNumber);
			AssertEquals("BM_GrossWeight", 20m, movementHeader.BM_GrossWeight);
			AssertEquals("BM_UniqueConsignmentReference", "reference", movementHeader.BM_UniqueConsignmentReference);
			AssertEquals("GoodsLocation.CGL_AdditionalIdentifier", "goodsLocation", movementHeader.GoodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("BM_PlaceOfLoading", "ESBCN", movementHeader.BM_PlaceOfLoading);
			AssertEquals("BM_PlaceOfUnloading", "FRPAR", movementHeader.BM_PlaceOfUnloading);
			AssertEquals("BM_MethodOfPayment", "A", movementHeader.BM_MethodOfPayment);

			AssertContainsExactElementsInAnyOrder("CusAuthorizationUsages (AGC_Code, AGC_Number, AGC_OH_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("TRD", "Number1", ZGuid.Empty)
													}, nctsHeader.MovementHeader.CusAuthorizationUsages.Select(x => (x.AGC_Code, x.AGC_Number, x.AGC_OH_Owner)));

			AssertEquals("CustomsOffices.DEP", "FR008889", nctsHeader.MovementHeader.CustomsOffices.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).FirstOrDefault().CY_Data);
			AssertEquals("CustomsOffices.DES", "DE000002", nctsHeader.MovementHeader.CustomsOffices.Where(x => x.CY_Code == EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination).FirstOrDefault().CY_Data);

			AssertContainsExactElementsInAnyOrder("CustomsOffices.TRA (CY_Data, CY_Order)",
													new (ZString, ZShort)[]
													{
															("ES000001", 1)
													}, nctsHeader.MovementHeader.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit).Select(x => (x.CY_Data, x.CY_Order)));

			AssertContainsExactElementsInAnyOrder("CustomsOffices.TXT (CY_Data, CY_Order)",
													new (ZString, ZShort)[]
													{
															("DE000001", 1)
													}, nctsHeader.MovementHeader.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit).Select(x => (x.CY_Data, x.CY_Order)));

			AssertContainsExactElementsInAnyOrder("Guarantees (PW_BondType, PW_BondNumber, PW_BondAmount, PW_Password, PW_Override)",
													new (ZString, ZString, ZDecimal, ZString, ZBool)[]
													{
															("3", "GRN1", 0, ZString.Empty, false)
													}, nctsHeader.MovementHeader.Guarantees.Select(x => (x.PW_BondType, x.PW_BondNumber, x.PW_BondAmount, x.PW_Password, x.PW_Override)));

			AssertContainsExactElementsInAnyOrder("CusSupplyChainActors (CFR_Code, CFR_Reference, CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("MF", "actorref", ZGuid.Empty),
															("CS", "actorId1", orgAddressForActor.PK)
													}, nctsHeader.MovementHeader.CusSupplyChainActors.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));

			AssertContainsExactElementsInAnyOrder("DepartureHeaderContainers (BC_SequenceNumber, BC_ContainerNum, BC_Mode, BC_Seal1, BC_Seal2)",
													new (ZShort, ZString, ZString, ZString, ZString)[]
														{
																(1, "CONT1", "CNT","Seal1", "Seal2"),
														}, containers.Select(x => (x.BC_SequenceNumber, x.BC_ContainerNum, x.BC_Mode, x.BC_Seal1, x.BC_Seal2)));

			var containerWithSeq1 = containers.Cast<NctsDepartureHeaderContainer>().First(x => x.BC_SequenceNumber == 1);
			AssertContainsExactElementsInAnyOrder("containerWithSeq1.AdditionalSeals (BK_SequenceNumber, BK_SealNumber)",
													new (ZShort, ZString)[]
													{
															(3, "Seal3")
													}, containerWithSeq1.AdditionalSeals.Select(x => (x.BK_SequenceNumber, x.BK_SealNumber)));

			AssertContainsExactElementsInAnyOrder("CountriesOfRouting (CY_Order, CY_Data)",
													new (ZShort, ZString)[]
													{
															(1, "DE")
													}, nctsHeader.CountriesOfRouting.Select(x => (x.CY_Order, x.CY_Data)));

			AssertContainsExactElementsInAnyOrder("Bills (MovementDetail.B9_SeqNo, B0_Weight, B0_WeightUQ, B0_TransportPaymentMethod, B0_RN_NKCountryOfExport)",
													new (ZString, ZDecimal, ZString, ZString, ZString)[]
													{
															("1", 20m, "G", "S", "ES"),
															("3", 30m, "G", "A", "DE")
													}, nctsHeader.Bills.Select(x => (x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ, x.B0_TransportPaymentMethod, x.B0_RN_NKCountryOfExport)));

			var bill2 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "1");
			AssertContainsExactElementsInAnyOrder("bill2.CusSupplyChainActorReferences (CFR_Code, CFR_Reference, x.CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("MF", "actorref2", ZGuid.Empty)
													}, bill2.CusSupplyChainActorReferences.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));

			AssertGoodsItemsWithoutChanges(bill2, "bill2", 1, containerWithSeq1);

			var bill3 = nctsHeader.Bills.First(x => x.MovementDetail.B9_SeqNo == "3");
			AssertContainsExactElementsInAnyOrder("bill3.CusSupplyChainActorReferences (CFR_Code, CFR_Reference, x.CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("MF", "actorref3", ZGuid.Empty)
													}, bill3.CusSupplyChainActorReferences.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));

			AssertGoodsItemsWithoutChanges(bill3, "bill3", 3, containerWithSeq1);

			#region PreviousDocuments
			AssertContainsExactElementsInAnyOrder("nctsHeader.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(3, "9003", ZString.Empty)
													}, nctsHeader.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));

			AssertContainsExactElementsInAnyOrder("bill2.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(2, "9002", ZString.Empty)
													}, bill2.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));
			#endregion
			#region SupportingDocuments
			AssertContainsExactElementsInAnyOrder("nctsHeader.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(3, "A003", ZString.Empty)
													}, movementHeader.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));

			AssertContainsExactElementsInAnyOrder("bill2.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(4, "Y001", ZString.Empty)
													}, bill2.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));
			#endregion
			#region AdditionalDocuments with SubType TRA
			AssertContainsExactElementsInAnyOrder("nctsHeader.AdditionalDocuments with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "T003", "TRA3", "DescTRA3")
													}, nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("bill2.AdditionalDocuments with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(2, "T002", "TRA2", "DescTRA2")
													}, bill2.AdditionalDocuments.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));
			#endregion
			#region AdditionalDocuments with SubType REF
			AssertContainsExactElementsInAnyOrder("nctsHeader.AdditionalDocuments with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "R003", "REF3", "DescREF3")
													}, nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("bill2.AdditionalDocuments with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(2, "R002", "REF2", "DescREF2")
													}, bill2.AdditionalDocuments.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));
			#endregion
			#region AdditionalDocuments with SubType INF
			AssertContainsExactElementsInAnyOrder("nctsHeader.AdditionalDocuments with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(1, "I003", "INF3", "DescINF3")
													}, nctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder("bill2.AdditionalDocuments with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(2, "I002", "INF2", "DescINF2")
													}, bill2.AdditionalDocuments.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));
			#endregion
		}

		void AssertGoodsItemsWithoutChanges(NctsBill bill, ZString billName, ZInt declarationGoodsItemNumber, NctsDepartureHeaderContainer containerWithSeq1)
		{
			AssertContainsExactElementsInAnyOrder(billName + ".GoodsItems (BY_LineNo, BY_DeclarationGoodsItemNumber, BY_Type,BY_RN_NKCountryOfDispatch, BY_RN_NKCountryOfDestination, BY_CommercialReferenceNumber, BY_Description" +
																				", BY_CusC4Number, BY_HarmonisedTariff, BY_GrossWeight, BY_GrossWeightUnit, BY_NetWeight, BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, IsVehicles)",
													new (ZShort, ZInt, ZString, ZString, ZString, ZString, ZString, ZString, ZString, ZDecimal, ZString, ZDecimal, ZString, ZDecimal, ZString, ZBool)[]
													{
															(5, declarationGoodsItemNumber, "T2F", "DE", "IT", "reference2", "description", "cuscode", "4407122011", 10, "G", 5, "HG", 6, "LT", false)
													}, bill.GoodsItems.Select(x => (x.BY_LineNo, x.BY_DeclarationGoodsItemNumber, x.BY_Type, x.BY_RN_NKCountryOfDispatch, x.BY_RN_NKCountryOfDestination, x.BY_CommercialReferenceNumber, x.BY_Description
																									, x.BY_CusC4Number, x.BY_HarmonisedTariff, x.BY_GrossWeight, x.BY_GrossWeightUnit, x.BY_NetWeight, x.BY_NetWeightUnit, x.BY_CustomsSecondQuantity, x.BY_CustomsSecondUnitQty, x.IsVehicles)));

			var billGoodsItem1 = bill.GoodsItems.Cast<NctsDepartureCargoDesc>().First(x => x.BY_DeclarationGoodsItemNumber == declarationGoodsItemNumber);
			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1.CusSupplyChainActorReferences (CFR_Code, CFR_Reference, CFR_OA_Owner)",
													new (ZString, ZString, ZGuid)[]
													{
															("MF", "actorref3", ZGuid.Empty)
													}, billGoodsItem1.CusSupplyChainActorReferences.Select(x => (x.CFR_Code, x.CFR_Reference, x.CFR_OA_Owner)));

			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1.UNDGs (UNDGSubstance.DG_Code)",
													new ZString[] { "0004a" }, billGoodsItem1.UNDGs.Select(x => x.UNDGSubstance.DG_Code));

			var billGoodsItem1Packages = billGoodsItem1.Packages;
			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1Packages (B5_SequenceNumber, B5_UnitType, B5_UnitCount, B5_MarksAndNumbers)",
													new (ZShort, ZString, ZLong, ZString)[]
													{
															(5, "CT", 9, "marks")
													}, billGoodsItem1Packages.Select(x => (x.B5_SequenceNumber, x.B5_UnitType, x.B5_UnitCount, x.B5_MarksAndNumbers)));

			var billGoodsItem1Packages1Containers = billGoodsItem1Packages.Cast<EU.NCTS.Business.NctsPackage>().First(x => x.B5_SequenceNumber == 5).ContainersPivot.Containers;
			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1Packages1Containers.Container", new[] { containerWithSeq1 }, billGoodsItem1Packages1Containers);

			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1.PreviousDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_ItemNumber, CSI_PackType, CSI_PackQty, CSI_UnitOfQuantity, CSI_Quantity, CSI_ReferenceNumber2)",
													new (ZInt, ZString, ZString, ZInt, ZString, ZInt, ZString, ZDecimal, ZString)[]
													{
															(4, "9001", ZString.Empty, ZInt.Zero, ZString.Empty, ZInt.Zero, ZString.Empty, ZDecimal.Zero, ZString.Empty)
													}, billGoodsItem1.PreviousDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_ItemNumber, x.CSI_PackType, x.CSI_PackQty, x.CSI_UnitOfQuantity, x.CSI_Quantity, x.CSI_ReferenceNumber2)));

			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1.SupportingDocuments (CSI_LineNo, CSI_Code, CSI_ReferenceNumber)",
													new (ZInt, ZString, ZString)[]
													{
															(2, "9005", ZString.Empty)
													}, billGoodsItem1.SupportingDocuments.Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber)));

			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1.AdditionalInfos with SubType TRA (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(3, "T001", "TRA1", "DescTRA1")
													}, billGoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "TRA").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1.AdditionalInfos with SubType REF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(3, "R001", "REF1", "DescREF1")
													}, billGoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "REF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));

			AssertContainsExactElementsInAnyOrder(billName + "GoodsItem1.AdditionalInfos with SubType INF (CSI_LineNo, CSI_Code, CSI_ReferenceNumber, CSI_Description)",
													new (ZInt, ZString, ZString, ZString)[]
													{
															(3, "I001", "INF1", "DescINF1")
													}, billGoodsItem1.AdditionalInfos.Where(x => x.CSI_SubType == "INF").Select(x => (x.CSI_LineNo, x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_Description)));
		}
	}

	void AssertNewTransaction(string message, SharedCusPermitLineTransaction transaction, ZDecimal amount, ZDateTime acceptanceDate)
	{
		AssertEquals(message + ".CPL_Reference", mrnEntryNumber, transaction.CPL_Reference);
		AssertEquals(message + ".CPL_Comment", WriteOffTransactionCommentPrefix + " " + ApplicationReference, transaction.CPL_Comment);
		AssertEquals(message + ".CPL_TranValue", amount, transaction.CPL_TranValue);
		AssertEquals(message + ".CPL_TransactionDate", acceptanceDate, transaction.CPL_TransactionDate);
		AssertEquals(message + ".CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, transaction.CPL_TransactionStatus);
	}

	ZString GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatus(ZString statusDescription, string extraGuaranteeData = "", bool isMrnEntryNumberNoES = false) => ZString.Format("<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>NCTS5 (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>{0}</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>20-11-2020, 00:00:00</td></tr>" +
			"<tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + (isMrnEntryNumberNoES ? mrnEntryNumberNoES : mrnEntryNumber) + "</td></tr></table>" +
			"<br><br><H4>Departure</H4>" +
			"<br><table border=\"0\"><tr><td>Limit date of arrival:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Clearance:</td><td>&nbsp;&nbsp;</td><td>" + CsvClearance + "</td></tr>" +
			"<tr><td>Date:</td><td>&nbsp;&nbsp;</td><td>21-11-2020</td></tr></table>" +
			extraGuaranteeData +
			"<br><br><H4>Arrival</H4>" +
			"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>04-06-2023, 02:01:01</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Location:</td><td>&nbsp;&nbsp;</td><td>nacUbicac1</td></tr></table>" +
			"<br><br><H4>Unloading</H4>" +
			"<br><table border=\"0\"><tr><td>Unloading Result Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Ultimate Date:</td><td>&nbsp;&nbsp;</td><td>02-07-2023</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Result:</td><td>&nbsp;&nbsp;</td><td>Major discrepancies in unloading but unlocked guarantee (B11)</td></tr></table>",
			statusDescription);

	ZString GetExpectedMessageInterpretationTextGreenCircuitWithAllDataForStatusCA_PI_IG_IV(ZString statusDescription, string extraGuaranteeData = "") => ZString.Format("<H4>Management Data</H4>" +
			"<br><table border=\"0\"><tr><td>Version:</td><td>&nbsp;&nbsp;</td><td>NCTS5 (XML)</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Status:</td><td>&nbsp;&nbsp;</td><td>{0}</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Cancellation Date:</td><td>&nbsp;&nbsp;</td><td>03-06-2023</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Register (MRN):</td><td>&nbsp;&nbsp;</td><td>" + mrnEntryNumber + "</td></tr></table>" +
			extraGuaranteeData +
			"<br><br><H4>Arrival</H4>" +
			"<br><table border=\"0\"><tr><td>Arrival Date:</td><td>&nbsp;&nbsp;</td><td>04-06-2023, 02:01:01</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#F57800\">ORANGE</font></strong></td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Location:</td><td>&nbsp;&nbsp;</td><td>nacUbicac1</td></tr></table>" +
			"<br><br><H4>Unloading</H4>" +
			"<br><table border=\"0\"><tr><td>Unloading Result Date:</td><td>&nbsp;&nbsp;</td><td>16-04-2020</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Ultimate Date:</td><td>&nbsp;&nbsp;</td><td>02-07-2023</td></tr></table>" +
			"<br><table border=\"0\"><tr><td>Result:</td><td>&nbsp;&nbsp;</td><td>Major discrepancies in unloading but unlocked guarantee (B11)</td></tr></table>",
			statusDescription);

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PRE", "Customs PRE Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "C01", "Customs C01 Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Customs CAN Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "GIV", "Customs GIV Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "NRL", "Customs NRL Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "REL", "Customs REL Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();
	}

	void AddUnloadingDataToArrival(ZBool isDataLoadFromDeparture, bool shouldAddBillsToArrival = true)
	{
		nctsHeader.ArrivalMovementHeader.BM_GrossWeight = 20m;
		nctsHeader.ArrivalMovementHeader.BM_GrossWeightUQ = "KG";

		var container1 = nctsHeader.ArrivalHeaderContainers.AddNew();
		container1.BC_SequenceNumber = 2;
		container1.BC_UnloadedState = "NEW";
		container1.BC_ContainerNum = "CONTAINER2";
		container1.BC_Mode = "CNT";
		var seal1 = container1.Seals.AddNew();
		seal1.BK_SequenceNumber = 2;
		seal1.BK_UnloadingState = "NEW";
		seal1.BK_SealNumber = "SEAL11";
		var seal2 = container1.Seals.AddNew();
		seal2.BK_SequenceNumber = 3;
		seal2.BK_UnloadingState = "NEW";
		seal2.BK_SealNumber = "SEAL12";

		var container2 = nctsHeader.ArrivalHeaderContainers.AddNew();
		container2.BC_SequenceNumber = 3;
		container2.BC_UnloadedState = "NEW";
		container2.BC_ContainerNum = "CONTAINER3";
		container2.BC_Mode = "CNT";
		var seal3 = container2.Seals.AddNew();
		seal3.BK_SequenceNumber = 1;
		seal3.BK_UnloadingState = "NEW";
		seal3.BK_SealNumber = "SEAL22";

		AddArrivalTransportInfos(nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos);
		AddSupportingDocuments(nctsHeader.ArrivalMovementHeader.SupportingDocuments);
		AddTransportDocuments(nctsHeader.ArrivalMovementHeader.AdditionalDocuments);
		AddReferenceDocuments(nctsHeader.ArrivalMovementHeader.AdditionalDocuments);

		if (shouldAddBillsToArrival)
		{
			var bill1 = nctsHeader.Bills.AddNew();
			bill1.MovementDetail.B9_SeqNo = "2";
			bill1.MovementDetail.B9_UnloadedState = "NEW";
			bill1.B0_Weight = 10m;
			bill1.B0_WeightUQ = "LT";
			AddArrivalTransportInfos(bill1.ArrivalTransportInfos);
			AddSupportingDocuments(bill1.SupportingDocuments);
			AddTransportDocuments(docCollectionBill: bill1.AdditionalDocuments);
			AddReferenceDocuments(docCollectionBill: bill1.AdditionalDocuments);
			AddGoodsItemsToBill(bill1, container2);

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.MovementDetail.B9_SeqNo = "3";
			bill2.MovementDetail.B9_UnloadedState = "NEW";
			bill2.B0_Weight = 15m;
			bill2.B0_WeightUQ = "T";
			AddArrivalTransportInfos(bill2.ArrivalTransportInfos);
			AddSupportingDocuments(bill2.SupportingDocuments);
			AddTransportDocuments(docCollectionBill: bill2.AdditionalDocuments);
			AddReferenceDocuments(docCollectionBill: bill2.AdditionalDocuments);
			AddGoodsItemsToBill(bill2, container2);
		}

		void AddArrivalTransportInfos(IArrivalCusTransportMeansCollection<ArrivalCusTransportMeans> transportMeans)
		{
			var transpInfo1 = transportMeans.AddNew();
			transpInfo1.TPM_SequenceNumber = 2;
			transpInfo1.TPM_TransportState = "NEW";
			transpInfo1.TPM_TypeOfIdentification = "10";
			transpInfo1.TPM_IdentificationNumber = "transport";
			transpInfo1.TPM_RN_NKTransportNationality = "FR";

			var transpInfo2 = transportMeans.AddNew();
			transpInfo2.TPM_SequenceNumber = 3;
			transpInfo2.TPM_TransportState = "NEW";
			transpInfo2.TPM_TypeOfIdentification = "11";
			transpInfo2.TPM_IdentificationNumber = "transport2";
			transpInfo2.TPM_RN_NKTransportNationality = "DE";
		}

		void AddGoodsItemsToBill(NctsBill bill, NctsArrivalHeaderContainer container)
		{
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_LineNo = 2;
			goodsItem1.BY_DeclarationGoodsItemNumber = 1;
			goodsItem1.BY_UnloadedState = "NEW";
			goodsItem1.BY_Description = "somedescription";
			goodsItem1.BY_CusC4Number = "AAA";
			goodsItem1.BY_HarmonisedTariff = "11111111";
			goodsItem1.BY_GrossWeight = 4;
			goodsItem1.BY_GrossWeightUnit = "HG";
			goodsItem1.BY_NetWeight = 6;
			goodsItem1.BY_NetWeightUnit = "G";

			AddPackagesToGoodsItem(goodsItem1, container, isDataLoadFromDeparture);
			AddSupportingDocuments(goodsItem1.SupportingDocuments);
			AddTransportDocuments(goodsItem1.AdditionalInfos);
			AddReferenceDocuments(goodsItem1.AdditionalInfos);

			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_LineNo = 3;
			goodsItem2.BY_DeclarationGoodsItemNumber = 2;
			goodsItem2.BY_UnloadedState = "NEW";
			goodsItem2.BY_Description = "somedescription2";
			goodsItem2.BY_CusC4Number = "BBB";
			goodsItem2.BY_HarmonisedTariff = "22222222";
			goodsItem2.BY_GrossWeight = 3;
			goodsItem2.BY_GrossWeightUnit = "T";
			goodsItem2.BY_NetWeight = 7;
			goodsItem2.BY_NetWeightUnit = "LT";

			AddPackagesToGoodsItem(goodsItem2, container, isDataLoadFromDeparture);
			AddSupportingDocuments(goodsItem2.SupportingDocuments);
			AddTransportDocuments(goodsItem2.AdditionalInfos);
			AddReferenceDocuments(goodsItem2.AdditionalInfos);
		}

		void AddPackagesToGoodsItem(NctsArrivalCargoDesc goodsItem, NctsArrivalHeaderContainer container, bool isDataLoadFromDeparture = false)
		{
			var pack1 = goodsItem.Packages.AddNew();
			pack1.B5_SequenceNumber = 2;
			pack1.B5_TypeOfDifference = "NEW";
			pack1.B5_UnitType = "FR";
			pack1.B5_UnitCount = 3;
			pack1.B5_PackageID = "vin1";
			pack1.B5_Brand = "brand1";
			pack1.B5_Model = "model1";
			pack1.IsDataLoadFromDeparture = isDataLoadFromDeparture;
			pack1.ContainersPivot.AddPivotFor(container);

			var pack2 = goodsItem.Packages.AddNew();
			pack2.B5_SequenceNumber = 3;
			pack2.B5_TypeOfDifference = "NEW";
			pack2.B5_UnitType = "BX";
			pack2.B5_UnitCount = 2;
			pack2.B5_MarksAndNumbers = "marks2";
			pack2.IsDataLoadFromDeparture = isDataLoadFromDeparture;
			pack2.ContainersPivot.AddPivotFor(container);
		}

		void AddSupportingDocuments(INctsSupportingDocumentCollection<NctsSupportingDocument> docCollection)
		{
			var supDoc1 = docCollection.AddNew();
			supDoc1.CSI_LineNo = 2;
			supDoc1.CSI_Status = "NEW";
			supDoc1.CSI_Code = "1234";
			supDoc1.CSI_ReferenceNumber = "REFERENCE1";

			var supDoc2 = docCollection.AddNew();
			supDoc2.CSI_LineNo = 3;
			supDoc2.CSI_Status = "NEW";
			supDoc2.CSI_Code = "4321";
			supDoc2.CSI_ReferenceNumber = "REFERENCE2";
		}

		void AddTransportDocuments(INctsAdditionalInfoCollection<NctsAdditionalInfo> docCollection = null, INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> docCollectionBill = null)
		{
			var transpDoc1 = (CusSupportingInfo)(docCollectionBill != null ? docCollectionBill.AddNew() : docCollection.AddNew());
			transpDoc1.CSI_Code = "A000";
			transpDoc1.CSI_SubType = "TRA";
			transpDoc1.CSI_Status = "NEW";
			transpDoc1.CSI_ReferenceNumber = "TRA1";
			transpDoc1.CSI_LineNo = 2;

			var transpDoc2 = (CusSupportingInfo)(docCollectionBill != null ? docCollectionBill.AddNew() : docCollection.AddNew());
			transpDoc2.CSI_Code = "A001";
			transpDoc2.CSI_SubType = "TRA";
			transpDoc2.CSI_Status = "NEW";
			transpDoc2.CSI_ReferenceNumber = "TRA2";
			transpDoc2.CSI_LineNo = 3;
		}

		void AddReferenceDocuments(INctsAdditionalInfoCollection<NctsAdditionalInfo> docCollection = null, INctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument> docCollectionBill = null)
		{
			var refDoc1 = (CusSupportingInfo)(docCollectionBill != null ? docCollectionBill.AddNew() : docCollection.AddNew());
			refDoc1.CSI_Code = "A002";
			refDoc1.CSI_SubType = "REF";
			refDoc1.CSI_Status = "NEW";
			refDoc1.CSI_ReferenceNumber = "REF1";
			refDoc1.CSI_LineNo = 2;

			var refDoc2 = (CusSupportingInfo)(docCollectionBill != null ? docCollectionBill.AddNew() : docCollection.AddNew());
			refDoc2.CSI_Code = "A003";
			refDoc2.CSI_SubType = "REF";
			refDoc2.CSI_Status = "NEW";
			refDoc2.CSI_ReferenceNumber = "REF2";
			refDoc2.CSI_LineNo = 3;
		}
	}

	void AddPreDeclarationDataToDeparture(bool addSecondGoodsItem = false, bool shouldSetDifferentItemType = false, int secondGoodsItemDeclarationGoodsItemNumber1 = 2, int secondGoodsItemDeclarationGoodsItemNumber2 = 4, OrgAddress orgAddressForActor = null)
	{
		nctsHeader.BH_ReleaseStatus = "1";

		var movementHeader = nctsHeader.MovementHeader;

		movementHeader.BM_InBondEntryType = "T2F";
		movementHeader.TirCarnetNumber = "CarnetNumber";
		movementHeader.BM_ReducedDatasetIndicator = true;
		movementHeader.BM_SpecificCircumstance = "XXX";
		movementHeader.BM_RN_NKCountryOfDispatch = "FR";
		movementHeader.BM_RL_NKDestinationPort = "IT";
		movementHeader.InlandTransportModeAtDeparture = "2";
		movementHeader.TransportTypeAtDeparture = "20";
		movementHeader.TransportAtDeparture = "wagon";
		movementHeader.TransportCountryAtDeparture = "GB";
		movementHeader.BM_ExportTransportMode = "8";
		movementHeader.BM_CustomsOfficeAtBorder = "ES009999";
		movementHeader.BM_ActiveBorderIdentificationType = "80";
		movementHeader.BM_TOLCarrierID = "Vessel";
		movementHeader.BM_RN_NKTOLCarrierNationality = "PL";
		movementHeader.BM_ConveyanceNumber = "Conveyance";
		movementHeader.BM_GrossWeight = 20m;
		movementHeader.BM_GrossWeightUQ = "KG";
		movementHeader.BM_UniqueConsignmentReference = "reference";
		movementHeader.GoodsLocation.CGL_AdditionalIdentifier = "goodsLocation";
		movementHeader.BM_PlaceOfLoading = "ESBCN";
		movementHeader.BM_PlaceOfUnloading = "FRPAR";
		movementHeader.BM_MethodOfPayment = "A";

		var auth1 = nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew();
		auth1.AGC_Code = "TRD";
		auth1.AGC_Number = "Number1";

		var customsOffice1 = movementHeader.CustomsOffices.AddNew();
		customsOffice1.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
		customsOffice1.CY_Data = "FR008889";

		var customsOffice2 = movementHeader.CustomsOffices.AddNew();
		customsOffice2.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;
		customsOffice2.CY_Data = "DE000002";

		var customsOffice3 = movementHeader.CustomsOffices.AddNew();
		customsOffice3.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit;
		customsOffice3.CY_Data = "ES000001";

		var customsOffice4 = movementHeader.CustomsOffices.AddNew();
		customsOffice4.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit;
		customsOffice4.CY_Data = "DE000001";

		var guarantee = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee.PW_BondType = "3";
		guarantee.PW_BondNumber = "GRN1";

		var actor1 = nctsHeader.MovementHeader.CusSupplyChainActors.AddNew();
		actor1.CFR_Code = "MF";
		actor1.CFR_Reference = "actorref";

		var actor2 = nctsHeader.MovementHeader.CusSupplyChainActors.AddNew();
		actor2.CFR_Code = "CS";
		actor2.CFR_Reference = "actorId1";
		actor2.CFR_OA_Owner = orgAddressForActor?.PK ?? ZGuid.Empty;

		var cont1 = nctsHeader.DepartureHeaderContainers.AddNew();
		cont1.BC_Mode = "CNT";
		cont1.BC_ContainerNum = "CONT1";
		cont1.Seal1 = "Seal1";
		cont1.Seal2 = "Seal2";
		cont1.BC_SequenceNumber = 1;
		cont1.AdditionalSeals.AddNew().BK_SealNumber = "Seal3";

		nctsHeader.CountriesOfRouting.AddNew().CY_Data = "DE";

		var bill1 = nctsHeader.Bills.AddNew();
		bill1.MovementDetail.B9_SeqNo = "1";
		bill1.B0_Weight = 20;
		bill1.B0_WeightUQ = "G";
		bill1.B0_TransportPaymentMethod = "S";
		bill1.B0_RN_NKCountryOfExport = "ES";
		var billActor1 = bill1.CusSupplyChainActorReferences.AddNew();
		billActor1.CFR_Code = "MF";
		billActor1.CFR_Reference = "actorref2";
		AddGoodsItemsToBill(bill1, 1, "T2F", secondGoodsItemDeclarationGoodsItemNumber1);

		var bill2 = nctsHeader.Bills.AddNew();
		bill2.MovementDetail.B9_SeqNo = "3";
		bill2.B0_Weight = 30;
		bill2.B0_WeightUQ = "G";
		bill2.B0_TransportPaymentMethod = "A";
		bill2.B0_RN_NKCountryOfExport = "DE";
		var billActor2 = bill2.CusSupplyChainActorReferences.AddNew();
		billActor2.CFR_Code = "MF";
		billActor2.CFR_Reference = "actorref3";
		AddGoodsItemsToBill(bill2, 3, shouldSetDifferentItemType ? "T1" : "T2F", secondGoodsItemDeclarationGoodsItemNumber2);

		var prevdoc2 = bill1.PreviousDocuments.AddNew();
		prevdoc2.CSI_Code = "9002";
		prevdoc2.CSI_LineNo = 2;

		var prevdoc3 = nctsHeader.PreviousDocuments.AddNew();
		prevdoc3.CSI_Code = "9003";
		prevdoc3.CSI_LineNo = 3;

		var supdoc2 = bill1.SupportingDocuments.AddNew();
		supdoc2.CSI_Code = "Y001";
		supdoc2.CSI_LineNo = 4;

		var supdoc3 = movementHeader.SupportingDocuments.AddNew();
		supdoc3.CSI_Code = "A003";
		supdoc3.CSI_LineNo = 3;

		var addInfo2TRA = bill1.AdditionalDocuments.AddNew();
		addInfo2TRA.CSI_Code = "T002";
		addInfo2TRA.CSI_SubType = "TRA";
		addInfo2TRA.CSI_ReferenceNumber = "TRA2";
		addInfo2TRA.CSI_Description = "DescTRA2";
		addInfo2TRA.CSI_LineNo = 2;

		var addInfo3TRA = nctsHeader.AdditionalDocuments.AddNew();
		addInfo3TRA.CSI_Code = "T003";
		addInfo3TRA.CSI_SubType = "TRA";
		addInfo3TRA.CSI_ReferenceNumber = "TRA3";
		addInfo3TRA.CSI_Description = "DescTRA3";
		addInfo3TRA.CSI_LineNo = 1;

		var addInfo2REF = bill1.AdditionalDocuments.AddNew();
		addInfo2REF.CSI_Code = "R002";
		addInfo2REF.CSI_SubType = "REF";
		addInfo2REF.CSI_ReferenceNumber = "REF2";
		addInfo2REF.CSI_Description = "DescREF2";
		addInfo2REF.CSI_LineNo = 2;

		var addInfo3REF = nctsHeader.AdditionalDocuments.AddNew();
		addInfo3REF.CSI_Code = "R003";
		addInfo3REF.CSI_SubType = "REF";
		addInfo3REF.CSI_ReferenceNumber = "REF3";
		addInfo3REF.CSI_Description = "DescREF3";
		addInfo3REF.CSI_LineNo = 1;

		var addInfo2INF = bill1.AdditionalDocuments.AddNew();
		addInfo2INF.CSI_Code = "I002";
		addInfo2INF.CSI_SubType = "INF";
		addInfo2INF.CSI_ReferenceNumber = "INF2";
		addInfo2INF.CSI_Description = "DescINF2";
		addInfo2INF.CSI_LineNo = 2;

		var addInfo3INF = nctsHeader.AdditionalDocuments.AddNew();
		addInfo3INF.CSI_Code = "I003";
		addInfo3INF.CSI_SubType = "INF";
		addInfo3INF.CSI_ReferenceNumber = "INF3";
		addInfo3INF.CSI_Description = "DescINF3";
		addInfo3INF.CSI_LineNo = 1;

		void AddGoodsItemsToBill(NctsBill bill, ZInt declarationGoodsItemNumber, ZString type, ZInt secondGoodsItemDeclarationGoodsItemNumber)
		{
			var goodsItem = bill.GoodsItems.AddNew();
			goodsItem.BY_LineNo = 5;
			goodsItem.BY_DeclarationGoodsItemNumber = declarationGoodsItemNumber;
			goodsItem.BY_Type = type;
			goodsItem.BY_RN_NKCountryOfDispatch = "DE";
			goodsItem.BY_RN_NKCountryOfDestination = "IT";
			goodsItem.BY_CommercialReferenceNumber = "reference2";
			goodsItem.BY_Description = "description";
			goodsItem.BY_CusC4Number = "cuscode";
			goodsItem.BY_HarmonisedTariff = "4407122011";
			goodsItem.BY_GrossWeight = 10;
			goodsItem.BY_GrossWeightUnit = "G";
			goodsItem.BY_NetWeight = 5;
			goodsItem.BY_NetWeightUnit = "HG";
			goodsItem.BY_CustomsSecondQuantity = 6;
			goodsItem.BY_CustomsSecondUnitQty = "LT";
			goodsItem.IsVehicles = false;
			goodsItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Spain;
			goodsItem.BY_MonetaryValue = 1_000m;
			var supplementaryCode1 = goodsItem.AdditionalSupplementaryCodes.AddNew();
			var supplementaryCode2 = goodsItem.AdditionalSupplementaryCodes.AddNew();
			supplementaryCode1.CY_Code = "AC01";
			supplementaryCode2.CY_Code = "AC02";

			var goodsItemActor = goodsItem.CusSupplyChainActorReferences.AddNew();
			goodsItemActor.CFR_Code = "MF";
			goodsItemActor.CFR_Reference = "actorref3";

			var dangerousGood1 = goodsItem.UNDGs.AddNew();
			dangerousGood1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = "marks";
			package1.B5_UnitType = "CT";
			package1.B5_UnitCount = 9;
			package1.B5_SequenceNumber = 5;
			package1.ContainersPivot.AddPivotFor(cont1);
			package1.ContainersPivotsForBindingOnly[0].ContainerSelected = true;

			var prevdoc1 = goodsItem.PreviousDocuments.AddNew();
			prevdoc1.CSI_Code = "9001";
			prevdoc1.CSI_LineNo = 4;

			var supdoc1 = goodsItem.SupportingDocuments.AddNew();
			supdoc1.CSI_Code = "9005";
			supdoc1.CSI_LineNo = 2;

			var addInfo1TRA = goodsItem.AdditionalInfos.AddNew();
			addInfo1TRA.CSI_Code = "T001";
			addInfo1TRA.CSI_SubType = "TRA";
			addInfo1TRA.CSI_ReferenceNumber = "TRA1";
			addInfo1TRA.CSI_Description = "DescTRA1";
			addInfo1TRA.CSI_LineNo = 3;

			var addInfo1REF = goodsItem.AdditionalInfos.AddNew();
			addInfo1REF.CSI_Code = "R001";
			addInfo1REF.CSI_SubType = "REF";
			addInfo1REF.CSI_ReferenceNumber = "REF1";
			addInfo1REF.CSI_Description = "DescREF1";
			addInfo1REF.CSI_LineNo = 3;

			var addInfo1INF = goodsItem.AdditionalInfos.AddNew();
			addInfo1INF.CSI_Code = "I001";
			addInfo1INF.CSI_SubType = "INF";
			addInfo1INF.CSI_ReferenceNumber = "INF1";
			addInfo1INF.CSI_Description = "DescINF1";
			addInfo1INF.CSI_LineNo = 3;

			if (addSecondGoodsItem)
			{
				var goodsItem2 = bill.GoodsItems.AddNew();
				goodsItem2.BY_LineNo = 6;
				goodsItem2.BY_DeclarationGoodsItemNumber = secondGoodsItemDeclarationGoodsItemNumber;
			}
		}
	}

	void AddGuarantees(bool addTransactions = false, bool addOBLTransaction = true, bool setPositiveTranAmount = false, string guaranteeReference = GuaranteeReference, string otherGuaranteeReference = OtherGuaranteeReference)
	{
		var balance = addTransactions ? 1000m + (-50m - 200) : 1000m;
		var transactionAmount = setPositiveTranAmount ? 150m : -50m;
		SetUpGuarantee(guaranteeReference, EUGuaranteeTypeList.Codes.TRA, mrnEntryNumber, ApplicationReference, transactionAmount, 1000m, balance, addTransactions, addOBLTransaction);
		var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee1.PW_BondNumber = guaranteeReference;
		guarantee1.PW_BondAmount = 1000m;

		balance = addTransactions ? 1000m + (-540m - 200) : 1200m;
		transactionAmount = setPositiveTranAmount ? 640m : -540m;
		SetUpGuarantee(otherGuaranteeReference, EUGuaranteeTypeList.Codes.TRA, mrnEntryNumber, ApplicationReference, transactionAmount, 1200m, balance, addTransactions, addOBLTransaction);
		var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
		guarantee2.PW_BondNumber = otherGuaranteeReference;
		guarantee2.PW_BondAmount = 1200m;
	}

	void SetupLiabilityRates()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		_ = helper.CreateRefCusTaxOrFeeType("VAT");
		var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");

		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey, "Turkey", euGrouping);
		var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Spain, Customs.Universal.Constants.TariffTypes.Import, nomenclatureGroupType: "ES", ensureDataGroupingExists: false);
		Factory.Save();

		var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Spain, tariffType.PK, "4407122011", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 1");
		
		var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Turkey, "EU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, ensureDataGroupingExists: false);
		_ = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Spain);
		var rateType1 = helper.CreateCusRateType(Core.Constants.CountryCodes.Spain, Customs.Universal.Constants.RateTypes.Duty, ensureDataGroupingExists: false);
		var rateType2 = helper.CreateCusRateType(Core.Constants.CountryCodes.Spain, Customs.Universal.Constants.RateTypes.AntiDumping, ensureDataGroupingExists: false);
		var rateType3 = helper.CreateCusRateType(Core.Constants.CountryCodes.Spain, Customs.Universal.Constants.RateTypes.Countervailing, ensureDataGroupingExists: false);
		var rateCode1 = helper.CreateCusRateCode(Factory, "RC1", rateType1.PK);
		var rateCode2 = helper.CreateCusRateCode(Factory, "RC2", rateType2.PK);
		var rateCode3 = helper.CreateCusRateCode(Factory, "RC3", rateType3.PK);

		var testRate1 = helper.CreateRate(cusTariff, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "  4.200 * [FLAT]");
		var testRate2 = helper.CreateRate(cusTariff, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, " 9.0010 * [FLAT]");
		var testRate3 = helper.CreateRate(cusTariff, rateCode3.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "6.00000 * [FLAT]");
		_ = helper.CreateCusApplicability(testRate1, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateCusApplicability(testRate2, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "AC01");
		_ = helper.CreateCusApplicability(testRate3, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "AC02");
		Factory.Save();
	}

	protected override string GetRejectedTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "RejectedMessage.txt");

	protected override string GetErrorTestFile() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "ErrorMessage.txt");

	readonly ZString mrnEntryNumber = "23ES000101500190J9";
	readonly ZString mrnEntryNumberNoES = "23FR000101500190J9";
	const string CsvClearance = "CD89SVWH9K852Q6M";
	readonly ZDateTime admissionDate = new ZDateTime(2020, 11, 20);
	readonly ZDateTime receptionDate = new ZDateTime(2023, 06, 04, 02, 01, 01);

	const string GuaranteeReference = "16ESAGL9990000096";
	const string OtherGuaranteeReference = "17ESAGL9990000097";
	const string WriteOffTransactionCommentPrefix = "Write-off NCTS Departure";

	const string GuaranteeDataNotWrittenOff = "<br><H2>Guarantees</H2>" +
												"<br><table border=\"0\"><tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off</td></tr></table>";

	const string GuaranteeDataULNotWrittenOffPositiveBalance = "<br><H2>Guarantees</H2>" +
																"<br><table border=\"0\"><tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Not Written Off (at least one positive balance)</td></tr></table>";

	const string GuaranteeDataULWrittenOff = "<br><H2>Guarantees</H2>" +
											"<br><table border=\"0\"><tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Written Off</td></tr></table>";

	const string GuaranteeDataULExcluded = "<br><H2>Guarantees</H2>" +
											"<br><table border=\"0\"><tr><td>Guarantee Status:</td><td>&nbsp;&nbsp;</td><td>Excluded (no pending debt)</td></tr></table>";

	string GetAcceptanceTestFilePA() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusPA.txt");

	string GetAcceptanceTestFileCA() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusCA.txt");

	string GetAcceptanceTestFilePG() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusPG.txt");

	string GetAcceptanceTestFilePD() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusPD.txt");

	string GetAcceptanceTestFileDE() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusDE.txt");

	string GetAcceptanceTestFilePI() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusPI.txt");

	string GetAcceptanceTestFileIG() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusIG.txt");

	string GetAcceptanceTestFileIV() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusIV.txt");

	string GetAcceptanceTestFileNL() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusNL.txt");

	string GetAcceptanceTestFileRQ() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusRQ.txt");

	string GetAcceptanceTestFileRZ() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusRZ.txt");

	string GetAcceptanceTestFileRE() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusRE.txt");

	string GetAcceptanceTestFileLI() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusLI.txt");

	string GetAcceptanceTestFileUC() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusUC.txt");

	string GetAcceptanceTestFileRD() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusRD.txt");

	string GetAcceptanceTestFileUL() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusUL.txt");

	string GetAcceptanceTestFileRE_MrnESWithPreviousDocuments() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusREMrnESWithPreviousDocuments.txt");

	string GetAcceptanceTestFileRE_MrnNoESWithPreviousDocuments() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusREMrnNoESWithPreviousDocuments.txt");

	string GetAcceptanceTestFilREWithoutCommodityCodeAndGoodsMeasure() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusREWithoutCommodityCodeAndGoodsMeasure.txt");

	string GetAcceptanceTestFileULWithoutReceptionCircuitOrDate() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusULWithoutReceptionCircuitOrDate.txt");

	protected override string GetAcceptanceTestFileWithLongSegmentId() => ESNctsTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageWithLongSegmentId.txt");

	protected override ZString GetExpectedProcessorFriendlyName() => "NCTS Query Declaration Message Processor";

	protected override ZString[] GetExpectedProcessorMessageTypesToInclude() => new ZString[] { DeclarationMessageTypeList.Codes.TransitNcts5Query };

	protected override QueryNCTSResponseMessageProcessor GetNewResponseMessageProcessor(LoggingInformation logger) => new QueryNCTSResponseMessageProcessor(logger);
}
