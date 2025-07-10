using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation
{
	public partial class BankTransactionForm : ZChildForm
	{
		public BankTransactionForm(DirectTransactionsBusinessObject transactionsBizO) : base(transactionsBizO)
		{
			this.TransactionsBizO = transactionsBizO;
			transactionsBizO.Headers.NotifyUserAboutNotSupportedException += new DirectTransactionHeaderBaseCollection.ShowNotSupportedExceptionHandler(NotifyUserAboutNotSupportedException);
			UpdateGridColumns();

			DirectReceiptSplitter.AllowOverlap(DirectReceiptLinesPanel);
			DirectPaymentSplitter.AllowOverlap(DirectPaymentLinesPanel);
			DirectReceiptLinesGrid.AllowOutsideOfParent();
			DirectPaymentLinesGrid.AllowOutsideOfParent();
		}

		void UpdateGridColumns()
		{
			if (!this.IsDesignMode())
			{
				CashBookTransactionGUIHelper.ConfigureColumns(DirectPaymentLinesGrid, TransactionTypes.DirectPayment, null);
				CashBookTransactionGUIHelper.ConfigureColumns(DirectReceiptLinesGrid, TransactionTypes.DirectReceipt, null);
			}

			SetTransactionHeaderCaptionString();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			DirectPaymentsGrid.ContextMenu.Popup += DirectPaymentGridMenu_Popup;
			DirectReceiptsGrid.ContextMenu.Popup += DirectReceiptGridMenu_Popup;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DirectPaymentsGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, [TransactionHeaderWithLines.Schema.AH_GB_TaxBranch, TransactionHeaderWithLines.Schema.AH_Calc_TaxBranchName]);

			DirectPaymentLinesGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, [DependentTransactionLine.Schema.AL_GB_TaxBranch , DependentTransactionLine.Schema.TaxBranchName]);

			DirectReceiptsGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, [TransactionHeaderWithLines.Schema.AH_GB_TaxBranch, TransactionHeaderWithLines.Schema.AH_Calc_TaxBranchName]);

			DirectReceiptLinesGrid.SetAvailability(AccountingMasterFilesUtils.IsTaxBranchApplicable, [DependentTransactionLine.Schema.AL_GB_TaxBranch, DependentTransactionLine.Schema.TaxBranchName]);
		}

		void SetTransactionHeaderCaptionString()
		{
			CommonSetting(DirectPaymentsGrid);
			DirectPaymentsGrid.GetColumnStyle("AH_DrawerBranch").CaptionResourceString
				= Res.GetData("BankTransactionForm|AC678089-9CA4-4F12-98E5-5B18964B31FB", "BSB Number");
			DirectPaymentsGrid.GetColumnStyle("AH_DrawerBank").CaptionResourceString
				= Res.GetData("BankTransactionForm|B4658252-5D7A-42F1-9D26-DFE3F1A1211F", "Account Number");

			CommonSetting(DirectReceiptsGrid);
			DirectReceiptsGrid.GetColumnStyle("AH_DrawerBranch").CaptionResourceString
				= Res.GetData("BankTransactionForm|C0E3D6A1-4C89-4310-B03B-A03F76F7846A", "Branch");
			DirectReceiptsGrid.GetColumnStyle("AH_DrawerBank").CaptionResourceString
				= Res.GetData("BankTransactionForm|8BD7D140-6994-4863-90F9-36A4EBCC8DE9", "Bank");

			void CommonSetting(ZGrid zGrid)
			{
				zGrid.GetColumnStyle("AH_TransactionType").CaptionResourceString
					= Res.GetData("BankTransactionForm|ee3269a0-25e5-407e-915c-e890b6c904cb", "Type");

				zGrid.GetColumnStyle("AH_InvoiceDate").CaptionResourceString
					= Res.GetData("BankTransactionForm|03daf1db-d50a-4899-ab9b-b2132f277177", "Transaction Date");

				zGrid.GetColumnStyle("AH_PostDate").CaptionResourceString
					= Res.GetData("BankTransactionForm|be2233c3-3332-40c3-b98f-72cfbca758bb", "Post Date");

				zGrid.GetColumnStyle("AH_ReceiptType").CaptionResourceString
					= Res.GetData("BankTransactionForm|e0c7dadf-1679-4f21-87c2-59b68a1873d9", "Pay/Rcpt Type");

				zGrid.GetColumnStyle("AH_ChequeOrReference").CaptionResourceString
					= Res.GetData("BankTransactionForm|a98c7f88-5752-4082-8ebb-725b7f9f43b3", "Chq./Ref.", "Cheque/Ref.");

				zGrid.GetColumnStyle("AH_ChequeDrawer").CaptionResourceString
					= Res.GetData("BankTransactionForm|82a77c08-6ad3-437a-abd9-22498be24fea", "Drawer/Payee");

				zGrid.GetColumnStyle("AH_Calc_TaxBranchName").CaptionResourceString
					= Res.GetData("f96f382c-e051-41da-a880-39b993e575f2", "Tax Branch Name");
			}
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			TransactionsBizO.Headers.NotifyUserAboutNotSupportedException -= new DirectTransactionHeaderBaseCollection.ShowNotSupportedExceptionHandler(NotifyUserAboutNotSupportedException);
		}

		void NotifyUserAboutNotSupportedException(object sender, string message)
		{
			Globals.Message.ShowError(message);
		}

		#region Implementation

		readonly DirectTransactionsBusinessObject TransactionsBizO;

		BusinessObjectFactory Factory
		{
			get { return TransactionsBizO.Factory; }
		}

		void NewDirectReceiptButton_Click(object sender, EventArgs e)
		{
			BankReconDirectReceipt receipt = Factory.New<BankReconDirectReceipt>();
			SetupDirectTransaction(receipt);
			TransactionsBizO.DirectReceipts.Add(receipt);
			TabControl.SelectTab(DirectReceiptTabPage);
			DirectReceiptsGrid.Focus();
			DirectReceiptsGrid.ListManager.Position = DirectReceiptsGrid.List.Count - 1;
		}

		void NewDirectPaymentButton_Click(object sender, EventArgs e)
		{
			BankReconDirectPayment payment = Factory.New<BankReconDirectPayment>();
			SetupDirectTransaction(payment);
			TransactionsBizO.DirectPayments.Add(payment);
			TabControl.SelectTab(DirectPaymentTabPage);
			DirectPaymentsGrid.Focus();
			DirectPaymentsGrid.ListManager.Position = DirectPaymentsGrid.List.Count - 1;
		}

		void SetupDirectTransaction(DirectTransactionHeaderBase directTransaction)
		{
			directTransaction.AH_AB = TransactionsBizO.ReconciliationBankAccountPK.IsValid ? TransactionsBizO.ReconciliationBankAccountPK : ZGuid.Empty;
			directTransaction.AH_ReceiptType = "";
			directTransaction.AH_RX_NKTransactionCurrency_ReadOnly = !directTransaction.AH_RX_NKTransactionCurrency.IsEmpty;
			directTransaction.CheckCurrencyOnEqualToCurrectCompany = true;
		}

		void ApplyButton_Click(object sender, EventArgs e)
		{
			ApplyButton.Focus();
			TransactionsBizO.RunPreSaveValidation();
			if (!TransactionsBizO.HasErrors)
			{
				foreach (BankReconDirectReceipt directReceipt in TransactionsBizO.DirectReceipts)
				{
					directReceipt.MakeDepositBatch();
					TransactionsBizO.AddToBatchToDirectReceiptPKMapping(directReceipt.RelatedDepositBatch.PK, directReceipt.PK);
				}
				TransactionsBizO.MoveDirectReceiptsAndPaymentsToBaseCollection();
				DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		#region Menu Item Handlers

		void DirectPaymentGridMenu_Popup(object sender, EventArgs e)
		{
			DirectPaymentReceiptGridMenu_Popup(DirectPaymentsGrid, EditDirectReceiptMenuItem);
		}

		void DirectReceiptGridMenu_Popup(object sender, EventArgs e)
		{
			DirectPaymentReceiptGridMenu_Popup(DirectReceiptsGrid, EditDirectPaymentMenuItem);
		}

		void DirectPaymentReceiptGridMenu_Popup(ZGrid grid, MenuItem menuItem)
		{
			if (grid.SelectedElements.Length == 1)
			{
				if (!grid.ContextMenu.MenuItems.Contains(menuItem))
				{
					grid.ContextMenu.MenuItems.Add(0, menuItem);
				}
			}
			else
			{
				grid.ContextMenu.MenuItems.Remove(menuItem);
			}
		}

		void EditDirectReceipt(object sender, EventArgs e)
		{
			HandleDirectReceiptOrPayment(DirectPaymentsGrid, ControllerIDs.BankReconDirectPayment);
		}

		void EditDirectPayment(object sender, EventArgs e)
		{
			HandleDirectReceiptOrPayment(DirectReceiptsGrid, ControllerIDs.BankReconDirectReceipt);
		}

		IZForm HandleDirectReceiptOrPayment(ZGrid grid, ControllerID controllerID)
		{
			IZForm form = null;
			if (grid.SelectedElements.Length == 1)
			{
				var bizo = grid.SelectedElements[0] as DirectTransactionHeaderBase;
				var controller = ZControllerFactory.Create(controllerID);
				controller.SetFormsModalTo(this);
				form = controller.ShowEditForm(bizo);
				form.Closed += EditDirectReceiptOrPaymentForm_Closed;
			}
			return form;
		}

		void EditDirectReceiptOrPaymentForm_Closed(object sender, EventArgs e)
		{
			var businessEntity = ((ZForm)sender).BusinessEntity as DirectTransactionHeaderBase;
			businessEntity.RunPreSaveValidation();
		}

		#endregion

		#region Menu Item

		string EditMenuItemText => ResString.GetMultilingualString("0E8F7FFB-E8A3-4BE7-98E9-F0073C3F6CD6", "Edit");

		MenuItem EditDirectReceiptMenuItem => editDirectReceiptMenuItem ?? (editDirectReceiptMenuItem = new ZMenuItem(EditMenuItemText, new EventHandler(EditDirectReceipt)));
		MenuItem editDirectReceiptMenuItem;

		MenuItem EditDirectPaymentMenuItem => editDirectPaymentMenuItem ?? (editDirectPaymentMenuItem = new ZMenuItem(EditMenuItemText, new EventHandler(EditDirectPayment)));
		MenuItem editDirectPaymentMenuItem;

		#endregion

		#endregion
	}
}

