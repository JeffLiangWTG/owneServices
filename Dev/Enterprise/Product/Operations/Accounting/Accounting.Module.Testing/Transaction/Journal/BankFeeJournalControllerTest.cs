namespace Enterprise.Accounting.Module.Testing
{
	abstract class BankFeeJournalControllerTest : MiscellaneousTransactionControllerTest
	{
		#region TestCantShowDeleteFormForTransactionViewBizOs

		protected override string GetExpectedCantReverseMesaage
		{
			get { return "This transaction cannot be reversed because it has been matched with other transactions."; }
		}

		#endregion

		protected override bool ShouldHaveReversedBizoForTest
		{
			get { return false; }
		}
	}
}
