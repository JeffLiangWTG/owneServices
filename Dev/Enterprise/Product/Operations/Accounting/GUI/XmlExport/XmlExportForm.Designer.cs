namespace Enterprise.Accounting.GUI.XmlExport
{
	public partial class XmlExportForm
	{

		#region Component Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ARInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARCreditNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARAdjustmentNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APCreditNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APAdjustmentNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportBatchNumberCalcEdit = new ZArchitecture.ZCalcEdit();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrganisationModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.NewExportBatchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HighWaterMarkLabel = new ZArchitecture.ZLabel();
			this.TransactionNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransactionNumbersFromZTextBox = new ZArchitecture.ZTextBox();
			this.TransactionNumbersToZTextBox = new ZArchitecture.ZTextBox();
			this.TransactionNumbersFilterLabel = new ZArchitecture.ZLabel();
			this.BranchModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.DepartmentModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.JobModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.OrganisationsLabel = new ZArchitecture.ZLabel();
			this.JobsLabel = new ZArchitecture.ZLabel();
			this.DepartmentsLabel = new ZArchitecture.ZLabel();
			this.BranchesLabel = new ZArchitecture.ZLabel();
			this.ExcludeTransactionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExcludeAPNonJobRelatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExcludeARNonJobRelatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExcludeARJobRelatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExcludeAPJobRelatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransactionTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UnallocatedAPCreditNotesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AccrualReversingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AccrualPostingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UnAllocatedAPInvoicesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WipReversalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WipPostingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PeriodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PeriodToPeriodEdit = new Enterprise.ZArchitecture.GUI.ZPeriodEdit();
			this.PeriodFromPeriodEdit = new Enterprise.ZArchitecture.GUI.ZPeriodEdit();
			this.zLabel4 = new ZArchitecture.ZLabel();
			this.DatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DatesFilterLabel = new ZArchitecture.ZLabel();
			this.ToZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FromZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportExistingBatchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.printDocument1 = new System.Drawing.Printing.PrintDocument();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).BeginInit();
			this.NewExportBatchGroupBox.SuspendLayout();
			this.TransactionNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DepartmentModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.JobModuleButtonGrid.InnerGrid)).BeginInit();
			this.ExcludeTransactionsGroupBox.SuspendLayout();
			this.TransactionTypesGroupBox.SuspendLayout();
			this.PeriodsGroupBox.SuspendLayout();
			this.DatesGroupBox.SuspendLayout();
			this.ExportExistingBatchGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 618, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(810);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(XmlExportGUIWrapper);
			// 
			// ARInvoiceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ARInvoiceCheckBox, "IncludeARInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeARInvoices)));
			this.ARInvoiceCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|11462edf-edb2-4674-8e69-51a327a6b0ca", "AR Invoice", "AR Invoice", "");
			this.ARInvoiceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.ARInvoiceCheckBox.Name = "ARInvoiceCheckBox";
			this.ARInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.ARInvoiceCheckBox.TabIndex = 0;
			// 
			// ARCreditNoteCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ARCreditNoteCheckBox, "IncludeARCreditNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeARCreditNotes)));
			this.ARCreditNoteCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|d3349330-bda5-4b94-b41b-203e60fa62eb", "AR Credit Note", "AR Credit Note", "");
			this.ARCreditNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARCreditNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.ARCreditNoteCheckBox.Name = "ARCreditNoteCheckBox";
			this.ARCreditNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.ARCreditNoteCheckBox.TabIndex = 2;
			// 
			// ARAdjustmentNoteCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ARAdjustmentNoteCheckBox, "IncludeARAdjustmentNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeARAdjustmentNotes)));
			this.ARAdjustmentNoteCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|c80f05df-9bca-4905-9106-60cd18a9a761", "AR Adjustment Note", "AR Adjustment Note", "");
			this.ARAdjustmentNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARAdjustmentNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 59, true);
			this.ARAdjustmentNoteCheckBox.Name = "ARAdjustmentNoteCheckBox";
			this.ARAdjustmentNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.ARAdjustmentNoteCheckBox.TabIndex = 4;
			// 
			// APInvoiceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.APInvoiceCheckBox, "IncludeAPInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeAPInvoices)));
			this.APInvoiceCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|fc3e1469-ae70-4cf8-ad8d-8241ca43971c", "AP Invoice", "AP Invoice", "");
			this.APInvoiceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.APInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 15, true);
			this.APInvoiceCheckBox.Name = "APInvoiceCheckBox";
			this.APInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.APInvoiceCheckBox.TabIndex = 1;
			// 
			// APCreditNoteCheckBox
			// 
			this.BindingSource.SetBindingMember(this.APCreditNoteCheckBox, "IncludeAPCreditNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeAPCreditNotes)));
			this.APCreditNoteCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|7d02b645-c9fa-4bc6-970a-05bc69343618", "AP Credit Note", "AP Credit Note", "");
			this.APCreditNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.APCreditNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 37, true);
			this.APCreditNoteCheckBox.Name = "APCreditNoteCheckBox";
			this.APCreditNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.APCreditNoteCheckBox.TabIndex = 3;
			// 
			// APAdjustmentNoteCheckBox
			// 
			this.BindingSource.SetBindingMember(this.APAdjustmentNoteCheckBox, "IncludeAPAdjustmentNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeAPAdjustmentNotes)));
			this.APAdjustmentNoteCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|8c567735-f328-4821-bd47-40fa154ede99", "AP Adjustment Note", "AP Adjustment Note", "");
			this.APAdjustmentNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.APAdjustmentNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 59, true);
			this.APAdjustmentNoteCheckBox.Name = "APAdjustmentNoteCheckBox";
			this.APAdjustmentNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 23, true);
			this.APAdjustmentNoteCheckBox.TabIndex = 5;
			// 
			// ExportBatchNumberCalcEdit
			// 
			this.ExportBatchNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ExportBatchNumberCalcEdit, "ExistingBatchNumberToExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((XmlExportGUIWrapper)(null)).ExistingBatchNumberToExport)));
			this.ExportBatchNumberCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|4a7de6aa-58e2-4283-9f73-cd3634dd3bad", "Batch Number", "Batch Number", "");
			this.ExportBatchNumberCalcEdit.Decimals = 0;
			this.ExportBatchNumberCalcEdit.IsCalculatorEnabled = false;
			this.ExportBatchNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 22, true);
			this.ExportBatchNumberCalcEdit.Name = "ExportBatchNumberCalcEdit";
			this.ExportBatchNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExportBatchNumberCalcEdit.TabIndex = 0;
			this.ExportBatchNumberCalcEdit.Text = "0";
			this.ExportBatchNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExportBatchNumberCalcEdit.WordWrap = false;
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ExportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|69d9ec0c-d427-44f3-be59-a1642078db0e", "&Export", "Export.");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(631, 596, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.ExportButton.TabIndex = 2;
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|b8703c06-15ec-44cb-a18c-cd012585e2c2", "&Close", "Close.");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(709, 596, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 3;
			// 
			// OrganisationModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.OrganisationModuleButtonGrid, "SelectedOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).SelectedOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).OrgHeadersList)));
			this.OrganisationModuleButtonGrid.BindToFindBoxList = "OrgHeadersList";
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.OrganisationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrganisationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrganisationModuleButtonGrid.GridId = "73ea30a5-24c5-4898-bdc4-b734b10f6a7f";
			// 
			// 
			// 
			this.OrganisationModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrganisationModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrganisationModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrganisationModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.OrganisationModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.OrganisationModuleButtonGrid.InnerGrid.Name = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.OrganisationModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.OrganisationModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.OrganisationModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 51, true);
			this.OrganisationModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.OrganisationModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 297, true);
			this.OrganisationModuleButtonGrid.Name = "OrganisationModuleButtonGrid";
			this.OrganisationModuleButtonGrid.ReadOnly = true;
			this.OrganisationModuleButtonGrid.ShowEditButton = false;
			this.OrganisationModuleButtonGrid.ShowNewButton = false;
			this.OrganisationModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 89, true);
			this.OrganisationModuleButtonGrid.TabIndex = 12;
			// 
			// NewExportBatchGroupBox
			// 
			this.NewExportBatchGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|7c9e82c8-3bc9-415a-8f16-4fd72561da7e", "Export New Batch");
			this.NewExportBatchGroupBox.Controls.Add(this.HighWaterMarkLabel);
			this.NewExportBatchGroupBox.Controls.Add(this.TransactionNumbersGroupBox);
			this.NewExportBatchGroupBox.Controls.Add(this.BranchModuleButtonGrid);
			this.NewExportBatchGroupBox.Controls.Add(this.DepartmentModuleButtonGrid);
			this.NewExportBatchGroupBox.Controls.Add(this.JobModuleButtonGrid);
			this.NewExportBatchGroupBox.Controls.Add(this.OrganisationsLabel);
			this.NewExportBatchGroupBox.Controls.Add(this.JobsLabel);
			this.NewExportBatchGroupBox.Controls.Add(this.DepartmentsLabel);
			this.NewExportBatchGroupBox.Controls.Add(this.BranchesLabel);
			this.NewExportBatchGroupBox.Controls.Add(this.ExcludeTransactionsGroupBox);
			this.NewExportBatchGroupBox.Controls.Add(this.TransactionTypesGroupBox);
			this.NewExportBatchGroupBox.Controls.Add(this.PeriodsGroupBox);
			this.NewExportBatchGroupBox.Controls.Add(this.DatesGroupBox);
			this.NewExportBatchGroupBox.Controls.Add(this.OrganisationModuleButtonGrid);
			this.NewExportBatchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.NewExportBatchGroupBox.Name = "NewExportBatchGroupBox";
			this.NewExportBatchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 489, true);
			this.NewExportBatchGroupBox.TabIndex = 0;
			this.NewExportBatchGroupBox.TabStop = false;
			// 
			// HighWaterMarkLabel
			// 
			this.BindingSource.SetBindingMember(this.HighWaterMarkLabel, "HighWaterMarkMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((XmlExportGUIWrapper)(null)).HighWaterMarkMessage)));
			this.HighWaterMarkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 389, true);
			this.HighWaterMarkLabel.Name = "HighWaterMarkLabel";
			this.HighWaterMarkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 85, true);
			this.HighWaterMarkLabel.TabIndex = 13;
			// 
			// TransactionNumbersGroupBox
			// 
			this.TransactionNumbersGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|379cd07d-3bf6-48ee-9783-b2ba936e27ed", "Transaction Numbers");
			this.TransactionNumbersGroupBox.Controls.Add(this.TransactionNumbersFromZTextBox);
			this.TransactionNumbersGroupBox.Controls.Add(this.TransactionNumbersToZTextBox);
			this.TransactionNumbersGroupBox.Controls.Add(this.TransactionNumbersFilterLabel);
			this.TransactionNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 208, true);
			this.TransactionNumbersGroupBox.Name = "TransactionNumbersGroupBox";
			this.TransactionNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 57, true);
			this.TransactionNumbersGroupBox.TabIndex = 1;
			this.TransactionNumbersGroupBox.TabStop = false;
			// 
			// TransactionNumbersFromZTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionNumbersFromZTextBox, "TransactionNumberFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((XmlExportGUIWrapper)(null)).TransactionNumberFrom)));
			this.TransactionNumbersFromZTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|d7cd3414-39b8-4e03-9aa5-46f4c12e30f4", "From", "From", "");
			this.TransactionNumbersFromZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 33, true);
			this.TransactionNumbersFromZTextBox.Name = "TransactionNumbersFromZTextBox";
			this.TransactionNumbersFromZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TransactionNumbersFromZTextBox.TabIndex = 1;
			// 
			// TransactionNumbersToZTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionNumbersToZTextBox, "TransactionNumberTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((XmlExportGUIWrapper)(null)).TransactionNumberTo)));
			this.TransactionNumbersToZTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|67a7f692-8f8b-4640-82a2-d7434e0bbc35", "To", "To", "");
			this.TransactionNumbersToZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 33, true);
			this.TransactionNumbersToZTextBox.Name = "TransactionNumbersToZTextBox";
			this.TransactionNumbersToZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TransactionNumbersToZTextBox.TabIndex = 2;
			// 
			// TransactionNumbersFilterLabel
			// 
			this.TransactionNumbersFilterLabel.AutoSize = true;
			this.TransactionNumbersFilterLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|f2875317-efff-4344-a243-531684a3c53c", "Leave empty for all Transactions");
			this.TransactionNumbersFilterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.TransactionNumbersFilterLabel.Name = "TransactionNumbersFilterLabel";
			this.TransactionNumbersFilterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 13, true);
			this.TransactionNumbersFilterLabel.TabIndex = 0;
			// 
			// BranchModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.BranchModuleButtonGrid, "SelectedBranches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).SelectedBranches)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).BranchesList)));
			this.BranchModuleButtonGrid.BindToFindBoxList = "BranchesList";
			zTextBoxColumnStyleInfo3.ColumnName = "GB_Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.ColumnName = "GB_BranchName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.BranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.BranchModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.BranchModuleButtonGrid.GridId = "2ca1a1b9-2bea-4b5a-8d74-3ee6399932d3";
			// 
			// 
			// 
			this.BranchModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.BranchModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BranchModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.BranchModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BranchModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.BranchModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.BranchModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.BranchModuleButtonGrid.InnerGrid.Name = "Grid";
			this.BranchModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.BranchModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.BranchModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.BranchModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 51, true);
			this.BranchModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.BranchModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 22, true);
			this.BranchModuleButtonGrid.Name = "BranchModuleButtonGrid";
			this.BranchModuleButtonGrid.ReadOnly = true;
			this.BranchModuleButtonGrid.ShowEditButton = false;
			this.BranchModuleButtonGrid.ShowNewButton = false;
			this.BranchModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 89, true);
			this.BranchModuleButtonGrid.TabIndex = 6;
			// 
			// DeparmentModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.DepartmentModuleButtonGrid, "SelectedDepartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).SelectedDepartments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).DepartmentsList)));
			this.DepartmentModuleButtonGrid.BindToFindBoxList = "DepartmentsList";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ZModuleButtonGrid|92cbba4a-09fe-473c-be24-727c2c2972ff", "Code");
			zTextBoxColumnStyleInfo5.ColumnName = "GE_Code";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ZModuleButtonGrid|d31c62e6-73e8-4b1a-9d48-58899dc30b17", "Department Description");
			zTextBoxColumnStyleInfo6.ColumnName = "GE_Desc";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.DepartmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DepartmentModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DepartmentModuleButtonGrid.GridId = "fd140de6-442c-454e-b58c-8414915c0c3f";
			// 
			// 
			// 
			this.DepartmentModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DepartmentModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DepartmentModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DepartmentModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DepartmentModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.DepartmentModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DepartmentModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.DepartmentModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DepartmentModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.DepartmentModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.DepartmentModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.DepartmentModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 52, true);
			this.DepartmentModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DepartmentModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 111, true);
			this.DepartmentModuleButtonGrid.Name = "DeparmentModuleButtonGrid";
			this.DepartmentModuleButtonGrid.ReadOnly = true;
			this.DepartmentModuleButtonGrid.ShowEditButton = false;
			this.DepartmentModuleButtonGrid.ShowNewButton = false;
			this.DepartmentModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 90, true);
			this.DepartmentModuleButtonGrid.TabIndex = 8;
			// 
			// JobModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.JobModuleButtonGrid, "SelectedJobs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).SelectedJobs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((XmlExportGUIWrapper)(null)).JobHeaderList)));
			this.JobModuleButtonGrid.BindToFindBoxList = "JobHeaderList";
			zTextBoxColumnStyleInfo7.ColumnName = "JH_JobNum";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.ColumnName = "JH_Status";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.JobModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.JobModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.JobModuleButtonGrid.GridId = "0722ceb3-842b-4f85-9fdd-154ae81f0ab7";
			// 
			// 
			// 
			this.JobModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.JobModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.JobModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.JobModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.JobModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.JobModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.JobModuleButtonGrid.InnerGrid.Name = "Grid";
			this.JobModuleButtonGrid.InnerGrid.ReadOnly = true;
			this.JobModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.JobModuleButtonGrid.InnerGrid.RowHeadersVisible = false;
			this.JobModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 51, true);
			this.JobModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.JobModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 201, true);
			this.JobModuleButtonGrid.Name = "JobModuleButtonGrid";
			this.JobModuleButtonGrid.ReadOnly = true;
			this.JobModuleButtonGrid.ShowEditButton = false;
			this.JobModuleButtonGrid.ShowNewButton = false;
			this.JobModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 89, true);
			this.JobModuleButtonGrid.TabIndex = 10;
			// 
			// OrganisationsLabel
			// 
			this.OrganisationsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|9b108f91-cda1-428a-af06-0fbcb5a80183", "Organizations");
			this.OrganisationsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 297, true);
			this.OrganisationsLabel.Name = "OrganisationsLabel";
			this.OrganisationsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.OrganisationsLabel.TabIndex = 11;
			// 
			// JobsLabel
			// 
			this.JobsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|3f7996a9-443e-4bf2-9fd0-ff05265e6879", "Jobs");
			this.JobsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 201, true);
			this.JobsLabel.Name = "JobsLabel";
			this.JobsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
			this.JobsLabel.TabIndex = 9;
			// 
			// DepartmentsLabel
			// 
			this.DepartmentsLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|9578f749-7851-4d69-aafa-413f12cfcec7", "Departments");
			this.DepartmentsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 111, true);
			this.DepartmentsLabel.Name = "DepartmentsLabel";
			this.DepartmentsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.DepartmentsLabel.TabIndex = 7;
			// 
			// BranchesLabel
			// 
			this.BranchesLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|7eefbad3-c943-4e50-ad7c-a3758bf62034", "Branches");
			this.BranchesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 22, true);
			this.BranchesLabel.Name = "BranchesLabel";
			this.BranchesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.BranchesLabel.TabIndex = 5;
			// 
			// ExcludeTransactionsGroupBox
			// 
			this.ExcludeTransactionsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|8b3a0ad7-3f11-4486-8055-a873cda4535c", "Exclude Transactions");
			this.ExcludeTransactionsGroupBox.Controls.Add(this.ExcludeAPNonJobRelatedCheckBox);
			this.ExcludeTransactionsGroupBox.Controls.Add(this.ExcludeARNonJobRelatedCheckBox);
			this.ExcludeTransactionsGroupBox.Controls.Add(this.ExcludeARJobRelatedCheckBox);
			this.ExcludeTransactionsGroupBox.Controls.Add(this.ExcludeAPJobRelatedCheckBox);
			this.ExcludeTransactionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 271, true);
			this.ExcludeTransactionsGroupBox.Name = "ExcludeTransactionsGroupBox";
			this.ExcludeTransactionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 67, true);
			this.ExcludeTransactionsGroupBox.TabIndex = 2;
			this.ExcludeTransactionsGroupBox.TabStop = false;
			// 
			// ExcludeAPNonJobRelatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExcludeAPNonJobRelatedCheckBox, "ExcludeNonJobRelatedTransactionsForAP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).ExcludeNonJobRelatedTransactionsForAP)));
			this.ExcludeAPNonJobRelatedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|da0642b3-dc2c-42c6-9f23-82b8f085ab95", "AP Non Job Related", "AP Non Job Related", "");
			this.ExcludeAPNonJobRelatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcludeAPNonJobRelatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 37, true);
			this.ExcludeAPNonJobRelatedCheckBox.Name = "ExcludeAPNonJobRelatedCheckBox";
			this.ExcludeAPNonJobRelatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.ExcludeAPNonJobRelatedCheckBox.TabIndex = 3;
			// 
			// ExcludeARNonJobRelatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExcludeARNonJobRelatedCheckBox, "ExcludeNonJobRelatedTransactionsForAR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).ExcludeNonJobRelatedTransactionsForAR)));
			this.ExcludeARNonJobRelatedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|137123a9-752f-473f-8838-c12858e2344d", "AR Non Job Related", "AR Non Job Related", "");
			this.ExcludeARNonJobRelatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcludeARNonJobRelatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.ExcludeARNonJobRelatedCheckBox.Name = "ExcludeARNonJobRelatedCheckBox";
			this.ExcludeARNonJobRelatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.ExcludeARNonJobRelatedCheckBox.TabIndex = 2;
			// 
			// ExcludeARJobRelatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExcludeARJobRelatedCheckBox, "ExcludeJobRelatedTransactionsForAR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).ExcludeJobRelatedTransactionsForAR)));
			this.ExcludeARJobRelatedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|602f647a-ca67-4b7d-88e5-beb5118f9ac3", "AR Job Related", "AR Job Related", "");
			this.ExcludeARJobRelatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcludeARJobRelatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.ExcludeARJobRelatedCheckBox.Name = "ExcludeARJobRelatedCheckBox";
			this.ExcludeARJobRelatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.ExcludeARJobRelatedCheckBox.TabIndex = 0;
			// 
			// ExcludeAPJobRelatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExcludeAPJobRelatedCheckBox, "ExcludeJobRelatedTransactionsForAP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).ExcludeJobRelatedTransactionsForAP)));
			this.ExcludeAPJobRelatedCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|8a9a37ca-7b07-408d-b77f-bb13bc521380", "AP Job Related", "AP Job Related", "");
			this.ExcludeAPJobRelatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcludeAPJobRelatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 15, true);
			this.ExcludeAPJobRelatedCheckBox.Name = "ExcludeAPJobRelatedCheckBox";
			this.ExcludeAPJobRelatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.ExcludeAPJobRelatedCheckBox.TabIndex = 1;
			// 
			// TransactionTypesGroupBox
			// 
			this.TransactionTypesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|f2758922-4382-4bcd-a77c-e805ffb3ee3a", "Transaction Types");
			this.TransactionTypesGroupBox.Controls.Add(this.APAdjustmentNoteCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.UnallocatedAPCreditNotesCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.AccrualReversingCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.AccrualPostingCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.UnAllocatedAPInvoicesCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.WipReversalCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.WipPostingCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.ARInvoiceCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.ARCreditNoteCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.ARAdjustmentNoteCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.APInvoiceCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.APCreditNoteCheckBox);
			this.TransactionTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 22, true);
			this.TransactionTypesGroupBox.Name = "TransactionTypesGroupBox";
			this.TransactionTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 179, true);
			this.TransactionTypesGroupBox.TabIndex = 0;
			this.TransactionTypesGroupBox.TabStop = false;
			// 
			// UnallocatedAPCreditNotesCheckBox
			// 
			this.UnallocatedAPCreditNotesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UnallocatedAPCreditNotesCheckBox, "IncludeUnallocatedAPCreditNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeUnallocatedAPCreditNotes)));
			this.UnallocatedAPCreditNotesCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|8dafa83c-3602-419b-8d0c-32fb3ef9e77d", "Unallocated AP Credit Notes");
			this.UnallocatedAPCreditNotesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnallocatedAPCreditNotesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 151, true);
			this.UnallocatedAPCreditNotesCheckBox.Name = "UnallocatedAPCreditNotesCheckBox";
			this.UnallocatedAPCreditNotesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 22, true);
			this.UnallocatedAPCreditNotesCheckBox.TabIndex = 11;
			// 
			// AccrualReversingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AccrualReversingCheckBox, "IncludeAccrualsReversing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeAccrualsReversing)));
			this.AccrualReversingCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|10016517-14fd-487b-a8f0-9c1068cdbf0f", "Accrual Reversal", "Accrual Reversal", "");
			this.AccrualReversingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AccrualReversingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 104, true);
			this.AccrualReversingCheckBox.Name = "AccrualReversingCheckBox";
			this.AccrualReversingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.AccrualReversingCheckBox.TabIndex = 9;
			// 
			// AccrualPostingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AccrualPostingCheckBox, "IncludeAccrualsPosting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeAccrualsPosting)));
			this.AccrualPostingCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|fe9363d7-1e22-4c3c-92bb-092cf9574fd7", "Accrual Posting", "Accrual Posting", "");
			this.AccrualPostingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AccrualPostingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 82, true);
			this.AccrualPostingCheckBox.Name = "AccrualPostingCheckBox";
			this.AccrualPostingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.AccrualPostingCheckBox.TabIndex = 7;
			// 
			// UnAllocatedAPInvoicesCheckBox
			// 
			this.UnAllocatedAPInvoicesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UnAllocatedAPInvoicesCheckBox, "IncludeUnallocatedAPInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeUnallocatedAPInvoices)));
			this.UnAllocatedAPInvoicesCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|d5bc58ef-09e6-4841-b05a-0470e040ee9c", "Unallocated AP Invoices");
			this.UnAllocatedAPInvoicesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnAllocatedAPInvoicesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 127, true);
			this.UnAllocatedAPInvoicesCheckBox.Name = "UnAllocatedAPInvoicesCheckBox";
			this.UnAllocatedAPInvoicesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 22, true);
			this.UnAllocatedAPInvoicesCheckBox.TabIndex = 10;
			// 
			// WipReversalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.WipReversalCheckBox, "IncludeWipsReversing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeWipsReversing)));
			this.WipReversalCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|f36affbe-d209-4ec4-ad14-ce9ed549bcbb", "WIP Reversal", "WIP Reversal", "");
			this.WipReversalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WipReversalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.WipReversalCheckBox.Name = "WipReversalCheckBox";
			this.WipReversalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.WipReversalCheckBox.TabIndex = 8;
			// 
			// WipPostingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.WipPostingCheckBox, "IncludeWipsPosting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((XmlExportGUIWrapper)(null)).IncludeWipsPosting)));
			this.WipPostingCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|59d5bb73-a00c-46f1-859f-20d341ff1c49", "WIP Posting", "WIP Posting", "");
			this.WipPostingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WipPostingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 82, true);
			this.WipPostingCheckBox.Name = "WipPostingCheckBox";
			this.WipPostingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 22, true);
			this.WipPostingCheckBox.TabIndex = 6;
			// 
			// PeriodsGroupBox
			// 
			this.PeriodsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|5db35fab-55e7-42ab-89a0-696edfb50790", "Periods");
			this.PeriodsGroupBox.Controls.Add(this.PeriodToPeriodEdit);
			this.PeriodsGroupBox.Controls.Add(this.PeriodFromPeriodEdit);
			this.PeriodsGroupBox.Controls.Add(this.zLabel4);
			this.PeriodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 417, true);
			this.PeriodsGroupBox.Name = "PeriodsGroupBox";
			this.PeriodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 66, true);
			this.PeriodsGroupBox.TabIndex = 4;
			this.PeriodsGroupBox.TabStop = false;
			// 
			// PeriofToPeriodEdit
			// 
			this.PeriodToPeriodEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PeriodToPeriodEdit, "PeriodTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((XmlExportGUIWrapper)(null)).PeriodTo)));
			this.PeriodToPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|a47ffead-bd1c-4208-b731-12c088db1e9d", "To", "To", "");
			this.PeriodToPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 37, true);
			this.PeriodToPeriodEdit.Name = "PeriofToPeriodEdit";
			this.PeriodToPeriodEdit.TabIndex = 2;
			// 
			// PeriodFromPeriodEdit
			// 
			this.PeriodFromPeriodEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PeriodFromPeriodEdit, "PeriodFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((XmlExportGUIWrapper)(null)).PeriodFrom)));
			this.PeriodFromPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|0f13ac3a-6463-4245-a7bd-005d9ac6d00c", "From", "From", "");
			this.PeriodFromPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 37, true);
			this.PeriodFromPeriodEdit.Name = "PeriodFromPeriodEdit";
			this.PeriodFromPeriodEdit.TabIndex = 1;
			// 
			// zLabel4
			// 
			this.zLabel4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|909f2286-fe6a-4897-a06c-1d74192b5ee1", "Leave empty for all periods");
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 21, true);
			this.zLabel4.TabIndex = 0;
			// 
			// DatesGroupBox
			// 
			this.DatesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|baf39e71-1aee-4285-b4cf-2c58326e5ad3", "Dates");
			this.DatesGroupBox.Controls.Add(this.DatesFilterLabel);
			this.DatesGroupBox.Controls.Add(this.ToZDateEdit);
			this.DatesGroupBox.Controls.Add(this.FromZDateEdit);
			this.DatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 344, true);
			this.DatesGroupBox.Name = "DatesGroupBox";
			this.DatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 67, true);
			this.DatesGroupBox.TabIndex = 3;
			this.DatesGroupBox.TabStop = false;
			// 
			// DatesFilterLabel
			// 
			this.DatesFilterLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|cf82336c-ff9e-4063-b060-c18c0f1836eb", "Leave empty for all dates");
			this.DatesFilterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.DatesFilterLabel.Name = "DatesFilterLabel";
			this.DatesFilterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 21, true);
			this.DatesFilterLabel.TabIndex = 0;
			// 
			// ToZDateEdit
			// 
			this.ToZDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToZDateEdit, "DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((XmlExportGUIWrapper)(null)).DateTo)));
			this.ToZDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|8381b2d6-7383-4d9a-a744-91b2174df43f", "To", "To", "");
			this.ToZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 37, true);
			this.ToZDateEdit.Name = "ToZDateEdit";
			this.ToZDateEdit.TabIndex = 2;
			// 
			// FromZDateEdit
			// 
			this.FromZDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromZDateEdit, "DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((XmlExportGUIWrapper)(null)).DateFrom)));
			this.FromZDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|054f8997-e833-4c7d-87f3-cf17356c37c9", "From", "From", "");
			this.FromZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 37, true);
			this.FromZDateEdit.Name = "FromZDateEdit";
			this.FromZDateEdit.TabIndex = 1;
			// 
			// ExportExistingBatchGroupBox
			// 
			this.ExportExistingBatchGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|c4664c7d-ff33-4677-94ec-7e7533bc0ecc", "Export Existing Batch");
			this.ExportExistingBatchGroupBox.Controls.Add(this.ExportBatchNumberCalcEdit);
			this.ExportExistingBatchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 502, true);
			this.ExportExistingBatchGroupBox.Name = "ExportExistingBatchGroupBox";
			this.ExportExistingBatchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 52, true);
			this.ExportExistingBatchGroupBox.TabIndex = 1;
			this.ExportExistingBatchGroupBox.TabStop = false;
			// 
			// XmlExportForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 640, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|4d3d5a22-fd30-4752-bdb6-26c64e0b8996", "Export Accounting Transactions");
			this.Controls.Add(this.ExportExistingBatchGroupBox);
			this.Controls.Add(this.NewExportBatchGroupBox);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.GUI";
			this.DataSourceType = typeof(XmlExportGUIWrapper);
			this.DataSourceTypeName = "Enterprise.Accounting.GUI.XmlExport.XmlExportGUIWrapper";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Menu = null;
			this.MinimizeBox = false;
			this.Name = "XmlExportForm";
			this.RememberFormPosition = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ExportButton, 0);
			this.Controls.SetChildIndex(this.NewExportBatchGroupBox, 0);
			this.Controls.SetChildIndex(this.ExportExistingBatchGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).EndInit();
			this.NewExportBatchGroupBox.ResumeLayout(false);
			this.TransactionNumbersGroupBox.ResumeLayout(false);
			this.TransactionNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BranchModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DepartmentModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.JobModuleButtonGrid.InnerGrid)).EndInit();
			this.ExcludeTransactionsGroupBox.ResumeLayout(false);
			this.TransactionTypesGroupBox.ResumeLayout(false);
			this.PeriodsGroupBox.ResumeLayout(false);
			this.PeriodsGroupBox.PerformLayout();
			this.DatesGroupBox.ResumeLayout(false);
			this.ExportExistingBatchGroupBox.ResumeLayout(false);
			this.ExportExistingBatchGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		protected internal ZArchitecture.ZCalcEdit ExportBatchNumberCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZButton ExportButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox ARInvoiceCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox ARCreditNoteCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox ARAdjustmentNoteCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox APInvoiceCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox APCreditNoteCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox APAdjustmentNoteCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZModuleButtonGrid OrganisationModuleButtonGrid;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox NewExportBatchGroupBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox WipPostingCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox WipReversalCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox AccrualPostingCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox AccrualReversingCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox ExcludeARJobRelatedCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox ExcludeARNonJobRelatedCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox ExcludeAPJobRelatedCheckBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCheckBox ExcludeAPNonJobRelatedCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ExportExistingBatchGroupBox;
		protected System.Drawing.Printing.PrintDocument printDocument1;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit FromZDateEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit ToZDateEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox DatesGroupBox;
		protected ZArchitecture.ZLabel DatesFilterLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox PeriodsGroupBox;
		protected ZArchitecture.ZLabel zLabel4;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox TransactionTypesGroupBox;
		protected internal Enterprise.ZArchitecture.GUI.ZGroupBox ExcludeTransactionsGroupBox;
		protected ZArchitecture.ZLabel BranchesLabel;
		protected ZArchitecture.ZLabel DepartmentsLabel;
		protected ZArchitecture.ZLabel JobsLabel;
		protected ZArchitecture.ZLabel OrganisationsLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZModuleButtonGrid DepartmentModuleButtonGrid;
		protected internal Enterprise.ZArchitecture.GUI.ZModuleButtonGrid BranchModuleButtonGrid;
		protected internal Enterprise.ZArchitecture.GUI.ZModuleButtonGrid JobModuleButtonGrid;
		protected internal Enterprise.ZArchitecture.GUI.ZPeriodEdit PeriodFromPeriodEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZPeriodEdit PeriodToPeriodEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox TransactionNumbersGroupBox;
		private ZArchitecture.ZLabel TransactionNumbersFilterLabel;
		protected ZArchitecture.ZTextBox TransactionNumbersToZTextBox;
		protected ZArchitecture.ZTextBox TransactionNumbersFromZTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox UnallocatedAPCreditNotesCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox UnAllocatedAPInvoicesCheckBox;
		protected internal ZArchitecture.ZLabel HighWaterMarkLabel;
	}
}
