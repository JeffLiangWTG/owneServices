using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Contra
{
	public partial class ContraForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new Container();
			this.ButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.PayGroupBox = new ZGroupBox();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZArchitecture.ZCalcEdit();
			this.PostDateDateEdit = new ZDateEdit();
			this.ContraNumberEdit = new ZArchitecture.ZTextBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.DescriptionTextEdit = new ZArchitecture.ZTextBox();
			this.InvoiceDateDateEdit = new ZDateEdit();
			this.LocalAmountEdit = new ZCalcFindBox();
			this.AH_OSTotalBoundCalcEdit = new ZCalcFindBox();
			this.ARGroupBox = new ZGroupBox();
			this.AH_Calc_RecAfterContraEdit = new ZCalcFindBox();
			this.AH_Calc_RecBeforeContraEdit = new ZCalcFindBox();
			this.ARAccountBindBox = new ZGuidFindBox();
			this.AccountsPayableGroupBox = new ZGroupBox();
			this.AH_Calc_PayAfterContraEdit = new ZCalcFindBox();
			this.AH_Calc_PayBeforeContraEdit = new ZCalcFindBox();
			this.AH_APAccountFindBox = new ZGuidFindBox();
			this.MainTabControl = new ZTemplateTabControl();
			this.ContraTabPage = new ZTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PayGroupBox.SuspendLayout();
			this.ARGroupBox.SuspendLayout();
			this.AccountsPayableGroupBox.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.ContraTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 460, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(293);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(293);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ARAP.Contra);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 429, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// PayGroupBox
			// 
			this.PayGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|d8eb6627-f933-4fb0-902b-9ff59e465c92", "Contra Details");
			this.PayGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.PayGroupBox.Controls.Add(this.PostDateDateEdit);
			this.PayGroupBox.Controls.Add(this.ContraNumberEdit);
			this.PayGroupBox.Controls.Add(this.ExchangeRateControl);
			this.PayGroupBox.Controls.Add(this.DescriptionTextEdit);
			this.PayGroupBox.Controls.Add(this.InvoiceDateDateEdit);
			this.PayGroupBox.Controls.Add(this.LocalAmountEdit);
			this.PayGroupBox.Controls.Add(this.AH_OSTotalBoundCalcEdit);
			this.PayGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.PayGroupBox.Name = "PayGroupBox";
			this.PayGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 144, true);
			this.PayGroupBox.TabIndex = 0;
			this.PayGroupBox.TabStop = false;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.ARAP.Contra)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 88, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 24;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PostDateDateEdit
			// 
			this.PostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.ARAP.Contra)(null)).AH_PostDate)));
			this.PostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|cb8f8a7f-128a-4332-9aff-7d01c67c86c5", "Post Date");
			this.PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 40, true);
			this.PostDateDateEdit.Name = "PostDateDateEdit";
			this.PostDateDateEdit.TabIndex = 5;
			// 
			// ContraNumberEdit
			// 
			this.ContraNumberEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.ContraNumberEdit, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ARAP.Contra)(null)).AH_TransactionNum)));
			this.ContraNumberEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|3d1ecff3-0ee0-4165-9901-8f39e47fa656", "Contra Number");
			this.ContraNumberEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 16, true);
			this.ContraNumberEdit.Name = "ContraNumberEdit";
			this.ContraNumberEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.ContraNumberEdit.TabIndex = 3;
			// 
			// ExchangeRateControl
			// 
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((Business.ARAP.Contra)(null)).ExchangeRate)));
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|c2cd7bc9-50fa-4775-8681-63a87ce5c9a5", "Currency");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 88, true);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ExchangeRateControl.TabIndex = 9;
			// 
			// DescriptionTextEdit
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextEdit, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ARAP.Contra)(null)).AH_Desc)));
			this.DescriptionTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|3ed8e620-66fa-47b2-85b0-bf4044716495", "Description");
			this.DescriptionTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 64, true);
			this.DescriptionTextEdit.Name = "DescriptionTextEdit";
			this.DescriptionTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 20, true);
			this.DescriptionTextEdit.TabIndex = 7;
			// 
			// InvoiceDateDateEdit
			// 
			this.InvoiceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.ARAP.Contra)(null)).AH_InvoiceDate)));
			this.InvoiceDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|42b14afd-0940-4e4a-af70-731e5baf945e", "Date", "Date", "");
			this.InvoiceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 16, true);
			this.InvoiceDateDateEdit.Name = "InvoiceDateDateEdit";
			this.InvoiceDateDateEdit.TabIndex = 1;
			// 
			// LocalAmountEdit
			// 
			this.LocalAmountEdit.BackColor = System.Drawing.SystemColors.Control;
			this.LocalAmountEdit.BindToAmount = "AH_InvoiceAmount";
			this.LocalAmountEdit.BindToDecimalPlaces = "LocalCurrencySubUnitRatio";
			this.LocalAmountEdit.BindToUnit = "AH_Calc_LocalRX";
			this.LocalAmountEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|087b1482-f4cd-40c1-a5e1-be1c51f9d4ac", "Local Amount");
			this.LocalAmountEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 112, true);
			this.LocalAmountEdit.Name = "LocalAmountEdit";
			this.LocalAmountEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LocalAmountEdit.TabIndex = 13;
			// 
			// AH_OSTotalBoundCalcEdit
			// 
			this.AH_OSTotalBoundCalcEdit.BindToAmount = "AH_OSTotal";
			this.AH_OSTotalBoundCalcEdit.BindToDecimalPlaces = "OSCurrencySubUnitRatio";
			this.AH_OSTotalBoundCalcEdit.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.AH_OSTotalBoundCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|26e600df-1541-4fcd-a5ef-728c5f8b1a09", "Contra Amount");
			this.AH_OSTotalBoundCalcEdit.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AH_OSTotalBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 112, true);
			this.AH_OSTotalBoundCalcEdit.Name = "AH_OSTotalBoundCalcEdit";
			this.AH_OSTotalBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.AH_OSTotalBoundCalcEdit.TabIndex = 11;
			// 
			// ARGroupBox
			// 
			this.ARGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|69d65dcf-076b-48cc-9488-ca5755c41895", "Accounts Receivable");
			this.ARGroupBox.Controls.Add(this.AH_Calc_RecAfterContraEdit);
			this.ARGroupBox.Controls.Add(this.AH_Calc_RecBeforeContraEdit);
			this.ARGroupBox.Controls.Add(this.ARAccountBindBox);
			this.ARGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 148, true);
			this.ARGroupBox.Name = "ARGroupBox";
			this.ARGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 72, true);
			this.ARGroupBox.TabIndex = 1;
			this.ARGroupBox.TabStop = false;
			// 
			// AH_Calc_RecAfterContraEdit
			// 
			this.AH_Calc_RecAfterContraEdit.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_RecAfterContraEdit.BindToAmount = "AH_Calc_RecAfterContra";
			this.AH_Calc_RecAfterContraEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_RecAfterContraEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|ab471407-6160-4101-b969-1b1baf2c33b3", "After Contra");
			this.AH_Calc_RecAfterContraEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 40, true);
			this.AH_Calc_RecAfterContraEdit.Name = "AH_Calc_RecAfterContraEdit";
			this.AH_Calc_RecAfterContraEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.AH_Calc_RecAfterContraEdit.TabIndex = 5;
			// 
			// AH_Calc_RecBeforeContraEdit
			// 
			this.AH_Calc_RecBeforeContraEdit.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_RecBeforeContraEdit.BindToAmount = "AH_Calc_RecBeforeContra";
			this.AH_Calc_RecBeforeContraEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_RecBeforeContraEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|f2fff79f-f80d-4cb9-b274-1a9f0a80ad7e", "Before Contra");
			this.AH_Calc_RecBeforeContraEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 40, true);
			this.AH_Calc_RecBeforeContraEdit.Name = "AH_Calc_RecBeforeContraEdit";
			this.AH_Calc_RecBeforeContraEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.AH_Calc_RecBeforeContraEdit.TabIndex = 3;
			// 
			// ARAccountBindBox
			// 
			this.BindingSource.SetBindingMember(this.ARAccountBindBox, "AH_ARAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ARAP.Contra)(null)).AH_ARAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Contra)(null)).OrgDebtors)));
			this.ARAccountBindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|00007614-c53d-41bc-9931-62656226d5a8", "Account");
			this.ARAccountBindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 16, true);
			this.ARAccountBindBox.Name = "ARAccountBindBox";
			this.ARAccountBindBox.PopupCaption = null;
			this.ARAccountBindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 21, true);
			this.ARAccountBindBox.TabIndex = 1;
			// 
			// AccountsPayableGroupBox
			// 
			this.AccountsPayableGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|2b2bb4af-554a-48c1-a37e-77732c50cfd3", "Accounts Payable");
			this.AccountsPayableGroupBox.Controls.Add(this.AH_Calc_PayAfterContraEdit);
			this.AccountsPayableGroupBox.Controls.Add(this.AH_Calc_PayBeforeContraEdit);
			this.AccountsPayableGroupBox.Controls.Add(this.AH_APAccountFindBox);
			this.AccountsPayableGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 220, true);
			this.AccountsPayableGroupBox.Name = "AccountsPayableGroupBox";
			this.AccountsPayableGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 72, true);
			this.AccountsPayableGroupBox.TabIndex = 2;
			this.AccountsPayableGroupBox.TabStop = false;
			// 
			// AH_Calc_PayAfterContraEdit
			// 
			this.AH_Calc_PayAfterContraEdit.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_PayAfterContraEdit.BindToAmount = "AH_Calc_PayAfterContra";
			this.AH_Calc_PayAfterContraEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_PayAfterContraEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|f6032f85-f813-4140-bac6-9045e3d92722", "After Contra", "After Contra", "");
			this.AH_Calc_PayAfterContraEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 40, true);
			this.AH_Calc_PayAfterContraEdit.Name = "AH_Calc_PayAfterContraEdit";
			this.AH_Calc_PayAfterContraEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.AH_Calc_PayAfterContraEdit.TabIndex = 5;
			// 
			// AH_Calc_PayBeforeContraEdit
			// 
			this.AH_Calc_PayBeforeContraEdit.BackColor = System.Drawing.SystemColors.Control;
			this.AH_Calc_PayBeforeContraEdit.BindToAmount = "AH_Calc_PayBeforeContra";
			this.AH_Calc_PayBeforeContraEdit.BindToUnit = "AH_Calc_LocalRX";
			this.AH_Calc_PayBeforeContraEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|6b1ceac1-ac81-481b-b5c2-64c71b97be89", "Before Contra", "Before Contra", "");
			this.AH_Calc_PayBeforeContraEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 40, true);
			this.AH_Calc_PayBeforeContraEdit.Name = "AH_Calc_PayBeforeContraEdit";
			this.AH_Calc_PayBeforeContraEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.AH_Calc_PayBeforeContraEdit.TabIndex = 3;
			// 
			// AH_APAccountFindBox
			// 
			this.BindingSource.SetBindingMember(this.AH_APAccountFindBox, "AH_APAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.ARAP.Contra)(null)).AH_APAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Contra)(null)).OrgCreditors)));
			this.AH_APAccountFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|f1c5211d-cef3-49ca-a451-fcd280a392c5", "Account");
			this.AH_APAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 16, true);
			this.AH_APAccountFindBox.Name = "AH_APAccountFindBox";
			this.AH_APAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 21, true);
			this.AH_APAccountFindBox.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ContraTabPage);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 415, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// ContraTabPage
			// 
			this.ContraTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|e27e45d8-8c46-40e3-b8d5-4d811c8e5ac0", "Contra");
			this.ContraTabPage.Controls.Add(this.PayGroupBox);
			this.ContraTabPage.Controls.Add(this.ARGroupBox);
			this.ContraTabPage.Controls.Add(this.AccountsPayableGroupBox);
			this.ContraTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContraTabPage.Name = "ContraTabPage";
			this.ContraTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 388, true);
			this.ContraTabPage.TabIndex = 0;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 388, true);
			this.zEventTabPage1.TabIndex = 1;
			// 
			// ContraForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 484, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContraForm|81c566f8-b547-4b12-9722-bc6d14d59084", "Contra");
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Business.ARAP.Contra);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.Contra";
			this.IsPostOnly = true;
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(540, 400, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 512, true);
			this.Name = "ContraForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PayGroupBox.ResumeLayout(false);
			this.PayGroupBox.PerformLayout();
			this.ARGroupBox.ResumeLayout(false);
			this.AccountsPayableGroupBox.ResumeLayout(false);
			this.MainTabControl.ResumeLayout(false);
			this.ContraTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
