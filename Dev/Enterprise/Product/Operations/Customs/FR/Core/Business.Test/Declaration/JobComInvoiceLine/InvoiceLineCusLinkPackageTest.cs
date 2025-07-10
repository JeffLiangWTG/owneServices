using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineCusLinkPackage))]
	class InvoiceLineCusLinkPackageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsLinked_ReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "BX";
			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackType = "BX";
			var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackType = "BG";

			var linkPackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;

			AssertEquals(3, linkPackageCollection.Count);

			linkPackageCollection[0].IsLinked = false;
			AssertEquals("IsLinked should not be read-only when no other package is flagged as linked", false, linkPackageCollection[0].IsLinkedInfo.ReadOnly);

			linkPackageCollection[1].IsLinked = false;
			AssertEquals("IsLinked should not be read-only when no other package is flagged as linked", false, linkPackageCollection[1].IsLinkedInfo.ReadOnly);

			linkPackageCollection[2].IsLinked = false;
			AssertEquals("IsLinked should not be read-only when no other package is flagged as linked", false, linkPackageCollection[2].IsLinkedInfo.ReadOnly);

			linkPackageCollection[0].IsLinked = true;
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[0].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[1].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should be read-only when another package with different package type is flagged as linked", true, linkPackageCollection[2].IsLinkedInfo.ReadOnly);

			linkPackageCollection[0].IsLinked = false;
			linkPackageCollection[1].IsLinked = true;
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[0].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[1].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should be read-only when another package with different package type is flagged as linked", true, linkPackageCollection[2].IsLinkedInfo.ReadOnly);

			linkPackageCollection[1].IsLinked = false;
			linkPackageCollection[2].IsLinked = true;
			AssertEquals("IsLinked should be read-only when another package with different package type is flagged as linked", true, linkPackageCollection[0].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should be read-only when another package with different package type is flagged as linked", true, linkPackageCollection[1].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[2].IsLinkedInfo.ReadOnly);

			linkPackageCollection[2].IsLinked = false;
			linkPackageCollection[0].IsLinked = true;
			linkPackageCollection[1].IsLinked = true;
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[0].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[1].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should be read-only when another package with different package type is flagged as linked", true, linkPackageCollection[2].IsLinkedInfo.ReadOnly);
		}
		public void TestIsLinked_ReadOnly_WhenParentInvoiceLineIsSubjectToExistingUCC6Declaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackType = "BX";
			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackType = "BX";
			var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackType = "BG";

			var linkPackageCollection = invoiceLine.PackagesForInvoiceLinesForBindingOnly;

			AssertEquals(3, linkPackageCollection.Count);

			linkPackageCollection[0].IsLinked = true;
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[0].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should not be read-only when no other package with different package type is flagged as linked", false, linkPackageCollection[1].IsLinkedInfo.ReadOnly);
			AssertEquals("IsLinked should be read-only when another package with different package type is flagged as linked", true, linkPackageCollection[2].IsLinkedInfo.ReadOnly);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertEquals("IsLinked should not be read-only when parent invoice line is subject to existing UCC6 declaration", false, linkPackageCollection[2].IsLinkedInfo.ReadOnly);
		}

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
	}
}
