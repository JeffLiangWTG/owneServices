using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class DuimpLinesProviderTest : TestCaseWithFactory
	{
		public void TestDuimpLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var dataProvider = DuimpLinesProvider.New(entryHeader.MergedLines);
			AssertEquals(0, dataProvider.Count);

			var invoice = declaration.Invoices.AddNew();
			entryHeader.MergedLines.AddNew().InvoiceLines.Add(invoice.InvoiceLines.AddNew());
			entryHeader.MergedLines.AddNew().InvoiceLines.Add(invoice.InvoiceLines.AddNew());

			dataProvider = DuimpLinesProvider.New(entryHeader.MergedLines);

			AssertNotNull("Identification should NOT be null", dataProvider);
			AssertEquals(2, dataProvider.Count);

			dataProvider = DuimpLinesProvider.New(null);
			AssertNull("Identification should be null", dataProvider);
		}
	}
}
