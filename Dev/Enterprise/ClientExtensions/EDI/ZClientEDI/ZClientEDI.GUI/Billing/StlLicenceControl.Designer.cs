namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class StlLicenceControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.SettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProductionServiceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.ServerCode = new Enterprise.ZArchitecture.ZTextBox();
			this.settingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.showExpiredSettingsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.settingsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.newButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.settingDetail = new Enterprise.Client.EDI.Billing.GUI.LicenceSettingDetailControl();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.priceCurrencyBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.companyBillingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.priceLinkGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.showExpiredPricesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.priceLinkGrid = new Enterprise.ZArchitecture.ZGrid();
			this.pricesSettingsSplitter = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SettingsGrid)).BeginInit();
			this.SettingsGrid.SuspendLayout();
			this.settingsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.settingsSplitContainer)).BeginInit();
			this.settingsSplitContainer.Panel1.SuspendLayout();
			this.settingsSplitContainer.Panel2.SuspendLayout();
			this.settingsSplitContainer.SuspendLayout();
			this.settingDetail.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.priceCurrencyBox.SuspendLayout();
			this.priceLinkGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.priceLinkGrid)).BeginInit();
			this.priceLinkGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pricesSettingsSplitter)).BeginInit();
			this.pricesSettingsSplitter.Panel1.SuspendLayout();
			this.pricesSettingsSplitter.Panel2.SuspendLayout();
			this.pricesSettingsSplitter.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader);
			// 
			// SettingsGrid
			// 
			this.SettingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SettingsGrid, "Database.LicenceSettingsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)).LS9_ValidFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)).LS9_ValidTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)).TypeDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)).Summary)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)).LS9_Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)).LS9_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)).SystemCreateTimeLocal)));
			this.SettingsGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.Caption = "Valid From";
			zDateEditColumnStyleInfo1.ColumnName = "LS9_ValidFrom";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Valid To";
			zDateEditColumnStyleInfo2.ColumnName = "LS9_ValidTo";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "Type";
			zTextBoxColumnStyleInfo1.ColumnName = "TypeDesc";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Summary";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Comment";
			zTextBoxColumnStyleInfo3.ColumnName = "LS9_Comment";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.Caption = "Created By";
			zTextBoxColumnStyleInfo4.ColumnName = "LS9_SystemCreateUser";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.Caption = "Created";
			zDateEditColumnStyleInfo3.ColumnName = "SystemCreateTimeLocal";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SettingsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SettingsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SettingsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.SettingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SettingsGrid.GridId = "c38892d1-d709-4ed1-9824-ab04dc44c46d";
			this.SettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SettingsGrid.LayoutKey = "SettingsGrid";
			this.SettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SettingsGrid.Name = "SettingsGrid";
			this.SettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 165, true);
			this.SettingsGrid.TabIndex = 0;
			// 
			// ProductionServiceLabel
			// 
			this.ProductionServiceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ProductionServiceLabel.ForeColor = System.Drawing.Color.Red;
			this.ProductionServiceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 3, true);
			this.ProductionServiceLabel.Name = "ProductionServiceLabel";
			this.ProductionServiceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.ProductionServiceLabel.TabIndex = 26;
			this.ProductionServiceLabel.Text = "Not a production database";
			// 
			// zLabel9
			// 
			this.zLabel9.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.zLabel9.TabIndex = 25;
			this.zLabel9.Text = "Database Code:";
			this.zLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ServerCode
			// 
			this.BindingSource.SetBindingMember(this.ServerCode, "Database.LD_ServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LD_ServerCode)));
			this.ServerCode.CaptionResourceString = null;
			this.ServerCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 3, true);
			this.ServerCode.Name = "ServerCode";
			this.ServerCode.ReadOnly = true;
			this.ServerCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ServerCode.TabIndex = 24;
			// 
			// settingsGroupBox
			// 
			this.settingsGroupBox.Controls.Add(this.showExpiredSettingsCheckBox);
			this.settingsGroupBox.Controls.Add(this.settingsSplitContainer);
			this.settingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.settingsGroupBox.Name = "settingsGroupBox";
			this.settingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 184, true);
			this.settingsGroupBox.TabIndex = 27;
			this.settingsGroupBox.TabStop = false;
			this.settingsGroupBox.Text = "Customer Settings";
			// 
			// showExpiredSettingsCheckBox
			// 
			this.showExpiredSettingsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showExpiredSettingsCheckBox, "Database.ShowExpiredLicenceSettings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.ShowExpiredLicenceSettings)));
			this.showExpiredSettingsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showExpiredSettingsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, -1, true);
			this.showExpiredSettingsCheckBox.Name = "showExpiredSettingsCheckBox";
			this.showExpiredSettingsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 17, true);
			this.showExpiredSettingsCheckBox.TabIndex = 0;
			this.showExpiredSettingsCheckBox.Text = "Show Expired";
			this.showExpiredSettingsCheckBox.UseVisualStyleBackColor = true;
			// 
			// settingsSplitContainer
			// 
			this.settingsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.settingsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.settingsSplitContainer.Name = "settingsSplitContainer";
			// 
			// settingsSplitContainer.Panel1
			// 
			this.settingsSplitContainer.Panel1.Controls.Add(this.SettingsGrid);
			// 
			// settingsSplitContainer.Panel2
			// 
			this.settingsSplitContainer.Panel2.Controls.Add(this.newButton);
			this.settingsSplitContainer.Panel2.Controls.Add(this.settingDetail);
			this.settingsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 165, true);
			this.settingsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(347);
			this.settingsSplitContainer.TabIndex = 2;
			// 
			// newButton
			// 
			this.newButton.IsCaptionOverridden = true;
			this.newButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.newButton.Name = "newButton";
			this.newButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.newButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.newButton.TabIndex = 4;
			this.newButton.Text = "New...";
			this.newButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.newButton.ToolTipCaption = null;
			this.newButton.UseVisualStyleBackColor = true;
			this.newButton.Click += new System.EventHandler(this.newButton_Click);
			// 
			// settingDetail
			// 
			this.settingDetail.AllowDrop = true;
			this.settingDetail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.settingDetail, "Database.LicenceSettingsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((Enterprise.Client.EDI.Billing.Business.EdiLicenceSetting)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LicenceSettingsForBinding)).SyncRoot)))));
			this.settingDetail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.settingDetail.Name = "settingDetail";
			this.settingDetail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 137, true);
			this.settingDetail.TabIndex = 1;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.priceCurrencyBox);
			this.topPanel.Controls.Add(this.companyBillingCheckBox);
			this.topPanel.Controls.Add(this.zLabel9);
			this.topPanel.Controls.Add(this.ServerCode);
			this.topPanel.Controls.Add(this.ProductionServiceLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 25, true);
			this.topPanel.TabIndex = 27;
			// 
			// priceCurrencyBox
			// 
			this.priceCurrencyBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.priceCurrencyBox, "LA_RX_NKPriceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).LA_RX_NKPriceCurrency)));
			this.priceCurrencyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 3, true);
			this.priceCurrencyBox.Name = "priceCurrencyBox";
			this.priceCurrencyBox.PreBoundMaxLength = 3;
			this.priceCurrencyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.priceCurrencyBox.TabIndex = 29;
			// 
			// companyBillingCheckBox
			// 
			this.companyBillingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyBillingCheckBox, "Database.LD_IsBilledPerCompany");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.LD_IsBilledPerCompany)));
			this.companyBillingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.companyBillingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 4, true);
			this.companyBillingCheckBox.Name = "companyBillingCheckBox";
			this.companyBillingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.companyBillingCheckBox.TabIndex = 27;
			this.companyBillingCheckBox.Text = "Per Company Billing";
			this.companyBillingCheckBox.UseVisualStyleBackColor = true;
			// 
			// priceLinkGroupBox
			// 
			this.priceLinkGroupBox.Controls.Add(this.showExpiredPricesCheckBox);
			this.priceLinkGroupBox.Controls.Add(this.priceLinkGrid);
			this.priceLinkGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.priceLinkGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.priceLinkGroupBox.Name = "priceLinkGroupBox";
			this.priceLinkGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 111, true);
			this.priceLinkGroupBox.TabIndex = 28;
			this.priceLinkGroupBox.TabStop = false;
			this.priceLinkGroupBox.Text = "Prices";
			// 
			// showExpiredPricesCheckBox
			// 
			this.showExpiredPricesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.showExpiredPricesCheckBox, "Database.ShowExpiredPriceHeaderLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.ShowExpiredPriceHeaderLinks)));
			this.showExpiredPricesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showExpiredPricesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, -1, true);
			this.showExpiredPricesCheckBox.Name = "showExpiredPricesCheckBox";
			this.showExpiredPricesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 17, true);
			this.showExpiredPricesCheckBox.TabIndex = 0;
			this.showExpiredPricesCheckBox.Text = "Show Expired";
			this.showExpiredPricesCheckBox.UseVisualStyleBackColor = true;
			// 
			// priceLinkGrid
			// 
			this.priceLinkGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.priceLinkGrid, "Database.PriceHeaderLinksForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_ValidFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_ValidTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PriceHeaderVersion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_VolumeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_VolumePercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_CorePackCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_CoreUpliftPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.EdiPriceHeaderLink)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader)(null)).Database.PriceHeaderLinksForBinding)).SyncRoot)).PHL_SystemCreateTimeUtc)));
			this.priceLinkGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo4.Caption = "Valid From";
			zDateEditColumnStyleInfo4.ColumnName = "PHL_ValidFrom";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.Caption = "Valid To";
			zDateEditColumnStyleInfo5.ColumnName = "PHL_ValidTo";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.Caption = "Version";
			zDropEditColumnStyleInfo1.ColumnName = "PriceHeaderVersion";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.Caption = "Currency";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "PHL_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.Caption = "Volume";
			zDropEditColumnStyleInfo2.ColumnName = "PHL_VolumeCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Volume %";
			zCalcEditColumnStyleInfo1.ColumnName = "PHL_VolumePercent";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.Caption = "Core Pack";
			zDropEditColumnStyleInfo3.ColumnName = "PHL_CorePackCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Core Uplift %";
			zCalcEditColumnStyleInfo2.ColumnName = "PHL_CoreUpliftPercent";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Created By";
			zTextBoxColumnStyleInfo5.ColumnName = "PHL_SystemCreateUser";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo6.Caption = "Created";
			zDateEditColumnStyleInfo6.ColumnName = "PHL_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.priceLinkGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.priceLinkGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.priceLinkGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.priceLinkGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.priceLinkGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.priceLinkGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.priceLinkGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.priceLinkGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.priceLinkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.priceLinkGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.priceLinkGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.priceLinkGrid.GridId = "18623672-2fff-4195-b9a3-1ad5b1a4c5eb";
			this.priceLinkGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.priceLinkGrid.LayoutKey = "priceLinkGrid";
			this.priceLinkGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.priceLinkGrid.Name = "priceLinkGrid";
			this.priceLinkGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 92, true);
			this.priceLinkGrid.TabIndex = 0;
			// 
			// pricesSettingsSplitter
			// 
			this.pricesSettingsSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pricesSettingsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.pricesSettingsSplitter.Name = "pricesSettingsSplitter";
			this.pricesSettingsSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// pricesSettingsSplitter.Panel1
			// 
			this.pricesSettingsSplitter.Panel1.Controls.Add(this.priceLinkGroupBox);
			// 
			// pricesSettingsSplitter.Panel2
			// 
			this.pricesSettingsSplitter.Panel2.Controls.Add(this.settingsGroupBox);
			this.pricesSettingsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 299, true);
			this.pricesSettingsSplitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(111);
			this.pricesSettingsSplitter.TabIndex = 29;
			// 
			// StlLicenceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.pricesSettingsSplitter);
			this.Controls.Add(this.topPanel);
			this.Name = "StlLicenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 324, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SettingsGrid)).EndInit();
			this.SettingsGrid.ResumeLayout(false);
			this.SettingsGrid.PerformLayout();
			this.settingsGroupBox.ResumeLayout(false);
			this.settingsGroupBox.PerformLayout();
			this.settingsSplitContainer.Panel1.ResumeLayout(false);
			this.settingsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.settingsSplitContainer)).EndInit();
			this.settingsSplitContainer.ResumeLayout(false);
			this.settingsSplitContainer.PerformLayout();
			this.settingDetail.ResumeLayout(true);
			this.settingDetail.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.priceCurrencyBox.ResumeLayout(true);
			this.priceCurrencyBox.PerformLayout();
			this.priceLinkGroupBox.ResumeLayout(false);
			this.priceLinkGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.priceLinkGrid)).EndInit();
			this.priceLinkGrid.ResumeLayout(false);
			this.priceLinkGrid.PerformLayout();
			this.pricesSettingsSplitter.Panel1.ResumeLayout(false);
			this.pricesSettingsSplitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pricesSettingsSplitter)).EndInit();
			this.pricesSettingsSplitter.ResumeLayout(false);
			this.pricesSettingsSplitter.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid SettingsGrid;
		private ZArchitecture.ZLabel ProductionServiceLabel;
		private ZArchitecture.ZLabel zLabel9;
		private ZArchitecture.ZTextBox ServerCode;
		private ZArchitecture.GUI.ZGroupBox settingsGroupBox;
		private ZArchitecture.GUI.ZPanel topPanel;
		private ZArchitecture.GUI.ZGroupBox priceLinkGroupBox;
		private ZArchitecture.ZGrid priceLinkGrid;
		private CargoWise.Windows.UI.KSplitContainer pricesSettingsSplitter;
		private LicenceSettingDetailControl settingDetail;
		private CargoWise.Windows.UI.KSplitContainer settingsSplitContainer;
		private ZArchitecture.GUI.ZButton newButton;
		private ZArchitecture.GUI.ZCheckBox companyBillingCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox priceCurrencyBox;
		private ZArchitecture.GUI.ZCheckBox showExpiredSettingsCheckBox;
		private ZArchitecture.GUI.ZCheckBox showExpiredPricesCheckBox;
	}
}
