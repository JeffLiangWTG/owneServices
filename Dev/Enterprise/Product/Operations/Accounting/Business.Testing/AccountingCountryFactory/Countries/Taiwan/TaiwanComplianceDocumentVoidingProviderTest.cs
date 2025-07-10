using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.AccountingCountryFactory.Testing
{
	public class TaiwanComplianceDocumentVoidingProviderTest : TestCaseWithFactory
	{
		public void TestTaiwanComplianceDocumentVoidingProvider_CanSpecialVoiding()
		{
			var complianceDocumentHeader = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			complianceDocumentHeader.ADH_TransactionType = TransactionTypes.Invoice;
			AssertEquals(true, ComplianceDocumentVoidingProvider.IsAllowedSpecialVoid(complianceDocumentHeader));

			complianceDocumentHeader.ADH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals(false, ComplianceDocumentVoidingProvider.IsAllowedSpecialVoid(complianceDocumentHeader));
		}

		public void TestTaiwanComplianceDocumentVoidingProvider_ShouldPreventVoidAmendingInvoiceWithCreditNote()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2011", testObjectCreator.TWD, 1m, 1, 1, 1, 0);
			Factory.Save();

			var relatedAmendingTransactionsCount = invoice?.GetRelatedAmendingTransactions().Count;
			AssertEquals(0, relatedAmendingTransactionsCount);
			AssertEquals("ShouldPreventVoidAmendingInvoiceWithCreditNote is false because invoice is not amending by credit notes.", false, ComplianceDocumentVoidingProvider.ShouldPreventVoidAmendingInvoiceWithCreditNote(new InvoicingBase[] { invoice }));

			var amendingCreditNote = testObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), testObjectCreator.ABIGAS, testObjectCreator.TWD, 1m);
			amendingCreditNote.AH_TransactionBelongsToGroup = invoice.PK;
			Factory.Save();

			relatedAmendingTransactionsCount = invoice?.GetRelatedAmendingTransactions().Count;
			AssertEquals(1, relatedAmendingTransactionsCount);
			AssertEquals(true, ComplianceDocumentVoidingProvider.ShouldPreventVoidAmendingInvoiceWithCreditNote(new InvoicingBase[] { invoice }));

			invoice.AH_TransactionType = TransactionTypes.CreditNote;
			AssertEquals("ShouldPreventVoidAmendingInvoiceWithCreditNote is false because transaction is not invoice.", false, ComplianceDocumentVoidingProvider.ShouldPreventVoidAmendingInvoiceWithCreditNote(new InvoicingBase[] { invoice }));
		}

		IComplianceDocumentVoidingProvider ComplianceDocumentVoidingProvider;

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ComplianceDocumentVoidingProvider = new TaiwanComplianceDocumentVoidingProvider();
		}

		#endregion
	}
}
