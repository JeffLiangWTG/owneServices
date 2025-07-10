using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.Rohlig.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Client.Rohlig.Bellin
{
	public partial class BellinExportForm : ZForm
	{
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ExportExistingBatchGroupBox;
		protected Enterprise.ZArchitecture.ZLabel zLabel1;
		protected Enterprise.ZArchitecture.ZCalcEdit ExportBatchNumberCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox NewExportBatchGroupBox;
		protected Enterprise.ZArchitecture.ZLabel OrganisationsLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox TransactionTypesGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox APAdjustmentNoteCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox APInvoiceCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox APCreditNoteCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox DatesGroupBox;
		protected Enterprise.ZArchitecture.ZLabel DatesToZLabel;
		protected Enterprise.ZArchitecture.ZLabel DatesFromZLabel;
		protected Enterprise.ZArchitecture.ZLabel DatesFilterLabel;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private ZModuleButtonGrid OrganisationModuleButtonGrid;
		private Enterprise.ZArchitecture.ZLabel AccountGroupLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton ExportButton;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox AccountCreditorGroupFindBox;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ExportExistingBatchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ExportBatchNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NewExportBatchGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AccountCreditorGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AccountGroupLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OrganisationsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TransactionTypesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.APAdjustmentNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APInvoiceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.APCreditNoteCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DatesToZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DatesFromZLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DatesFilterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OrganisationModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportExistingBatchGroupBox.SuspendLayout();
			this.NewExportBatchGroupBox.SuspendLayout();
			this.TransactionTypesGroupBox.SuspendLayout();
			this.DatesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 313, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper);
			// 
			// ExportExistingBatchGroupBox
			// 
			this.ExportExistingBatchGroupBox.Controls.Add(this.zLabel1);
			this.ExportExistingBatchGroupBox.Controls.Add(this.ExportBatchNumberCalcEdit);
			this.ExportExistingBatchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 232, true);
			this.ExportExistingBatchGroupBox.Name = "ExportExistingBatchGroupBox";
			this.ExportExistingBatchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 56, true);
			this.ExportExistingBatchGroupBox.TabIndex = 0;
			this.ExportExistingBatchGroupBox.TabStop = false;
			this.ExportExistingBatchGroupBox.Text = "Export Existing Batch";
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 13, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Batch Number:";
			// 
			// ExportBatchNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExportBatchNumberCalcEdit, "ExistingBatchNumberToExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).ExistingBatchNumberToExport)));
			this.ExportBatchNumberCalcEdit.Decimals = 0;
			this.ExportBatchNumberCalcEdit.IsCalculatorEnabled = false;
			this.ExportBatchNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 24, true);
			this.ExportBatchNumberCalcEdit.Name = "ExportBatchNumberCalcEdit";
			this.ExportBatchNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ExportBatchNumberCalcEdit.TabIndex = 0;
			this.ExportBatchNumberCalcEdit.Text = "0";
			this.ExportBatchNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExportBatchNumberCalcEdit.WordWrap = false;
			// 
			// NewExportBatchGroupBox
			// 
			this.NewExportBatchGroupBox.Controls.Add(this.AccountCreditorGroupFindBox);
			this.NewExportBatchGroupBox.Controls.Add(this.AccountGroupLabel);
			this.NewExportBatchGroupBox.Controls.Add(this.OrganisationsLabel);
			this.NewExportBatchGroupBox.Controls.Add(this.TransactionTypesGroupBox);
			this.NewExportBatchGroupBox.Controls.Add(this.DatesGroupBox);
			this.NewExportBatchGroupBox.Controls.Add(this.OrganisationModuleButtonGrid);
			this.NewExportBatchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.NewExportBatchGroupBox.Name = "NewExportBatchGroupBox";
			this.NewExportBatchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 216, true);
			this.NewExportBatchGroupBox.TabIndex = 0;
			this.NewExportBatchGroupBox.TabStop = false;
			this.NewExportBatchGroupBox.Text = "Export New Batch";
			// 
			// AccountCreditorGroupFindBox
			// 
			this.BindingSource.SetBindingMember(this.AccountCreditorGroupFindBox, "APCreditorGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).APCreditorGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).AccountCreditorGroup)));
			this.AccountCreditorGroupFindBox.BindToList = "AccountCreditorGroup";
			this.AccountCreditorGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 176, true);
			this.AccountCreditorGroupFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgCreditorGroup;
			this.AccountCreditorGroupFindBox.Name = "AccountCreditorGroupFindBox";
			this.AccountCreditorGroupFindBox.PopupCaption = "Select Creditor Group";
			this.AccountCreditorGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.AccountCreditorGroupFindBox.TabIndex = 2;
			// 
			// AccountGroupLabel
			// 
			this.AccountGroupLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 152, true);
			this.AccountGroupLabel.Name = "AccountGroupLabel";
			this.AccountGroupLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.AccountGroupLabel.TabIndex = 0;
			this.AccountGroupLabel.Text = "Account Group";
			// 
			// OrganisationsLabel
			// 
			this.OrganisationsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 24, true);
			this.OrganisationsLabel.Name = "OrganisationsLabel";
			this.OrganisationsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.OrganisationsLabel.TabIndex = 0;
			this.OrganisationsLabel.Text = "Organisations";
			// 
			// TransactionTypesGroupBox
			// 
			this.TransactionTypesGroupBox.Controls.Add(this.APAdjustmentNoteCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.APInvoiceCheckBox);
			this.TransactionTypesGroupBox.Controls.Add(this.APCreditNoteCheckBox);
			this.TransactionTypesGroupBox.Enabled = false;
			this.TransactionTypesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.TransactionTypesGroupBox.Name = "TransactionTypesGroupBox";
			this.TransactionTypesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 96, true);
			this.TransactionTypesGroupBox.TabIndex = 0;
			this.TransactionTypesGroupBox.TabStop = false;
			this.TransactionTypesGroupBox.Text = "Transaction Types";
			// 
			// APAdjustmentNoteCheckBox
			// 
			this.BindingSource.SetBindingMember(this.APAdjustmentNoteCheckBox, "IncludeAPAdjustmentNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).IncludeAPAdjustmentNotes)));
			this.APAdjustmentNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.APAdjustmentNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 64, true);
			this.APAdjustmentNoteCheckBox.Name = "APAdjustmentNoteCheckBox";
			this.APAdjustmentNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 24, true);
			this.APAdjustmentNoteCheckBox.TabIndex = 0;
			this.APAdjustmentNoteCheckBox.Text = "AP Adjustment Note";
			// 
			// APInvoiceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.APInvoiceCheckBox, "IncludeAPInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).IncludeAPInvoices)));
			this.APInvoiceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.APInvoiceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.APInvoiceCheckBox.Name = "APInvoiceCheckBox";
			this.APInvoiceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 24, true);
			this.APInvoiceCheckBox.TabIndex = 0;
			this.APInvoiceCheckBox.Text = "AP Invoice";
			// 
			// APCreditNoteCheckBox
			// 
			this.BindingSource.SetBindingMember(this.APCreditNoteCheckBox, "IncludeAPCreditNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).IncludeAPCreditNotes)));
			this.APCreditNoteCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.APCreditNoteCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 40, true);
			this.APCreditNoteCheckBox.Name = "APCreditNoteCheckBox";
			this.APCreditNoteCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 24, true);
			this.APCreditNoteCheckBox.TabIndex = 0;
			this.APCreditNoteCheckBox.Text = "AP Credit Note";
			// 
			// DatesGroupBox
			// 
			this.DatesGroupBox.Controls.Add(this.DatesToZLabel);
			this.DatesGroupBox.Controls.Add(this.DatesFromZLabel);
			this.DatesGroupBox.Controls.Add(this.DatesFilterLabel);
			this.DatesGroupBox.Controls.Add(this.zDateEdit2);
			this.DatesGroupBox.Controls.Add(this.zDateEdit1);
			this.DatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 128, true);
			this.DatesGroupBox.Name = "DatesGroupBox";
			this.DatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 72, true);
			this.DatesGroupBox.TabIndex = 0;
			this.DatesGroupBox.TabStop = false;
			this.DatesGroupBox.Text = "Invoice Dates";
			// 
			// DatesToZLabel
			// 
			this.DatesToZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 39, true);
			this.DatesToZLabel.Name = "DatesToZLabel";
			this.DatesToZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.DatesToZLabel.TabIndex = 0;
			this.DatesToZLabel.Text = "To";
			// 
			// DatesFromZLabel
			// 
			this.DatesFromZLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.DatesFromZLabel.Name = "DatesFromZLabel";
			this.DatesFromZLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 23, true);
			this.DatesFromZLabel.TabIndex = 0;
			this.DatesFromZLabel.Text = "From";
			// 
			// DatesFilterLabel
			// 
			this.DatesFilterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.DatesFilterLabel.Name = "DatesFilterLabel";
			this.DatesFilterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 23, true);
			this.DatesFilterLabel.TabIndex = 0;
			this.DatesFilterLabel.Text = "Must Specify From And To Invoice Dates";
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).DateTo)));
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 40, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).DateFrom)));
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 40, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 1;
			// 
			// OrganisationModuleButtonGrid
			// 
			this.OrganisationModuleButtonGrid.AttachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("EF357626-A795-4B4A-B987-F52029BB9A3C", "Add");
			this.BindingSource.SetBindingMember(this.OrganisationModuleButtonGrid, "SelectedOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).SelectedOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper)(null)).OrgHeadersList)));
			this.OrganisationModuleButtonGrid.BindToFindBoxList = "OrgHeadersList";
			zTextBoxColumnStyleInfo1.Caption = "Code";
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.Caption = "Name";
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.OrganisationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrganisationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrganisationModuleButtonGrid.GridId = "4680573e-df2f-464c-a869-9c99ab0851e1";
			this.OrganisationModuleButtonGrid.DetachButtonText = CargoWiseOne.ResourceStrings.Res.GetData("C5FBE59D-46C9-41A9-92D5-29AF670F3E94", "Remove");
			this.OrganisationModuleButtonGrid.DetachMessage = CargoWiseOne.ResourceStrings.Res.GetData("F6E876C4-E160-4D60-93DB-F8B87974554D", "Are you sure you want to remove the selected organisation?");
			// 
			// 
			// 
			this.OrganisationModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrganisationModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrganisationModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrganisationModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.OrganisationModuleButtonGrid.InnerGrid.Name = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 58, true);
			this.OrganisationModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.OrganisationModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(328, 48, true);
			this.OrganisationModuleButtonGrid.Name = "OrganisationModuleButtonGrid";
			this.OrganisationModuleButtonGrid.NameOfAGridElement = CargoWiseOne.ResourceStrings.Res.GetData("EC82EE90-4118-421C-A496-1E1639D82FA0", "Organisation");
			this.OrganisationModuleButtonGrid.ReadOnly = false;
			this.OrganisationModuleButtonGrid.ShowEditButton = false;
			this.OrganisationModuleButtonGrid.ShowNewButton = false;
			this.OrganisationModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 96, true);
			this.OrganisationModuleButtonGrid.TabIndex = 1;
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(511, 289, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExportButton.TabIndex = 1;
			this.ExportButton.Text = "&Export";
			this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 289, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Text = "&Close";
			// 
			// BellinExportForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 335, true);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.NewExportBatchGroupBox);
			this.Controls.Add(this.ExportExistingBatchGroupBox);
			this.DataSourceAssemblyName = "ZClientROH";
			this.DataSourceType = typeof(Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper);
			this.DataSourceTypeName = "Enterprise.Client.Rohlig.GUI.BellinExportGUIWrapper";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 392, true);
			this.Menu = null;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 392, true);
			this.Name = "BellinExportForm";
			this.Text = "BellinExportForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ExportExistingBatchGroupBox, 0);
			this.Controls.SetChildIndex(this.NewExportBatchGroupBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ExportButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportExistingBatchGroupBox.ResumeLayout(false);
			this.ExportExistingBatchGroupBox.PerformLayout();
			this.NewExportBatchGroupBox.ResumeLayout(false);
			this.TransactionTypesGroupBox.ResumeLayout(false);
			this.DatesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
