using System;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class GlPeriodForm : ZChildForm
	{
		ZTemplateTabControl TabControl;
		ZTabPage DetailsTabPage;
		ZLogsTabPage LogsTabPage;
		protected ZPostOrCancelButton CloseButton;
		ZCheckBox ReopenForAdjustmentsCheckBox;
		ZCheckBox ReopenGeneralLedgerPeriodCheckBox;
		ZCheckBox ReopenSubLedgerPeriodCheckBox;
		ZPeriodEdit PeriodEdit;
		ZDateEdit EndDateEdit;
		ZDateEdit StartDateEdit;
		ZYearEdit YearEdit;
		System.ComponentModel.IContainer components;

		public GlPeriodForm(AccPeriodManagement period)
			: base(period)
		{
			this.period = period;
			Name = "GlPeriodForm";
			Text = Res.GetString("FEC51D24-9E1A-42c2-A22C-DEA32FACBAF9", "GL Period Form");
			SetReadOnlyIncludingChildren();
			PlugIns.Add(ControllerIDs.Audit);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void SetReadOnlyIncludingChildren()
		{
			period.ReadOnly = true;
		}

		protected AccPeriodManagement period;

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
	}
}

