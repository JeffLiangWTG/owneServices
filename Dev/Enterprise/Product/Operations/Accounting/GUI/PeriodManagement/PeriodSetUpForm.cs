using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class PeriodSetUpForm : ZChildForm
	{
		public PeriodSetUpForm(NewYearPeriodSettings newYearSettings) : base(newYearSettings)
		{
			this.NewYearSettings = newYearSettings;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (NewYearSettings.IsPeriodsSetBefore)
			{
				this.accountingYearBasedDropEdit.Visible = false;
			}
		}

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		#region Implementation

		protected NewYearPeriodSettings NewYearSettings;
		ZButton CloseButton;
		ZButton OKButton;
		ZDropEdit zDropEdit1;
		ZDropEdit zDropEdit2;
		ZDropEdit accountingYearBasedDropEdit;
		ZDateEdit zDateEdit2;
		ZDateEdit zDateEdit1;
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

		void OKButton_Click(object sender, EventArgs e)
		{
			NewYearSettings.RunPreSaveValidation();
			if (NewYearSettings.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("8f8674cb-557b-468c-8e59-f5e026ac6e55", "Please fix errors before proceeding."));
				return;
			}
			else
			{
				bool result = true;

				if (NewYearSettings.EndDate.Year < ZDateTime.Now.Year)
				{
					result = Globals.Message.Show(Res.GetString("345ae573-d73c-435b-bab7-f3ac1dfaa4e9", @"You are currently in year {0}. You are attempting to create periods for an accounting year that ends before today's date. If you continue new periods will be created for the {1} accounting year with the given dates.
{2} will create accounting periods for the previous accounting year automatically, because this is the first accounting year you are creating.

Please click Yes to continue or No to cancel and setup a different initial accounting year.", ZDateTime.Now.Year, NewYearSettings.EndDate.Year, Core.Constants.ProductName),
						Res.GetString("09b78bc0-6f2d-4c6f-a9d9-33ab0ff71edb", "Setup Initial Accounting Year"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
				}

				if (result)
				{
					this.DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		#endregion
	}
}

