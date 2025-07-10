using System.ComponentModel;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class OpeningPaymentForm
	{


		#region Windows Form Designer generated code

		private ZDateEdit InvoiceDateEdit;
		private ZDateEdit PostDateEdit;
		private ZGuidFindBox AccountGuidFindBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZGroupBox BankDetailsGroupBox;
		private ZGuidFindBox zGuidFindBox1;
		private ZArchitecture.ZTextBox ChequeNoTextBox;
		private ZGroupBox PaymentAmountGroupBox;
		private ZExchangeRateControl ExchangeRateControl;
		private ZGroupBox PaymentDetailsGroupBox;
		private ZArchitecture.ZTextBox TransactionNumberTextBox;
		private ZCalcFindBox zCalcFindBox2;
		private ZDropEdit PaymentTypeDropEdit;
		private ZTemplateTabControl zTabControl1;
		private ZTabPage OpeningPaymentTabPage;
		private ZCalcFindBox PaymentAmountCalcFindBox;
		private ZLogsTabPage zEventTabPage1;
		private ZPanel BottomPanel;
		private Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl1;
		private IContainer components;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			this.InvoiceDateEdit = new ZDateEdit();
			this.PaymentDetailsGroupBox = new ZGroupBox();
			this.TransactionNumberTextBox = new ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new ZArchitecture.ZTextBox();
			this.AccountGuidFindBox = new ZGuidFindBox();
			this.PostDateEdit = new ZDateEdit();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.ChequeNoTextBox = new ZArchitecture.ZTextBox();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.PaymentTypeDropEdit = new ZDropEdit();
			this.PaymentAmountGroupBox = new ZGroupBox();
			this.zCalcFindBox2 = new ZCalcFindBox();
			this.PaymentAmountCalcFindBox = new ZCalcFindBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.zTabControl1 = new ZTemplateTabControl();
			this.OpeningPaymentTabPage = new ZTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.BottomPanel = new ZPanel();
			this.oPostingButtonsUserControl1 = new Core.Forms.ZPostingButtonsUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PaymentDetailsGroupBox.SuspendLayout();
			this.BankDetailsGroupBox.SuspendLayout();
			this.PaymentAmountGroupBox.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.OpeningPaymentTabPage.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 405, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(271);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(271);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OpeningPayment);
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OpeningPayment)(null)).AH_InvoiceDate)));
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 17, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 0;
			// 
			// PaymentDetailsGroupBox
			// 
			this.PaymentDetailsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.PaymentDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|5dbe70df-e35e-4bcd-9892-1cda3797b3bc", "Payment Details");
			this.PaymentDetailsGroupBox.Controls.Add(this.TransactionNumberTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.AccountGuidFindBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.PaymentDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.PaymentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 19, true);
			this.PaymentDetailsGroupBox.Name = "PaymentDetailsGroupBox";
			this.PaymentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 125, true);
			this.PaymentDetailsGroupBox.TabIndex = 2;
			this.PaymentDetailsGroupBox.TabStop = false;
			// 
			// TransactionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionNumberTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningPayment)(null)).AH_TransactionNum)));
			this.TransactionNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|c2e40512-8655-475a-97c7-edfb6b36ee3e", "Payment No");
			this.TransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 17, true);
			this.TransactionNumberTextBox.Name = "TransactionNumberTextBox";
			this.TransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20, true);
			this.TransactionNumberTextBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningPayment)(null)).AH_Desc)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 96, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 20, true);
			this.DescriptionTextBox.TabIndex = 4;
			// 
			// AccountGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.AccountGuidFindBox, "AH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((OpeningPayment)(null)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OpeningPayment)(null)).Lookups.Headers)));
			this.AccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 68, true);
			this.AccountGuidFindBox.Name = "AccountGuidFindBox";
			this.AccountGuidFindBox.PopupCaption = null;
			this.AccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 21, true);
			this.AccountGuidFindBox.TabIndex = 3;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OpeningPayment)(null)).AH_PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|17181844-AE8A-4F76-8917-CFD30F0693DB", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 43, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 2;
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|12c91ac5-b806-4848-9109-bca80dfeadad", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.ChequeNoTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.zGuidFindBox1);
			this.BankDetailsGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 147, true);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 99, true);
			this.BankDetailsGroupBox.TabIndex = 3;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// ChequeNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNoTextBox, "AH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((OpeningPayment)(null)).AH_ChequeOrReference)));
			this.ChequeNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|6aa8107e-6b70-4a6f-958c-ac44aa45a87a", "Check No.");
			this.ChequeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 69, true);
			this.ChequeNoTextBox.Name = "ChequeNoTextBox";
			this.ChequeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 20, true);
			this.ChequeNoTextBox.TabIndex = 7;
			// 
			// zGuidFindBox1
			// 
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "AH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((OpeningPayment)(null)).AH_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OpeningPayment)(null)).Lookups.BankAccounts)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 43, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PopupCaption = null;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 21, true);
			this.zGuidFindBox1.TabIndex = 6;
			// 
			// PaymentTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "AH_ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((OpeningPayment)(null)).AH_ReceiptType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((OpeningPayment)(null)).PaymentMethods)));
			this.PaymentTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|0c56ca4d-317c-4510-8ef7-7b70d85cd585", "Payment Type");
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 16, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 3;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(364, 20, true);
			this.PaymentTypeDropEdit.TabIndex = 5;
			// 
			// PaymentAmountGroupBox
			// 
			this.PaymentAmountGroupBox.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.PaymentAmountGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|56b11a64-bb5a-477a-803f-d75c54ffd581", "Payment Amount");
			this.PaymentAmountGroupBox.Controls.Add(this.zCalcFindBox2);
			this.PaymentAmountGroupBox.Controls.Add(this.PaymentAmountCalcFindBox);
			this.PaymentAmountGroupBox.Controls.Add(this.ExchangeRateControl);
			this.PaymentAmountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 249, true);
			this.PaymentAmountGroupBox.Name = "PaymentAmountGroupBox";
			this.PaymentAmountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 71, true);
			this.PaymentAmountGroupBox.TabIndex = 4;
			this.PaymentAmountGroupBox.TabStop = false;
			// 
			// zCalcFindBox2
			// 
			this.zCalcFindBox2.BindToAmount = "AH_LocalExTaxAmount";
			this.zCalcFindBox2.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.zCalcFindBox2.BindToUnit = "AH_Calc_LocalRX";
			this.zCalcFindBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|a0afd9b9-0e8f-4246-b077-10931dc8b875", "Local Amount");
			this.zCalcFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 44, true);
			this.zCalcFindBox2.Name = "zCalcFindBox2";
			this.zCalcFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.zCalcFindBox2.TabIndex = 10;
			// 
			// PaymentAmountCalcFindBox
			// 
			this.PaymentAmountCalcFindBox.BindToAmount = "AH_OSExTaxAmount";
			this.PaymentAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.PaymentAmountCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.PaymentAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|2825173c-4c0e-4790-89ea-c60c62635661", "Payment Amount");
			this.PaymentAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.PaymentAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 44, true);
			this.PaymentAmountCalcFindBox.Name = "PaymentAmountCalcFindBox";
			this.PaymentAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.PaymentAmountCalcFindBox.TabIndex = 9;
			// 
			// ExchangeRateControl
			// 
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((OpeningPayment)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|77c93385-7600-474f-b791-1628c85221cf", "Exchange Rate");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 17, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 22, true);
			this.ExchangeRateControl.TabIndex = 8;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.OpeningPaymentTabPage);
			this.zTabControl1.Controls.Add(this.zEventTabPage1);
			this.zTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 370, true);
			this.zTabControl1.TabIndex = 1;
			// 
			// OpeningPaymentTabPage
			// 
			this.OpeningPaymentTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|58808071-ca8f-403d-9dcd-f22c67211ab7", "Opening Payment");
			this.OpeningPaymentTabPage.Controls.Add(this.PaymentDetailsGroupBox);
			this.OpeningPaymentTabPage.Controls.Add(this.BankDetailsGroupBox);
			this.OpeningPaymentTabPage.Controls.Add(this.PaymentAmountGroupBox);
			this.OpeningPaymentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OpeningPaymentTabPage.Name = "OpeningPaymentTabPage";
			this.OpeningPaymentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 343, true);
			this.OpeningPaymentTabPage.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 343, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.oPostingButtonsUserControl1);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 370, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 35, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// oPostingButtonsUserControl1
			// 
			this.oPostingButtonsUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 6, true);
			this.oPostingButtonsUserControl1.Name = "oPostingButtonsUserControl1";
			this.oPostingButtonsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.oPostingButtonsUserControl1.TabIndex = 12;
			// 
			// OpeningPaymentForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 431, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningPaymentForm|69c165d1-53de-4fc5-a5b3-463774321566", "Opening Payment");
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(OpeningPayment);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.CashBook.OpeningPayment.OpeningPayment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 456, true);
			this.Name = "OpeningPaymentForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PaymentDetailsGroupBox.ResumeLayout(false);
			this.PaymentDetailsGroupBox.PerformLayout();
			this.BankDetailsGroupBox.ResumeLayout(false);
			this.BankDetailsGroupBox.PerformLayout();
			this.PaymentAmountGroupBox.ResumeLayout(false);
			this.zTabControl1.ResumeLayout(false);
			this.OpeningPaymentTabPage.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}