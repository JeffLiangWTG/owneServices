using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.GUI.DataExport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.CashBook.DirectDebitBatch
{
	public partial class DirectDebitBatchForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.BatchNoTextBox = new ZArchitecture.ZTextBox();
			this.BankAccountFindBox = new ZGuidFindBox();
			this.DepositBatchLineGrid = new ZArchitecture.ZGrid();
			this.InvoiceAmountCalcFind = new ZCalcFindBox();
			this.GenerateDDRFileButton = new ZButton();
			this.MainTabControl = new ZTemplateTabControl();
			this.zTabPage1 = new ZTabPage();
			this.zCalcFind1 = new ZCalcFindBox();
			this.PostDateDateEdit = new ZDateEdit();
			this.BatchDateDateEdit = new ZDateEdit();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.zGroupBox1 = new ZGroupBox();
			this.AutoDDRRadioButton = new ZRadioButton();
			this.NonAutoDDRRadioButton = new ZRadioButton();
			this.BothRadioButton = new ZRadioButton();
			this.ClearButton = new ZButton();
			this.FindButton = new ZButton();
			this.CancelledBatchLabel = new ZArchitecture.ZLabel();
			this.BankReferenceNumberTextBox = new ZArchitecture.ZTextBox();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DepositBatchLineGrid)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 410, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 23, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(444);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(445);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DirectDebitBatchHeader);
			// 
			// BatchNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.BatchNoTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((DirectDebitBatchHeader)(null)).AH_TransactionNum)));
			this.BatchNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 7, true);
			this.BatchNoTextBox.Name = "BatchNoTextBox";
			this.BatchNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.BatchNoTextBox.TabIndex = 1;
			// 
			// BankAccountFindBox
			// 
			this.BankAccountFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountFindBox, "AH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((DirectDebitBatchHeader)(null)).AH_AB)));
			this.BankAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 7, true);
			this.BankAccountFindBox.Name = "BankAccountFindBox";
			this.BankAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.BankAccountFindBox.TabIndex = 0;
			// 
			// DepositBatchLineGrid
			// 
			this.DepositBatchLineGrid.AllowNavigation = false;
			this.DepositBatchLineGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DepositBatchLineGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).IncludeInTheBatch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AH_TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AH_PostDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AH_OSTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AccountCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AccountTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).PayeeBankAccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).PayeeBankBSB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).PayeeBankName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).PayeeBankSwift)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).PayeeIBANNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).PayeeCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).AllowAutoDDR)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).BankCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).BankCreateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).BankLastEditUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((IDirectDebitBatchTransaction)(((System.Collections.IList)(((DirectDebitBatchHeader)(null)).Lines)).SyncRoot)).BankLastEditTimeLocal)));
			this.DepositBatchLineGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|1042de8e-f64e-4bfb-a796-cfbc2efde01a", "Include In The Batch");
			zCheckBoxColumnStyleInfo1.ColumnName = "IncludeInTheBatch";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|2f6c438e-98d0-45e9-b70b-566946586d9f", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "AH_TransactionType";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|1f2bf38b-996e-45da-bbe4-d4c04ff13474", "Ledger ");
			zTextBoxColumnStyleInfo2.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|7fdab589-5355-4197-a793-56e3061d7dbc", "Transaction");
			zTextBoxColumnStyleInfo3.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|5a96a325-ca92-4ad8-8f55-69ebc2851ee4", "Post date");
			zDateEditColumnStyleInfo1.ColumnName = "AH_PostDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|FBE37D53-6EF9-410B-97B2-DDB317B2B20D", "Bank Created Time");
			zDateEditColumnStyleInfo2.ColumnName = "BankCreateTimeLocal";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|91B487B9-45BB-4C49-A231-BE07BD5A6341", "Bank Last Edited Time");
			zDateEditColumnStyleInfo3.ColumnName = "BankLastEditTimeLocal";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|8e4db7f1-fabd-45e4-9ac2-4cd54727ca6f", "Payee");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|602aab37-a610-4350-ba64-a2f6ff6dcd88", "Transaction Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotalAmount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|44febd30-815d-4b1e-95f7-f44a56363715", "Currency");
			zTextBoxColumnStyleInfo4.ColumnName = "AccountCurrency";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|e22644d9-c168-424f-8206-7ad2a37a1a7d", "Payee Account Name");
			zTextBoxColumnStyleInfo5.ColumnName = "AccountTitle";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|6c5ca25e-d1de-4531-ad3c-7eca791b8a0d", "Bank Account No. ");
			zTextBoxColumnStyleInfo6.ColumnName = "PayeeBankAccountNumber";
			zTextBoxColumnStyleInfo6.IsMandatory = true;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|020485a9-3dcb-4637-9ca3-e0d99a62bcba", "Bank/Branch");
			zTextBoxColumnStyleInfo7.ColumnName = "PayeeBankBSB";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|04f785e1-45a5-4553-a314-7f93c7effa96", "Bank Name");
			zTextBoxColumnStyleInfo8.ColumnName = "PayeeBankName";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|5a81f9e1-8b06-4c1f-b158-99d573456554", "SWIFT Code");
			zTextBoxColumnStyleInfo9.ColumnName = "PayeeBankSwift";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|4af32003-a40b-436e-96de-29b503a3fc7c", "IBAN Number");
			zTextBoxColumnStyleInfo10.ColumnName = "PayeeIBANNumber";
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|7390a124-f050-4576-add2-5130c28d967e", "Country/Region Code");
			zTextBoxColumnStyleInfo11.ColumnName = "PayeeCountryCode";
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|DD48B8D5-4478-4AC1-A2CE-E2BB3C4D60FA", "Bank Created By");
			zTextBoxColumnStyleInfo12.ColumnName = "BankCreateUser";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|25C76939-A3DF-4920-8B04-DDB4739AFAD9", "Bank Last Edit");
			zTextBoxColumnStyleInfo13.ColumnName = "BankLastEditUser";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|d716d0e6-0f64-409d-b6d2-f95974ed0326", "Auto Direct Debit");
			zCheckBoxColumnStyleInfo2.ColumnName = "AllowAutoDDR";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DepositBatchLineGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DepositBatchLineGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.DepositBatchLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DepositBatchLineGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DepositBatchLineGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.DepositBatchLineGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.DepositBatchLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.DepositBatchLineGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.DepositBatchLineGrid.CopySelectedRowsAllowed = true;
			this.DepositBatchLineGrid.GridId = "d8457de4-6f5b-427e-9bc5-feea732b00d7";
			this.DepositBatchLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DepositBatchLineGrid.LayoutKey = "DepositBatchLineGrid";
			this.DepositBatchLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 98, true);
			this.DepositBatchLineGrid.Name = "DepositBatchLineGrid";
			this.DepositBatchLineGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DepositBatchLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 214, true);
			this.DepositBatchLineGrid.TabIndex = 9;
			this.DepositBatchLineGrid.DoubleClick += new EventHandler(this.DepositBatchLineGrid_DoubleClick);
			// 
			// InvoiceAmountCalcFind
			// 
			this.InvoiceAmountCalcFind.AllowDrop = true;
			this.InvoiceAmountCalcFind.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.InvoiceAmountCalcFind.BackColor = System.Drawing.SystemColors.Window;
			this.InvoiceAmountCalcFind.BindToAmount = "AH_OSTotalAmount";
			this.InvoiceAmountCalcFind.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.InvoiceAmountCalcFind.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.InvoiceAmountCalcFind.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InvoiceAmountCalcFind.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 317, true);
			this.InvoiceAmountCalcFind.Name = "InvoiceAmountCalcFind";
			this.InvoiceAmountCalcFind.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.InvoiceAmountCalcFind.TabIndex = 11;
			// 
			// GenerateDDRFileButton
			// 
			this.GenerateDDRFileButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateDDRFileButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|a1dd588a-e666-4699-b84a-efc7425787ae", "Generate DDR File");
			this.GenerateDDRFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 383, true);
			this.GenerateDDRFileButton.Name = "GenerateDDRFileButton";
			this.GenerateDDRFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 24, true);
			this.GenerateDDRFileButton.TabIndex = 1;
			this.GenerateDDRFileButton.Click += new EventHandler(this.GenerateDDRFileButton_Click);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.zTabPage1);
			this.MainTabControl.Controls.Add(this.zEventTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 371, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// zTabPage1
			// 
			this.zTabPage1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|d339d14c-93ac-4f3e-994a-327172430c51", "DDR Batch");
			this.zTabPage1.Controls.Add(this.zCalcFind1);
			this.zTabPage1.Controls.Add(this.PostDateDateEdit);
			this.zTabPage1.Controls.Add(this.BatchDateDateEdit);
			this.zTabPage1.Controls.Add(this.zLabel2);
			this.zTabPage1.Controls.Add(this.zGroupBox1);
			this.zTabPage1.Controls.Add(this.ClearButton);
			this.zTabPage1.Controls.Add(this.FindButton);
			this.zTabPage1.Controls.Add(this.CancelledBatchLabel);
			this.zTabPage1.Controls.Add(this.BatchNoTextBox);
			this.zTabPage1.Controls.Add(this.BankAccountFindBox);
			this.zTabPage1.Controls.Add(this.InvoiceAmountCalcFind);
			this.zTabPage1.Controls.Add(this.BankReferenceNumberTextBox);
			this.zTabPage1.Controls.Add(this.DepositBatchLineGrid);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 344, true);
			this.zTabPage1.TabIndex = 1;
			// 
			// zCalcFind1
			// 
			this.zCalcFind1.AllowDrop = true;
			this.zCalcFind1.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zCalcFind1.BackColor = System.Drawing.SystemColors.Window;
			this.zCalcFind1.BindToAmount = "AH_LocalTotalAmount";
			this.zCalcFind1.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.zCalcFind1.BindToUnit = "AH_Calc_LocalRX";
			this.zCalcFind1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 317, true);
			this.zCalcFind1.Name = "zCalcFind1";
			this.zCalcFind1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.zCalcFind1.TabIndex = 12;
			// 
			// PostDateDateEdit
			// 
			this.PostDateDateEdit.AllowDrop = true;
			this.PostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DirectDebitBatchHeader)(null)).AH_PostDate)));
			this.PostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1b7edfa9-0498-45dd-83c9-c9c29bc03452", "Post Date");
			this.PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 64, true);
			this.PostDateDateEdit.Name = "PostDateDateEdit";
			this.PostDateDateEdit.TabIndex = 5;
			// 
			// BatchDateDateEdit
			// 
			this.BatchDateDateEdit.AllowDrop = true;
			this.BatchDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BatchDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BatchDateDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DirectDebitBatchHeader)(null)).AH_InvoiceDate)));
			this.BatchDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4ddcd735-4509-4fb5-b8f0-f34ec52ec4db", "Batch Date");
			this.BatchDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 64, true);
			this.BatchDateDateEdit.Name = "BatchDateDateEdit";
			this.BatchDateDateEdit.TabIndex = 4;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|291ca635-710c-4170-8832-a594bb3bf1a6", "This reference will be used for Reconciliation purpose. You can override or leave blank to save with batch number.");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 37, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 21, true);
			this.zLabel2.TabIndex = 3;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.AutoDDRRadioButton);
			this.zGroupBox1.Controls.Add(this.NonAutoDDRRadioButton);
			this.zGroupBox1.Controls.Add(this.BothRadioButton);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 79, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 37, true);
			this.zGroupBox1.TabIndex = 6;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Visible = false;
			// 
			// AutoDDRRadioButton
			// 
			this.AutoDDRRadioButton.AutoCheck = false;
			this.AutoDDRRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|1d98f8c6-a275-4da1-bc4c-d60c23eab68e", "Auto DDR");
			this.AutoDDRRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoDDRRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 12, true);
			this.AutoDDRRadioButton.Name = "AutoDDRRadioButton";
			this.AutoDDRRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.AutoDDRRadioButton.TabIndex = 0;
			// 
			// NonAutoDDRRadioButton
			// 
			this.NonAutoDDRRadioButton.AutoCheck = false;
			this.NonAutoDDRRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|53bf95e1-7be9-4a5f-94cf-42fa4bd98df6", "Non-Auto DDR");
			this.NonAutoDDRRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NonAutoDDRRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 11, true);
			this.NonAutoDDRRadioButton.Name = "NonAutoDDRRadioButton";
			this.NonAutoDDRRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.NonAutoDDRRadioButton.TabIndex = 1;
			// 
			// BothRadioButton
			// 
			this.BothRadioButton.AutoCheck = false;
			this.BothRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|5e98f2f4-3852-400c-b9db-decad0b371fa", "Both");
			this.BothRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BothRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 11, true);
			this.BothRadioButton.Name = "BothRadioButton";
			this.BothRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.BothRadioButton.TabIndex = 2;
			// 
			// ClearButton
			// 
			this.ClearButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|fff87531-cbb3-4982-ac85-1550947102ef", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 89, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.ClearButton.TabIndex = 8;
			this.ClearButton.Visible = false;
			// 
			// FindButton
			// 
			this.FindButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|1453c5fe-9ad9-4896-984a-f95cbde4a9a3", "Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 89, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.FindButton.TabIndex = 7;
			this.FindButton.Visible = false;
			// 
			// CancelledBatchLabel
			// 
			this.CancelledBatchLabel.BackColor = System.Drawing.SystemColors.AppWorkspace;
			this.CancelledBatchLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|9720735c-7786-472f-b1a6-ec249d4dfd5f", "This DDR batch is canceled.");
			this.CancelledBatchLabel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.CancelledBatchLabel.IsFontBold = true;
			this.CancelledBatchLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 134, true);
			this.CancelledBatchLabel.Name = "CancelledBatchLabel";
			this.CancelledBatchLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 21, true);
			this.CancelledBatchLabel.TabIndex = 10;
			// 
			// BankReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BankReferenceNumberTextBox, "AH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((DirectDebitBatchHeader)(null)).AH_ChequeOrReference)));
			this.BankReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 37, true);
			this.BankReferenceNumberTextBox.Name = "BankReferenceNumberTextBox";
			this.BankReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.BankReferenceNumberTextBox.TabIndex = 2;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 344, true);
			this.zEventTabPage1.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(630, 383, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// DirectDebitBatchForm
			// 
			this.AutoAddPreviousNextButtons = false;

			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DirectDebitBatchForm|3594a056-7bef-4742-802e-f07842525b77", "Direct Debit Batch");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 433, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.GenerateDDRFileButton);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(DirectDebitBatchHeader);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.CashBook.DirectDebitBatch.DirectDebitBatchHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 453, true);
			this.Name = "DirectDebitBatchForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.GenerateDDRFileButton, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DepositBatchLineGrid)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}