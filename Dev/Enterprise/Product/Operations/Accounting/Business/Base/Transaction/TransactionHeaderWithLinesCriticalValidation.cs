#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderWithLinesCriticalValidation : TransactionHeaderCriticalValidation
	{
		public TransactionHeaderWithLinesCriticalValidation(TransactionHeaderWithLines parent)
			: base(parent)
		{
		}

		public new TransactionHeaderWithLines Parent
		{
			get { return (TransactionHeaderWithLines)base.Parent; }
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			yield return CheckTransactionHeaderBranchAndTransactionLineBranchBelongToTransactionHeaderCompany();
			yield return CheckTransactionLineTotalsMatchTransactionHeaderAmounts();
			yield return CheckSumOfLinesEqualZero();
		}

		CriticalValidationResult CheckTransactionHeaderBranchAndTransactionLineBranchBelongToTransactionHeaderCompany()
		{
			if (!Parent.IsInDatabase || Parent.AH_GBInfo.HasChanges || Parent.AH_GCInfo.HasChanges)
			{
				if (Parent.Branch != null)
				{
					if (Parent.Branch.GB_GC == Parent.AH_GC)
					{
						foreach (DependentTransactionLine line in Parent.Lines)
						{
							if (line.Branch != null && line.Branch.GB_GC != Parent.AH_GC)
							{
								return new CriticalValidationResult(
									CriticalValidationErrorType.TransactionLineBranchDoesNotBelongToTransactionHeaderCompany_2,
									CriticalValidationMessageTemplate.TransactionLineBranchDoesNotBelongToTransactionHeaderCompanyErrorMessage,
									Parent.GetTransactionHeaderWithLinesInfo());
							}
						}
					}
					else
					{
						return new CriticalValidationResult(
							CriticalValidationErrorType.TransactionHeaderBranchDoesNotBelongToTransactionHeaderCompany_2,
							CriticalValidationMessageTemplate.TransactionHeaderBranchDoesNotBelongToTransactionHeaderCompanyErrorMessage,
							Parent.GetTransactionHeaderWithLinesInfo());
					}
				}
			}

			return null;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "It will be more complex if this code will be in separate methods.")]
		CriticalValidationResult CheckTransactionLineTotalsMatchTransactionHeaderAmounts()
		{
			var ledgerTypesForARAP = new[] { LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable };
			var transactionTypesToCheckForARAP = new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote };
			var transactionTypesToCheckForCashBook = new[] { TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt };

			var isTransactionTypeToCheck = (ledgerTypesForARAP.Contains((string)Parent.AH_Ledger) && transactionTypesToCheckForARAP.Contains((string)Parent.AH_TransactionType)
				|| Parent.AH_Ledger == LedgerTypes.CashBook && transactionTypesToCheckForCashBook.Contains((string)Parent.AH_TransactionType));

			Func<TransactionLine, bool> isLineChanged = x => !x.IsInDatabase || x.AL_OSAmountInfo.HasChanges || x.AL_LineAmountInfo.HasChanges || x.AL_GSTVATInfo.HasChanges;

			if (isTransactionTypeToCheck
				&& (
					!Parent.IsInDatabase
					|| Parent.AH_OSTotalInfo.HasChanges
					|| Parent.AH_InvoiceAmountInfo.HasChanges
					|| Parent.AH_GSTAmountInfo.HasChanges
					|| Parent.Lines.Cast<TransactionLine>().Any(isLineChanged)))
			{
				var sumOfOSAmount = 0M;
				var sumOfLineAmount = 0M;
				var sumOfGSTVAT = 0M;
				var isTransactionCurrencyDifferentInLineAndHeader = false;
				var isNotSavedByFactoryChangedLinesCount = 0;
				var isNotSavedByFactoryNewLinesCount = 0;

				PAToAPTransactionLineMonitor.GetInstance(Parent)?.RecordLineCount("CheckTransactionLineTotalsMatchTransactionHeaderAmounts", Parent.Lines.Count);

				foreach (DependentTransactionLine line in Parent.Lines)
				{
					if (!isTransactionCurrencyDifferentInLineAndHeader && Parent.AH_RX_NKTransactionCurrency != line.AL_RX_NKTransactionCurrency)
					{
						isTransactionCurrencyDifferentInLineAndHeader = true;
					}
					sumOfOSAmount += line.AL_OSAmount;
					sumOfLineAmount += line.AL_LineAmount;
					sumOfGSTVAT += line.AL_GSTVAT;
					if (isLineChanged(line) && !line.IsSavedByFactory)
					{
						if (!line.IsInDatabase)
						{
							isNotSavedByFactoryNewLinesCount++;
						}
						else
						{
							isNotSavedByFactoryChangedLinesCount++;
						}
					}
				}

				var errorType = CriticalValidationErrorType.NoError;
				ResourceString userFriendlyErrorMessage = null;
				var additionalErrorMessage = string.Empty;

				if (isNotSavedByFactoryNewLinesCount > 0 || isNotSavedByFactoryChangedLinesCount > 0)
				{
					errorType = CriticalValidationErrorType.SumOfTransactionLineAmountsWillNotMatchTransactionHeaderInvoiceAmountBecauseNotAllLinesWillBeSaved_2;
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfTransactionLineAmountsWillNotMatchTransactionHeaderInvoiceAmountBecauseNotAllLinesWillBeSaved(Parent.Lines.Count, isNotSavedByFactoryChangedLinesCount, isNotSavedByFactoryNewLinesCount);
				}
				else if (sumOfLineAmount != Parent.AH_InvoiceAmount)
				{
					additionalErrorMessage = PAToAPTransactionLineMonitor.GetInstance(Parent)?.GetInfo();
					errorType = CriticalValidationErrorType.SumOfTransactionLineAmountsDoesNotMatchTransactionHeaderInvoiceAmount_3;
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfTransactionLineAmountsDoesNotMatchTransactionHeaderInvoiceAmountErrorMessage(sumOfLineAmount, Parent.AH_InvoiceAmount);
				}
				else if (sumOfGSTVAT != Parent.AH_GSTAmount)
				{
					errorType = CriticalValidationErrorType.SumOfTransactionLineGSTAmountsDoesNotMatchTransactionHeaderGSTAmount_2;
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfTransactionLineGSTAmountsDoesNotMatchTransactionHeaderGSTAmountErrorMessage(sumOfGSTVAT, Parent.AH_GSTAmount);
				}
				else if (!isTransactionCurrencyDifferentInLineAndHeader && sumOfOSAmount + Parent.AH_OSTaxAmountOtherTaxes != Parent.AH_OSTotal)
				{
					additionalErrorMessage = FormattableString.Invariant($"Calculate Tax at Header Level : {AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value}");
					errorType = CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3;
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmountErrorMessage(sumOfOSAmount, Parent.AH_OSTotal, Parent.AH_OSTaxAmountOtherTaxes);
				}

				if (errorType != CriticalValidationErrorType.NoError
					&& !Parent.HasContext(BusinessContext.CheckTransactionLineTotalsMatchTransactionHeaderAmountsIsSuspended))
				{
					var shouldAddStackTraceInfoForOSAmountIncorrect = errorType == CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3;
					return new CriticalValidationResult(errorType, userFriendlyErrorMessage, additionalErrorMessage, Parent.GetTransactionHeaderWithLinesInfo(shouldAddStackTraceInfoForOSAmountIncorrect));
				}
			}

			return null;
		}

		CriticalValidationResult CheckSumOfLinesEqualZero()
		{
#if DEBUG
			if (Globals.IsTest && Enterprise.MasterFiles.Business.Testing.SuspendSumOfLinesEqualZeroCriticalValidationAttribute.IsActive)
			{
				return null;
			}
#endif

			var headersToCheck = new List<(string, string)>
			{
				(TransactionTypes.GLStandardJournal,	LedgerTypes.General),
				(TransactionTypes.GLAutoJournal,		LedgerTypes.General),
				(TransactionTypes.GLReversingJournal,	LedgerTypes.General),
				(TransactionTypes.JobRevenueJournal,	LedgerTypes.JobCosting),
			};

			if (headersToCheck.Contains((Parent.AH_TransactionType, Parent.AH_Ledger)))
			{
				if (!Parent.IsInDatabase || Parent.AH_InvoiceAmountInfo.HasChanges || Parent.Lines.Cast<TransactionLine>().Any(x => !x.IsInDatabase || x.AL_LineAmountInfo.HasChanges))
				{
					var sumOfLines = Parent.GetSumOfLines(AccTransactionLinesSchema.AL_LineAmount.Name);

					if (sumOfLines != 0m)
					{
						return new CriticalValidationResult(
							CriticalValidationErrorType.SumOfTransactionLineAmountsDoesNotEqualToZero_6,
							CriticalValidationMessageTemplate.GetSumOfTransactionLineAmountsDoesNotEqualToZeroErrorMessage(Parent.AH_Ledger, Parent.AH_TransactionType, sumOfLines),
							Parent.GetTransactionHeaderWithLinesInfo());
					}
				}
			}

			return null;
		}
	}
}

