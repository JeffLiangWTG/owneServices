using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using InvoiceLinePackagePivot = Enterprise.Customs.IT.Business.Declaration.InvoiceLinePackagePivot;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class PackageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new PackageWrapper(null));

		packagePivot.CHC_CW = ZGuid.Empty;
		AssertExceptionThrown<ArgumentNullException>("Exception expected when packagePivot has no Package is null", () => new PackageWrapper(packagePivot));
	}

	public void TestPackageType()
	{
		var packageWrapper = GetNewPackageWrapper();
		AssertEquals(nameof(IPackage.PackageType), "", packageWrapper.PackageType);

		package.CW_PackType = "CT";
		packageWrapper = GetNewPackageWrapper();
		AssertEquals(nameof(IPackage.PackageType), "CT", packageWrapper.PackageType);
	}

	public void TestNumberOfPacks()
	{
		var packageWrapper = GetNewPackageWrapper();
		AssertEquals(nameof(IPackage.NumberOfPacks), 0, packageWrapper.NumberOfPacks);

		packagePivot.CHC_NumberOfPacks = 20;
		packageWrapper = GetNewPackageWrapper();
		AssertEquals(nameof(IPackage.NumberOfPacks), 20, packageWrapper.NumberOfPacks);
	}

	public void TestNumberOfPacks_WhenPackageTypeIsBulk()
	{
		SetUpBulkPackageType();
		Factory.Save();

		package = declaration.Packages.AddNew();
		package.CW_PackType = "VQ";
		packagePivot = invoiceLine.PackagesPivot.AddNew();
		packagePivot.CHC_CW = package.PK;
		packagePivot.CHC_NumberOfPacks = 20;
		var packageWrapper = GetNewPackageWrapper();
		AssertNull(nameof(IPackage.NumberOfPacks), packageWrapper.NumberOfPacks);
	}

	public void TestMarksAndNumbers()
	{
		var packageWrapper = GetNewPackageWrapper();
		AssertEquals(nameof(IPackage.MarksAndNumbers), "", packageWrapper.MarksAndNumbers);

		package.CW_MarksAndNos = "MARKS AND NO";
		packageWrapper = GetNewPackageWrapper();
		AssertEquals(nameof(IPackage.MarksAndNumbers), "MARKS AND NO", packageWrapper.MarksAndNumbers);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		package = declaration.Packages.AddNew();
		packagePivot = invoiceLine.PackagesPivot.AddNew();
		packagePivot.CHC_CW = package.PK;
	}

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	BasePackage package;
	InvoiceLinePackagePivot packagePivot;

	IPackage GetNewPackageWrapper() => new PackageWrapper(packagePivot);

	void SetUpBulkPackageType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
	}
}
