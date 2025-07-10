using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class InvoiceLinePackageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIsLinked()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "999";
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 0;
			package1.CW_PackType = "VG";
			package1.CW_HouseBill = bill.CU_BillUniqueCode;

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 10;
			package2.CW_PackType = "NE";
			package2.CW_HouseBill = bill.CU_BillUniqueCode;

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 20;
			package3.CW_PackType = "VG";
			package3.CW_HouseBill = bill.CU_BillUniqueCode;

			AssertEquals("Precondition", 3, invoiceLine.PackagesForInvoiceLinesForBindingOnly.Count);

			var linkPackage1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			var linkPackage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
			var linkPackage3 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[2];

			linkPackage1.IsLinked = true;
			linkPackage2.IsLinked = true;
			linkPackage3.IsLinked = true;

			var bulkMessage = "Where an Invoice Line is linked to a bulk package type, it is invalid to link the Invoice Line to other non-bulk package types.";
			CombineAssertions("Mixed, has bulk", () =>
			{
				AssertHasMessageError("Break bulk", linkPackage2.IsLinkedInfo, bulkMessage);
				AssertNoMessageError("Bulk", linkPackage3.IsLinkedInfo, bulkMessage);
			});

			var breakBulkMessage = "Where an Invoice Line is linked to a break bulk package type, it is invalid to link the Invoice Line to other non-break bulk package types.";
			package1.CW_PackType = "NE";
			package3.CW_PackType = "CT";
			linkPackage1.Validation.ValidateIsLinked();
			linkPackage3.Validation.ValidateIsLinked();
			CombineAssertions("Mixed, no bulk, has break bulk", () =>
			{
				AssertNoMessageError("Break bulk", linkPackage1.IsLinkedInfo, breakBulkMessage);
				AssertHasMessageError("Packed", linkPackage3.IsLinkedInfo, breakBulkMessage);
			});

			package1.CW_PackType = "CT";
			package2.CW_PackType = "CT";
			linkPackage1.Validation.ValidateIsLinked();
			linkPackage2.Validation.ValidateIsLinked();
			linkPackage3.Validation.ValidateIsLinked();
			CombineAssertions("All are packed", () =>
			{
				AssertNoMessageErrors(linkPackage1.IsLinkedInfo);
				AssertNoMessageErrors(linkPackage2.IsLinkedInfo);
				AssertNoMessageErrors(linkPackage3.IsLinkedInfo);
			});
		}
	}
}
