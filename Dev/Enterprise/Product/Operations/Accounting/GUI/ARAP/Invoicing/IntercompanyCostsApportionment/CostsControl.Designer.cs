namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	partial class CostsControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DocReceivedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SelfBillingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSTInclusiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExpectedTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExpectedTotalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CreditorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExchangeRateControl = new Enterprise.ZArchitecture.GUI.ZExchangeRateControl();
			this.InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TaxBranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LineSummaryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LineSummaryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryGroupBox.SuspendLayout();
			this.DocReceivedDateEdit.SuspendLayout();
			this.CreditorGuidFindBox.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.PostedDateEdit.SuspendLayout();
			this.DueDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.TaxBranchGuidFindBox.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineSummaryGrid)).BeginInit();
			this.LineSummaryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice);
			// 
			// SummaryGroupBox
			// 
			this.SummaryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|0d4de607-14c3-4d22-9031-7cad791964ae", "Invoice Summary");
			this.SummaryGroupBox.Controls.Add(this.DocReceivedDateEdit);
			this.SummaryGroupBox.Controls.Add(this.SelfBillingCheckBox);
			this.SummaryGroupBox.Controls.Add(this.GSTInclusiveCheckBox);
			this.SummaryGroupBox.Controls.Add(this.ExpectedTotalCalcEdit);
			this.SummaryGroupBox.Controls.Add(this.ExpectedTotalCheckBox);
			this.SummaryGroupBox.Controls.Add(this.CreditorGuidFindBox);
			this.SummaryGroupBox.Controls.Add(this.DescriptionTextBox);
			this.SummaryGroupBox.Controls.Add(this.ExchangeRateControl);
			this.SummaryGroupBox.Controls.Add(this.InvoiceNumberTextBox);
			this.SummaryGroupBox.Controls.Add(this.PostedDateEdit);
			this.SummaryGroupBox.Controls.Add(this.DueDateEdit);
			this.SummaryGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.SummaryGroupBox.Controls.Add(this.TaxBranchGuidFindBox);
			this.SummaryGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SummaryGroupBox.Name = "SummaryGroupBox";
			this.SummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 142, true);
			this.SummaryGroupBox.TabIndex = 1;
			this.SummaryGroupBox.TabStop = false;
			// 
			// DocReceivedDateEdit
			// 
			this.DocReceivedDateEdit.AllowDrop = true;
			this.DocReceivedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DocReceivedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DocReceivedDateEdit, "DocumentReceivedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).DocumentReceivedDate)));
			this.DocReceivedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|633EB419-2A2F-4CC5-9570-8D496C48DF95", "Doc Rec Date","Document Received Date");
			this.DocReceivedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 16, true);
			this.DocReceivedDateEdit.Name = "DocReceivedDateEdit";
			this.DocReceivedDateEdit.TabIndex = 1;
			// 
			// SelfBillingCheckBox
			// 
			this.SelfBillingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SelfBillingCheckBox, "IsSelfBillingInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).IsSelfBillingInvoice)));
			this.SelfBillingCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|5eb8bcea-74bc-4e76-9441-9bf954636013", "Is Self Billing Invoice");
			this.SelfBillingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SelfBillingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 65, true);
			this.SelfBillingCheckBox.Name = "SelfBillingCheckBox";
			this.SelfBillingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SelfBillingCheckBox.TabIndex = 7;
			this.SelfBillingCheckBox.UseVisualStyleBackColor = true;
			// 
			// GSTInclusiveCheckBox
			// 
			this.GSTInclusiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GSTInclusiveCheckBox, "GSTInclusive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).GSTInclusive)));
			this.GSTInclusiveCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|87f84360-4ae4-4b85-9435-ff72c11ffa7d", "Tax Inclusive Amounts");
			this.GSTInclusiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GSTInclusiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(533, 72, true);
			this.GSTInclusiveCheckBox.Name = "GSTInclusiveCheckBox";
			this.GSTInclusiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.GSTInclusiveCheckBox.TabIndex = 8;
			this.GSTInclusiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExpectedTotalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExpectedTotalCalcEdit, "ExpectedInvoiceTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).ExpectedInvoiceTotal)));
			this.ExpectedTotalCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExpectedTotalCalcEdit, false);
			this.ExpectedTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(643, 42, true);
			this.ExpectedTotalCalcEdit.Name = "ExpectedTotalCalcEdit";
			this.ExpectedTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ExpectedTotalCalcEdit.TabIndex = 5;
			this.ExpectedTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExpectedTotalCheckBox
			// 
			this.ExpectedTotalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExpectedTotalCheckBox, "ShouldValidateExpectedInvoiceTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).ShouldValidateExpectedInvoiceTotal)));
			this.ExpectedTotalCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|f65a7470-f5ca-497e-ab7f-ed7e13d8c11a", "Expected Total");
			this.ExpectedTotalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExpectedTotalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(533, 42, true);
			this.ExpectedTotalCheckBox.Name = "ExpectedTotalCheckBox";
			this.ExpectedTotalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExpectedTotalCheckBox.TabIndex = 4;
			this.ExpectedTotalCheckBox.UseVisualStyleBackColor = true;
			// 
			// CreditorGuidFindBox
			// 
			this.CreditorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreditorGuidFindBox, "Creditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Creditor)));
			this.CreditorGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|439d3e03-40e4-44b2-ad15-7645241c910b", "Creditor");
			this.CreditorGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.CreditorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 42, true);
			this.CreditorGuidFindBox.Name = "CreditorGuidFindBox";
			this.CreditorGuidFindBox.ShouldResize = true;
			this.CreditorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 17, true);
			this.CreditorGuidFindBox.TabIndex = 3;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Description)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|753ab967-3e9c-456d-a6a2-ce6d282f84e7", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 111, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 17, true);
			this.DescriptionTextBox.TabIndex = 10;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZExchangeRate)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|c1318053-0186-400f-ad6b-79e9c5f57613", "Exchange Rate");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 88, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ExchangeRateControl.TabIndex = 9;
			// 
			// InvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).InvoiceNumber)));
			this.InvoiceNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|85355e4a-41c6-4cc1-97b8-412a91a9a615", "Invoice Number");
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(643, 16, true);
			this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
			this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.InvoiceNumberTextBox.TabIndex = 2;
			// 
			// PostedDateEdit
			// 
			this.PostedDateEdit.AllowDrop = true;
			this.PostedDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostedDateEdit, "PostedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).PostedDate)));
			this.PostedDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|8b2f2b78-6569-4460-a745-bfa1e9be19e5", "Posted Date");
			this.PostedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 16, true);
			this.PostedDateEdit.Name = "PostedDateEdit";
			this.PostedDateEdit.TabIndex = 1;
			// 
			// DueDateEdit
			// 
			this.DueDateEdit.AllowDrop = true;
			this.DueDateEdit.AutoCompleteMonthThreshold = 1;
			this.DueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DueDateEdit, "DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).DueDate)));
			this.DueDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|894f975f-1320-406c-9978-41aa699a3797", "Due Date");
			this.DueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 65, true);
			this.DueDateEdit.Name = "DueDateEdit";
			this.DueDateEdit.TabIndex = 6;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).InvoiceDate)));
			this.InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|b87e5726-d7f3-4628-aaa3-511977cc8408", "Invoice Date");
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 16, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 0;
			// 
			// TaxBranchGuidFindBox
			// 
			this.TaxBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxBranchGuidFindBox, "TaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).TaxBranch)));
			this.TaxBranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|612e31c9-65f2-4d83-8931-f253d73bdb2f", "Tax Branch");
			this.TaxBranchGuidFindBox.PopupCaption = null;
			this.TaxBranchGuidFindBox.ShouldResize = true;
			this.TaxBranchGuidFindBox.ShowDescriptionBox = false;
			this.TaxBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(533, 111, true);
			this.TaxBranchGuidFindBox.Name = "TaxBranchGuidFindBox";
			this.TaxBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.TaxBranchGuidFindBox.TabIndex = 3;
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Controls.Add(this.ChargesGroupBox);
			this.LineSummaryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineSummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 142, true);
			this.LineSummaryPanel.Name = "LineSummaryPanel";
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 157, true);
			this.LineSummaryPanel.TabIndex = 2;
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|76c45cb8-502c-4223-8445-58ca18e04f36", "Charges");
			this.ChargesGroupBox.Controls.Add(this.LineSummaryGrid);
			this.ChargesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargesGroupBox.Name = "ChargesGroupBox";
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 157, true);
			this.ChargesGroupBox.TabIndex = 0;
			this.ChargesGroupBox.TabStop = false;
			// 
			// LineSummaryGrid
			// 
			this.LineSummaryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LineSummaryGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).GenericCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).ApportionmentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Amount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).AL_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).AL_TaxDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).AL_A9_VATClass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Tax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Total)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).LocalTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).LocalTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).GSTInclusiveAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).GSTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).AL_GovtChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).BranchName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).DepartmentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).AL_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).TaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).TaxBranchName)));
			this.LineSummaryGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|2f0e36a9-b0e4-4cba-9d7a-15c6ccbb0c00", "Charges");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GenericCharge";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|fc1e0a96-bbbd-496e-ba97-12ff3fc70787", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|00b46896-7e2c-488d-9bf7-82a41f69b422", "Apportionment Method");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ApportionmentMethod";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|a826b5ea-2365-4848-ba76-66460c1797f9", "Branch");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Branch";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|7773b074-e529-4a56-a70e-b72e7b421235", "Department");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|3e7d6e7b-3fc9-454d-b457-463106623922", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "Amount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|fbe0631b-ef65-47fc-be44-4df571b3932b", "Tax ID");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AL_AT";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "AL_TaxDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo6.ColumnName = "AL_A9_VATClass";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|d93963e2-4344-40c5-83e6-cdf5c0d8fa08", "Tax");
			zCalcEditColumnStyleInfo2.ColumnName = "Tax";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|6d208369-0e1a-41fd-8e7b-5544fb77d6ad", "Total");
			zCalcEditColumnStyleInfo3.ColumnName = "Total";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|b4162ba7-f9c2-4ad8-b9ad-b7c7102c9ca1", "Local Total");
			zCalcEditColumnStyleInfo4.ColumnName = "LocalTotal";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|5914173a-e3bb-4cc5-b696-3cba7d52e960", "Local Tax");
			zCalcEditColumnStyleInfo5.ColumnName = "LocalTax";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|8f279dc5-197a-42dc-bc95-e149d6f9d51e", "Local Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|92d4b40e-e987-4dd0-b553-904a2bc67a70", "Tax Inclusive Amount");
			zCalcEditColumnStyleInfo7.ColumnName = "GSTInclusiveAmount";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|9a58ea98-e75d-4ae5-9d57-a95913e5b995", "GST Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "GSTAmount";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|860f6488-5757-420a-82ce-bf669d6f22e8", "Government Charge Code");
			zTextBoxColumnStyleInfo2.ColumnName = "AL_GovtChargeCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|84f98d67-34d9-409e-8549-35a7ebfb0333", "Branch Name");
			zTextBoxColumnStyleInfo3.ColumnName = "BranchName";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|ab2c55ac-b7af-4aa4-9176-c5713ca7b600", "Department Description");
			zTextBoxColumnStyleInfo4.ColumnName = "DepartmentDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|74E4930A-3190-47A9-9EEC-7C7862E882C8", "Supply Type");
			zDropEditColumnStyleInfo1.ColumnName = "AL_SupplyType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|5096abcd-cb60-4626-9828-c75937f855c9", "Tax Branch");
			zGuidFindBoxColumnStyleInfo7.ColumnName = "TaxBranch";
			zGuidFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CostsControl|386ce8c5-1102-4aaf-a169-b30a69e9ec15", "Tax Branch Name");
			zTextBoxColumnStyleInfo5.ColumnName = "TaxBranchName";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.LineSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LineSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LineSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.LineSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.LineSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo7);
			this.LineSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LineSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.LineSummaryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LineSummaryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.LineSummaryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.LineSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LineSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LineSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LineSummaryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LineSummaryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LineSummaryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LineSummaryGrid.GridId = "d1d9a983-d44b-4700-b0e5-3984cabfbf0a";
			this.LineSummaryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LineSummaryGrid.LayoutKey = "LineSummaryGrid";
			this.LineSummaryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.LineSummaryGrid.Name = "LineSummaryGrid";
			this.LineSummaryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 140, true);
			this.LineSummaryGrid.TabIndex = 11;
			// 
			// CostsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LineSummaryPanel);
			this.Controls.Add(this.SummaryGroupBox);
			this.Name = "CostsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 299, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SummaryGroupBox.ResumeLayout(false);
			this.SummaryGroupBox.PerformLayout();
			this.DocReceivedDateEdit.ResumeLayout(true);
			this.DocReceivedDateEdit.PerformLayout();
			this.CreditorGuidFindBox.ResumeLayout(true);
			this.CreditorGuidFindBox.PerformLayout();
			this.ExchangeRateControl.ResumeLayout(true);
			this.ExchangeRateControl.PerformLayout();
			this.PostedDateEdit.ResumeLayout(true);
			this.PostedDateEdit.PerformLayout();
			this.DueDateEdit.ResumeLayout(true);
			this.DueDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.LineSummaryPanel.ResumeLayout(false);
			this.LineSummaryPanel.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LineSummaryGrid)).EndInit();
			this.LineSummaryGrid.ResumeLayout(false);
			this.LineSummaryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox SummaryGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel LineSummaryPanel;
		Enterprise.ZArchitecture.GUI.ZGroupBox ChargesGroupBox;
		public Enterprise.ZArchitecture.ZGrid LineSummaryGrid;
		Enterprise.ZArchitecture.ZTextBox InvoiceNumberTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit PostedDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit DueDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit InvoiceDateEdit;
		Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		Enterprise.ZArchitecture.GUI.ZExchangeRateControl ExchangeRateControl;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox CreditorGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox ExpectedTotalCheckBox;
		Enterprise.ZArchitecture.ZCalcEdit ExpectedTotalCalcEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox GSTInclusiveCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox SelfBillingCheckBox;
		ZArchitecture.GUI.ZDateEdit DocReceivedDateEdit;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox TaxBranchGuidFindBox;
	}
}
