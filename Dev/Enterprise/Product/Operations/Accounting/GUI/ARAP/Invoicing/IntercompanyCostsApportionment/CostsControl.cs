using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	public partial class CostsControl : ZUserControl
	{
		public CostsControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				LineSummaryGrid.RemoveFromAvailableColumns("AL_SupplyType");
			}

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				TaxBranchGuidFindBox.Visible = false;
				LineSummaryGrid.RemoveFromAvailableColumns(IntercompanyCostsApportionmentInvoiceLine.Schema.TaxBranch);
				LineSummaryGrid.RemoveFromAvailableColumns(IntercompanyCostsApportionmentInvoiceLine.Schema.TaxBranchName);
			}
		}
	}
}
