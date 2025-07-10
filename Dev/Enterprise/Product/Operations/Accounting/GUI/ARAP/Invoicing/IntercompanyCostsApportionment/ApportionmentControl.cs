using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	public partial class ApportionmentControl : ZUserControl
	{
		public ApportionmentControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				apportionmentDetailsGrid.RemoveFromAvailableColumns(Business.ARAP.Invoicing.IntercompanyCostsApportionment.Schema.TaxBranch);
				apportionmentDetailsGrid.RemoveFromAvailableColumns(Business.ARAP.Invoicing.IntercompanyCostsApportionment.Schema.TaxBranchName);
			}
		}
	}
}
