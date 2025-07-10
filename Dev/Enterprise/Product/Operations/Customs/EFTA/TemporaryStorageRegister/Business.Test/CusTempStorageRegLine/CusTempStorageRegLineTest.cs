using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLine))]
sealed class CusTempStorageRegLineTest : EnterpriseBusinessObjectTestCase
{
	public void TestRegLineItemPivots()
	{
		AssertType<CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>>(line.RegLineItemPivots);
	}

	public void TestSRL_LineNumber_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_LineNumber)
			.WithCaption("Line Number")
			.WithShortCaption("Line No.");
	});

	public void TestSRL_OwnerReferenceType_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_OwnerReferenceType)
			.WithCaption("Owner Reference Type");
	});

	public void TestSRL_OwnerReference_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_OwnerReference)
			.WithCaption("Owner Reference Number");
	});

	public void TestSRL_LocationOfGoods_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_LocationOfGoods)
			.WithCaption("Location of Goods");
	});

	public void TestSRL_GoodsDescription_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_GoodsDescription)
			.WithCaption("Goods Description");
	});

	public void TestSRL_LimitDate_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_LimitDate)
			.WithCaption("Limit Date");
	});

	public void TestSRL_CustodianIdentifier_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_CustodianIdentifier)
			.WithCaption("Custodian EORI");
	});

	public void TestSRL_GoodsOwnerIdentifierBranchNo_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_GoodsOwnerIdentifierBranchNo)
			.WithCaption("Disposal Entitled Trader");
	});

	public void TestSRL_CustodianIdentifierBranchNo_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_CustodianIdentifierBranchNo)
			.WithCaption("Branch");
	});

	public void TestSRL_PackagesRemaining_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_PackagesRemaining)
			.WithCaption("Packages Remaining");
	});

	public void TestSRL_PackageType_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_PackageType)
			.WithCaption("Package Type");
	});

	public void TestSRL_CustomsStatus_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_CustomsStatus)
			.WithCaption("Customs Status");
	});

	public void TestSRL_UnionStatus_Caption() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_UnionStatus)
			.WithCaption("Union Status");
	});

	public void TestSRL_PackageMarks() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_PackageMarks)
			.WithCaption("Marks & Numbers")
			.WithMediumCaption("Marks & Numbers")
			.WithShortCaption("Marks")
			.WithFullDescription("Marks and Numbers for selected packages");
	});

	public void TestSRL_GoodsOwnerIdentifier() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.SRL_GoodsOwnerIdentifier)
			.WithCaption("Owner ID")
			.WithMediumCaption("Owner ID")
			.WithShortCaption("Owner ID")
			.WithFullDescription("Owner's Identification Number");
	});

	public void TestPackagesRemainingCalculated_Attributes() => CombineAssertions(() =>
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.PackagesRemainingCalculated)
			.WithCaption("Packages Remaining")
			.WithMediumCaption("Packs Remaining")
			.WithShortCaption("Packs Remain")
			.WithFullDescription("Packages Remaining Amount");
	});

	public void TestOriginalPackagesQuantity() => CombineAssertions(() =>
	{
		AssertEquals("When no transactions exist OriginalPackagesQuantity = 0", 0, line.OriginalPackagesQuantity);

		var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		transaction1.SRT_PackageQty = 2;
		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
		transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
		transaction2.SRT_PackageQty = 4;
		var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
		transaction3.SRT_PackageQty = 6;
		var transaction4 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
		transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction4.SRT_PackageQty = 3;
		AssertEquals("When there are transactions, OriginalPackagesQuantity considers only OBL type transactions", 6, line.OriginalPackagesQuantity);
	});

	public void TestPackagesRemainingCalculated()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When no transactions exist PackagesRemainingCalculated = 0", 0, line.PackagesRemainingCalculated);

			var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction1.SRT_PackageQty = 2;
			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			transaction2.SRT_PackageQty = 4;
			var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction3.SRT_PackageQty = 6;
			var transaction4 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
			transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
			transaction4.SRT_PackageQty = 3;
			AssertEquals("When there are transactions, PackagesRemainingCalculated = 12, the sum of all transactions with status not DEL", 12, line.PackagesRemainingCalculated);

			var transaction5 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction5.SRT_PackageQty = -5;
			AssertEquals("When a new transaction is added value is recalculated, PackagesRemainingCalculated = 7", 7, line.PackagesRemainingCalculated);

			transaction2.SRT_PackageQty = 7;
			AssertEquals("When a transaction is changed value is recalculated, PackagesRemainingCalculated = 10", 10, line.PackagesRemainingCalculated);
		});
	}

	public void TestGrossWeightRemainingCalculated_Attributes()
	{
		_ = AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.GrossWeightRemainingCalculated)
			.WithCaption("Gross Weight Remaining")
			.WithMediumCaption("Gross Weight Rem.")
			.WithShortCaption("GWT Rem.")
			.WithFullDescription("Gross Weight Remaining Amount");
	}

	public void TestGrossWeightRemainingCalculated()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When no transactions exist GrossWeightRemainingCalculated = 0", 0m, line.GrossWeightRemainingCalculated);

			var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction1.SRT_GrossWeight = 2;
			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			transaction2.SRT_GrossWeight = 4;
			var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction3.SRT_GrossWeight = 6;
			var transaction4 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
			transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
			transaction4.SRT_GrossWeight = 3;
			AssertEquals("When there are transactions, GrossWeightRemainingCalculated = 12, the sum of all transactions with status not DEL", 12m, line.GrossWeightRemainingCalculated);

			var transaction5 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction5.SRT_GrossWeight = -5;
			AssertEquals("When a new transaction is added value is recalculated, GrossWeightRemainingCalculated = 7", 7m, line.GrossWeightRemainingCalculated);

			transaction2.SRT_GrossWeight = 7;
			AssertEquals("When a transaction is changed value is recalculated, GrossWeightRemainingCalculated = 10", 10m, line.GrossWeightRemainingCalculated);
		});
	}

	public void TestBulkPackageUnitTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		_ = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		_ = helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		_ = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		Factory.Save();

		CombineAssertions(() =>
		{
			AssertEquals("When SRL_PackageType is empty it is not bulk", expected: false, line.IsPackageTypeBulk);
			line.SRL_PackageType = "VQ";
			AssertEquals("When SRL_PackageType is VQ it is bulk", expected: true, line.IsPackageTypeBulk);
			line.SRL_PackageType = "AA";
			AssertEquals("When SRL_PackageType is AA it is not bulk", expected: false, line.IsPackageTypeBulk);
			line.SRL_PackageType = "VG";
			AssertEquals("When SRL_PackageType is VG it is bulk", expected: true, line.IsPackageTypeBulk);
			line.SRL_PackageType = "NE";
			AssertEquals("When SRL_PackageType is NE it is not bulk", expected: false, line.IsPackageTypeBulk);
		});
	}

	public void TestBondAmountRemainingCalculated_Attributes()
	{
		AssertEntity<CusTempStorageRegLine>()
			.HasProperty(l => l.BondAmountRemainingCalculated)
			.WithCaption("Liability Amount Remaining")
			.WithMediumCaption("Bond Amount Rem.")
			.WithShortCaption("Bond Amt. Rem.")
			.WithFullDescription("Remaining Liability Amount");
	}

	public void TestBondAmountRemainingCalculated()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When no transactions exist BondAmountRemainingCalculated = 0", 0m, line.BondAmountRemainingCalculated);

			var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction1.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction1.SRT_BondAmount = 2.0m;
			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction2.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			transaction2.SRT_BondAmount = 4.0m;
			var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction3.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
			transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction3.SRT_BondAmount = 6.0m;
			var transaction4 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction4.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Adjustment;
			transaction4.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
			transaction4.SRT_BondAmount = 3.0m;
			AssertEquals("When there are transactions, BondAmountRemainingCalculated = 12, the sum of all transactions with status not DEL", 12.0m, line.BondAmountRemainingCalculated);

			var transaction5 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction5.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
			transaction5.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
			transaction5.SRT_BondAmount = -5.0m;
			AssertEquals("When a new transaction is added value is recalculated, BondAmountRemainingCalculated = 7", 7.0m, line.BondAmountRemainingCalculated);

			transaction2.SRT_BondAmount = 7.0m;
			AssertEquals("When a transaction is changed value is recalculated, BondAmountRemainingCalculated = 10", 10.0m, line.BondAmountRemainingCalculated);
		});
	}

	public void TestGetStorageRegLineItemPivotType()
		=> AssertEquals(typeof(CusTempStorageRegLineItemPivot), line.GetStorageRegLineItemPivotType());

	public void TestTSDItemNumber() => AssertEquals(ZString.Empty, line.TSDItemNumber);

	public void TestGoodsDescription() => AssertEquals(ZString.Empty, line.GoodsDescription);

	public void TestCommodityCode() => AssertEquals(ZString.Empty, line.CommodityCode);

	public void TestHasManyPivotItems() => CombineAssertions(() =>
	{
		AssertEquals(ZBool.False, line.HasManyPivotItems);
		line.RegLineItemPivots.AddNew();
		AssertEquals(ZBool.False, line.HasManyPivotItems);
		line.RegLineItemPivots.AddNew();
		AssertEquals(ZBool.True, line.HasManyPivotItems);
	});

	public void TestRegLineItemQuantities()
	{
		line.RegLineItemPivots.AddNew();
		line.RegLineItemPivots.AddNew();
		line.RegLineItemPivots.AddNew();
		var regLineItemQuantities = line.RegLineItemQuantities;
		CombineAssertions(() =>
		{
			AssertEquals("Count", 3, regLineItemQuantities.Count);
			AssertSame("Cached", regLineItemQuantities, line.RegLineItemQuantities);
		});
	}

	public void TestIsPackageTypeFrame() => CombineAssertions(() =>
	{
		AssertEquals("IsPackageTypeFrame false", false, line.IsPackageTypeFrame);

		line.SRL_PackageType = UniversalReferenceConstants.PackageType.Frame;
		AssertEquals("IsPackageTypeFrame true", true, line.IsPackageTypeFrame);
	});

	public void TestIsOpen() => CombineAssertions(() =>
	{
		line.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
		AssertEquals("Customs Status open", true, line.IsOpen);

		line.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Closed;
		AssertEquals("Customs Status closed", false, line.IsOpen);
	});

	public void TestIsClosed() => CombineAssertions(() =>
	{
		line.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Open;
		AssertEquals("Customs Status open", false, line.IsClosed);

		line.SRL_CustomsStatus = UniversalReferenceConstants.TemporaryStorageStatus.Closed;
		AssertEquals("Customs Status closed", true, line.IsClosed);
	});

	public void TestOpeningBalanceTransactionOBL() => CombineAssertions(() =>
	{
		AssertNull("no opening balance transaction", line.OpeningBalanceTransaction);

		var regTrxOBL = line.CusTempStorageRegLineTransactions.AddNew();
		regTrxOBL.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance;
		regTrxOBL.SRT_InternalReferenceNumber = "1234";
		regTrxOBL.SRT_Reference = "75E500008192929292";
		regTrxOBL.SRT_BondAmount = 100m;
		regTrxOBL.SRT_GrossWeight = 1500m;
		regTrxOBL.SRT_PackageQty = 3;
		regTrxOBL.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber;
		regTrxOBL.SRT_Comments = "NCTS Arrival NCT00002862";

		AssertNotNull("opening balance transaction exists", line.OpeningBalanceTransaction);
		AssertEquals("SRT Transaction Type", CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, line.OpeningBalanceTransaction.SRT_TransactionType);
	});

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	static BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "AAA";
		header.SRH_Reference = "reference";
		var regLine = header.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		return regLine;
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_AppCode = "AAA";
		header.SRH_Reference = "reference";
		line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
	}
	CusTempStorageRegLine line;
}
