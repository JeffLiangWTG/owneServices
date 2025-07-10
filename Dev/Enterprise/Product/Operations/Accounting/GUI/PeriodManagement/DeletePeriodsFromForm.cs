using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class DeletePeriodsFromForm : ZChildForm
	{
		public DeletePeriodsFromForm(DeletePeriodsFromSetting deletePeriodsRangeSetting, PeriodManager periodManager) : base(deletePeriodsRangeSetting)
		{
			this.deletePeriodsRangeSetting = deletePeriodsRangeSetting;
			this.periodManager = periodManager;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		readonly DeletePeriodsFromSetting deletePeriodsRangeSetting;
		readonly PeriodManager periodManager;
		ZButton CloseButton;
		ZButton OKButton;
		ZDateEdit zStartDateEdit;

		readonly System.ComponentModel.IContainer components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}

