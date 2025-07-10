using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeaderImport;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalReferenceConstants = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransactionsDataTransferProcessor))]
	sealed class CusTempStorageRegLineTransactionsDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport_NoMatchOrCls() => CombineAssertions(() =>
		{
			var reference = "99985000113";
			var intReference = "TestNoMatchOrCls";

			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			var testCase = $"{intReference}: unknown TSD Number";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow("INV reference", "xxx");
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: INV reference, Item Number: 1, Package Type: BG] Excluded: no match or status=CLS");

			ResetImportCollection();

			testCase = $"{intReference}: unknown Customs Location";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, "xxx", customsLocation: "INV");
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: BG] Excluded: no match or status=CLS");

			ResetImportCollection();

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Closed;

			testCase = $"{intReference}: RegHeader closed";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, "xxx");
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: BG] Excluded: no match or status=CLS");

			ResetImportCollection();

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Closed;

			testCase = $"{intReference}: RegLine closed";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, "xxx");
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: BG] Excluded: no match or status=CLS");

			ResetImportCollection();

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			testCase = $"{intReference}: no corresponding Goods Item found";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, "xxx", goodsItemNumber: 5);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 5, Package Type: BG] Excluded: no match or status=CLS");
		});

		public void TestImport_SaveException() => CombineAssertions(() =>
		{
			var reference = "99985000113";
			var intReference = "TestSaveException";

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			tsRegLine2.SRL_PackageType = "BG";
			tsRegLine2.SRL_PackageMarks = "1234";

			AddFlatRow(reference, intReference, pckQty: -3, grossWeight: -1800);
			Factory.Saving += (f) => throw new ZSaveException(new DataExceptionForTesting("saving test error"), f);
			AssertProcessorStatus(RunImport(), intReference, 1, 0, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: BG] Unable to save: <ROW IS NULL>\r\nInnerException Message = saving test error\r\n");
			AssertGuaranteeWriteOffTransaction(intReference, reference, shouldExist: false);
		});

		public void TestImport_Vehicle() => CombineAssertions(() =>
		{
			var reference = "99985000113";
			var intReference = "TestVehicle";

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			tsRegLine2.SRL_PackageType = UniversalReferenceConstants.PackageType.Frame;
			tsRegLine2.SRL_PackageMarks = "ES-12";

			var testCase = $"{intReference}: no matching package marks";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckType: UniversalReferenceConstants.PackageType.Frame, pckMarks: "XX-XX", pckQty: 1);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: FR] Excluded: no match or status=CLS");

			ResetImportCollection();

			testCase = $"{intReference}: package quantity not -1";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckType: UniversalReferenceConstants.PackageType.Frame, pckMarks: "ES-12", pckQty: 1);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: FR] Excluded: For Vehicles (FR) Package Quantity must be -1");

			ResetImportCollection();

			testCase = $"{intReference}: valid row";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckType: UniversalReferenceConstants.PackageType.Frame, pckMarks: "ES-12", pckQty: -1);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 3, 0, 0);
			AssertCusTempStorageRegLineTransaction(testCase, intReference, -1);
			AssertGuaranteeWriteOffTransaction(testCase, reference, expectedTranValue: 5m);
		});

		public void TestImport_Bulk() => CombineAssertions(() =>
		{
			var reference = "99985000113";
			var intReference = "TestBulk";

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			tsRegLine2.SRL_PackageType = "VQ";
			tsRegLine2.SRL_PackageMarks = "1234";

			var testCase = $"{intReference}: not enough Remaining Gross Weight";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckType: "VQ", grossWeight: -1800);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: VQ] Excluded: There is not enough Remaining Gross Weight for this Line");

			ResetImportCollection();

			testCase = $"{intReference}: valid row";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckType: "VQ");
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 3, 0, 0);
			AssertCusTempStorageRegLineTransaction(testCase, intReference, 0);
			AssertGuaranteeWriteOffTransaction(testCase, reference, expectedTranValue: 5m);
		});

		public void TestImport_Others() => CombineAssertions(() =>
		{
			var reference = "99985000113";
			var intReference = "TestOthers";

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			tsRegLine2.SRL_PackageType = "BG";
			tsRegLine2.SRL_PackageMarks = "1234";

			var testCase = $"{intReference}: not enough Remaining Packages";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckQty: -4, grossWeight: -1800);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: BG] Excluded: There are not enough Remaining Packages for this Line");

			ResetImportCollection();

			testCase = $"{intReference}: not enough Remaining Gross Weight";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckQty: -2, grossWeight: -1800);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), testCase, 3, 2, 0, 1, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: BG] Excluded: There is not enough Remaining Gross Weight for this Line");
	
			ResetImportCollection();

			testCase = $"{intReference}: valid row";
			AddFlatRow(reference, "3", goodsItemNumber: 3, pckMarks: "3456");
			AddFlatRow(reference, intReference, pckQty: -3, grossWeight: -1800);
			AddFlatRow(reference, "4", goodsItemNumber: 4, pckMarks: "4567");
			AssertProcessorStatus(RunImport(), intReference, 3, 3, 0, 0);
			AssertCusTempStorageRegLineTransaction(testCase, intReference, -3, expGrossWeight: -1500);
			AssertGuaranteeWriteOffTransaction(testCase, reference, expectedTranValue: 5m);
		});

		public void TestImport_StatusCLS()
		{
			var reference = "99985000113";
			var intReference = "TestStatusCLS";

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Closed;
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Closed;

			tsRegLine2.SRL_PackageType = "BG";
			tsRegLine2.SRL_PackageMarks = "1234";

			AddFlatRow(reference, intReference, grossWeight: -500);
			AddFlatRow(reference, intReference, grossWeight: -500);
			AddFlatRow(reference, intReference, grossWeight: -500);

			CombineAssertions(() =>
			{
				AssertProcessorStatus(RunImport(), intReference, 3, 3, 0, 0);

				AssertEquals($"{intReference}: tsRegLine closed", UniversalReferenceConstants.TemporaryStorageStatus.Closed, tsRegLine2.SRL_CustomsStatus);
				AssertEquals($"{intReference}: tsRegHeader closed", UniversalReferenceConstants.TemporaryStorageStatus.Closed, tsRegHeader.SRH_Status);
			});
		}

		public void TestImport_NoWriteOffTransaction_Warnings() => CombineAssertions(() =>
		{
			var reference = "99985000113";
			var intReference = "TestWriteOffTransaction";

			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			tsRegLine2.SRL_PackageType = "BG";
			tsRegLine2.SRL_PackageMarks = "1234";

			guaTrxOBL.CPL_TranValue = 100m;
			Factory.Save();

			var testCase = $"{intReference}: warning positive balance";
			AddFlatRow(reference, intReference, pckQty: -1, grossWeight: -500);
			AssertProcessorStatus(RunImport(), intReference, 1, 0, 1, 0, "Temporary Storage Register [TSD Number: 99985000113, Item Number: 1, Package Type: BG] Warning (not excluded): Reference 99985000113 has a positive balance of 100 EUR. Please, check the existing transactions for this reference and create a manual adjustment if needed.\r\n");
			AssertCusTempStorageRegLineTransaction(testCase, intReference, -1, expGrossWeight: -500);
			AssertGuaranteeWriteOffTransaction(testCase, reference, shouldExist: false);
		});

		void AssertProcessorStatus(CusTempStorageRegLineTransactionsDataTransferProcessor processor, ZString testHint, int rows, int created, int wwarnings, int excluded, string logMsg = "")
		{
			AssertEquals($"{testHint}: Rows to import", rows, processor.TransactionsToCreate);
			AssertEquals($"{testHint}: Transactions created", created, processor.TransactionsCreated);
			AssertEquals($"{testHint}: Transactions created with warnings", wwarnings, processor.TransactionsWithWarnings);
			AssertEquals($"{testHint}: Transactions excluded", excluded, processor.TransactionsExcluded);

			if (!string.IsNullOrEmpty(logMsg))
			{
				Assert($"{testHint}: Log message expected {logMsg}", processor.Log.Contains(logMsg));
			}
		}

		void AssertCusTempStorageRegLineTransaction(ZString testHint, ZString intReference, int expPckQty, decimal expGrossWeight = -1500m)
		{
			var recentTransaction = tsRegLine2.CusTempStorageRegLineTransactions.FirstOrDefault(t => t.SRT_InternalReferenceNumber == intReference);

			AssertNotNull($"{testHint}: CusTempStorageRegLineTransaction exists", recentTransaction);

			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_PhysicalInOutDate", new ZDateTimeOffset(2025, 2, 1), recentTransaction.SRT_PhysicalInOutDate);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_TransactionDate", new ZDateTimeOffset(2025, 2, 2), recentTransaction.SRT_TransactionDate);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_GrossWeight", expGrossWeight, recentTransaction.SRT_GrossWeight);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_PackageQty", expPckQty, recentTransaction.SRT_PackageQty);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_InternalReferenceType", "DES", recentTransaction.SRT_InternalReferenceType);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_InternalReferenceNumber", intReference, recentTransaction.SRT_InternalReferenceNumber);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_ReferenceType", "MRN", recentTransaction.SRT_ReferenceType);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_Reference", "75E500008192929292", recentTransaction.SRT_Reference);
			AssertEquals($"{testHint}: CusTempStorageRegLineTransaction.SRT_Comments", "no comment", recentTransaction.SRT_Comments);

			recentTransaction.Exile();
		}

		void AssertGuaranteeWriteOffTransaction(ZString testHint, ZString reference, decimal expectedTranValue = 100m, bool shouldExist = true)
		{
			var writeOffTransaction = guaHolder.CusGuaranteeLineTransactions.FirstOrDefault(t => t.CPL_TransactionType == PermitTransactionTypeList.Codes.TRA && t.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed && t.CPL_Reference == reference);

			if (shouldExist)
			{
				AssertNotNull($"{testHint}: PermitTransaction exists", writeOffTransaction);

				AssertEquals($"{testHint}: PermitTransaction CPL_Reference", "99985000113", writeOffTransaction.CPL_Reference);
				AssertEquals($"{testHint}: PermitTransaction CPL_TransactionDate", new ZDateTime(2025, 2, 1), writeOffTransaction.CPL_TransactionDate);
				AssertEquals($"{testHint}: PermitTransaction CPL_TranValue", expectedTranValue, writeOffTransaction.CPL_TranValue);
				AssertEquals($"{testHint}: PermitTransaction CPL_TransactionType", PermitTransactionTypeList.Codes.TRA, writeOffTransaction.CPL_TransactionType);
				AssertEquals($"{testHint}: PermitTransaction CPL_TransactionStatus", PermitTransactionStatusList.Codes.Confirmed, writeOffTransaction.CPL_TransactionStatus);
				AssertEquals($"{testHint}: PermitTransaction CPL_TransactionCategory", PermitTransactionCategoryList.Codes.CUM, writeOffTransaction.CPL_TransactionCategory);
				AssertEquals($"{testHint}: PermitTransaction CPL_Comment", "Write-off TS 99985000113 / MRN 75E500008192929292", writeOffTransaction.CPL_Comment);

				writeOffTransaction.CPL_Reference = "XXX";
			}
			else
			{
				AssertNull($"{testHint}: PermitTransaction should not exist", writeOffTransaction);
			}
		}

		void AddFlatRow(ZString reference, ZString intReference, string srtReference = "75E500008192929292", string customsLocation = "ES003591ZF0018", int goodsItemNumber = 1, string pckType = "BG", string pckMarks = "1234", int pckQty = -1, decimal grossWeight = -1500m)
		{
			var newRow = ImportCollection.AddNew();
			newRow.SRP_CustomsLocation = customsLocation;
			newRow.SRH_Reference = reference;
			newRow.SRI_GoodsItemNumber = goodsItemNumber;
			newRow.SRL_PackageType = pckType;
			newRow.SRL_PackageMarks = pckMarks;
			newRow.SRT_PhysicalInOutDate = new ZDateTime(2025, 2, 1);
			newRow.SRT_TransactionDate = new ZDateTime(2025, 2, 2);
			newRow.SRT_GrossWeight = grossWeight;
			newRow.SRT_PackageQty = pckQty;
			newRow.SRT_InternalReferenceType = "DES";
			newRow.SRT_InternalReferenceNumber = intReference;
			newRow.SRT_ReferenceType = "MRN";
			newRow.SRT_Reference = srtReference;
			newRow.SRT_Comments = "no comment";
		}

		CusTempStorageRegLineTransactionsDataTransferProcessor RunImport()
		{
			var processor = new CusTempStorageRegLineTransactionsDataTransferProcessor(ImportCollection, ImportCollectionInfo);
			processor.Import();
			return processor;
		}

		void ResetImportCollection()
		{
			importCollection = null;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TST";
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.Address1 = "Test Address";

			var customsLocation = Factory.New<CusTempStorageRegPremises>();
			customsLocation.SRP_Code = "TST123";
			customsLocation.SRP_Description = "Test Premises";
			customsLocation.SRP_Type = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			customsLocation.SRP_OA_PremisesAddress = orgAddress.PK;
			customsLocation.SRP_CustomsLocation = "ES003591ZF0018";

			guaHolder = Factory.New<CusGuaranteeHeader>();
			guaHolder.CPH_Type = "TST";
			guaHolder.CPH_OH_PermitHolder = orgHeader.PK;
			guaHolder.CPH_Number = "17ESAGW0011223344";
			guaHolder.CPH_StartDate = new ZDate("2025-01-01");

			guaTrxOBL = guaHolder.CusGuaranteeLineTransactions.AddNew();
			guaTrxOBL.CPL_TransactionDate = new ZDateTime("2025-03-01 00:00:00");
			guaTrxOBL.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			guaTrxOBL.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			guaTrxOBL.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			guaTrxOBL.CPL_Reference = "99985000113";
			guaTrxOBL.CPL_Comment = "NCTS Arrival NCT00002862";
			guaTrxOBL.CPL_TranValue = -1000m;

			tsRegHeader = Factory.New<CusTempStorageRegHeader>();
			tsRegHeader.SRH_AppCode = CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;
			tsRegHeader.SRH_Status = UniversalReferenceConstants.TemporaryStorageStatus.Open;
			tsRegHeader.SRH_Reference = "99985000113";
			tsRegHeader.SRH_SRP_Premises = customsLocation.PK;

			guarantee = Factory.New<CusBondDetail>();
			guarantee.PW_ParentID = tsRegHeader.PK;
			guarantee.PW_ParentTableCode = CusTempStorageRegHeaderSchema.Constants.Prefix;
			guarantee.PW_BondNumber = guaHolder.CPH_Number;

			var tsRegLine1 = (CusTempStorageRegLine)tsRegHeader.CusTempStorageRegLines.AddNew();
			tsRegLine1.SRL_LineNumber = 1;
			tsRegLine1.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Closed;

			tsRegLine2 = (CusTempStorageRegLine)tsRegHeader.CusTempStorageRegLines.AddNew();
			tsRegLine2.SRL_LineNumber = 2;
			tsRegLine2.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			tsRegLine3 = (CusTempStorageRegLine)tsRegHeader.CusTempStorageRegLines.AddNew();
			tsRegLine3.SRL_LineNumber = 3;
			tsRegLine3.SRL_PackageType = "BG";
			tsRegLine3.SRL_PackageMarks = "3456";
			tsRegLine3.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			tsRegLine4 = (CusTempStorageRegLine)tsRegHeader.CusTempStorageRegLines.AddNew();
			tsRegLine4.SRL_LineNumber = 4;
			tsRegLine4.SRL_PackageType = "BG";
			tsRegLine4.SRL_PackageMarks = "4567";
			tsRegLine4.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;

			var tsRegLineItem1 = Factory.New<CusTempStorageRegLineItem>();
			tsRegLineItem1.SRI_GoodsItemNumber = 1;

			var tsRegLineItem3 = Factory.New<CusTempStorageRegLineItem>();
			tsRegLineItem3.SRI_GoodsItemNumber = 3;

			var tsRegLineItem4 = Factory.New<CusTempStorageRegLineItem>();
			tsRegLineItem4.SRI_GoodsItemNumber = 4;

			var tsRegLineItemPivot1 = tsRegLine1.RegLineItemPivots.AddNew();
			tsRegLineItemPivot1.SRV_SRL_Line = tsRegLine1.PK;
			tsRegLineItemPivot1.SRV_SRI_Item = tsRegLineItem1.PK;

			var tsRegLineItemPivot2 = tsRegLine2.RegLineItemPivots.AddNew();
			tsRegLineItemPivot2.SRV_SRL_Line = tsRegLine2.PK;
			tsRegLineItemPivot2.SRV_SRI_Item = tsRegLineItem1.PK;

			var tsRegLineItemPivot3 = tsRegLine3.RegLineItemPivots.AddNew();
			tsRegLineItemPivot3.SRV_SRL_Line = tsRegLine3.PK;
			tsRegLineItemPivot3.SRV_SRI_Item = tsRegLineItem3.PK;

			var tsRegLineItemPivot4 = tsRegLine4.RegLineItemPivots.AddNew();
			tsRegLineItemPivot4.SRV_SRL_Line = tsRegLine4.PK;
			tsRegLineItemPivot4.SRV_SRI_Item = tsRegLineItem4.PK;

			var regTrxOBL = tsRegLine2.CusTempStorageRegLineTransactions.AddNew();
			regTrxOBL.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regTrxOBL.SRT_InternalReferenceNumber = "1234";
			regTrxOBL.SRT_Reference = "75E500008192929292";
			regTrxOBL.SRT_BondAmount = 100m;
			regTrxOBL.SRT_GrossWeight = 1500m;
			regTrxOBL.SRT_PackageQty = 3;
			regTrxOBL.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber;
			regTrxOBL.SRT_Comments = "NCTS Arrival NCT00002862";

			var regTrxOBL3 = tsRegLine3.CusTempStorageRegLineTransactions.AddNew();
			regTrxOBL3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regTrxOBL3.SRT_InternalReferenceNumber = "3";
			regTrxOBL3.SRT_Reference = "75E500008192929292";
			regTrxOBL3.SRT_BondAmount = 100m;
			regTrxOBL3.SRT_GrossWeight = 30000m;
			regTrxOBL3.SRT_PackageQty = 300;
			regTrxOBL3.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber;
			regTrxOBL3.SRT_Comments = "NCTS Arrival NCT00002863";

			var regTrxOBL4 = tsRegLine4.CusTempStorageRegLineTransactions.AddNew();
			regTrxOBL4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			regTrxOBL4.SRT_InternalReferenceNumber = "4";
			regTrxOBL4.SRT_Reference = "75E500008192929292";
			regTrxOBL4.SRT_BondAmount = 100m;
			regTrxOBL4.SRT_GrossWeight = 30000m;
			regTrxOBL4.SRT_PackageQty = 300;
			regTrxOBL4.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber;
			regTrxOBL4.SRT_Comments = "NCTS Arrival NCT00002864";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
			_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			_ = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

			Factory.Save();
		}
		CusGuaranteeHeader guaHolder;
		CusBondDetail guarantee;
		CusTempStorageRegHeader tsRegHeader;
		CusTempStorageRegLine tsRegLine2;
		CusTempStorageRegLine tsRegLine3;
		CusTempStorageRegLine tsRegLine4;
		BaseCusGuaranteeLineTransaction guaTrxOBL;

		CusTempStorageRegLineTransactionFlattenedCollection ImportCollection => importCollection ??= GetNewCollection();
		CusTempStorageRegLineTransactionFlattenedCollection importCollection;

		CusTempStorageRegLineTransactionFlattenedImportCollectionInfo ImportCollectionInfo => importCollectionInfo ??= new CusTempStorageRegLineTransactionFlattenedImportCollectionInfo(ImportCollection);
		CusTempStorageRegLineTransactionFlattenedImportCollectionInfo importCollectionInfo;

		CusTempStorageRegLineTransactionFlattenedCollection GetNewCollection() => new CusTempStorageRegLineTransactionFlattenedCollection(Factory);
	}

	class DataExceptionForTesting : ZDataException
	{
		public DataExceptionForTesting(string message)
			: base(new Exception(message), message, message, null, null)
		{
		}
	}
}
