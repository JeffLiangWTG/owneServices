using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.ARAP.TransactionView
{
	public partial class TransactionViewForm
	{


		#region Windows Form Designer generated code

		private ZDateEdit AH_InvoiceDateBoundDateEdit;
		private ZArchitecture.ZTextBox AH_DescBoundPeriodEdit;
		private ZArchitecture.ZTextBox AH_TransactionNumBoundTextEdit;
		private ZExchangeRateControl AH_ExchangeRateBoundExchangeRateControl;
		private ZGuidFindBox AH_OHBoundFindBox;
		private ZCalcFindBox AH_OsTotalBoundCalcFindBox;
		private ZCalcFindBox AH_InvoiceAmountBoundCalcFindBox;
		private TransactionHeader Header;
		private ZDateEdit zPostDateDateEdit;
		private ZDateEdit zDueDateDateEdit;
		private ZTabPage TransactionTabPage;
		private ZLogsTabPage zEventTabPage1;
		private ZTemplateTabControl MainTabControl;
		private Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl1;
		private ZGuidFindBox AH_AGGuidFindBox;
		private ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		new void InitializeComponent()
		{
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AH_InvoiceDateBoundDateEdit = new ZDateEdit();
			this.AH_DescBoundPeriodEdit = new ZArchitecture.ZTextBox();
			this.AH_TransactionNumBoundTextEdit = new ZArchitecture.ZTextBox();
			this.AH_ExchangeRateBoundExchangeRateControl = new ZExchangeRateControl();
			this.AH_OHBoundFindBox = new ZGuidFindBox();
			this.AH_OsTotalBoundCalcFindBox = new ZCalcFindBox();
			this.AH_InvoiceAmountBoundCalcFindBox = new ZCalcFindBox();
			this.zPostDateDateEdit = new ZDateEdit();
			this.zDueDateDateEdit = new ZDateEdit();
			this.MainTabControl = new ZTemplateTabControl();
			this.TransactionTabPage = new ZTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.oPostingButtonsUserControl1 = new Core.Forms.ZPostingButtonsUserControl();
			this.AH_AGGuidFindBox = new ZGuidFindBox();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.TransactionTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(237);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(237);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TransactionHeader);
			// 
			// AH_InvoiceDateBoundDateEdit
			// 
			this.AH_InvoiceDateBoundDateEdit.AllowDrop = true;
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateBoundDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TransactionHeader)(null)).AH_InvoiceDate)));
			this.AH_InvoiceDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 15, true);
			this.AH_InvoiceDateBoundDateEdit.Name = "AH_InvoiceDateBoundDateEdit";
			this.AH_InvoiceDateBoundDateEdit.TabIndex = 0;
			// 
			// AH_DescBoundPeriodEdit
			// 
			this.BindingSource.SetBindingMember(this.AH_DescBoundPeriodEdit, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(null)).AH_Desc)));
			this.AH_DescBoundPeriodEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescBoundPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 59, true);
			this.AH_DescBoundPeriodEdit.Name = "AH_DescBoundPeriodEdit";
			this.AH_DescBoundPeriodEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.AH_DescBoundPeriodEdit.TabIndex = 4;
			// 
			// AH_TransactionNumBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AH_TransactionNumBoundTextEdit, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((TransactionHeader)(null)).AH_TransactionNum)));
			this.AH_TransactionNumBoundTextEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionViewForm|2d2ceb7c-b7e6-495d-ad26-799c3fa77170", "Transaction No.");
			this.AH_TransactionNumBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 15, true);
			this.AH_TransactionNumBoundTextEdit.Name = "AH_TransactionNumBoundTextEdit";
			this.AH_TransactionNumBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AH_TransactionNumBoundTextEdit.TabIndex = 1;
			// 
			// AH_ExchangeRateBoundExchangeRateControl
			// 
			this.AH_ExchangeRateBoundExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_ExchangeRateBoundExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((TransactionHeader)(null)).ExchangeRate)));
			this.AH_ExchangeRateBoundExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionViewForm|a6629bd0-f48e-4423-a245-e63f0107e2fc", "Currency");
			this.AH_ExchangeRateBoundExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 104, true);
			this.AH_ExchangeRateBoundExchangeRateControl.Name = "AH_ExchangeRateBoundExchangeRateControl";
			this.AH_ExchangeRateBoundExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.AH_ExchangeRateBoundExchangeRateControl.TabIndex = 6;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((TransactionHeader)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 104, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 24;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AH_OHBoundFindBox
			// 
			this.AH_OHBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_OHBoundFindBox, "AH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(null)).AH_OH)));
			this.AH_OHBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 82, true);
			this.AH_OHBoundFindBox.Name = "AH_OHBoundFindBox";
			this.AH_OHBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.AH_OHBoundFindBox.TabIndex = 5;
			// 
			// AH_OsTotalBoundCalcFindBox
			// 
			this.AH_OsTotalBoundCalcFindBox.AllowDrop = true;
			this.AH_OsTotalBoundCalcFindBox.BindToAmount = "BindableOSAmount";
			this.AH_OsTotalBoundCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.AH_OsTotalBoundCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.AH_OsTotalBoundCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionViewForm|3ba3fb24-0844-4980-bac0-5de2b452930d", "Amount");
			this.AH_OsTotalBoundCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OsTotalBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 126, true);
			this.AH_OsTotalBoundCalcFindBox.Name = "AH_OsTotalBoundCalcFindBox";
			this.AH_OsTotalBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.AH_OsTotalBoundCalcFindBox.TabIndex = 7;
			// 
			// AH_InvoiceAmountBoundCalcFindBox
			// 
			this.AH_InvoiceAmountBoundCalcFindBox.AllowDrop = true;
			this.AH_InvoiceAmountBoundCalcFindBox.BindToAmount = "BindableInvoiceAmount";
			this.AH_InvoiceAmountBoundCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.AH_InvoiceAmountBoundCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.AH_InvoiceAmountBoundCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionViewForm|4d1fe5a5-ebbc-463d-b0c7-5369d4b83cc7", "Local Amount");
			this.AH_InvoiceAmountBoundCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 126, true);
			this.AH_InvoiceAmountBoundCalcFindBox.Name = "AH_InvoiceAmountBoundCalcFindBox";
			this.AH_InvoiceAmountBoundCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.AH_InvoiceAmountBoundCalcFindBox.TabIndex = 8;
			// 
			// zPostDateDateEdit
			// 
			this.zPostDateDateEdit.AllowDrop = true;
			this.zPostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.zPostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zPostDateDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TransactionHeader)(null)).AH_PostDate)));
			this.zPostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionViewForm|1802ac4d-3df2-448e-91d5-db8003672c95", "Post To");
			this.zPostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 37, true);
			this.zPostDateDateEdit.Name = "zPostDateDateEdit";
			this.zPostDateDateEdit.TabIndex = 2;
			// 
			// zDueDateDateEdit
			// 
			this.zDueDateDateEdit.AllowDrop = true;
			this.zDueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.zDueDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDueDateDateEdit, "AH_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TransactionHeader)(null)).AH_DueDate)));
			this.zDueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 37, true);
			this.zDueDateDateEdit.Name = "zDueDateDateEdit";
			this.zDueDateDateEdit.TabIndex = 3;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.TransactionTabPage);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 206, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// TransactionTabPage
			// 
			this.TransactionTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionViewForm|7c5db714-369f-41aa-8ab6-fa8e5f291e3d", "Transaction");
			this.TransactionTabPage.Controls.Add(this.AH_TransactionNumBoundTextEdit);
			this.TransactionTabPage.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.TransactionTabPage.Controls.Add(this.AH_AGGuidFindBox);
			this.TransactionTabPage.Controls.Add(this.AH_OHBoundFindBox);
			this.TransactionTabPage.Controls.Add(this.AH_InvoiceDateBoundDateEdit);
			this.TransactionTabPage.Controls.Add(this.AH_OsTotalBoundCalcFindBox);
			this.TransactionTabPage.Controls.Add(this.AH_InvoiceAmountBoundCalcFindBox);
			this.TransactionTabPage.Controls.Add(this.zPostDateDateEdit);
			this.TransactionTabPage.Controls.Add(this.zDueDateDateEdit);
			this.TransactionTabPage.Controls.Add(this.AH_ExchangeRateBoundExchangeRateControl);
			this.TransactionTabPage.Controls.Add(this.AH_DescBoundPeriodEdit);
			this.TransactionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransactionTabPage.Name = "TransactionTabPage";
			this.TransactionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 179, true);
			this.TransactionTabPage.TabIndex = 0;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 179, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// oPostingButtonsUserControl1
			// 
			this.oPostingButtonsUserControl1.AllowDrop = true;
			this.oPostingButtonsUserControl1.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl1.AutoSize = true;
			this.oPostingButtonsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 221, true);
			this.oPostingButtonsUserControl1.Name = "oPostingButtonsUserControl1";
			this.oPostingButtonsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 26, true);
			this.oPostingButtonsUserControl1.TabIndex = 1;
			// 
			// AH_AGGuidFindBox
			// 
			this.AH_AGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_AGGuidFindBox, "AH_AG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((TransactionHeader)(null)).AH_AG)));
			this.AH_AGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 150, true);
			this.AH_AGGuidFindBox.Name = "AH_AGGuidFindBox";
			this.AH_AGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.AH_AGGuidFindBox.TabIndex = 9;
			// 
			// TransactionViewForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 275, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransactionViewForm|53eccca3-0077-411e-ac29-5511e340dc46", "Transaction View");
			this.Controls.Add(this.oPostingButtonsUserControl1);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(TransactionHeader);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.Transaction.TransactionHeader";
			this.IsPostOnly = true;
			this.Name = "TransactionViewForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.oPostingButtonsUserControl1, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.TransactionTabPage.ResumeLayout(false);
			this.TransactionTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
