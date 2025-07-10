using System.Windows.Forms;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	partial class SystemUserAccountsWizardForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainTopPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.mainBottomPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.adicionalLicenceDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.webSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.adicionalLicenceDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.linkedContactGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.webSecurityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.contactDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WebAccessContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ContactGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EnterpriseIdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseIdValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserIDValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserNameValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserEmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserEmailValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactEmailLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactEmailValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UserStatusValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SystemIdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SystemIdValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseCodeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ServerCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ServerCodeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DatabaseNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DatabaseNumberValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TenantIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TenantIDValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SaveToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.EdiCustomerUserAccountBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ContactOrganisationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ContactOrganisationValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WebAccessEnabledLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WebAccessEnabledCheckBox= new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PasswordSetLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PasswordSetCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendPasswordInstructionsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InstructionsLastSentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.InstructionsLastSentValueLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.ContactGroupBox.SuspendLayout();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.adicionalLicenceDetailsGroupBox.SuspendLayout();
			this.linkedContactGroupBox.SuspendLayout();
			this.webSecurityGroupBox.SuspendLayout();
			this.WebAccessContactGuidFindBox.SuspendLayout();
			this.mainTopPanel1.SuspendLayout();
			this.mainBottomPanel2.SuspendLayout();
			this.adicionalLicenceDetailsPanel.SuspendLayout();
			this.webSecurityPanel.SuspendLayout();
			this.contactDetailsPanel.SuspendLayout();
			this.SaveToolStrip.SuspendLayout();
			this.EdiCustomerUserAccountBottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 376, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 10, true);
			this.MainStatusBar.Text = "";
			this.MainStatusBar.Visible = false;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("36948e51-dea1-44dd-858d-3df3e6f53997", "Associate Customer User Account to Contact");
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 800, true);
			this.MainGroupBox.TabIndex = 1;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.Dock = DockStyle.Fill;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.MainSplitContainer.TabIndex = 4;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(680);
			// 
			// EdiCustomerUserAccountBottomPanel
			// 
			this.EdiCustomerUserAccountBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EdiCustomerUserAccountBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EdiCustomerUserAccountBottomPanel.Name = "EdiCustomerUserAccountBottomPanel";
			this.EdiCustomerUserAccountBottomPanel.TabIndex = 1;
			this.EdiCustomerUserAccountBottomPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 3, true);
			this.MainSplitContainer.Panel1.Controls.Add(EdiCustomerUserAccountBottomPanel);
			// 
			// UserIDLabel
			// 
			this.UserIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 25, true);
			this.UserIDLabel.Name = "UserIDLabel";
			this.UserIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.UserIDLabel.TabIndex = 0;
			this.UserIDLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("9f4d2314-91ad-4850-afe6-ee05367e36ff", "System User ID");
			this.UserIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.UserIDLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UserIDValueLabel
			// 
			this.UserIDValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 25, true);
			this.UserIDValueLabel.Name = "UserIDValueLabel";
			this.UserIDValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 14, true);
			this.UserIDValueLabel.TabIndex = 0;
			this.UserIDValueLabel.Text = "";
			this.UserIDValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// UserNameLabel
			// 
			this.UserNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 41, true);
			this.UserNameLabel.Name = "UserNameLabel";
			this.UserNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.UserNameLabel.TabIndex = 0;
			this.UserNameLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("67f78c83-a988-4e66-9b9b-dd0aec629244", "User Full Name");
			this.UserNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.UserNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UserNameValueLabel
			// 
			this.UserNameValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 41, true);
			this.UserNameValueLabel.Name = "UserNameValueLabel";
			this.UserNameValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 14, true);
			this.UserNameValueLabel.TabIndex = 0;
			this.UserNameValueLabel.Text = "";
			this.UserNameValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// UserEmailLabel
			// 
			this.UserEmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 57, true);
			this.UserEmailLabel.Name = "UserEmailLabel";
			this.UserEmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 14, true);
			this.UserEmailLabel.TabIndex = 0;
			this.UserEmailLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("b3732146-c53b-4270-ae29-1bb8921c5e6e", "User Email");
			this.UserEmailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.UserEmailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UserEmailValueLabel
			// 
			this.UserEmailValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 57, true);
			this.UserEmailValueLabel.Name = "UserEmailValueLabel";
			this.UserEmailValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 14, true);
			this.UserEmailValueLabel.TabIndex = 0;
			this.UserEmailValueLabel.Text = "";
			this.UserEmailValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// UserStatusLabel
			// 
			this.UserStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 73, true);
			this.UserStatusLabel.Name = "UserStatusLabel";
			this.UserStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 14, true);
			this.UserStatusLabel.TabIndex = 0;
			this.UserStatusLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("f84cef4c-31ad-4614-af34-d55308485da4", "Verification Status");
			this.UserStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.UserStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UserStatusValueLabel
			// 
			this.UserStatusValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 73, true);
			this.UserStatusValueLabel.Name = "UserStatusValueLabel";
			this.UserStatusValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 14, true);
			this.UserStatusValueLabel.TabIndex = 0;
			this.UserStatusValueLabel.Text = "";
			this.UserStatusValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ProductLabel
			// 
			this.ProductLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 25, true);
			this.ProductLabel.Name = "ProductLabel";
			this.ProductLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.ProductLabel.TabIndex = 0;
			this.ProductLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("5994a639-fe2a-4484-8880-cca0ef97beac", "Product");
			this.ProductLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ProductLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ProductValueLabel
			// 
			this.ProductValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 25, true);
			this.ProductValueLabel.Name = "ProductValueLabel";
			this.ProductValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 14, true);
			this.ProductValueLabel.TabIndex = 0;
			this.ProductValueLabel.Text = "";
			this.ProductValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TenantIDLabel
			// 
			this.TenantIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 41, true);
			this.TenantIDLabel.Name = "TenantIDLabel";
			this.TenantIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.TenantIDLabel.TabIndex = 0;
			this.TenantIDLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("753deb0b-d83d-4bb5-97e7-c9458377a48e", "Tenant ID");
			this.TenantIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TenantIDLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TenantIDValueLabel
			// 
			this.TenantIDValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 41, true);
			this.TenantIDValueLabel.Name = "TenantIDValueLabel";
			this.TenantIDValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 14, true);
			this.TenantIDValueLabel.TabIndex = 0;
			this.TenantIDValueLabel.Text = "";
			this.TenantIDValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SystemIdLabel
			// 
			this.SystemIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 57, true);
			this.SystemIdLabel.Name = "SystemIdLabel";
			this.SystemIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.SystemIdLabel.TabIndex = 0;
			this.SystemIdLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("4dbc1205-97b8-4fad-b4d0-38f733a22c16", "System ID");
			this.SystemIdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SystemIdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SystemIdValueLabel
			// 
			this.SystemIdValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 57, true);
			this.SystemIdValueLabel.Name = "SystemIdValueLabel";
			this.SystemIdValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 14, true);
			this.SystemIdValueLabel.TabIndex = 0;
			this.SystemIdValueLabel.Text = "";
			this.SystemIdValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ServerCodeLabel
			// 
			this.ServerCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 73, true);
			this.ServerCodeLabel.Name = "ServerCodeLabel";
			this.ServerCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.ServerCodeLabel.TabIndex = 0;
			this.ServerCodeLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("05e7f761-c628-40eb-87ff-d4cc91d65bda", "Server Code");
			this.ServerCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ServerCodeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ServerCodeValueLabel
			// 
			this.ServerCodeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 73, true);
			this.ServerCodeValueLabel.Name = "ServerCodeValueLabel";
			this.ServerCodeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.ServerCodeValueLabel.TabIndex = 0;
			this.ServerCodeValueLabel.Text = "";
			this.ServerCodeValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DatabaseNumberLabel
			// 
			this.DatabaseNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 89, true);
			this.DatabaseNumberLabel.Name = "DatabaseNumberLabel";
			this.DatabaseNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.DatabaseNumberLabel.TabIndex = 0;
			this.DatabaseNumberLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("60fc49f9-fa99-4d8a-8a34-91db0dad86c9", "Database Number");
			this.DatabaseNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DatabaseNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DatabaseNumberValueLabel
			// 
			this.DatabaseNumberValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 89, true);
			this.DatabaseNumberValueLabel.Name = "DatabaseNumberValueLabel";
			this.DatabaseNumberValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.DatabaseNumberValueLabel.TabIndex = 0;
			this.DatabaseNumberValueLabel.Text = "";
			this.DatabaseNumberValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// EnterpriseIdLabel
			// 
			this.EnterpriseIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 105, true);
			this.EnterpriseIdLabel.Name = "EnterpriseIdLabel";
			this.EnterpriseIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.EnterpriseIdLabel.TabIndex = 0;
			this.EnterpriseIdLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("2d6fe889-2178-4f69-9018-18c859b69b4f", "Enterprise ID");
			this.EnterpriseIdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.EnterpriseIdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// EnterpriseIdValueLabel
			// 
			this.EnterpriseIdValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 105, true);
			this.EnterpriseIdValueLabel.Name = "EnterpriseIdValueLabel";
			this.EnterpriseIdValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.EnterpriseIdValueLabel.TabIndex = 1;
			this.EnterpriseIdValueLabel.Text = "";
			this.EnterpriseIdValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// EnterpriseCodeLabel
			// 
			this.EnterpriseCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 121, true);
			this.EnterpriseCodeLabel.Name = "EnterpriseCodeLabel";
			this.EnterpriseCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 14, true);
			this.EnterpriseCodeLabel.TabIndex = 0;
			this.EnterpriseCodeLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("309e0075-f236-47de-adfc-201f24263b51", "Enterprise Code");
			this.EnterpriseCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.EnterpriseCodeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// EnterpriseCodeValueLabel
			// 
			this.EnterpriseCodeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 121, true);
			this.EnterpriseCodeValueLabel.Name = "EnterpriseCodeValueLabel";
			this.EnterpriseCodeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.EnterpriseCodeValueLabel.TabIndex = 0;
			this.EnterpriseCodeValueLabel.Text = "";
			this.EnterpriseCodeValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// WebAccessEnabledLabel
			// 
			this.WebAccessEnabledLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
			this.WebAccessEnabledLabel.Name = "WebAccessEnabledLabel";
			this.WebAccessEnabledLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 14, true);
			this.WebAccessEnabledLabel.TabIndex = 0;
			this.WebAccessEnabledLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("9d6a1c8e-789c-4daf-b147-9858ecde488b", "Web Access Enabled");
			this.WebAccessEnabledLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.WebAccessEnabledLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WebAccessEnabledCheckBox
			// 
			this.WebAccessEnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WebAccessEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 20, true);
			this.WebAccessEnabledCheckBox.Name = "WebAccessEnabledCheckBox";
			this.WebAccessEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 16, true);
			this.WebAccessEnabledCheckBox.TabIndex = 0;
			this.WebAccessEnabledCheckBox.UseVisualStyleBackColor = true;
			this.WebAccessEnabledCheckBox.Enabled = false;
			// 
			// PasswordSetLabel
			// 
			this.PasswordSetLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.PasswordSetLabel.Name = "PasswordSetLabel";
			this.PasswordSetLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 14, true);
			this.PasswordSetLabel.TabIndex = 0;
			this.PasswordSetLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("bfe89240-246f-41d6-8773-88ac5e19e484", "Password Set?");
			this.PasswordSetLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.PasswordSetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PasswordSetCheckBox
			// 
			this.PasswordSetCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PasswordSetCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 37, true);
			this.PasswordSetCheckBox.Name = "PasswordSetCheckBox";
			this.PasswordSetCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.PasswordSetCheckBox.TabIndex = 0;
			this.PasswordSetCheckBox.UseVisualStyleBackColor = true;
			this.PasswordSetCheckBox.Enabled = false;
			// 
			// SendPasswordInstructionsButton
			// 
			this.SendPasswordInstructionsButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5477d98c-fd4f-4c25-a9ce-9399742fd59a", "Send Password Instructions");
			this.SendPasswordInstructionsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 54, true);
			this.SendPasswordInstructionsButton.Name = "SetPasswordInstructions";
			this.SendPasswordInstructionsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 23, true);
			this.SendPasswordInstructionsButton.TabIndex = 3;
			this.SendPasswordInstructionsButton.Click += SendPasswordInstructionsButton_Click;
			// 
			// InstructionsLastSentLabel
			// 
			this.InstructionsLastSentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.InstructionsLastSentLabel.Name = "InstructionsLastSentLabel";
			this.InstructionsLastSentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 14, true);
			this.InstructionsLastSentLabel.TabIndex = 0;
			this.InstructionsLastSentLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("8cf153a6-533c-4a7a-af02-62ee1d1b7658", "Instructions Last Sent");
			this.InstructionsLastSentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.InstructionsLastSentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// InstructionsLastSentValueLabel
			// 
			this.InstructionsLastSentValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 80, true);
			this.InstructionsLastSentValueLabel.Name = "InstructionsLastSentValueLabel";
			this.InstructionsLastSentValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 14, true);
			this.InstructionsLastSentValueLabel.TabIndex = 0;
			this.InstructionsLastSentValueLabel.Text = "";
			this.InstructionsLastSentValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ContactGroupBox
			// 
			this.ContactGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ContactGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("15343cb6-03ac-4147-97ff-2c43e8b1a7a3", "Person Related System User Accounts");
			this.ContactGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContactGroupBox.Name = "ContactGroupBox";
			this.ContactGroupBox.TabIndex = 1;
			this.ContactGroupBox.TabStop = false;
			this.ContactGroupBox.Dock = DockStyle.Fill;
			// 
			// contactDetailsPanel
			// 
			this.contactDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.contactDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.contactDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.contactDetailsPanel.Name = "contactDetailsPanel";
			this.contactDetailsPanel.AutoSize = true;
			this.contactDetailsPanel.AutoScroll = false;
			this.contactDetailsPanel.TabIndex = 1;
			this.contactDetailsPanel.Controls.Add(this.ContactGroupBox);
			this.MainSplitContainer.Panel2.Controls.Add(this.contactDetailsPanel);
			// 
			// adicionalLicenceDetailsGroupBox
			// 
			this.adicionalLicenceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.adicionalLicenceDetailsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ac771266-8d2b-4bf4-853f-b537f3ba9b7b", "System User Information");
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseIdLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseIdValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserIDLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserIDValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserNameLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserNameValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserEmailLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserEmailValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserStatusLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.UserStatusValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ProductLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ProductValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.SystemIdLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.SystemIdValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseCodeLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseCodeValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ServerCodeLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ServerCodeValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.DatabaseNumberLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.DatabaseNumberValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.TenantIDLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.TenantIDValueLabel);
			this.adicionalLicenceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.adicionalLicenceDetailsGroupBox.Name = "adicionalLicenceDetailsGroupBox";
			this.adicionalLicenceDetailsGroupBox.TabIndex = 1;
			this.adicionalLicenceDetailsGroupBox.TabStop = false;
			this.adicionalLicenceDetailsGroupBox.Dock = DockStyle.Fill;
			this.adicionalLicenceDetailsGroupBox.Enabled = true;
			// 
			// linkedContactGroupBox
			// 
			this.linkedContactGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.linkedContactGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b8c8286f-bb0c-4c7c-8eca-abc029b6f48a", "Linked Contact");
			this.linkedContactGroupBox.Controls.Add(this.ContactOrganisationLabel);
			this.linkedContactGroupBox.Controls.Add(this.ContactOrganisationValueLabel);
			this.linkedContactGroupBox.Controls.Add(this.ContactEmailLabel);
			this.linkedContactGroupBox.Controls.Add(this.ContactEmailValueLabel);
			this.linkedContactGroupBox.Controls.Add(this.WebAccessContactGuidFindBox);
			this.linkedContactGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.linkedContactGroupBox.Name = "linkedContactGroupBox";
			this.linkedContactGroupBox.TabIndex = 1;
			this.linkedContactGroupBox.TabStop = false;
			this.linkedContactGroupBox.Enabled = true;
			this.linkedContactGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.linkedContactGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 50, true);
			// 
			// webSecurityGroupBox
			// 
			this.webSecurityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.webSecurityGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("08f57d1f-29b4-4908-8256-128153c08cab", "Web Security");
			this.webSecurityGroupBox.Controls.Add(this.WebAccessEnabledLabel);
			this.webSecurityGroupBox.Controls.Add(this.WebAccessEnabledCheckBox);
			this.webSecurityGroupBox.Controls.Add(this.PasswordSetLabel);
			this.webSecurityGroupBox.Controls.Add(this.PasswordSetCheckBox);
			this.webSecurityGroupBox.Controls.Add(this.SendPasswordInstructionsButton);
			this.webSecurityGroupBox.Controls.Add(this.InstructionsLastSentLabel);
			this.webSecurityGroupBox.Controls.Add(this.InstructionsLastSentValueLabel);
			this.webSecurityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.webSecurityGroupBox.Name = "webSecurityGroupBox";
			this.webSecurityGroupBox.TabIndex = 1;
			this.webSecurityGroupBox.TabStop = false;
			this.webSecurityGroupBox.Dock = DockStyle.Fill;
			this.webSecurityGroupBox.Enabled = true;
			// 
			// WebAccessContactGuidFindBox
			// 
			this.WebAccessContactGuidFindBox.AllowDrop = true;
			this.WebAccessContactGuidFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4cb26329-1a66-4103-87f8-84c09bb50143", "Contact");
			this.WebAccessContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 20, true);
			this.WebAccessContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.WebAccessContactGuidFindBox.Name = "WebAccessContactGuidFindBox";
			this.WebAccessContactGuidFindBox.PopupCaption = "Select Contact";
			this.WebAccessContactGuidFindBox.PreBoundMaxLength = 30;
			this.WebAccessContactGuidFindBox.ShowDescriptionBox = false;
			this.WebAccessContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.WebAccessContactGuidFindBox.TabIndex = 17;
			this.WebAccessContactGuidFindBox.Enabled = false;
			// 
			// ContactOrganisationLabel
			// 
			this.ContactOrganisationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 20, true);
			this.ContactOrganisationLabel.Name = "ContactOrganisationLabel";
			this.ContactOrganisationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 14, true);
			this.ContactOrganisationLabel.TabIndex = 0;
			this.ContactOrganisationLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("41b99473-243e-466e-a33f-b490c14e12a4", "Organisation:");
			this.ContactOrganisationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// ContactOrganisationValueLabel
			// 
			this.ContactOrganisationValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 20, true);
			this.ContactOrganisationValueLabel.Name = "ContactOrganisationValueLabel";
			this.ContactOrganisationValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 14, true);
			this.ContactOrganisationValueLabel.TabIndex = 0;
			this.ContactOrganisationValueLabel.Text = "";
			// 
			// ContactEmailLabel
			// 
			this.ContactEmailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 20, true);
			this.ContactEmailLabel.Name = "ContactEmailLabel";
			this.ContactEmailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 14, true);
			this.ContactEmailLabel.TabIndex = 0;
			this.ContactEmailLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("D966BFE3-F1EB-4176-A616-2987D27FFF32", "Contact Email");
			this.ContactEmailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ContactEmailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UserEmailValueLabel
			// 
			this.ContactEmailValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(760, 20, true);
			this.ContactEmailValueLabel.Name = "ContactEmailValueLabel";
			this.ContactEmailValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 14, true);
			this.ContactEmailValueLabel.TabIndex = 0;
			this.ContactEmailValueLabel.Text = "";
			this.ContactEmailValueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// webSecurityPanel
			// 
			this.webSecurityPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.webSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.webSecurityPanel.Name = "webSecurityPanel";
			this.webSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 110, true);
			this.webSecurityPanel.TabIndex = 1;
			this.webSecurityPanel.Controls.Add(this.webSecurityGroupBox);
			this.MainSplitContainer.Panel2.Controls.Add(this.webSecurityPanel);
			this.MainSplitContainer.Panel2.Controls.Add(this.linkedContactGroupBox);
			// 
			// adicionalLicenceDetailsPanel
			// 
			this.adicionalLicenceDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.adicionalLicenceDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.adicionalLicenceDetailsPanel.Name = "adicionalLicenceDetailsPanel";
			this.adicionalLicenceDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 150, true);
			this.adicionalLicenceDetailsPanel.TabIndex = 1;
			this.adicionalLicenceDetailsPanel.Controls.Add(this.adicionalLicenceDetailsGroupBox);
			this.MainSplitContainer.Panel2.Controls.Add(this.adicionalLicenceDetailsPanel);
			// 
			// SaveToolStrip
			// 
			this.SaveToolStrip.AutoSize = true;
			this.SaveToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
			this.SaveToolStrip.Name = "SaveToolStrip";
			this.SaveToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1045, 0, true);
			this.SaveToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.SaveToolStrip.TabIndex = 0;
			this.SaveToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SaveToolStrip.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SaveToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.SaveToolStrip.BackColor = System.Drawing.Color.Transparent;
			// 
			// mainTopPanel1
			// 
			this.mainTopPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.mainTopPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainTopPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.mainTopPanel1.Name = "mainTopPanel1";
			this.mainTopPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 700, true);
			this.mainTopPanel1.TabIndex = 1;
			this.mainTopPanel1.Controls.Add(this.MainSplitContainer);
			// 
			// mainBottomPanel2
			// 
			this.mainBottomPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.mainBottomPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.mainBottomPanel2.Name = "mainBottomPanel2";
			this.mainBottomPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 30, true);
			this.mainBottomPanel2.TabIndex = 1;
			this.mainBottomPanel2.Controls.Add(this.SaveToolStrip);
			// 
			// SystemUserAccountsWizardForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 5, 5, 5, true);
			this.CaptionRenderingEnabled = true;
			this.MainGroupBox.Controls.Add(this.mainTopPanel1);
			this.MainGroupBox.Controls.Add(this.mainBottomPanel2);
			this.Controls.Add(MainGroupBox);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 800, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1400, 800, true);
			this.Name = "SystemUserAccountsWizardForm";
			this.Text = "System User Accounts";
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.adicionalLicenceDetailsGroupBox.ResumeLayout(false);
			this.adicionalLicenceDetailsGroupBox.PerformLayout();
			this.linkedContactGroupBox.ResumeLayout(false);
			this.linkedContactGroupBox.PerformLayout();
			this.webSecurityGroupBox.ResumeLayout(false);
			this.webSecurityGroupBox.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.ContactGroupBox.ResumeLayout(false);
			this.ContactGroupBox.PerformLayout();
			this.mainTopPanel1.ResumeLayout(false);
			this.mainTopPanel1.PerformLayout();
			this.mainBottomPanel2.ResumeLayout(false);
			this.mainBottomPanel2.PerformLayout();
			this.adicionalLicenceDetailsPanel.ResumeLayout(false);
			this.adicionalLicenceDetailsPanel.PerformLayout();
			this.webSecurityPanel.ResumeLayout(false);
			this.webSecurityPanel.PerformLayout();
			this.contactDetailsPanel.ResumeLayout(false);
			this.contactDetailsPanel.PerformLayout();
			this.WebAccessContactGuidFindBox.ResumeLayout(true);
			this.WebAccessContactGuidFindBox.PerformLayout();
			this.SaveToolStrip.ResumeLayout(false);
			this.SaveToolStrip.PerformLayout();
			this.EdiCustomerUserAccountBottomPanel.ResumeLayout(false);
			this.EdiCustomerUserAccountBottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZGroupBox MainGroupBox;
		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		ZArchitecture.GUI.ZGroupBox ContactGroupBox;
		Enterprise.ZArchitecture.ZLabel EnterpriseIdLabel;
		protected Enterprise.ZArchitecture.ZLabel EnterpriseIdValueLabel;
		Enterprise.ZArchitecture.ZLabel UserIDLabel;
		protected Enterprise.ZArchitecture.ZLabel UserIDValueLabel;
		Enterprise.ZArchitecture.ZLabel UserNameLabel;
		protected Enterprise.ZArchitecture.ZLabel UserNameValueLabel;
		Enterprise.ZArchitecture.ZLabel UserEmailLabel;
		protected Enterprise.ZArchitecture.ZLabel UserEmailValueLabel;
		Enterprise.ZArchitecture.ZLabel ContactEmailLabel;
		protected Enterprise.ZArchitecture.ZLabel ContactEmailValueLabel;
		Enterprise.ZArchitecture.ZLabel UserStatusLabel;
		protected Enterprise.ZArchitecture.ZLabel UserStatusValueLabel;
		Enterprise.ZArchitecture.GUI.ZPanel mainTopPanel1;
		Enterprise.ZArchitecture.GUI.ZPanel mainBottomPanel2;
		Enterprise.ZArchitecture.GUI.ZPanel adicionalLicenceDetailsPanel;
		Enterprise.ZArchitecture.GUI.ZPanel webSecurityPanel;
		ZArchitecture.GUI.ZGroupBox adicionalLicenceDetailsGroupBox;
		ZArchitecture.GUI.ZGroupBox linkedContactGroupBox;
		ZArchitecture.GUI.ZGroupBox webSecurityGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel contactDetailsPanel;
		protected ZArchitecture.GUI.ZGuidFindBox WebAccessContactGuidFindBox;
		protected Enterprise.ZArchitecture.GUI.ZToolStrip SaveToolStrip;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZPanel EdiCustomerUserAccountBottomPanel;
		Enterprise.ZArchitecture.ZLabel ContactOrganisationLabel;
		protected Enterprise.ZArchitecture.ZLabel ContactOrganisationValueLabel;
		Enterprise.ZArchitecture.ZLabel ProductLabel;
		protected Enterprise.ZArchitecture.ZLabel ProductValueLabel;
		Enterprise.ZArchitecture.ZLabel SystemIdLabel;
		protected Enterprise.ZArchitecture.ZLabel SystemIdValueLabel;
		Enterprise.ZArchitecture.ZLabel EnterpriseCodeLabel;
		protected Enterprise.ZArchitecture.ZLabel EnterpriseCodeValueLabel;
		Enterprise.ZArchitecture.ZLabel ServerCodeLabel;
		protected Enterprise.ZArchitecture.ZLabel ServerCodeValueLabel;
		Enterprise.ZArchitecture.ZLabel DatabaseNumberLabel;
		protected Enterprise.ZArchitecture.ZLabel DatabaseNumberValueLabel;
		Enterprise.ZArchitecture.ZLabel TenantIDLabel;
		protected Enterprise.ZArchitecture.ZLabel TenantIDValueLabel;
		Enterprise.ZArchitecture.ZLabel WebAccessEnabledLabel;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox WebAccessEnabledCheckBox;
		Enterprise.ZArchitecture.ZLabel PasswordSetLabel;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox PasswordSetCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZButton SendPasswordInstructionsButton;
		Enterprise.ZArchitecture.ZLabel InstructionsLastSentLabel;
		protected Enterprise.ZArchitecture.ZLabel InstructionsLastSentValueLabel;
	}
}
