using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.BankStatement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AccStatement = Enterprise.Accounting.Business.Base.AccStatement;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation
{
	public partial class BankReconcilationForm : ZForm, IDoDisplayModeBrowseOverride
	{
		#region Controls

		ZGuidFindBox BankGuidFindBox;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZDateEdit StatementDateDateEdit;
		ZGrid BankReconGrid;
		ZCalcEdit CashBookAmountCalcEdit;
		ZCalcEdit ClosingBalanceCalcEdit;
		ZCalcEdit UnclearedCashbookAmountCalcEdit;
		ZButton AutoReconcileButton;
		ZCalcEdit CashBookTotalCalcEdit;
		ZCalcEdit StatementTotalCalcEdit;
		ZCalcEdit TotalDifferenceCalcEdit;
		ZCalcEdit CurrentDebitTotalCalcEdit;
		ZCalcEdit CurrentCreditTotalCalcEdit;
		ZCalcEdit zCalcEdit1;
		ZButton BankTransactionButton;
		ZDateEdit ReconcileDateDateEdit;
		ZGroupBox zGroupBox1;
		ZGroupBox zGroupBox2;
		ZCalcEdit ClosingBalanceCalcReadOnlyEdit;
		ZCheckBox IgnoreRefCheckBox;
		ZButton BankStatementButton;
		ZButton FindFilterButton;
		ZButton ClearFilterButton;
		ZTextBox TextSearchFilterTextBox;
		ZDateEdit FromDateFilterDateEdit;
		ZDateEdit ToDateFilterDateEdit;
		ZCheckBox zCheckBox1;
		ZDropEdit DateFilterDropEdit;
		ZGroupBox FilterGroupBox;
		ZCalcEdit AmountFilterCalcEdit;
		ZDropEdit MethodFilterDropEdit;
		ZDropEdit TypeFilterDropEdit;
		ZCalcEdit UnclearedStatementAmountCalcEdit;
		ZCalcEdit AmendedBankStatementBalanceCalcEdit;

		#endregion

		public BankReconcilationForm()
		{
		}

		public BankReconcilationForm(Business.CashBook.BankReconciliation bankRecon)
			: base(bankRecon)
		{
			this.BankRecon = bankRecon;
			MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 544);

			bankRecon.BankAndDatesSet += new EventHandler(BankRecon_BankAndDatesSet);
			bankRecon.TransactionsPopulated += new EventHandler(BankRecon_TransactionsPopulated);
			bankRecon.BankOrDatesGoingToChange += new EventHandler(BankRecon_BankOrDatesGoingToChange);

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			BankStatementButton.Enabled = false;
			AutoReconcileButton.Enabled = false;
			BankTransactionButton.Enabled = false;

			ShowStatementTotal(false);

			BankReconGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("B799A4C2-6E7B-465d-883B-40F5AB81801B", "Re-Allocate Check Number"), HandleReallocateCheckNumber));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			ZFormStrategy.DoDisplayModeBrowse(this);
			fApplyButton.Enabled = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				BankRecon.BankAndDatesSet -= new EventHandler(BankRecon_BankAndDatesSet);
				BankRecon.TransactionsPopulated -= new EventHandler(BankRecon_TransactionsPopulated);
				BankRecon.BankOrDatesGoingToChange -= new EventHandler(BankRecon_BankOrDatesGoingToChange);
			}
			base.Dispose(disposing);
		}

		#region Overrides

		protected override void SaveInternal()
		{
			if (AccountingConfigurationRegistry.Instance.AutomaticallySaveReconciliationReportWhenSavingBankReconciliation.Value)
			{
				BankRecon.AttachHistoryEDocForCurrentSession();     // EDoc must be created before BusinessObjectFactory.SaveTogether(), which is in base().
			}
			base.SaveInternal();
		}

		#endregion

		#region Implementation

		protected void HandleReallocateCheckNumber(object sender, EventArgs e)
		{
			if (BankReconGrid.SelectedElements == null || BankReconGrid.SelectedElements.Length == 0)
			{
				Globals.Message.Show(ChequeNumberReallocator.NoTransactionIsSelectedToReAllocate);
			}
			else if (!Env.Security.ReprintReallocateCheque.IsAllowed)
			{
				Env.Security.ReprintReallocateCheque.ShowError();
			}
			else
			{
				ReallocateCheckNumbers();
			}
		}

		void ReallocateCheckNumbers()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var headerCollection = new List<TransactionHeader>();
			ZBool isSelectedElementsAllAPPayments = true;
			ZBool isSelectedElementsInDB = true;

			foreach (BusinessObject businessObject in BankReconGrid.SelectedElements)
			{
				AccTransactionHeader transactionHeader = businessObject as AccTransactionHeader;
				if (transactionHeader != null && transactionHeader.IsInDatabase &&
					(transactionHeader.AH_TransactionType == TransactionTypes.Payment || transactionHeader.AH_TransactionType == TransactionTypes.DirectPayment))
				{
					headerCollection.Add(factory.Load<TransactionHeader>(transactionHeader.PK));
				}
				else
				{
					isSelectedElementsAllAPPayments = false;
					if (transactionHeader != null && !transactionHeader.IsInDatabase)
					{
						isSelectedElementsInDB = false;
					}
					break;
				}
			}

			if (isSelectedElementsAllAPPayments && ChequeNumberReallocator.CheckIsValidForReallocationOfCheckNumbers(headerCollection))
			{
				AccChequeBook chequeBook = ((ICanUpdateChequeNumber)headerCollection[0]).ChequeBook;
				chequeBook.Reload();
				ChequeNumberReallocator reallocator = new ChequeNumberReallocator(factory, chequeBook, headerCollection, false);

				using (ChequeNumberReallocationForm form = new ChequeNumberReallocationForm(reallocator))
				{
					DialogResult result = ZFormModaliser.ShowDialogAndDispose(form);

					if (result == DialogResult.OK)
					{
						ZString errorMessage = reallocator.Process(transactions => new PaymentBatchPrintManager(transactions, null, TransactionTypes.Payment, factory).Print());
						if (errorMessage != ZString.Empty)
						{
							Globals.Message.ShowError(errorMessage);
						}
					}
				}
			}
			else
			{
				string errorMessage = ChequeNumberReallocator.ChequeNumberCannotBeReAllocated;
				if (!isSelectedElementsInDB)
				{
					errorMessage = Res.GetString("88a95365-8426-459b-8663-48bf1abd959a", "The check numbers on transactions you have selected cannot be re-allocated because some transactions are not saved.");
				}
				Globals.Message.ShowError(errorMessage);
			}
		}

		protected Business.CashBook.BankReconciliation BankRecon;

		void FindFilterButton_Click(object sender, EventArgs e)
		{
			string msg = BankRecon.ApplyFilter();
			if (!string.IsNullOrEmpty(msg))
			{
				Globals.Message.ShowError(msg);
			}
		}

		void BankStatementButton_Click(object sender, EventArgs e)
		{
			if (BankRecon.CanReloadTransactions)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				AccStatement.BankStatement bankStatement = factory.Load<AccStatement.BankStatement>(BankRecon.BankAccountPK);

				bankStatement.AB_LastReconcileDate = BankRecon.ReconcileDate;
				bankStatement.AB_LastStatementDate = BankRecon.StatementDate;
				bankStatement.StatementBalance = BankRecon.ClosingBalance;
				var snapshotBefore = BankRecon.MergedTransactionsSnapshot();

				BankStatementForm statementForm = new BankStatementForm(bankStatement);

				statementForm.DisplayMode = ODisplayMode.Browse;
				statementForm.Saved += new EventHandler(StatementForm_ClosedOrSaved);       // Manual data entry scenario
				statementForm.Closed += new EventHandler(StatementForm_ClosedOrSaved);      // Imported data entry scenario
				ZFormModaliser.Show(statementForm, this);

				void StatementForm_ClosedOrSaved(object sender2, EventArgs e2)
				{
					BankRecon.ReloadRecords();
					var snapshotAfter = BankRecon.MergedTransactionsSnapshot();
					BankRecon.TrackTransactionChanges(snapshotBefore, snapshotAfter);
					try
					{
						SaveInternal(); // To save the History eDoc after any changes to BankStatement
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						HandleSaveException(ex);
					}

					snapshotBefore = BankRecon.MergedTransactionsSnapshot();        // Ensure when we Save & Close the operation is idempotent.
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("4d3fd12c-d925-4408-9787-c056e418b231", "Please save your changes before entering bank statement."));
			}
		}

		void AutoReconcileButton_Click(object sender, EventArgs e)
		{
			new AutoReconciler().Match(BankRecon.GetStatements(), BankRecon.CombinedTransactions, IgnoreRefCheckBox.Checked);

			BankRecon.StatementTotalInfo.RefreshBinding();
			BankRecon.CashbookTotalInfo.RefreshBinding();
		}

		protected void HandleBankStatementButtonEnabled()
		{
			BankStatementButton.Enabled = BankRecon.AreKeyFieldsValid;
		}

		protected void HandleAutoReconcileButtonEnabled()
		{
			if (BankRecon.AreKeyFieldsValid && BankRecon.GetStatements() != null && BankRecon.GetStatements().Count > 0)
			{
				AutoReconcileButton.Enabled = true;
				IgnoreRefCheckBox.Enabled = true;
			}
			else
			{
				AutoReconcileButton.Enabled = false;
				IgnoreRefCheckBox.Enabled = false;
			}
		}

		protected void HandleBankTransactionButtonEnabled()
		{
			BankTransactionButton.Enabled = BankRecon.AreKeyFieldsValid;
		}

		void BankRecon_BankAndDatesSet(object sender, EventArgs e)
		{
			HandleBankStatementButtonEnabled();
			HandleAutoReconcileButtonEnabled();
			HandleBankTransactionButtonEnabled();

			ShowStatementTotal(BankRecon.GetStatements() != null && BankRecon.GetStatements().Count > 0);
		}

		void ShowStatementTotal(bool show)
		{
			StatementTotalCalcEdit.Visible = show;
			TotalDifferenceCalcEdit.Visible = show;
		}

		void ClearFilterButton_Click(object sender, EventArgs e)
		{
			BankRecon.ClearFilter();
		}

		void BankRecon_TransactionsPopulated(object sender, EventArgs e)
		{
			string message = BankRecon.ValidateBeforeLoad();
			if (!string.IsNullOrEmpty(message))
			{
				Globals.Message.ShowWarning(message, Res.GetString("596569a8-dc0e-477d-9e52-69f3ab2e66c0", "Bank Reconciliation"));
			}
		}

		void BankRecon_BankOrDatesGoingToChange(object sender, EventArgs e)
		{
			Business.CashBook.BankReconciliation.BankReconEventArgs args = (Business.CashBook.BankReconciliation.BankReconEventArgs)e;
			string message = Res.GetString("05564d9e-e2e2-4c7c-8f4e-d462068529d1", "Changing the {0} will reset the current bank reconciliation.\r\nDo you want to proceed with the change?", args.FieldName);
			args.Result = (Globals.Message.Show(message, Res.GetString("596569a8-dc0e-477d-9e52-69f3ab2e66c0", "Bank Reconciliation"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes);
		}

		void BankTransactionButton_Click(object sender, EventArgs e)
		{
			BankTransactionFormHelper helper = GetBankTransactionFormHelper(BankRecon.BankAccount, BankRecon.StatementDate, BankRecon.AdditionalTransactions, false);
			var snapshotBefore = BankRecon.MergedTransactionsSnapshot();
			if (helper.ShowBankTransactionForm() == DialogResult.OK)
			{
				BankRecon.ReloadAdditionalTransactions();
				var snapshotAfter = BankRecon.MergedTransactionsSnapshot();
				BankRecon.TrackTransactionChanges(snapshotBefore, snapshotAfter);
			}
		}

		internal virtual BankTransactionFormHelper GetBankTransactionFormHelper(AccBankAccount bankAccount, ZDateTime statementDate, DirectTransactionsBusinessObject transactions, bool isReadOnly)
		{
			return new BankTransactionFormHelper(bankAccount, statementDate, transactions, isReadOnly);
		}

		#endregion
	}
}

