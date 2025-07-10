using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class CommissionAgreementApprovalFilterControl : ZFilterStripControl
	{
		public CommissionAgreementApprovalFilterControl(CommissionAgreementApprovalWizard commissionAgreementApprovalWizard, FilterStripBusinessObject filterStripBusinessObject)
			: base(commissionAgreementApprovalWizard.CommissionAgreementApprovalItemCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			SetMenuItemsVisibility();

			wizard = commissionAgreementApprovalWizard;
			filterStrip = filterStripBusinessObject;
		}

		readonly CommissionAgreementApprovalWizard wizard;
		readonly FilterStripBusinessObject filterStrip;

		public event EventHandler SearchPerformed;

		protected override void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			base.OnSearchPerformed(showError, didSearch, isManualSearch, form);

			var query = filterStrip.Filter.AddToFilter(new ZQuery(OrgCommissionAgreementSchema.CA0_LastApprovedDateUtc, null));
			wizard.CommissionAgreementApprovalItemCollection.Load(new OrgCommissionAgreementCollection(wizard.Factory, query));

			SearchPerformed?.Invoke(this, new EventArgs());
		}

		void SetMenuItemsVisibility()
		{
			FilteredGrid.ForceShowExportToExcelMenuItem = true;
			((IZDisplayGridInternals)FilteredGrid).ForceShowMassUpdateMenuItem = true;
		}
	}
}
