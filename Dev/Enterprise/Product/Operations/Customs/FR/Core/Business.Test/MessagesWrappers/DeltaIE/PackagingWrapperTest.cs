using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class PackagingWrapperTest : DataProviderTestCase<PackagingWrapper>
	{
		public void TestNumberOfPackages()
		{
			AssertEquals("NumberOfPackages should equal numberofpackages parameter.", "35", Provider.NumberOfPackages);
		}

		public void TestShippingMarks()
		{
			AssertEquals("ShippingMarks should equal package.Package.CW_MarksAndNos.", "XXX", Provider.ShippingMarks);
		}

		public void TestTypeOfPackages()
		{
			AssertEquals("TypeOfPackages should equal package.Package.CW_PackType.", "YYY", Provider.TypeOfPackages);
		}

		protected override PackagingWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			declaration.JE_MasterBill = "X";

			var bill = declaration.PrimaryMasterBill;
			var basePackage = bill.PackingGroups[0].Packages.AddNew();
			basePackage.CW_MarksAndNos = "XXX";
			basePackage.CW_PackType = "YYY";
			invoiceLine.PackagesPivot.AddPivotFor(basePackage);

			var package = new InvoiceLineCusLinkPackage(invoiceLine) { IsLinked = true, Package = basePackage };
			package.PackQty = 1;
			return PackagingWrapper.New(package, 35);
		}
	}
}
