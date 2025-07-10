namespace Enterprise.Customs.BE.GUI
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
			this.UserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrentPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.BEGlbCompanyWrapper);
			// 
			// UserID
			// 
			this.BindingSource.SetBindingMember(this.UserIDTextBox, "Credential.GP_UserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.BEGlbCompanyWrapper)(null)).Credential.GP_UserID)));
			this.UserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.UserIDTextBox.Name = "UserIDTextBox";
			this.UserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.UserIDTextBox.TabIndex = 1;
			// 
			// CurrentPassword
			// 
			this.BindingSource.SetBindingMember(this.CurrentPasswordTextBox, "Credential.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.BEGlbCompanyWrapper)(null)).Credential.CurrentDecryptedPassword)));
			this.CurrentPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CurrentPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.CurrentPasswordTextBox.Name = "CurrentPasswordTextBox";
			this.CurrentPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CurrentPasswordTextBox.PasswordChar = '*';
			this.CurrentPasswordTextBox.TabIndex = 2;
			// 
			// CompanyCredentialsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.Controls.Add(this.CurrentPasswordTextBox);
			this.Controls.Add(this.UserIDTextBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 100, true);
			this.Name = "CompanyCredentialsUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 3, 3, 3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox CurrentPasswordTextBox;
		internal ZArchitecture.ZTextBox UserIDTextBox;
	}
}
