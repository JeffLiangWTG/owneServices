using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationApprovalRequestFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public TransactionPendingAllocationApprovalRequestFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		TransactionPendingAllocationApprovalRequest ApprovalRequest => BusinessObject as TransactionPendingAllocationApprovalRequest;

		string LinkedTransaction => nameof(ApprovalRequest.LinkedTransaction);

		string[] LinkedTransactionColumns => new string[] {
				$"{nameof(TransactionPendingAllocationApprovalRequest.JobNumber)}",
				$"{LinkedTransaction}+{AutoAccTransactionHeader.Schema.AH_ComplianceSubType}",
				$"{LinkedTransaction}+{AutoAccTransactionHeader.Schema.AH_OH}",
				$"{LinkedTransaction}+{nameof(InvoicingBase.InvoiceDate)}",
				$"{LinkedTransaction}+{nameof(TransactionHeader.EInvoicingStatus)}",
				$"{LinkedTransaction}+{nameof(TransactionHeader.EInvoicingError)}"
			};

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(c => LinkedTransactionColumns.Contains(c.ColumnName)))
			{
				Factory.AddFetchHint(typeof(TransactionPendingAllocation), ApprovalRequest.XP_ParentID);
			}
		}
	}
}
