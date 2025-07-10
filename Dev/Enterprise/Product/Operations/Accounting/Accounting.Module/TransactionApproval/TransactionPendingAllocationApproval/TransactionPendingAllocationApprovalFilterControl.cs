using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public partial class TransactionPendingAllocationApprovalFilterControl : ZFilterStripControl
	{
		public TransactionPendingAllocationApprovalFilterControl()
		{
			Initialize();
		}

		public TransactionPendingAllocationApprovalFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			Initialize();
		}

		void Initialize()
		{
			InitializeComponent();

			ManageColumns();
		}

		void ManageColumns()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.Value)
			{
				const string linkedTransaction = nameof(TransactionPendingAllocationApprovalRequest.LinkedTransaction);

				grid.RemoveFromAvailableColumns(
					$"{linkedTransaction}+{AutoAccTransactionHeader.Schema.AH_ComplianceSubType}",
					$"{linkedTransaction}+{AutoAccTransactionHeader.Schema.AH_OH}",
					$"{linkedTransaction}+{nameof(InvoicingBase.InvoiceDate)}",
					$"{linkedTransaction}+{nameof(TransactionHeader.EInvoicingStatus)}",
					$"{linkedTransaction}+{nameof(TransactionHeader.EInvoicingError)}"
				);
			}
		}
	}
}
