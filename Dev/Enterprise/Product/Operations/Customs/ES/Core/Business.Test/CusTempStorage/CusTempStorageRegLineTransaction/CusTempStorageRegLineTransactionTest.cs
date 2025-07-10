using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageRegLineTransaction))]
class CusTempStorageRegLineTransactionTest : EnterpriseBusinessObjectTestCase
{
	public void TestSRT_TransactionType()
	{
		AssertEquals(true, transaction.SRT_TransactionTypeInfo.ReadOnly);
	}

	public void TestSRT_BondAmount()
	{
		AssertEquals(true, transaction.SRT_BondAmountInfo.ReadOnly);
	}

	public void TestSRT_SystemCreateTimeUtc_ReadOnly()
	{
		AssertEquals(true, transaction.SRT_SystemCreateTimeUtcInfo.ReadOnly);
	}

	public void TestSRT_SystemCreateUser_ReadOnly()
	{
		AssertEquals(true, transaction.SRT_SystemCreateUserInfo.ReadOnly);
	}

	public void TestSRT_TransactionStatus_ReadOnly()
	{
		AssertEquals(true, transaction.SRT_TransactionStatusInfo.ReadOnly);
	}

	public void TestReadOnly()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Transaction is editable when it's not saved", false, transaction.ReadOnly);

			Factory.Save();
			AssertEquals("Transaction is readonly after it's saved", true, transaction.ReadOnly);
		});
	}

	public void TestLookups()
	{
		AssertType<CusTempStorageRegLineTransactionLookups>(transaction.Lookups);
	}

	public void TestValidation()
	{
		AssertType<CusTempStorageRegLineTransactionValidation>(transaction.Validation);
	}

	public void TestOnSaving_Validations()
	{
		const string expectedMessageGrossWeight = "Please note value entered in Gross Weight in KGs will be summed to the stock. If you need to make a manual exit, value entered should be negative.";
		const string expectedMessagePackageQty = "Please note value entered in Package Quantity will be summed to the stock. If you need to make a manual exit, value entered should be negative.";
		const string expectedMessageBondAmount = "When adding goods to stock, the liability amount cannot be calculated so no guarantee transaction will be created. If needed, it should be manually added by the user.";

		transaction.SRT_GrossWeight = 1;
		transaction.Validation.ValidateSRT_GrossWeight();
		transaction.SRT_PackageQty = 1;
		transaction.Validation.ValidateSRT_PackageQty();
		CombineAssertions(() => 
		{
			AssertHasWarning("[PreReq] SRT_GrossWeight > 0 warning", transaction.SRT_GrossWeightInfo, expectedMessageGrossWeight);
			AssertHasWarning("[PreReq] SRT_PackageQty > 0 warning", transaction.SRT_PackageQtyInfo, expectedMessagePackageQty);
			AssertHasWarning("[PreReq] When SRT_GrossWeight > 0, warning in SRT_BondAmount", transaction.SRT_BondAmountInfo, expectedMessageBondAmount);

			Factory.Save();
			AssertNoWarning("When transaction is saved, SRT_GrossWeight > 0 no warning in SRT_GrossWeight", transaction.SRT_GrossWeightInfo, expectedMessageGrossWeight);
			AssertNoWarning("When transaction is saved, SRT_PackageQty > 0 no warning in SRT_PackageQty", transaction.SRT_PackageQtyInfo, expectedMessagePackageQty);
			AssertNoWarning("When transaction is saved, SRT_GrossWeight > 0, no warning in SRT_BondAmount", transaction.SRT_BondAmountInfo, expectedMessageBondAmount);
		});
	}

	public void TestSRT_GrossWeight_Captions() => CombineAssertions(() =>
	{
		AssertResourceStringData(transaction.SRT_GrossWeightInfo, "Gross Weight in KGs", "Gross Weight in KGs", "Gross Weight in KGs", "Gross Weight in Kilograms");
	});

	public void TestSRT_InternalReferenceType_Captions() => CombineAssertions(() =>
	{
		AssertResourceStringData(transaction.SRT_InternalReferenceTypeInfo, "Internal Reference Type", "Int. Ref. Type", "Int. Type", "The Internal Reference Type");
	});

	public void TestSRT_InternalReferenceNumber_Captions() => CombineAssertions(() =>
	{
		AssertResourceStringData(transaction.SRT_InternalReferenceNumberInfo, "Internal Reference No.", "Internal Ref. No.", "Int. Ref. No.", "Internal Reference Number");
	});

	void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
		AssertEquals("Caption", caption, captionResourceString.Caption);
		AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
		AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
		AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<CusTempStorageRegHeader>();
		header.SRH_Reference = "TEST";
		var line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		return line.CusTempStorageRegLineTransactions.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<CusTempStorageRegHeader>();
		header.SRH_Status = TempStorageDeclarationStatusList.Codes.Open;
		header.SRH_Reference = "TEST";
		line = header.CusTempStorageRegLines.AddNew();
		line.SRL_LineNumber = 1;
		transaction = line.CusTempStorageRegLineTransactions.AddNew();
	}
	CusTempStorageRegHeader header;
	CusTempStorageRegLine line;
	CusTempStorageRegLineTransaction transaction;
}
