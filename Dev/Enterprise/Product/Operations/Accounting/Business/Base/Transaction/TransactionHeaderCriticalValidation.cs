using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderCriticalValidation : AccTransactionHeaderCriticalValidation
	{
		public TransactionHeaderCriticalValidation(TransactionHeader parent)
			: base(parent)
		{
		}

		public new TransactionHeader Parent
		{
			get { return (TransactionHeader)base.Parent; }
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			yield return CheckNumberFountainBaseDataIsNotChanged();

			if (!CheckIsCancelledIsNotSetWithoutMatchLinks())
			{
				yield return GetCriticalValidationResultForHeader(CriticalValidationErrorType.MissingRevesingTransactionForCanceledTransaction_2,
															CriticalValidationMessageTemplate.MissingRevesingTransactionForCanceledTransactionErrorMessage);
			}
			if (!CheckOutstandingAmountAndFullyPaidDateAreValid())
			{
				var matchLinks = AccTransactionMatchLinkLoader.LoadByAccTransactionHeader(Parent);
				yield return GetCriticalValidationResultForHeader(CriticalValidationErrorType.InvalidFullyPaidDateWithRespectToTheOutstandingAmount_2,
															CriticalValidationMessageTemplate.InvalidFullyPaidDateWithRespectToTheOutstandingAmountErrorMessage,
															matchLinks?.GetTransactionHeaderMatchLinkInfos());
			}
			if (!CheckIsLocalInvoiceAmountEqualToForeignAmountForAllExceptInvoiceBatch())
			{
				yield return GetCriticalValidationResultForHeader(CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4,
															CriticalValidationMessageTemplate.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1ErrorMessage(Parent.AH_RX_NKTransactionCurrency, Parent.AH_ExchangeRate),
															GetMoreInfoToErrorWhenLocalInvoiceAmountNotEqualToForeignAmount());
			}
			if (Parent.AH_TransactionType == TransactionTypes.Journal
				&& (Parent.AH_TransactionCategory == Constants.TransactionCategory.Codes.Clearing
				|| Parent.AH_TransactionCategory == Constants.TransactionCategory.Codes.ClearingJournal)
				&& Parent.AH_AG.IsEmpty)
			{
				yield return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionWithEmptyGLAccountField_7,
															CriticalValidationMessageTemplate.TransactionWithEmptyGLAccountField_ClearingJournalErrorMessage);
			}
			if (!CheckIsLocalInvoiceAmountEqualToForeignAmountForInvoiceBatch())
			{
				yield return GetCriticalValidationResultForHeader(CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4,
															CriticalValidationMessageTemplate.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1ErrorMessage(Parent.AH_RX_NKTransactionCurrency, Parent.AH_ExchangeRate));
			}
			if (Parent.Factory.HasContext(BusinessContext.MaximumJobInvoiceNumberError))
			{
				yield return new CriticalValidationResult(CriticalValidationErrorType.JobInvoiceNumberExceedTheMaximumNumber,
													CriticalValidationMessageTemplate.GetJobInvoiceNumberExceedMaximumNumberMessage(
														AccountingConfigurationRegistry.Instance.MaximumNumberOfInvoicesAllowedOnJob.Value));
			}

			yield return CheckMatchingBasisTaxTransactionsWithEmptyRealisationDate();

			yield return CheckIsOSTotalAmountEqualToRoundedAmount();

			yield return CheckRemittanceReferenceNumberExceedMaxLength();

			if (!Parent.Factory.HasContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation))
			{
				yield return CheckDirectDebitBatchLocalAmountIsEqualToSumOfAllPaymentLocalAmounts();

				yield return CheckDirectDebitBatchOSAmountIsEqualToSumOfAllPaymentOSAmountsWhenBatchIsInForeignCurrency();

				yield return CheckDirectDebitBatchOSAmountIsEqualToLocalAmountWhenBatchIsInLocalCurrencyAndAllPaymentsAreInLocalCurreny();
			}

			yield return CheckInvoiceBatchLinesTotalsMatchWithInvoiceBatchHeader();

			yield return CheckCancelledInvoiceBatchHeaderAmountsEqualsToZero();

			yield return CheckTransactionHeaderGeneralLedgerAccountIsNotNull();
		}

		#region OnSaving Check methods

		IEnumerable<TransactionHeader> PaymentsIncludedInTheBatch(DirectDebitBatchLineCollection lines)
		{
			return lines.Cast<IDirectDebitBatchTransaction>().Where(x => x.IncludeInTheBatch).Select(x => x as TransactionHeader);
		}

		CriticalValidationResult CheckDirectDebitBatchLocalAmountIsEqualToSumOfAllPaymentLocalAmounts()
		{
			CriticalValidationResult result = null;
			if (Parent is DirectDebitBatchHeader batchHeader && !batchHeader.IsReverseTransaction)
			{
				Func<TransactionHeader, bool> isPaymentChanged = x => !x.IsInDatabase || x.AH_InvoiceAmountInfo.HasChanges || x.AH_GSTAmountInfo.HasChanges;
				var payments = PaymentsIncludedInTheBatch(batchHeader.Lines);
				var doesAnyPaymentIncludedInTheBatchHasChanges = payments.Any(isPaymentChanged);

				if (!batchHeader.IsInDatabase || batchHeader.AH_InvoiceAmountInfo.HasChanges || doesAnyPaymentIncludedInTheBatchHasChanges)
				{
					var sumOfPaymentInvoiceAmount = payments.Sum(x => Math.Abs(x.AH_InvoiceAmount));
					var sumOfPaymentGSTAmount = payments.Sum(x => Math.Abs(x.AH_GSTAmount));
					if (Math.Abs(batchHeader.AH_InvoiceAmount) != sumOfPaymentInvoiceAmount + sumOfPaymentGSTAmount)
					{
						return GetCriticalValidationResultForHeader(CriticalValidationErrorType.DirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts_2,
											CriticalValidationMessageTemplate.GetDirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmountsErrorMessage(batchHeader.AH_InvoiceAmount, sumOfPaymentInvoiceAmount, sumOfPaymentGSTAmount),
											GetDirectDebitBatchPaymentInformation(payments));
					}
				}
			}
			return result;
		}

		CriticalValidationResult CheckDirectDebitBatchOSAmountIsEqualToSumOfAllPaymentOSAmountsWhenBatchIsInForeignCurrency()
		{
			CriticalValidationResult result = null;
			if (Parent is DirectDebitBatchHeader batchHeader && !batchHeader.IsReverseTransaction && batchHeader.AH_RX_NKTransactionCurrency != batchHeader.Company.GC_RX_NKLocalCurrency)
			{
				Func<TransactionHeader, bool> isPaymentChanged = x => !x.IsInDatabase || x.AH_OSTotalInfo.HasChanges;
				var payments = PaymentsIncludedInTheBatch(batchHeader.Lines);
				var doesAnyPaymentIncludedInTheBatchHasChanges = payments.Any(isPaymentChanged);
				if (!batchHeader.IsInDatabase || batchHeader.AH_OSTotalInfo.HasChanges || doesAnyPaymentIncludedInTheBatchHasChanges)
				{
					var sumOfPaymentOSTotalAmount = payments.Sum(x => Math.Abs(x.AH_OSTotal));
					if (Math.Abs(batchHeader.AH_OSTotal) != sumOfPaymentOSTotalAmount)
					{
						return GetCriticalValidationResultForHeader(CriticalValidationErrorType.DirectDebitBatchOSAmountIsNotEqualToSumOfAllPaymentOSAmountsWhenBatchIsInForeignCurrency,
											CriticalValidationMessageTemplate.GetDirectDebitBatchOSAmountIsNotEqualToSumOfAllPaymentOSAmountsErrorMessage(batchHeader.AH_OSTotal, sumOfPaymentOSTotalAmount),
											GetDirectDebitBatchPaymentInformation(payments));
					}
				}
			}
			return result;
		}

		CriticalValidationResult CheckDirectDebitBatchOSAmountIsEqualToLocalAmountWhenBatchIsInLocalCurrencyAndAllPaymentsAreInLocalCurreny()
		{
			CriticalValidationResult result = null;
			if (Parent is DirectDebitBatchHeader batchHeader && !batchHeader.IsReverseTransaction && batchHeader.AH_RX_NKTransactionCurrency == batchHeader.Company.GC_RX_NKLocalCurrency)
			{
				var payments = PaymentsIncludedInTheBatch(batchHeader.Lines);
				if (payments.All(x => x.AH_RX_NKTransactionCurrency == batchHeader.AH_RX_NKTransactionCurrency))
				{
					if (!batchHeader.IsInDatabase || batchHeader.AH_OSTotalInfo.HasChanges || batchHeader.AH_InvoiceAmountInfo.HasChanges)
					{
						if (Math.Abs(batchHeader.AH_OSTotal) != Math.Abs(batchHeader.AH_InvoiceAmount))
						{
							return GetCriticalValidationResultForHeader(CriticalValidationErrorType.DirectDebitBatchOSAmountIsNotEqualToLocalAmountWhenBatchIsInLocalCurrency,
												CriticalValidationMessageTemplate.GetDirectDebitBatchOSAmountIsNotEqualToLocalAmountWhenBatchIsInLocalCurrency(batchHeader.AH_OSTotal, batchHeader.AH_InvoiceAmount),
												GetDirectDebitBatchPaymentInformation(payments));
						}
					}
				}
			}
			return result;
		}

		string GetDirectDebitBatchPaymentInformation(IEnumerable<TransactionHeader> paymentsIncludedInBatch)
		{
			var paymentInfos = new ZStringBuilder();
			paymentInfos.AppendLine();
			paymentInfos.AppendLine($"There are {paymentsIncludedInBatch.Count()} Payment(s) included in this batch.");
			if (paymentsIncludedInBatch.Any())
			{
				paymentInfos.AppendLine((NoResString)"Payment Details: ");
				foreach (var payment in paymentsIncludedInBatch)
				{
					paymentInfos.AppendLine(payment.GetTransactionHeaderInfo());
				}
			}
			return paymentInfos.ToStringWithNewLineBetweenAppends();
		}

		CriticalValidationResult CheckNumberFountainBaseDataIsNotChanged()
		{
			if (!Parent.IsInDatabase && Parent.HasNumberFountain)
			{
				var (isCorrect, errorMessage) = NumberFountainTransactionDataProvider.CheckDataIsCorrect(Parent);
				if (!isCorrect)
				{
					return GetCriticalValidationResultForHeader(CriticalValidationErrorType.NumberFountainBaseDataWasChanged_2,
																CriticalValidationMessageTemplate.NumberFountainBaseDataWasChanged,
																errorMessage);
				}
			}

			return null;
		}

		bool CheckIsCancelledIsNotSetWithoutMatchLinks()
		{
			bool result = true;
			if (Parent.AH_IsCancelled && Parent is IMatching &&
				(!Parent.IsInDatabase || Parent.AH_IsCancelledInfo.HasChanges))
			{
				if ((Parent.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.AH_Ledger == LedgerTypes.AccountsPayable) &&
					(Parent.AH_TransactionType == TransactionTypes.AdjustmentNote ||  //	ADJ, CRD, CTR, DSC, EXX, INV, JNL, OVP, PAY, REC or TRF
					Parent.AH_TransactionType == TransactionTypes.CreditNote ||
					Parent.AH_TransactionType == TransactionTypes.Contra ||
					Parent.AH_TransactionType == TransactionTypes.Discount ||
					Parent.AH_TransactionType == TransactionTypes.ExchangeDifference ||
					Parent.AH_TransactionType == TransactionTypes.Invoice ||
					Parent.AH_TransactionType == TransactionTypes.Journal ||
					Parent.AH_TransactionType == TransactionTypes.Overpayment ||
					Parent.AH_TransactionType == TransactionTypes.Payment ||
					Parent.AH_TransactionType == TransactionTypes.Receipt ||
					Parent.AH_TransactionType == TransactionTypes.Transfer)
					 )
				{
					if (((IMatching)Parent).Matchlinks.Count == 0)
					{
						((IMatching)Parent).Matchlinks.Load();
					}
					result = ((IMatching)Parent).Matchlinks.Count > 0;
				}
			}

			return result;
		}

		bool CheckOutstandingAmountAndFullyPaidDateAreValid()
		{
			bool result = true;
			if ((Parent.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(Parent.AH_TransactionType == TransactionTypes.AdjustmentNote ||  //	INV, ADJ, CRD, PAY, REC
				Parent.AH_TransactionType == TransactionTypes.Invoice ||
				Parent.AH_TransactionType == TransactionTypes.CreditNote ||
				Parent.AH_TransactionType == TransactionTypes.Payment ||
				Parent.AH_TransactionType == TransactionTypes.Receipt)
				 )
			{
				if (!Parent.IsInDatabase ||
					Parent.AH_InvoiceAmountInfo.HasChanges ||
					Parent.AH_GSTAmountInfo.HasChanges ||
					Parent.AH_FullyPaidDateInfo.HasChanges ||
					Parent.AH_OutstandingAmountInfo.HasChanges)
				{
					bool outstandingAmountZeroAndFullyPaidDateNotSet = Parent.AH_OutstandingAmount == 0 && Parent.AH_FullyPaidDate.IsEmpty;
					bool outstandingAmountNonZeroAndFullyPaidDateSet = Parent.AH_OutstandingAmount != 0 && !Parent.AH_FullyPaidDate.IsEmpty;
					bool hasInvoiceAmount = Parent.AH_LocalTotal != 0;
					result = !(hasInvoiceAmount && (outstandingAmountZeroAndFullyPaidDateNotSet || outstandingAmountNonZeroAndFullyPaidDateSet));
				}
			}
			return result;
		}

		bool CheckIsLocalInvoiceAmountEqualToForeignAmountForAllExceptInvoiceBatch()
		{
			bool result = true;

			if (Parent.AH_TransactionType != TransactionTypes.InvoiceBatch)
			{
				result = CheckIsLocalInvoiceAmountEqualToForeignAmountCore();
			}
			return result;
		}

		bool CheckIsLocalInvoiceAmountEqualToForeignAmountForInvoiceBatch()
		{
			bool result = true;
			if (Parent.AH_TransactionType == TransactionTypes.InvoiceBatch)
			{
				result = CheckIsLocalInvoiceAmountEqualToForeignAmountCore();
			}
			return result;
		}

		bool CheckIsLocalInvoiceAmountEqualToForeignAmountCore()
		{
			if (Parent.AH_Ledger == LedgerTypes.CashBook &&
				(Parent.AH_TransactionType == TransactionTypes.ReceiptBatch || Parent.AH_TransactionType == TransactionTypes.DDRBatch))
			{
				return true;
			}

			if (Parent.AH_TransactionType == TransactionTypes.ExchangeDifference &&
				Parent.TransactionCategory == Constants.TransactionCategory.Codes.RealizedExchangeGainLoss &&
				Parent.AH_Ledger == LedgerTypes.CashBook &&
				Parent.BankAccount != null &&
				Parent.BankAccount.AB_RX_NKAccountCurrency != Env.CurrentCompany.LocalCurrency.Code)
			{
				return true;
			}

			bool result = true;
			if (Parent.AH_RX_NKTransactionCurrency == Parent.Company.GC_RX_NKLocalCurrency
				&& (!Parent.IsInDatabase ||
				Parent.AH_ExchangeRateInfo.HasChanges ||
				Parent.AH_InvoiceAmountInfo.HasChanges ||
				Parent.AH_GSTAmountInfo.HasChanges ||
				Parent.AH_OSTotalInfo.HasChanges))
			{
				result = Math.Abs(Parent.AH_LocalTotal) == Math.Abs(Parent.AH_OSTotal);
			}

			return result || Parent.HasContext(BusinessContext.CheckTransactionLineTotalsMatchTransactionHeaderAmountsIsSuspended);
		}

		protected virtual string GetMoreInfoToErrorWhenLocalInvoiceAmountNotEqualToForeignAmount()
		{
			var errorMessage = new ZStringBuilder();

			errorMessage.AppendLine(CriticalValidationInfoCollectorService.GetOrCreateService(Parent.Factory).GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.EvaluateTransactionHeaderWithNotEqualOneExchangeRateAndLocalCurrency));

			errorMessage.AppendLine("\r\n" + Res.GetString("274CF4A6-9FA1-414a-A607-A0074ECD2315",
				@"Transaction Header:
- Local Amount: {0}
- Foreign Currency Amount: {1}
- Currency: {2}
- Exchange Rate: {3}
- Company: {4}
- Company Local Currency: {5}",
				Parent.AH_LocalTotal,
				Parent.AH_OSTotal,
				Parent.AH_RX_NKTransactionCurrency,
				Parent.AH_ExchangeRate,
				Parent.Company?.GC_Code,
				Parent.Company?.GC_RX_NKLocalCurrency));

			return errorMessage.ToString();
		}

		CriticalValidationResult CheckMatchingBasisTaxTransactionsWithEmptyRealisationDate()
		{
			bool isAPARINVCRD = (Parent.AH_Ledger == LedgerTypes.AccountsReceivable || Parent.AH_Ledger == LedgerTypes.AccountsPayable) &&
				(Parent.AH_TransactionType == TransactionTypes.Invoice || Parent.AH_TransactionType == TransactionTypes.CreditNote);

			if (isAPARINVCRD && !Parent.AH_IsCancelled && Parent.AH_OutstandingAmount == 0 && (!Parent.IsInDatabase || Parent.AH_OutstandingAmountInfo.HasChanges))
			{
				var query = new ZQuery(AccTaxTransactionSchema.ATT_AH, Parent.PK).AddToFilter(new ZQuery(AccTaxTransactionSchema.ATT_Basis, TaxBasisList.Matching.Code)).AddToFilter(new ZQuery(AccTaxTransactionSchema.ATT_RealisationDate, ZDate.Empty));
				query.FetchOnlyFromLocalCache = !Parent.IsInDatabase;

				if (Parent.Factory.Exists(typeof(AccTaxTransaction), query))
				{
					return new CriticalValidationResult(CriticalValidationErrorType.MatchingBasisTaxTransactionsWithEmptyRealisationDate, CriticalValidationMessageTemplate.MatchingBasisTaxTransactionsWithEmptyRealisationDateErrorMessage);
				}
			}
			return null;
		}

		CriticalValidationResult CheckIsOSTotalAmountEqualToRoundedAmount()
		{
			if (InvoiceRoundingLineCreator.ShouldApplyRounding(Parent))
			{
				var roundedAmount = InvoiceRoundingLineCreator.GetRoundingAmountCachedValue(Parent);
				if (roundedAmount != 0M && roundedAmount != Parent.AH_OSTotalAmount)
				{
					return GetCriticalValidationResultForHeader(CriticalValidationErrorType.OSTotalAmountMustNotChangeAfterRoundingLineCreation,
															CriticalValidationMessageTemplate.OSTotalAmountMustNotChangeAfterRoundingLineCreationErrorMessage);
				}
			}
			return null;
		}

		CriticalValidationResult CheckRemittanceReferenceNumberExceedMaxLength()
		{
			if (Parent.HasContext(BusinessContext.RemittanceReferenceNumberExceedMaxLength))
			{
				var remittanceReferenceNumber = ZString.Empty;
				var dictionary = Parent.Factory.GetCachedValue("RemittanceReferenceNumber", () => { return new Dictionary<ZGuid, ZString>(); });
				if (dictionary.ContainsKey(Parent.PK))
				{
					remittanceReferenceNumber = dictionary[Parent.PK];
				}
				return new CriticalValidationResult(CriticalValidationErrorType.RemittanceReferenceNumberExceedMaxLength,
													CriticalValidationMessageTemplate.GetRemittanceReferenceNumberExceedMaxLengthErrorMessage(remittanceReferenceNumber));
			}
			return null;
		}

		CriticalValidationResult CheckInvoiceBatchLinesTotalsMatchWithInvoiceBatchHeader()
		{
			var invoiceBatchHeader = Parent as InvoiceBatchHeader;

			if (invoiceBatchHeader != null && !invoiceBatchHeader.IsCancelled && !Parent.IsInDatabase)
			{
				var batchLines = invoiceBatchHeader.Line;

				if (batchLines.Count == 0)
				{
					return null;
				}

				var sumOfAH_InvoiceAmount = 0m;
				var sumOfAH_LocalTotal = 0m;
				var sumOfAH_OSTotal = 0m;
				var sumOfAH_GSTAmount = 0m;

				foreach (InvoicingBase batchLine in batchLines)
				{
					if (batchLine.IncludeInTheBatch)
					{
						sumOfAH_InvoiceAmount += batchLine.AH_InvoiceAmount;
						sumOfAH_LocalTotal += batchLine.AH_LocalTotal;
						sumOfAH_OSTotal += batchLine.AH_OSTotal;
						sumOfAH_GSTAmount += batchLine.AH_GSTAmount;
					}
				}

				var errorType = CriticalValidationErrorType.NoError;
				ResourceString userFriendlyErrorMessage = null;

				if (sumOfAH_InvoiceAmount != invoiceBatchHeader.AH_InvoiceAmount)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfInvoiceBatchLineInvoiceAmountNotEqualToInvoiceBatchHeaderInvoiceAmountErrorMessage(sumOfAH_InvoiceAmount, Parent.AH_InvoiceAmount);
				}
				else if (sumOfAH_LocalTotal != invoiceBatchHeader.AH_OutstandingAmount)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfInvoiceBatchLineLocalTotalNotEqualToInvoiceBatchHeaderOutstandingAmountErrorMessage(sumOfAH_LocalTotal, Parent.AH_OutstandingAmount);
				}
				else if (sumOfAH_OSTotal != invoiceBatchHeader.AH_OSTotal)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfInvoiceBatchLineOSTotalNotEqualToInvoiceBatchHeaderOSTotalErrorMessage(sumOfAH_OSTotal, Parent.AH_OSTotal);
				}
				else if (sumOfAH_GSTAmount != invoiceBatchHeader.AH_GSTAmount)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.GetSumOfInvoiceBatchLineGSTAmountNotEqualToInvoiceBatchHeaderGSTAmountErrorMessage(sumOfAH_GSTAmount, Parent.AH_GSTAmount);
				}

				if (userFriendlyErrorMessage != null)
				{
					errorType = CriticalValidationErrorType.SumOfInvoiceBatchLineAmountNotEqualToInvoiceBatchHeaderAmount;
					return new CriticalValidationResult(errorType, userFriendlyErrorMessage);
				}
			}
			return null;
		}

		CriticalValidationResult CheckCancelledInvoiceBatchHeaderAmountsEqualsToZero()
		{
			var invoiceBatchHeader = Parent as InvoiceBatchHeader;

			if (invoiceBatchHeader != null && invoiceBatchHeader.IsCancelled)
			{
				var errorType = CriticalValidationErrorType.NoError;
				ResourceString userFriendlyErrorMessage = null;

				if (invoiceBatchHeader.AH_InvoiceAmount != 0)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.CancelledInvoiceBatchHeaderInvoiceAmountIsNonZeroErrorMessage;
				}
				else if (invoiceBatchHeader.AH_OutstandingAmount != 0)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.CancelledInvoiceBatchHeaderOutstandingAmountIsNonZeroErrorMessage;
				}
				else if (invoiceBatchHeader.AH_OSTotal != 0)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.CancelledInvoiceBatchHeaderOSTotalIsNonZeroErrorMessage;
				}
				else if (invoiceBatchHeader.AH_GSTAmount != 0)
				{
					userFriendlyErrorMessage = CriticalValidationMessageTemplate.CancelledInvoiceBatchHeaderGSTAmountIsNonZeroErrorMessage;
				}

				if (userFriendlyErrorMessage != null)
				{
					errorType = CriticalValidationErrorType.CancelledInvoiceBatchHeaderAmountIsNonZero;
					return new CriticalValidationResult(errorType, userFriendlyErrorMessage);
				}
			}
			return null;
		}

		CriticalValidationResult CheckTransactionHeaderGeneralLedgerAccountIsNotNull()
		{
			if (Parent.AH_AG.IsEmpty
				&& (Parent.AH_Ledger == LedgerTypes.AccountsPayable
					|| Parent.AH_Ledger == LedgerTypes.AccountsReceivable)
				&& (Parent.AH_TransactionType == TransactionTypes.ExchangeDifference
					|| Parent.AH_TransactionType == TransactionTypes.Overpayment
					|| Parent.AH_TransactionType == TransactionTypes.Journal
					|| Parent.AH_TransactionType == TransactionTypes.Discount))
			{
				ZString extraInfo = ZString.Empty;
				var collectorService = CriticalValidationInfoCollectorService.GetService(Parent.Factory);
				if (collectorService != null)
				{
					extraInfo = collectorService.GetInfo(Parent.PK, CriticalValidationInfoCollectorServiceKeyType.TransactionHeaderWithGLAccountThatShouldNotBeNull);
				}

				#region SuppressResourceStringsCheckRegion

				extraInfo += $"IsValidationSuspended: {Parent.IsValidationSuspended}, ";
				extraInfo += $"IsDeleted: {Parent.IsDeleted}, ";
				extraInfo += $"IsDeleting: {Parent.IsDeleting}, ";
				extraInfo += $"Last Edit User: {Parent.AH_SystemLastEditUser}";

				#endregion

				var messageResString = CriticalValidationMessageTemplate.GetTransactionWithEmptyGLAccountFieldErrorMessage(Parent.AH_Ledger, Parent.AH_TransactionType);
				return GetCriticalValidationResultForHeader(CriticalValidationErrorType.TransactionWithEmptyGLAccountField_7, messageResString, extraInfo);
			}

			return null;
		}

		#endregion
	}
}
