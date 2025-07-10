using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(InvoiceLineCusLinkPackage))]
	class InvoiceLineCusLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var collection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var package = Factory.New<EU.Business.Declaration.Package>();

			var linkPackage = collection.AddNew();
			linkPackage.Package = package;
			linkPackage.IsLinked = true;

			return linkPackage;
		}

		public void TestIsLinked_ReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MasterBill = "M";

			var package = declaration.Bills[0].PackingGroups[0].Packages[0];
			package.CW_PackQty = 1;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var linePackageCollection = (InvoiceLineCusLinkPackageCollection)invoiceLine.PackagesForInvoiceLinesForBindingOnly;
			var lineLinkPackage = linePackageCollection[0];
			lineLinkPackage.Package = package;
			lineLinkPackage.IsLinked = false;
			Assert(lineLinkPackage.IsPackQty_ReadOnly);

			lineLinkPackage.IsLinked = true;
			Assert(!lineLinkPackage.IsPackQty_ReadOnly);

			invoiceLine.JI_IsMainPack = true;
			Assert(lineLinkPackage.IsPackQty_ReadOnly);

			invoiceLine.JI_IsMainPack = false;
			Assert(!lineLinkPackage.IsPackQty_ReadOnly);
		}
	}
}
