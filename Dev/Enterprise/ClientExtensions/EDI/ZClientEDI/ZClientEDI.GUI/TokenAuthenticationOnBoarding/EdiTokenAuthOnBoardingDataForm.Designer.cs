using System.Windows.Forms;
using CargoWise.Types;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.GUI
{
    partial class EdiTokenAuthOnBoardingDataForm
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
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.IncidentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.RelatedIncidentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.LicenseEnterpriseFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.PullRequestGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.EnableTokenBasedAuthenticationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ActivationParametersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.VerificationUsernameTextbox = new Enterprise.ZArchitecture.ZTextBox();
            this.VerificationUsernameButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.VerificationUserPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.VerificationUserPasswordButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.OIDCServerDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.IdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SystemUniqueIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ValidTokenIssuerPrefixTextBox = new ZArchitecture.ZTextBox();
            this.ClaimMappingNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ClaimMappingIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.WinzorOnlyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.SettingsVerificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.VerifySettingsButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.VerificationResultTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ProcessMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
            this.StagingDeploymentMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
            this.CompletedMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
            this.CustomerTestCompletedMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
            this.RevertMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
            this.RestartProcessingMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
            this.EnvironmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TenantFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.StagingPrLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.StagingPrZLabel = new ZArchitecture.ZLabel();
            this.ProdPrLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.ProdPrZLabel = new ZArchitecture.ZLabel();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.SaveButtonUserControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zPanel1.SuspendLayout();
            this.IncidentGroupBox.SuspendLayout();
            this.RelatedIncidentFindBox.SuspendLayout();
            this.LicenseEnterpriseFindBox.SuspendLayout();
            this.PullRequestGroupBox.SuspendLayout();
            this.ActivationParametersGroupBox.SuspendLayout();
            this.OIDCServerDropEdit.SuspendLayout();
            this.ClaimMappingIdentifierDropEdit.SuspendLayout();
            this.StatusDropEdit.SuspendLayout();
            this.SettingsVerificationGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 552, true);
            // 
            // MainTabPage
            // 
            this.MainTabPage.Controls.Add(this.zPanel1);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 525, true);
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 515, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 515, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 552, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 24, true);
            // 
            // MainMenu
            // 
            this.MainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.ProcessMenuItem});
            // 
            // HelpMenuItem
            // 
            this.HelpMenuItem.Index = 4;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData);
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.IncidentGroupBox);
            this.zPanel1.Controls.Add(this.PullRequestGroupBox);
            this.zPanel1.Controls.Add(this.ActivationParametersGroupBox);
            this.zPanel1.Controls.Add(this.SettingsVerificationGroupBox);
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 525, true);
            this.zPanel1.TabIndex = 0;
            // 
            // IncidentGroupBox
            // 
            this.IncidentGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5437f994-c0a1-47b9-b3f9-59d1e1af6c1f", "Incident");
            this.IncidentGroupBox.Controls.Add(this.RelatedIncidentFindBox);
            this.IncidentGroupBox.Controls.Add(this.LicenseEnterpriseFindBox);
            this.IncidentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
            this.IncidentGroupBox.Name = "IncidentGroupBox";
            this.IncidentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 58, true);
            this.IncidentGroupBox.TabIndex = 0;
            this.IncidentGroupBox.TabStop = false;
            this.IncidentGroupBox.Text = "Incident";
            // 
            // RelatedIncidentFindBox
            // 
            this.RelatedIncidentFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RelatedIncidentFindBox, "TOD_IM");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_IM)));
            this.RelatedIncidentFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("75606122-d70c-4bb9-a275-609bc0a1c75e", "Related Incident");
            this.RelatedIncidentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 17, true);
            this.RelatedIncidentFindBox.Name = "RelatedIncidentFindBox";
            this.RelatedIncidentFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.RelatedIncidentFindBox.ParentType = null;
            this.RelatedIncidentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 20, true);
            this.RelatedIncidentFindBox.TabIndex = 0;
            // 
            // LicenseEnterpriseFindBox
            //
            this.LicenseEnterpriseFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LicenseEnterpriseFindBox, "TOD_LE");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_LE)));
            this.LicenseEnterpriseFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("85e27527-eda7-4b93-b39e-cc1cf8843fe3", "Enterprise Code");
            this.LicenseEnterpriseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 37, true);
            this.LicenseEnterpriseFindBox.Name = "LicenseEnterpriseFindBox";
            this.LicenseEnterpriseFindBox.PreBoundMaxLength = 3;
            this.LicenseEnterpriseFindBox.ShouldResize = true;
            this.LicenseEnterpriseFindBox.ShowDescriptionBox = false;
            this.LicenseEnterpriseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 20, true);
            this.LicenseEnterpriseFindBox.TabIndex = 0;
            // 
            // PullRequestGroupBox
            // 
            this.PullRequestGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9355da6e-f53e-4a26-960c-7e734a8f77b1", "Pull Request");
            this.PullRequestGroupBox.Controls.Add(this.StagingPrLinkLabel);
            this.PullRequestGroupBox.Controls.Add(this.StagingPrZLabel);
            this.PullRequestGroupBox.Controls.Add(this.ProdPrLinkLabel);
            this.PullRequestGroupBox.Controls.Add(this.ProdPrZLabel);
            this.PullRequestGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 68, true);
            this.PullRequestGroupBox.Name = "PullRequestGroupBox";
            this.PullRequestGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 58, true);
            this.PullRequestGroupBox.TabIndex = 1;
            this.PullRequestGroupBox.TabStop = false;
            this.PullRequestGroupBox.Text = "Pull Request";
            //
            //StagingPrZLabel
            //
            this.StagingPrZLabel.AutoSize = true;
            this.StagingPrZLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("F523FCDF-9266-42BC-9B9D-FABC79175049", "Staging Pull Request Link");
            this.StagingPrZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
            this.StagingPrZLabel.Name = "StagingPrZLabel";
            this.StagingPrZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 75, true);
            this.StagingPrZLabel.TabIndex = 18;
            //
            // StagingPrLinkLabel
            //
            this.StagingPrLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 12, true);
            this.StagingPrLinkLabel.Name = "StagingPrLinkLabel";
            this.StagingPrLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.StagingPrLinkLabel.TabIndex = 17;
            this.StagingPrLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.StagingPrLink_Clicked);
            //
            //ProdPrZLabel
            //
            this.ProdPrZLabel.AutoSize = true;
            this.ProdPrZLabel.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("F523FCDF-9266-42BC-9B9D-FABC79175049", "Prod Pull Request Link");
            this.ProdPrZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 35, true);
            this.ProdPrZLabel.Name = "ProdPrZLabel";
            this.ProdPrZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 75, true);
            this.ProdPrZLabel.TabIndex = 18;
            //
            // ProdPrLinkLabel
            //
            this.ProdPrLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 32, true);
            this.ProdPrLinkLabel.Name = "ProdPrLinkLabel";
            this.ProdPrLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.ProdPrLinkLabel.TabIndex = 17;
            this.ProdPrLinkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(this.ProdPrLink_Clicked);
            // 
            // ActivationParametersGroupBox
            // 
            this.ActivationParametersGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5e58c95b-aa72-45fd-b2a2-3d3d06cb125f", "Activation Parameters");
            this.ActivationParametersGroupBox.Controls.Add(this.VerificationUsernameTextbox);
            this.ActivationParametersGroupBox.Controls.Add(this.VerificationUsernameButton);
            this.ActivationParametersGroupBox.Controls.Add(this.VerificationUserPasswordTextBox);
            this.ActivationParametersGroupBox.Controls.Add(this.VerificationUserPasswordButton);
            this.ActivationParametersGroupBox.Controls.Add(this.OIDCServerDropEdit);
            this.ActivationParametersGroupBox.Controls.Add(this.IdentifierTextBox);
            this.ActivationParametersGroupBox.Controls.Add(this.SystemUniqueIdentifierTextBox);
            this.ActivationParametersGroupBox.Controls.Add(this.ValidTokenIssuerPrefixTextBox);
            this.ActivationParametersGroupBox.Controls.Add(this.ClaimMappingNameTextBox);
            this.ActivationParametersGroupBox.Controls.Add(this.ClaimMappingIdentifierDropEdit);
            this.ActivationParametersGroupBox.Controls.Add(this.StatusDropEdit);
            this.ActivationParametersGroupBox.Controls.Add(this.TenantFindBox);
            this.ActivationParametersGroupBox.Controls.Add(this.WinzorOnlyCheckBox);
            this.ActivationParametersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 135, true);
            this.ActivationParametersGroupBox.Name = "ActivationParametersGroupBox";
            this.ActivationParametersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 268, true);
            this.ActivationParametersGroupBox.TabIndex = 2;
            this.ActivationParametersGroupBox.TabStop = false;
            this.ActivationParametersGroupBox.Text = "Activation Parameters";
            // 
            // VerificationUsernameTextbox
            // 
            this.BindingSource.SetBindingMember(this.VerificationUsernameTextbox, "TOD_VerificationUsername");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_VerificationUsername)));
            this.VerificationUsernameTextbox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9cd1c3d7-3364-49ce-8f8f-3f2b68c6102e", "Test Username");
            this.VerificationUsernameTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.VerificationUsernameTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 19, true);
            this.VerificationUsernameTextbox.Name = "VerificationUsernameTextbox";
            this.VerificationUsernameTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
            this.VerificationUsernameTextbox.TabIndex = 0;
            // 
            // VerificationUsernameButton
            // 
            this.VerificationUsernameButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("03cdc086-6cbf-47bf-88e6-752d330b3110", "Copy");
            this.VerificationUsernameButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(557, 18, true);
            this.VerificationUsernameButton.Name = "VerificationUsernameButton";
            this.VerificationUsernameButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
            this.VerificationUsernameButton.TabIndex = 1;
            this.VerificationUsernameButton.ToolTipCaption = null;
            this.VerificationUsernameButton.Click += new System.EventHandler(this.VerificationUsernameButton_Click);
            // 
            // VerificationUserPasswordTextBox
            // 
            this.BindingSource.SetBindingMember(this.VerificationUserPasswordTextBox, "TOD_VerificationUserPassword");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_VerificationUserPassword)));
            this.VerificationUserPasswordTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("53aa6257-e1c0-4e67-8e58-0dca80f0b077", "Test Password");
            this.VerificationUserPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.VerificationUserPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 40, true);
            this.VerificationUserPasswordTextBox.Name = "VerificationUserPasswordTextBox";
            this.VerificationUserPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 20, true);
            this.VerificationUserPasswordTextBox.TabIndex = 2;
            this.VerificationUserPasswordTextBox.UseSystemPasswordChar = true;
            // 
            // VerificationUserPasswordButton
            // 
            this.VerificationUserPasswordButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("03cdc086-6cbf-47bf-88e6-752d330b3110", "Copy");
            this.VerificationUserPasswordButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(557, 39, true);
            this.VerificationUserPasswordButton.Name = "VerificationUserPasswordButton";
            this.VerificationUserPasswordButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
            this.VerificationUserPasswordButton.TabIndex = 3;
            this.VerificationUserPasswordButton.ToolTipCaption = null;
            this.VerificationUserPasswordButton.Click += new System.EventHandler(this.VerificationUserPasswordButton_Click);
            // 
            // OIDCServerDropEdit
            // 
            this.OIDCServerDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OIDCServerDropEdit, "TOD_OIDCServer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_OIDCServer)));
            this.OIDCServerDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("45d46354-6601-49f7-8bb6-c2f109c5e645", "OpenID Connect Server");
            this.OIDCServerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 61, true);
            this.OIDCServerDropEdit.Name = "OIDCServerDropEdit";
            this.OIDCServerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.OIDCServerDropEdit.TabIndex = 5;
            // 
            // IdentifierTextBox
            // 
            this.BindingSource.SetBindingMember(this.IdentifierTextBox, "TOD_ConfigurationIdentifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_ConfigurationIdentifier)));
            this.IdentifierTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("13bf2198-f6c3-4c07-a046-16d20d6d7b2a", "Identifier");
            this.IdentifierTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.IdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 82, true);
            this.IdentifierTextBox.Name = "IdentifierTextBox";
            this.IdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.IdentifierTextBox.TabIndex = 6;
            // 
            // SystemUniqueIdentifierTextBox
            // 
            this.BindingSource.SetBindingMember(this.SystemUniqueIdentifierTextBox, "TOD_SystemUniqueIdentifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_SystemUniqueIdentifier)));
            this.SystemUniqueIdentifierTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a388a295-d7b7-4c9c-b0db-2bbc312d1ed6", "System Unique Identifier");
            this.SystemUniqueIdentifierTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SystemUniqueIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 104, true);
            this.SystemUniqueIdentifierTextBox.Name = "SystemUniqueIdentifierTextBox";
            this.SystemUniqueIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.SystemUniqueIdentifierTextBox.TabIndex = 8;
            // 
            // ValidTokenIssuerPrefixTextBox
            // 
            this.BindingSource.SetBindingMember(this.ValidTokenIssuerPrefixTextBox, "TOD_ValidTokenIssuerPrefix");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_ValidTokenIssuerPrefix)));
            this.ValidTokenIssuerPrefixTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("221c363d-7196-46ad-85e7-a16c5e6c4e07", "Valid Token Issuer Prefix");
            this.ValidTokenIssuerPrefixTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ValidTokenIssuerPrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 126, true);
            this.ValidTokenIssuerPrefixTextBox.Name = "ValidTokenIssuerPrefixTextBox";
            this.ValidTokenIssuerPrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.ValidTokenIssuerPrefixTextBox.TabIndex = 9;
            // 
            // ClaimMappingNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.ClaimMappingNameTextBox, "TOD_ClaimMappingName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_ClaimMappingName)));
            this.ClaimMappingNameTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("81ea724d-32fb-4c5f-98f4-7ec151bcf98a", "Claim Mapping Name");
            this.ClaimMappingNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ClaimMappingNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 148, true);
            this.ClaimMappingNameTextBox.Name = "ClaimMappingNameTextBox";
            this.ClaimMappingNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.ClaimMappingNameTextBox.TabIndex = 10;
            // 
            // ClaimMappingIdentifierDropEdit
            // 
            this.ClaimMappingIdentifierDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ClaimMappingIdentifierDropEdit, "TOD_ClaimMappingIdentifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_ClaimMappingIdentifier)));
            this.ClaimMappingIdentifierDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9a710469-5801-4d77-88bc-b6b6410601b2", "Claim Mapping Identifier");
            this.ClaimMappingIdentifierDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ClaimMappingIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 170, true);
            this.ClaimMappingIdentifierDropEdit.Name = "ClaimMappingIdentifierDropEdit";
            this.ClaimMappingIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.ClaimMappingIdentifierDropEdit.TabIndex = 11;
            // 
            // TenantFindBox
            // 
            this.TenantFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TenantFindBox, "TOD_IDT");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_IDT)));
            this.TenantFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("836A10D0-8C49-411C-B90F-509F16DCC219", "Tenant");
            this.TenantFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 192, true);
            this.TenantFindBox.Name = "TenantFindBox";
            this.TenantFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TenantFindBox.ParentType = null;
            this.TenantFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.TenantFindBox.TabIndex = 13;
            this.TenantFindBox.CodeBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(228);
            // 
            // StatusDropEdit
            // 
            this.StatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatusDropEdit, "TOD_Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_Status)));
            this.StatusDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4dac794d-41c1-4ba7-9d85-1d44b4f74038", "Status");
            this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 214, true);
            this.StatusDropEdit.Name = "StatusDropEdit";
            this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.StatusDropEdit.TabIndex = 12;
            // 
            // WinzorOnlyCheckBox
            //
            this.WinzorOnlyCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.WinzorOnlyCheckBox, "TOD_WinzorOnly");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_WinzorOnly)));
            this.WinzorOnlyCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("96d934e6-36e6-4c25-91f5-b7220cfa5acd", "Winzor Only");
            this.WinzorOnlyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.WinzorOnlyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 236, true);
            this.WinzorOnlyCheckBox.Name = "WinzorOnlyCheckBox";
            this.WinzorOnlyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 20, true);
            this.WinzorOnlyCheckBox.TabIndex = 13;
            // 
            // SettingsVerificationGroupBox
            // 
            this.SettingsVerificationGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4a83f240-d92d-4543-93f1-7e352d318a16", "Azure B2C settings verification");
            this.SettingsVerificationGroupBox.Controls.Add(this.VerifySettingsButton);
            this.SettingsVerificationGroupBox.Controls.Add(this.VerificationResultTextBox);
            this.SettingsVerificationGroupBox.Controls.Add(this.EnvironmentDropEdit);
            this.SettingsVerificationGroupBox.Controls.Add(this.EnableTokenBasedAuthenticationCheckBox);
            this.SettingsVerificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 416, true);
            this.SettingsVerificationGroupBox.Name = "SettingsVerificationGroupBox";
            this.SettingsVerificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 80, true);
            this.SettingsVerificationGroupBox.TabIndex = 3;
            this.SettingsVerificationGroupBox.TabStop = false;
            this.SettingsVerificationGroupBox.Text = "Azure B2C settings verification";
            // 
            // VerifySettingsButton
            // 
            this.VerifySettingsButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c3310afa-c41f-419f-9b6a-f8b41f90146e", "Verify Settings");
            this.VerifySettingsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 19, true);
            this.VerifySettingsButton.Name = "VerifySettingsButton";
            this.VerifySettingsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 23, true);
            this.VerifySettingsButton.TabIndex = 0;
            this.VerifySettingsButton.ToolTipCaption = null;
            this.VerifySettingsButton.UseVisualStyleBackColor = true;
            this.VerifySettingsButton.Click += new System.EventHandler(this.VerifySettingsButton_Click);
            // 
            // VerificationResultTextBox
            // 
            this.VerificationResultTextBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VerificationResultTextBox, "VerificationResult");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).VerificationResult)));
            this.VerificationResultTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("fefa6496-9f19-4a68-a425-45daaad35c6f", "Verification Result");
            this.VerificationResultTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.VerificationResultTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 22, true);
            this.VerificationResultTextBox.Name = "VerificationResultTextBox";
            this.VerificationResultTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.VerificationResultTextBox.TabIndex = 1;
            this.VerificationResultTextBox.TabStop = false;
            //
            // EnvironmentDropEdit
            // 
            this.EnvironmentDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EnvironmentDropEdit, "Environment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).Environment)));
            this.EnvironmentDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("A650116D-2218-4E92-9571-93CBE72C7F21", "Environment");
            this.EnvironmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 22, true);
            this.EnvironmentDropEdit.Name = "EnvironmentDropEdit";
            this.EnvironmentDropEdit.PreBoundMaxLength = 3;
            this.EnvironmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(486, 20, true);
            this.EnvironmentDropEdit.TabIndex = 13;
            this.EnvironmentDropEdit.ShowDescriptionBox = false;
            // 
            // EnableTokenBasedAuthenticationCheckBox
            //
            this.EnableTokenBasedAuthenticationCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.EnableTokenBasedAuthenticationCheckBox, "TOD_Enabled");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData)(null)).TOD_Enabled)));
            this.EnableTokenBasedAuthenticationCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b7bf60f0-25c5-495f-ac62-5277959b4cae", "Allow customer to enable token-based authentication in their CW1 PROD environment");
            this.EnableTokenBasedAuthenticationCheckBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this.EnableTokenBasedAuthenticationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 45, true);
            this.EnableTokenBasedAuthenticationCheckBox.Name = "EnableTokenBasedAuthenticationCheckBox";
            this.EnableTokenBasedAuthenticationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
            this.EnableTokenBasedAuthenticationCheckBox.TabIndex = 3;
            this.EnableTokenBasedAuthenticationCheckBox.Enabled = false;
            this.EnableTokenBasedAuthenticationCheckBox.UseVisualStyleBackColor = true;
            // 
            // ProcessMenuItem
            // 
            this.ProcessMenuItem.Caption = null;
            this.ProcessMenuItem.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9bf7974b-4c6b-4eb9-8645-6366d9e56805", "Process");
            this.ProcessMenuItem.Index = 3;
            this.ProcessMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.StagingDeploymentMenuItem,
            this.CustomerTestCompletedMenuItem,
            this.CompletedMenuItem,
            this.RevertMenuItem,
            this.RestartProcessingMenuItem});
            this.ProcessMenuItem.Text = "Process";
            // 
            // StartProcessingMenuItem
            // 
            this.StagingDeploymentMenuItem.Caption = null;
            this.StagingDeploymentMenuItem.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("DFBE0C3A-B5BB-4B08-ADCE-48F438D5DB17", "Staging Deployment");
            this.StagingDeploymentMenuItem.Index = 0;
            this.StagingDeploymentMenuItem.Text = "Staging Deployment";
            this.StagingDeploymentMenuItem.Click += new System.EventHandler(this.StagingDeploymentMenuItem_Click);
            // 
            // CustomerTestCompletedMenuItem
            // 
            this.CustomerTestCompletedMenuItem.Caption = null;
            this.CustomerTestCompletedMenuItem.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("12DF9C17-F219-49F9-A2BD-D60437B21026", "Customer Test Completed");
            this.CustomerTestCompletedMenuItem.Index = 1;
            this.CustomerTestCompletedMenuItem.Text = "Customer Test Completed";
            this.CustomerTestCompletedMenuItem.Click += new System.EventHandler(this.CustomerTestCompletedMenuItemMenuItem_Click);
            // 
            // CompletedMenuItem
            // 
            this.CompletedMenuItem.Caption = null;
            this.CompletedMenuItem.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9009080C-3E37-4117-BD49-0A418B508ACD", "Completed");
            this.CompletedMenuItem.Index = 2;
            this.CompletedMenuItem.Text = "Completed";
            this.CompletedMenuItem.Click += new System.EventHandler(this.CompletedMenuItemMenuItem_Click);
            // 
            // RevertMenuItem
            // 
            this.RevertMenuItem.Caption = null;
            this.RevertMenuItem.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9b275a55-f65b-4e5f-a5fc-b56315832e2b", "Revert");
            this.RevertMenuItem.Index = 3;
            this.RevertMenuItem.Text = "Revert";
            this.RevertMenuItem.Click += new System.EventHandler(this.RevertMenuItem_Click);
            // 
            // RestartProcessingMenuItem
            // 
            this.RestartProcessingMenuItem.Caption = null;
            this.RestartProcessingMenuItem.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e754b1d7-036d-4ee1-984a-f1a9929bdffd", "Restart Processing");
            this.RestartProcessingMenuItem.Index = 4;
            this.RestartProcessingMenuItem.Text = "Restart Processing";
            this.RestartProcessingMenuItem.Click += new System.EventHandler(this.RestartProcessingMenuItem_Click);
            // 
            // EdiTokenAuthOnBoardingDataForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f79faea1-893b-4baa-9a96-7641732fc518", "Token Authentication Onboarding");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 608, true);
            this.DataSourceType = typeof(Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business.EdiTokenAuthOnBoardingData);
            this.Name = "EdiTokenAuthOnBoardingDataForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.SaveButtonUserControl.ResumeLayout(true);
            this.SaveButtonUserControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            this.IncidentGroupBox.ResumeLayout(false);
            this.IncidentGroupBox.PerformLayout();
            this.RelatedIncidentFindBox.ResumeLayout(true);
            this.RelatedIncidentFindBox.PerformLayout();
            this.LicenseEnterpriseFindBox.ResumeLayout(true);
            this.LicenseEnterpriseFindBox.PerformLayout();
            this.PullRequestGroupBox.ResumeLayout(false);
            this.PullRequestGroupBox.PerformLayout();
            this.ActivationParametersGroupBox.ResumeLayout(false);
            this.ActivationParametersGroupBox.PerformLayout();
            this.OIDCServerDropEdit.ResumeLayout(true);
            this.OIDCServerDropEdit.PerformLayout();
            this.ClaimMappingIdentifierDropEdit.ResumeLayout(true);
            this.ClaimMappingIdentifierDropEdit.PerformLayout();
            this.StatusDropEdit.ResumeLayout(true);
            this.StatusDropEdit.PerformLayout();
            this.SettingsVerificationGroupBox.ResumeLayout(false);
            this.SettingsVerificationGroupBox.PerformLayout();
            this.EnvironmentDropEdit.ResumeLayout(false);
            this.EnvironmentDropEdit.PerformLayout();
            this.TenantFindBox.ResumeLayout(false);
            this.TenantFindBox.PerformLayout();
            this.StagingPrLinkLabel.ResumeLayout(false);
            this.StagingPrLinkLabel.PerformLayout();
            this.StagingPrZLabel.ResumeLayout(false);
            this.StagingPrZLabel.PerformLayout();
            this.ProdPrLinkLabel.ResumeLayout(false);
            this.ProdPrLinkLabel.PerformLayout();
            this.ProdPrZLabel.ResumeLayout(false);
            this.ProdPrZLabel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        ZArchitecture.GUI.ZPanel zPanel1;
        ZArchitecture.GUI.ZGroupBox IncidentGroupBox;
        ZArchitecture.GUI.ZGuidFindBox RelatedIncidentFindBox;
        ZArchitecture.GUI.ZGuidFindBox LicenseEnterpriseFindBox;
        Enterprise.ZArchitecture.GUI.ZGroupBox PullRequestGroupBox;
        ZArchitecture.GUI.ZGroupBox ActivationParametersGroupBox;
        ZArchitecture.ZTextBox VerificationUsernameTextbox;
        Enterprise.ZArchitecture.GUI.ZButton VerificationUsernameButton;
        ZArchitecture.ZTextBox VerificationUserPasswordTextBox;
        Enterprise.ZArchitecture.GUI.ZButton VerificationUserPasswordButton;
        ZArchitecture.GUI.ZDropEdit OIDCServerDropEdit;
        ZArchitecture.ZTextBox IdentifierTextBox;
        ZArchitecture.ZTextBox SystemUniqueIdentifierTextBox;
        ZArchitecture.ZTextBox ValidTokenIssuerPrefixTextBox;
        internal ZArchitecture.ZTextBox ClaimMappingNameTextBox;
        internal ZArchitecture.GUI.ZDropEdit ClaimMappingIdentifierDropEdit;
        ZArchitecture.GUI.ZDropEdit StatusDropEdit;
        Enterprise.ZArchitecture.GUI.ZGroupBox SettingsVerificationGroupBox;
        Enterprise.ZArchitecture.GUI.ZButton VerifySettingsButton;
        Enterprise.ZArchitecture.ZTextBox VerificationResultTextBox;
        ZArchitecture.GUI.ZMenuItem ProcessMenuItem;
        ZArchitecture.GUI.ZMenuItem StagingDeploymentMenuItem;
        ZArchitecture.GUI.ZMenuItem CustomerTestCompletedMenuItem;
        ZArchitecture.GUI.ZMenuItem CompletedMenuItem;
        ZArchitecture.GUI.ZMenuItem RevertMenuItem;
        ZArchitecture.GUI.ZMenuItem RestartProcessingMenuItem;
        ZArchitecture.GUI.ZDropEdit EnvironmentDropEdit;
        ZArchitecture.GUI.ZGuidFindBox TenantFindBox;
        ZArchitecture.GUI.ZCheckBox EnableTokenBasedAuthenticationCheckBox;
        ZArchitecture.GUI.ZLinkLabel StagingPrLinkLabel;
        ZArchitecture.ZLabel StagingPrZLabel;
        ZArchitecture.GUI.ZLinkLabel ProdPrLinkLabel;
        ZArchitecture.ZLabel ProdPrZLabel;
        ZArchitecture.GUI.ZCheckBox WinzorOnlyCheckBox;
    }
}
