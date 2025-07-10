using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

public class CusTempStorageRegLineTransactionFormEditableValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidationTypeAndParent() => CombineAssertions(() =>
	{
		AssertType<CusTempStorageRegLineTransactionFormEditableValidation>(transactionEditable.Validation);
		AssertSame(transactionEditable.Validation.Parent, transactionEditable);
	});

	public void TestSRT_GrossWeight_Empty() => ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.SRT_GrossWeightInfo);

	public void TestSRT_GrossWeight_RamainingNegative()
	{
		const string message = "Remaining Gross Weight should not be negative.";

		var transaction2 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_GrossWeight = 1;

		var transaction3 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_GrossWeight = -10;

		CombineAssertions(() =>
		{
			transactionEditable.SRT_GrossWeight = -1;
			AssertNoError("GrossWeightRemainingCalculated = 0", transactionEditable.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = 0;
			transactionEditable.Validation.ValidateSRT_GrossWeight();
			AssertHasError("GrossWeightRemainingCalculated < 0", transactionEditable.SRT_GrossWeightInfo, message);

			transaction2.SRT_GrossWeight = 2;
			transactionEditable.Validation.ValidateSRT_GrossWeight();
			AssertNoError("GrossWeightRemainingCalculated > 0", transactionEditable.SRT_GrossWeightInfo, message);
		});
	}

	public void TestSRT_GrossWeight_Positive()
	{
		const string expectedMessage = "Please note value entered in Gross Weight in KGs will be summed to the stock. If you need to make a manual exit, value entered should be negative.";

		CombineAssertions(() =>
		{
			transactionEditable.SRT_GrossWeight = -1;
			AssertNoWarning("SRT_GrossWeight < 0", transactionEditable.SRT_GrossWeightInfo, expectedMessage);

			transactionEditable.SRT_GrossWeight = 1;
			transactionEditable.Validation.ValidateSRT_GrossWeight();
			AssertHasWarning("SRT_GrossWeight > 0", transactionEditable.SRT_GrossWeightInfo, expectedMessage);

			transactionEditable.SRT_GrossWeight = 0;
			transactionEditable.Validation.ValidateSRT_GrossWeight();
			AssertNoWarning("SRT_GrossWeight = 0", transactionEditable.SRT_GrossWeightInfo, expectedMessage);
		});
	}

	public void TestSRT_GrossWeight_RamainingZero()
	{
		const string message = "If Remaining Gross Weight is 0 then Remaining Packages must be also 0 and vice-versa.";

		SetUpRefData();

		var transaction1 = regLine.CusTempStorageRegLineTransactions.AddNew();

		transaction1.SRT_GrossWeight = 1;
		transaction1.SRT_PackageQty = 1;

		var transaction3 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_GrossWeight = -10;
		transaction3.SRT_PackageQty = -10;

		CombineAssertions("Having bulk line", () =>
		{
			regLine.SRL_PackageType = "VQ";
			transactionEditable.SRT_GrossWeight = 0;
			AssertNoError("When Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated > 0", transactionEditable.SRT_GrossWeightInfo, message);

			transactionEditable.SRT_GrossWeight = -1;
			AssertNoError("When Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated > 0", transactionEditable.SRT_GrossWeightInfo, message);

			transactionEditable.SRT_PackageQty = -1;
			transactionEditable.Validation.ValidateSRT_GrossWeight();
			AssertNoError("When Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated = 0", transactionEditable.SRT_GrossWeightInfo, message);

			transactionEditable.SRT_GrossWeight = 0;
			AssertNoError("When Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated = 0", transactionEditable.SRT_GrossWeightInfo, message);
		});

		transactionEditable.SRT_PackageQty = 1;
		CombineAssertions("Having not bulk line", () =>
		{
			regLine.SRL_PackageType = "AA";
			transactionEditable.SRT_GrossWeight = 0;
			AssertNoError("When not Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated > 0", transactionEditable.SRT_GrossWeightInfo, message);

			transactionEditable.SRT_GrossWeight = -1;
			AssertHasError("When not Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated > 0", transactionEditable.SRT_GrossWeightInfo, message);

			transactionEditable.SRT_PackageQty = -1;
			transactionEditable.Validation.ValidateSRT_GrossWeight();
			AssertNoError("When not Bulk, GrossWeightRemainingCalculated = 0 and PackagesRemainingCalculated = 0", transactionEditable.SRT_GrossWeightInfo, message);

			transactionEditable.SRT_GrossWeight = 0;
			AssertNoError("When not Bulk, GrossWeightRemainingCalculated > 0 and PackagesRemainingCalculated = 0", transactionEditable.SRT_GrossWeightInfo, message);
		});
	}

	public void TestSRT_PackageQty_Empty()
	{
		SetUpRefData();

		CombineAssertions(() =>
		{
			regLine.SRL_PackageType = "VQ";
			transactionEditable.SRT_PackageQty = 0;
			transactionEditable.Validation.ValidateSRT_PackageQty();
			AssertNoErrorContaining("No error when empty and package type is Bulk", transactionEditable.SRT_PackageQtyInfo, MandatoryValidation.MustBeEntered);

			regLine.SRL_PackageType = "AA";
			ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.SRT_PackageQtyInfo);
		});
	}

	public void TestSRT_PackageQty_RamainingNegative()
	{
		const string message = "Remaining Packages should not be negative.";

		var transaction2 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_PackageQty = 1;

		var transaction3 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_PackageQty = -10;

		CombineAssertions(() =>
		{
			transactionEditable.SRT_PackageQty = -1;
			AssertNoError("PackagesRemainingCalculated = 0", transactionEditable.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = 0;
			transactionEditable.Validation.ValidateSRT_PackageQty();
			AssertHasError("PackagesRemainingCalculated < 0", transactionEditable.SRT_PackageQtyInfo, message);

			transaction2.SRT_PackageQty = 2;
			transactionEditable.Validation.ValidateSRT_PackageQty();
			AssertNoError("PackagesRemainingCalculated > 0", transactionEditable.SRT_PackageQtyInfo, message);
		});
	}

	public void TestSRT_PackageQty_Positive()
	{
		const string expectedMessage = "Please note value entered in Package Quantity will be summed to the stock. If you need to make a manual exit, value entered should be negative.";

		CombineAssertions(() =>
		{
			transactionEditable.SRT_PackageQty = -1;
			AssertNoWarning("SRT_PackageQty < 0", transactionEditable.SRT_PackageQtyInfo, expectedMessage);

			transactionEditable.SRT_PackageQty = 1;
			transactionEditable.Validation.ValidateSRT_PackageQty();
			AssertHasWarning("SRT_PackageQty > 0", transactionEditable.SRT_PackageQtyInfo, expectedMessage);

			transactionEditable.SRT_PackageQty = 0;
			transactionEditable.Validation.ValidateSRT_PackageQty();
			AssertNoWarning("SRT_PackageQty = 0", transactionEditable.SRT_PackageQtyInfo, expectedMessage);
		});
	}

	public void TestSRT_PackageQty_RamainingZero()
	{
		const string message = "If Remaining Packages is 0 then Remaining Gross Weight must be also 0 and vice-versa.";
		SetUpRefData();

		var transaction2 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction2.SRT_GrossWeight = 1;
		transaction2.SRT_PackageQty = 1;

		var transaction3 = regLine.CusTempStorageRegLineTransactions.AddNew();
		transaction3.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Deleted;
		transaction3.SRT_GrossWeight = -10;
		transaction3.SRT_PackageQty = -10;

		CombineAssertions("Having bulk line", () =>
		{
			regLine.SRL_PackageType = "VQ";
			transactionEditable.SRT_PackageQty = 0;
			AssertNoError("When Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated > 0", transactionEditable.SRT_PackageQtyInfo, message);

			AssertNoError("When Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated > 0", transactionEditable.SRT_PackageQtyInfo, message);

			transactionEditable.SRT_GrossWeight = -1;
			transactionEditable.Validation.ValidateSRT_PackageQty();
			AssertNoError("When Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated = 0", transactionEditable.SRT_PackageQtyInfo, message);

			transactionEditable.SRT_PackageQty = 0;
			AssertNoError("When Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated = 0", transactionEditable.SRT_PackageQtyInfo, message);
		});

		transactionEditable.SRT_GrossWeight = 1;
		CombineAssertions("Having not bulk line", () =>
		{
			regLine.SRL_PackageType = "AA";
			transactionEditable.SRT_PackageQty = 0;
			AssertNoError("When not Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated > 0", transactionEditable.SRT_PackageQtyInfo, message);

			transactionEditable.SRT_PackageQty = -1;
			AssertHasError("When not Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated > 0", transactionEditable.SRT_PackageQtyInfo, message);

			transactionEditable.SRT_GrossWeight = -1;
			transactionEditable.Validation.ValidateSRT_PackageQty();
			AssertNoError("When not Bulk, PackagesRemainingCalculated = 0 and GrossWeightRemainingCalculated = 0", transactionEditable.SRT_PackageQtyInfo, message);

			transactionEditable.SRT_PackageQty = 0;
			AssertNoError("When not Bulk, PackagesRemainingCalculated > 0 and GrossWeightRemainingCalculated = 0", transactionEditable.SRT_PackageQtyInfo, message);
		});
	}

	public void TestSRT_ReferenceType() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.SRT_ReferenceTypeInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(transactionEditable.SRT_ReferenceTypeInfo, "AAA", CusTempStorageRegLineTransactionReferenceTypeList.Codes.MovementReferenceNumber);
	});

	public void TestSRT_Reference() => ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.SRT_ReferenceInfo);

	public void TestSRT_InternalReferenceType() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.SRT_InternalReferenceTypeInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(transactionEditable.SRT_InternalReferenceTypeInfo, "AAA", CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.ImportDeclaration);
	});

	public void TestSRT_InternalReferenceNumber() => ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.SRT_InternalReferenceNumberInfo);

	public void TestTransactionDate() => ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.TransactionDateInfo);

	public void TestPhysicalInOutDate() => ValidationTestHelper.AssertErrorIfNotEntered(transactionEditable.PhysicalInOutDateInfo);

	protected override void SetUp()
	{
		base.SetUp();
		var regHeader = Factory.New<CusTempStorageRegHeader>();
		regHeader.SRH_Reference = "UNITTEST";
		var premises = Factory.New<EU.TemporaryStorage.Business.CusTempStorageRegPremises>();
		premises.SRP_Code = "AH3";
		premises.SRP_Description = "DESC";
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_Code = "AH3";
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "TestAddress";
		premises.SRP_OA_PremisesAddress = orgAddress.PK;

		regHeader.SRH_SRP_Premises = premises.PK;
		regLine = regHeader.CusTempStorageRegLines.AddNew();
		regLine.SRL_LineNumber = 1;
		transactionEditable = new CusTempStorageRegLineTransactionFormEditable(regLine);
	}
	CusTempStorageRegLine regLine;
	CusTempStorageRegLineTransactionFormEditable transactionEditable;

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
