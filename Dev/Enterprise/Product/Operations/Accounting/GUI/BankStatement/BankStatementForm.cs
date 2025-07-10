using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.DataTransfer.BankStatement;
using Enterprise.Accounting.GUI.CashBook.BankReconciliation;
using Enterprise.Billing.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.BankStatement
{
	public partial class BankStatementForm : ZForm, IDoDisplayModeBrowseOverride
	{
		#region Controls

		ZOpenFileDialog OpenFileDialog;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZGroupBox FilterGroupBox;
		ZDateEdit StatementDateFilterDateEdit;
		ZButton ClearFilterButton;
		ZButton FindFilterButton;
		ZDropEdit DebitCreditFilterDropEdit;
		ZCalcEdit AmountFilterCalcEdit;
		ZTextBox ChequeREferenceFilterTextBox;
		ZCalcEdit PageNoCalcEdit;
		ZDropEdit TypeFilterDropEdit;
		ZCalcEdit MaxPageCalcEdit1;
		ZButton ImportStatementButton;
		ZTabControl StatementsTabControl;
		ZTabPage UnreconciledTabPage;
		protected ZGrid UnreconciledStatementsGrid;
		ZTabPage ReconciledTabPage;
		protected ZGrid ReconciledStatementsGrid;
		ZButton BankTransactionButton;
		ZDateEdit StatementDateDateEdit;
		ZCalcEdit StatementBalanceCalcEdit;
		ZCalcEdit OpeningBalanceCalcEdit;
		ZCalcEdit PageBalanceTotalCalcEdit;
		ZLabel PageCreditTotalLabel;
		ZLabel PageDebitTotalLabel;
		ZLabel CreditTotalCountLabel;
		ZLabel DebitTotalCountLabel;
		ZCalcEdit NettAmountCalcEdit;
		ZLabel TotalCreditLabel;
		ZLabel TotalDebitLabel;
		ZLabel PageDebitLabel;
		ZLabel PageCreditLabel;
		IContainer components;

		#endregion

		public BankStatementForm()
		{
		}

		public BankStatementForm(Business.Base.AccStatement.BankStatement bankStatement)
			: base(bankStatement)
		{
			this.BankStatement = bankStatement;
			this.BankStatement.StatementTypeChanged += new StatementEventHandler(BankStatement_StatementTypeChanged);
			this.BankStatement.StatementDirectTransactionCreated += new StatementEventHandler(BankStatement_StatementDirectTransactionCreated);

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		public Business.Base.AccStatement.BankStatement BankStatement;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				BankStatement.StatementTypeChanged -= new StatementEventHandler(BankStatement_StatementTypeChanged);
				this.BankStatement.StatementDirectTransactionCreated -= new StatementEventHandler(BankStatement_StatementDirectTransactionCreated);

				if (components != null)
				{
					components.Dispose();
				}
				if (OpenFileDialog != null)
				{
					OpenFileDialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void IDoDisplayModeBrowseOverride.DoDisplayModeBrowse()
		{
			ZFormStrategy.DoDisplayModeEdit(this);
			fPostButton.Enabled = false;
			fApplyButton.Enabled = false;
			fCancelButton.Text = Res.GetString("BankStatementForm|F89FA4AE-3450-49b5-9EAE-7C6A1AB2991F", "&Close");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BankStatement.StatementDateFilter = BankStatement.AB_LastStatementDate;
			BankStatement.ApplyFilter(true);
			BankStatement.SetTotalValuesForTheDate();
		}

		#region Implementation

		void BankStatement_StatementTypeChanged(object sender, StatementEventArgs e)
		{
			string message = Res.GetString("d36b6b66-f039-4e1d-a19e-215e8cefaaf5", "You have selected a bank charge type.\r\nDo you want system to create a bank transaction?");
			string caption = Res.GetString("b1410ae3-361c-4952-982f-7a9c1fcbddbe", "Create Bank Transaction");
			e.Result = (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
		}

		void BankStatement_StatementDirectTransactionCreated(object sender, StatementEventArgs e)
		{
			ShowBankTransactionForm();
		}

		void ImportStatementButton_Click(object sender, EventArgs e)
		{
			ImportStatement();
		}

		void ImportStatement()
		{
			BankStatementFormat bankStatementFormat = new BankStatementFormat();
			if (ZFormModaliser.ShowDialogAndDispose(new StatementFileFormatForm(bankStatementFormat)) == DialogResult.OK)
			{
				OpenFileDialog.Filter = (NoResString)"XML files (*.xml)|*.xml|CSV files (*.csv)|*.csv|All files (*.*)|*.*"; // Hard coded file filter
				OpenFileDialog.FilterIndex = 3;
				OpenFileDialog.RestoreDirectory = true;

				if (OpenFileDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						ImportStatementFromFile(bankStatementFormat.StatementFileFormat, OpenFileDialog.ForceLocalFile());
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}

		void ImportStatementFromFile(BankStatementFormat.StatementFileFormats fileFormat, string fileName)
		{
			BankStatementDataAdapter adapter = new BankStatementDataAdapter();
			BankStatementXmlDataTransferDirector director = new BankStatementXmlDataTransferDirector(BankStatement, fileFormat, adapter, true);
			director.Import(fileName, new NotificationBuffer(), new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.BankStatementImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, fileName));
		}

		void FildFilterButton_Click(object sender, EventArgs e)
		{
			try
			{
				BankStatement.ApplyFilter();
			}
			catch (BankStatementException e1)
			{
				Globals.Message.ShowError(e1.Message);
			}
		}

		void ClearFilterButton_Click(object sender, EventArgs e)
		{
			try
			{
				BankStatement.ClearFilter();
			}
			catch (BankStatementException e1)
			{
				Globals.Message.ShowError(e1.Message);
			}
		}

		void BankTransactionButton_Click(object sender, EventArgs e)
		{
			ShowBankTransactionForm();
		}

		void ShowBankTransactionForm()
		{
			BankTransactionFormHelper helper = new BankTransactionFormHelper(BankStatement, BankStatement.AB_LastStatementDate, BankStatement.DirectTransactions, true);
			helper.ShowBankTransactionForm();
		}

		#endregion
	}
}

