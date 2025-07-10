namespace Enterprise.Customs.IL.GUI
{
	partial class CompanyCredentialsUserControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.CompanyCredentialsDetailsUserControl = new CompanyCredentialsDetailsUserControl();
			this.CompanyCredentialsDetailsUserControl.SuspendLayout();
			// 
			// CompanyCredentialsUserControl
			// 
			this.CompanyCredentialsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyCredentialsDetailsUserControl, ".");
			this.CompanyCredentialsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 76, true);
			this.CompanyCredentialsDetailsUserControl.Name = "CompanyCredentialsDetailsUserControl";

			this.Controls.Add(this.CompanyCredentialsDetailsUserControl);

			this.CompanyCredentialsDetailsUserControl.ResumeLayout(true);
			this.CompanyCredentialsDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal CompanyCredentialsDetailsUserControl CompanyCredentialsDetailsUserControl;
	}
}
