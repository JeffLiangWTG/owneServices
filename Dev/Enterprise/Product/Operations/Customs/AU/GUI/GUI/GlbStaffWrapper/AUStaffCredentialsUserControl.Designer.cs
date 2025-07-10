namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class AUStaffCredentialsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BrokerGroupAU = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NUTCurrentPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BrokerGroupAU.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUGlbStaffWrapper);
			// 
			// BrokerGroupAU
			// 
			this.BrokerGroupAU.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("GlbStaffForm|E7834576-74C1-4764-AB50-9D63D792CAE9", "NEXDOCS Tokens");
			this.BrokerGroupAU.Controls.Add(this.NUTCurrentPasswordTextBox);
			this.BrokerGroupAU.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.BrokerGroupAU.Name = "BrokerGroupAU";
			this.BrokerGroupAU.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 55, true);
			this.BrokerGroupAU.TabIndex = 4;
			this.BrokerGroupAU.TabStop = false;
			// 
			// NUTCurrentPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.NUTCurrentPasswordTextBox, "NUTPassword.CurrentDecryptedPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUGlbStaffWrapper)(null)).NUTPassword.CurrentDecryptedPassword)));
			this.NUTCurrentPasswordTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("07A781E3-0FB1-419E-A10D-E2DAC973CD77", "NEXDOCS User Token");
			this.NUTCurrentPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NUTCurrentPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.NUTCurrentPasswordTextBox.Name = "NUTCurrentPasswordTextBox";
			this.NUTCurrentPasswordTextBox.PasswordChar = '*';
			this.NUTCurrentPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NUTCurrentPasswordTextBox.TabIndex = 0;
			// 
			// AUStaffCredentialsUserControl
			// 
			this.Controls.Add(this.BrokerGroupAU);
			this.Name = "AUStaffCredentialsUserControl";
			this.Controls.SetChildIndex(this.BrokerGroupAU, 0);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BrokerGroupAU.ResumeLayout(false);
			this.BrokerGroupAU.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		Enterprise.ZArchitecture.GUI.ZGroupBox BrokerGroupAU;
		Enterprise.ZArchitecture.ZTextBox NUTCurrentPasswordTextBox;

		#endregion
	}
}
