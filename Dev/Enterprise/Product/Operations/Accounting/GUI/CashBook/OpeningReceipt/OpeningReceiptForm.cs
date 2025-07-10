using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class OpeningReceiptForm : AccountingZForm
	{
		public OpeningReceiptForm(OpeningReceipt openingRec) : base(openingRec)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, oPostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			DisplayModeChanged += OpeningReceiptForm_DisplayModeChanged;
		}

		void OpeningReceiptForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override bool ShowAuditTab => true;

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		void OpeningReceiptTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ChequeDetailsGroupBox = new ZGroupBox();
			this.DrawerBranchTextBox = new ZArchitecture.ZTextBox();
			this.DrawerBankTextBox = new ZArchitecture.ZTextBox();
			this.ChequeDrawerTextBox = new ZArchitecture.ZTextBox();
			this.ReceiptAmountGroupBox = new ZGroupBox();
			this.LocalAmountCalcFindBox = new ZCalcFindBox();
			this.OSAmountCalcFindBox = new ZCalcFindBox();
			this.ExchangeRateControl = new ZExchangeRateControl();
			this.BankDetailsGroupBox = new ZGroupBox();
			this.ChequeNumberTextBox = new ZArchitecture.ZTextBox();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.ReceiptTypeDropEdit = new ZDropEdit();
			this.ReceiptDetailsGroupBox = new ZGroupBox();
			this.TransactionNumTextBox = new ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new ZArchitecture.ZTextBox();
			this.OrganisationGuidFindBox = new ZGuidFindBox();
			this.PostDateEdit = new ZDateEdit();
			this.InvoiceDateEdit = new ZDateEdit();
			this.OpeningReceiptTabPage.SuspendLayout();
			this.ChequeDetailsGroupBox.SuspendLayout();
			this.ReceiptAmountGroupBox.SuspendLayout();
			this.BankDetailsGroupBox.SuspendLayout();
			this.ReceiptDetailsGroupBox.SuspendLayout();
			this.OpeningReceiptTabPage.Controls.Add(this.ChequeDetailsGroupBox);
			this.OpeningReceiptTabPage.Controls.Add(this.ReceiptAmountGroupBox);
			this.OpeningReceiptTabPage.Controls.Add(this.BankDetailsGroupBox);
			this.OpeningReceiptTabPage.Controls.Add(this.ReceiptDetailsGroupBox);
			// 
			// ChequeDetailsGroupBox
			// 
			this.ChequeDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|6cc7b3c0-25bd-4453-8f4e-00be9ac8d13e", "Check Details");
			this.ChequeDetailsGroupBox.Controls.Add(this.DrawerBranchTextBox);
			this.ChequeDetailsGroupBox.Controls.Add(this.DrawerBankTextBox);
			this.ChequeDetailsGroupBox.Controls.Add(this.ChequeDrawerTextBox);
			this.ChequeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 280);
			this.ChequeDetailsGroupBox.Name = "ChequeDetailsGroupBox";
			this.ChequeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 96);
			this.ChequeDetailsGroupBox.TabIndex = 3;
			this.ChequeDetailsGroupBox.TabStop = false;
			// 
			// DrawerBranchTextBox
			// 
			this.BindingSource.SetBindingMember(this.DrawerBranchTextBox, "AH_DrawerBranch");
			this.DrawerBranchTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|618390a1-eccc-43ac-bfc2-04aab2aa1536", "Branch");
			this.DrawerBranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 64);
			this.DrawerBranchTextBox.Name = "DrawerBranchTextBox";
			this.DrawerBranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.DrawerBranchTextBox.TabIndex = 12;
			// 
			// DrawerBankTextBox
			// 
			this.BindingSource.SetBindingMember(this.DrawerBankTextBox, "AH_DrawerBank");
			this.DrawerBankTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|744ead8c-c951-4b34-907a-cb7bb3c9365c", "Bank");
			this.DrawerBankTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 40);
			this.DrawerBankTextBox.Name = "DrawerBankTextBox";
			this.DrawerBankTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.DrawerBankTextBox.TabIndex = 11;
			// 
			// ChequeDrawerTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeDrawerTextBox, "AH_ChequeDrawer");
			this.ChequeDrawerTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|f08c4538-18c3-491a-9fd7-8d6232cb13a3", "Drawer");
			this.ChequeDrawerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16);
			this.ChequeDrawerTextBox.Name = "ChequeDrawerTextBox";
			this.ChequeDrawerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.ChequeDrawerTextBox.TabIndex = 10;
			// 
			// ReceiptAmountGroupBox
			// 
			this.ReceiptAmountGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|a5967240-60e4-4d36-a888-cfd4bdab7ffb", "Receipt Amount");
			this.ReceiptAmountGroupBox.Controls.Add(this.LocalAmountCalcFindBox);
			this.ReceiptAmountGroupBox.Controls.Add(this.OSAmountCalcFindBox);
			this.ReceiptAmountGroupBox.Controls.Add(this.ExchangeRateControl);
			this.ReceiptAmountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 212);
			this.ReceiptAmountGroupBox.Name = "ReceiptAmountGroupBox";
			this.ReceiptAmountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 68);
			this.ReceiptAmountGroupBox.TabIndex = 2;
			this.ReceiptAmountGroupBox.TabStop = false;
			// 
			// LocalAmountCalcFindBox
			// 
			this.LocalAmountCalcFindBox.BindToAmount = "AH_LocalExTaxAmount";
			this.LocalAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_LocalRXDecimals";
			this.LocalAmountCalcFindBox.BindToUnit = "AH_Calc_LocalRX";
			this.LocalAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|539ab0b3-d759-4e57-98ff-317ea6fedb74", "Local Amount");
			this.LocalAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 40);
			this.LocalAmountCalcFindBox.Name = "LocalAmountCalcFindBox";
			this.LocalAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20);
			this.LocalAmountCalcFindBox.TabIndex = 10;
			// 
			// OSAmountCalcFindBox
			// 
			this.OSAmountCalcFindBox.BindToAmount = "AH_OSExTaxAmount";
			this.OSAmountCalcFindBox.BindToDecimalPlaces = "AH_Calc_RXDecimals";
			this.OSAmountCalcFindBox.BindToUnit = "AH_RX_NKTransactionCurrency";
			this.OSAmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|482af50a-f207-4ddc-8039-ba53daf7739a", "Receipt Amount", "Amount Excluding Tax.");
			this.OSAmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.OSAmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 40);
			this.OSAmountCalcFindBox.Name = "OSAmountCalcFindBox";
			this.OSAmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20);
			this.OSAmountCalcFindBox.TabIndex = 9;
			// 
			// ExchangeRateControl
			// 
			this.BindingSource.SetBindingMember(this.ExchangeRateControl, "ExchangeRate");
			this.ExchangeRateControl.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|d0529760-d5aa-4425-afb4-1295de5b663c", "Exchange Rate");
			this.ExchangeRateControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 16);
			this.ExchangeRateControl.Name = "ExchangeRateControl";
			this.ExchangeRateControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20);
			this.ExchangeRateControl.TabIndex = 8;
			// 
			// BankDetailsGroupBox
			// 
			this.BankDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|6f3c2770-02af-431b-889c-dea4715a8051", "Bank Details");
			this.BankDetailsGroupBox.Controls.Add(this.ChequeNumberTextBox);
			this.BankDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.BankDetailsGroupBox.Controls.Add(this.ReceiptTypeDropEdit);
			this.BankDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 120);
			this.BankDetailsGroupBox.Name = "BankDetailsGroupBox";
			this.BankDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 92);
			this.BankDetailsGroupBox.TabIndex = 1;
			this.BankDetailsGroupBox.TabStop = false;
			// 
			// ChequeNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeNumberTextBox, "AH_ChequeOrReference");
			this.ChequeNumberTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|7bb4dfac-c6a3-490e-84a5-b02c7f1c6f24", "Check No");
			this.ChequeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 64);
			this.ChequeNumberTextBox.Name = "ChequeNumberTextBox";
			this.ChequeNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.ChequeNumberTextBox.TabIndex = 9;
			// 
			// BankAccountGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "AH_AB");
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 40);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.PopupCaption = null;
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.BankAccountGuidFindBox.TabIndex = 8;
			// 
			// ReceiptTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ReceiptTypeDropEdit, "AH_ReceiptType");
			this.ReceiptTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 16);
			this.ReceiptTypeDropEdit.Name = "ReceiptTypeDropEdit";
			this.ReceiptTypeDropEdit.PreBoundMaxLength = 3;
			this.ReceiptTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.ReceiptTypeDropEdit.TabIndex = 7;
			// 
			// ReceiptDetailsGroupBox
			// 
			this.ReceiptDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|aff8cc2e-9076-41c5-89f3-6fcc785e9a1c", "Receipt Details");
			this.ReceiptDetailsGroupBox.Controls.Add(this.TransactionNumTextBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.OrganisationGuidFindBox);
			this.ReceiptDetailsGroupBox.Controls.Add(this.PostDateEdit);
			this.ReceiptDetailsGroupBox.Controls.Add(this.InvoiceDateEdit);
			this.ReceiptDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4);
			this.ReceiptDetailsGroupBox.Name = "ReceiptDetailsGroupBox";
			this.ReceiptDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 116);
			this.ReceiptDetailsGroupBox.TabIndex = 0;
			this.ReceiptDetailsGroupBox.TabStop = false;
			// 
			// TransactionNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransactionNumTextBox, "AH_TransactionNum");
			this.TransactionNumTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|76618bf1-e9ab-41ed-ba5a-358222eaa282", "Receipt No.");
			this.TransactionNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(332, 16);
			this.TransactionNumTextBox.Name = "TransactionNumTextBox";
			this.TransactionNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 20);
			this.TransactionNumTextBox.TabIndex = 8;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "AH_Desc");
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|f9d3d5e8-1a1a-4fc2-aeab-2c1cdf2035fe", "Description");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 88);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.DescriptionTextBox.TabIndex = 7;
			// 
			// OrganisationGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.OrganisationGuidFindBox, "AH_OH");
			this.OrganisationGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|31fdaef1-429d-4650-b0ee-6c836e75b469", "Account");
			this.OrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 64);
			this.OrganisationGuidFindBox.Name = "OrganisationGuidFindBox";
			this.OrganisationGuidFindBox.PopupCaption = null;
			this.OrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 20);
			this.OrganisationGuidFindBox.TabIndex = 6;
			// 
			// PostDateEdit
			// 
			this.PostDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateEdit, "AH_PostDate");
			this.PostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|a18bb007-7614-471d-b3bb-6f121cd6785d", "Post Date");
			this.PostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 40);
			this.PostDateEdit.Name = "PostDateEdit";
			this.PostDateEdit.TabIndex = 5;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.InvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "AH_InvoiceDate");
			this.InvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("OpeningReceiptForm|3468d4f1-fb5e-4511-8aa7-6b591d3775a1", "Date");
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 16);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 4;
			this.ChequeDetailsGroupBox.ResumeLayout(false);
			this.ChequeDetailsGroupBox.PerformLayout();
			this.ReceiptAmountGroupBox.ResumeLayout(false);
			this.BankDetailsGroupBox.ResumeLayout(false);
			this.BankDetailsGroupBox.PerformLayout();
			this.ReceiptDetailsGroupBox.ResumeLayout(false);
			this.ReceiptDetailsGroupBox.PerformLayout();
			this.OpeningReceiptTabPage.ResumeLayout(true);
		}

		void zEventTabPage1_InitializeTab(object sender, EventArgs e)
		{
		}
	}
}

