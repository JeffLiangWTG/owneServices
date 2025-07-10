using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public partial class APInvoiceApprovalFilterControl : ZFilterStripControl
	{
		public APInvoiceApprovalFilterControl()
		{
			InitializeComponent();
		}

		public APInvoiceApprovalFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveTaxBranchColumn();
		}

		void RemoveTaxBranchColumn()
		{
			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				RemoveColumnFromGrid(FilteredGrid, FilteredGrid.GetColumnStyle("TaxBranch"));
			}
		}

		void RemoveColumnFromGrid(ZDisplayGrid grid, ZGridColumnInfo columnInfo)
		{
			if (columnInfo != null)
			{
				grid.ColumnStyles.Remove(columnInfo);
			}
		}
	}
}

