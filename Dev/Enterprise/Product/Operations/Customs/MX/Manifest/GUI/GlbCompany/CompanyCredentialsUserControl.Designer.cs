namespace Enterprise.Customs.MX.Manifest.GUI
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
            this.CredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.Status = new Enterprise.ZArchitecture.ZTextBox();
            this.UserID = new Enterprise.ZArchitecture.ZTextBox();
            this.UserPassword = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CredentialsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Manifest.Business.GlbCompanyWrapper);
			// 
			// zGroupBox1
			// 
			this.CredentialsGroupBox.CaptionResourceString = Enterprise.Customs.MX.Manifest.GUI.Res.GetData("GlbCompanyForm|2BBFB633-2A1C-437B-BA82-9D115475FE7F", "Company Credentials");
            this.CredentialsGroupBox.Controls.Add(this.Status);
            this.CredentialsGroupBox.Controls.Add(this.UserID);
            this.CredentialsGroupBox.Controls.Add(this.UserPassword);
            this.CredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 26, true);
            this.CredentialsGroupBox.Name = "CredentialsGroupBox";
            this.CredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 147, true);
            this.CredentialsGroupBox.TabIndex = 0;
            this.CredentialsGroupBox.TabStop = false;
			// 
			// UserID
			// 
			this.BindingSource.SetBindingMember(this.UserID, "GlbExternalPassword.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_UserID)));
			this.UserID.CaptionResourceString = Enterprise.Customs.MX.Manifest.GUI.Res.GetData("968F95E2-4219-4AD3-AACA-D0368308B2C3", "Username");
			this.UserID.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserID.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 46, true);
			this.UserID.Name = "UserID";
			this.UserID.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.UserID.TabIndex = 1;
			// 
			// UserPassword
			// 
			this.BindingSource.SetBindingMember(this.UserPassword, "GlbExternalPassword.GP_CurrentPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.GP_CurrentPassword)));
			this.UserPassword.CaptionResourceString = Enterprise.Customs.MX.Manifest.GUI.Res.GetData("4F4BD3CD-5019-4714-B2F2-2F2BB6BB2154", "User Password");
			this.UserPassword.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 72, true);
			this.UserPassword.Name = "UserPassword";
			this.UserPassword.PasswordChar = '*';
			this.UserPassword.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.UserPassword.TabIndex = 2;
			this.UserPassword.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// Status
			// 
			this.BindingSource.SetBindingMember(this.Status, "GlbExternalPassword.PasswordStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.GlbCompanyWrapper)(null)).GlbExternalPassword.PasswordStatus)));
            this.Status.CaptionResourceString = Enterprise.Customs.MX.Manifest.GUI.Res.GetData("52E0BCD7-887B-453B-926F-92D94F106405", "Status");
            this.Status.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.Status.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 100, true);
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
            this.Status.TabIndex = 3;
			// 
			// CompanyCredentialsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.CredentialsGroupBox);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
            this.Name = "CompanyCredentialsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 413, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CredentialsGroupBox.ResumeLayout(false);
            this.CredentialsGroupBox.PerformLayout();
			this.CaptionRenderingEnabled = true;
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox CredentialsGroupBox;
		private ZArchitecture.ZTextBox UserPassword;
		private ZArchitecture.ZTextBox UserID;
		private ZArchitecture.ZTextBox Status;
	}
}
