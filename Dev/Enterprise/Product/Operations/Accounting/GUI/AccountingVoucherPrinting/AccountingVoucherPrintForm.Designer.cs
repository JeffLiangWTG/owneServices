using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.AccountingVoucherPrinting
{
	public partial class AccoutingVoucherPrintForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			this.GenerateButton = new ZButton();
			this.CloseButton = new ZButton();
			this.LedgerLabel = new ZLabel();
			this.TransactionTypeLabel = new ZLabel();
			this.BatchVoucherReportGroupBox = new ZGroupBox();
			this.HelpTextLabel = new ZLabel();
			this.TransactionCheckedListBox = new CargoWise.Windows.UI.KCheckedListBox();
			this.LedgerCheckedListBox = new CargoWise.Windows.UI.KCheckedListBox();
			this.EndDateEdit = new ZDateEdit();
			this.FromDateEdit = new ZDateEdit();
			this.VoucherPanel = new ZPanel();
			this.BranchGroupBox = new ZGroupBox();
			this.zModuleButtonGrid1 = new ZModuleButtonGrid();
			this.MainPanel = new ZPanel();
			this.DatePanel = new ZPanel();
			this.VoucherPeriodEdit = new ZPeriodEdit();
			this.PrintOptionsGroupBox = new ZGroupBox();
			this.IncludeOrganisationCodeCheckBox = new ZCheckBox();
			this.IncludeJobNumberCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BatchVoucherReportGroupBox.SuspendLayout();
			this.VoucherPanel.SuspendLayout();
			this.BranchGroupBox.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.DatePanel.SuspendLayout();
			this.PrintOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 624, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccountingVoucherPrintWrapper);
			// 
			// GenerateButton
			// 
			this.GenerateButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|28a3cab3-4568-4df4-8c3f-3df9e25153ae", "Go");
			this.GenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 598, true);
			this.GenerateButton.Name = "GenerateButton";
			this.GenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.GenerateButton.TabIndex = 1;
			this.GenerateButton.Click += new EventHandler(this.GenerateButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|f386527c-7617-41e9-9657-69f1a6fb7992", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 598, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// LedgerLabel
			// 
			this.LedgerLabel.AutoSize = true;
			this.LedgerLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|f8a667bf-17e7-424b-91de-28bebf55d717", "Ledger");
			this.LedgerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 62, true);
			this.LedgerLabel.Name = "LedgerLabel";
			this.LedgerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			this.LedgerLabel.TabIndex = 1;
			// 
			// TransactionTypeLabel
			// 
			this.TransactionTypeLabel.AutoSize = true;
			this.TransactionTypeLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|29a1830d-90b3-4285-8b3c-146442d637c6", "Transaction Type");
			this.TransactionTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 62, true);
			this.TransactionTypeLabel.Name = "TransactionTypeLabel";
			this.TransactionTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.TransactionTypeLabel.TabIndex = 3;
			// 
			// BatchVoucherReportGroupBox
			// 
			this.BatchVoucherReportGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|db3519dc-7181-4faf-a49b-37eeed64efbc", "Select Transaction Type");
			this.BatchVoucherReportGroupBox.Controls.Add(this.HelpTextLabel);
			this.BatchVoucherReportGroupBox.Controls.Add(this.TransactionCheckedListBox);
			this.BatchVoucherReportGroupBox.Controls.Add(this.LedgerCheckedListBox);
			this.BatchVoucherReportGroupBox.Controls.Add(this.LedgerLabel);
			this.BatchVoucherReportGroupBox.Controls.Add(this.TransactionTypeLabel);
			this.BatchVoucherReportGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BatchVoucherReportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 195, true);
			this.BatchVoucherReportGroupBox.Name = "BatchVoucherReportGroupBox";
			this.BatchVoucherReportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 240, true);
			this.BatchVoucherReportGroupBox.TabIndex = 1;
			this.BatchVoucherReportGroupBox.TabStop = false;
			// 
			// HelpTextLabel
			// 
			this.HelpTextLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|9d24f87c-b25e-4c07-a35c-4b6d863c9fd3", "Vouchers will be printed for selected Transaction Types. Please select a Ledger and the Transaction Types requiring Accounting Vouchers.");
			this.HelpTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 26, true);
			this.HelpTextLabel.Name = "HelpTextLabel";
			this.HelpTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 30, true);
			this.HelpTextLabel.TabIndex = 0;
			// 
			// TransactionCheckedListBox
			// 
			this.TransactionCheckedListBox.CheckOnClick = true;
			this.TransactionCheckedListBox.ColumnWidth = 134;
			this.TransactionCheckedListBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TransactionCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 80, true);
			this.TransactionCheckedListBox.MultiColumn = true;
			this.TransactionCheckedListBox.Name = "TransactionCheckedListBox";
			this.TransactionCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 148, true);
			this.TransactionCheckedListBox.TabIndex = 4;
			this.TransactionCheckedListBox.SelectedIndexChanged += new EventHandler(this.TransactionCheckedListBox_SelectedIndexChanged);
			// 
			// LedgerCheckedListBox
			// 
			this.LedgerCheckedListBox.CheckOnClick = true;
			this.LedgerCheckedListBox.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LedgerCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 80, true);
			this.LedgerCheckedListBox.Name = "LedgerCheckedListBox";
			this.LedgerCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 148, true);
			this.LedgerCheckedListBox.TabIndex = 2;
			this.LedgerCheckedListBox.SelectedIndexChanged += new EventHandler(this.LedgerCheckedListBox_SelectedIndexChanged);
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccountingVoucherPrintWrapper)(null)).EndDate)));
			this.EndDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|7b5cd74c-a235-446f-9d9e-2abcf66eeeea", "Date To");
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 40, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 2;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccountingVoucherPrintWrapper)(null)).FromDate)));
			this.FromDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|ad4784bb-4af9-47e7-9a52-850960ac8529", "Date From");
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 40, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 1;
			// 
			// VoucherPanel
			// 
			this.VoucherPanel.Controls.Add(this.BranchGroupBox);
			this.VoucherPanel.Controls.Add(this.BatchVoucherReportGroupBox);
			this.VoucherPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VoucherPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			this.VoucherPanel.Name = "VoucherPanel";
			this.VoucherPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 435, true);
			this.VoucherPanel.TabIndex = 1;
			// 
			// BranchGroupBox
			// 
			this.BranchGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|75c526d9-dea2-411b-86e8-ac74b147322c", "Branches");
			this.BranchGroupBox.Controls.Add(this.zModuleButtonGrid1);
			this.BranchGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BranchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BranchGroupBox.Name = "BranchGroupBox";
			this.BranchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 195, true);
			this.BranchGroupBox.TabIndex = 0;
			this.BranchGroupBox.TabStop = false;
			// 
			// zModuleButtonGrid1
			// 
			this.zModuleButtonGrid1.AllowAttachDetachWithoutEditSecurity = true;
			this.zModuleButtonGrid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zModuleButtonGrid1, "SelectedBranches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AccountingVoucherPrintWrapper)(null)).SelectedBranches)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((AccountingVoucherPrintWrapper)(null)).BranchesList)));
			this.zModuleButtonGrid1.BindToFindBoxList = "BranchesList";
			this.zModuleButtonGrid1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.ColumnName = "GB_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "GB_BranchName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zModuleButtonGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zModuleButtonGrid1.GridId = "60e6e9c8-7fe0-4b28-ba26-b3f037d5eaea";
			// 
			// 
			// 
			this.zModuleButtonGrid1.InnerGrid.AllowNavigation = false;
			this.zModuleButtonGrid1.InnerGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zModuleButtonGrid1.InnerGrid.CaptionVisible = false;
			this.zModuleButtonGrid1.InnerGrid.CopySelectedRowsAllowed = true;
			this.zModuleButtonGrid1.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zModuleButtonGrid1.InnerGrid.LayoutKey = "Grid";
			this.zModuleButtonGrid1.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.zModuleButtonGrid1.InnerGrid.Name = "Grid";
			this.zModuleButtonGrid1.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 138, true);
			this.zModuleButtonGrid1.InnerGrid.TabIndex = 0;
			this.zModuleButtonGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zModuleButtonGrid1.Name = "zModuleButtonGrid1";
			this.zModuleButtonGrid1.ReadOnly = false;
			this.zModuleButtonGrid1.ShowEditButton = false;
			this.zModuleButtonGrid1.ShowNewButton = false;
			this.zModuleButtonGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 176, true);
			this.zModuleButtonGrid1.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MainPanel.Controls.Add(this.VoucherPanel);
			this.MainPanel.Controls.Add(this.DatePanel);
			this.MainPanel.Controls.Add(this.PrintOptionsGroupBox);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 590, true);
			this.MainPanel.TabIndex = 0;
			// 
			// DatePanel
			// 
			this.DatePanel.Controls.Add(this.VoucherPeriodEdit);
			this.DatePanel.Controls.Add(this.EndDateEdit);
			this.DatePanel.Controls.Add(this.FromDateEdit);
			this.DatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DatePanel.Name = "DatePanel";
			this.DatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 64, true);
			this.DatePanel.TabIndex = 0;
			// 
			// VoucherPeriodEdit
			// 
			this.BindingSource.SetBindingMember(this.VoucherPeriodEdit, "FromPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((AccountingVoucherPrintWrapper)(null)).FromPeriod)));
			this.VoucherPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|d6fad9a6-f2d8-4b6d-842d-19c7ff2ce939", "Period");
			this.VoucherPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 16, true);
			this.VoucherPeriodEdit.Name = "VoucherPeriodEdit";
			this.VoucherPeriodEdit.TabIndex = 0;
			// 
			// PrintOptionsGroupBox
			// 
			this.PrintOptionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|f8535792-b8fb-4a2d-884c-f5407f44a362", "Print Options");
			this.PrintOptionsGroupBox.Controls.Add(this.IncludeOrganisationCodeCheckBox);
			this.PrintOptionsGroupBox.Controls.Add(this.IncludeJobNumberCheckBox);
			this.PrintOptionsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PrintOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 499, true);
			this.PrintOptionsGroupBox.Name = "PrintOptionsGroupBox";
			this.PrintOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 91, true);
			this.PrintOptionsGroupBox.TabIndex = 2;
			this.PrintOptionsGroupBox.TabStop = false;
			// 
			// IncludeOrganisationCodeCheckBox
			// 
			this.IncludeOrganisationCodeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeOrganisationCodeCheckBox, "IncludeOrganisationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AccountingVoucherPrintWrapper)(null)).IncludeOrganisationCode)));
			this.IncludeOrganisationCodeCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|cb250461-b9dd-45fd-a17d-ac63510f6874", "Include Organization Code");
			this.IncludeOrganisationCodeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeOrganisationCodeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 42, true);
			this.IncludeOrganisationCodeCheckBox.Name = "IncludeOrganisationCodeCheckBox";
			this.IncludeOrganisationCodeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 17, true);
			this.IncludeOrganisationCodeCheckBox.TabIndex = 1;
			this.IncludeOrganisationCodeCheckBox.UseVisualStyleBackColor = true;
			// 
			// IncludeJobNumberCheckBox
			// 
			this.IncludeJobNumberCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IncludeJobNumberCheckBox, "IncludeJobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AccountingVoucherPrintWrapper)(null)).IncludeJobNumber)));
			this.IncludeJobNumberCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|fdb19b1f-1772-4d19-9d8e-19526ad54172", "Include Job Number");
			this.IncludeJobNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeJobNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 65, true);
			this.IncludeJobNumberCheckBox.Name = "IncludeJobNumberCheckBox";
			this.IncludeJobNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 17, true);
			this.IncludeJobNumberCheckBox.TabIndex = 2;
			this.IncludeJobNumberCheckBox.UseVisualStyleBackColor = true;
			// 
			// AccoutingVoucherPrintForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccoutingVoucherPrintForm|d02e0191-267e-4236-8220-32a18ccd4bc3", "Accounting Voucher Print");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 648, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.GenerateButton);
			this.Controls.Add(this.MainPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(AccountingVoucherPrintWrapper);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.AccountingVoucherPrint.AccountingVoucherPrintWrapp" +
	"er";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 686, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 686, true);
			this.Name = "AccoutingVoucherPrintForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.GenerateButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BatchVoucherReportGroupBox.ResumeLayout(false);
			this.BatchVoucherReportGroupBox.PerformLayout();
			this.VoucherPanel.ResumeLayout(false);
			this.BranchGroupBox.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.DatePanel.ResumeLayout(false);
			this.DatePanel.PerformLayout();
			this.PrintOptionsGroupBox.ResumeLayout(false);
			this.PrintOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
