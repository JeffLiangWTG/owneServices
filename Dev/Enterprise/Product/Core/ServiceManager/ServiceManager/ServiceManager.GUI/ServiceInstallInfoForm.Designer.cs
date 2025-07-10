using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.GUI
{
	partial class ServiceInstallInfoForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.UsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConfirmPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StartupTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManualRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AutomaticRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.LogOnGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StartupTypeGroupBox.SuspendLayout();
			this.LogOnGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 237, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.ServiceInstallInfo);
			// 
			// UsernameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UsernameTextBox, "Username");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.ServiceInstallInfo)(null)).Username)));
			this.UsernameTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|db32a2ee-9a7e-4201-a13d-98792b57a897", "Log on as");
			this.UsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 19, true);
			this.UsernameTextBox.Name = "UsernameTextBox";
			this.UsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.UsernameTextBox.TabIndex = 1;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.ServiceInstallInfo)(null)).Password)));
			this.PasswordTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|ab101748-71f2-4799-ac8e-85922f08e967", "Password");
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 49, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.PasswordTextBox.TabIndex = 4;
			this.PasswordTextBox.UseSystemPasswordChar = true;
			// 
			// ConfirmPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConfirmPasswordTextBox, "ConfirmPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ServiceManager.Business.ServiceInstallInfo)(null)).ConfirmPassword)));
			this.ConfirmPasswordTextBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|bba15109-5f3b-4608-88d8-6f3fc6414b5d", "Confirm password");
			this.ConfirmPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConfirmPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 81, true);
			this.ConfirmPasswordTextBox.Name = "ConfirmPasswordTextBox";
			this.ConfirmPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.ConfirmPasswordTextBox.TabIndex = 6;
			this.ConfirmPasswordTextBox.UseSystemPasswordChar = true;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|7f4708a9-a9b1-4038-8286-010ba9db7a87", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 187, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 27, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|c7db5fde-c6b3-435c-a541-6de1701cb3bf", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 187, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 27, true);
			this.CancelButtonX.TabIndex = 5;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			// 
			// StartupTypeGroupBox
			// 
			this.StartupTypeGroupBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|bdb342ce-e938-48f6-a6ef-29a7ea0c5a76", "Startup Type");
			this.StartupTypeGroupBox.Controls.Add(this.ManualRadioButton);
			this.StartupTypeGroupBox.Controls.Add(this.AutomaticRadioButton);
			this.StartupTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 12, true);
			this.StartupTypeGroupBox.Name = "StartupTypeGroupBox";
			this.StartupTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 70, true);
			this.StartupTypeGroupBox.TabIndex = 1;
			this.StartupTypeGroupBox.TabStop = false;
			// 
			// ManualRadioButton
			// 
			this.ManualRadioButton.AutoCheck = false;
			this.ManualRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManualRadioButton, "Manual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.ServiceInstallInfo)(null)).Manual)));
			this.ManualRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|9ace0e47-1d83-45f0-817c-e1bb42080c35", "Manual");
			this.ManualRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ManualRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 42, true);
			this.ManualRadioButton.Name = "ManualRadioButton";
			this.ManualRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.ManualRadioButton.TabIndex = 1;
			this.ManualRadioButton.TabStop = true;
			this.ManualRadioButton.UseVisualStyleBackColor = true;
			// 
			// AutomaticRadioButton
			// 
			this.AutomaticRadioButton.AutoCheck = false;
			this.AutomaticRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutomaticRadioButton, "Automatic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ServiceManager.Business.ServiceInstallInfo)(null)).Automatic)));
			this.AutomaticRadioButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|dff33ebb-e1d9-48a3-9d92-fcff5f20ba3a", "Automatic");
			this.AutomaticRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutomaticRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 19, true);
			this.AutomaticRadioButton.Name = "AutomaticRadioButton";
			this.AutomaticRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.AutomaticRadioButton.TabIndex = 0;
			this.AutomaticRadioButton.TabStop = true;
			this.AutomaticRadioButton.UseVisualStyleBackColor = true;
			// 
			// LogOnGroupBox
			// 
			this.LogOnGroupBox.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|538bd5e0-8171-42bf-af4c-825531285cd4", "Log On");
			this.LogOnGroupBox.Controls.Add(this.UsernameTextBox);
			this.LogOnGroupBox.Controls.Add(this.PasswordTextBox);
			this.LogOnGroupBox.Controls.Add(this.ConfirmPasswordTextBox);
			this.LogOnGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.LogOnGroupBox.Name = "LogOnGroupBox";
			this.LogOnGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 118, true);
			this.LogOnGroupBox.TabIndex = 0;
			this.LogOnGroupBox.TabStop = false;
			// 
			// ServiceInstallInfoForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("ServiceInstallInfoForm|ca19f001-3f1d-4abd-80e0-a231271faa64", "Service Host Install");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 261, true);
			this.Controls.Add(this.LogOnGroupBox);
			this.Controls.Add(this.StartupTypeGroupBox);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.ServiceManager.Business";
			this.DataSourceType = typeof(Enterprise.ServiceManager.Business.ServiceInstallInfo);
			this.DataSourceTypeName = "Enterprise.ServiceManager.Business.ServiceInstallInfo";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 300, true);
			this.Name = "ServiceInstallInfoForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.StartupTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.LogOnGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StartupTypeGroupBox.ResumeLayout(false);
			this.StartupTypeGroupBox.PerformLayout();
			this.LogOnGroupBox.ResumeLayout(false);
			this.LogOnGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox UsernameTextBox;
		private Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		private Enterprise.ZArchitecture.ZTextBox ConfirmPasswordTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelButtonX;
		private Enterprise.ZArchitecture.GUI.ZGroupBox StartupTypeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LogOnGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton AutomaticRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ManualRadioButton;
	}
}
