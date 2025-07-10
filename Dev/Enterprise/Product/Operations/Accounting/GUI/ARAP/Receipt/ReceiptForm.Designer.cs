using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.OrgCollectionCalls;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class ReceiptForm
	{


		#region Windows Form Designer generated code

		ZGroupBox ReceiptDetailsGroupBox;
		ZDateEdit InvoiceDateEdit;
		protected ZDateEdit PostDateEdit;
		protected ZGuidFindBox OrganisationGuidFindBox;
		ZTextBox DescriptionTextBox;
		protected ZTextBox ReceiptNoTextBox;
		ZGroupBox BankDetailsGroupBox;
		ZDropEdit ReceiptTypeDropEdit;
		ZGuidFindBox BankAccountGuidFindBox;
		ZTextBox ChequeNoTextBox;
		ZGroupBox ReceiptAmountGroupBox;
		ZExchangeRateControl zExchangeRateControl1;
		ZCalcFindBox ReceiptAmountCalcFindBox;
		ZCalcFindBox zCalcFindBox1;
		ZGroupBox ChequeDetailsGroupBox;
		ZTextBox DrawerTextBox;
		ZTextBox BankTextBox;
		ZTextBox BranchTextBox;
		protected Core.Forms.ZPostOrCancelButton CloseButton;
		protected Core.Forms.ZPostOrCancelButton ReceiptDetailButton;
		ZTemplateTabControl zTabControl1;
		ZTabPage ReceiptTabPage;
		ZLogsTabPage EventTabPage;
		protected Core.Forms.ZPostOrCancelButton PostWithoutMatchingButton;
		protected ZCalcEdit AH_NumberOfSupportingDocumentsCalcEdit;
		protected ZDateEdit UnmatchDateEdit;
		private MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ReceiptDetailsGroupBox = new ZGroupBox();
			this.UnmatchDateEdit = new ZDateEdit();
			this.AH_NumberOfSupportingDocumentsCalcEdit = new ZCalcEdit();
			this.ReceiptNoTextBox = new ZTextBox();
			this.DescriptionTextBox = new ZTextBox();
			this.OrganisationGuidFindBox = new ZGuidFindBox();
			this.PostDateEdit = new ZDateEdit();
			this.InvoiceDateEdit = new ZDateEdit();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.ChequeNoTextBox = new ZTextBox();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.ReceiptTypeDropEdit = new ZDropEdit();
			this.ReceiptAmountGroupBox = new ZGroupBox();
			this.zCalcFindBox1 = new ZCalcFindBox();
			this.ReceiptAmountCalcFindBox = new ZCalcFindBox();
			this.zExchangeRateControl1 = new ZExchangeRateControl();
			this.ChequeDetailsGroupBox = new ZGroupBox();
			this.BranchTextBox = new ZTextBox();
			this.BankTextBox = new ZTextBox();
			this.DrawerTextBox = new ZTextBox();
			this.ReceiptDetailButton = new Core.Forms.ZPostOrCancelButton();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.zTabControl1 = new ZTemplateTabControl();
			this.ReceiptTabPage = new ZTabPage();
			this.WorkflowTabPage = new MasterFiles.GUI.ZWorkflowTabPage();
			this.EventTabPage = new ZLogsTabPage();
			this.PostWithoutMatchingButton = new Core.Forms.ZPostOrCancelButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReceiptDetailsGroupBox.SuspendLayout();
			this.UnmatchDateEdit.SuspendLayout();
			this.OrganisationGuidFindBox.SuspendLayout();
			this.PostDateEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.BankDetailsGroupBox.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.ReceiptTypeDropEdit.SuspendLayout();
			this.ReceiptAmountGroupBox.SuspendLayout();
			this.zCalcFindBox1.SuspendLayout();
			this.ReceiptAmountCalcFindBox.SuspendLayout();
			this.zExchangeRateControl1.SuspendLayout();
			this.ChequeDetailsGroupBox.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.ReceiptTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 453, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(275);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Receipt);
			// 
			// ReceiptDetailsGroupBox
			// 
			this.ReceiptDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|6124e70d-3682-4c43-9309-3f28f44eac20", "Receipt Details");
			this.ReceiptDetailsGroupBox.Controls.Add(this.UnmatchDateEdit);
			this.ReceiptDetailsGroupBox.Controls.Add(this.AH_NumberOfSupportingDocumentsCalcEdit);
			this.ReceiptDetailsGroupBox.Controls.Add(this.ReceiptNoTextBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.OrganisationGuidFindBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.ReceiptDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.ReceiptDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 7, true);
			this.ReceiptDetailsGroupBox.Name = "ReceiptDetailsGroupBox";
			this.ReceiptDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 112, true);
			this.ReceiptDetailsGroupBox.TabIndex = 1;
			this.ReceiptDetailsGroupBox.TabStop = false;
			// 
			// UnmatchDateEdit
			// 
			this.UnmatchDateEdit.AllowDrop = true;
			this.UnmatchDateEdit.AutoCompleteMonthThreshold = 1;
			this.UnmatchDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.UnmatchDateEdit, "UnmatchDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Receipt)(null)).UnmatchDate)));
			this.UnmatchDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|d0ba4886-4aa2-43af-be62-f29d213e4750", "Unmatch Date");
			this.UnmatchDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 37, true);
			this.UnmatchDateEdit.Name = "UnmatchDateEdit";
			this.UnmatchDateEdit.TabIndex = 3;
			// 
			// AH_NumberOfSupportingDocumentsCalcEdit
			// 
			this.AH_NumberOfSupportingDocumentsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_NumberOfSupportingDocumentsCalcEdit, "AH_NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Receipt)(null)).AH_NumberOfSupportingDocuments)));
			this.AH_NumberOfSupportingDocumentsCalcEdit.DecimalPlaces = 2;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(447, 37, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.Name = "AH_NumberOfSupportingDocumentsCalcEdit";
			this.AH_NumberOfSupportingDocumentsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.AH_NumberOfSupportingDocumentsCalcEdit.TabIndex = 4;
			this.AH_NumberOfSupportingDocumentsCalcEdit.Text = "0";
			this.AH_NumberOfSupportingDocumentsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReceiptNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceiptNoTextBox, "AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Receipt)(null)).AH_TransactionNum)));
			this.ReceiptNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|27c3b09f-7669-4c08-9871-3bebde2e30c8", "Receipt No");
			this.ReceiptNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 15, true);
			this.ReceiptNoTextBox.Name = "ReceiptNoTextBox";
			this.ReceiptNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.ReceiptNoTextBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Receipt)(null)).AH_Desc)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 82, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.DescriptionTextBox.TabIndex = 6;
			// 
			// OrganisationGuidFindBox
			// 
			this.OrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationGuidFindBox, "AH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Receipt)(null)).AH_OH)));
			this.OrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 59, true);
			this.OrganisationGuidFindBox.Name = "OrganisationGuidFindBox";
			this.OrganisationGuidFindBox.PopupCaption = null;
			this.OrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.OrganisationGuidFindBox.TabIndex = 5;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AllowDrop = true;
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "AH_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Receipt)(null)).AH_PostDate)));
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|E7E3531E-A9A2-44A6-AAB8-E80DD5695D73", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 37, true);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 2;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "AH_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Receipt)(null)).AH_InvoiceDate)));
			this.InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|4106a8b5-71d4-484b-bfcf-e2efc6c2ca50", "Receipt Date");
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 0;
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|5785884c-9c42-4105-a569-ec87a46e4e74", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.ChequeNoTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.ReceiptTypeDropEdit);
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 119, true);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 89, true);
			this.BankDetailsGroupBox.TabIndex = 2;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// ChequeNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNoTextBox, "AH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Receipt)(null)).AH_ChequeOrReference)));
			this.ChequeNoTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|1b5c984e-066a-490d-8bb2-e3e52d231d43", "Reference No");
			this.ChequeNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 59, true);
			this.ChequeNoTextBox.Name = "ChequeNoTextBox";
			this.ChequeNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.ChequeNoTextBox.TabIndex = 3;
			// 
			// BankAccountGuidFindBox
			// 
			this.BankAccountGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "AH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Receipt)(null)).AH_AB)));
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 37, true);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.PopupCaption = null;
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.BankAccountGuidFindBox.TabIndex = 2;
			// 
			// ReceiptTypeDropEdit
			// 
			this.ReceiptTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReceiptTypeDropEdit, "AH_ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Receipt)(null)).AH_ReceiptType)));
			this.ReceiptTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.ReceiptTypeDropEdit.Name = "ReceiptTypeDropEdit";
			this.ReceiptTypeDropEdit.PreBoundMaxLength = 5;
			this.ReceiptTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.ReceiptTypeDropEdit.TabIndex = 1;
			// 
			// ReceiptAmountGroupBox
			// 
			this.ReceiptAmountGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|60f4e94d-b669-45f0-bdd8-07dc9b7be7b3", "Receipt Amount");
			this.ReceiptAmountGroupBox.Controls.Add(this.zCalcFindBox1);
			this.ReceiptAmountGroupBox.Controls.Add(this.ReceiptAmountCalcFindBox);
			this.ReceiptAmountGroupBox.Controls.Add(this.zExchangeRateControl1);
			this.ReceiptAmountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 208, true);
			this.ReceiptAmountGroupBox.Name = "ReceiptAmountGroupBox";
			this.ReceiptAmountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 67, true);
			this.ReceiptAmountGroupBox.TabIndex = 3;
			this.ReceiptAmountGroupBox.TabStop = false;
			// 
			// zCalcFindBox1
			// 
			this.zCalcFindBox1.AllowDrop = true;
			this.zCalcFindBox1.BindToAmount = "AH_LocalExTaxAmount";
			this.zCalcFindBox1.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.zCalcFindBox1.BindToUnit = "AH_Calc_LocalRX";
			this.zCalcFindBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|8f73f1fe-362c-4f6e-986f-da371928e92d", "Local Amount");
			this.zCalcFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 37, true);
			this.zCalcFindBox1.Name = "zCalcFindBox1";
			this.zCalcFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.zCalcFindBox1.TabIndex = 4;
			// 
			// ReceiptAmountCalcFindBox
			// 
			this.ReceiptAmountCalcFindBox.AllowDrop = true;
			this.ReceiptAmountCalcFindBox.BindToAmount = "AH_OSExTaxAmount";
			this.ReceiptAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.ReceiptAmountCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.ReceiptAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|31ce42f6-5635-4923-af57-0bd463ed0b14", "Receipt Amount");
			this.ReceiptAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ReceiptAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 37, true);
			this.ReceiptAmountCalcFindBox.Name = "ReceiptAmountCalcFindBox";
			this.ReceiptAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ReceiptAmountCalcFindBox.TabIndex = 2;
			// 
			// zExchangeRateControl1
			// 
			this.zExchangeRateControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zExchangeRateControl1, "ExchangeRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZArchitecture.Business.ZExchangeRate)(((Receipt)(null)).ExchangeRate)));
			this.zExchangeRateControl1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|e34589ae-3d9d-43c5-8df2-a035043cc038", "Exchange Rate");
			this.zExchangeRateControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.zExchangeRateControl1.Name = "zExchangeRateControl1";
			this.zExchangeRateControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.zExchangeRateControl1.TabIndex = 0;
			// 
			// ChequeDetailsGroupBox
			// 
			this.ChequeDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|77a35931-f957-44b2-915b-6233d6b980db", "Check Details");
			this.ChequeDetailsGroupBox.Controls.Add(this.BranchTextBox);
			this.ChequeDetailsGroupBox.Controls.Add(this.BankTextBox);
			this.ChequeDetailsGroupBox.Controls.Add(this.DrawerTextBox);
			this.ChequeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 275, true);
			this.ChequeDetailsGroupBox.Name = "ChequeDetailsGroupBox";
			this.ChequeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 89, true);
			this.ChequeDetailsGroupBox.TabIndex = 4;
			this.ChequeDetailsGroupBox.TabStop = false;
			// 
			// BranchTextBox
			// 
			this.BindingSource.SetBindingMember(this.BranchTextBox, "AH_DrawerBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Receipt)(null)).AH_DrawerBranch)));
			this.BranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 59, true);
			this.BranchTextBox.Name = "BranchTextBox";
			this.BranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.BranchTextBox.TabIndex = 5;
			// 
			// BankTextBox
			// 
			this.BindingSource.SetBindingMember(this.BankTextBox, "AH_DrawerBank");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Receipt)(null)).AH_DrawerBank)));
			this.BankTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 37, true);
			this.BankTextBox.Name = "BankTextBox";
			this.BankTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.BankTextBox.TabIndex = 4;
			// 
			// DrawerTextBox
			// 
			this.BindingSource.SetBindingMember(this.DrawerTextBox, "AH_ChequeDrawer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Receipt)(null)).AH_ChequeDrawer)));
			this.DrawerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.DrawerTextBox.Name = "DrawerTextBox";
			this.DrawerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 20, true);
			this.DrawerTextBox.TabIndex = 3;
			// 
			// ReceiptDetailButton
			// 
			this.ReceiptDetailButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ReceiptDetailButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|72b1fe8a-a5a9-4549-b2e4-e8c7012071e4", "Receipt Detail");
			this.ReceiptDetailButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 426, true);
			this.ReceiptDetailButton.Name = "ReceiptDetailButton";
			this.ReceiptDetailButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 21, true);
			this.ReceiptDetailButton.TabIndex = 10;
			this.ReceiptDetailButton.Click += new EventHandler(this.ReceiptDetailButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|44700c22-3d66-4b02-99ca-94617665ccc0", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 426, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 12;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.ReceiptTabPage);
			this.zTabControl1.Controls.Add(this.WorkflowTabPage);
			this.zTabControl1.Controls.Add(this.EventTabPage);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.SelectedIndex = 0;
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 413, true);
			this.zTabControl1.TabIndex = 7;
			// 
			// ReceiptTabPage
			// 
			this.ReceiptTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|aa795dae-2c59-42ec-be30-330ab788568e", "Receipt");
			this.ReceiptTabPage.Controls.Add(this.ReceiptAmountGroupBox);
			this.ReceiptTabPage.Controls.Add(this.ChequeDetailsGroupBox);
			this.ReceiptTabPage.Controls.Add(this.BankDetailsGroupBox);
			this.ReceiptTabPage.Controls.Add(this.ReceiptDetailsGroupBox);
			this.ReceiptTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ReceiptTabPage.Name = "ReceiptTabPage";
			this.ReceiptTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 386, true);
			this.ReceiptTabPage.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 834, true);
			this.WorkflowTabPage.TabIndex = 1;
			this.WorkflowTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.WorkflowTabPage_InitializeTab));
			// 
			// EventTabPage
			// 
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 409, true);
			this.EventTabPage.TabIndex = 2;
			// 
			// PostWithoutMatchingButton
			// 
			this.PostWithoutMatchingButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostWithoutMatchingButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|63bbade9-aa72-4905-9c14-659dbdaf6e2f", "Post Without Matching");
			this.PostWithoutMatchingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 426, true);
			this.PostWithoutMatchingButton.Name = "PostWithoutMatchingButton";
			this.PostWithoutMatchingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 21, true);
			this.PostWithoutMatchingButton.TabIndex = 11;
			// 
			// ReceiptForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 475, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ReceiptForm|6b584740-acab-479d-bc81-a435f6cd812a", "Receipt Form");
			this.Controls.Add(this.PostWithoutMatchingButton);
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ReceiptDetailButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Receipt);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ARAP.ReceiptPayment.Receipt";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ReceiptForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.ReceiptDetailButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostWithoutMatchingButton, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReceiptDetailsGroupBox.ResumeLayout(false);
			this.ReceiptDetailsGroupBox.PerformLayout();
			this.UnmatchDateEdit.ResumeLayout(true);
			this.UnmatchDateEdit.PerformLayout();
			this.OrganisationGuidFindBox.ResumeLayout(true);
			this.OrganisationGuidFindBox.PerformLayout();
			this.PostDateEdit.ResumeLayout(true);
			this.PostDateEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.BankDetailsGroupBox.ResumeLayout(false);
			this.BankDetailsGroupBox.PerformLayout();
			this.BankAccountGuidFindBox.ResumeLayout(true);
			this.BankAccountGuidFindBox.PerformLayout();
			this.ReceiptTypeDropEdit.ResumeLayout(true);
			this.ReceiptTypeDropEdit.PerformLayout();
			this.ReceiptAmountGroupBox.ResumeLayout(false);
			this.ReceiptAmountGroupBox.PerformLayout();
			this.zCalcFindBox1.ResumeLayout(true);
			this.zCalcFindBox1.PerformLayout();
			this.ReceiptAmountCalcFindBox.ResumeLayout(true);
			this.ReceiptAmountCalcFindBox.PerformLayout();
			this.zExchangeRateControl1.ResumeLayout(true);
			this.zExchangeRateControl1.PerformLayout();
			this.ChequeDetailsGroupBox.ResumeLayout(false);
			this.ChequeDetailsGroupBox.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.ReceiptTabPage.ResumeLayout(false);
			this.ReceiptTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}