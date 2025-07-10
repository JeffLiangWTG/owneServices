using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using CusTempStorageRegLineTransactionStatusList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

class CusTempStorageRegLineTransactionValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidationTypeAndParent()
	{
		var transaction = Factory.New<CusTempStorageRegLineTransaction>();

		CombineAssertions(() =>
		{
			AssertType<CusTempStorageRegLineTransactionValidation>(transaction.Validation);
			AssertSame(transaction.Validation.Parent, transaction);
		});
	}

	public void TestSRT_GrossWeight_Empty()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(transaction.SRT_GrossWeightInfo);

			transaction.SRT_GrossWeight = 0;
			Factory.Save();
			transaction.Validation.ValidateSRT_GrossWeight();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction.SRT_GrossWeightInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestSRT_GrossWeight_RamainingNegative()
	{
		const string message = "Remaining Gross Weight should not be negative.";

		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_GrossWeight = 1;

		var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_GrossWeight = -10;

		CombineAssertions(() =>
		{
			transaction.SRT_GrossWeight = -1;
			AssertNoError("GrossWeightRemainingCalculated = 0", transaction.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = 0;
			transaction.Validation.ValidateSRT_GrossWeight();
			AssertHasError("GrossWeightRemainingCalculated < 0", transaction.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = 2;
			transaction.Validation.ValidateSRT_GrossWeight();
			AssertNoError("GrossWeightRemainingCalculated > 0", transaction.SRT_GrossWeightInfo, message);

			transaction.SRT_GrossWeight = -5;
			AssertHasError("Before saving, GrossWeightRemainingCalculated < 0", transaction.SRT_GrossWeightInfo, message);
			Factory.Save();
			transaction.Validation.ValidateSRT_GrossWeight();
			AssertNoError("When transaction is in database, GrossWeightRemainingCalculated < 0", transaction2.SRT_PackageQtyInfo, message);
		});
	}

	public void TestSRT_GrossWeight_Positive()
	{
		const string expectedMessage = "Please note value entered in Gross Weight in KGs will be summed to the stock. If you need to make a manual exit, value entered should be negative.";

		CombineAssertions(() =>
		{
			transaction.SRT_GrossWeight = -1;
			AssertNoWarning("SRT_GrossWeight < 0", transaction.SRT_GrossWeightInfo, expectedMessage);

			transaction.SRT_GrossWeight = 1;
			transaction.Validation.ValidateSRT_GrossWeight();
			AssertHasWarning("SRT_GrossWeight > 0", transaction.SRT_GrossWeightInfo, expectedMessage);

			transaction.SRT_GrossWeight = 0;
			transaction.Validation.ValidateSRT_GrossWeight();
			AssertNoWarning("SRT_GrossWeight = 0", transaction.SRT_GrossWeightInfo, expectedMessage);

			transaction.SRT_GrossWeight = 1;
			Factory.Save();
			AssertNoWarning("When transaction is in database, SRT_GrossWeight > 0", transaction.SRT_GrossWeightInfo, expectedMessage);
		});
	}

	public void TestSRT_GrossWeight_RamainingZero()
	{
		const string message = "If Remaining Gross Weight is 0 then Remaining Packages must be also 0 and vice-versa.";

		SetUpRefData();

		transaction.SRT_GrossWeight = 1;
		transaction.SRT_PackageQty = 1;

		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();

		var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_GrossWeight = -10;
		transaction3.SRT_PackageQty = -10;

		CombineAssertions("Having bulk line", () =>
		{
			line.SRL_PackageType = "VQ";
			transaction2.SRT_GrossWeight = 0;
			AssertNoError("When Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated > 0", transaction2.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = -1;
			AssertNoError("When Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated > 0", transaction2.SRT_GrossWeightInfo, message);

			transaction2.SRT_PackageQty = -1;
			transaction2.Validation.ValidateSRT_GrossWeight();
			AssertNoError("When Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated = 0", transaction2.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = 0;
			AssertNoError("When Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated = 0", transaction2.SRT_GrossWeightInfo, message);
		});

		transaction2.SRT_PackageQty = 1;
		CombineAssertions("Having not bulk line", () =>
		{
			line.SRL_PackageType = "AA";
			transaction2.SRT_GrossWeight = 0;
			AssertNoError("When not Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated > 0", transaction2.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = -1;
			AssertHasError("When not Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated > 0", transaction2.SRT_GrossWeightInfo, message);

			transaction2.SRT_PackageQty = -1;
			transaction2.Validation.ValidateSRT_GrossWeight();
			AssertNoError("When not Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated = 0", transaction2.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = 0;
			AssertNoError("When not Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated = 0", transaction2.SRT_GrossWeightInfo, message);

			transaction2.SRT_PackageQty = 1;
			transaction2.SRT_GrossWeight = -1;
			AssertHasError("Before saving, when not Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated > 0", transaction2.SRT_GrossWeightInfo, message);
			Factory.Save();
			transaction2.Validation.ValidateSRT_GrossWeight();
			AssertNoError("When transaction is in database, when not Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated > 0", transaction2.SRT_GrossWeightInfo, message);
		});
	}

	public void TestSRT_PackageQty_Empty()
	{
		SetUpRefData();

		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();

		CombineAssertions(() =>
		{
			line.SRL_PackageType = "VQ";
			transaction2.SRT_PackageQty = 0;
			transaction2.Validation.ValidateSRT_PackageQty();
			AssertNoErrorContaining("No error when empty and package type is Bulk", transaction2.SRT_PackageQtyInfo, MandatoryValidation.MustBeEntered);

			line.SRL_PackageType = "AA";
			ValidationTestHelper.AssertErrorIfNotEntered(transaction2.SRT_PackageQtyInfo);

			transaction2.SRT_PackageQty = 0;
			Factory.Save();
			transaction2.Validation.ValidateSRT_PackageQty();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction2.SRT_PackageQtyInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestSRT_PackageQty_RamainingNegative()
	{
		const string message = "Remaining Packages should not be negative.";

		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_PackageQty = 1;

		var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_PackageQty = -10;

		CombineAssertions(() =>
		{
			transaction.SRT_PackageQty = -1;
			AssertNoError("PackagesRemainingCalculated = 0", transaction.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = 0;
			transaction.Validation.ValidateSRT_PackageQty();
			AssertHasError("PackagesRemainingCalculated < 0", transaction.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = 2;
			transaction.Validation.ValidateSRT_PackageQty();
			AssertNoError("PackagesRemainingCalculated > 0", transaction.SRT_PackageQtyInfo, message);

			transaction.SRT_PackageQty = -5;
			AssertHasError("Before saving, PackagesRemainingCalculated < 0", transaction.SRT_PackageQtyInfo, message);

			transaction.SRT_PackageQty = 1;
			Factory.Save();
			transaction.SRT_PackageQty = -5;
			AssertNoError("When transaction is in database, PackagesRemainingCalculated < 0", transaction2.SRT_PackageQtyInfo, message);
		});
	}

	public void TestSRT_PackageQty_Positive()
	{
		const string expectedMessage = "Please note value entered in Package Quantity will be summed to the stock. If you need to make a manual exit, value entered should be negative.";

		CombineAssertions(() =>
		{
			transaction.SRT_PackageQty = -1;
			AssertNoWarning("SRT_PackageQty < 0", transaction.SRT_PackageQtyInfo, expectedMessage);

			transaction.SRT_PackageQty = 1;
			transaction.Validation.ValidateSRT_PackageQty();
			AssertHasWarning("SRT_PackageQty > 0", transaction.SRT_PackageQtyInfo, expectedMessage);

			transaction.SRT_PackageQty = 0;
			transaction.Validation.ValidateSRT_PackageQty();
			AssertNoWarning("SRT_PackageQty = 0", transaction.SRT_PackageQtyInfo, expectedMessage);

			transaction.SRT_PackageQty = 1;
			Factory.Save();
			AssertNoWarning("When transaction is in database, SRT_PackageQty > 0", transaction.SRT_PackageQtyInfo, expectedMessage);
		});
	}

	public void TestSRT_PackageQty_RamainingZero()
	{
		const string message = "If Remaining Packages is 0 then Remaining Gross Weight must be also 0 and vice-versa.";

		SetUpRefData();

		transaction.SRT_GrossWeight = 1;
		transaction.SRT_PackageQty = 1;

		var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();

		var transaction3 = line.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_GrossWeight = -10;
		transaction3.SRT_PackageQty = -10;

		CombineAssertions("Having bulk line", () =>
		{
			line.SRL_PackageType = "VQ";
			transaction2.SRT_PackageQty = 0;
			AssertNoError("When Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated > 0", transaction2.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = -1;
			AssertNoError("When Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated > 0", transaction2.SRT_PackageQtyInfo, message);

			transaction2.SRT_GrossWeight = -1;
			transaction2.Validation.ValidateSRT_PackageQty();
			AssertNoError("When Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated = 0", transaction2.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = 0;
			AssertNoError("When Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated = 0", transaction2.SRT_PackageQtyInfo, message);
		});

		transaction2.SRT_GrossWeight = 1;
		CombineAssertions("Having not bulk line", () =>
		{
			line.SRL_PackageType = "AA";
			transaction2.SRT_PackageQty = 0;
			AssertNoError("When not Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated > 0", transaction2.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = -1;
			AssertHasError("When not Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated > 0", transaction2.SRT_PackageQtyInfo, message);

			transaction2.SRT_GrossWeight = -1;
			transaction2.Validation.ValidateSRT_PackageQty();
			AssertNoError("When not Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated = 0", transaction2.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = 0;
			AssertNoError("When not Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated = 0", transaction2.SRT_PackageQtyInfo, message);

			transaction2.SRT_GrossWeight = 1;
			transaction2.SRT_PackageQty = -1;
			AssertHasError("Before saving, when not Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated > 0", transaction2.SRT_PackageQtyInfo, message);
			Factory.Save();
			transaction2.Validation.ValidateSRT_PackageQty();
			AssertNoError("When transaction is in database, when not Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated > 0", transaction2.SRT_PackageQtyInfo, message);
		});
	}

	public void TestSRT_BondAmount_GrossWeightPositive()
	{
		const string expectedMessage = "When adding goods to stock, the liability amount cannot be calculated so no guarantee transaction will be created. If needed, it should be manually added by the user.";

		CombineAssertions(() =>
		{
			transaction.SRT_GrossWeight = -1;
			AssertNoWarning("When SRT_GrossWeight < 0, no warning in SRT_BondAmount", transaction.SRT_BondAmountInfo, expectedMessage);

			transaction.SRT_GrossWeight = 1;
			AssertHasWarning("When SRT_GrossWeight > 0, warning in SRT_BondAmount", transaction.SRT_BondAmountInfo, expectedMessage);

			transaction.SRT_GrossWeight = 0;
			AssertNoWarning("When SRT_GrossWeight = 0, no warning in SRT_BondAmount", transaction.SRT_BondAmountInfo, expectedMessage);

			transaction.SRT_GrossWeight = 1;
			Factory.Save();
			AssertNoWarning("When transaction is in database, SRT_GrossWeight > 0, no warning in SRT_BondAmount", transaction.SRT_BondAmountInfo, expectedMessage);
		});
	}

	public void TestSRT_ReferenceType()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(transaction.SRT_ReferenceTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(transaction.SRT_ReferenceTypeInfo, "AAA", CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber);

			transaction.SRT_ReferenceType = "";
			Factory.Save(); 
			transaction.Validation.ValidateSRT_ReferenceType();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction.SRT_ReferenceTypeInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestSRT_Reference()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(transaction.SRT_ReferenceInfo);

			transaction.SRT_Reference = "";
			Factory.Save();
			transaction.Validation.ValidateSRT_Reference();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction.SRT_ReferenceInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestSRT_InternalReferenceType()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(transaction.SRT_InternalReferenceTypeInfo);
			ValidationTestHelper.AssertErrorIfInvalidCode(transaction.SRT_InternalReferenceTypeInfo, "AAA", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);

			transaction.SRT_InternalReferenceType = "";
			Factory.Save();
			transaction.Validation.ValidateSRT_InternalReferenceType();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction.SRT_InternalReferenceTypeInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestSRT_InternalReferenceNumber()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(transaction.SRT_InternalReferenceNumberInfo);

			transaction.SRT_InternalReferenceNumber = "";
			Factory.Save();
			transaction.Validation.ValidateSRT_InternalReferenceNumber();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction.SRT_InternalReferenceNumberInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestSRT_TransactionDate()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(transaction.SRT_TransactionDateInfo);

			transaction.SRT_TransactionDate = ZDateTimeOffset.Empty;
			Factory.Save();
			transaction.Validation.ValidateSRT_TransactionDate();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction.SRT_TransactionDateInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestSRT_PhysicalInOutDate()
	{
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertErrorIfNotEntered(transaction.SRT_PhysicalInOutDateInfo);

			transaction.SRT_PhysicalInOutDate = ZDateTimeOffset.Empty;
			Factory.Save();
			transaction.Validation.ValidateSRT_PhysicalInOutDate();
			AssertNoErrorContaining("When transaction is in database we don't have the message error", transaction.SRT_PhysicalInOutDateInfo, MandatoryValidation.MustBeEntered);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
		header.SRH_Reference = "TEST";
		line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		transaction = line.CusTempStorageRegLineTransactions.AddNew();
	}
	CusTempStorageRegLine line;
	CusTempStorageRegLineTransaction transaction;

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		Factory.Save();
	}
}
