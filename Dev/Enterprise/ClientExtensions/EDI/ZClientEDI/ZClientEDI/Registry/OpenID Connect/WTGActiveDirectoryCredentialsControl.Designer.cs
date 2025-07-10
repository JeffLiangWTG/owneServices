using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class WTGActiveDirectoryCredentialsControl
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
		private void InitializeComponent()
		{
			this.EnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DomainNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OrganizationalUnitPathLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DomainUsernameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DomainPasswordLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DomainNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DomainUsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DomainPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrgUnitPathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ZClientEDI.Business.WTGActiveDirectoryCredentials);
			// 
			// EnabledCheckBox
			// 
			this.EnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnabledCheckBox, "IsEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ZClientEDI.Business.WTGActiveDirectoryCredentials)(null)).IsEnabled)));
			this.EnabledCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("6A89DCD7-6964-4893-BB64-317972EA5AE4", "Enabled");
			this.EnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnabledCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.EnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(34, 46, true);
			this.EnabledCheckBox.Name = "EnabledCheckBox";
			this.EnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 14, true);
			this.EnabledCheckBox.TabIndex = 1;
			this.EnabledCheckBox.UseVisualStyleBackColor = true;
			// 
			// DomainNameLabel
			// 
			this.DomainNameLabel.CaptionResourceString = ZClientEDI.Res.GetData("E1D9E8D3-83BF-4034-BF5A-6141AFFF74CF", "Domain Name:");
			this.DomainNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DomainNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 80, true);
			this.DomainNameLabel.Name = "DomainNameLabel";
			this.DomainNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 25, true);
			this.DomainNameLabel.TabIndex = 2;
			this.DomainNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// OrganizationalUnitPathLabel
			// 
			this.OrganizationalUnitPathLabel.CaptionResourceString = ZClientEDI.Res.GetData("604332B6-8A25-4253-96C8-3663A84AAB8A", "Organizational Unit Path:");
			this.OrganizationalUnitPathLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OrganizationalUnitPathLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 260, true);
			this.OrganizationalUnitPathLabel.Name = "OrganizationalUnitPathLabel";
			this.OrganizationalUnitPathLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 25, true);
			this.OrganizationalUnitPathLabel.TabIndex = 3;
			this.OrganizationalUnitPathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DomainUsernameLabel
			// 
			this.DomainUsernameLabel.CaptionResourceString = ZClientEDI.Res.GetData("FAE1A503-2D3E-4D86-918F-AD8386CBE00B", "Domain Username:");
			this.DomainUsernameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DomainUsernameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 140, true);
			this.DomainUsernameLabel.Name = "DomainUsernameLabel";
			this.DomainUsernameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 25, true);
			this.DomainUsernameLabel.TabIndex = 4;
			this.DomainUsernameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DomainPasswordLabel
			// 
			this.DomainPasswordLabel.CaptionResourceString = ZClientEDI.Res.GetData("8924F367-6933-4972-8254-A4763C6F2D75", "Domain Password:");
			this.DomainPasswordLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DomainPasswordLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 200, true);
			this.DomainPasswordLabel.Name = "DomainPasswordLabel";
			this.DomainPasswordLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 25, true);
			this.DomainPasswordLabel.TabIndex = 5;
			this.DomainPasswordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DomainNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DomainNameTextBox, "DomainName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.WTGActiveDirectoryCredentials)(null)).DomainName)));
			this.DomainNameTextBox.CaptionResourceString = ZClientEDI.Res.GetData("43867FDC-6010-4203-890B-05AA14A6B5FC", "Domain Name");
			this.DomainNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DomainNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 80, true);
			this.DomainNameTextBox.Name = "DomainNameTextBox";
			this.DomainNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.DomainNameTextBox.TabIndex = 6;
			// 
			// DomainUsernameTextBox
			// 
			this.BindingSource.SetBindingMember(this.DomainUsernameTextBox, "DomainUserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.WTGActiveDirectoryCredentials)(null)).DomainUserName)));
			this.DomainUsernameTextBox.CaptionResourceString = ZClientEDI.Res.GetData("7BC0B870-43A2-47CB-8A7A-10CCBDC11169", "Domain Username");
			this.DomainUsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DomainUsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 140, true);
			this.DomainUsernameTextBox.Name = "DomainUsernameTextBox";
			this.DomainUsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.DomainUsernameTextBox.TabIndex = 7;
			// 
			// DomainPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.DomainPasswordTextBox, "DomainUserPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.WTGActiveDirectoryCredentials)(null)).DomainUserPassword)));
			this.DomainPasswordTextBox.CaptionResourceString = ZClientEDI.Res.GetData("E5997863-961C-45F6-A492-88EFE89AD70F", "Domain Password");
			this.DomainPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DomainPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 200, true);
			this.DomainPasswordTextBox.Name = "DomainPasswordTextBox";
			this.DomainPasswordTextBox.PasswordChar = '*';
			this.DomainPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.DomainPasswordTextBox.TabIndex = 8;
			// 
			// OrgUnitPathTextBox
			// 
			this.BindingSource.SetBindingMember(this.OrgUnitPathTextBox, "OrganizationalUnitPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZClientEDI.Business.WTGActiveDirectoryCredentials)(null)).OrganizationalUnitPath)));
			this.OrgUnitPathTextBox.CaptionResourceString = ZClientEDI.Res.GetData("A7AFEED8-19C4-496A-9780-FCD82763FD1E", "Organizational Unit Path");
			this.OrgUnitPathTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OrgUnitPathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 260, true);
			this.OrgUnitPathTextBox.Name = "OrgUnitPathTextBox";
			this.OrgUnitPathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.OrgUnitPathTextBox.TabIndex = 9;
			// 
			// WTGActiveDirectoryCredentialsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgUnitPathTextBox);
			this.Controls.Add(this.DomainPasswordTextBox);
			this.Controls.Add(this.DomainUsernameTextBox);
			this.Controls.Add(this.DomainNameTextBox);
			this.Controls.Add(this.DomainPasswordLabel);
			this.Controls.Add(this.DomainUsernameLabel);
			this.Controls.Add(this.OrganizationalUnitPathLabel);
			this.Controls.Add(this.DomainNameLabel);
			this.Controls.Add(this.EnabledCheckBox);
			this.Name = "WTGActiveDirectoryCredentialsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZCheckBox EnabledCheckBox;
		private Enterprise.ZArchitecture.ZLabel DomainNameLabel;
		private Enterprise.ZArchitecture.ZLabel OrganizationalUnitPathLabel;
		private Enterprise.ZArchitecture.ZLabel DomainUsernameLabel;
		private Enterprise.ZArchitecture.ZLabel DomainPasswordLabel;
		private ZTextBox DomainNameTextBox;
		private ZTextBox DomainUsernameTextBox;
		private ZTextBox DomainPasswordTextBox;
		private ZTextBox OrgUnitPathTextBox;
	}
}
