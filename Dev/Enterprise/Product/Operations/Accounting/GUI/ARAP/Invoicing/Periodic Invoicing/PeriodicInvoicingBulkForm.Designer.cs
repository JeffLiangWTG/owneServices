using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoicingBulkForm
	{


		#region Windows Form Designer generated code

		protected Core.Forms.ZPostOrCancelButton CloseButton;
		protected Core.Forms.ZPostOrCancelButton PostButton;
		protected ZDateEdit DateEdit;
		protected ZPanel BottomButtonPanel;
		protected ZPanel TopPanel;
		protected ZCodeFindBox CurrencyCodeFindBox;
		protected ZDateEdit PostDateEdit;
		protected ZCheckedListBox JobTypeCheckedListBox;
		private JobTypeSelectionControl jobTypeSelectionControl1;
		protected PeriodicInvoiceBulkControl periodicInvoiceBulkControl;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "CW1042", Justification = "Generated code")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017:DpiAwareDevelopmentRule", Justification = "Generated code")]
		new void InitializeComponent()
		{
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.PostButton = new Core.Forms.ZPostOrCancelButton();
			this.DateEdit = new ZDateEdit();
			this.BottomButtonPanel = new ZPanel();
			this.TopPanel = new ZPanel();
			this.JobTypeCheckedListBox = new ZCheckedListBox();
			this.CurrencyCodeFindBox = new ZCodeFindBox();
			this.PostDateEdit = new ZDateEdit();
			this.periodicInvoiceBulkControl = new PeriodicInvoiceBulkControl();
			this.jobTypeSelectionControl1 = new JobTypeSelectionControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DateEdit.SuspendLayout();
			this.BottomButtonPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.CurrencyCodeFindBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.periodicInvoiceBulkControl.SuspendLayout();
			this.jobTypeSelectionControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 641, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 20, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 436;
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = 437;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PeriodicInvoiceBulk);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2216edf5-dbfd-4861-a9bb-2403953a4c6b", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 12, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 1;
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 12, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
			this.PostButton.TabIndex = 0;
			// 
			// DateEdit
			// 
			this.DateEdit.AllowDrop = true;
			this.DateEdit.AutoCompleteMonthThreshold = 1;
			this.DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEdit, "InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoiceBulk)(null)).InvoiceDate)));
			this.DateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingBulkForm|869bd85b-1c94-4789-a66d-ba8028885eb8", "Invoice Date");
			this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 13, true);
			this.DateEdit.Name = "DateEdit";
			this.DateEdit.TabIndex = 0;
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.CloseButton);
			this.BottomButtonPanel.Controls.Add(this.PostButton);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 597, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 44, true);
			this.BottomButtonPanel.TabIndex = 2;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.jobTypeSelectionControl1);
			this.TopPanel.Controls.Add(this.JobTypeCheckedListBox);
			this.TopPanel.Controls.Add(this.CurrencyCodeFindBox);
			this.TopPanel.Controls.Add(this.PostDateEdit);
			this.TopPanel.Controls.Add(this.DateEdit);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 113, true);
			this.TopPanel.TabIndex = 0;
			// 
			// JobTypeCheckedListBox
			// 
			this.JobTypeCheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.JobTypeCheckedListBox, "JobTypeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZBoolDescriptionPairList)(((PeriodicInvoiceBulk)(null)).JobTypeList)));
			this.JobTypeCheckedListBox.CheckOnClick = true;
			this.JobTypeCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(390, 60, true);
			this.JobTypeCheckedListBox.Name = "JobTypeCheckedListBox";
			this.JobTypeCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 19, true);
			this.JobTypeCheckedListBox.TabIndex = 8;
			this.JobTypeCheckedListBox.Visible = false;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "CurrencyNK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoiceBulk)(null)).CurrencyNK)));
			this.CurrencyCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingBulkForm|8a8311cc-8b22-40a9-b639-6fe96c8708f6", "Currency");
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 39, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 20, true);
			this.CurrencyCodeFindBox.TabIndex = 1;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoiceBulk)(null)).PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingBulkForm|67902bff-9c94-431f-a66a-f8d6352749e2", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 13, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 0;
			// 
			// periodicInvoiceBulkControl
			// 
			this.periodicInvoiceBulkControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.periodicInvoiceBulkControl, ".");
			this.periodicInvoiceBulkControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.periodicInvoiceBulkControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 113, true);
			this.periodicInvoiceBulkControl.Name = "periodicInvoiceBulkControl";
			this.periodicInvoiceBulkControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 484, true);
			this.periodicInvoiceBulkControl.TabIndex = 1;
			// 
			// jobTypeSelectionControl1
			// 
			this.jobTypeSelectionControl1.AllowDrop = true;
			this.jobTypeSelectionControl1.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.jobTypeSelectionControl1, "JobTypesPicker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.JobTypePicker)(((PeriodicInvoiceBulk)(null)).JobTypesPicker)));
			this.jobTypeSelectionControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 4, true);
			this.jobTypeSelectionControl1.Name = "jobTypeSelectionControl1";
			this.jobTypeSelectionControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 107, true);
			this.jobTypeSelectionControl1.TabIndex = 9;
			// 
			// PeriodicInvoicingBulkForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodicInvoicingBulkForm|5877969d-7e6f-44bc-b7fa-d2fc23649f1b", "Bulk Periodic Invoices");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 661, true);
			this.Controls.Add(this.periodicInvoiceBulkControl);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomButtonPanel);
			this.DataSourceType = typeof(PeriodicInvoiceBulk);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 700, true);
			this.Name = "PeriodicInvoicingBulkForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.periodicInvoiceBulkControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DateEdit.ResumeLayout(true);
			this.DateEdit.PerformLayout();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.CurrencyCodeFindBox.ResumeLayout(true);
			this.CurrencyCodeFindBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.periodicInvoiceBulkControl.ResumeLayout(true);
			this.periodicInvoiceBulkControl.PerformLayout();
			this.jobTypeSelectionControl1.ResumeLayout(true);
			this.jobTypeSelectionControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}