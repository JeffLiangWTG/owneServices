using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(InvoiceLineCusLinkPackage))]
	class InvoiceLineCusLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsLinked_AutoSetMainPackWhenIsLinkedFirstTime()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "999";
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "VG";
			package1.CW_HouseBill = bill.CU_BillUniqueCode;
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 10;
			package2.CW_PackType = "CT";
			package2.CW_HouseBill = bill.CU_BillUniqueCode;

			invoiceLine1.PackagesForInvoiceLinesForBindingOnly[0].IsLinked = true;
			invoiceLine2.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;
			invoiceLine3.PackagesForInvoiceLinesForBindingOnly[1].IsLinked = true;

			AssertEquals("Not Packed", false, invoiceLine1.ZG_IsMainPack);
			AssertEquals("Packed, link first time", true, invoiceLine2.ZG_IsMainPack);
			AssertEquals("Packed, link second time", false, invoiceLine3.ZG_IsMainPack);
		}

		public void TestPackQtyAndReadonly()
		{
			TestDataHelper.SetUpPackageTypes(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;

			var pkgVG = declaration.Packages.AddNew();
			pkgVG.CW_PackType = "VG";
			pkgVG.CW_MarksAndNos = "VGMark";
			pkgVG.CW_PackQty = 100;
			var pkgNE = declaration.Packages.AddNew();
			pkgNE.CW_PackType = "NE";
			pkgNE.CW_MarksAndNos = "NEMark";
			pkgNE.CW_PackQty = 200;
			var pkg1A = declaration.Packages.AddNew();
			pkg1A.CW_PackType = "1A";
			pkg1A.CW_MarksAndNos = "1AMark";
			pkg1A.CW_PackQty = 300;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var linkVG = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault(link => link.Package == pkgVG);
			linkVG.IsLinked = true;
			linkVG.PackQty = 10;
			AssertEquals("PackQty for BULK packs returns Package.CW_PackQty.", 10, linkVG.PackQty);
			AssertEquals("PackQty readonly for BULK packs.", true, linkVG.PackQtyInfo.ReadOnly);

			var linkNE = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault(link => link.Package == pkgNE);
			linkNE.IsLinked = true;
			linkNE.PackQty = 20;
			AssertEquals("PackQty for UNPACKED packs returns base.PackQty.", 20, linkNE.PackQty);
			AssertEquals("PackQty writable for UNPACKED packs.", false, linkNE.PackQtyInfo.ReadOnly);

			var link1A = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().FirstOrDefault(link => link.Package == pkg1A);
			link1A.IsLinked = true;
			link1A.PackQty = 30;
			AssertEquals("PackQty for normal packs returns Package.CW_PackQty.", 30, link1A.PackQty);
			AssertEquals("PackQty readonly for normal packs.", false, link1A.PackQtyInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var collection = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>();

			var linkPackage = collection.FirstOrDefault();
			linkPackage.IsLinked = true;

			return linkPackage;
		}
	}
}
