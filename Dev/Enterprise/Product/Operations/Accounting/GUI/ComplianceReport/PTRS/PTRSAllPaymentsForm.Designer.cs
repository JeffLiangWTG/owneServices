namespace Enterprise.Accounting.GUI.ComplianceReport.PTRS
{
	partial class PTRSAllPaymentsForm : ZArchitecture.GUI.ZChildForm
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.markAsSubmittedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostingButtons = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.dateToZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.dateFromZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.abnText = new Enterprise.ZArchitecture.ZLabel();
			this.abnRegNoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.PostingButtons.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.statusDropEdit.SuspendLayout();
			this.dateToZDateEdit.SuspendLayout();
			this.dateFromZDateEdit.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BottomPanel.Controls.Add(this.markAsSubmittedButton);
			this.BottomPanel.Controls.Add(this.PostingButtons);
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 193, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(935, 37, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// markAsSubmittedButton
			// 
			this.markAsSubmittedButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6e7eadec-bdc3-4103-a12c-2d647c184f89", "Submit");
			this.markAsSubmittedButton.IsCaptionOverridden = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.markAsSubmittedButton, false);
			this.markAsSubmittedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 9, true);
			this.markAsSubmittedButton.Name = "markAsSubmittedButton";
			this.markAsSubmittedButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.markAsSubmittedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 22, true);
			this.markAsSubmittedButton.TabIndex = 93;
			this.markAsSubmittedButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.markAsSubmittedButton.ToolTipCaption = null;
			this.markAsSubmittedButton.Click += new System.EventHandler(this.SubmitButton_Click);
			// 
			// PostingButtons
			// 
			this.PostingButtons.AllowDrop = true;
			this.PostingButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(689, 8, true);
			this.PostingButtons.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 25, true);
			this.PostingButtons.Name = "PostingButtons";
			this.PostingButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 25, true);
			this.PostingButtons.TabIndex = 5;
			this.PostingButtons.TabStop = true;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1b725daa-ef6a-47ab-bd49-913cca76f0d1", "Company and Report Details");
			this.zGroupBox1.Controls.Add(this.descriptionTextBox);
			this.zGroupBox1.Controls.Add(this.statusDropEdit);
			this.zGroupBox1.Controls.Add(this.dateToZDateEdit);
			this.zGroupBox1.Controls.Add(this.dateFromZDateEdit);
			this.zGroupBox1.Controls.Add(this.abnText);
			this.zGroupBox1.Controls.Add(this.abnRegNoLabel);
			this.zGroupBox1.Controls.Add(this.companyNameLabel);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 94, true);
			this.zGroupBox1.TabIndex = 17;
			this.zGroupBox1.TabStop = false;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "ATR_Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).ATR_Comment)));
			this.descriptionTextBox.CaptionResourceString = null;
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 67, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 17, true);
			this.descriptionTextBox.TabIndex = 31;
			// 
			// statusDropEdit
			// 
			this.statusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.statusDropEdit, "ATR_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).ATR_Status)));
			this.statusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 46, true);
			this.statusDropEdit.Name = "statusDropEdit";
			this.statusDropEdit.ShouldResizeByMaxLength = true;
			this.statusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 17, true);
			this.statusDropEdit.TabIndex = 29;
			// 
			// dateToZDateEdit
			// 
			this.dateToZDateEdit.AllowDrop = true;
			this.dateToZDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateToZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateToZDateEdit, "ComplianceReport.ACR_DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).ComplianceReport.ACR_DateTo)));
			this.dateToZDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cd595fd5-34a1-474b-b60c-93d72775fc01", "Date To");
			this.dateToZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 46, true);
			this.dateToZDateEdit.Name = "dateToZDateEdit";
			this.dateToZDateEdit.TabIndex = 28;
			// 
			// dateFromZDateEdit
			// 
			this.dateFromZDateEdit.AllowDrop = true;
			this.dateFromZDateEdit.AutoCompleteMonthThreshold = 1;
			this.dateFromZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.dateFromZDateEdit, "ComplianceReport.ACR_DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).ComplianceReport.ACR_DateFrom)));
			this.dateFromZDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("566a7945-8559-447d-bb56-4bf01f61415e", "Date From");
			this.dateFromZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 46, true);
			this.dateFromZDateEdit.Name = "dateFromZDateEdit";
			this.dateFromZDateEdit.TabIndex = 26;
			// 
			// abnText
			// 
			this.abnText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("92f1786b-8511-4e4e-843a-d4791fd9e43e", "ABN");
			this.abnText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.abnText, false);
			this.abnText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(557, 21, true);
			this.abnText.Name = "abnText";
			this.abnText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 17, true);
			this.abnText.TabIndex = 23;
			this.abnText.Text = "ABN ";
			this.abnText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// abnRegNoLabel
			// 
			this.BindingSource.SetBindingMember(this.abnRegNoLabel, "ATR_VATRegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).ATR_VATRegNo)));
			this.abnRegNoLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8e6ae3b3-ae61-43b9-a4b6-eaaf46bb0d68", "ABN");
			this.abnRegNoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.abnRegNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 21, true);
			this.abnRegNoLabel.Name = "abnRegNoLabel";
			this.abnRegNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.abnRegNoLabel.TabIndex = 24;
			this.abnRegNoLabel.Text = "<ABN>";
			// 
			// companyNameLabel
			// 
			this.BindingSource.SetBindingMember(this.companyNameLabel, "ATR_CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).ATR_CompanyName)));
			this.companyNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyNameLabel, false);
			this.companyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 21, true);
			this.companyNameLabel.Name = "companyNameLabel";
			this.companyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 17, true);
			this.companyNameLabel.TabIndex = 16;
			this.companyNameLabel.Text = "<Company Name>";
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("515da31e-a6f4-47b7-afc0-5b778eea16d2", "Payment Data");
			this.zGroupBox2.Controls.Add(this.zCalcEdit3);
			this.zGroupBox2.Controls.Add(this.zTextBox1);
			this.zGroupBox2.Controls.Add(this.zCalcEdit1);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 94, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 93, true);
			this.zGroupBox2.TabIndex = 18;
			this.zGroupBox2.TabStop = false;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "AllInvoicesPaidWithOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).AllInvoicesPaidWithOverride)));
			this.zCalcEdit3.CaptionResourceString = null;
			this.zCalcEdit3.DecimalPlaces = 2;
			this.zCalcEdit3.IsCalculatorEnabled = false;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 45, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.ShowGroupSeparators = false;
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.zCalcEdit3.TabIndex = 31;
			this.zCalcEdit3.Text = "0.00";
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "ReasonToOverrideAllInvoicesPaid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).ReasonToOverrideAllInvoicesPaid)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 66, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 17, true);
			this.zTextBox1.TabIndex = 32;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "AllInvoicesPaid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport)(null)).AllInvoicesPaid)));
			this.zCalcEdit1.CaptionResourceString = null;
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.IsCalculatorEnabled = false;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 23, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.ShowGroupSeparators = false;
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.zCalcEdit1.TabIndex = 30;
			this.zCalcEdit1.Text = "0.00";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PTRSAllPaymentsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9463b590-20cb-47db-9a47-71ff0c928b09", "Payment Times Reporting Scheme - All Payments");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 257, true);
			this.Controls.Add(this.zGroupBox2);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ComplianceReport.PTRS.PtrsAllPaymentsReport";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(953, 295, true);
			this.Name = "PTRSAllPaymentsForm";
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.zGroupBox2, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtons.ResumeLayout(true);
			this.PostingButtons.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.statusDropEdit.ResumeLayout(true);
			this.statusDropEdit.PerformLayout();
			this.dateToZDateEdit.ResumeLayout(true);
			this.dateToZDateEdit.PerformLayout();
			this.dateFromZDateEdit.ResumeLayout(true);
			this.dateFromZDateEdit.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtons;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZDateEdit dateToZDateEdit;
		private ZArchitecture.GUI.ZDateEdit dateFromZDateEdit;
		private ZArchitecture.ZLabel abnText;
		private ZArchitecture.ZLabel abnRegNoLabel;
		private ZArchitecture.ZLabel companyNameLabel;
		private ZArchitecture.GUI.ZDropEdit statusDropEdit;
		private ZArchitecture.ZTextBox descriptionTextBox;
		private ZArchitecture.GUI.ZButton markAsSubmittedButton;
		private ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZCalcEdit zCalcEdit1;
		private ZArchitecture.ZCalcEdit zCalcEdit3;
	}
}
