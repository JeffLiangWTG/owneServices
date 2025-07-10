using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class JobManagementFilterControlBase : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new JobManagementFilterStrip();
		}

		public JobManagementFilterControlBase()
		{
			InitializeComponent();
		}

		public JobManagementFilterControlBase(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			FilteredGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, AutoJobHeader.Schema.JH_GB_TaxBranch);
		}
	}
}
