using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing.Documents.Common;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.DocumentWrappers.Transit.Testing;

sealed class T2LEntryLinePacksMarksAndNumbersBuilderTest : EntryLinePacksMarksAndNumbersBuilderTest
{
	public void TestFormatPackageDetailFields()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		var builder = new T2LEntryLinePacksMarksAndNumbersBuilderForTest(entryLine);
		AssertEquals("Packs and Nos formatted", "Nombre et nature :    6  PK\r\nMarques et numéro :    RED", builder.FormatPackageDetailFields_Exposed("RED", "6", "PK"));
	}

	class T2LEntryLinePacksMarksAndNumbersBuilderForTest : T2LEntryLinePacksMarksAndNumbersBuilder
	{
		public T2LEntryLinePacksMarksAndNumbersBuilderForTest(CusEntryLine entryLine) : base(entryLine)
		{
		}

		public ZString FormatPackageDetailFields_Exposed(string pm, string pn, string pc) => base.FormatPackageDetailFields(pm, pn, pc);
	}
}
