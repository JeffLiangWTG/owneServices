using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<JobComInvoiceLine, DocJobComInvoiceLine>
	{
		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(JobComInvoiceLine invoiceLine) => DocJobComInvoiceLine.New(invoiceLine, Factory);
		protected override string TestingCountry => Enterprise.Core.Constants.CountryCodes.Mexico;

		public void TestInvoiceLineCurrency()
		{
			InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("InvoiceCurrency", Core.Constants.CurrencyCodes.UnitedStates, InvoiceLineWrapper.DocInvoiceCurrencyCode);
		}

		public void TestInvoiceLineDescription()
		{
			InvoiceLine.JI_Description = "Goods Description Test";
			AssertEquals("JI_Description", InvoiceLine.JI_Description, InvoiceLineWrapper.DocGoodsDescription);
		}

		public void TestInvoiceLineQuantity()
		{
			InvoiceLine.JI_InvoiceQuantity = 120d;
			AssertEquals("JI_InvoiceQuantity", InvoiceLine.JI_InvoiceQuantity.ToString(2), InvoiceLineWrapper.DocInvoiceQuantity);
		}

		public void TestInvoiceLineUQ()
		{
			InvoiceLine.JI_InvoiceUQ = "KG";
			AssertEquals("JI_InvoiceUQ", InvoiceLine.JI_InvoiceUQ, InvoiceLineWrapper.DocInvoiceUQ);
		}

		public void TestInvoiceLinePrice()
		{
			InvoiceLine.JI_LinePrice = 200.00d;
			AssertEquals("JI_LinePrice", InvoiceLine.JI_LinePrice.ToString(2), InvoiceLineWrapper.DocInvoiceLinePrice);
		}

		public void TestInvoiceUnitPrice()
		{
			InvoiceLine.UnitPrice = 400.00d;
			AssertEquals("UnitPrice", InvoiceLine.UnitPrice.ToString(2), InvoiceLineWrapper.DocInvoiceUnitPrice);
		}
	}
}
