using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(InvoiceLineCusLinkPackageCollection))]
	sealed class InvoiceLineCusLinkPackageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoiceLineCusLinkPackageCollection>
	{
		public void TestCheckPackageUnitCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MasterBill = "MB1";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			declaration.Packages.RemoveAndDeleteAll();
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Bag;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BaleCompressed;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BaleUncompressed;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Basket;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Bottle;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Box;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BreakBulk;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.BulkBag;
			declaration.Packages.AddNew().CW_PackType = Core.Constants.PkgUnit.Bundle;
			var package10 = declaration.Packages.AddNew();
			package10.CW_PackType = Core.Constants.PkgUnit.Carton;

			var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly.OfType<InvoiceLineCusLinkPackage>();
			npbos.ForEach(x => x.IsLinked = true);
			Assert(npbos.Any(x => x.RowMessageErrors.Any(y => y.Message == "No more than 9 different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined.")));

			package10.CW_PackType = Core.Constants.PkgUnit.Bundle;
			Assert(!npbos.Any(x => x.RowMessageErrors.Any(y => y.Message == "No more than 9 different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined.")));

			package10.CW_PackType = Core.Constants.PkgUnit.Carton;
			npbos.First().IsLinked = false;
			Assert(!npbos.Any(x => x.RowMessageErrors.Any(y => y.Message == "No more than 9 different units of measure may be used here. Note that pack lines with marks and numbers cannot be combined.")));
		}

		public void TestOnPackageParentChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MasterBill = "MB1";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();

			var collection = new InvoiceLineCusLinkPackageCollection(invoiceLine);

			var expectedPks = new[] { package1.PK, package2.PK, package3.PK };
			var actualPks = collection.Cast<BaseCusLinkPackage>().Select(c => c.PackagePk);

			AssertContainsExactElementsInAnyOrder("Should create 3 link packages.", expectedPks, actualPks);

			package3.CW_CW_Parent = package2.PK;

			expectedPks = new[] { package1.PK, package3.PK };
			actualPks = collection.Cast<BaseCusLinkPackage>().Select(c => c.PackagePk);
			AssertContainsExactElementsInAnyOrder("Should refresh all packages.", expectedPks, actualPks);

			package3.CW_CW_Parent = ZGuid.Empty;

			expectedPks = new[] { package1.PK, package2.PK, package3.PK };
			actualPks = collection.Cast<BaseCusLinkPackage>().Select(c => c.PackagePk);

			AssertContainsExactElementsInAnyOrder("Should refresh all packages.", expectedPks, actualPks);
		}

		public void TestRebuildAllElements()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_MasterBill = "MB1";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();

			declaration.Packages.RemoveAndDeleteAll();

			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			var package3 = declaration.Packages.AddNew();
			package3.CW_CW_Parent = package2.PK;

			var collection = new InvoiceLineCusLinkPackageCollection(invoiceLine);

			var expectedPks = new[] { package1.PK, package3.PK };
			var actualPks = collection.Cast<BaseCusLinkPackage>().Select(c => c.PackagePk);

			AssertContainsExactElementsInAnyOrder("Should only create link package from these lowest packages.", expectedPks, actualPks);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, GetCollectionToTest().AllowNew);
		}

		protected override InvoiceLineCusLinkPackageCollection GetCollectionToTest()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new InvoiceLineCusLinkPackageCollection(invoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new InvoiceLineCusLinkPackage(invoiceLine);
		}
	}
}
