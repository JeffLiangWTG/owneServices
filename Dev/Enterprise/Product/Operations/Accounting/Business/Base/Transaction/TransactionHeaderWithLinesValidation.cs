using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class TransactionHeaderWithLinesValidation : TransactionHeaderValidation
	{
		public TransactionHeaderWithLinesValidation(TransactionHeader parent)
			: base(parent)
		{
		}

		new TransactionHeaderWithLines Parent
		{
			get { return (TransactionHeaderWithLines)base.Parent; }
		}

		protected override void CheckAH_GB_TaxBranch()
		{
			base.CheckAH_GB_TaxBranch();

			if (Parent.CanApplyTaxBranch)
			{
				MandatoryValidation.CheckEntered(Parent.AH_GB_TaxBranchInfo);
			}
		}

		protected override sealed void CheckAH_OSTotalAmount()
		{
			if (!Parent.ValidateAH_OSTotalAmountSuspender.IsSuspended)
			{
#if DEBUG
				Parent.ValidateAH_OSTotalAmountCallCount_ForTestOnly++;
#endif
				base.CheckAH_OSTotalAmount();
				CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines();

				var ledgerTypesForARAP = new List<string>();
				ledgerTypesForARAP.Add(LedgerTypes.AccountsPayable);
				ledgerTypesForARAP.Add(LedgerTypes.AccountsReceivable);

				var transactionTypesToCheckForARAP = new List<string>();
				transactionTypesToCheckForARAP.Add(TransactionTypes.Invoice);
				transactionTypesToCheckForARAP.Add(TransactionTypes.CreditNote);
				transactionTypesToCheckForARAP.Add(TransactionTypes.AdjustmentNote);

				var transactionTypesToCheckForCashBook = new List<string>();
				transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectPayment);
				transactionTypesToCheckForCashBook.Add(TransactionTypes.DirectReceipt);

				var isTransactionTypeToCheck = ((ledgerTypesForARAP.Contains(Parent.AH_Ledger) && transactionTypesToCheckForARAP.Contains(Parent.AH_TransactionType)) ||
				(Parent.AH_Ledger == LedgerTypes.CashBook && transactionTypesToCheckForCashBook.Contains(Parent.AH_TransactionType)));

				if (isTransactionTypeToCheck && Parent.Lines != null && Parent.Lines.Any()
					&& IsLinesUseSameCurrencyAsHeader() && !IsTransactionLinesAddUpToTotal())
				{
					var message = Res.GetString("B3F675DD-0290-40A2-930E-A54386D0723C", "Transaction Line OS Amount Total does not match the Transaction Header OS Amount.");
					if (IsInvoiceTotalEditable)
					{
						Parent.AH_OSTotalAmountInfo.AddWarning(message);
					}
					else
					{
						Parent.AH_OSTotalAmountInfo.AddError(message);
					}
				}

				if (IsInvoiceTotalEditable)
				{
					TypeValidation.CheckValidDecimal(Parent.AH_OSTotalAmountInfo, 19, 4);
				}
			}
		}

		protected virtual void CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines()
		{
		}

		protected bool IsLinesUseSameCurrencyAsHeader()
		{
			bool result = true;

			foreach (DependentTransactionLine line in Parent.Lines)
			{
				if (line.AL_RX_NKTransactionCurrency != Parent.AH_RX_NKTransactionCurrency)
				{
					result = false;
					break;
				}
			}

			return result;
		}

		protected bool IsTransactionLinesAddUpToTotal()
		{
			bool result = false;

			ZDecimal total = 0M;

			foreach (DependentTransactionLine line in Parent.Lines)
			{
				total += line.AL_OSAmount;
			}

			var invoiceTotal = IsInvoiceTotalEditable ? Parent.AH_OSTotalAmount : (ZDecimal)(Parent.AH_OSExTaxAmount + Parent.AH_OSTaxAmount);

			if (ZDecimal.Equals(Math.Abs(total), Math.Abs(invoiceTotal)))
			{
				result = true;
			}

			return result;
		}

		bool IsInvoiceTotalEditable => Parent.IsSourceReferenceUsed;
	}
}
