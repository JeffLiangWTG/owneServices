using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class PeriodApportionmentGLJournalReversingTest : TestCaseWithFactory
	{
		public void TestSetCancellationFlag()
		{
			var originalJournal = Factory.NewWithValidTestData<GLJournal>();
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.GBP, 0.55M, testObjectCreator.AALSHI);
			var reversing = new PeriodApportionmentGLJournalReversing(originalJournal, invoice, Factory, true);
			reversing.Reverse();

			Assert("Original Journal should not be cancelled", !originalJournal.AH_IsCancelled);
			Assert("Reversing Journal should not be cancelled", !((GLJournal)reversing.ReverseTransaction).AH_IsCancelled);
		}

		public void TestSetTransactionBelongsToGroup()
		{
			var originalJournal = Factory.NewWithValidTestData<GLJournal>();
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "001001", testObjectCreator.GBP, 0.55M, testObjectCreator.AALSHI);
			var reversing = new PeriodApportionmentGLJournalReversing(originalJournal, invoice, Factory, true);
			reversing.Reverse();

			AssertEquals(invoice.PK, ((GLJournal)reversing.ReverseTransaction).AH_TransactionBelongsToGroup);
		}
	}
}