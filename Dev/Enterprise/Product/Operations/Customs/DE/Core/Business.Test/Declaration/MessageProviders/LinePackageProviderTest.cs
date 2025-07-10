using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class LinePackageProviderTest : Customs.Business.Testing.DataProviderTestCase<LinePackageProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", LinePackageProvider.NewOrNull(null));
		}

		public void TestQuantity()
		{
			AssertEquals(10, dataProvider.Quantity);
		}

		public void TestIsSupportEmptyPackType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CW_PackType = 'CT'", true, dataProvider.IsSupportEmptyPackType);

				package.CW_PackType = "VG";
				dataProvider = LinePackageProvider.NewOrNull(entryLine.PackagingDetails.Single());
				AssertEquals("CW_PackType = 'VG'", false, dataProvider.IsSupportEmptyPackType);
			});
		}

		public void TestKind()
		{
			AssertEquals("CT", dataProvider.Kind);
		}

		public void TestMarksNumbers()
		{
			AssertEquals("MARKS AND NUMBERS", dataProvider.MarksNumbers);
		}

		public void TestPositionNumber()
		{
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var npbos2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Single();
			npbos2.IsLinked = true;
			npbos2.PackQty = 0;
			dataProvider = LinePackageProvider.NewOrNull(entryLine2.PackagingDetails.Single());
			AssertEquals(1, dataProvider.PositionNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_IsMainPack = true;
			package = declaration.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = "CT";
			package.CW_MarksAndNos = "MARKS AND NUMBERS";
			var npbos = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Single();
			npbos.IsLinked = true;
			dataProvider = LinePackageProvider.NewOrNull(entryLine.PackagingDetails.Single());
		}
		Declaration.CusEntryHeader entryHeader;
		JobComInvoiceHeader invoiceHeader;
		Declaration.CusEntryLine entryLine;
		BasePackage package;
		IPackage dataProvider;

		protected override LinePackageProvider GetProvider() => (LinePackageProvider)dataProvider;
	}
}
