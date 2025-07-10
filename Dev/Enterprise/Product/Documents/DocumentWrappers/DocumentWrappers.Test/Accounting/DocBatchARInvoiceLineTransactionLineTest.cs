using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARInvoiceLine))]
	sealed class DocBatchARInvoiceLineTransactionLineTest : DocARInvoiceLineTest
	{
		public void TestLineAmounts()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			ARCreditNote creditNote = Factory.New<ARCreditNote>();

			ARInvoiceLine invoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			invoiceLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_OSTaxAmount = 15m;
			ARCreditNoteLine creditNoteLine = (ARCreditNoteLine)creditNote.Lines.AddNew();
			creditNoteLine.AL_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			creditNoteLine.AL_OSExTaxAmount = 50m;
			creditNoteLine.AL_OSTaxAmount = 8m;

			DocBatchARInvoiceLineTransactionLine invoiceLineWrapper = DocBatchARInvoiceLineTransactionLine.New(invoiceLine, Factory);
			DocBatchARInvoiceLineTransactionLine creditNoteLineWrapper = DocBatchARInvoiceLineTransactionLine.New(creditNoteLine, Factory);

			AssertEquals(100m, invoiceLineWrapper.OSExTaxAmount);
			AssertEquals(15m, invoiceLineWrapper.OSTaxAmount);
			AssertEquals(115m, invoiceLineWrapper.OSAmount);
			AssertEquals(100m, invoiceLineWrapper.LineAmount);

			AssertEquals(-50m, creditNoteLineWrapper.OSExTaxAmount);
			AssertEquals(-8m, creditNoteLineWrapper.OSTaxAmount);
			AssertEquals(-58m, creditNoteLineWrapper.OSAmount);
			AssertEquals(-50m, creditNoteLineWrapper.LineAmount);
		}
	}
}
