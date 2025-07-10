using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MoneyProviderTest : Customs.Business.Testing.DataProviderTestCase<MoneyProvider>
	{
		public void TestValue()
		{
			AssertEquals(33.33m, dataProvider.Value);
		}

		public void TestCurrencyCode()
		{
			AssertEquals("INR", dataProvider.CurrencyCode);
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "INR";

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_LinePrice = 11.11m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_LinePrice = 22.22m;

			dataProvider = new MoneyProvider(entryInstruction, invoice);
		}
		MoneyProvider dataProvider;

		protected override MoneyProvider GetProvider() => dataProvider;
	}
}
