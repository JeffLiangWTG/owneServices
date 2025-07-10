namespace Enterprise.Client.JAS.Business.Invoicing.Testing
{
	internal class FinancialMessageExportHelperForTest : FinancialMessageExportHelper
	{
		public FinancialMessageExportHelperForTest(IJASInvoicingBase invoice) : base(invoice)
		{
		}

		public override void OnSaving()
		{
			OnSavingCalled = true;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			LastSaveSucceeded = saveSucceeded;
		}

		public bool? LastSaveSucceeded;
		public bool OnSavingCalled;
	}
}
