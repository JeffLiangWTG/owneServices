using Enterprise.Client.EDI.IdentityTenant.Business;

namespace Enterprise.Client.EDI.IdentityTenant.GUI
{
	partial class EdiIdentityTenantForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TenantGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TenantIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OidcClientIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TenantNameTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorityUrlTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.GraphClientIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OnboardingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 552, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 525, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 515, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 552, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(941, 24, true);
			// 
			// HelpMenuItem
			// 
			this.HelpMenuItem.Index = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EdiIdentityTenant);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.TenantGroupBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1055, 525, true);
			this.zPanel1.TabIndex = 0;
			// 
			// TenantGroupBox
			//
			this.TenantGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("944C338B-D3B9-430A-95BB-88F0A1CFB0BD", "Tenant");
			this.TenantGroupBox.Controls.Add(this.TenantNameTextbox);
			this.TenantGroupBox.Controls.Add(this.TenantIdTextBox);
			this.TenantGroupBox.Controls.Add(this.OidcClientIdTextBox);
			this.TenantGroupBox.Controls.Add(this.AuthorityUrlTextbox);
			this.TenantGroupBox.Controls.Add(this.GraphClientIdTextBox);
			this.TenantGroupBox.Controls.Add(this.OnboardingCheckBox);
			this.TenantGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.TenantGroupBox.Name = "TenantGroupBox";
			this.TenantGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 205, true);
			this.TenantGroupBox.TabIndex = 0;
			this.TenantGroupBox.TabStop = false;
			this.TenantGroupBox.Text = "Tenant";
			// 
			// TenantIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.TenantIdTextBox, "IDT_TenantId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityTenant)(null)).IDT_TenantId)));
			this.TenantIdTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("89FA0F41-D3DD-4A0A-AC62-862D10852E8C", "Tenant ID");
			this.TenantIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TenantIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 19, true);
			this.TenantIdTextBox.Name = "TenantIdTextBox";
			this.TenantIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 20, true);
			this.TenantIdTextBox.TabIndex = 1;
			// 
			// ClientIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.OidcClientIdTextBox, "IDT_OidcClientId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityTenant)(null)).IDT_OidcClientId)));
			this.OidcClientIdTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0B02289B-A2CB-470E-96A4-521E82CE434D", "OIDC Client ID");
			this.OidcClientIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OidcClientIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 49, true);
			this.OidcClientIdTextBox.Name = "OidcClientIdTextBox";
			this.OidcClientIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 20, true);
			this.OidcClientIdTextBox.TabIndex = 2;
			// 
			// TenantNameTextbox
			// 
			this.BindingSource.SetBindingMember(this.TenantNameTextbox, "IDT_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityTenant)(null)).IDT_Name)));
			this.TenantNameTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4B187A11-F48F-4DEC-A9F5-B51779557969", "Name");
			this.TenantNameTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TenantNameTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 79, true);
			this.TenantNameTextbox.Name = "TenantNameTextbox";
			this.TenantNameTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 20, true);
			this.TenantNameTextbox.TabIndex = 3;
			// 
			// AuthorityUrlTextbox
			// 
			this.BindingSource.SetBindingMember(this.AuthorityUrlTextbox, "IDT_AuthorityUrl");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityTenant)(null)).IDT_AuthorityUrl)));
			this.AuthorityUrlTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("136E19DB-F5D4-4C06-B526-5B834197E4DD", "Authority URL");
			this.AuthorityUrlTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuthorityUrlTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 109, true);
			this.AuthorityUrlTextbox.Name = "AuthorityUrlTextbox";
			this.AuthorityUrlTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 20, true);
			this.AuthorityUrlTextbox.TabIndex = 4;
			// 
			// GraphClientIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.GraphClientIdTextBox, "IDT_GraphClientId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityTenant)(null)).IDT_GraphClientId)));
			this.GraphClientIdTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3C3F85EF-C4D0-4DAA-8770-45DB2169B5D4", "Graph Client Id");
			this.GraphClientIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GraphClientIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 139, true);
			this.GraphClientIdTextBox.Name = "GraphClientIdTextBox";
			this.GraphClientIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 20, true);
			this.GraphClientIdTextBox.TabIndex = 5;
			// 
			// OnboardingCheckBox
			//
			this.OnboardingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OnboardingCheckBox, "IDT_Onboarding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EdiIdentityTenant)(null)).IDT_Onboarding)));
			this.OnboardingCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("A5A1D4D1-9D3E-494C-B28F-640CF09ADAF9", "Onboarding");
			this.OnboardingCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this.OnboardingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 169, true);
			this.OnboardingCheckBox.Name = "OnboardingCheckBox";
			this.OnboardingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.OnboardingCheckBox.TabIndex = 6;
			this.OnboardingCheckBox.UseVisualStyleBackColor = true;
			// 
			// EdiIdentityTenantForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("12A8F5AC-B249-4308-B55A-038CD22C1E57", "Identity Tenant");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 608, true);
			this.DataSourceType = typeof(EdiIdentityTenant);
			this.Name = "EdiIdentityTenantForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "EdiIdentityTenantForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.TenantGroupBox.ResumeLayout(false);
			this.TenantGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		ZArchitecture.GUI.ZPanel zPanel1;
		ZArchitecture.GUI.ZGroupBox TenantGroupBox;
		ZArchitecture.ZTextBox TenantIdTextBox;
		ZArchitecture.ZTextBox OidcClientIdTextBox;
		ZArchitecture.ZTextBox TenantNameTextbox;
		ZArchitecture.ZTextBox AuthorityUrlTextbox;
		ZArchitecture.ZTextBox GraphClientIdTextBox;
		internal ZArchitecture.GUI.ZCheckBox OnboardingCheckBox;
	}
}
