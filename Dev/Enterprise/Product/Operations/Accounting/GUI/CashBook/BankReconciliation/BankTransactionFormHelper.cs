using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation
{
	class BankTransactionFormHelper
	{
		public BankTransactionFormHelper(AccBankAccount bankAccount, ZDateTime statementDate, DirectTransactionsBusinessObject transactions, bool isReadOnly)
		{
			this.Factory = transactions.Factory;
			this.BankAccount = bankAccount;
			this.StatementDate = statementDate;
			this.Transactions = transactions;
			this.IsReadOnly = isReadOnly;
		}

		internal DialogResult ShowBankTransactionForm()
		{
			DialogResult result = DialogResult.Cancel;

			AccountingPeriodCalculator calc = new AccountingPeriodCalculator(Factory);
			if (!calc.IsPeriodSubLedgerClosed(calc.GetPeriodFromDate(StatementDate)))
			{
				DirectTransactionsBusinessObject tempAdditionalTransactions = GetTempAdditionalTransactions();

				BankTransactionForm form = new BankTransactionForm(tempAdditionalTransactions);
#if DEBUG
				if (Globals.IsTest)
				{
					form.Load += new EventHandler(Form_Load);
				}
#endif

				if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
				{
#if DEBUG
					if (Globals.IsTest && BankTransactionFormShown != null)
					{
						BankTransactionFormShown(this, new EventArgs());
					}
#endif
					CommitChanges(tempAdditionalTransactions);
					result = DialogResult.OK;
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("282861af-f495-429a-b98b-543fd81e62d4", "Statement Date's Period is closed - cannot create transactions for this date"));
			}

			return result;
		}

#if DEBUG
		void Form_Load(object sender, EventArgs e)
		{
			if (BankTransactionFormLoad != null)
			{
				BankTransactionFormLoad(sender, e);
			}
		}

		internal event EventHandler BankTransactionFormLoad;
		internal event EventHandler BankTransactionFormShown;
#endif

		DirectTransactionsBusinessObject GetTempAdditionalTransactions()
		{
			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			DirectTransactionsBusinessObject result = new DirectTransactionsBusinessObject(tempFactory, BankAccount != null ? BankAccount.PK : ZGuid.Empty, StatementDate);
			foreach (DirectTransactionHeaderBase header in Transactions.Headers)
			{
				DirectTransactionHeaderBase importedHeader = (DirectTransactionHeaderBase)tempFactory.ImportFromAnotherFactory(header);
				if (!header.IsInDatabase)
				{
					foreach (DirectTransactionLineBase line in header.Lines)
					{
						DirectTransactionLineBase importedLine = (DirectTransactionLineBase)tempFactory.ImportFromAnotherFactory(line);
						SubAccountHelper.CopySubAccounts(importedLine, line, true);
						importedLine.OnLoaded();
						importedHeader.Lines.Add(importedLine);
					}
				}
				importedHeader.OnLoaded();
				SetReadonly(importedHeader, header.RelatedStatementPK);

				BankReconDirectReceipt bankReconDirectReceipt = header as BankReconDirectReceipt;
				if (bankReconDirectReceipt != null)
				{
					Business.CashBook.DepositBatch.DepositBatch importedBatch = (Business.CashBook.DepositBatch.DepositBatch)tempFactory.ImportFromAnotherFactory(bankReconDirectReceipt.RelatedDepositBatch);
					importedBatch.OnLoaded();
					((BankReconDirectReceipt)importedHeader).SetDepositBatch(importedBatch);
					result.DirectReceipts.Add(importedHeader);
				}
				else
				{
					result.DirectPayments.Add(importedHeader);
				}
			}

			return result;
		}

		void SetReadonly(DirectTransactionHeaderBase header, ZGuid relatedStatementPK)
		{
			if (IsReadOnly && relatedStatementPK.IsValid)
			{
				header.SetReadOnlyIncludingChildren(true);
				header.ReadOnly = false;
				header.SetReadOnlyForBankStatement(true);
			}
			else
			{
				header.AH_RX_NKTransactionCurrency_ReadOnly = true;
				header.CheckCurrencyOnEqualToCurrectCompany = true;
			}
		}

		void CommitChanges(DirectTransactionsBusinessObject tempAdditionalTransactions)
		{
			List<DirectTransactionHeaderBase> headersToDelete = new List<DirectTransactionHeaderBase>();
			foreach (DirectTransactionHeaderBase header in Transactions.Headers)
			{
				if (!tempAdditionalTransactions.Headers.Contains(header))
				{
					headersToDelete.Add(header);
				}
			}

			foreach (DirectTransactionHeaderBase headerToDelete in headersToDelete)
			{
				BankReconDirectReceipt headerToDeleteAsBankReconDirectReceipt = headerToDelete as BankReconDirectReceipt;

				if (headerToDeleteAsBankReconDirectReceipt != null)
				{
					headerToDeleteAsBankReconDirectReceipt.RelatedDepositBatch.Delete();
				}

				BankReconDirectPayment headerToDeleteAsBankReconDirectPayment = headerToDelete as BankReconDirectPayment;

				if (headerToDeleteAsBankReconDirectPayment != null)
				{
					headerToDeleteAsBankReconDirectPayment.DeleteDirectDebitBatch();
				}

				Transactions.Headers.RemoveAndDelete(headerToDelete);
			}

			foreach (DirectTransactionHeaderBase tempHeader in tempAdditionalTransactions.Headers)
			{
				if (!Transactions.Headers.Contains(tempHeader.PK))
				{
					DirectTransactionHeaderBase importedHeader = (DirectTransactionHeaderBase)Factory.ImportFromAnotherFactory(tempHeader);
					CommitLineAddsAndEdits(importedHeader.Lines, tempHeader.Lines);
					importedHeader.OnLoaded();
					Transactions.Headers.Add(importedHeader);

					ZGuid pkToLoad = importedHeader.PK;

					BankReconDirectReceipt bankReconDirectReceipt = tempHeader as BankReconDirectReceipt;
					if (bankReconDirectReceipt != null)
					{
						Business.CashBook.DepositBatch.DepositBatch importedBatch = (Business.CashBook.DepositBatch.DepositBatch)Factory.ImportFromAnotherFactory(bankReconDirectReceipt.RelatedDepositBatch);
						importedBatch.RelatedTransactionPK = importedHeader.PK;
						importedBatch.OnLoaded();
						((BankReconDirectReceipt)importedHeader).SetDepositBatch(importedBatch);
						pkToLoad = bankReconDirectReceipt.RelatedDepositBatch.PK;
					}

					BankReconDirectPayment bankReconDirectPayment = tempHeader as BankReconDirectPayment;
					if (bankReconDirectPayment != null && bankReconDirectPayment.AH_ReceiptType == ReceiptTypes.DirectDebit)
					{
						((BankReconDirectPayment)importedHeader).MakeDirectDebitBatch();

						if (BankAccount != null && !BankAccount.AB_ShowDetailsOnDirectDebits)
						{
							pkToLoad = ((BankReconDirectPayment)importedHeader).RelatedDirectDebitBatch.PK;
						}
					}

					BankReconTransaction bankReconTransaction = Factory.Load<BankReconTransaction>(pkToLoad);
					bankReconTransaction.OnLoaded();
					bankReconTransaction.IsCleared = true;
					Transactions.BankReconTransactions.Add(bankReconTransaction);
				}
				else
				{
					DirectTransactionHeaderBase header = (DirectTransactionHeaderBase)Transactions.Headers.FindByPK(tempHeader.PK);
					header.AH_InvoiceDate = tempHeader.AH_InvoiceDate;
					header.AH_PostDate = tempHeader.AH_PostDate;
					header.AH_ReceiptType = tempHeader.AH_ReceiptType;
					header.AH_ChequeOrReference = tempHeader.AH_ChequeOrReference;
					header.AH_RX_NKTransactionCurrency = tempHeader.AH_RX_NKTransactionCurrency;
					header.AH_ExchangeRate = tempHeader.AH_ExchangeRate;
					header.AH_ChequeDrawer = tempHeader.AH_ChequeDrawer;
					header.AH_DrawerBank = tempHeader.AH_DrawerBank;
					header.AH_DrawerBranch = tempHeader.AH_DrawerBranch;
					header.AH_GB_TaxBranch = tempHeader.AH_GB_TaxBranch;

					List<DirectTransactionLineBase> linesToDelete = new List<DirectTransactionLineBase>();
					foreach (DirectTransactionLineBase line in header.Lines)
					{
						if (!tempHeader.Lines.Contains(line))
						{
							linesToDelete.Add(line);
						}
					}
					foreach (DirectTransactionLineBase lineToDelete in linesToDelete)
					{
						lineToDelete.Delete();
					}

					CommitLineAddsAndEdits(header.Lines, tempHeader.Lines);
					UpdateRelatedBatchAmount(header);

					LoadBankReconTransaction(header);
				}
			}

			foreach (var batchPK in tempAdditionalTransactions.DirectReceiptBatchPKs)
			{
				Transactions.AddToBatchToDirectReceiptPKMapping(batchPK, tempAdditionalTransactions.GetDirectReceiptForBatch(batchPK));
			}
		}

		public void UpdateRelatedBatchAmount(DirectTransactionHeaderBase header)
		{
			if (header is BankReconDirectPayment bankReconDirectPayment)
			{
				bankReconDirectPayment.UpdateRelatedDirectDebitBatchAmount();
			}
		}

		void LoadBankReconTransaction(DirectTransactionHeaderBase header)
		{
			var pkToLoad = header.PK;
			if (header is BankReconDirectReceipt bankReconDirectReceipt)
			{
				pkToLoad = bankReconDirectReceipt.RelatedDepositBatch.PK;
			}
			else if (header is BankReconDirectPayment bankReconDirectPayment && bankReconDirectPayment.RelatedDirectDebitBatch != null &&
					BankAccount != null && !BankAccount.AB_ShowDetailsOnDirectDebits)
			{
				pkToLoad = bankReconDirectPayment.RelatedDirectDebitBatch.PK;
			}

			if (!pkToLoad.IsEmpty)
			{
				var bankReconTransaction = Factory.Load<BankReconTransaction>(pkToLoad);
				bankReconTransaction.OnLoaded();
			}
		}

		void CommitLineAddsAndEdits(DependentTransactionLineCollection lines, DependentTransactionLineCollection tempLines)
		{
			foreach (DirectTransactionLineBase tempLine in tempLines)
			{
				if (!lines.Contains(tempLine.PK))
				{
					DirectTransactionLineBase importedLine = (DirectTransactionLineBase)Factory.ImportFromAnotherFactory(tempLine);
					SubAccountHelper.CopySubAccounts(importedLine, tempLine, true);
					importedLine.OnLoaded();
					lines.Add(importedLine);
				}
				else
				{
					DirectTransactionLineBase line = (DirectTransactionLineBase)lines.FindByPK(tempLine.PK);
					line.AL_AG = tempLine.AL_AG;
					line.AL_GB = tempLine.AL_GB;
					line.AL_GE = tempLine.AL_GE;
					line.AL_Desc = tempLine.AL_Desc;
					line.AL_OSExTaxAmount = tempLine.AL_OSExTaxAmount;
					line.AL_AT = tempLine.AL_AT;
					line.SetTaxDateSafe(tempLine.AL_TaxDate);
					line.AL_A9_VATClass = tempLine.AL_A9_VATClass;
					line.AL_OSTaxAmount = tempLine.AL_OSTaxAmount;
					line.AL_GovtChargeCode = tempLine.AL_GovtChargeCode;
					line.AL_PlaceOfSupply = tempLine.AL_PlaceOfSupply;
					line.AL_Calc_InputGSTVATRecoverablePercentage = tempLine.AL_Calc_InputGSTVATRecoverablePercentage;
					line.AL_GB_TaxBranch = tempLine.AL_GB_TaxBranch;
					SubAccountHelper.CopySubAccounts(line, tempLine, true);
				}
			}
		}

		readonly BusinessObjectFactory Factory;
		readonly AccBankAccount BankAccount;
		readonly ZDateTime StatementDate;
		readonly DirectTransactionsBusinessObject Transactions;
		readonly bool IsReadOnly;

#if DEBUG
		internal void CommitChanges_ForTestOnly(DirectTransactionsBusinessObject tempAdditionalTransactions) => CommitChanges(tempAdditionalTransactions);
#endif
	}
}
