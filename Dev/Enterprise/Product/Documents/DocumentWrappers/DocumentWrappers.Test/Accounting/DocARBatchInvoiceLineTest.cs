using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARInvoice))]
	sealed class DocARBatchInvoiceLineTest : DocARInvoiceTest
	{
		public void TestAmounts()
		{
			ARCreditNote creditNote = Factory.New<ARCreditNote>();
			ARCreditNoteLine creditNoteLine = (ARCreditNoteLine)creditNote.Lines.AddNew();
			creditNoteLine.AL_OSExTaxAmount = 100m;
			creditNoteLine.AL_AT = GetRate();
			creditNoteLine.AL_OSTaxAmount = 10m;

			DocARBatchInvoiceLine creditNoteWrapper = DocARBatchInvoiceLine.New(creditNote, Factory);
			AssertEquals(-110m, creditNoteWrapper.OSTotal);
			AssertEquals(-10m, creditNoteWrapper.TotalOSTaxAmount);
			AssertEquals(-100m, creditNoteWrapper.InvoiceSubTotal);

			ARInvoice invoice = Factory.New<ARInvoice>();
			ARInvoiceLine invoiceLine = (ARInvoiceLine)invoice.Lines.AddNew();
			invoiceLine.AL_OSExTaxAmount = 100m;
			invoiceLine.AL_AT = GetRate();
			invoiceLine.AL_OSTaxAmount = 10m;

			DocARBatchInvoiceLine invoiceWrapper = DocARBatchInvoiceLine.New(invoice, Factory);
			AssertEquals(110m, invoiceWrapper.OSTotal);
			AssertEquals(10m, invoiceWrapper.TotalOSTaxAmount);
			AssertEquals(100m, invoiceWrapper.InvoiceSubTotal);
		}
	}
}
