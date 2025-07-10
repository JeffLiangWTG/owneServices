using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415DeclarationTypeValuationInformationProviderTest : DataProviderTestCase<IM413AndIM415DeclarationTypeValuationInformationProvider>
	{
		public void TestInvoiceCurrency()
		{
			SetUpTestData();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine.JI_LinePrice = 150m;
			AssertEquals("InvoiceCurrency", Core.Constants.CurrencyCodes.Australia, Provider.InvoiceCurrency);
		}

		public void TestInvoiceAmount()
		{
			SetUpTestData();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine.JI_LinePrice = 150m;
			AssertEquals("InvoiceAmount", 150m, Provider.InvoiceAmount);
		}

		public void TestExchangeRate()
		{
			SetUpTestData();
			AssertEquals("ExchangeRate", 0m, Provider.ExchangeRate);
		}

		protected override IM413AndIM415DeclarationTypeValuationInformationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415DeclarationTypeValuationInformationProvider(entryHeader);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
