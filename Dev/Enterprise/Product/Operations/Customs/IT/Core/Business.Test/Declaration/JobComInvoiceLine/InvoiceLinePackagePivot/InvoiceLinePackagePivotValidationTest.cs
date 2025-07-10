using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InvoiceLinePackagePivotValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckWhenItemsAreBulkOrUnpacked()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageNE, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageVG, 0);

		var invoiceLine3 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff 3", "invoiceLine3");
		var packagePivot3 = MergeTestHelper.GetNewPackagePivot(invoiceLine3, packageCT, 0);

		new LineMerger(declaration).DoMerge();

		packagePivot1.Validation.ValidateAll();
		packagePivot2.Validation.ValidateAll();
		packagePivot3.Validation.ValidateAll();

		CombineAssertions("Message errors related to Bulk, Unpacked and normal packages", () =>
		{
			AssertNoMessageErrorContaining("When Pack type is NE, the error related to packs and marks is not shown", packagePivot1.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			AssertNoMessageErrorContaining("When Pack type is VG, the error related to packs and marks is not shown", packagePivot2.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			AssertHasMessageErrorContaining("When Pack type is not VG or NE, the error related to packs and marks is shown", packagePivot3.CHC_NumberOfPacksInfo, ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
		});
	}

	public void TestCheckCHC_NumberOfPacks()
	{
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "1";

		var invoiceLine1 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff1", "invoiceLine1");
		var packagePivot1 = MergeTestHelper.GetNewPackagePivot(invoiceLine1, packageCT, 0);

		var invoiceLine2 = MergeTestHelper.GetNewInvoiceLine(invoice, "tariff2", "invoiceLine2");
		var packagePivot2 = MergeTestHelper.GetNewPackagePivot(invoiceLine2, packageCT, 0);

		var quantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame = ValidationCaptions.Package.QuantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame;

		packagePivot1.Validation.ValidateAll();
		packagePivot2.Validation.ValidateAll();
		CombineAssertions("Message errors related to Number of Packages never appear in unmerged declarations", () =>
		{
			AssertNoMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, quantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, quantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
		});

		LineMerger merger = new LineMerger(declaration);
		merger.DoMerge();

		packagePivot1.Validation.ValidateAll();
		packagePivot2.Validation.ValidateAll();
		CombineAssertions("Message errors related to Number of Packages appear in merged declarations", () =>
		{
			AssertHasMessageErrorContaining(packagePivot1.CHC_NumberOfPacksInfo, quantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
			AssertNoMessageErrorContaining(packagePivot2.CHC_NumberOfPacksInfo, quantityCanBeZeroOnlyWhenTypeAndMarkAreTheSame);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		Factory.Save();

		declaration = Factory.New<JobDeclaration>();

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "EXP";
		declaration.JE_ApplicationCode = "BLT";

		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "OOCL0000006";

		var declarationBill = declaration.Bills.AddNew();
		declarationBill.CU_BillType = BillTypeList.Codes.MasterBill;
		declarationBill.CU_BillNum = "999";

		packageCT = declaration.Packages.AddNew();
		packageCT.CW_PackQty = 1;
		packageCT.CW_PackType = "CT";
		packageCT.CW_MarksAndNos = "IND";
		packageCT.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		packageVG = (EU.Business.Declaration.Package)declaration.Packages.AddNew();
		packageVG.CW_PackQty = 1;
		packageVG.CW_PackType = "VG";
		packageVG.CW_MarksAndNos = "IND";
		packageVG.CW_HouseBill = declarationBill.CU_BillUniqueCode;

		packageNE = (EU.Business.Declaration.Package)declaration.Packages.AddNew();
		packageNE.CW_PackQty = 1;
		packageNE.CW_PackType = "NE";
		packageNE.CW_MarksAndNos = "IND";
		packageNE.CW_HouseBill = declarationBill.CU_BillUniqueCode;
	}

	JobDeclaration declaration;

	BasePackage packageNE;
	BasePackage packageVG;
	BasePackage packageCT;
}
