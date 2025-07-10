#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class ARInvoice
	{
		public new void OnFactorySavingBeforeTransactionCore_ForTestOnly()
		{
			OnFactorySavingBeforeTransactionCore();
		}

		public void OnSavingCore_ForTestOnly()
		{
			OnSavingCore();
		}
	}
}

#endif
