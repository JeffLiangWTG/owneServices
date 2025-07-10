namespace Enterprise.Customs.CA.GUI
{
	partial class LVSDutiesAndTaxesUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InvoiceCurrExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FOBValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AdjustmentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MiddlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AdjustmentValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CustomsValueOvrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TaxesAmountValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrConvOvrrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CurrencyConversionValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutiesAndTaxesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AmountDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AmountDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TableLayoutPanel.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.AdjustmentTypeDropEdit.SuspendLayout();
			this.MiddlePanel.SuspendLayout();
			this.RightPanel.SuspendLayout();
			this.DutiesAndTaxesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DutyAndTaxGrid)).BeginInit();
			this.DutyAndTaxGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobComInvoiceLine);
			// 
			// TableLayoutPanel
			// 
			this.TableLayoutPanel.ColumnCount = 3;
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.19838F));
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.98374F));
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.84553F));
			this.TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.TableLayoutPanel.Controls.Add(this.LeftPanel, 0, 0);
			this.TableLayoutPanel.Controls.Add(this.MiddlePanel, 1, 0);
			this.TableLayoutPanel.Controls.Add(this.RightPanel, 2, 0);
			this.TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TableLayoutPanel.Name = "TableLayoutPanel";
			this.TableLayoutPanel.RowCount = 1;
			this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50)));
			this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20)));
			this.TableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 54, true);
			this.TableLayoutPanel.TabIndex = 0;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.InvoiceCurrExRateCalcEdit);
			this.LeftPanel.Controls.Add(this.FOBValueCalcEdit);
			this.LeftPanel.Controls.Add(this.AdjustmentTypeDropEdit);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 48, true);
			this.LeftPanel.TabIndex = 0;
			// 
			// InvoiceCurrExRateCalcEdit
			// 
			this.InvoiceCurrExRateCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoiceCurrExRateCalcEdit, "InvoiceHeader.JZ_InvoiceCurrExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).InvoiceHeader.JZ_InvoiceCurrExRate)));
			this.InvoiceCurrExRateCalcEdit.DecimalPlaces = 2;
			this.InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 23, true);
			this.InvoiceCurrExRateCalcEdit.Name = "InvoiceCurrExRateCalcEdit";
			this.InvoiceCurrExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.InvoiceCurrExRateCalcEdit.TabIndex = 2;
			this.InvoiceCurrExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FOBValueCalcEdit
			// 
			this.FOBValueCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FOBValueCalcEdit, "JI_Calc_FOB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).JI_Calc_FOB)));
			this.FOBValueCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|2c6f4a9e-1708-4d10-9ac6-4d95a9df7c17", "Value", "Line Value", "");
			this.FOBValueCalcEdit.DecimalPlaces = 2;
			this.FOBValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 1, true);
			this.FOBValueCalcEdit.Name = "FOBValueCalcEdit";
			this.FOBValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.FOBValueCalcEdit.TabIndex = 0;
			this.FOBValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AdjustmentTypeDropEdit
			// 
			this.AdjustmentTypeDropEdit.AllowDrop = true;
			this.AdjustmentTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AdjustmentTypeDropEdit, "CA_ADJCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_ADJCode)));
			this.AdjustmentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 1, true);
			this.AdjustmentTypeDropEdit.Name = "AdjustmentTypeDropEdit";
			this.AdjustmentTypeDropEdit.PreBoundMaxLength = 1;
			this.AdjustmentTypeDropEdit.ShowDescriptionBox = false;
			this.AdjustmentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.AdjustmentTypeDropEdit.TabIndex = 1;
			// 
			// MiddlePanel
			// 
			this.MiddlePanel.Controls.Add(this.AdjustmentValueCalcEdit);
			this.MiddlePanel.Controls.Add(this.CustomsValueOvrCheckBox);
			this.MiddlePanel.Controls.Add(this.CustomsValueCalcEdit);
			this.MiddlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 3, true);
			this.MiddlePanel.Name = "MiddlePanel";
			this.MiddlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 48, true);
			this.MiddlePanel.TabIndex = 1;
			// 
			// AdjustmentValueCalcEdit
			// 
			this.AdjustmentValueCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AdjustmentValueCalcEdit, "CA_ADJValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_ADJValue)));
			this.AdjustmentValueCalcEdit.DecimalPlaces = 2;
			this.AdjustmentValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 1, true);
			this.AdjustmentValueCalcEdit.Name = "AdjustmentValueCalcEdit";
			this.AdjustmentValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.AdjustmentValueCalcEdit.TabIndex = 0;
			this.AdjustmentValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CustomsValueOvrCheckBox
			// 
			this.CustomsValueOvrCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CustomsValueOvrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CustomsValueOvrCheckBox, "CA_CustomsValueOvr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CustomsValueOvr)));
			this.CustomsValueOvrCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CustomsValueOvrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 26, true);
			this.CustomsValueOvrCheckBox.Name = "CustomsValueOvrCheckBox";
			this.CustomsValueOvrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CustomsValueOvrCheckBox.TabIndex = 2;
			this.CustomsValueOvrCheckBox.UseVisualStyleBackColor = true;
			// 
			// CustomsValueCalcEdit
			// 
			this.CustomsValueCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CustomsValueCalcEdit, "CA_CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CustomsValue)));
			this.CustomsValueCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|e90d48a3-efb2-481d-8707-95dd8882d9c6", "VFD", "Value for Duty", "");
			this.CustomsValueCalcEdit.DecimalPlaces = 2;
			this.CustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 23, true);
			this.CustomsValueCalcEdit.Name = "CustomsValueCalcEdit";
			this.CustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.CustomsValueCalcEdit.TabIndex = 1;
			this.CustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RightPanel
			// 
			this.RightPanel.Controls.Add(this.TaxesAmountValueCalcEdit);
			this.RightPanel.Controls.Add(this.CurrConvOvrrideCheckBox);
			this.RightPanel.Controls.Add(this.CurrencyConversionValueCalcEdit);
			this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 3, true);
			this.RightPanel.Name = "RightPanel";
			this.RightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 48, true);
			this.RightPanel.TabIndex = 2;
			// 
			// TaxesAmountValueCalcEdit
			// 
			this.TaxesAmountValueCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TaxesAmountValueCalcEdit, "CA_ValueForTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_ValueForTax)));
			this.TaxesAmountValueCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|fbcda7e6-3671-45d1-8140-1d17e11d983e", "VFT", "Value for Tax", "");
			this.TaxesAmountValueCalcEdit.DecimalPlaces = 2;
			this.TaxesAmountValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 23, true);
			this.TaxesAmountValueCalcEdit.Name = "TaxesAmountValueCalcEdit";
			this.TaxesAmountValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.TaxesAmountValueCalcEdit.TabIndex = 2;
			this.TaxesAmountValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrConvOvrrideCheckBox
			// 
			this.CurrConvOvrrideCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CurrConvOvrrideCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CurrConvOvrrideCheckBox, "CA_CVforCurrConvOvr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CVforCurrConvOvr)));
			this.CurrConvOvrrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CurrConvOvrrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(189, 3, true);
			this.CurrConvOvrrideCheckBox.Name = "CurrConvOvrrideCheckBox";
			this.CurrConvOvrrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CurrConvOvrrideCheckBox.TabIndex = 1;
			this.CurrConvOvrrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// CurrencyConversionValueCalcEdit
			// 
			this.CurrencyConversionValueCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CurrencyConversionValueCalcEdit, "CA_CVforCurrConv");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).CA_CVforCurrConv)));
			this.CurrencyConversionValueCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|18b49213-cb76-4264-a566-6b61b5f76e74", "VFCC", "Value for Currency Conversion", "");
			this.CurrencyConversionValueCalcEdit.DecimalPlaces = 2;
			this.CurrencyConversionValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 1, true);
			this.CurrencyConversionValueCalcEdit.Name = "CurrencyConversionValueCalcEdit";
			this.CurrencyConversionValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.CurrencyConversionValueCalcEdit.TabIndex = 0;
			this.CurrencyConversionValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutiesAndTaxesGroupBox
			// 
			this.DutiesAndTaxesGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|46f0150a-e1ce-4ebf-ab1b-4ca6b8ad10a7", "Line Duties and Taxes");
			this.DutiesAndTaxesGroupBox.Controls.Add(this.AmountDescriptionLabel);
			this.DutiesAndTaxesGroupBox.Controls.Add(this.AmountDescriptionTextBox);
			this.DutiesAndTaxesGroupBox.Controls.Add(this.DutyAndTaxGrid);
			this.DutiesAndTaxesGroupBox.Controls.Add(this.TableLayoutPanel);
			this.DutiesAndTaxesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DutiesAndTaxesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DutiesAndTaxesGroupBox.Name = "DutiesAndTaxesGroupBox";
			this.DutiesAndTaxesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 192, true);
			this.DutiesAndTaxesGroupBox.TabIndex = 0;
			this.DutiesAndTaxesGroupBox.TabStop = false;
			// 
			// AmountDescriptionLabel
			// 
			this.AmountDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AmountDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 155, true);
			this.AmountDescriptionLabel.Name = "AmountDescriptionLabel";
			this.AmountDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 34, true);
			this.AmountDescriptionLabel.TabIndex = 2;
			this.AmountDescriptionLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|b41b2f0c-4384-413c-9c2b-a6581b43fc3f", "Amount Calculation Description");
			this.AmountDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// AmountDescriptionTextBox
			// 
			this.AmountDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AmountDescriptionTextBox, "DutiesAndTaxes.AmountDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).AmountDescription)));
			this.AmountDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AmountDescriptionTextBox, false);
			this.AmountDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 155, true);
			this.AmountDescriptionTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(4, 20, true);
			this.AmountDescriptionTextBox.Multiline = true;
			this.AmountDescriptionTextBox.Name = "AmountDescriptionTextBox";
			this.AmountDescriptionTextBox.ReadOnly = true;
			this.AmountDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 34, true);
			this.AmountDescriptionTextBox.TabIndex = 3;
			// 
			// DutyAndTaxGrid
			// 
			this.DutyAndTaxGrid.AllowNavigation = false;
			this.DutyAndTaxGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DutyAndTaxGrid, "DutiesAndTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_TaxType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_ExemptCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_Override)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_RateType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_UnitOfMeasure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_PreviousTranNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_PreviousTranLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_NormalValuePerUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_NormalValueCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).NormalValueCurrencyExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_ForeignRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).C1_ForeignCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.DutyAndTax)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(null)).DutiesAndTaxes)).SyncRoot)).ForeignCurrencyExchangeRate)));
			this.DutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.ColumnName = "C1_TaxType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo7.ColumnName = "C1_Code";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo8.ColumnName = "C1_ExemptCode";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo2.ColumnName = "C1_Override";
			zCheckBoxColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "C1_Rate";
			zCalcEditColumnStyleInfo5.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|b770a183-4214-462c-a496-ec51fe8935fd", "Rate");
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo9.ColumnName = "C1_RateType";
			zDropEditColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|b770a183-4214-462c-a496-ec51fe8935fd", "Rate");
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|f9364ace-dc8d-46f3-993f-544ebf2b9f6e", "Quantity");
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo10.ColumnName = "C1_UnitOfMeasure";
			zDropEditColumnStyleInfo10.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|f9364ace-dc8d-46f3-993f-544ebf2b9f6e", "Quantity");
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "C1_Amount";
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "C1_PreviousTranNumber";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|5bd154e6-218f-4bcc-aec3-01edf4200d16", "Previous Transaction");
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "C1_PreviousTranLine";
			zCalcEditColumnStyleInfo8.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("LVSDutiesAndTaxesUserControl|5bd154e6-218f-4bcc-aec3-01edf4200d16", "Previous Transaction");
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo9.ColumnName = "C1_NormalValuePerUnit";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCalcEditColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|6e57d040-b852-4f9b-85f1-3a1677102842", "Normal Values");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "C1_NormalValueCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|6e57d040-b852-4f9b-85f1-3a1677102842", "Normal Values");
			zCalcEditColumnStyleInfo10.ColumnName = "NormalValueCurrencyExchangeRate";
			zCalcEditColumnStyleInfo10.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|6e57d040-b852-4f9b-85f1-3a1677102842", "Normal Values");
			zCalcEditColumnStyleInfo11.ColumnName = "C1_ForeignRate";
			zCalcEditColumnStyleInfo11.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|fc8684ad-9bd1-44da-8292-fcfa29219397", "Foreign Rate");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "C1_ForeignCurrency";
			zCodeFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|fc8684ad-9bd1-44da-8292-fcfa29219397", "Foreign Rate");
			zCalcEditColumnStyleInfo12.ColumnName = "ForeignCurrencyExchangeRate";
			zCalcEditColumnStyleInfo12.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("DutyAndTaxGrid|fc8684ad-9bd1-44da-8292-fcfa29219397", "Foreign Rate");
			this.DutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.DutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.DutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.DutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.DutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.DutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.DutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.DutyAndTaxGrid.CopySelectedRowsAllowed = true;
			this.DutyAndTaxGrid.GridId = "fd908a1c-b898-4b26-bf3d-e45abbb85c80";
			this.DutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DutyAndTaxGrid.LayoutKey = "DutyAndTaxGrid";
			this.DutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 73, true);
			this.DutyAndTaxGrid.Name = "DutyAndTaxGrid";
			this.DutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(615, 80, true);
			this.DutyAndTaxGrid.TabIndex = 1;
			// 
			// LVSDutiesAndTaxesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DutiesAndTaxesGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 192, true);
			this.Name = "LVSDutiesAndTaxesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 192, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TableLayoutPanel.ResumeLayout(false);
			this.TableLayoutPanel.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.AdjustmentTypeDropEdit.ResumeLayout(true);
			this.AdjustmentTypeDropEdit.PerformLayout();
			this.MiddlePanel.ResumeLayout(false);
			this.MiddlePanel.PerformLayout();
			this.RightPanel.ResumeLayout(false);
			this.RightPanel.PerformLayout();
			this.DutiesAndTaxesGroupBox.ResumeLayout(false);
			this.DutiesAndTaxesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DutyAndTaxGrid)).EndInit();
			this.DutyAndTaxGrid.ResumeLayout(false);
			this.DutyAndTaxGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel TableLayoutPanel;
		private ZArchitecture.GUI.ZPanel LeftPanel;
		private ZArchitecture.ZCalcEdit FOBValueCalcEdit;
		private ZArchitecture.GUI.ZGroupBox DutiesAndTaxesGroupBox;
		private ZArchitecture.GUI.ZPanel MiddlePanel;
		private ZArchitecture.ZCalcEdit InvoiceCurrExRateCalcEdit;
		private ZArchitecture.ZCalcEdit TaxesAmountValueCalcEdit;
		private ZArchitecture.ZCalcEdit AdjustmentValueCalcEdit;
		private ZArchitecture.GUI.ZDropEdit AdjustmentTypeDropEdit;
		private ZArchitecture.GUI.ZPanel RightPanel;
		private ZArchitecture.GUI.ZCheckBox CustomsValueOvrCheckBox;
		private ZArchitecture.ZCalcEdit CustomsValueCalcEdit;
		private ZArchitecture.GUI.ZCheckBox CurrConvOvrrideCheckBox;
		private ZArchitecture.ZCalcEdit CurrencyConversionValueCalcEdit;
		private ZArchitecture.ZTextBox AmountDescriptionTextBox;
		private ZArchitecture.ZGrid DutyAndTaxGrid;
		private ZArchitecture.ZLabel AmountDescriptionLabel;
	}
}
