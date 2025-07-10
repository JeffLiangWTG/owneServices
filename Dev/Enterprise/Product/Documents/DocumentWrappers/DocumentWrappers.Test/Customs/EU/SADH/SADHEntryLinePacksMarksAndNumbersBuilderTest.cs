using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing.Documents.Common;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Latvia)]
	sealed class SADHEntryLinePacksMarksAndNumbersBuilderTest : EntryLinePacksMarksAndNumbersBuilderTest
	{
		public void TestFormatPackageDetailFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var wrapper = new SADHEntryLinePacksMarksAndNumbersBuilderForTest(entryLine);
			AssertEquals("Packs and Nos formatted and in English", "Marks & Number=ARG Number=1 Package type=1A", wrapper.FormatPackageDetailFields_Exposed("ARG", "1", "1A"));
		}

		public void TestPackagesSeparator()
		{
			AssertEquals("Separator is always semicolon and space in EU", "; ", new SADHEntryLinePacksMarksAndNumbersBuilderForTest(Factory.New<CusEntryLine>()).PackagesSeparator_Exposed);
		}

		class SADHEntryLinePacksMarksAndNumbersBuilderForTest : SADHEntryLinePacksMarksAndNumbersBuilder
		{
			public SADHEntryLinePacksMarksAndNumbersBuilderForTest(CusEntryLine entryLine) : base(entryLine)
			{
			}

			public ZString FormatPackageDetailFields_Exposed(string pm, string pn, string pc) => base.FormatPackageDetailFields(pm, pn, pc);
			public ZString PackagesSeparator_Exposed => base.PackagesSeparator;
		}
	}
}
