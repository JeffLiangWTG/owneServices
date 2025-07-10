using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.DocumentWrappers.SADH.Testing;

sealed class SADHEntryLinePacksMarksAndNumbersBuilderTest : EU.Business.Testing.Documents.Common.EntryLinePacksMarksAndNumbersBuilderTest
{
	public void TestFormatPackageDetailFields()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		string expectedPacksMarksAndNos = @"Nb et nature des colis : 6 - PK
Marques et numéros : RED";

		var wrapper = new FRSADHEntryLinePacksMarksAndNumbersBuilderForTest(entryLine);
		AssertEquals("Packs and Nos formatted and in English", expectedPacksMarksAndNos, wrapper.FormatPackageDetailFields_Exposed("RED", "6", "PK"));
	}

	public void TestPackagesSeparator()
	{
		AssertEquals("Separator is always carriage return in FR", "\r\n", new FRSADHEntryLinePacksMarksAndNumbersBuilderForTest(Factory.New<CusEntryLine>()).PackagesSeparator_Exposed);
	}

	class FRSADHEntryLinePacksMarksAndNumbersBuilderForTest : SADHEntryLinePacksMarksAndNumbersBuilder
	{
		public FRSADHEntryLinePacksMarksAndNumbersBuilderForTest(CusEntryLine entryLine) : base(entryLine)
		{
		}

		public ZString FormatPackageDetailFields_Exposed(string pm, string pn, string pc) => base.FormatPackageDetailFields(pm, pn, pc);
		public ZString PackagesSeparator_Exposed => base.PackagesSeparator;
	}
}
