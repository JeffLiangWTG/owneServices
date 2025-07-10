using System.Windows.Forms;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ExtendLastFinancialYearForm : ZChildForm
	{
		public ExtendLastFinancialYearForm(ExtendLastFinancialYearSettings extendLastFinancialYearSettings, PeriodManager periodManager)
			 : base(extendLastFinancialYearSettings)
		{
			this.extendLastFinancialYearSettings = extendLastFinancialYearSettings;
			this.PeriodManager = periodManager;
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

		#region Implementation

		protected PeriodManager PeriodManager;
		protected ExtendLastFinancialYearSettings extendLastFinancialYearSettings;
		ZButton CloseButton;
		ZButton OKButton;
		ZDropEdit zPeriodFormatDropEdit;
		ZDateEdit zEndDateEdit;
		ZDateEdit zStartDateEdit;
		ZArchitecture.ZTextBox zFinancialYearTextBox;
		readonly System.ComponentModel.Container components;

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

		void OKButton_Click(object sender, System.EventArgs e)
		{
			extendLastFinancialYearSettings.RunPreSaveValidation();
			if (extendLastFinancialYearSettings.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("EA6C1527-A5AE-404D-BE61-A26038AD0439", "Please fix errors before proceeding."));
				return;
			}
			else
			{
				DialogResult result = Globals.Message.Show(Res.GetString("8E653410-2938-4FE7-B3F2-B8602F0D134D", "Are you sure you want to set new Financial Year Range based on specified dates for company [{0}]?", GlbCompany.CurrentCompany.GC_Name), Res.GetString("2EC4CA4A-EE91-46F2-ACB5-7FF13F204872", "Modify Financial Periods"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (result == DialogResult.Yes)
				{
					if (extendLastFinancialYearSettings.EndDate > extendLastFinancialYearSettings.LastPeriodEndDate)
					{
						PeriodManager.ExtendLastYearPeriod(extendLastFinancialYearSettings);
					}
					Close();
				}
			}
		}

		#endregion
	}
}

