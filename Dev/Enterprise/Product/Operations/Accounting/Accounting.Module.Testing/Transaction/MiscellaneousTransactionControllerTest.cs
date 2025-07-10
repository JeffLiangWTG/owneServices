using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Module.Testing
{
	abstract class MiscellaneousTransactionControllerTest : AccountingTransactionControllerTest
	{
		#region TestCantShowDeleteFormForTransactionViewBizOs

		public void TestCantShowDeleteFormForTransactionViewBizOs()
		{
			AssertNull("Last shown form should be null as precondition", AccountingTransactionController.LastShownForm);

			AccountingTransactionController.ShowDeleteForm(ParentTransactionHeaderRow);

			AssertNull("Should still be null as form should not have been shown", AccountingTransactionController.LastShownForm);

			AssertEquals("Error message about why can't reverse", GetExpectedCantReverseMesaage,
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected virtual string GetExpectedCantReverseMesaage
		{
			get { return "Overpayments, Discounts and Exchange Differences can only be reversed by unmatching."; }
		}

		#endregion

		public void TestCantShowCopyFormForTransactionViewBizOs()
		{
			AssertNull("Last Shown form should be null", AccountingTransactionController.LastShownForm);

			AccountingTransactionController.ShowTemplateCopyForm(ParentTransactionHeaderRow);

			AssertNull("Should still be null as form should not be shown", AccountingTransactionController.LastShownForm);

			AssertEquals("Error message about why can't copy", "The copy function is only used for AR Invoice, AP Invoice, Bank Transfer, Direct Receipt and Direct Payment at this point",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override bool DeleteShouldShowForm
		{
			get { return false; }
		}

		protected new MiscellaneousTransactionController AccountingTransactionController
		{
			get { return (MiscellaneousTransactionController)Controller; }
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		protected override bool ShouldHaveReversedBizoForTest
		{
			get { return false; }
		}
	}
}
