using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PackageValidation))]
sealed class PackageValidationTest : CusDecHouseContainerPackValidationTest
{
	protected override BaseJobDeclaration GetJobDeclaration()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		return declaration;
	}

	JobDeclaration Declaration => declaration ??= (JobDeclaration)GetJobDeclaration();
	JobDeclaration declaration;

	public void TestCheckCW_PackQty()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var declaration = Factory.New<JobDeclaration>();

		var package = declaration.Packages.AddNew();
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		package.CW_PackQty = 10;

		var otherPackage = declaration.Packages.AddNew();
		otherPackage.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		otherPackage.CW_PackQty = 20;

		CombineAssertions(() =>
		{
			AssertNoMessageError("When no invoice lines at all", package.CW_PackQtyInfo, ValidationMessages.Package.TotalLineQtyTooSmall);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			package.Validation.ValidateCW_PackQty();
			AssertHasMessageError("When no invoice line assigned", package.CW_PackQtyInfo, ValidationMessages.Package.TotalLineQtyTooSmall);

			var bindingLine1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
			bindingLine1.IsLinked = true;
			var bindingLine1OtherPackage = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1];
			bindingLine1OtherPackage.IsLinked = true;

			var bindingLine2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
			bindingLine2.IsLinked = true;

			bindingLine1.PackQty = 8;
			bindingLine2.PackQty = 0;
			package.Validation.ValidateCW_PackQty();
			AssertHasMessageError("When sum < CW_PackQty", package.CW_PackQtyInfo, ValidationMessages.Package.TotalLineQtyTooSmall);

			bindingLine1.PackQty = 8;
			bindingLine2.PackQty = 0;
			bindingLine1OtherPackage.PackQty = 2;
			package.Validation.ValidateCW_PackQty();
			AssertHasMessageError("Should not take other packages into account", package.CW_PackQtyInfo, ValidationMessages.Package.TotalLineQtyTooSmall);

			bindingLine1.PackQty = 8;
			bindingLine2.PackQty = 2;
			package.Validation.ValidateCW_PackQty();
			AssertNoMessageError("When sum = CW_PackQty", package.CW_PackQtyInfo, ValidationMessages.Package.TotalLineQtyTooSmall);

			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;

			bindingLine1.PackQty = 8;
			bindingLine2.PackQty = 0;
			package.Validation.ValidateCW_PackQty();
			AssertNoMessageError("When loose package", package.CW_PackQtyInfo, ValidationMessages.Package.TotalLineQtyTooSmall);
		});
	}

	public void TestCheckCW_MarksAndNos_R132() => AssertCheckCW_MarksAndNos(JobMessageTypeList.Codes.Import, ValidationMessages.Plausi.MessageR132);

	public void TestCheckCW_MarksAndNos_NS30021() => AssertCheckCW_MarksAndNos(JobMessageTypeList.Codes.Export, "[NS30021] You have not entered Shipping Marks.");

	void AssertCheckCW_MarksAndNos(string messageType, string validationMessage) => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);
		Declaration.JE_MessageType = messageType;
		var package = Declaration.Packages.AddNew();

		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
		package.Validation.ValidateCW_MarksAndNos();
		AssertNoMessageError($"CW_PackType={RefCusCodeTestHelper.UNPKGCodeWithBulkYes}", package.CW_MarksAndNosInfo, validationMessage);
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes;
		package.Validation.ValidateCW_MarksAndNos();
		AssertNoMessageError($"CW_PackType={RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes}", package.CW_MarksAndNosInfo, validationMessage);
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		AssertHasMessageError($"CW_PackType={RefCusCodeTestHelper.UNPKGCodeNoBulk}", package.CW_MarksAndNosInfo, validationMessage);

		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
		package.CW_MarksAndNos = "Test";
		AssertNoMessageError($"CW_PackType={RefCusCodeTestHelper.UNPKGCodeWithBulkYes}", package.CW_MarksAndNosInfo, validationMessage);
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes;
		AssertNoMessageError($"CW_PackType={RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes}", package.CW_MarksAndNosInfo, validationMessage);
		package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
		AssertNoMessageError($"CW_PackType={RefCusCodeTestHelper.UNPKGCodeNoBulk}", package.CW_MarksAndNosInfo, validationMessage);
	});

	public void TestCheckCW_PackType_Import() => AssertCheckCW_PackType(JobMessageTypeList.Codes.Import);

	public void TestCheckCW_PackType_Export() => AssertCheckCW_PackType(JobMessageTypeList.Codes.Export);

	void AssertCheckCW_PackType(string messageType)
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);
		Declaration.JE_MessageType = messageType;
		var package = Declaration.Packages.AddNew();

		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(package.CW_PackTypeInfo, "X", RefCusCodeTestHelper.UNPKGCodeWithBulkYes);
	}
}
