namespace Enterprise.Customs.GB.GUI
{
	partial class CompanyCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CredentialsDetailsUserControl = new CredentialsDetailsUserControl();
			this.CredentialsDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// CredentialsDetailsUserControl
			// 
			this.CredentialsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CredentialsDetailsUserControl, ".");
			this.CredentialsDetailsUserControl.Name = "CredentialsDetailsUserControl";

			this.Controls.Add(this.CredentialsDetailsUserControl);

			this.CredentialsDetailsUserControl.ResumeLayout(true);
			this.CredentialsDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal CredentialsDetailsUserControl CredentialsDetailsUserControl;
	}
}
