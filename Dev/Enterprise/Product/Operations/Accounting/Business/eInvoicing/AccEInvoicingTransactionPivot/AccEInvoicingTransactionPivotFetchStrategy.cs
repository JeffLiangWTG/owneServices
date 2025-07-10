using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccEInvoicingTransactionPivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public AccEInvoicingTransactionPivotFetchStrategy(AccEInvoicingTransactionPivot businessObject) : base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(AccEInvoicingBatch), ((AccEInvoicingTransactionPivot)BusinessObject).AIP_AIB);
		}
	}
}
