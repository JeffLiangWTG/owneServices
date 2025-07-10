
namespace Enterprise.Accounting.GUI.ComplianceReport.ZMD
{
	partial class ZMGermanyForm
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
		protected new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            this.reportDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.reportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.companyNameLabel = new Enterprise.ZArchitecture.ZLabel();
            this.dateFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.dateToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.taxReturnPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.bottomSectionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.buttonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.markAsSubmittedButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.generateButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.reportLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.reportLinesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.middleSectionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.customsCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.registrationIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.senderIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.versionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.versionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.reportDetailsPanel.SuspendLayout();
            this.reportGroupBox.SuspendLayout();
            this.dateFromDateEdit.SuspendLayout();
            this.dateToDateEdit.SuspendLayout();
            this.taxReturnPanel.SuspendLayout();
            this.bottomSectionPanel.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            this.reportLinesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.reportLinesGrid)).BeginInit();
            this.reportLinesGrid.SuspendLayout();
            this.middleSectionPanel.SuspendLayout();
            this.customsCodesGroupBox.SuspendLayout();
            this.versionGroupBox.SuspendLayout();
            this.versionDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 521, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport);
            // 
            // reportDetailsPanel
            // 
            this.reportDetailsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reportDetailsPanel.Controls.Add(this.reportGroupBox);
            this.reportDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.reportDetailsPanel.Name = "reportDetailsPanel";
            this.reportDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 104, true);
            this.reportDetailsPanel.TabIndex = 16;
            // 
            // reportGroupBox
            // 
            this.reportGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reportGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("09839145-4c88-40af-b640-998c53e6ebe7", "Company and Report details");
            this.reportGroupBox.Controls.Add(this.companyNameLabel);
            this.reportGroupBox.Controls.Add(this.dateFromDateEdit);
            this.reportGroupBox.Controls.Add(this.dateToDateEdit);
            this.reportGroupBox.Controls.Add(this.descriptionTextBox);
            this.reportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
            this.reportGroupBox.Name = "reportGroupBox";
            this.reportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 96, true);
            this.reportGroupBox.TabIndex = 2;
            this.reportGroupBox.TabStop = false;
            // 
            // companyNameLabel
            // 
            this.BindingSource.SetBindingMember(this.companyNameLabel, "ComplianceReport.Company.GC_Name");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).ComplianceReport.Company.GC_Name)));
            this.companyNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyNameLabel, false);
            this.companyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
            this.companyNameLabel.Name = "companyNameLabel";
            this.companyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 17, true);
            this.companyNameLabel.TabIndex = 2;
            this.companyNameLabel.Text = "<Company Name>";
            // 
            // dateFromDateEdit
            // 
            this.dateFromDateEdit.AllowDrop = true;
            this.dateFromDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateFromDateEdit, "ComplianceReport.ACR_DateFrom");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).ComplianceReport.ACR_DateFrom)));
            this.dateFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 36, true);
            this.dateFromDateEdit.Name = "dateFromDateEdit";
            this.dateFromDateEdit.TabIndex = 3;
            // 
            // dateToDateEdit
            // 
            this.dateToDateEdit.AllowDrop = true;
            this.dateToDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateToDateEdit, "ComplianceReport.ACR_DateTo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).ComplianceReport.ACR_DateTo)));
            this.dateToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 36, true);
            this.dateToDateEdit.Name = "dateToDateEdit";
            this.dateToDateEdit.TabIndex = 4;
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.descriptionTextBox, "ComplianceReport.ACR_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).ComplianceReport.ACR_Description)));
            this.descriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bfa77bb8-317e-4969-92e2-7c97ee6ab76a", "Description");
            this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 62, true);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.ReadOnly = true;
            this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(638, 20, true);
            this.descriptionTextBox.TabIndex = 5;
            // 
            // taxReturnPanel
            // 
            this.taxReturnPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.taxReturnPanel.Controls.Add(this.bottomSectionPanel);
            this.taxReturnPanel.Controls.Add(this.middleSectionPanel);
            this.taxReturnPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 110, true);
            this.taxReturnPanel.Name = "taxReturnPanel";
            this.taxReturnPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(725, 410, true);
            this.taxReturnPanel.TabIndex = 17;
            // 
            // bottomSectionPanel
            // 
            this.bottomSectionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bottomSectionPanel.Controls.Add(this.buttonsPanel);
            this.bottomSectionPanel.Controls.Add(this.reportLinesGroupBox);
            this.bottomSectionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 92, true);
            this.bottomSectionPanel.Name = "bottomSectionPanel";
            this.bottomSectionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 312, true);
            this.bottomSectionPanel.TabIndex = 1;
            // 
            // buttonsPanel
            // 
            this.buttonsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonsPanel.Controls.Add(this.closeButton);
            this.buttonsPanel.Controls.Add(this.markAsSubmittedButton);
            this.buttonsPanel.Controls.Add(this.generateButton);
            this.buttonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 279, true);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 32, true);
            this.buttonsPanel.TabIndex = 13;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e0adc907-9fde-465b-854a-5b4aa1b657b9", "Close");
            this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(583, 3, true);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
            this.closeButton.TabIndex = 18;
            this.closeButton.ToolTipCaption = null;
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // markAsSubmittedButton
            // 
            this.markAsSubmittedButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d364f1f8-cee1-4b9a-9d90-1ba2dbd920a8", "Mark as Submitted");
            this.markAsSubmittedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 3, true);
            this.markAsSubmittedButton.Name = "markAsSubmittedButton";
            this.markAsSubmittedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
            this.markAsSubmittedButton.TabIndex = 17;
            this.markAsSubmittedButton.ToolTipCaption = null;
            this.markAsSubmittedButton.UseVisualStyleBackColor = true;
            this.markAsSubmittedButton.Click += new System.EventHandler(this.markAsSubmittedButton_Click);
            // 
            // generateButton
            // 
            this.generateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("70a0a35d-1b33-4cf4-a38b-5a233bb17742", "Generate ELMA5");
            this.generateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
            this.generateButton.TabIndex = 16;
            this.generateButton.ToolTipCaption = null;
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // reportLinesGroupBox
            // 
            this.reportLinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.reportLinesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1b000475-0c35-4c03-90b8-8061646cf377", "Report lines");
            this.reportLinesGroupBox.Controls.Add(this.reportLinesGrid);
            this.reportLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.reportLinesGroupBox.Name = "reportLinesGroupBox";
            this.reportLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 272, true);
            this.reportLinesGroupBox.TabIndex = 12;
            this.reportLinesGroupBox.TabStop = false;
            // 
            // reportLinesGrid
            // 
            this.reportLinesGrid.AllowNavigation = false;
            this.reportLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.reportLinesGrid, "GridData");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccTaxReturnLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)).SyncRoot)).TaxReturn.ATR_Version)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccTaxReturnLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)).SyncRoot)).ARL_RN_NKCountryCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccTaxReturnLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)).SyncRoot)).ARL_OrgRegNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccTaxReturnLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)).SyncRoot)).ARL_Comment)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccTaxReturnLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)).SyncRoot)).ARL_OrgName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.AccTaxReturnLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)).SyncRoot)).ARL_City)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.AccTaxReturnLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).GridData)).SyncRoot)).ARL_TotalAmountIncludingTax)));
            this.reportLinesGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ebe6bdc5-0192-4bc9-8e6b-bba476bb2749", "Version");
            zCalcEditColumnStyleInfo3.ColumnName = "TaxReturn+ATR_Version";
            zCalcEditColumnStyleInfo3.IsReadOnly = true;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5307334a-dc3b-48b0-b579-2d6e1b78af5e", "Country/Region");
            zTextBoxColumnStyleInfo6.ColumnName = "ARL_RN_NKCountryCode";
            zTextBoxColumnStyleInfo6.IsReadOnly = true;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e63fa537-1e5b-4279-acd4-141bf37bbbe0", "Registration No.");
            zTextBoxColumnStyleInfo7.ColumnName = "ARL_OrgRegNo";
            zTextBoxColumnStyleInfo7.IsReadOnly = true;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ca5b1dd3-9d9f-4759-89a7-738067e40d3e", "Org. Code");
            zTextBoxColumnStyleInfo8.ColumnName = "ARL_Comment";
            zTextBoxColumnStyleInfo8.IsReadOnly = true;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8d07563e-a0ef-4c6f-8170-f244a461af9d", "Org. Name");
            zTextBoxColumnStyleInfo9.ColumnName = "ARL_OrgName";
            zTextBoxColumnStyleInfo9.IsReadOnly = true;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7de2fe68-fe19-4e8e-a9fa-06d79c329536", "Org. City");
            zTextBoxColumnStyleInfo10.ColumnName = "ARL_City";
            zTextBoxColumnStyleInfo10.IsReadOnly = true;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f8d04951-4afa-4ff0-b44f-29d65a9396c5", "Amount");
            zCalcEditColumnStyleInfo4.ColumnName = "ARL_TotalAmountIncludingTax";
            zCalcEditColumnStyleInfo4.IsReadOnly = true;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.reportLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.reportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.reportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.reportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.reportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.reportLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.reportLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.reportLinesGrid.GridId = "f8ca4fc2-de48-43eb-8d4a-4af1ba11b922";
            this.reportLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.reportLinesGrid.LayoutKey = "reportLinesGrid";
            this.reportLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
            this.reportLinesGrid.Name = "reportLinesGrid";
            this.reportLinesGrid.ReadOnly = true;
            this.reportLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 248, true);
            this.reportLinesGrid.TabIndex = 12;
            // 
            // middleSectionPanel
            // 
            this.middleSectionPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.middleSectionPanel.Controls.Add(this.customsCodesGroupBox);
            this.middleSectionPanel.Controls.Add(this.versionGroupBox);
            this.middleSectionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 7, true);
            this.middleSectionPanel.Name = "middleSectionPanel";
            this.middleSectionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 80, true);
            this.middleSectionPanel.TabIndex = 0;
            // 
            // customsCodesGroupBox
            // 
            this.customsCodesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customsCodesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7bf5a3e0-652a-4990-9581-a63617fff3d3", "Registration details");
            this.customsCodesGroupBox.Controls.Add(this.registrationIDTextBox);
            this.customsCodesGroupBox.Controls.Add(this.senderIDTextBox);
            this.customsCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 3, true);
            this.customsCodesGroupBox.Name = "customsCodesGroupBox";
            this.customsCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 72, true);
            this.customsCodesGroupBox.TabIndex = 9;
            this.customsCodesGroupBox.TabStop = false;
            // 
            // registrationIDTextBox
            // 
            this.BindingSource.SetBindingMember(this.registrationIDTextBox, "RegistrationID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).RegistrationID)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.registrationIDTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.registrationIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 36, true);
            this.registrationIDTextBox.Name = "registrationIDTextBox";
            this.registrationIDTextBox.ReadOnly = true;
            this.registrationIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.registrationIDTextBox.TabIndex = 9;
            // 
            // senderIDTextBox
            // 
            this.BindingSource.SetBindingMember(this.senderIDTextBox, "SenderID");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).SenderID)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.senderIDTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.senderIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 36, true);
            this.senderIDTextBox.Name = "senderIDTextBox";
            this.senderIDTextBox.ReadOnly = true;
            this.senderIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
            this.senderIDTextBox.TabIndex = 10;
            // 
            // versionGroupBox
            // 
            this.versionGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("bba06bb5-9a3d-4a44-ac75-17e4c4ffca0a", "Filter report lines");
            this.versionGroupBox.Controls.Add(this.versionDropEdit);
            this.versionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.versionGroupBox.Name = "versionGroupBox";
            this.versionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 72, true);
            this.versionGroupBox.TabIndex = 7;
            this.versionGroupBox.TabStop = false;
            // 
            // versionDropEdit
            // 
            this.versionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.versionDropEdit, "SelectedVersion");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport)(null)).SelectedVersion)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.versionDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.versionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 36, true);
            this.versionDropEdit.Name = "versionDropEdit";
            this.versionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.versionDropEdit.TabIndex = 7;
            this.versionDropEdit.SelectedIndexChanged += new System.EventHandler(this.versionDropEdit_SelectedIndexChanged);
            // 
            // ZMGermanyForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4fe63c9f-c5c0-4e7a-a9a5-47cdea3db093", "ZMD Summary");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 545, true);
            this.Controls.Add(this.taxReturnPanel);
            this.Controls.Add(this.reportDetailsPanel);
            this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.ZMGermany.ZMGermanyReport);
            this.Name = "ZMGermanyForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.reportDetailsPanel, 0);
            this.Controls.SetChildIndex(this.taxReturnPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.reportDetailsPanel.ResumeLayout(false);
            this.reportDetailsPanel.PerformLayout();
            this.reportGroupBox.ResumeLayout(false);
            this.reportGroupBox.PerformLayout();
            this.dateFromDateEdit.ResumeLayout(true);
            this.dateFromDateEdit.PerformLayout();
            this.dateToDateEdit.ResumeLayout(true);
            this.dateToDateEdit.PerformLayout();
            this.taxReturnPanel.ResumeLayout(false);
            this.taxReturnPanel.PerformLayout();
            this.bottomSectionPanel.ResumeLayout(false);
            this.bottomSectionPanel.PerformLayout();
            this.buttonsPanel.ResumeLayout(false);
            this.buttonsPanel.PerformLayout();
            this.reportLinesGroupBox.ResumeLayout(false);
            this.reportLinesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.reportLinesGrid)).EndInit();
            this.reportLinesGrid.ResumeLayout(false);
            this.reportLinesGrid.PerformLayout();
            this.middleSectionPanel.ResumeLayout(false);
            this.middleSectionPanel.PerformLayout();
            this.customsCodesGroupBox.ResumeLayout(false);
            this.customsCodesGroupBox.PerformLayout();
            this.versionGroupBox.ResumeLayout(false);
            this.versionGroupBox.PerformLayout();
            this.versionDropEdit.ResumeLayout(true);
            this.versionDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel reportDetailsPanel;
		private ZArchitecture.GUI.ZGroupBox reportGroupBox;
		private ZArchitecture.ZLabel companyNameLabel;
		private ZArchitecture.GUI.ZDateEdit dateFromDateEdit;
		private ZArchitecture.GUI.ZDateEdit dateToDateEdit;
		private ZArchitecture.ZTextBox descriptionTextBox;
		private ZArchitecture.GUI.ZPanel taxReturnPanel;
		private ZArchitecture.GUI.ZPanel middleSectionPanel;
		private ZArchitecture.GUI.ZGroupBox versionGroupBox;
		private ZArchitecture.GUI.ZDropEdit versionDropEdit;
		private ZArchitecture.GUI.ZGroupBox customsCodesGroupBox;
		private ZArchitecture.ZTextBox registrationIDTextBox;
		private ZArchitecture.ZTextBox senderIDTextBox;
		private ZArchitecture.GUI.ZPanel bottomSectionPanel;
		private ZArchitecture.GUI.ZGroupBox reportLinesGroupBox;
		private ZArchitecture.ZGrid reportLinesGrid;
		private ZArchitecture.GUI.ZPanel buttonsPanel;
		private ZArchitecture.GUI.ZButton closeButton;
		private ZArchitecture.GUI.ZButton markAsSubmittedButton;
		private ZArchitecture.GUI.ZButton generateButton;
	}
}
