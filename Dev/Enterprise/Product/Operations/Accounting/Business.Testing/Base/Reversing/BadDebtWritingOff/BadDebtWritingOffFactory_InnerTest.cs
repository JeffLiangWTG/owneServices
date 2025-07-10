using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;

namespace Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff.Testing
{
	class BadDebtWritingOffFactory_InnerTest : TestCaseWithFactory
	{
		public void TestNewReversing()
		{
			ReversingBase returnedReversing = ReversingFactory.NewWritingOff((IBadDebtWritingOff)PayablesAndReceivablesTransaction);
			AssertEquals("Payables And Receivables Transaction should return payables and receivables writing off object",
				typeof(PayablesAndReceivablesWritingOff), returnedReversing.GetType());

			returnedReversing = ReversingFactory.NewWritingOff((IBadDebtWritingOff)Factory.New(typeof(ARInvoice)));
			AssertEquals("Invoicing Base transaction should return InvoicingBase writing off object",
				typeof(InvoicingBaseWritingOff), returnedReversing.GetType());
		}

		public void TestNullGivesNullResultOnNew()
		{
			ReversingBase returnedReversing = ReversingFactory.NewWritingOff(null);
			AssertNull("Passing null to new should return null", returnedReversing);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			PayablesAndReceivablesTransaction = new TestIBadDebtWritingOffTransaction();
			ReversingFactory = new BadDebtWritingOffFactory();
		}

		TestIPayablesAndReceivables PayablesAndReceivablesTransaction;
		BadDebtWritingOffFactory ReversingFactory;

		#endregion
	}
}
