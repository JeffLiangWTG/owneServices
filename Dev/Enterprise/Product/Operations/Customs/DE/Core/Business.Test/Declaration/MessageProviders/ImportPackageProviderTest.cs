using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Business;
using JobDeclaration = Enterprise.Customs.DE.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportPackageProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportPackageProvider>
	{
		public void TestNew()
		{
			AssertNull("Argument == null", ImportPackageProvider.NewOrNull(Enumerable.Empty<InvoiceLinePackagePivot>()));
		}

		public void TestKind()
		{
			package1.CW_PackType = "BO";
			package2.CW_PackType = "NE";
			AssertEquals("PackType of first package", "BO", dataProvider.Kind);
		}

		public void TestQuantity_AllPackTypesAreCountable()
		{
			AssertEquals("Sum of CHC_NumberOfPacks", 15, dataProvider.Quantity);
		}

		public void TestQuantity_NotAllPackTypesAreCountable()
		{
			package1.CW_PackType = "NE";
			package2.CW_PackType = "BO";
			AssertNull(dataProvider.Quantity);
		}

		public void TestMarksNumbers_AllPackTypesAreCountable()
		{
			package1.CW_PackType = "BO";
			package1.CW_MarksAndNos = "MARKS AND NUMBERS1";
			package2.CW_PackType = "BO";
			package2.CW_MarksAndNos = "MARKS AND NUMBERS2";
			AssertEquals("Marks and numbers of first package", "MARKS AND NUMBERS1", dataProvider.MarksNumbers);
		}

		public void TestMarksNumbers_NotAllPackTypesAreCountable()
		{
			package1.CW_PackType = "NE";
			package1.CW_MarksAndNos = "MARKS AND NUMBERS1";
			package2.CW_PackType = "BO";
			package2.CW_MarksAndNos = "MARKS AND NUMBERS2";
			AssertEquals(ZString.Empty, dataProvider.MarksNumbers);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_IsMainPack = true;
			package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 10;
			package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 5;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().First().IsLinked = true;
			var linkPackage2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().Skip(1).First().IsLinked = true;
			dataProvider = ImportPackageProvider.NewOrNull(entryLine.PackagingDetails);
		}
		Declaration.CusEntryHeader entryHeader;
		IImportPackage dataProvider;
		BasePackage package1;
		BasePackage package2;

		protected override ImportPackageProvider GetProvider() => (ImportPackageProvider)dataProvider;
	}
}
