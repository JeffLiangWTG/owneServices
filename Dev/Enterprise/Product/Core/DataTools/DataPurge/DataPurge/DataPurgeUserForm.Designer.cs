namespace Enterprise.DataPurge
{
	public partial class DataPurgeUserForm
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
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OutputTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.SystemWideTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CompanySpecificTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 536, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 22, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(396);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(397);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DataPurge.DataPurger);
			// 
			// OutputTextBox
			// 
			this.OutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OutputTextBox, "Output");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DataPurge.DataPurger)(null)).Output)));
			this.OutputTextBox.CaptionResourceString = null;
			this.OutputTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OutputTextBox, false);
			this.OutputTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 160, true);
			this.OutputTextBox.Multiline = true;
			this.OutputTextBox.Name = "OutputTextBox";
			this.OutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.OutputTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 343, true);
			this.OutputTextBox.TabIndex = 3;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|1d3c9f31-e45f-4019-b935-a922a3ce6f19", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 508, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainTabControl.Controls.Add(this.SystemWideTabPage);
			this.MainTabControl.Controls.Add(this.CompanySpecificTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(790, 146, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// SystemWideTabPage
			// 
			this.SystemWideTabPage.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|D6E20FF6-445C-4825-8330-A37FC06F7182", "System Wide");
			this.SystemWideTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.SystemWideTabPage.Name = "SystemWideTabPage";
			this.SystemWideTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 122, true);
			this.SystemWideTabPage.TabIndex = 0;
			this.SystemWideTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SystemWideTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DataPurge.DataPurger)(null)).HasNonSystemChageCodesPurgeScript)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DataPurge.DataPurger)(null)).HasNonProxyOrganisationsPurgeScript)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DataPurge.DataPurger)(null)).HasRatingInfoPurgeScript)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DataPurge.DataPurger)(null)).HasTariffInfoPurgeScript)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DataPurge.DataPurger)(null)).HasQuotationsPurgeScript)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DataPurge.DataPurger)(null)).HasProductInfoPurgeScript)));
			// 
			// CompanySpecificTabPage
			// 
			this.CompanySpecificTabPage.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|CAA32927-3417-4BD8-BF18-4AA5DB95099F", "Company Specific");
			this.CompanySpecificTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.CompanySpecificTabPage.Name = "CompanySpecificTabPage";
			this.CompanySpecificTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 122, true);
			this.CompanySpecificTabPage.TabIndex = 1;
			this.CompanySpecificTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CompanySpecificTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DataPurge.DataPurger)(null)).CompanySpecificPk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DataPurge.DataPurger)(null)).NonDemoCompanies)));
			// 
			// DataPurgeUserForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|e5b6c9cd-c0d0-4074-bb29-181943851b9e", "Purge Data");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 558, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OutputTextBox);
			this.DataSourceAssemblyName = "Enterprise.DataPurge";
			this.DataSourceType = typeof(Enterprise.DataPurge.DataPurger);
			this.DataSourceTypeName = "Enterprise.DataPurge.DataPurger";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(768, 431, true);
			this.Name = "DataPurgeUserForm";
			this.ShouldSerializeTabPageMethods = true;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OutputTextBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void SystemWideTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.BaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonSystemChargeCodesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NonProxyOrganisationsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RatingInfoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RunSystemWideButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.QuotationsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ProductCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EnterpriseWideGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RunSystemWideWithoutTransactionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SystemWideTabPage.SuspendLayout();
			this.EnterpriseWideGroupBox.SuspendLayout();
			this.SystemWideTabPage.Controls.Add(this.EnterpriseWideGroupBox);
			// 
			// BaseCheckBox
			// 
			this.BaseCheckBox.AutoSize = true;
			this.BaseCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|59a094f6-cd99-4b81-a4d3-9369d5763ba4", "Operations && Transactions");
			this.BaseCheckBox.Checked = true;
			this.BaseCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.BaseCheckBox.Enabled = false;
			this.BaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 20, true);
			this.BaseCheckBox.Name = "BaseCheckBox";
			this.BaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 17, true);
			this.BaseCheckBox.TabIndex = 0;
			// 
			// NonSystemChargeCodesCheckBox
			// 
			this.NonSystemChargeCodesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NonSystemChargeCodesCheckBox, "HasNonSystemChageCodesPurgeScript");
			this.NonSystemChargeCodesCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|13a0e61b-bf58-4074-b07a-fbd4591ee26a", "Non System Charge Codes");
			this.NonSystemChargeCodesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NonSystemChargeCodesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 42, true);
			this.NonSystemChargeCodesCheckBox.Name = "NonSystemChargeCodesCheckBox";
			this.NonSystemChargeCodesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.NonSystemChargeCodesCheckBox.TabIndex = 5;
			// 
			// NonProxyOrganisationsCheckBox
			// 
			this.NonProxyOrganisationsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.NonProxyOrganisationsCheckBox, "HasNonProxyOrganisationsPurgeScript");
			this.NonProxyOrganisationsCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|7844adae-b1a1-41a1-85a2-76c12fa43319", "Non Proxy Organizations");
			this.NonProxyOrganisationsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NonProxyOrganisationsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 20, true);
			this.NonProxyOrganisationsCheckBox.Name = "NonProxyOrganisationsCheckBox";
			this.NonProxyOrganisationsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.NonProxyOrganisationsCheckBox.TabIndex = 2;
			// 
			// RatingInfoCheckBox
			// 
			this.RatingInfoCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RatingInfoCheckBox, "HasRatingInfoPurgeScript");
			this.RatingInfoCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|85f1b11e-96ee-4673-8f11-ca03cba6648d", "Rating and Quotations");
			this.RatingInfoCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RatingInfoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 42, true);
			this.RatingInfoCheckBox.Name = "RatingInfoCheckBox";
			this.RatingInfoCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 17, true);
			this.RatingInfoCheckBox.TabIndex = 4;
			// 
			// RunSystemWideButton
			// 
			this.RunSystemWideButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RunSystemWideButton.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|5d8e9bb3-65e5-48e7-9d2e-0f4db7656c56", "Run");
			this.RunSystemWideButton.IsCaptionOverridden = false;
			this.RunSystemWideButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 98, true);
			this.RunSystemWideButton.Name = "RunSystemWideButton";
			this.RunSystemWideButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RunSystemWideButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.RunSystemWideButton.TabIndex = 8;
			this.RunSystemWideButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RunSystemWideButton.ToolTipCaption = null;
			this.RunSystemWideButton.Click += new System.EventHandler(this.RunSystemWideButton_Click);
			// 
			// TariffCheckBox
			// 
			this.TariffCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TariffCheckBox, "HasTariffInfoPurgeScript");
			this.TariffCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|95b69fce-ac93-4760-b041-cfff4a90d566", "Tariff Lookups");
			this.TariffCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TariffCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 42, true);
			this.TariffCheckBox.Name = "TariffCheckBox";
			this.TariffCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			this.TariffCheckBox.TabIndex = 3;
			// 
			// QuotationsCheckBox
			// 
			this.QuotationsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.QuotationsCheckBox, "HasQuotationsPurgeScript");
			this.QuotationsCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|9c576bc8-3b4d-422d-b493-4cc3c8983c7a", "Quotations");
			this.QuotationsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.QuotationsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.QuotationsCheckBox.Name = "QuotationsCheckBox";
			this.QuotationsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 17, true);
			this.QuotationsCheckBox.TabIndex = 6;
			// 
			// ProductCheckBox
			// 
			this.ProductCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ProductCheckBox, "HasProductInfoPurgeScript");
			this.ProductCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|f2100049-1330-45d4-b67f-6704a172e551", "Products");
			this.ProductCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ProductCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 20, true);
			this.ProductCheckBox.Name = "ProductCheckBox";
			this.ProductCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.ProductCheckBox.TabIndex = 1;
			// 
			// EnterpriseWideGroupBox
			// 
			this.EnterpriseWideGroupBox.Controls.Add(this.RunSystemWideWithoutTransactionButton);
			this.EnterpriseWideGroupBox.Controls.Add(this.NonProxyOrganisationsCheckBox);
			this.EnterpriseWideGroupBox.Controls.Add(this.ProductCheckBox);
			this.EnterpriseWideGroupBox.Controls.Add(this.RunSystemWideButton);
			this.EnterpriseWideGroupBox.Controls.Add(this.BaseCheckBox);
			this.EnterpriseWideGroupBox.Controls.Add(this.TariffCheckBox);
			this.EnterpriseWideGroupBox.Controls.Add(this.QuotationsCheckBox);
			this.EnterpriseWideGroupBox.Controls.Add(this.RatingInfoCheckBox);
			this.EnterpriseWideGroupBox.Controls.Add(this.NonSystemChargeCodesCheckBox);
			this.EnterpriseWideGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EnterpriseWideGroupBox, false);
			this.EnterpriseWideGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EnterpriseWideGroupBox.Name = "EnterpriseWideGroupBox";
			this.EnterpriseWideGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 122, true);
			this.EnterpriseWideGroupBox.TabIndex = 1;
			this.EnterpriseWideGroupBox.TabStop = false;
			// 
			// RunSystemWideWithoutTransactionButton
			// 
			this.RunSystemWideWithoutTransactionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RunSystemWideWithoutTransactionButton.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|C89F6FCE-A8A2-4AB3-8B97-D2FB36958E0F", "Run Without Transaction");
			this.RunSystemWideWithoutTransactionButton.IsCaptionOverridden = false;
			this.RunSystemWideWithoutTransactionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 98, true);
			this.RunSystemWideWithoutTransactionButton.Name = "RunSystemWideWithoutTransactionButton";
			this.RunSystemWideWithoutTransactionButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RunSystemWideWithoutTransactionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 21, true);
			this.RunSystemWideWithoutTransactionButton.TabIndex = 9;
			this.RunSystemWideWithoutTransactionButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RunSystemWideWithoutTransactionButton.ToolTipCaption = null;
			this.RunSystemWideWithoutTransactionButton.Click += new System.EventHandler(this.RunSystemWideWithoutTransactionButton_Click);
			this.SystemWideTabPage.PerformLayout();
			this.EnterpriseWideGroupBox.ResumeLayout(false);
			this.EnterpriseWideGroupBox.PerformLayout();
			this.SystemWideTabPage.ResumeLayout(true);
		}

		private void CompanySpecificTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.CompanySpecificGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PurgeCompanyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CompanySpecificAccountingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RunCompanySpecificButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CompanySpecificTabPage.SuspendLayout();
			this.CompanySpecificGroupBox.SuspendLayout();
			this.PurgeCompanyGuidFindBox.SuspendLayout();
			this.CompanySpecificTabPage.Controls.Add(this.CompanySpecificGroupBox);
			// 
			// CompanySpecificGroupBox
			// 
			this.CompanySpecificGroupBox.Controls.Add(this.RunCompanySpecificButton);
			this.CompanySpecificGroupBox.Controls.Add(this.PurgeCompanyGuidFindBox);
			this.CompanySpecificGroupBox.Controls.Add(this.CompanySpecificAccountingCheckBox);
			this.CompanySpecificGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CompanySpecificGroupBox, false);
			this.CompanySpecificGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompanySpecificGroupBox.Name = "CompanySpecificGroupBox";
			this.CompanySpecificGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(783, 122, true);
			this.CompanySpecificGroupBox.TabIndex = 2;
			this.CompanySpecificGroupBox.TabStop = false;
			// 
			// PurgeCompanyGuidFindBox
			// 
			this.PurgeCompanyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PurgeCompanyGuidFindBox, "CompanySpecificPk");
			this.PurgeCompanyGuidFindBox.BindToList = "NonDemoCompanies";
			this.PurgeCompanyGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PurgeCompanyGuidFindBox, false);
			this.PurgeCompanyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 17, true);
			this.PurgeCompanyGuidFindBox.Name = "PurgeCompanyGuidFindBox";
			this.PurgeCompanyGuidFindBox.ShouldResize = true;
			this.PurgeCompanyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 18, true);
			this.PurgeCompanyGuidFindBox.TabIndex = 0;
			// 
			// CompanySpecificAccountingCheckBox
			// 
			this.CompanySpecificAccountingCheckBox.AutoSize = true;
			this.CompanySpecificAccountingCheckBox.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|ffa821ee-1713-43e4-9e1d-0eca5ba064e6", "Accounting Transactions");
			this.CompanySpecificAccountingCheckBox.Checked = true;
			this.CompanySpecificAccountingCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.CompanySpecificAccountingCheckBox.Enabled = false;
			this.CompanySpecificAccountingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompanySpecificAccountingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 18, true);
			this.CompanySpecificAccountingCheckBox.Name = "CompanySpecificAccountingCheckBox";
			this.CompanySpecificAccountingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 17, true);
			this.CompanySpecificAccountingCheckBox.TabIndex = 1;
			// 
			// RunCompanySpecificButton
			// 
			this.RunCompanySpecificButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.RunCompanySpecificButton.CaptionResourceString = Enterprise.DataPurge.Res.GetData("DataPurgeUserForm|5d8e9bb3-65e5-48e7-9d2e-0f4db7656c56", "Run");
			this.RunCompanySpecificButton.IsCaptionOverridden = false;
			this.RunCompanySpecificButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 98, true);
			this.RunCompanySpecificButton.Name = "RunCompanySpecificButton";
			this.RunCompanySpecificButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RunCompanySpecificButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.RunCompanySpecificButton.TabIndex = 3;
			this.RunCompanySpecificButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RunCompanySpecificButton.ToolTipCaption = null;
			this.RunCompanySpecificButton.Click += new System.EventHandler(this.RunCompanySpecificButton_Click);
			this.CompanySpecificTabPage.PerformLayout();
			this.CompanySpecificGroupBox.ResumeLayout(false);
			this.CompanySpecificGroupBox.PerformLayout();
			this.PurgeCompanyGuidFindBox.ResumeLayout(true);
			this.PurgeCompanyGuidFindBox.PerformLayout();
			this.CompanySpecificTabPage.ResumeLayout(true);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox BaseCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonSystemChargeCodesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox NonProxyOrganisationsCheckBox;
		private Enterprise.ZArchitecture.ZTextBox OutputTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox TariffCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ProductCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CompanySpecificGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox PurgeCompanyGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CompanySpecificAccountingCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage SystemWideTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage CompanySpecificTabPage;
		internal ZArchitecture.GUI.ZCheckBox RatingInfoCheckBox;
		internal ZArchitecture.GUI.ZCheckBox QuotationsCheckBox;
		internal ZArchitecture.GUI.ZGroupBox EnterpriseWideGroupBox;
		internal ZArchitecture.GUI.ZButton RunCompanySpecificButton;
		internal ZArchitecture.GUI.ZButton RunSystemWideButton;
		internal ZArchitecture.GUI.ZButton RunSystemWideWithoutTransactionButton;
		internal ZArchitecture.GUI.ZTabControl MainTabControl;

	}
}
