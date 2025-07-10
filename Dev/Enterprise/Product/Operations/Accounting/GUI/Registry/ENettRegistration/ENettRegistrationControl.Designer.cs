namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ENettRegistrationControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RegistrationCodeTextBox = new ZArchitecture.ZTextBox();
			this.AuthenticationCodeTextBox = new ZArchitecture.ZTextBox();
			this.AH_OHFindbox = new ZArchitecture.GUI.ZGuidFindBox();
			this.CustomHouseUsernameTextBox = new ZArchitecture.ZTextBox();
			this.CustomHousePasswordTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.EnettRegistrationCode);
			// 
			// RegistrationCodeTextBox
			// 
			this.RegistrationCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RegistrationCodeTextBox, "RegistrationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.EnettRegistrationCode)(null)).RegistrationCode)));
			this.RegistrationCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RegistrationCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegistrationControl|896a83c6-8d4b-4b51-af4f-b59a17d010a4", "ComPay Client Number");
			this.RegistrationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 27, true);
			this.RegistrationCodeTextBox.Name = "RegistrationCodeTextBox";
			this.RegistrationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.RegistrationCodeTextBox.TabIndex = 1;
			// 
			// AuthenticationCodeTextBox
			// 
			this.AuthenticationCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AuthenticationCodeTextBox, "AuthenticationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.EnettRegistrationCode)(null)).AuthenticationCode)));
			this.AuthenticationCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuthenticationCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegistrationControl|74e698cb-fa4c-4f3d-a0bd-16465b9ce4fd", "Integrator Auth. Code");
			this.AuthenticationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 51, true);
			this.AuthenticationCodeTextBox.Name = "AuthenticationCodeTextBox";
			this.AuthenticationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.AuthenticationCodeTextBox.TabIndex = 3;
			// 
			// AH_OHFindbox
			// 
			this.BindingSource.SetBindingMember(this.AH_OHFindbox, "OrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.EnettRegistrationCode)(null)).OrganisationPK)));
			this.AH_OHFindbox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegistrationControl|80be23b4-da8e-4491-af61-90403f97ddd8", "Organization");
			this.AH_OHFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 75, true);
			this.AH_OHFindbox.Name = "AH_OHFindbox";
			this.AH_OHFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.AH_OHFindbox.TabIndex = 5;
			// 
			// CustomHouseUsernameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomHouseUsernameTextBox, "CustomHouseUsername");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.EnettRegistrationCode)(null)).CustomHouseUsername)));
			this.CustomHouseUsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomHouseUsernameTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegistrationControl|6ebec0a0-e5cf-423e-8dd0-5b11799f82c1", "Custom House Username");
			this.CustomHouseUsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 101, true);
			this.CustomHouseUsernameTextBox.Name = "CustomHouseUsernameTextBox";
			this.CustomHouseUsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.CustomHouseUsernameTextBox.TabIndex = 7;
			// 
			// CustomHousePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomHousePasswordTextBox, "CustomHousePassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.EnettRegistrationCode)(null)).CustomHousePassword)));
			this.CustomHousePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomHousePasswordTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ENettRegistrationControl|1d62c9c5-2c06-4a87-992f-69353430ce04", "Custom House Password");
			this.CustomHousePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 127, true);
			this.CustomHousePasswordTextBox.Name = "CustomHousePasswordTextBox";
			this.CustomHousePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.CustomHousePasswordTextBox.TabIndex = 8;
			// 
			// ENettRegistrationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomHousePasswordTextBox);
			this.Controls.Add(this.CustomHouseUsernameTextBox);
			this.Controls.Add(this.AH_OHFindbox);
			this.Controls.Add(this.AuthenticationCodeTextBox);
			this.Controls.Add(this.RegistrationCodeTextBox);
			this.Name = "ENettRegistrationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 157, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.GUI.ZGuidFindBox AH_OHFindbox;
		private ZArchitecture.ZTextBox CustomHouseUsernameTextBox;
		private ZArchitecture.ZTextBox CustomHousePasswordTextBox;
		internal ZArchitecture.ZTextBox RegistrationCodeTextBox;
		internal ZArchitecture.ZTextBox AuthenticationCodeTextBox;
	}
}
