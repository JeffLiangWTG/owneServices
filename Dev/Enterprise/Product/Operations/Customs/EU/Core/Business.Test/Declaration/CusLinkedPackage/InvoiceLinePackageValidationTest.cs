using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

class InvoiceLinePackageValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckRuleR0219()
	{
		var messageError = "[R0219] If any of the package qty lines = 0 (zero) no other package quantity greater than 0 (zero) is allowed for the item";

		var basePackage2 = declaration.Packages.AddNew();
		var basePackage3 = declaration.Packages.AddNew();

		linePackage.IsLinked = false;
		var linePackage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage2.Package = basePackage2;
		var linePackage3 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage3.Package = basePackage3;

		CombineAssertions(() =>
		{
			using (var context = new InvoiceLinePackageValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(r => r.IsRuleR0219Active);

				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError("Zero Linked Packages", linePackage.PackQtyInfo, messageError);

				linePackage.IsLinked = true;
				linePackage2.IsLinked = true;
				linePackage3.IsLinked = true;
				linePackage2.PackQty = 0;
				linePackage3.PackQty = 0;
				linePackage.PackQty = 0;
				AssertNoMessageError("All Linked Packages have Pack Qty. 0", linePackage.PackQtyInfo, messageError);

				linePackage2.PackQty = 0;
				linePackage.PackQty = 1;
				AssertHasMessageError("Line Package Pack Qty. is 1 while a linked line package with pack qty as 0 exist", linePackage.PackQtyInfo, messageError);

				linePackage3.PackQty = 0;
				linePackage.Validation.ValidatePackQty();
				AssertHasMessageError("Line Package Pack Qty. is 1 while two linked line packages with pack qty as 0 exist", linePackage.PackQtyInfo, messageError);

				linePackage2.IsLinked = false;
				linePackage3.IsLinked = false;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError("Line Package Pack Qty. is 1 but no linked line package with pack qty 0 exists", linePackage.PackQtyInfo, messageError);

				linePackage2.IsLinked = true;
				linePackage2.PackQty = 1;
				linePackage.PackQty = 0;
				AssertNoMessageError("The Line Package has Pack Qty. 0", linePackage.PackQtyInfo, messageError);

				context.DisableRule(r => r.IsRuleR0219Active);
				linePackage3.IsLinked = true;
				linePackage3.PackQty = 0;
				linePackage2.PackQty = 0;
				linePackage.PackQty = 1;
				AssertNoMessageError("Line Package Pack Qty. is 1 while two linked line packages with pack qty as 0 exist but RULE IS INACTIVE", linePackage.PackQtyInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR0220()
	{
		var messageError = "[R0220] If packages are 0 (zero) the package type cannot be NE, NF or NG";

		CombineAssertions(() =>
		{
			using (var context = new InvoiceLinePackageValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(r => r.IsRuleR0220Active);

				basePackage.CW_PackType = "NE";
				linePackage.PackQty = 1;
				AssertNoMessageError("BreakBulk code PackQty is 0", linePackage.PackQtyInfo, messageError);

				linePackage.PackQty = 0;
				AssertHasMessageError("BreakBulk code PackQty not 0", linePackage.PackQtyInfo, messageError);

				linePackage.IsLinked = false;
				AssertNoMessageError("Not Linked", linePackage.PackQtyInfo, messageError);
				linePackage.IsLinked = true;
				basePackage.CW_PackType = "AA";
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError("BreakBulk code PackQty not 0", linePackage.PackQtyInfo, messageError);

				context.DisableRule(r => r.IsRuleR0220Active);
				linePackage.PackQty = 0;
				basePackage.CW_PackType = "NE";
				AssertNoMessageError("BreakBulk code PackQty not 0", linePackage.PackQtyInfo, messageError);
			}
		});
	}

	public void TestCheckRuleR0364()
	{
		var messageError = "[R0364] At least one other line item must have a number of packages greater than 0 for the same package number when the type of packages are not NE, NF or NG";

		var invoiceLine2 = header.InvoiceLines.AddNew();
		basePackage.CW_MarksAndNos = "M&N";
		basePackage.CW_PackType = "NE";

		var linePackage2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage2.Package = basePackage;
		linePackage2.IsLinked = true;
		linePackage2.PackQty = 0;

		CombineAssertions(() =>
		{
			using (var context = new InvoiceLinePackageValidationDeciderTestContext(declaration, true))
			{
				context.EnableRule(r => r.IsRuleR0364Active);

				linePackage.PackQty = 0;
				AssertNoMessageError("Package Type is NE - BreakBulk", linePackage.PackQtyInfo, messageError);

				basePackage.CW_PackType = "VO";
				linePackage.PackQty = 0;
				AssertHasMessageError("Line Package Pack Qty. is 0, Not BreakBulk PackType and no other line item has Pack Qty. greater than 0 for the same package number", linePackage.PackQtyInfo, messageError);

				linePackage2.PackQty = 1;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError("Line Package Pack Qty. is 0, Not BreakBulk Pack Type but another line item has Pack Qty. greater than 0 for the same package number", linePackage.PackQtyInfo, messageError);

				context.DisableRule(r => r.IsRuleR0364Active);
				linePackage2.PackQty = 0;
				linePackage.Validation.ValidatePackQty();
				AssertNoMessageError("Line Package Pack Qty. is 0, Not BreakBulk Pack Type and no other line item has Pack Qty. greater than 0 for the same package number but RULE IS INACTIVE", linePackage.PackQtyInfo, messageError);
			}
		});
	}

	void SetupPackageTypes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VO", "VO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

		Factory.Save();
	}

	protected override void SetUp()
	{
		SetupPackageTypes();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		basePackage = declaration.Packages.AddNew();
		header = declaration.Invoices.AddNew();
		invoiceLine = header.InvoiceLines.AddNew();
		linePackage = invoiceLine.PackagesForInvoiceLinesForBindingOnly.AddNew();
		linePackage.Package = basePackage;
		linePackage.IsLinked = true;
	}

	protected JobDeclaration declaration;
	protected JobComInvoiceHeader header;
	protected JobComInvoiceLine invoiceLine;
	protected BasePackage basePackage;
	protected BaseCusLinkPackage linePackage;
}
