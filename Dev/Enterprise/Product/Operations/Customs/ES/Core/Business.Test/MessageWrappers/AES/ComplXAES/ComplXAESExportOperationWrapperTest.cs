using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXAESExportOperationWrapperTest : WrapperHelperTest<ComplXAESExportOperationWrapper>
	{
		public void TestTotalAmount()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";

				invoiceLine.JI_LinePrice = 1.12m;
				AssertEquals("Expected filled TotalAmount with 1 invoice line", 1.12m, wrapper.TotalAmount);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 2.32m;

				var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge", true, mergeResult);
				entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Expected filled TotalAmount with 2 invoice lines", 3.44m, wrapper.TotalAmount);

				invoiceHeader.JZ_RX_NKInvoice_Currency = "000";
				AssertEquals("Expected 0 TotalAmount when currency is 000", ZDecimal.Zero, wrapper.TotalAmount);
			});
		}

		public void TestCurrency()
		{
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			AssertEquals("Expected filled Currency", "EUR", wrapper.Currency);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "ES00001";

			wrapper = new ComplXAESExportOperationWrapper(entryHeader);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		ComplXAESExportOperationWrapper wrapper;

		protected override ComplXAESExportOperationWrapper GetProvider() => wrapper;
	}
}
