using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;

namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class LicenceDatabaseRegistrationWizardForm
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
			this.additionalLicenceDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.adicionalLicenceDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.organisationDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.masterOrganisationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WebAccessOrgGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OrganisationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrganisationDetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OrganisationDetailsTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.EnterpriseIdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OwnerNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OwnerNameValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Address1Label = new Enterprise.ZArchitecture.ZLabel();
			this.Address1ValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Address2Label = new Enterprise.ZArchitecture.ZLabel();
			this.Address2ValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CityValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StateValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PostCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PostCodeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SystemIdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SystemIdValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TenantIdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TenantIdValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BusinessRegoNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BusinessRegoNoValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EnterpriseCodeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ServerCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ServerCodeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CargowiseCompanyCodeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CargowiseCompanyCodeValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DatabaseNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DatabaseNumberValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.InfoExpiresLabel = new Enterprise.ZArchitecture.ZLabel();
			this.InfoExpiresValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SetSelectOrganisationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.saveToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ShowFiltersLicenceDatabaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShowFiltersOrganisationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OrganisationDetailsTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrganisationDetailsGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrganisationDetailsBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.licenceDatabaseBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.licenceDatabaseTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LicenceDatabaseToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.OrganisationToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.OrganisationGroupBox.SuspendLayout();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.adicionalLicenceDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationDetailsSplitContainer)).BeginInit();
			this.OrganisationDetailsSplitContainer.Panel1.SuspendLayout();
			this.OrganisationDetailsSplitContainer.Panel2.SuspendLayout();
			this.OrganisationDetailsSplitContainer.SuspendLayout();
			this.OrganisationDetailsTableLayoutPanel.SuspendLayout();
			this.WebAccessOrgGuidFindBox.SuspendLayout();
			this.mainTopPanel1.SuspendLayout();
			this.mainBottomPanel2.SuspendLayout();
			this.additionalLicenceDetailsPanel.SuspendLayout();
			this.organisationDetailsPanel.SuspendLayout();
			this.masterOrganisationPanel.SuspendLayout();
			this.saveToolStrip.SuspendLayout();
			this.OrganisationDetailsTopPanel.SuspendLayout();
			this.OrganisationDetailsGridPanel.SuspendLayout();
			this.OrganisationDetailsBottomPanel.SuspendLayout();
			this.licenceDatabaseBottomPanel.SuspendLayout();
			this.licenceDatabaseTopPanel.SuspendLayout();
			this.LicenceDatabaseToolStrip.SuspendLayout();
			this.OrganisationToolStrip.SuspendLayout();
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
			this.MainGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("DE6DB9D5-320F-4548-A67A-848183979CC3", "Associate Licence Databases to Master Organisation");
			this.MainGroupBox.Controls.Add(this.ShowFiltersLicenceDatabaseCheckBox);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 800, true);
			this.MainGroupBox.TabIndex = 1;
			this.MainGroupBox.TabStop = false;
			this.MainGroupBox.Dock = DockStyle.Fill;
			// 
			// ShowFiltersLicenceDatabaseCheckBox
			// 
			this.ShowFiltersLicenceDatabaseCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("91A4545B-2899-4B59-9FF2-EC855B9CA80A", "Show Filters");
			this.ShowFiltersLicenceDatabaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowFiltersLicenceDatabaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 0, true);
			this.ShowFiltersLicenceDatabaseCheckBox.Name = "ShowFiltersLicenceDatabaseCheckBox";
			this.ShowFiltersLicenceDatabaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 16, true);
			this.ShowFiltersLicenceDatabaseCheckBox.TabIndex = 0;
			this.ShowFiltersLicenceDatabaseCheckBox.UseVisualStyleBackColor = true;
			this.ShowFiltersLicenceDatabaseCheckBox.CheckedChanged += ShowFiltersLicenceDatabaseCheckBox_CheckedChanged;
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
			// licenceDatabaseBottomPanel
			// 
			this.licenceDatabaseBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.licenceDatabaseBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.licenceDatabaseBottomPanel.Name = "licenceDatabaseBottomPanel";
			this.licenceDatabaseBottomPanel.TabIndex = 1;
			this.licenceDatabaseBottomPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 3, true);
			this.MainSplitContainer.Panel1.Controls.Add(licenceDatabaseBottomPanel);
			// 
			// licenceDatabaseTopPanel
			// 
			this.licenceDatabaseTopPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.licenceDatabaseTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.licenceDatabaseTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.licenceDatabaseTopPanel.Name = "licenceDatabaseTopPanel";
			this.licenceDatabaseTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 25, true);
			this.licenceDatabaseTopPanel.TabIndex = 1;
			this.licenceDatabaseTopPanel.Controls.Add(this.LicenceDatabaseToolStrip);
			this.MainSplitContainer.Panel1.Controls.Add(licenceDatabaseTopPanel);
			// 
			// EnterpriseIdLabel
			// 
			this.EnterpriseIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 25, true);
			this.EnterpriseIdLabel.Name = "EnterpriseIdLabel";
			this.EnterpriseIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 14, true);
			this.EnterpriseIdLabel.TabIndex = 0;
			this.EnterpriseIdLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("2d6fe889-2178-4f69-9018-18c859b69b4f", "Enterprise ID");
			// 
			// EnterpriseIdTextBox
			// 
			this.EnterpriseIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EnterpriseIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 22, true);
			this.EnterpriseIdTextBox.Name = "EnterpriseIdTextBox";
			this.EnterpriseIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.EnterpriseIdTextBox.TabIndex = 1;
			this.EnterpriseIdTextBox.ReadOnly = true;
			// 
			// OwnerNameLabel
			// 
			this.OwnerNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.OwnerNameLabel.Name = "OwnerNameLabel";
			this.OwnerNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.OwnerNameLabel.TabIndex = 0;
			this.OwnerNameLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("acb3b910-eb1f-4e81-8c0d-77365eb917b8", "Owner Name");
			this.OwnerNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// OwnerNameValueLabel
			// 
			this.OwnerNameValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 45, true);
			this.OwnerNameValueLabel.Name = "OwnerNameValueLabel";
			this.OwnerNameValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 14, true);
			this.OwnerNameValueLabel.TabIndex = 0;
			this.OwnerNameValueLabel.Text = "";
			// 
			// Address1Label
			// 
			this.Address1Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 45, true);
			this.Address1Label.Name = "Address1Label";
			this.Address1Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.Address1Label.TabIndex = 0;
			this.Address1Label.Text = CargoWiseOne.ResourceStrings.Res.GetString("9f4d2314-91ad-4850-afe6-ee05367e36ff", "Address 1");
			this.Address1Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// Address1ValueLabel
			// 
			this.Address1ValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 45, true);
			this.Address1ValueLabel.Name = "Address1ValueLabel";
			this.Address1ValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 14, true);
			this.Address1ValueLabel.TabIndex = 0;
			this.Address1ValueLabel.Text = "";
			// 
			// Address2Label
			// 
			this.Address2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 60, true);
			this.Address2Label.Name = "Address2Label";
			this.Address2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.Address2Label.TabIndex = 0;
			this.Address2Label.Text = CargoWiseOne.ResourceStrings.Res.GetString("67f78c83-a988-4e66-9b9b-dd0aec629244", "Address 2");
			this.Address2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// Address2ValueLabel
			// 
			this.Address2ValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 60, true);
			this.Address2ValueLabel.Name = "Address2ValueLabel";
			this.Address2ValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 14, true);
			this.Address2ValueLabel.TabIndex = 0;
			this.Address2ValueLabel.Text = "";
			// 
			// CityLabel
			// 
			this.CityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 75, true);
			this.CityLabel.Name = "CityLabel";
			this.CityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.CityLabel.TabIndex = 0;
			this.CityLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("b3732146-c53b-4270-ae29-1bb8921c5e6e", "City");
			this.CityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// CityValueLabel
			// 
			this.CityValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 75, true);
			this.CityValueLabel.Name = "CityValueLabel";
			this.CityValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.CityValueLabel.TabIndex = 0;
			this.CityValueLabel.Text = "";
			// 
			// StateLabel
			// 
			this.StateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 90, true);
			this.StateLabel.Name = "StateLabel";
			this.StateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.StateLabel.TabIndex = 0;
			this.StateLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("f84cef4c-31ad-4614-af34-d55308485da4", "State");
			this.StateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// StateValueLabel
			// 
			this.StateValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 90, true);
			this.StateValueLabel.Name = "StateValueLabel";
			this.StateValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.StateValueLabel.TabIndex = 0;
			this.StateValueLabel.Text = "";
			// 
			// PostCodeLabel
			// 
			this.PostCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 105, true);
			this.PostCodeLabel.Name = "PostCodeLabel";
			this.PostCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.PostCodeLabel.TabIndex = 0;
			this.PostCodeLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("e8ed7a02-0a16-4a00-9089-11919cb22cde", "Post Code");
			this.PostCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// PostCodeValueLabel
			// 
			this.PostCodeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 105, true);
			this.PostCodeValueLabel.Name = "PostCodeValueLabel";
			this.PostCodeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 14, true);
			this.PostCodeValueLabel.TabIndex = 0;
			this.PostCodeValueLabel.Text = "";
			// 
			// CountryLabel
			// 
			this.CountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 120, true);
			this.CountryLabel.Name = "CountryLabel";
			this.CountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.CountryLabel.TabIndex = 0;
			this.CountryLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("cf5275a6-3897-4530-a4f7-ab40213fdad6", "Country/Region");
			this.CountryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// CountryValueLabel
			// 
			this.CountryValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 120, true);
			this.CountryValueLabel.Name = "CountryValueLabel";
			this.CountryValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.CountryValueLabel.TabIndex = 0;
			this.CountryValueLabel.Text = "";
			// 
			// ProductLabel
			// 
			this.ProductLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 60, true);
			this.ProductLabel.Name = "ProductLabel";
			this.ProductLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.ProductLabel.TabIndex = 0;
			this.ProductLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("5994a639-fe2a-4484-8880-cca0ef97beac", "Product");
			this.ProductLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// ProductValueLabel
			// 
			this.ProductValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 60, true);
			this.ProductValueLabel.Name = "ProductValueLabel";
			this.ProductValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.ProductValueLabel.TabIndex = 0;
			this.ProductValueLabel.Text = "";
			// 
			// SystemIdLabel
			// 
			this.SystemIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 75, true);
			this.SystemIdLabel.Name = "SystemIdLabel";
			this.SystemIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 14, true);
			this.SystemIdLabel.TabIndex = 0;
			this.SystemIdLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("4dbc1205-97b8-4fad-b4d0-38f733a22c16", "System ID");
			this.SystemIdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// SystemIdValueLabel
			// 
			this.SystemIdValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 75, true);
			this.SystemIdValueLabel.Name = "SystemIdValueLabel";
			this.SystemIdValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.SystemIdValueLabel.TabIndex = 0;
			this.SystemIdValueLabel.Text = "";
			// 
			// TenantIdLabel
			// 
			this.TenantIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 90, true);
			this.TenantIdLabel.Name = "TenantIdLabel";
			this.TenantIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 14, true);
			this.TenantIdLabel.TabIndex = 0;
			this.TenantIdLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("d832f3d5-5fe7-4293-a5e5-00db5bf7bfda", "Tenant ID");
			this.TenantIdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// TenantIdValueLabel
			// 
			this.TenantIdValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 90, true);
			this.TenantIdValueLabel.Name = "TenantIdValueLabel";
			this.TenantIdValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.TenantIdValueLabel.TabIndex = 0;
			this.TenantIdValueLabel.Text = "";
			// 
			// EnterpriseCodeLabel
			// 
			this.EnterpriseCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 105, true);
			this.EnterpriseCodeLabel.Name = "EnterpriseCodeLabel";
			this.EnterpriseCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 14, true);
			this.EnterpriseCodeLabel.TabIndex = 0;
			this.EnterpriseCodeLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("309e0075-f236-47de-adfc-201f24263b51", "Enterprise Code");
			this.EnterpriseCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// EnterpriseCodeValueLabel
			// 
			this.EnterpriseCodeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 105, true);
			this.EnterpriseCodeValueLabel.Name = "EnterpriseCodeValueLabel";
			this.EnterpriseCodeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.EnterpriseCodeValueLabel.TabIndex = 0;
			this.EnterpriseCodeValueLabel.Text = "";
			// 
			// ServerCodeLabel
			// 
			this.ServerCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 120, true);
			this.ServerCodeLabel.Name = "ServerCodeLabel";
			this.ServerCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 14, true);
			this.ServerCodeLabel.TabIndex = 0;
			this.ServerCodeLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("05e7f761-c628-40eb-87ff-d4cc91d65bda", "Server Code");
			this.ServerCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// ServerCodeValueLabel
			// 
			this.ServerCodeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 120, true);
			this.ServerCodeValueLabel.Name = "ServerCodeValueLabel";
			this.ServerCodeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.ServerCodeValueLabel.TabIndex = 0;
			this.ServerCodeValueLabel.Text = "";
			// 
			// DatabaseNumberLabel
			// 
			this.DatabaseNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 150, true);
			this.DatabaseNumberLabel.Name = "DatabaseNumberLabel";
			this.DatabaseNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 14, true);
			this.DatabaseNumberLabel.TabIndex = 0;
			this.DatabaseNumberLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("60fc49f9-fa99-4d8a-8a34-91db0dad86c9", "Database Number");
			this.DatabaseNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// DatabaseNumberValueLabel
			// 
			this.DatabaseNumberValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 150, true);
			this.DatabaseNumberValueLabel.Name = "DatabaseNumberValueLabel";
			this.DatabaseNumberValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 14, true);
			this.DatabaseNumberValueLabel.TabIndex = 0;
			this.DatabaseNumberValueLabel.Text = "";
			// 
			// InfoExpiresLabel
			// 
			this.InfoExpiresLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 165, true);
			this.InfoExpiresLabel.Name = "InfoExpiresLabel";
			this.InfoExpiresLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 14, true);
			this.InfoExpiresLabel.TabIndex = 0;
			this.InfoExpiresLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("753deb0b-d83d-4bb5-97e7-c9458377a48e", "Info Expires");
			this.InfoExpiresLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// InfoExpiresValueLabel
			// 
			this.InfoExpiresValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 165, true);
			this.InfoExpiresValueLabel.Name = "InfoExpiresValueLabel";
			this.InfoExpiresValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.InfoExpiresValueLabel.TabIndex = 0;
			this.InfoExpiresValueLabel.Text = "";
			// 
			// BusinessRegoNoLabel
			// 
			this.BusinessRegoNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 135, true);
			this.BusinessRegoNoLabel.Name = "BusinessRegoNoLabel";
			this.BusinessRegoNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.BusinessRegoNoLabel.TabIndex = 0;
			this.BusinessRegoNoLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("6afc0a30-b315-4fe1-a01e-623501cca2fd", "Business Rego No.");
			this.BusinessRegoNoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// BusinessRegoNoValueLabel
			// 
			this.BusinessRegoNoValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 135, true);
			this.BusinessRegoNoValueLabel.Name = "BusinessRegoNoValueLabel";
			this.BusinessRegoNoValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.BusinessRegoNoValueLabel.TabIndex = 0;
			this.BusinessRegoNoValueLabel.Text = "";
			// 
			// CargowiseCompanyCodeLabel
			// 
			this.CargowiseCompanyCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 135, true);
			this.CargowiseCompanyCodeLabel.Name = "CargowiseCompanyCodeLabel";
			this.CargowiseCompanyCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.CargowiseCompanyCodeLabel.TabIndex = 0;
			this.CargowiseCompanyCodeLabel.Text = CargoWiseOne.ResourceStrings.Res.GetString("da6adbe1-3ecc-4612-b396-d6a145d3bded", "Company Code");
			this.CargowiseCompanyCodeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			// 
			// CargowiseCompanyCodeValueLabel
			// 
			this.CargowiseCompanyCodeValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 135, true);
			this.CargowiseCompanyCodeValueLabel.Name = "CargowiseCompanyCodeValueLabel";
			this.CargowiseCompanyCodeValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 14, true);
			this.CargowiseCompanyCodeValueLabel.TabIndex = 0;
			this.CargowiseCompanyCodeValueLabel.Text = "";
			//
			// SetSelectOrganisationButton
			//
			this.SetSelectOrganisationButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7FC24206-839B-48B4-9AC8-4C238605E197", "Set Selected Organisation");
			this.SetSelectOrganisationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SetSelectOrganisationButton.Name = "SetSelectOrganisationButton";
			this.SetSelectOrganisationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 23, true);
			this.SetSelectOrganisationButton.TabIndex = 3;
			this.SetSelectOrganisationButton.Click += SetSelectOrganisationButton_Click;
			this.OrganisationDetailsBottomPanel.Controls.Add(SetSelectOrganisationButton);
			// 
			// OrganisationDetailsSplitContainer
			// 
			this.OrganisationDetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationDetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationDetailsSplitContainer.Name = "OrganisationDetailsSplitContainer";
			this.OrganisationDetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.OrganisationDetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(305);
			this.OrganisationDetailsSplitContainer.TabIndex = 0;
			this.OrganisationDetailsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.OrganisationDetailsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.OrganisationDetailsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// 
			// OrganisationDetailsTableLayoutPanel
			// 
			this.OrganisationDetailsTableLayoutPanel.ColumnCount = 1;
			this.OrganisationDetailsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.OrganisationDetailsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationDetailsTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationDetailsTableLayoutPanel.Name = "OrganisationDetailsTableLayoutPanel";
			this.OrganisationDetailsTableLayoutPanel.RowCount = 2;
			this.OrganisationDetailsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OrganisationDetailsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.OrganisationDetailsTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 400, true);
			this.OrganisationDetailsTableLayoutPanel.TabIndex = 0;
			this.OrganisationDetailsSplitContainer.Panel2.Controls.Add(this.OrganisationDetailsTableLayoutPanel);
			// 
			// OrganisationDetailsGridPanel
			// 
			this.OrganisationDetailsGridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.OrganisationDetailsGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrganisationDetailsGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrganisationDetailsGridPanel.Name = "OrganisationDetailsGridPanel";
			this.OrganisationDetailsGridPanel.TabIndex = 0;
			this.OrganisationDetailsSplitContainer.Panel1.Controls.Add(this.OrganisationDetailsGridPanel);
			// 
			// OrganisationDetailsTopPanel
			// 
			this.OrganisationDetailsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationDetailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrganisationDetailsTopPanel.Name = "OrganisationDetailsTopPanel";
			this.OrganisationDetailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 25, true);
			this.OrganisationDetailsTopPanel.TabIndex = 1;
			this.OrganisationDetailsSplitContainer.Panel1.Controls.Add(this.OrganisationDetailsTopPanel);
			// 
			// OrganisationToolStrip
			// 
			this.OrganisationToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left)));
			this.OrganisationToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationToolStrip.Name = "OrganisationToolStrip";
			this.OrganisationToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 22, true);
			this.OrganisationToolStrip.TabIndex = 0;
			this.OrganisationDetailsTopPanel.Controls.Add(OrganisationToolStrip);
			// 
			// OrganisationDetailsBottomPanel
			// 
			this.OrganisationDetailsBottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.OrganisationDetailsBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrganisationDetailsBottomPanel.Name = "OrganisationDetailsBottomPanel";
			this.OrganisationDetailsBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 30, true);
			this.OrganisationDetailsBottomPanel.TabIndex = 2;
			this.OrganisationDetailsSplitContainer.Panel1.Controls.Add(this.OrganisationDetailsBottomPanel);
			// 
			// OrganisationGroupBox
			// 
			this.OrganisationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrganisationGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("181448D5-B303-49DD-97EB-31BADB16CE87", "Set Master Organisation");
			this.OrganisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OrganisationGroupBox.Name = "OrganisationGroupBox";
			this.OrganisationGroupBox.TabIndex = 1;
			this.OrganisationGroupBox.TabStop = false;
			this.OrganisationGroupBox.Dock = DockStyle.Fill;
			this.OrganisationGroupBox.Controls.Add(this.OrganisationDetailsSplitContainer);
			this.OrganisationGroupBox.Controls.Add(this.ShowFiltersOrganisationCheckBox);
			// 
			// ShowFiltersOrganisationCheckBox
			// 
			this.ShowFiltersOrganisationCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ECCFF660-BB03-44C5-914E-485663A2458D", "Show Matched Organisations");
			this.ShowFiltersOrganisationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowFiltersOrganisationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 0, true);
			this.ShowFiltersOrganisationCheckBox.Name = "ShowFiltersOrganisationCheckBox";
			this.ShowFiltersOrganisationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 16, true);
			this.ShowFiltersOrganisationCheckBox.TabIndex = 0;
			this.ShowFiltersOrganisationCheckBox.UseVisualStyleBackColor = true;
			this.ShowFiltersOrganisationCheckBox.CheckedChanged += ShowFiltersOrganisationCheckBox_CheckedChanged;
			// 
			// organisationDetailsPanel
			// 
			this.organisationDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.organisationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.organisationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.organisationDetailsPanel.Name = "organisationDetailsPanel";
			this.organisationDetailsPanel.AutoSize = true;
			this.organisationDetailsPanel.AutoScroll = false;
			this.organisationDetailsPanel.TabIndex = 1;
			this.organisationDetailsPanel.Controls.Add(this.OrganisationGroupBox);
			this.MainSplitContainer.Panel2.Controls.Add(this.organisationDetailsPanel);
			// 
			// adicionalLicenceDetailsGroupBox
			// 
			this.adicionalLicenceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.adicionalLicenceDetailsGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3BDF0B0E-4852-4C75-831C-71E862296C2A", "Additional Licence Information");
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseIdLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseIdTextBox);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.OwnerNameLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.OwnerNameValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.Address1Label);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.Address1ValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.Address2Label);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.Address2ValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.CityLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.CityValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.StateLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.StateValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.PostCodeLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.PostCodeValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.CountryLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.CountryValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ProductLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ProductValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.SystemIdLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.SystemIdValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.TenantIdLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.TenantIdValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseCodeLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.EnterpriseCodeValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ServerCodeLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.ServerCodeValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.DatabaseNumberLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.DatabaseNumberValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.InfoExpiresLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.InfoExpiresValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.BusinessRegoNoLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.BusinessRegoNoValueLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.CargowiseCompanyCodeLabel);
			this.adicionalLicenceDetailsGroupBox.Controls.Add(this.CargowiseCompanyCodeValueLabel);

			this.adicionalLicenceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.adicionalLicenceDetailsGroupBox.Name = "adicionalLicenceDetailsGroupBox";
			this.adicionalLicenceDetailsGroupBox.TabIndex = 1;
			this.adicionalLicenceDetailsGroupBox.TabStop = false;
			this.adicionalLicenceDetailsGroupBox.Dock = DockStyle.Fill;
			this.adicionalLicenceDetailsGroupBox.Enabled = true;
			// 
			// WebAccessOrgGuidFindBox
			// 
			this.WebAccessOrgGuidFindBox.AllowDrop = true;
			this.WebAccessOrgGuidFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("C5D66325-454C-4301-A4D2-BDDBEC9B7189", "Master Org");
			this.WebAccessOrgGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 3, true);
			this.WebAccessOrgGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.WebAccessOrgGuidFindBox.Name = "WebAccessOrgGuidFindBox";
			this.WebAccessOrgGuidFindBox.PopupCaption = "Select Master Org";
			this.WebAccessOrgGuidFindBox.ShouldResize = true;
			this.WebAccessOrgGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 17, true);
			this.WebAccessOrgGuidFindBox.TabIndex = 17;
			this.WebAccessOrgGuidFindBox.EditableInViewMode = true;
			// 
			// masterOrganisationPanel
			// 
			this.masterOrganisationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.masterOrganisationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.masterOrganisationPanel.Name = "masterOrganisationPanel";
			this.masterOrganisationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 30, true);
			this.masterOrganisationPanel.TabIndex = 1;
			this.masterOrganisationPanel.Controls.Add(this.WebAccessOrgGuidFindBox);
			this.OrganisationGroupBox.Controls.Add(this.masterOrganisationPanel);
			// 
			// additionalLicenceDetailsPanel
			// 
			this.additionalLicenceDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.additionalLicenceDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.additionalLicenceDetailsPanel.Name = "additionalLicenceDetailsPanel";
			this.additionalLicenceDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 185, true);
			this.additionalLicenceDetailsPanel.TabIndex = 1;
			this.additionalLicenceDetailsPanel.Controls.Add(this.adicionalLicenceDetailsGroupBox);
			this.MainSplitContainer.Panel2.Controls.Add(this.additionalLicenceDetailsPanel);
			// 
			// toolStrip
			// 
			this.saveToolStrip.AutoSize = true;
			this.saveToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom)));
			this.saveToolStrip.Name = "toolStrip";
			this.saveToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1045, 0, true);
			this.saveToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.saveToolStrip.TabIndex = 0;
			this.saveToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.saveToolStrip.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.saveToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.saveToolStrip.BackColor = System.Drawing.Color.Transparent;
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
			this.mainBottomPanel2.Controls.Add(this.saveToolStrip);
			// 
			// LicenceDatabaseToolStrip
			// 
			this.LicenceDatabaseToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicenceDatabaseToolStrip.Name = "LicenceDatabaseToolStrip";
			this.LicenceDatabaseToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 22, true);
			this.LicenceDatabaseToolStrip.TabIndex = 0;
			// 
			// LicenceDatabaseRegistrationWizardForm
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
			this.Name = "LicenceDatabaseRegistrationWizardForm";
			this.Text = "Licence Database Registration";
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.adicionalLicenceDetailsGroupBox.ResumeLayout(false);
			this.adicionalLicenceDetailsGroupBox.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.mainTopPanel1.ResumeLayout(false);
			this.mainTopPanel1.PerformLayout();
			this.additionalLicenceDetailsPanel.ResumeLayout(false);
			this.additionalLicenceDetailsPanel.PerformLayout();
			this.organisationDetailsPanel.ResumeLayout(false);
			this.organisationDetailsPanel.PerformLayout();
			this.OrganisationGroupBox.ResumeLayout(false);
			this.OrganisationGroupBox.PerformLayout();
			this.OrganisationDetailsTableLayoutPanel.ResumeLayout(false);
			this.OrganisationDetailsTableLayoutPanel.PerformLayout();
			this.OrganisationDetailsSplitContainer.Panel1.ResumeLayout(false);
			this.OrganisationDetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OrganisationDetailsSplitContainer)).EndInit();
			this.OrganisationDetailsSplitContainer.ResumeLayout(false);
			this.OrganisationDetailsSplitContainer.PerformLayout();
			this.mainBottomPanel2.ResumeLayout(false);
			this.mainBottomPanel2.PerformLayout();
			this.WebAccessOrgGuidFindBox.ResumeLayout(false);
			this.WebAccessOrgGuidFindBox.PerformLayout();
			this.saveToolStrip.ResumeLayout(false);
			this.saveToolStrip.PerformLayout();
			this.OrganisationDetailsGridPanel.ResumeLayout(false);
			this.OrganisationDetailsGridPanel.PerformLayout();
			this.OrganisationDetailsBottomPanel.ResumeLayout(false);
			this.OrganisationDetailsBottomPanel.PerformLayout();
			this.licenceDatabaseBottomPanel.ResumeLayout(false);
			this.licenceDatabaseBottomPanel.PerformLayout();
			this.LicenceDatabaseToolStrip.ResumeLayout(false);
			this.LicenceDatabaseToolStrip.PerformLayout();
			this.OrganisationToolStrip.ResumeLayout(false);
			this.OrganisationToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZGroupBox MainGroupBox;
		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		ZArchitecture.GUI.ZGroupBox OrganisationGroupBox;
		protected CargoWise.Windows.UI.KSplitContainer OrganisationDetailsSplitContainer;
		CargoWise.Windows.UI.KTableLayoutPanel OrganisationDetailsTableLayoutPanel;
		Enterprise.ZArchitecture.ZLabel EnterpriseIdLabel;
		protected Enterprise.ZArchitecture.ZTextBox EnterpriseIdTextBox;
		Enterprise.ZArchitecture.ZLabel OwnerNameLabel;
		protected Enterprise.ZArchitecture.ZLabel OwnerNameValueLabel;
		Enterprise.ZArchitecture.ZLabel Address1Label;
		protected Enterprise.ZArchitecture.ZLabel Address1ValueLabel;
		Enterprise.ZArchitecture.ZLabel Address2Label;
		protected Enterprise.ZArchitecture.ZLabel Address2ValueLabel;
		Enterprise.ZArchitecture.ZLabel CityLabel;
		protected Enterprise.ZArchitecture.ZLabel CityValueLabel;
		Enterprise.ZArchitecture.ZLabel StateLabel;
		protected Enterprise.ZArchitecture.ZLabel StateValueLabel;
		Enterprise.ZArchitecture.ZLabel PostCodeLabel;
		protected Enterprise.ZArchitecture.ZLabel PostCodeValueLabel;
		Enterprise.ZArchitecture.ZLabel CountryLabel;
		protected Enterprise.ZArchitecture.ZLabel CountryValueLabel;
		ZArchitecture.GUI.ZButton SetSelectOrganisationButton;
		Enterprise.ZArchitecture.GUI.ZPanel mainTopPanel1;
		Enterprise.ZArchitecture.GUI.ZPanel mainBottomPanel2;
		Enterprise.ZArchitecture.GUI.ZPanel additionalLicenceDetailsPanel;
		ZArchitecture.GUI.ZGroupBox adicionalLicenceDetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel organisationDetailsPanel;
		Enterprise.ZArchitecture.GUI.ZPanel masterOrganisationPanel;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox WebAccessOrgGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZToolStrip saveToolStrip;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox ShowFiltersLicenceDatabaseCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox ShowFiltersOrganisationCheckBox;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZPanel OrganisationDetailsTopPanel;
		Enterprise.ZArchitecture.GUI.ZPanel OrganisationDetailsGridPanel;
		Enterprise.ZArchitecture.GUI.ZPanel OrganisationDetailsBottomPanel;
		Enterprise.ZArchitecture.GUI.ZPanel licenceDatabaseBottomPanel;
		Enterprise.ZArchitecture.GUI.ZPanel licenceDatabaseTopPanel;
		Enterprise.ZArchitecture.ZLabel ProductLabel;
		protected Enterprise.ZArchitecture.ZLabel ProductValueLabel;
		Enterprise.ZArchitecture.ZLabel SystemIdLabel;
		protected Enterprise.ZArchitecture.ZLabel SystemIdValueLabel;
		private ZArchitecture.ZLabel TenantIdLabel;
		protected ZArchitecture.ZLabel TenantIdValueLabel;
		Enterprise.ZArchitecture.ZLabel BusinessRegoNoLabel;
		protected Enterprise.ZArchitecture.ZLabel BusinessRegoNoValueLabel;
		Enterprise.ZArchitecture.ZLabel EnterpriseCodeLabel;
		protected Enterprise.ZArchitecture.ZLabel EnterpriseCodeValueLabel;
		Enterprise.ZArchitecture.ZLabel ServerCodeLabel;
		protected Enterprise.ZArchitecture.ZLabel ServerCodeValueLabel;
		Enterprise.ZArchitecture.ZLabel CargowiseCompanyCodeLabel;
		protected Enterprise.ZArchitecture.ZLabel CargowiseCompanyCodeValueLabel;
		Enterprise.ZArchitecture.ZLabel DatabaseNumberLabel;
		protected Enterprise.ZArchitecture.ZLabel DatabaseNumberValueLabel;
		Enterprise.ZArchitecture.ZLabel InfoExpiresLabel;
		protected Enterprise.ZArchitecture.ZLabel InfoExpiresValueLabel;
		ZArchitecture.GUI.ZToolStrip LicenceDatabaseToolStrip;
		protected ZArchitecture.GUI.ZToolStrip OrganisationToolStrip;
	}
}
