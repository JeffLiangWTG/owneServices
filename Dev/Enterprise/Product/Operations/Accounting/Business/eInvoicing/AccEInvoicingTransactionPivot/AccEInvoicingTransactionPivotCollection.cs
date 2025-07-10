using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccEInvoicingTransactionPivotCollection : DependentBusinessObjectCollection<AccEInvoicingTransactionPivot, AccEInvoicingBatch>
	{
		public AccEInvoicingTransactionPivotCollection(AccEInvoicingBatch master)
			: base(master, new ZQuery())
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
