using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class ServerUsernamePasswordConfigurationControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.RegionalSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ConfirmPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RegionalSettingsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ServerUsernamePasswordConfiguration);
			// 
			// RegionalSettingsGroupBox
			// 
			this.RegionalSettingsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RegionalSettingsGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ServerUsernamePasswordConfigurationControl|0e36aca1-c764-4626-a77f-f09c0328b8b3", "Configuration Settings");
			this.RegionalSettingsGroupBox.Controls.Add(this.ConfirmPasswordTextBox);
			this.RegionalSettingsGroupBox.Controls.Add(this.PasswordTextBox);
			this.RegionalSettingsGroupBox.Controls.Add(this.UserNameTextBox);
			this.RegionalSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 4, true);
			this.RegionalSettingsGroupBox.Name = "RegionalSettingsGroupBox";
			this.RegionalSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 108, true);
			this.RegionalSettingsGroupBox.TabIndex = 3;
			this.RegionalSettingsGroupBox.TabStop = false;
			// 
			// ConfirmPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConfirmPasswordTextBox, "ConfirmPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ServerUsernamePasswordConfiguration)(null)).ConfirmPassword)));
			this.ConfirmPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConfirmPasswordTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ServerUsernamePasswordConfigurationControl|444b8e1b-b4fa-46ed-a37b-2365b89a98c4", "Confirm Password");
			this.ConfirmPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 72, true);
			this.ConfirmPasswordTextBox.Name = "ConfirmPasswordTextBox";
			this.ConfirmPasswordTextBox.PasswordChar = '*';
			this.ConfirmPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ConfirmPasswordTextBox.TabIndex = 13;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ServerUsernamePasswordConfiguration)(null)).Password)));
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ServerUsernamePasswordConfigurationControl|1fb41f3d-9e18-407d-9dbf-886a4203aeb5", "Password");
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 47, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PasswordTextBox.TabIndex = 11;
			// 
			// UserNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserNameTextBox, "UserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ServerUsernamePasswordConfiguration)(null)).UserName)));
			this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserNameTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ServerUsernamePasswordConfigurationControl|8d7bdd5e-0242-4c0a-ae36-b1627e74949a", "User name");
			this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 22, true);
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.UserNameTextBox.TabIndex = 9;
			// 
			// ServerUsernamePasswordConfigurationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RegionalSettingsGroupBox);
			this.Name = "ServerUsernamePasswordConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 116, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RegionalSettingsGroupBox.ResumeLayout(false);
			this.RegionalSettingsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox RegionalSettingsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox UserNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		private Enterprise.ZArchitecture.ZTextBox ConfirmPasswordTextBox;
	}
}
