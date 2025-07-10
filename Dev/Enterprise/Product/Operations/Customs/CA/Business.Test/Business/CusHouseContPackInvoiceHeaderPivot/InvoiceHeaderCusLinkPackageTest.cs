using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderCusLinkPackage))]
	sealed class InvoiceHeaderCusLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableNameCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Package", new InvoiceHeaderCusLinkPackage(invoice).HumanReadableName);
		}

		public void TestPackQty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.CA_RequiresMerge = false;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = declaration.Invoices.AddNew();

			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();

			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();
			var package = packageGroup.Packages.AddNew();

			package.CW_PackQty = 20;
			AssertEquals("Precondition", 0m, invoice.JZ_NoOfPacks);

			var headerPackageCollection = (InvoiceHeaderCusLinkPackageCollection)invoice.PackagesForInvoicesForBindingOnly;
			var headerLinkPackage = headerPackageCollection[0];
			headerLinkPackage.Package = package;
			headerLinkPackage.IsLinked = true;

			headerLinkPackage.PackQty = 3;
			Assert("Should mark as merge as the PackQty is changed.", declaration.CA_RequiresMerge);

			declaration.CA_RequiresMerge = false;
			headerLinkPackage.IsLinked = false;
			Assert("Should mark as merge as the IsLinked is changed.", declaration.CA_RequiresMerge);

			headerLinkPackage.IsLinked = true;
			headerLinkPackage.PackQty = 0;

			invoice.JZ_NoOfPacks = 5;
			declaration.CA_RequiresMerge = false;

			headerLinkPackage.IsLinked = false;
			Assert("Should not mark as merge as the before PackQty is 0.", !declaration.CA_RequiresMerge);
		}

		public void TestIsLinked()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			declaration.JE_MasterBill = "X";
			declaration.Packages.RemoveAndDeleteAll();

			var bill = declaration.PrimaryMasterBill;
			var packageGroup = bill.PackingGroups.AddNew();
			var package = packageGroup.Packages.AddNew();

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage = linePackageCollection[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = true;

			var headerPackageCollection = (InvoiceHeaderCusLinkPackageCollection)invoice.PackagesForInvoicesForBindingOnly;
			var headerLinkPackage = headerPackageCollection[0];
			headerLinkPackage.Package = package;
			headerLinkPackage.IsLinked = false;

			AssertEquals(true, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals(false, invoice.PackagesForInvoicesForBindingOnly[0].IsLinked);

			headerLinkPackage.IsLinked = true;

			AssertEquals(false, invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			AssertEquals(true, invoice.PackagesForInvoicesForBindingOnly[0].IsLinked);
		}

		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
			ErrorReporter.Clear(); // ReportErrorIfPivotIsEmpty is triggered
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var package = Factory.New<Package>();
			var collection = (InvoiceHeaderCusLinkPackageCollection)invoice.PackagesForInvoicesForBindingOnly;

			var linkPackage = collection.AddNew();
			linkPackage.Package = package;
			linkPackage.IsLinked = true;

			return linkPackage;
		}
	}
}
