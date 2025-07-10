using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class AdvancedAvailabilityForm : ZChildForm
	{
		public static void ShowAdvancedAvailabilityForm(GlbStaff staff)
		{
			var loadedStaff = new BusinessObjectFactory { NameForDebugging = "AdvancedAvailabilityForm.ShowAdvancedAvailabilityForm" }.Load<GlbStaff>(staff.PK);
			var viewModel = new ResourceAvailabilityOverrideViewModel(loadedStaff);
			ZFormModaliser.ShowDialogAndDispose(new AdvancedAvailabilityForm(viewModel, loadedStaff));
		}

		public AdvancedAvailabilityForm(ResourceAvailabilityOverrideViewModel viewModel, GlbStaff staff)
			: base(viewModel)
		{
			this.viewModel = viewModel;

			viewModel.Closed += viewModel_Closed;
			SetLeaveGridVisibility(staff);
		}

		void SetLeaveGridVisibility(GlbStaff staff)
		{
			var canShowLeave = GlbStaffVisibilityHelper.CanShowLeave(staff);
			futureLeaveGrid.Visible = canShowLeave;
			leaveViewNotAllowedLabel.Visible = !canShowLeave;
		}

		void viewModel_Closed(object sender, EventArgs e)
		{
			Close();
		}

		readonly ResourceAvailabilityOverrideViewModel viewModel;

		void okButton_Click(object sender, EventArgs e)
		{
			if (!IsDisposed)
			{
				viewModel.Save();
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && (components != null))
				{
					components.Dispose();
				}
				viewModel.Closed -= viewModel_Closed;
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}
	}
}
