using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationFetchStrategy : TransactionHeaderFetchStrategy
	{
		public TransactionPendingAllocationFetchStrategy(TransactionPendingAllocation businessObject) : base(businessObject)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var eInvoicingColumns = new string[] { "EInvoicingStatus", "EInvoicingError", "EInvoicingGovernmentAllocatedNumber" };

			if (columns.Any(c => eInvoicingColumns.Contains(c.ColumnName)))
			{
				Factory.AddFetchHint(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID, BusinessObject.PK);
			}
		}
	}
}
