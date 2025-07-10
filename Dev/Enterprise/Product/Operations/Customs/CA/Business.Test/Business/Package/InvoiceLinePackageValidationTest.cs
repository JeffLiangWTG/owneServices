using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class InvoiceLinePackageValidationTest : TestCaseWithFactory
	{
		public void TestCheckIsLinked()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();

			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();

			var pack = packageGroup.Packages.AddNew();
			var pack2 = packageGroup.Packages.AddNew();
			pack.CW_PackQty = 1;
			pack.CW_PackType = "AE";
			pack2.CW_PackQty = 2;
			pack2.CW_PackType = "AB";

			var headerPackageCollection = (InvoiceHeaderCusLinkPackageCollection)invoice.PackagesForInvoicesForBindingOnly;
			var headerLinkPackage = headerPackageCollection[0];
			headerLinkPackage.Package = pack;
			headerLinkPackage.IsLinked = true;

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine1.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage1 = linePackageCollection[1];
			lineLinkPackage1.IsLinked = true;
			lineLinkPackage1.Package = pack2;

			AssertHasWarningContaining(lineLinkPackage1.IsLinkedInfo, "The packages indicated on this line will override the packages indicated on the invoice header.");
			lineLinkPackage1.IsLinked = false;
			AssertNoWarningContaining(lineLinkPackage1.IsLinkedInfo, "The packages indicated on this line will override the packages indicated on the invoice header.");

			var lineLinkPackage0 = linePackageCollection[0];
			lineLinkPackage0.Package = pack;
			lineLinkPackage0.IsLinked = true;
			AssertNoWarningContaining(lineLinkPackage0.IsLinkedInfo, "The packages indicated on this line will override the packages indicated on the invoice header.");
		}

		public void TestCheckPackQty()
		{
			const string expectedMessage = "Packages cannot be entered at both Invoice Header and Invoice Line levels. Please remove packages at the Invoice Line level if being entered for the entire invoice at the Invoice Header level.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();

			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();

			var pack = packageGroup.Packages.AddNew();
			pack.CW_PackQty = 1;
			pack.CW_PackType = "AE";

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;

			var headerLinkPackage = Factory.New<InvoiceHeaderPackagePivot>();
			headerLinkPackage.CHZ_JE = declaration.PK;
			headerLinkPackage.CHZ_JZ = invoice.PK;
			headerLinkPackage.CHZ_CW = pack.PK;
			headerLinkPackage.CHZ_NumberOfPacks = 10;
			invoice.PackagesPivot.Add(headerLinkPackage);

			var lineLinkPackage = Factory.New<InvoiceLinePackagePivot>();
			lineLinkPackage.CHC_JE = declaration.PK;
			lineLinkPackage.CHC_JI = invoiceLine.PK;
			lineLinkPackage.CHC_CW = pack.PK;
			lineLinkPackage.CHC_NumberOfPacks = 20;
			invoiceLine.PackagesPivot.Add(lineLinkPackage);

			linePackageCollection[0].Validation.ValidatePackQty();
			AssertHasMessageErrorContaining(linePackageCollection[0].PackQtyInfo, expectedMessage);
			linePackageCollection[0].IsLinked = false;
			linePackageCollection[0].Validation.ValidatePackQty();
			AssertNoMessageErrorContaining(linePackageCollection[0].PackQtyInfo, expectedMessage);
		}
	}
}
