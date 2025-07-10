using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(Package))]
class PackageTest : BasePackageTest
{
	public void TestValidation_Type()
	{
		var package = Factory.New<Package>();
		AssertType<PackageValidation>(package.Validation);
	}

	public void TestShouldDeleteIfPackQtyIsEmpty()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var bill = declaration.Bills.AddNew();
		bill.CU_BillNum = "ABC";

		var package0 = declaration.Packages.AddNew();
		package0.CW_HouseBill = bill.CU_BillUniqueCode;
		package0.CW_PackType = RefCusCodeTestHelper.UNPKGCodeAttributeYes;
		package0.CW_PackQty = 0;
		var package1 = declaration.Packages.AddNew();
		package1.CW_HouseBill = bill.CU_BillUniqueCode;
		package1.CW_PackType = RefCusCodeTestHelper.UNPKGCodeAttributeNo;
		package1.CW_PackQty = 0;

		Factory.Save();

		AssertEquals(2, declaration.Packages.Count);
	}

	public void TestIsLoosePackaging()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var package = declaration.Packages.AddNew();

		CombineAssertions(() =>
		{
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
			AssertEquals("Bulk", true, package.IsLoosePackaging);
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
			AssertEquals("No bulk", false, package.IsLoosePackaging);
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes;
			AssertEquals("BreakBulk", true, package.IsLoosePackaging);
		});
	}

	public void TestIsBulkOnlyPackaging()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var package = declaration.Packages.AddNew();

		CombineAssertions(() =>
		{
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
			AssertEquals("Bulk", true, package.IsBulkOnlyPackaging);
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
			AssertEquals("No bulk", false, package.IsBulkOnlyPackaging);
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes;
			AssertEquals("BreakBulk", false, package.IsBulkOnlyPackaging);
		});
	}

	public void TestIsBreakBulkPackaging()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var package = declaration.Packages.AddNew();

		CombineAssertions(() =>
		{
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBulkYes;
			AssertEquals("Bulk", false, package.IsBreakBulkPackaging);
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeNoBulk;
			AssertEquals("No bulk", false, package.IsBreakBulkPackaging);
			package.CW_PackType = RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes;
			AssertEquals("BreakBulk", true, package.IsBreakBulkPackaging);
		});
	}

	public void TestTotalPackQtyOnInvoiceLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var package = declaration.Packages.AddNew();
		_ = declaration.Packages.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		var bindingLine1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0];
		bindingLine1.IsLinked = true;
		bindingLine1.PackQty = 3;
		var bindingLine1OtherPackage = invoiceLine1.PackagesForInvoiceLinesForBindingOnly[1];
		bindingLine1OtherPackage.IsLinked = true;
		bindingLine1OtherPackage.PackQty = 5;

		var bindingLine2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly[0];
		bindingLine2.IsLinked = true;
		bindingLine2.PackQty = 4;

		AssertEquals(7, package.TotalPackQtyOnInvoiceLines);
	}

	public void TestGetNewInvoiceLinePivotCollectionCore()
	{
		var package = Factory.New<Package>();
		AssertType<PackagePivotsCollection>(package.InvoiceLinePivotCollection);
	}
}
