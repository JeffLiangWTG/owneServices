namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class AdjustmentNoteValidationTest : InvoiceBaseValidationTest
	{
		public override void TestCheckTransactionNumForUAInvNumAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForUAInvNumAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestUAInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForAPInvNumAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForAPInvNumAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestAPInvoiceNumberDoesntCheckInvoiceNumbersForOtherCompanies()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Standard()
		{
			Assert(true);
		}

		public override void TestCheckTransactionNumForUAInvNumExcludingItselfAlreadyExist_Calendar()
		{
			Assert(true);
		}

		public override void TestJobInvoicingExist()
		{
			Assert(true);
		}
	}
}
