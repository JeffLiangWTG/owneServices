using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.Transfer
{
	public partial class BankTransferForm
	{


		#region Windows Form Designer generated code

		private ZTemplateTabControl TabControl;
		private ZTabPage MainTabPage;
		private ZGroupBox TransferDetailsGroupBox;
		private ZGroupBox FinanceChargeGroupBox;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.ZTextBox TransactionNumberTextBox;
		private ZGuidFindBox FromBankGuidFindBox;
		private ZGuidFindBox ToBankGuidFindBox;
		private ZGuidFindBox AH_ABBoundZGuidFindBox;
		private ZArchitecture.ZTextBox BankChargeTransactionNumTextBox;
		private ZCalcFindBox SellAmountCalcFindBox;
		private ZCalcFindBox BankChargeLocalAmountCalcFindBox;
		private ZCalcFindBox BankChargeCalcFindBox;
		private ZCalcFindBox BuyAmountCalcFindBox;
		private ZGuidFindBox TaxCodeGuidFindBox;
		ZDateEdit TaxDateEdit;
		private ZCalcFindBox TotalLocalAmountCalcFindBox;
		private ZCalcFindBox TotalBankChargeCalcFindBox;
		private ZCalcFindBox LocalTaxAmountCalcFindBox;
		private ZCalcFindBox TaxAmountCalcFindBox;
		private ZExchangeRateControl BuyExchangeRateControl;
		private ZExchangeRateControl ExchangeRateControl;
		private ZExchangeRateControl BankChargeExchangeRateControl;
		private ZCheckBox FinanceChargeCheckBox;
		private ZPanel TotalPanel;
		private ZPanel TaxPanel;
		private ZGroupBox TransferReferenceGroupBox;
		private ZArchitecture.ZTextBox ReferenceTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZDateEdit PostDateEdit;
		private ZDateEdit InvoiceDateEdit;
		private ZArchitecture.ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		private ZArchitecture.ZTextBox BankChargeDescriptionTextBox;
		private ZArchitecture.ZTextBox BankChargeGovtChargeCodeTextBox;
		private ZPanel GovtChargeCodePanel;
		private ZCheckBox CalcExVarianceCheckBox;
		private ZCalcFindBox ExRateGainLossCalcFindBox;
		private ZCalcFindBox LocalBuyAmountCalcFindBox;
		private ZCalcFindBox LocalSellAmountCalcFindBox;
		private IContainer components;

		new void InitializeComponent()
		{
			this.components = new Container();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TabControl = new ZTemplateTabControl();
			this.MainTabPage = new ZTabPage();
			this.FinanceChargeGroupBox = new ZGroupBox();
			this.GovtChargeCodePanel = new ZPanel();
			this.BankChargeGovtChargeCodeTextBox = new ZArchitecture.ZTextBox();
			this.BankChargeDescriptionTextBox = new ZArchitecture.ZTextBox();
			this.TotalPanel = new ZPanel();
			this.TotalBankChargeCalcFindBox = new ZCalcFindBox();
			this.TotalLocalAmountCalcFindBox = new ZCalcFindBox();
			this.TaxPanel = new ZPanel();
			this.TaxCodeGuidFindBox = new ZGuidFindBox();
			this.TaxDateEdit = new ZDateEdit();
			this.TaxAmountCalcFindBox = new ZCalcFindBox();
			this.LocalTaxAmountCalcFindBox = new ZCalcFindBox();
			this.FinanceChargeCheckBox = new ZCheckBox();
			this.BankChargeExchangeRateControl = new ZExchangeRateControl();
			this.BankChargeLocalAmountCalcFindBox = new ZCalcFindBox();
			this.BankChargeCalcFindBox = new ZCalcFindBox();
			this.BankChargeTransactionNumTextBox = new ZArchitecture.ZTextBox();
			this.AH_ABBoundZGuidFindBox = new ZGuidFindBox();
			this.TransferDetailsGroupBox = new ZGroupBox();
			this.LocalBuyAmountCalcFindBox = new ZCalcFindBox();
			this.LocalSellAmountCalcFindBox = new ZCalcFindBox();
			this.ExRateGainLossCalcFindBox = new ZCalcFindBox();
			this.BuyExchangeRateControl = new ZExchangeRateControl();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.BuyAmountCalcFindBox = new ZCalcFindBox();
			this.SellAmountCalcFindBox = new ZCalcFindBox();
			this.ToBankGuidFindBox = new ZGuidFindBox();
			this.FromBankGuidFindBox = new ZGuidFindBox();
			this.TransactionNumberTextBox = new ZArchitecture.ZTextBox();
			this.CalcExVarianceCheckBox = new ZCheckBox();
			this.TransferReferenceGroupBox = new ZGroupBox();
			this.ReferenceTextBox = new ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new ZArchitecture.ZTextBox();
			this.PostDateEdit = new ZDateEdit();
			this.InvoiceDateEdit = new ZDateEdit();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.FinanceChargeGroupBox.SuspendLayout();
			this.GovtChargeCodePanel.SuspendLayout();
			this.TotalPanel.SuspendLayout();
			this.TotalBankChargeCalcFindBox.SuspendLayout();
			this.TotalLocalAmountCalcFindBox.SuspendLayout();
			this.TaxPanel.SuspendLayout();
			this.TaxCodeGuidFindBox.SuspendLayout();
			this.TaxDateEdit.SuspendLayout();
			this.TaxAmountCalcFindBox.SuspendLayout();
			this.LocalTaxAmountCalcFindBox.SuspendLayout();
			this.BankChargeExchangeRateControl.SuspendLayout();
			this.BankChargeLocalAmountCalcFindBox.SuspendLayout();
			this.BankChargeCalcFindBox.SuspendLayout();
			this.AH_ABBoundZGuidFindBox.SuspendLayout();
			this.TransferDetailsGroupBox.SuspendLayout();
			this.LocalBuyAmountCalcFindBox.SuspendLayout();
			this.LocalSellAmountCalcFindBox.SuspendLayout();
			this.ExRateGainLossCalcFindBox.SuspendLayout();
			this.BuyExchangeRateControl.SuspendLayout();
			this.ExchangeRateControl.SuspendLayout();
			this.BuyAmountCalcFindBox.SuspendLayout();
			this.SellAmountCalcFindBox.SuspendLayout();
			this.ToBankGuidFindBox.SuspendLayout();
			this.FromBankGuidFindBox.SuspendLayout();
			this.TransferReferenceGroupBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1139, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(299);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BankTransfer);
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((BankTransfer)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 71, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 18, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 24;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.MainTabPage);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 597, true);
			this.TabControl.TabIndex = 1;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|03938a98-5578-4c58-99b4-21f7ca658455", "Bank Transfer");
			this.MainTabPage.Controls.Add(this.FinanceChargeGroupBox);
			this.MainTabPage.Controls.Add(this.TransferDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.TransferReferenceGroupBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 574, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// FinanceChargeGroupBox
			// 
			this.FinanceChargeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FinanceChargeGroupBox.Controls.Add(this.GovtChargeCodePanel);
			this.FinanceChargeGroupBox.Controls.Add(this.BankChargeDescriptionTextBox);
			this.FinanceChargeGroupBox.Controls.Add(this.TotalPanel);
			this.FinanceChargeGroupBox.Controls.Add(this.TaxPanel);
			this.FinanceChargeGroupBox.Controls.Add(this.FinanceChargeCheckBox);
			this.FinanceChargeGroupBox.Controls.Add(this.BankChargeExchangeRateControl);
			this.FinanceChargeGroupBox.Controls.Add(this.BankChargeLocalAmountCalcFindBox);
			this.FinanceChargeGroupBox.Controls.Add(this.BankChargeCalcFindBox);
			this.FinanceChargeGroupBox.Controls.Add(this.BankChargeTransactionNumTextBox);
			this.FinanceChargeGroupBox.Controls.Add(this.AH_ABBoundZGuidFindBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FinanceChargeGroupBox, false);
			this.FinanceChargeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 315, true);
			this.FinanceChargeGroupBox.Name = "FinanceChargeGroupBox";
			this.FinanceChargeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 252, true);
			this.FinanceChargeGroupBox.TabIndex = 2;
			this.FinanceChargeGroupBox.TabStop = false;
			// 
			// GovtChargeCodePanel
			// 
			this.GovtChargeCodePanel.Controls.Add(this.BankChargeGovtChargeCodeTextBox);
			this.GovtChargeCodePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GovtChargeCodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 224, true);
			this.GovtChargeCodePanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GovtChargeCodePanel.Name = "GovtChargeCodePanel";
			this.GovtChargeCodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(677, 25, true);
			this.GovtChargeCodePanel.TabIndex = 9;
			// 
			// BankChargeGovtChargeCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.BankChargeGovtChargeCodeTextBox, "FinanceChargeGovtChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BankTransfer)(null)).FinanceChargeGovtChargeCode)));
			this.BankChargeGovtChargeCodeTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|964b95d6-a1e1-4a6f-bc43-183511c62e9a", "Govt Charge Code", "Government Charge Code", "");
			this.BankChargeGovtChargeCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 3, true);
			this.BankChargeGovtChargeCodeTextBox.Name = "BankChargeGovtChargeCodeTextBox";
			this.BankChargeGovtChargeCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 18, true);
			this.BankChargeGovtChargeCodeTextBox.TabIndex = 9;
			// 
			// BankChargeDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.BankChargeDescriptionTextBox, "FinanceChargeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BankTransfer)(null)).FinanceChargeDescription)));
			this.BankChargeDescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|{A07CB77-229E-4f30-96E0-E5A1A1F78F59", "Description");
			this.BankChargeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 72, true);
			this.BankChargeDescriptionTextBox.Name = "BankChargeDescriptionTextBox";
			this.BankChargeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 18, true);
			this.BankChargeDescriptionTextBox.TabIndex = 3;
			// 
			// TotalPanel
			// 
			this.TotalPanel.Controls.Add(this.TotalBankChargeCalcFindBox);
			this.TotalPanel.Controls.Add(this.TotalLocalAmountCalcFindBox);
			this.TotalPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 198, true);
			this.TotalPanel.Name = "TotalPanel";
			this.TotalPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 25, true);
			this.TotalPanel.TabIndex = 8;
			// 
			// TotalBankChargeCalcFindBox
			// 
			this.TotalBankChargeCalcFindBox.AllowDrop = true;
			this.TotalBankChargeCalcFindBox.BindToAmount = "FinanceChargeOSTotal";
			this.TotalBankChargeCalcFindBox.BindToDecimalPlaces = "FinanceChargeOSAmountDecimals";
			this.TotalBankChargeCalcFindBox.BindToUnit = "FinanceChargeOSAmountCurrency";
			this.TotalBankChargeCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|1e90879b-944d-4547-9b9d-4fc701e2be3c", "Total Bank Charge");
			this.TotalBankChargeCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalBankChargeCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 3, true);
			this.TotalBankChargeCalcFindBox.Name = "TotalBankChargeCalcFindBox";
			this.TotalBankChargeCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TotalBankChargeCalcFindBox.TabIndex = 0;
			// 
			// TotalLocalAmountCalcFindBox
			// 
			this.TotalLocalAmountCalcFindBox.AllowDrop = true;
			this.TotalLocalAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TotalLocalAmountCalcFindBox.BindToAmount = "FinanceChargeTotalLocalAmount";
			this.TotalLocalAmountCalcFindBox.BindToDecimalPlaces = "FinanceChargeLocalAmountDecimals";
			this.TotalLocalAmountCalcFindBox.BindToUnit = "FinanceChargeLocalAmountCurrency";
			this.TotalLocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|d580a038-538e-4f20-be3c-6b63f5c86d64", "Total Local Amount");
			this.TotalLocalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TotalLocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 3, true);
			this.TotalLocalAmountCalcFindBox.Name = "TotalLocalAmountCalcFindBox";
			this.TotalLocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TotalLocalAmountCalcFindBox.TabIndex = 1;
			// 
			// TaxPanel
			// 
			this.TaxPanel.Controls.Add(this.TaxCodeGuidFindBox);
			this.TaxPanel.Controls.Add(this.TaxDateEdit);
			this.TaxPanel.Controls.Add(this.TaxAmountCalcFindBox);
			this.TaxPanel.Controls.Add(this.LocalTaxAmountCalcFindBox);
			this.TaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 146, true);
			this.TaxPanel.Name = "TaxPanel";
			this.TaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 51, true);
			this.TaxPanel.TabIndex = 7;
			// 
			// TaxCodeGuidFindBox
			// 
			this.TaxCodeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxCodeGuidFindBox, "FinanceChargeTaxID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((BankTransfer)(null)).FinanceChargeTaxID)));
			this.TaxCodeGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|0a1bc75f-9620-4223-8642-ca6364f3f830", "Tax Code");
			this.TaxCodeGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.TaxCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 4, true);
			this.TaxCodeGuidFindBox.Name = "TaxCodeGuidFindBox";
			this.TaxCodeGuidFindBox.PopupCaption = null;
			this.TaxCodeGuidFindBox.ShouldResize = true;
			this.TaxCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 18, true);
			this.TaxCodeGuidFindBox.TabIndex = 0;
			// 
			// TaxDateEdit
			// 
			this.TaxDateEdit.AllowDrop = true;
			this.TaxDateEdit.AutoCompleteMonthThreshold = 1;
			this.TaxDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.TaxDateEdit, "FinanceChargeTaxDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((BankTransfer)(null)).FinanceChargeTaxDate)));
			this.TaxDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|9e4c87ad-f2b2-4fe4-8f16-d02b8198107b", "Tax Date");
			this.TaxDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 4, true);
			this.TaxDateEdit.Name = "TaxDateEdit";
			this.TaxDateEdit.TabIndex = 1;
			// 
			// TaxAmountCalcFindBox
			// 
			this.TaxAmountCalcFindBox.AllowDrop = true;
			this.TaxAmountCalcFindBox.BindToAmount = "FinanceChargeOSTaxAmount";
			this.TaxAmountCalcFindBox.BindToDecimalPlaces = "FinanceChargeOSAmountDecimals";
			this.TaxAmountCalcFindBox.BindToUnit = "FinanceChargeOSAmountCurrency";
			this.TaxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|4af18266-e3e6-4d30-8e34-e9a2192a540a", "Tax Amount");
			this.TaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.TaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 29, true);
			this.TaxAmountCalcFindBox.Name = "TaxAmountCalcFindBox";
			this.TaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TaxAmountCalcFindBox.TabIndex = 2;
			// 
			// LocalTaxAmountCalcFindBox
			// 
			this.LocalTaxAmountCalcFindBox.AllowDrop = true;
			this.LocalTaxAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LocalTaxAmountCalcFindBox.BindToAmount = "FinanceChargeLocalTaxAmount";
			this.LocalTaxAmountCalcFindBox.BindToDecimalPlaces = "FinanceChargeLocalAmountDecimals";
			this.LocalTaxAmountCalcFindBox.BindToUnit = "FinanceChargeLocalAmountCurrency";
			this.LocalTaxAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|cb3f14c1-2b6a-4638-93b0-a3ed697592ee", "Local Tax Amount");
			this.LocalTaxAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalTaxAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 28, true);
			this.LocalTaxAmountCalcFindBox.Name = "LocalTaxAmountCalcFindBox";
			this.LocalTaxAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LocalTaxAmountCalcFindBox.TabIndex = 3;
			// 
			// FinanceChargeCheckBox
			// 
			this.FinanceChargeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FinanceChargeCheckBox, "EnableFinanceCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((BankTransfer)(null)).EnableFinanceCharge)));
			this.FinanceChargeCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|8d3ec05c-2856-410c-ab86-89cd34410a03", "Finance Charge");
			this.FinanceChargeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FinanceChargeCheckBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FinanceChargeCheckBox, false);
			this.FinanceChargeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 1, true);
			this.FinanceChargeCheckBox.Name = "FinanceChargeCheckBox";
			this.FinanceChargeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 17, true);
			this.FinanceChargeCheckBox.TabIndex = 0;
			this.FinanceChargeCheckBox.UseVisualStyleBackColor = true;
			// 
			// BankChargeExchangeRateControl
			// 
			this.BankChargeExchangeRateControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankChargeExchangeRateControl, "FinanceChargeZExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((BankTransfer)(null)).FinanceChargeZExchangeRate)));
			this.BankChargeExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|2d0f63bd-6eff-481b-b4e4-bbdcb518fed6", "Exchange Rate To Local");
			this.BankChargeExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 98, true);
			this.BankChargeExchangeRateControl.Name = "BankChargeExchangeRateControl";
			this.BankChargeExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.BankChargeExchangeRateControl.TabIndex = 4;
			// 
			// BankChargeLocalAmountCalcFindBox
			// 
			this.BankChargeLocalAmountCalcFindBox.AllowDrop = true;
			this.BankChargeLocalAmountCalcFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BankChargeLocalAmountCalcFindBox.BindToAmount = "FinanceChargeLocalAmount";
			this.BankChargeLocalAmountCalcFindBox.BindToDecimalPlaces = "FinanceChargeLocalAmountDecimals";
			this.BankChargeLocalAmountCalcFindBox.BindToUnit = "FinanceChargeLocalAmountCurrency";
			this.BankChargeLocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|e3b6140d-5452-4b04-aa11-aaad156d919d", "Local Amount");
			this.BankChargeLocalAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.BankChargeLocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 123, true);
			this.BankChargeLocalAmountCalcFindBox.Name = "BankChargeLocalAmountCalcFindBox";
			this.BankChargeLocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.BankChargeLocalAmountCalcFindBox.TabIndex = 6;
			// 
			// BankChargeCalcFindBox
			// 
			this.BankChargeCalcFindBox.AllowDrop = true;
			this.BankChargeCalcFindBox.BindToAmount = "FinanceChargeOSAmount";
			this.BankChargeCalcFindBox.BindToDecimalPlaces = "FinanceChargeOSAmountDecimals";
			this.BankChargeCalcFindBox.BindToUnit = "FinanceChargeOSAmountCurrency";
			this.BankChargeCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|f406409a-ae20-4aee-ad14-ee90c3523df5", "Bank Charge");
			this.BankChargeCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.BankChargeCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 123, true);
			this.BankChargeCalcFindBox.Name = "BankChargeCalcFindBox";
			this.BankChargeCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.BankChargeCalcFindBox.TabIndex = 5;
			// 
			// BankChargeTransactionNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.BankChargeTransactionNumTextBox, "FinanceChargeTransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BankTransfer)(null)).FinanceChargeTransactionNum)));
			this.BankChargeTransactionNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|515ffa18-623c-41ab-9347-d1eefaabb1b9", "Transaction Number");
			this.BankChargeTransactionNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 23, true);
			this.BankChargeTransactionNumTextBox.Name = "BankChargeTransactionNumTextBox";
			this.BankChargeTransactionNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 18, true);
			this.BankChargeTransactionNumTextBox.TabIndex = 1;
			// 
			// AH_ABBoundZGuidFindBox
			// 
			this.AH_ABBoundZGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AH_ABBoundZGuidFindBox, "FinanceChargeBankPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((BankTransfer)(null)).FinanceChargeBankPK)));
			this.AH_ABBoundZGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|657e0f0b-e279-43ec-a942-b7d6215b5cd5", "Bank Account");
			this.AH_ABBoundZGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.AH_ABBoundZGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 48, true);
			this.AH_ABBoundZGuidFindBox.Name = "AH_ABBoundZGuidFindBox";
			this.AH_ABBoundZGuidFindBox.PopupCaption = null;
			this.AH_ABBoundZGuidFindBox.ShouldResize = true;
			this.AH_ABBoundZGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 18, true);
			this.AH_ABBoundZGuidFindBox.TabIndex = 2;
			// 
			// TransferDetailsGroupBox
			// 
			this.TransferDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.TransferDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|99a09a9f-51cc-4a64-9620-773006fefc80", "Transfer Details");
			this.TransferDetailsGroupBox.Controls.Add(this.LocalBuyAmountCalcFindBox);
			this.TransferDetailsGroupBox.Controls.Add(this.LocalSellAmountCalcFindBox);
			this.TransferDetailsGroupBox.Controls.Add(this.ExRateGainLossCalcFindBox);
			this.TransferDetailsGroupBox.Controls.Add(this.BuyExchangeRateControl);
			this.TransferDetailsGroupBox.Controls.Add(this.ExchangeRateControl);
			this.TransferDetailsGroupBox.Controls.Add(this.BuyAmountCalcFindBox);
			this.TransferDetailsGroupBox.Controls.Add(this.SellAmountCalcFindBox);
			this.TransferDetailsGroupBox.Controls.Add(this.ToBankGuidFindBox);
			this.TransferDetailsGroupBox.Controls.Add(this.FromBankGuidFindBox);
			this.TransferDetailsGroupBox.Controls.Add(this.TransactionNumberTextBox);
			this.TransferDetailsGroupBox.Controls.Add(this.CalcExVarianceCheckBox);
			this.TransferDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 113, true);
			this.TransferDetailsGroupBox.Name = "TransferDetailsGroupBox";
			this.TransferDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 199, true);
			this.TransferDetailsGroupBox.TabIndex = 1;
			this.TransferDetailsGroupBox.TabStop = false;
			// 
			// LocalBuyAmountCalcFindBox
			// 
			this.LocalBuyAmountCalcFindBox.AllowDrop = true;
			this.LocalBuyAmountCalcFindBox.BindToAmount = "LocalBuyAmount";
			this.LocalBuyAmountCalcFindBox.BindToDecimalPlaces = "LocalAmountDecimals";
			this.LocalBuyAmountCalcFindBox.BindToUnit = "LocalCurrency";
			this.LocalBuyAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|ca5dfff8-681e-4bde-bbbb-d8b96cf3ce77", "Buy Local");
			this.LocalBuyAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalBuyAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 128, true);
			this.LocalBuyAmountCalcFindBox.Name = "LocalBuyAmountCalcFindBox";
			this.LocalBuyAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LocalBuyAmountCalcFindBox.TabIndex = 9;
			// 
			// LocalSellAmountCalcFindBox
			// 
			this.LocalSellAmountCalcFindBox.AllowDrop = true;
			this.LocalSellAmountCalcFindBox.BindToAmount = "LocalSellAmount";
			this.LocalSellAmountCalcFindBox.BindToDecimalPlaces = "LocalAmountDecimals";
			this.LocalSellAmountCalcFindBox.BindToUnit = "LocalCurrency";
			this.LocalSellAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|1f00cc76-aa6a-47d2-b5bf-14db3766b7b9", "Sell Local");
			this.LocalSellAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.LocalSellAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 104, true);
			this.LocalSellAmountCalcFindBox.Name = "LocalSellAmountCalcFindBox";
			this.LocalSellAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LocalSellAmountCalcFindBox.TabIndex = 6;
			// 
			// ExRateGainLossCalcFindBox
			// 
			this.ExRateGainLossCalcFindBox.AllowDrop = true;
			this.ExRateGainLossCalcFindBox.BindToAmount = "ExRateGainLoss";
			this.ExRateGainLossCalcFindBox.BindToDecimalPlaces = "LocalAmountDecimals";
			this.ExRateGainLossCalcFindBox.BindToUnit = "LocalCurrency";
			this.ExRateGainLossCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ExRateGainLossCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 176, true);
			this.ExRateGainLossCalcFindBox.Name = "ExRateGainLossCalcFindBox";
			this.ExRateGainLossCalcFindBox.Visible = false;
			this.ExRateGainLossCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ExRateGainLossCalcFindBox.TabIndex = 12;
			// 
			// BuyExchangeRateControl
			// 
			this.BuyExchangeRateControl.AllowDrop = true;
			this.BuyExchangeRateControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BuyExchangeRateControl, "BuyZExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((BankTransfer)(null)).BuyZExchangeRate)));
			this.BuyExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|0f1a6058-e6cb-47de-af6a-e64ce47f24aa", "Exchange Rate");
			this.BuyExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 128, true);
			this.BuyExchangeRateControl.Name = "BuyExchangeRateControl";
			this.BuyExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.BuyExchangeRateControl.TabIndex = 8;
			// 
			// ExchangeRateControl
			// 
			this.ExchangeRateControl.AllowDrop = true;
			this.ExchangeRateControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "SellZExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((BankTransfer)(null)).SellZExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|3b38a758-2c77-41e2-b551-9b0d54663144", "Exchange Rate");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 104, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 20, true);
			this.ExchangeRateControl.TabIndex = 5;
			// 
			// BuyAmountCalcFindBox
			// 
			this.BuyAmountCalcFindBox.AllowDrop = true;
			this.BuyAmountCalcFindBox.BindToAmount = "BuyAmount";
			this.BuyAmountCalcFindBox.BindToDecimalPlaces = "BuyAmountDecimals";
			this.BuyAmountCalcFindBox.BindToUnit = "BuyCurrency";
			this.BuyAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|cb47aa73-5c58-410c-b084-310ae98671ea", "Buy Amount");
			this.BuyAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.BuyAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 128, true);
			this.BuyAmountCalcFindBox.Name = "BuyAmountCalcFindBox";
			this.BuyAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.BuyAmountCalcFindBox.TabIndex = 7;
			// 
			// SellAmountCalcFindBox
			// 
			this.SellAmountCalcFindBox.AllowDrop = true;
			this.SellAmountCalcFindBox.BindToAmount = "SellAmount";
			this.SellAmountCalcFindBox.BindToDecimalPlaces = "SellAmountDecimals";
			this.SellAmountCalcFindBox.BindToUnit = "SellCurrency";
			this.SellAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|40632660-e4f7-46b4-a1a7-0c8f0cb0e48c", "Sell Amount");
			this.SellAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.SellAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 104, true);
			this.SellAmountCalcFindBox.Name = "SellAmountCalcFindBox";
			this.SellAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.SellAmountCalcFindBox.TabIndex = 4;
			// 
			// ToBankGuidFindBox
			// 
			this.ToBankGuidFindBox.AllowDrop = true;
			this.ToBankGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ToBankGuidFindBox, "BankTransferToPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((BankTransfer)(null)).BankTransferToPK)));
			this.ToBankGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|a936f97f-4a10-4789-acdb-99a4e3a1468a", "To Bank Account");
			this.ToBankGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ToBankGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 72, true);
			this.ToBankGuidFindBox.Name = "ToBankGuidFindBox";
			this.ToBankGuidFindBox.PopupCaption = null;
			this.ToBankGuidFindBox.ShouldResize = true;
			this.ToBankGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 18, true);
			this.ToBankGuidFindBox.TabIndex = 3;
			// 
			// FromBankGuidFindBox
			// 
			this.FromBankGuidFindBox.AllowDrop = true;
			this.FromBankGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FromBankGuidFindBox, "BankTransferFromPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((BankTransfer)(null)).BankTransferFromPK)));
			this.FromBankGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|92a61f41-b843-470a-96c7-ae2cd6308719", "From Bank Account");
			this.FromBankGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.FromBankGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 48, true);
			this.FromBankGuidFindBox.Name = "FromBankGuidFindBox";
			this.FromBankGuidFindBox.PopupCaption = null;
			this.FromBankGuidFindBox.ShouldResize = true;
			this.FromBankGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 18, true);
			this.FromBankGuidFindBox.TabIndex = 2;
			// 
			// TransactionNumberTextBox
			// 
			this.TransactionNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransactionNumberTextBox, "TransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BankTransfer)(null)).TransactionNumber)));
			this.TransactionNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|2c5d5975-ee6f-429e-9ce7-4c11ba25a391", "Transaction Number");
			this.TransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 24, true);
			this.TransactionNumberTextBox.Name = "TransactionNumberTextBox";
			this.TransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 18, true);
			this.TransactionNumberTextBox.TabIndex = 1;
			// 
			// CalcExVarianceCheckBox
			// 
			this.CalcExVarianceCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CalcExVarianceCheckBox, "ShouldCalculateExchangeVariance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((BankTransfer)(null)).ShouldCalculateExchangeVariance)));
			this.CalcExVarianceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 176, true);
			this.CalcExVarianceCheckBox.Name = "CalcExVarianceCheckBox";
			this.CalcExVarianceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(163, 16, true);
			this.CalcExVarianceCheckBox.TabIndex = 11;
			this.CalcExVarianceCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransferReferenceGroupBox
			// 
			this.TransferReferenceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.TransferReferenceGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|07cc3915-002e-4629-9ad8-c80cf5352129", "Bank Transfer reference");
			this.TransferReferenceGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.TransferReferenceGroupBox.Controls.Add(this.ReferenceTextBox);
			this.TransferReferenceGroupBox.Controls.Add(this.DescriptionTextBox);
			this.TransferReferenceGroupBox.Controls.Add(this.PostDateEdit);
			this.TransferReferenceGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.TransferReferenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 6, true);
			this.TransferReferenceGroupBox.Name = "TransferReferenceGroupBox";
			this.TransferReferenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 104, true);
			this.TransferReferenceGroupBox.TabIndex = 0;
			this.TransferReferenceGroupBox.TabStop = false;
			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BankTransfer)(null)).Reference)));
			this.ReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|64135918-1b02-4605-b84d-c5228de4379f", "Reference");
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 71, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 18, true);
			this.ReferenceTextBox.TabIndex = 7;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BankTransfer)(null)).Description)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|43cb4f96-9e87-4e57-ad95-b780e7c1d88c", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 48, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 18, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((BankTransfer)(null)).AH_PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|4ceb81da-4028-4bbe-8383-056ca0291dbd", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 24, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 3;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "TransactionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((BankTransfer)(null)).TransactionDate)));
			this.InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|c9eaf67d-e8fd-4326-8838-c7f59914559b", "Date");
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 24, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 830, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// BankTransferForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BankTransferForm|5b884f7f-f66a-45e0-8657-b7f1b7c02c81", "Bank Transfer");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 1163, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(BankTransfer);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.CashBook.Transfer.BankTransfer";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "BankTransferForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.FinanceChargeGroupBox.ResumeLayout(false);
			this.FinanceChargeGroupBox.PerformLayout();
			this.GovtChargeCodePanel.ResumeLayout(false);
			this.GovtChargeCodePanel.PerformLayout();
			this.TotalPanel.ResumeLayout(false);
			this.TotalPanel.PerformLayout();
			this.TotalBankChargeCalcFindBox.ResumeLayout(true);
			this.TotalBankChargeCalcFindBox.PerformLayout();
			this.TotalLocalAmountCalcFindBox.ResumeLayout(true);
			this.TotalLocalAmountCalcFindBox.PerformLayout();
			this.TaxPanel.ResumeLayout(false);
			this.TaxPanel.PerformLayout();
			this.TaxCodeGuidFindBox.ResumeLayout(true);
			this.TaxCodeGuidFindBox.PerformLayout();
			this.TaxDateEdit.ResumeLayout(true);
			this.TaxDateEdit.PerformLayout();
			this.TaxAmountCalcFindBox.ResumeLayout(true);
			this.TaxAmountCalcFindBox.PerformLayout();
			this.LocalTaxAmountCalcFindBox.ResumeLayout(true);
			this.LocalTaxAmountCalcFindBox.PerformLayout();
			this.BankChargeExchangeRateControl.ResumeLayout(true);
			this.BankChargeExchangeRateControl.PerformLayout();
			this.BankChargeLocalAmountCalcFindBox.ResumeLayout(true);
			this.BankChargeLocalAmountCalcFindBox.PerformLayout();
			this.BankChargeCalcFindBox.ResumeLayout(true);
			this.BankChargeCalcFindBox.PerformLayout();
			this.AH_ABBoundZGuidFindBox.ResumeLayout(true);
			this.AH_ABBoundZGuidFindBox.PerformLayout();
			this.TransferDetailsGroupBox.ResumeLayout(false);
			this.TransferDetailsGroupBox.PerformLayout();
			this.LocalBuyAmountCalcFindBox.ResumeLayout(true);
			this.LocalBuyAmountCalcFindBox.PerformLayout();
			this.LocalSellAmountCalcFindBox.ResumeLayout(true);
			this.LocalSellAmountCalcFindBox.PerformLayout();
			this.ExRateGainLossCalcFindBox.ResumeLayout(true);
			this.ExRateGainLossCalcFindBox.PerformLayout();
			this.BuyExchangeRateControl.ResumeLayout(true);
			this.BuyExchangeRateControl.PerformLayout();
			this.ExchangeRateControl.ResumeLayout(true);
			this.ExchangeRateControl.PerformLayout();
			this.BuyAmountCalcFindBox.ResumeLayout(true);
			this.BuyAmountCalcFindBox.PerformLayout();
			this.SellAmountCalcFindBox.ResumeLayout(true);
			this.SellAmountCalcFindBox.PerformLayout();
			this.ToBankGuidFindBox.ResumeLayout(true);
			this.ToBankGuidFindBox.PerformLayout();
			this.FromBankGuidFindBox.ResumeLayout(true);
			this.FromBankGuidFindBox.PerformLayout();
			this.TransferReferenceGroupBox.ResumeLayout(false);
			this.TransferReferenceGroupBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}