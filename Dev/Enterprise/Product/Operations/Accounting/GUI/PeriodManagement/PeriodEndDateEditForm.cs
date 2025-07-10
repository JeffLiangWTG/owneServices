using System;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodEndDateEditForm : ZForm
	{
		ZArchitecture.ZGrid PeriodsGrid;
		ZPanel TopPanel;
		ZYearEdit FinancialYearEdit;
		ZPanel BottomPanel;
		Core.Forms.ZPostOrCancelButton CloseButton;
		Core.Forms.ZPostOrCancelButton OKButton;
		readonly System.ComponentModel.Container components;

		public PeriodEndDateEditForm(PeriodManager periodManager) : base(periodManager)
		{
			this.PeriodManager = periodManager;
			PeriodManager.PeriodEndDateFormEvent(true);
			ZFormPostingButtonsStrategy.SetupPosting(this, OKButton, CloseButton, null);

			ZFormMenuStrategy.SetMenuItemEnabled(this, ZFormMenuStrategy.FileNewMenuItemName, false);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		readonly PeriodManager PeriodManager;

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			PeriodManager.PeriodEndDateFormEvent(false);
		}
	}
}

