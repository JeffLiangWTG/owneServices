using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class InvoiceLineWrapperTest : DataProviderTestCase<InvoiceLineWrapper>
	{
		public void TestItemAmountInvoiced()
		{
			AssertEquals("IsMultiInvoiceCurrency is false, ItemAmountInvoiced should equal sum of all JI_LinePrices in invoice currency.", 3d, Provider.ItemAmountInvoiced);
		}

		public void TestItemAmountInvoicedWithMultipleCurrencies()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line1 = CreateInvoiceWithLinesAndCurrency(declaration, "USD", 1d, 2d);
			var line2 = CreateInvoiceWithLinesAndCurrency(declaration, "JPY", 10d, 10d);

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = line1.CusEntryLine as CusEntryLine;

			var provider2 = InvoiceLineWrapper.New(entryLine);
			AssertEquals("IsMultiInvoiceCurrency is true, ItemAmountInvoiced should equal sum of all JI_LinePrices converted to local currency.", 32.46d, provider2.ItemAmountInvoiced);
		}

		protected override InvoiceLineWrapper GetProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line1 = CreateInvoiceWithLinesAndCurrency(declaration, "USD", 1d, 2d);

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = line1.CusEntryLine as CusEntryLine;

			return InvoiceLineWrapper.New(entryLine);
		}

		JobComInvoiceLine CreateInvoiceWithLinesAndCurrency(JobDeclaration declaration, string currency, ZDecimal line1Price, ZDecimal line2Price)
		{
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = currency;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var line11 = invoiceHeader1.InvoiceLines.AddNew();
			line11.JI_LinePrice = line1Price;
			var line12 = invoiceHeader1.InvoiceLines.AddNew();
			line12.JI_LinePrice = line2Price;

			return line11;
		}
	}
}
