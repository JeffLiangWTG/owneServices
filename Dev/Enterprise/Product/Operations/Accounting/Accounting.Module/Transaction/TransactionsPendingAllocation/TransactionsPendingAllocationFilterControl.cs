using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Transaction
{
	public partial class TransactionsPendingAllocationFilterControl : ZFilterStripControl
	{
		public TransactionsPendingAllocationFilterControl()
		{
		}

		public TransactionsPendingAllocationFilterControl(IBusinessObjectCollection gridCollection, TransactionsPendingAllocationFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveEReportingColumns();
		}

		void RemoveEReportingColumns()
		{
			var eInvoicingEnabled = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.Value;
			if (eInvoicingEnabled)
			{
				return;
			}

			grid.RemoveFromAvailableColumns(
				TransactionPendingAllocation.Schema.EInvoicingStatus,
				TransactionPendingAllocation.Schema.EInvoicingError,
				TransactionPendingAllocation.Schema.EInvoicingGovernmentAllocatedNumber);
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new TransactionModuleFilterStrip();
		}
	}
}
