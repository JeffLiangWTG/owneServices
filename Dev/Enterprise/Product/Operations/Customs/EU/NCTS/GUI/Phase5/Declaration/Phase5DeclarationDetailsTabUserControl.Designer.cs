namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DeclarationDetailsTabUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.DynamicTraderDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.TraderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DepartureDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicDepartureDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.DeclarationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicDeclarationDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CustomsOfficesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicCustomsOfficesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.GuaranteesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicGuaranteesUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.SecurityAtDepartureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SecurityAtDepartureDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.DeclarationDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AuthorizationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicAuthorizationsTabUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicSupportingDocumentsTabUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.AdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicAdditionalDocumentsTabUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.PreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicPreviousDocumentsTabUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.CountryOfRoutingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CountryOfRoutingTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5DeclarationCountryOfRoutingTabUserControl();
			this.SupplyChainActorTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicSupplyChainActorTabUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TraderDetailsGroupBox.SuspendLayout();
			this.DepartureDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			this.CustomsOfficesGroupBox.SuspendLayout();
			this.GuaranteesGroupBox.SuspendLayout();
			this.SecurityAtDepartureGroupBox.SuspendLayout();
			this.DeclarationDetailsTabControl.SuspendLayout();
			this.AuthorizationsTabPage.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.AdditionalDocumentsTabPage.SuspendLayout();
			this.PreviousDocumentsTabPage.SuspendLayout();
			this.CountryOfRoutingTabPage.SuspendLayout();
			this.CountryOfRoutingTabUserControl.SuspendLayout();
			this.SupplyChainActorTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// DynamicTraderDetailsPanel
			// 
			this.DynamicTraderDetailsPanel.AllowDrop = true;
			this.DynamicTraderDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicTraderDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicTraderDetailsPanel.Name = "DynamicTraderDetailsPanel";
			this.DynamicTraderDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 631, true);
			this.DynamicTraderDetailsPanel.TabIndex = 0;
			// 
			// TraderDetailsGroupBox
			// 
			this.TraderDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b312521f-d43b-4786-a938-9035f8f25825", "Trader Details");
			this.TraderDetailsGroupBox.Controls.Add(this.DynamicTraderDetailsPanel);
			this.TraderDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.TraderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TraderDetailsGroupBox.Name = "TraderDetailsGroupBox";
			this.TraderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 650, true);
			this.TraderDetailsGroupBox.TabIndex = 0;
			this.TraderDetailsGroupBox.TabStop = false;
			// 
			// DepartureDetailsGroupBox
			// 
			this.DepartureDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("2368D2C7-16A7-4E1C-A16E-278FED92CBC9", "Departure Details");
			this.DepartureDetailsGroupBox.Controls.Add(this.DynamicDepartureDetailsPanel);
			this.DepartureDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 0, true);
			this.DepartureDetailsGroupBox.Name = "DepartureDetailsGroupBox";
			this.DepartureDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 396, true);
			this.DepartureDetailsGroupBox.TabIndex = 1;
			this.DepartureDetailsGroupBox.TabStop = false;
			// 
			// DynamicDepartureDetailsPanel
			//
			this.DynamicDepartureDetailsPanel.AllowDrop = true;
			this.DynamicDepartureDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicDepartureDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicDepartureDetailsPanel.Name = "DynamicDepartureDetailsPanel";
			this.DynamicDepartureDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 377, true);
			this.DynamicDepartureDetailsPanel.TabIndex = 0;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("11590D9A-3AF0-43BF-88E9-06EF3B719F9A", "Declaration Details");
			this.DeclarationDetailsGroupBox.Controls.Add(this.DynamicDeclarationDetailsPanel);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(682, 0, true);
			this.DeclarationDetailsGroupBox.Name = "DeclarationDetailsGroupBox";
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 141, true);
			this.DeclarationDetailsGroupBox.TabIndex = 3;
			this.DeclarationDetailsGroupBox.TabStop = false;
			// 
			// DynamicDeclarationDetailsPanel
			// 
			this.DynamicDeclarationDetailsPanel.AllowDrop = true;
			this.DynamicDeclarationDetailsPanel.AutoScroll = true;
			this.DynamicDeclarationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicDeclarationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicDeclarationDetailsPanel.Name = "DynamicDeclarationDetailsPanel";
			this.DynamicDeclarationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 122, true);
			this.DynamicDeclarationDetailsPanel.TabIndex = 0;
			// 
			// CustomsOfficesGroupBox
			// 
			this.CustomsOfficesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CustomsOfficesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("2ca1b0e8-fefd-46a0-aae5-3b7d4cc6d70f", "Customs Offices");
			this.CustomsOfficesGroupBox.Controls.Add(this.DynamicCustomsOfficesUserControl);
			this.CustomsOfficesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(682, 142, true);
			this.CustomsOfficesGroupBox.Name = "CustomsOfficesGroupBox";
			this.CustomsOfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 199, true);
			this.CustomsOfficesGroupBox.TabIndex = 4;
			this.CustomsOfficesGroupBox.TabStop = false;
			// 
			// DynamicCustomsOfficesUserControl
			// 
			this.DynamicCustomsOfficesUserControl.AllowDrop = true;
			this.DynamicCustomsOfficesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicCustomsOfficesUserControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DynamicCustomsOfficesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicCustomsOfficesUserControl.Name = "DynamicCustomsOfficesUserControl";
			this.DynamicCustomsOfficesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 180, true);
			this.DynamicCustomsOfficesUserControl.TabIndex = 0;
			// 
			// GuaranteesGroupBox
			// 
			this.GuaranteesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.GuaranteesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("AA380E23-2308-49BA-BE7A-00FF68A505DE", "Guarantees");
			this.GuaranteesGroupBox.Controls.Add(this.DynamicGuaranteesUserControl);
			this.GuaranteesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(682, 347, true);
			this.GuaranteesGroupBox.Name = "GuaranteesGroupBox";
			this.GuaranteesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 148, true);
			this.GuaranteesGroupBox.TabIndex = 5;
			this.GuaranteesGroupBox.TabStop = false;
			// 
			// DynamicGuaranteesUserControl
			// 
			this.DynamicGuaranteesUserControl.AllowDrop = true;
			this.DynamicGuaranteesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicGuaranteesUserControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DynamicGuaranteesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicGuaranteesUserControl.Name = "DynamicGuaranteesUserControl";
			this.DynamicGuaranteesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 94, true);
			this.DynamicGuaranteesUserControl.TabIndex = 0;
			// 
			// SecurityAtDepartureGroupBox
			// 
			this.SecurityAtDepartureGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("516C8C64-A001-4DD1-8258-1083C8C2C7F5", "Security");
			this.SecurityAtDepartureGroupBox.Controls.Add(this.SecurityAtDepartureDynamicLayoutPanel);
			this.SecurityAtDepartureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 402, true);
			this.SecurityAtDepartureGroupBox.Name = "SecurityAtDepartureGroupBox";
			this.SecurityAtDepartureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 93, true);
			this.SecurityAtDepartureGroupBox.TabIndex = 2;
			this.SecurityAtDepartureGroupBox.TabStop = false;
			// 
			// SecurityAtDepartureDynamicLayoutPanel
			// 
			this.SecurityAtDepartureDynamicLayoutPanel.AllowDrop = true;
			this.SecurityAtDepartureDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SecurityAtDepartureDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SecurityAtDepartureDynamicLayoutPanel.Name = "SecurityAtDepartureDynamicLayoutPanel";
			this.SecurityAtDepartureDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 74, true);
			this.SecurityAtDepartureDynamicLayoutPanel.TabIndex = 0;
			// 
			// DeclarationDetailsTabControl
			// 
			this.DeclarationDetailsTabControl.Controls.Add(this.AuthorizationsTabPage);
			this.DeclarationDetailsTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.DeclarationDetailsTabControl.Controls.Add(this.AdditionalDocumentsTabPage);
			this.DeclarationDetailsTabControl.Controls.Add(this.PreviousDocumentsTabPage);
			this.DeclarationDetailsTabControl.Controls.Add(this.CountryOfRoutingTabPage);
			this.DeclarationDetailsTabControl.Controls.Add(this.SupplyChainActorTabPage);
			this.DeclarationDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 502, true);
			this.DeclarationDetailsTabControl.Name = "DeclarationDetailsTabControl";
			this.DeclarationDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 144, true);
			this.DeclarationDetailsTabControl.TabIndex = 6;
			// 
			// AuthorizationsTabPage
			// 
			this.AuthorizationsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("2417BC84-5CA0-449A-8FAB-FBF0B5E87326", "Authorizations");
			this.AuthorizationsTabPage.Controls.Add(this.DynamicAuthorizationsTabUserControl);
			this.AuthorizationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AuthorizationsTabPage.Name = "AuthorizationsTabPage";
			this.AuthorizationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.AuthorizationsTabPage.TabIndex = 0;
			this.AuthorizationsTabPage.UseVisualStyleBackColor = true;
			// 
			// DynamicAuthorizationsTabUserControl
			// 
			this.DynamicAuthorizationsTabUserControl.AllowDrop = true;
			this.DynamicAuthorizationsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicAuthorizationsTabUserControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DynamicAuthorizationsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicAuthorizationsTabUserControl.Name = "DynamicAuthorizationsTabUserControl";
			this.DynamicAuthorizationsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.DynamicAuthorizationsTabUserControl.TabIndex = 0;
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("FD6BFF11-0ADD-4A75-A921-42DFFD76AF25", "Supporting Documents");
			this.SupportingDocumentsTabPage.Controls.Add(this.DynamicSupportingDocumentsTabUserControl);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.SupportingDocumentsTabPage.TabIndex = 1;
			this.SupportingDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// DynamicSupportingDocumentsTabUserControl
			// 
			this.DynamicSupportingDocumentsTabUserControl.AllowDrop = true;
			this.DynamicSupportingDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicSupportingDocumentsTabUserControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DynamicSupportingDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DynamicSupportingDocumentsTabUserControl.Name = "DynamicSupportingDocumentsTabUserControl";
			this.DynamicSupportingDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 111, true);
			this.DynamicSupportingDocumentsTabUserControl.TabIndex = 0;
			// 
			// AdditionalDocumentsTabPage
			// 
			this.AdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("6fd57320-2a31-48b6-ab0e-8953c86773c4", "Additional Documents");
			this.AdditionalDocumentsTabPage.Controls.Add(this.DynamicAdditionalDocumentsTabUserControl);
			this.AdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalDocumentsTabPage.Name = "AdditionalDocumentsTabPage";
			this.AdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.AdditionalDocumentsTabPage.TabIndex = 3;
			this.AdditionalDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// DynamicAdditionalDocumentsTabUserControl
			// 
			this.DynamicAdditionalDocumentsTabUserControl.AllowDrop = true;
			this.DynamicAdditionalDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicAdditionalDocumentsTabUserControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DynamicAdditionalDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicAdditionalDocumentsTabUserControl.Name = "DynamicAdditionalDocumentsTabUserControl";
			this.DynamicAdditionalDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.DynamicAdditionalDocumentsTabUserControl.TabIndex = 0;
			// 
			// PreviousDocumentsTabPage
			// 
			this.PreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("D15DFDE8-1C25-44D0-AB2F-FE065DD137DF", "Previous Documents");
			this.PreviousDocumentsTabPage.Controls.Add(this.DynamicPreviousDocumentsTabUserControl);
			this.PreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviousDocumentsTabPage.Name = "PreviousDocumentsTabPage";
			this.PreviousDocumentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.PreviousDocumentsTabPage.TabIndex = 2;
			this.PreviousDocumentsTabPage.UseVisualStyleBackColor = true;
			// 
			// DynamicPreviousDocumentsTabUserControl
			// 
			this.DynamicPreviousDocumentsTabUserControl.AllowDrop = true;
			this.DynamicPreviousDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicPreviousDocumentsTabUserControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DynamicPreviousDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DynamicPreviousDocumentsTabUserControl.Name = "DynamicPreviousDocumentsTabUserControl";
			this.DynamicPreviousDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 111, true);
			this.DynamicPreviousDocumentsTabUserControl.TabIndex = 0;
			// 
			// CountryOfRoutingTabPage
			// 
			this.CountryOfRoutingTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E08EF2CB-99C5-4FCB-B611-86C7EF16D808", "Country/Region of Routing");
			this.CountryOfRoutingTabPage.Controls.Add(this.CountryOfRoutingTabUserControl);
			this.CountryOfRoutingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CountryOfRoutingTabPage.Name = "CountryOfRoutingTabPage";
			this.CountryOfRoutingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CountryOfRoutingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.CountryOfRoutingTabPage.TabIndex = 4;
			this.CountryOfRoutingTabPage.UseVisualStyleBackColor = true;
			// 
			// CountryOfRoutingTabUserControl
			// 
			this.CountryOfRoutingTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfRoutingTabUserControl, "CountriesOfRouting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.CountryOfRoutingCollection<Enterprise.Customs.EU.NCTS.Business.CountryOfRouting>)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).CountriesOfRouting)));
			this.CountryOfRoutingTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CountryOfRoutingTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CountryOfRoutingTabUserControl.Name = "CountryOfRoutingTabUserControl";
			this.CountryOfRoutingTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 111, true);
			this.CountryOfRoutingTabUserControl.TabIndex = 0;
			// 
			// SupplyChainActorTabPage
			// 
			this.SupplyChainActorTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("900bbf32-aac7-41aa-8be8-6a712a19f32f", "Supply Chain Actors");
			this.SupplyChainActorTabPage.Controls.Add(this.DynamicSupplyChainActorTabUserControl);
			this.SupplyChainActorTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupplyChainActorTabPage.Name = "SupplyChainActorTabPage";
			this.SupplyChainActorTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SupplyChainActorTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 117, true);
			this.SupplyChainActorTabPage.TabIndex = 5;
			this.SupplyChainActorTabPage.UseVisualStyleBackColor = true;
			// 
			// DynamicSupplyChainActorTabUserControl
			// 
			this.DynamicSupplyChainActorTabUserControl.AllowDrop = true;
			this.DynamicSupplyChainActorTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicSupplyChainActorTabUserControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.DynamicSupplyChainActorTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DynamicSupplyChainActorTabUserControl.Name = "DynamicSupplyChainActorTabUserControl";
			this.DynamicSupplyChainActorTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(959, 111, true);
			this.DynamicSupplyChainActorTabUserControl.TabIndex = 0;
			// 
			// Phase5DeclarationDetailsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsOfficesGroupBox);
			this.Controls.Add(this.SecurityAtDepartureGroupBox);
			this.Controls.Add(this.TraderDetailsGroupBox);
			this.Controls.Add(this.DepartureDetailsGroupBox);
			this.Controls.Add(this.DeclarationDetailsGroupBox);
			this.Controls.Add(this.GuaranteesGroupBox);
			this.Controls.Add(this.DeclarationDetailsTabControl);
			this.Name = "Phase5DeclarationDetailsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1248, 650, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TraderDetailsGroupBox.ResumeLayout(false);
			this.TraderDetailsGroupBox.PerformLayout();
			this.DepartureDetailsGroupBox.ResumeLayout(false);
			this.DepartureDetailsGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			this.CustomsOfficesGroupBox.ResumeLayout(false);
			this.CustomsOfficesGroupBox.PerformLayout();
			this.GuaranteesGroupBox.ResumeLayout(false);
			this.GuaranteesGroupBox.PerformLayout();
			this.SecurityAtDepartureGroupBox.ResumeLayout(false);
			this.SecurityAtDepartureGroupBox.PerformLayout();
			this.DeclarationDetailsTabControl.ResumeLayout(false);
			this.DeclarationDetailsTabControl.PerformLayout();
			this.AuthorizationsTabPage.ResumeLayout(false);
			this.AuthorizationsTabPage.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.AdditionalDocumentsTabPage.ResumeLayout(false);
			this.AdditionalDocumentsTabPage.PerformLayout();
			this.PreviousDocumentsTabPage.ResumeLayout(false);
			this.PreviousDocumentsTabPage.PerformLayout();
			this.CountryOfRoutingTabPage.ResumeLayout(false);
			this.CountryOfRoutingTabPage.PerformLayout();
			this.CountryOfRoutingTabUserControl.ResumeLayout(true);
			this.CountryOfRoutingTabUserControl.PerformLayout();
			this.SupplyChainActorTabPage.ResumeLayout(false);
			this.SupplyChainActorTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox TraderDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicTraderDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DepartureDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicDepartureDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationDetailsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicDeclarationDetailsPanel;
		internal ZArchitecture.GUI.ZGroupBox CustomsOfficesGroupBox;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DynamicCustomsOfficesUserControl;
		internal ZArchitecture.GUI.ZGroupBox GuaranteesGroupBox;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DynamicGuaranteesUserControl;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SecurityAtDepartureGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel SecurityAtDepartureDynamicLayoutPanel;
		internal ZArchitecture.GUI.ZTabControl DeclarationDetailsTabControl;
		internal ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DynamicSupportingDocumentsTabUserControl;
		internal ZArchitecture.GUI.ZTabPage PreviousDocumentsTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DynamicPreviousDocumentsTabUserControl;
		protected internal ZArchitecture.GUI.ZTabPage CountryOfRoutingTabPage;
		internal Phase5DeclarationCountryOfRoutingTabUserControl CountryOfRoutingTabUserControl;
		protected internal ZArchitecture.GUI.ZTabPage SupplyChainActorTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DynamicSupplyChainActorTabUserControl;
		internal ZArchitecture.GUI.ZTabPage AdditionalDocumentsTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DynamicAdditionalDocumentsTabUserControl;
		internal ZArchitecture.GUI.ZTabPage AuthorizationsTabPage;
		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl DynamicAuthorizationsTabUserControl;
	}
}

