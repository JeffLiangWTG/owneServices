using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.GCG.DocWrappers.Testing
{
	[TestedType(typeof(DocGCGARInvoice))]
	public class DocGCGARInvoiceTestCase : DocumentWrapperTestCase
	{
		public void TestClientOverride()
		{
			AssertEquals("Client's DocInvoice Override", typeof(DocGCGARInvoice), DocARInvoice.New(Invoice, Factory).GetType());
		}

		public void TestCreditTerms()
		{
			Invoice.AH_TransactionType = TransactionTypes.Invoice;
			Invoice.AH_InvoiceTerm = "INV";
			AssertEquals("Terms should not be " + ImmediatePayment, true, InvoiceWrapper.CreditTerms != ImmediatePayment);
			Invoice.AH_InvoiceTerm = "COD";
			AssertEquals("Terms should be " + ImmediatePayment, true, InvoiceWrapper.CreditTerms == ImmediatePayment);
		}

		#region Implementation
		DocGCGARInvoice InvoiceWrapper;
		InvoicingBase Invoice;
		const string ImmediatePayment = "Immediate Payment";
		protected override void SetUp()
		{
			Invoice = Factory.New<ARInvoice>();
			InvoiceWrapper = DocGCGARInvoice.New(Invoice, Factory);
			base.SetUp();
			Factory.Save();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { InvoiceWrapper };
		}
		#endregion
	}
}
