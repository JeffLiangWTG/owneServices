using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class BiReportCredentialControl: RegistryBusinessObjectTemplateZUserControl
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
			this.userNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.passwordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.domainTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.BiReportCredential);
			// 
			// userNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.userNameTextBox, "UserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BiReportCredential)(null)).UserName)));
			this.userNameTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("A269DBD3-A29F-4272-A70F-D3FB8AB7466A", "User Name");
			this.userNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.userNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 13, true);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 18, true);
			this.userNameTextBox.TabIndex = 0;
			// 
			// passwordTextBox
			// 
			this.BindingSource.SetBindingMember(this.passwordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BiReportCredential)(null)).Password)));
			this.passwordTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("33ED4CE3-E6E6-4B8B-85C1-3EA9CECCFAA2", "Password");
			this.passwordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.passwordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 35, true);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.PasswordChar = '*';
			this.passwordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 18, true);
			this.passwordTextBox.TabIndex = 1;
			// 
			// domainTextBox
			// 
			this.BindingSource.SetBindingMember(this.domainTextBox, "Domain");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BiReportCredential)(null)).Domain)));
			this.domainTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("64BA7F54-CE36-4F33-BD84-BB65B804F836", "Domain");
			this.domainTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.domainTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 56, true);
			this.domainTextBox.Name = "domainTextBox";
			this.domainTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 18, true);
			this.domainTextBox.TabIndex = 2;
			// 
			// BiReportCredentialControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.domainTextBox);
			this.Controls.Add(this.passwordTextBox);
			this.Controls.Add(this.userNameTextBox);
			this.Name = "BiReportCredentialControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox userNameTextBox;
		private ZArchitecture.ZTextBox passwordTextBox;
		private ZArchitecture.ZTextBox domainTextBox;

	}
}
