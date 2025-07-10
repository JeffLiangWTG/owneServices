using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoicingForm
	{


		#region Windows Form Designer generated code

		protected Core.Forms.ZPostOrCancelButton CloseButton;
		protected Core.Forms.ZPostOrCancelButton PostAndCloseButton;
		protected ZDateEdit DateEdit;
		protected ZPanel BottomButtonPanel;
		protected ZPanel TopPanel;
		protected ZDateEdit DueDateDateEdit;
		protected ZArchitecture.ZCalcEdit TermDaysCalcEdit;
		protected ZDropEdit TermsDropEdit;
		protected ZGuidFindBox DebtorGuidFindBox;
		protected ZCodeFindBox CurrencyCodeFindBox;
		protected PeriodicInvoiceControl periodicInvoiceControl;
		protected Core.Forms.ZPostOrCancelButton PostButton;
		protected ZDateEdit PostDateEdit;
		protected ZDropEdit InvoiceTypeDropEdit;
		protected ZGuidFindBox TaxBranchFindBox;

		new void InitializeComponent()
		{
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.PostAndCloseButton = new Core.Forms.ZPostOrCancelButton();
			this.DateEdit = new ZDateEdit();
			this.BottomButtonPanel = new ZPanel();
			this.PostButton = new Core.Forms.ZPostOrCancelButton();
			this.TopPanel = new ZPanel();
			this.SellReferenceTextBox = new ZArchitecture.ZTextBox();
			this.jobTypeSelectionControl1 = new JobTypeSelectionControl();
			this.JobTypeCheckedListBox = new ZCheckedListBox();
			this.InvoiceTypeDropEdit = new ZDropEdit();
			this.CurrencyCodeFindBox = new ZCodeFindBox();
			this.DebtorGuidFindBox = new ZGuidFindBox();
			this.TermDaysCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TermsDropEdit = new ZDropEdit();
			this.DueDateDateEdit = new ZDateEdit();
			this.PostDateEdit = new ZDateEdit();
			this.TaxBranchFindBox = new ZGuidFindBox();
			this.periodicInvoiceControl = new PeriodicInvoiceControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DateEdit.SuspendLayout();
			this.BottomButtonPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.jobTypeSelectionControl1.SuspendLayout();
			this.InvoiceTypeDropEdit.SuspendLayout();
			this.CurrencyCodeFindBox.SuspendLayout();
			this.DebtorGuidFindBox.SuspendLayout();
			this.TermsDropEdit.SuspendLayout();
			this.DueDateDateEdit.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.TaxBranchFindBox.SuspendLayout();
			this.periodicInvoiceControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 601, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 20, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(436);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(437);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PeriodicInvoice);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|2216edf5-dbfd-4861-a9bb-2403953a4c6b", "Close");
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(924, 12, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			// 
			// PostAndCloseButton
			// 
			this.PostAndCloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostAndCloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|0090dd51-63d5-4c42-8de6-5513d6f69044", "Post");
			this.PostAndCloseButton.IsCaptionOverridden = true;
			this.PostAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(831, 12, true);
			this.PostAndCloseButton.Name = "PostAndCloseButton";
			this.PostAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PostAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.PostAndCloseButton.TabIndex = 1;
			this.PostAndCloseButton.Text = "Post && Close";
			this.PostAndCloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PostAndCloseButton.ToolTipCaption = null;
			// 
			// DateEdit
			// 
			this.DateEdit.AllowDrop = true;
			this.DateEdit.AutoCompleteMonthThreshold = 1;
			this.DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEdit, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoice)(null)).InvoiceDate)));
			this.DateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|869bd85b-1c94-4789-a66d-ba8028885eb8", "Invoice Date");
			this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 34, true);
			this.DateEdit.Name = "DateEdit";
			this.DateEdit.TabIndex = 1;
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.PostButton);
			this.BottomButtonPanel.Controls.Add(this.CloseButton);
			this.BottomButtonPanel.Controls.Add(this.PostAndCloseButton);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 557, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 44, true);
			this.BottomButtonPanel.TabIndex = 2;
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.IsCaptionOverridden = true;
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 12, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PostButton.TabIndex = 0;
			this.PostButton.Text = "Post";
			this.PostButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PostButton.ToolTipCaption = null;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.SellReferenceTextBox);
			this.TopPanel.Controls.Add(this.jobTypeSelectionControl1);
			this.TopPanel.Controls.Add(this.JobTypeCheckedListBox);
			this.TopPanel.Controls.Add(this.InvoiceTypeDropEdit);
			this.TopPanel.Controls.Add(this.CurrencyCodeFindBox);
			this.TopPanel.Controls.Add(this.DebtorGuidFindBox);
			this.TopPanel.Controls.Add(this.TermDaysCalcEdit);
			this.TopPanel.Controls.Add(this.TermsDropEdit);
			this.TopPanel.Controls.Add(this.DueDateDateEdit);
			this.TopPanel.Controls.Add(this.PostDateEdit);
			this.TopPanel.Controls.Add(this.DateEdit);
			this.TopPanel.Controls.Add(this.TaxBranchFindBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 114, true);
			this.TopPanel.TabIndex = 0;
			// 
			// SellReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.SellReferenceTextBox, "SellReference");
			this.SellReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a7fd8733-a64d-49ec-bed9-465587e95aa4", "Sell Reference");
			this.SellReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 85, true);
			this.SellReferenceTextBox.Name = "SellReferenceTextBox";
			this.SellReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SellReferenceTextBox.TabIndex = 14;
			// 
			// jobTypeSelectionControl1
			// 
			this.jobTypeSelectionControl1.AllowDrop = true;
			this.jobTypeSelectionControl1.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.jobTypeSelectionControl1, "JobTypesPicker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobTypePicker)(((PeriodicInvoice)(null)).JobTypesPicker)));
			this.jobTypeSelectionControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 3, true);
			this.jobTypeSelectionControl1.Name = "jobTypeSelectionControl1";
			this.jobTypeSelectionControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 102, true);
			this.jobTypeSelectionControl1.TabIndex = 8;
			// 
			// JobTypeCheckedListBox
			// 
			this.JobTypeCheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.JobTypeCheckedListBox, "JobTypeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZBoolDescriptionPairList)(((PeriodicInvoice)(null)).JobTypeList)));
			this.JobTypeCheckedListBox.CheckOnClick = true;
			this.JobTypeCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(703, 86, true);
			this.JobTypeCheckedListBox.Name = "JobTypeCheckedListBox";
			this.JobTypeCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 19, true);
			this.JobTypeCheckedListBox.TabIndex = 7;
			this.JobTypeCheckedListBox.Visible = false;
			// 
			// InvoiceTypeDropEdit
			// 
			this.InvoiceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceTypeDropEdit, "InvoiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoice)(null)).InvoiceType)));
			this.InvoiceTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|da880151-3949-449e-8044-9a9200517d0c", "Invoice Type");
			this.InvoiceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 8, true);
			this.InvoiceTypeDropEdit.Name = "InvoiceTypeDropEdit";
			this.InvoiceTypeDropEdit.PreBoundMaxLength = 3;
			this.InvoiceTypeDropEdit.ShouldResizeByMaxLength = true;
			this.InvoiceTypeDropEdit.ShowDescriptionBox = false;
			this.InvoiceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.InvoiceTypeDropEdit.TabIndex = 4;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "CurrencyNK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((PeriodicInvoice)(null)).CurrencyNK)));
			this.CurrencyCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|8a8311cc-8b22-40a9-b639-6fe96c8708f6", "Currency");
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 34, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.PopupCaption = null;
			this.CurrencyCodeFindBox.ShouldResize = true;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.CurrencyCodeFindBox.TabIndex = 3;
			// 
			// DebtorGuidFindBox
			// 
			this.DebtorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DebtorGuidFindBox, "DebtorPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((PeriodicInvoice)(null)).DebtorPK)));
			this.DebtorGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|00b172c0-0ccf-4cc4-b1f4-ca1f75c76965", "Debtor");
			this.DebtorGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.DebtorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.DebtorGuidFindBox.Name = "DebtorGuidFindBox";
			this.DebtorGuidFindBox.PopupCaption = null;
			this.DebtorGuidFindBox.ShouldResize = true;
			this.DebtorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.DebtorGuidFindBox.TabIndex = 0;
			// 
			// TermDaysCalcEdit
			// 
			this.TermDaysCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TermDaysCalcEdit, "InvoiceTermDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((PeriodicInvoice)(null)).InvoiceTermDays)));
			this.TermDaysCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|12d7284c-fc56-4a76-9c13-643a572f3161", "Term Days/Months");
			this.TermDaysCalcEdit.DecimalPlaces = 0;
			this.TermDaysCalcEdit.Decimals = 0;
			this.TermDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 60, true);
			this.TermDaysCalcEdit.Name = "TermDaysCalcEdit";
			this.TermDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.TermDaysCalcEdit.TabIndex = 6;
			this.TermDaysCalcEdit.Text = "0";
			this.TermDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TermsDropEdit
			// 
			this.TermsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsDropEdit, "InvoiceTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoice)(null)).InvoiceTerm)));
			this.TermsDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|d7d9a046-a1c5-452d-8ee0-e87d85c2fc1c", "Invoice Terms");
			this.TermsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(551, 34, true);
			this.TermsDropEdit.Name = "TermsDropEdit";
			this.TermsDropEdit.PreBoundMaxLength = 3;
			this.TermsDropEdit.ShouldResizeByMaxLength = true;
			this.TermsDropEdit.ShowDescriptionBox = false;
			this.TermsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.TermsDropEdit.TabIndex = 5;
			// 
			// DueDateDateEdit
			// 
			this.DueDateDateEdit.AllowDrop = true;
			this.DueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.DueDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DueDateDateEdit, "DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoice)(null)).DueDate)));
			this.DueDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|2998d588-96d0-422e-98b4-a5b54e6fbe7f", "Due Date");
			this.DueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 86, true);
			this.DueDateDateEdit.Name = "DueDateDateEdit";
			this.DueDateDateEdit.TabIndex = 2;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoice)(null)).PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|c2373bac-b23e-4ba8-9e7e-2421ae02cb1a", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 60, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 1;
			// 
			// TaxBranchFindBox
			// 
			this.TaxBranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxBranchFindBox, "TaxBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoice)(null)).TaxBranch)));
			this.TaxBranchFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|36224b6f-473c-4d84-9b66-e0d0dccda3d4", "Tax Branch");
			this.TaxBranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(243, 60, true);
			this.TaxBranchFindBox.Name = "TaxBranchFindBox";
			this.TaxBranchFindBox.PopupCaption = null;
			this.TaxBranchFindBox.ShouldResize = true;
			this.TaxBranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.TaxBranchFindBox.TabIndex = 3;
			this.TaxBranchFindBox.ShowDescriptionBox = false;
			// 
			// periodicInvoiceControl
			// 
			this.periodicInvoiceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.periodicInvoiceControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PeriodicInvoiceBase)(((PeriodicInvoice)(null)))));
			this.periodicInvoiceControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.periodicInvoiceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114, true);
			this.periodicInvoiceControl.Name = "periodicInvoiceControl";
			this.periodicInvoiceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 443, true);
			this.periodicInvoiceControl.TabIndex = 1;
			// 
			// PeriodicInvoicingForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingForm|5877969d-7e6f-44bc-b7fa-d2fc23649f1b", "Periodic Invoice");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 621, true);
			this.Controls.Add(this.periodicInvoiceControl);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomButtonPanel);
			this.DataSourceType = typeof(PeriodicInvoice);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 660, true);
			this.Name = "PeriodicInvoicingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.periodicInvoiceControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DateEdit.ResumeLayout(true);
			this.DateEdit.PerformLayout();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.jobTypeSelectionControl1.ResumeLayout(true);
			this.jobTypeSelectionControl1.PerformLayout();
			this.InvoiceTypeDropEdit.ResumeLayout(true);
			this.InvoiceTypeDropEdit.PerformLayout();
			this.CurrencyCodeFindBox.ResumeLayout(true);
			this.CurrencyCodeFindBox.PerformLayout();
			this.DebtorGuidFindBox.ResumeLayout(true);
			this.DebtorGuidFindBox.PerformLayout();
			this.TermsDropEdit.ResumeLayout(true);
			this.TermsDropEdit.PerformLayout();
			this.DueDateDateEdit.ResumeLayout(true);
			this.DueDateDateEdit.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.TaxBranchFindBox.ResumeLayout(true);
			this.TaxBranchFindBox.PerformLayout();
			this.periodicInvoiceControl.ResumeLayout(true);
			this.periodicInvoiceControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}