using System;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class TransferForm
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZArchitecture.ZCalcEdit();
			this.ButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.DetailsGroupBox = new ZGroupBox();
			this.AH_PostDateBoundDateEdit = new ZDateEdit();
			this.AH_ExchangeRateBoundExchangeRateControl = new ZExchangeRateControl();
			this.AH_Calc_InvoiceAmountBoundCalcEdit = new ZCalcFindBox();
			this.AH_InvoiceDateBoundDateEdit = new ZDateEdit();
			this.AH_OSTotalBoundCurrencyControl = new ZCalcFindBox();
			this.AH_DescBoundTextEdit = new ZArchitecture.ZTextBox();
			this.AH_TransactionNumBoundTextBox = new ZArchitecture.ZTextBox();
			this.ToGroupBox = new ZGroupBox();
			this.AH_Calc_ToAgeingDateBoundDateEdit = new ZDateEdit();
			this.AH_Calc_ToAfterTransferBoundCurrencyControl = new ZCalcFindBox();
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl = new ZCalcFindBox();
			this.AH_Calc_ToAccountBoundFindBox = new ZGuidFindBox();
			this.FromGroupBox = new ZGroupBox();
			this.AH_Calc_FromAgeingDateBoundDateEdit = new ZDateEdit();
			this.AH_Calc_FromAfterTransferBoundCurrencyControl = new ZCalcFindBox();
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl = new ZCalcFindBox();
			this.AH_Calc_FromAccountBoundFindBox = new ZGuidFindBox();
			this.MainTabControl = new ZTemplateTabControl();
			this.TransferTabPage = new ZTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.ToGroupBox.SuspendLayout();
			this.FromGroupBox.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.TransferTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 406, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 20, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 6;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(281);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(281);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Transfer);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 379, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|69875914-2c5b-4801-9515-04e0dc4886f4", "Transfer Details");
			this.DetailsGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.AH_PostDateBoundDateEdit);
			this.DetailsGroupBox.Controls.Add(this.AH_ExchangeRateBoundExchangeRateControl);
			this.DetailsGroupBox.Controls.Add(this.AH_Calc_InvoiceAmountBoundCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.AH_InvoiceDateBoundDateEdit);
			this.DetailsGroupBox.Controls.Add(this.AH_OSTotalBoundCurrencyControl);
			this.DetailsGroupBox.Controls.Add(this.AH_DescBoundTextEdit);
			this.DetailsGroupBox.Controls.Add(this.AH_TransactionNumBoundTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 130, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Transfer)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 37, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 24;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AH_PostDateBoundDateEdit
			// 
			this.AH_PostDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_PostDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_PostDateBoundDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Transfer)(null)).AH_PostDate)));
			this.AH_PostDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|346aae44-c4b1-483f-9ca4-b220c3bf1350", "Post Date");
			this.AH_PostDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 37, true);
			this.AH_PostDateBoundDateEdit.Name = "AH_PostDateBoundDateEdit";
			this.AH_PostDateBoundDateEdit.TabIndex = 5;
			// 
			// AH_ExchangeRateBoundExchangeRateControl
			// 
			this.BindingSource.SetBindingMember(this.AH_ExchangeRateBoundExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((Transfer)(null)).ExchangeRate)));
			this.AH_ExchangeRateBoundExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|016a9f4a-9fa9-4291-a414-8745e7a38af1", "Currency");
			this.AH_ExchangeRateBoundExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 59, true);
			this.AH_ExchangeRateBoundExchangeRateControl.Name = "AH_ExchangeRateBoundExchangeRateControl";
			this.AH_ExchangeRateBoundExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_ExchangeRateBoundExchangeRateControl.TabIndex = 7;
			// 
			// AH_Calc_InvoiceAmountBoundCalcEdit
			// 
			this.AH_Calc_InvoiceAmountBoundCalcEdit.BindToAmount = "AH_InvoiceAmount";
			this.AH_Calc_InvoiceAmountBoundCalcEdit.BindToDecimalPlaces = "LocalCurrencySubUnitRatio";
			this.AH_Calc_InvoiceAmountBoundCalcEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_InvoiceAmountBoundCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|131db42e-69ad-4132-8d71-302606efb400", "Local Amount");
			this.AH_Calc_InvoiceAmountBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 82, true);
			this.AH_Calc_InvoiceAmountBoundCalcEdit.Name = "AH_Calc_InvoiceAmountBoundCalcEdit";
			this.AH_Calc_InvoiceAmountBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_Calc_InvoiceAmountBoundCalcEdit.TabIndex = 11;
			// 
			// AH_InvoiceDateBoundDateEdit
			// 
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_InvoiceDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_InvoiceDateBoundDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Transfer)(null)).AH_InvoiceDate)));
			this.AH_InvoiceDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|fba29227-afa3-4459-b66b-727f1f9059f3", "Date", "Date", "");
			this.AH_InvoiceDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 15, true);
			this.AH_InvoiceDateBoundDateEdit.Name = "AH_InvoiceDateBoundDateEdit";
			this.AH_InvoiceDateBoundDateEdit.TabIndex = 1;
			// 
			// AH_OSTotalBoundCurrencyControl
			// 
			this.AH_OSTotalBoundCurrencyControl.BindToAmount = "AH_OSTotal";
			this.AH_OSTotalBoundCurrencyControl.BindToDecimalPlaces = "OSCurrencySubUnitRatio";
			this.AH_OSTotalBoundCurrencyControl.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.AH_OSTotalBoundCurrencyControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|274a5235-9fdc-4f8b-aaa4-45884d9d54b8", "Transfer Amount");
			this.AH_OSTotalBoundCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSTotalBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 82, true);
			this.AH_OSTotalBoundCurrencyControl.Name = "AH_OSTotalBoundCurrencyControl";
			this.AH_OSTotalBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_OSTotalBoundCurrencyControl.TabIndex = 9;
			this.AH_OSTotalBoundCurrencyControl.PreBoundMaxLength = 3;
			// 
			// AH_DescBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AH_DescBoundTextEdit, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transfer)(null)).AH_Desc)));
			this.AH_DescBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescBoundTextEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|00cc0880-298e-40ad-bc7c-f6d630fdb8db", "Description");
			this.AH_DescBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 104, true);
			this.AH_DescBoundTextEdit.Name = "AH_DescBoundTextEdit";
			this.AH_DescBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.AH_DescBoundTextEdit.TabIndex = 13;
			// 
			// AH_TransactionNumBoundTextBox
			// 
			this.AH_TransactionNumBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.AH_TransactionNumBoundTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transfer)(null)).AH_TransactionNum)));
			this.AH_TransactionNumBoundTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|1a70b32b-4500-4768-ab8b-380596524268", "Transfer Number");
			this.AH_TransactionNumBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 15, true);
			this.AH_TransactionNumBoundTextBox.Name = "AH_TransactionNumBoundTextBox";
			this.AH_TransactionNumBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_TransactionNumBoundTextBox.TabIndex = 3;
			// 
			// ToGroupBox
			// 
			this.ToGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|6bb77624-847e-4fbb-b884-83a8c62fe438", "To");
			this.ToGroupBox.Controls.Add(this.AH_Calc_ToAgeingDateBoundDateEdit);
			this.ToGroupBox.Controls.Add(this.AH_Calc_ToAfterTransferBoundCurrencyControl);
			this.ToGroupBox.Controls.Add(this.AH_Calc_ToBeforeTransferBoundCurrencyControl);
			this.ToGroupBox.Controls.Add(this.AH_Calc_ToAccountBoundFindBox);
			this.ToGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 238, true);
			this.ToGroupBox.Name = "ToGroupBox";
			this.ToGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 89, true);
			this.ToGroupBox.TabIndex = 2;
			this.ToGroupBox.TabStop = false;
			// 
			// AH_Calc_ToAgeingDateBoundDateEdit
			// 
			this.AH_Calc_ToAgeingDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_Calc_ToAgeingDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_Calc_ToAgeingDateBoundDateEdit, "AH_Calc_ToDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Transfer)(null)).AH_Calc_ToDueDate)));
			this.AH_Calc_ToAgeingDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|e30a870c-274e-4f4b-bad2-8caf0de92268", "Due Date");
			this.AH_Calc_ToAgeingDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 37, true);
			this.AH_Calc_ToAgeingDateBoundDateEdit.Name = "AH_Calc_ToAgeingDateBoundDateEdit";
			this.AH_Calc_ToAgeingDateBoundDateEdit.TabIndex = 3;
			// 
			// AH_Calc_ToAfterTransferBoundCurrencyControl
			// 
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.BindToAmount = "AH_Calc_ToAfterTransfer";
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|21d47d12-7b27-4e9c-a6e2-b9d705f0fb47", "After Transfer");
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 61, true);
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.Name = "AH_Calc_ToAfterTransferBoundCurrencyControl";
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_Calc_ToAfterTransferBoundCurrencyControl.TabIndex = 7;
			// 
			// AH_Calc_ToBeforeTransferBoundCurrencyControl
			// 
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.BindToAmount = "AH_Calc_ToBeforeTransfer";
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|10228eb2-dce8-41e9-af8c-87ed8710921d", "Before Transfer");
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 61, true);
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.Name = "AH_Calc_ToBeforeTransferBoundCurrencyControl";
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_Calc_ToBeforeTransferBoundCurrencyControl.TabIndex = 5;
			// 
			// AH_Calc_ToAccountBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AH_Calc_ToAccountBoundFindBox, "AH_ToAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Transfer)(null)).AH_ToAccount)));
			this.AH_Calc_ToAccountBoundFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|72286841-c1da-435f-895b-0cf6593fb3bb", "Account");
			this.AH_Calc_ToAccountBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 15, true);
			this.AH_Calc_ToAccountBoundFindBox.Name = "AH_Calc_ToAccountBoundFindBox";
			this.AH_Calc_ToAccountBoundFindBox.PopupCaption = null;
			this.AH_Calc_ToAccountBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.AH_Calc_ToAccountBoundFindBox.TabIndex = 1;
			// 
			// FromGroupBox
			// 
			this.FromGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|76ff1f82-03bc-466e-87f1-3e045d687d7b", "From");
			this.FromGroupBox.Controls.Add(this.AH_Calc_FromAgeingDateBoundDateEdit);
			this.FromGroupBox.Controls.Add(this.AH_Calc_FromAfterTransferBoundCurrencyControl);
			this.FromGroupBox.Controls.Add(this.AH_Calc_FromBeforeTransferBoundCurrencyControl);
			this.FromGroupBox.Controls.Add(this.AH_Calc_FromAccountBoundFindBox);
			this.FromGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 141, true);
			this.FromGroupBox.Name = "FromGroupBox";
			this.FromGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 89, true);
			this.FromGroupBox.TabIndex = 1;
			this.FromGroupBox.TabStop = false;
			// 
			// AH_Calc_FromAgeingDateBoundDateEdit
			// 
			this.AH_Calc_FromAgeingDateBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.AH_Calc_FromAgeingDateBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AH_Calc_FromAgeingDateBoundDateEdit, "AH_Calc_FromDueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Transfer)(null)).AH_Calc_FromDueDate)));
			this.AH_Calc_FromAgeingDateBoundDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|9dda4650-ee2d-4e76-8831-c89cd85433a4", "Due Date");
			this.AH_Calc_FromAgeingDateBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 41, true);
			this.AH_Calc_FromAgeingDateBoundDateEdit.Name = "AH_Calc_FromAgeingDateBoundDateEdit";
			this.AH_Calc_FromAgeingDateBoundDateEdit.TabIndex = 3;
			// 
			// AH_Calc_FromAfterTransferBoundCurrencyControl
			// 
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.BindToAmount = "AH_Calc_FromAfterTransfer";
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|66ca8647-18ee-4dd4-8faf-f49b344946df", "After Transfer");
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 61, true);
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.Name = "AH_Calc_FromAfterTransferBoundCurrencyControl";
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_Calc_FromAfterTransferBoundCurrencyControl.TabIndex = 7;
			// 
			// AH_Calc_FromBeforeTransferBoundCurrencyControl
			// 
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.BindToAmount = "AH_Calc_FromBeforeTransfer";
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|9a899e87-ed18-4e51-9045-b260d5aef96d", "Before Transfer");
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 63, true);
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.Name = "AH_Calc_FromBeforeTransferBoundCurrencyControl";
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 20, true);
			this.AH_Calc_FromBeforeTransferBoundCurrencyControl.TabIndex = 5;
			// 
			// AH_Calc_FromAccountBoundFindBox
			// 
			this.BindingSource.SetBindingMember(this.AH_Calc_FromAccountBoundFindBox, "AH_FromAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Transfer)(null)).AH_FromAccount)));
			this.AH_Calc_FromAccountBoundFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|af4f33f0-71f7-4ac3-b149-1f687615b20b", "Account");
			this.AH_Calc_FromAccountBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 19, true);
			this.AH_Calc_FromAccountBoundFindBox.Name = "AH_Calc_FromAccountBoundFindBox";
			this.AH_Calc_FromAccountBoundFindBox.PopupCaption = null;
			this.AH_Calc_FromAccountBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.AH_Calc_FromAccountBoundFindBox.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.TransferTabPage);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 367, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// TransferTabPage
			// 
			this.TransferTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("TransferForm|745521c9-ca0c-4e12-9f06-79a86e020215", "Transfer");
			this.TransferTabPage.Controls.Add(this.FromGroupBox);
			this.TransferTabPage.Controls.Add(this.DetailsGroupBox);
			this.TransferTabPage.Controls.Add(this.ToGroupBox);
			this.TransferTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransferTabPage.Name = "TransferTabPage";
			this.TransferTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 340, true);
			this.TransferTabPage.TabIndex = 0;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 340, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// TransferForm
			// 
			this.AutoAddPreviousNextButtons = false;

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 426, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Transfer);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Transfer";
			this.IsPostOnly = true;
			this.Name = "TransferForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ToGroupBox.ResumeLayout(false);
			this.FromGroupBox.ResumeLayout(false);
			this.MainTabControl.ResumeLayout(false);
			this.TransferTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
