using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Actions on transactions created but not yet posted.
	/// </summary>
	/// <remarks>
	/// These are (usually) validation rules designed to handle conditions that would trigger critical validation.
	/// </remarks>
	public interface IPostManagerCreatedTransactionActions
	{
		bool PerformInvoiceExchangeRateCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformSurchargeLineDeptCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformCashAdvanceRelatedCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformExporterExemptionCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformInvoicingTermsCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformDuplicateTransactionNumberCheckForPayableTransactions(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool AreAnyInvoicesWhereOrgIsNotARorAP(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier, BasePostManager postManager);

		bool PerformInvPostDateCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformInvTaxDateCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformInvComplianceCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier);

		bool PerformARCreditNoteLevelAuthorization(BasePostManager postManager, IPostingJobTransactionsApprovalGUIProvider arCreditNoteApprovalGUIProvider);
	}

	public sealed class PostManagerCreatedTransactionActions : IPostManagerCreatedTransactionActions
	{
		public bool PerformInvoiceExchangeRateCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			Argument.NotNull(transactions, nameof(transactions));
			Argument.NotNull(notifier, nameof(notifier));

			var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes().Cast<TransactionHeader>();
			var arTransactions = transactions.GetAllARTransactions();
			var allTransactions = apTransactions.Concat(arTransactions);

			foreach (var transaction in allTransactions)
			{
				if (!transaction.IsInDatabase
					&& transaction.AH_ExchangeRate == 0m
					&& (transaction.AH_OSTotalAmount == 0m || transaction.AH_LocalTotalAmount == 0m))
				{
					var errorMessage = Res.GetString("EFF7DF13-838D-4AED-8C66-3577D3FE4BCC",
@"Invoice was created with zero exchange rate. Exchange rate must be greater than 0.
Invoice currency: {0}, debtor / creditor: {1}, ledger: {2}, post date: {3}, invoice date {4}.",
											transaction.AH_RX_NKTransactionCurrency,
											transaction.Header?.OH_Code ?? ZString.Empty,
											transaction.AH_Ledger,
											transaction.AH_PostDate.Date,
											transaction.AH_InvoiceDate.Date);
					notifier.NotifyPostValidationError(errorMessage);
					return false;
				}
			}
			return true;
		}

		public bool PerformSurchargeLineDeptCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			var arTransactions = transactions.GetAllARInvoicesAndCreditNotes().Where(x => x.AH_TransactionType == TransactionTypes.Invoice);

			foreach (var transaction in arTransactions)
			{
				foreach (InvoicingLineBase line in transaction.Lines)
				{
					if (line.HasContext(BusinessContext.SurchargeLine))
					{
						var errorMessage = AccountingUtils.GetDeptNotInChargeDeptListMessage(line);
						if (!errorMessage.IsEmpty)
						{
							notifier.NotifyPostValidationError(errorMessage);
							return false;
						}
					}
				}
			}

			return true;
		}

		public bool PerformCashAdvanceRelatedCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			if (!checker.IsPayablesCashAdvanceFunctionalityEnabled && !checker.IsReceivablesCashAdvanceFunctionalityEnabled)
			{
				return true;
			}

			var transactionWithCashAdvances = transactions.Values.OfType<IInvoiceAssociatedToCashAdvanceRequest>();
			var errorMessages = new Dictionary<string, List<string>>();

			foreach (var transaction in transactionWithCashAdvances)
			{
				transaction.Accept(new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages));
			}

			if (errorMessages.Any())
			{
				var fullErrorMessageBuilder = new ZStringBuilder();
				foreach (var errorMessage in errorMessages)
				{
					fullErrorMessageBuilder.Append(errorMessage.Key);
					fullErrorMessageBuilder.Append(string.Join(System.Environment.NewLine, errorMessage.Value));
					fullErrorMessageBuilder.Append(string.Empty);
				}
				notifier.NotifyPostValidationError(fullErrorMessageBuilder.ToStringWithNewLineBetweenAppends());
				return false;
			}

			return true;
		}

		public bool PerformExporterExemptionCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			var arTransactions = transactions.GetAllARInvoicesAndCreditNotes();

			foreach (var transaction in arTransactions)
			{
				var checkDate = transaction.AH_PostDate.IsEmpty ?
					(transaction.AH_InvoiceDate.IsEmpty ? ZDateTime.Now : transaction.AH_InvoiceDate)
					: transaction.AH_PostDate;

				var (_, errorMessage) = ExporterExemptionValidationHelper.CheckExporterExemption(transaction.Header, LedgerTypes.AccountsReceivable, checkDate, transaction.Lines);
				if (!errorMessage.IsEmpty)
				{
					notifier.NotifyPostValidationError(errorMessage);
					return false;
				}
			}

			return true;
		}

		public bool PerformInvoicingTermsCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes().Cast<TransactionHeader>();
			var arTransactions = transactions.GetAllARTransactions();
			var allTransactions = apTransactions.Concat(arTransactions).ToArray();

			foreach (var transaction in allTransactions)
			{
				transaction.Validation.ValidateAH_InvoiceTerm();
				if (transaction.AH_InvoiceTermInfo.Notifications.HasErrors())
				{
					var errorMessage = Res.GetString("f9c92636-028d-45c4-b788-77c234adc982",
						@"{0} Invoice cannot be created as the defaulted Invoice Term {1} is invalid. Please check the Invoice Terms on the debtor / creditor: {2}.",
						transaction.AH_Ledger,
						transaction.AH_InvoiceTerm,
						transaction.Header?.OH_Code ?? ZString.Empty);
					notifier.NotifyPostValidationError(errorMessage);
					return false;
				}
			}

			return true;
		}

		public bool PerformDuplicateTransactionNumberCheckForPayableTransactions(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes().Where(x => !x.IsSelfBillingInvoice);
			foreach (var transaction in apTransactions)
			{
				var previousTransactionNumberData = AccountingUtils.APTransactionNumberExists(transaction.AH_TransactionType, transaction.AH_TransactionNum, transaction.AH_OH, transaction.AH_InvoiceDate);
				if (previousTransactionNumberData.IsDuplicateTransactionNumberAllowed)
				{
					transaction.SetPreviousSameNumberTransactionDetails(previousTransactionNumberData);
				}

				if (previousTransactionNumberData.HasError)
				{
					var errorMessage = Res.GetString("7022a1ec-9cdd-4a59-bb5e-4a1751ae0e50", "AP transaction number {0} is already used for the organization: {1}. Please use another transaction number.", transaction.AH_TransactionNum, transaction.Header.OH_Code);
					notifier.NotifyPostValidationError(errorMessage);
					return false;
				}
			}
			return true;
		}

		public bool AreAnyInvoicesWhereOrgIsNotARorAP(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier, BasePostManager postManager)
		{
			bool result = false;

			List<ZString> orgsRequiredToBeAR = new List<ZString>();
			foreach (InvoicingBase invoice in postManager.Poster.PostedInvoices)
			{
				if (invoice.Header != null && !invoice.Header.OH_IsDebtor)
				{
					if (!orgsRequiredToBeAR.Contains(invoice.Header.OH_Code))
					{
						orgsRequiredToBeAR.Add(invoice.Header.OH_Code);
					}
				}
			}

			List<ZString> orgsRequiredToBeAP = new List<ZString>();
			InvoicingBase[] aPInvoicesAndCredits = transactions.GetAllAPInvoicesAndCreditNotes();
			foreach (InvoicingBase apInvoice in aPInvoicesAndCredits)
			{
				if (apInvoice.Header != null && !apInvoice.Header.OH_IsCreditor)
				{
					if (!orgsRequiredToBeAP.Contains(apInvoice.Header.OH_Code))
					{
						orgsRequiredToBeAP.Add(apInvoice.Header.OH_Code);
					}
				}
			}

			if (orgsRequiredToBeAR.Count > 0 || orgsRequiredToBeAP.Count > 0)
			{
				result = true;
				ZStringBuilder errorBuilder = new ZStringBuilder();
				errorBuilder.Append(Res.GetString("3a04f686-b8d8-4e60-9296-821feb1cae0e", "No Transactions have been posted."));
				if (orgsRequiredToBeAR.Count > 0)
				{
					errorBuilder.Append(Res.GetString("24d56971-86f8-4009-8828-ae02846fedfe", "The following organizations must be marked as 'Receivables':"));
					errorBuilder.Append("");
					foreach (ZString orgCode in orgsRequiredToBeAR)
					{
						errorBuilder.Append("\t" + orgCode);
					}
				}

				if (orgsRequiredToBeAP.Count > 0)
				{
					errorBuilder.Append("");
					errorBuilder.Append(Res.GetString("a75d762f-55e1-4b9f-88db-d25a9b88398f", "The following organizations must be marked as 'Payables':"));
					errorBuilder.Append("");
					foreach (ZString orgCode in orgsRequiredToBeAP)
					{
						errorBuilder.Append("\t" + orgCode);
					}
				}
				postManager.Poster.PostedInvoices.RemoveAndDeleteAll();
				notifier.NotifyPostValidationError(errorBuilder.ToStringWithNewLineBetweenAppends());
			}

			return result;
		}

		public bool PerformInvPostDateCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			if (AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.Value)
			{
				var apTransactions = transactions.GetAllAPInvoicesAndCreditNotes();
				foreach (var transaction in apTransactions)
				{
					if (!transaction.IsInDatabase && transaction.AH_InvoiceDate.Date > transaction.AH_PostDate)
					{
						var errorMessage = Res.GetString("386F807B-1CA1-4015-9157-A979359F43B1", @"Invoice Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date.");
						notifier.NotifyPostValidationError(errorMessage);
						return false;
					}
				}
			}
			return true;
		}

		public bool PerformInvTaxDateCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			var arTransactions = transactions.GetAllARInvoicesAndCreditNotes();

			foreach (var transaction in arTransactions)
			{
				if (transaction.Lines.Cast<InvoicingLineBase>().Any(x =>
				{
					var taxRate = x.TaxRate;
					var taxDate = x.AL_TaxDate;
					return taxRate != null && (taxDate.IsEmpty || !taxRate.DoesRateExists(taxDate));
				}))
				{
					var errorMessage = Res.GetString("D6F34AE4-464F-4D1F-87C7-5ABE953370F1", @"Posting is prevented. No valid Tax Rate found for the selected Tax Date, or Tax Date is empty.");
					notifier.NotifyPostValidationError(errorMessage);
					return false;
				}
			}
			return true;
		}

		public bool PerformInvComplianceCheck(TransactionCreatorHashtable transactions, IPostManagerUserNotifier notifier)
		{
			bool ret = true;
			ret = VerifyTransactionsForCompliance(transactions.GetAllAPInvoicesAndCreditNotes().Where(x => x.ShouldCheckForCompliance).ToArray(), notifier);
			ret = ret && VerifyTransactionsForCompliance(transactions.GetAllARInvoicesAndCreditNotes().Where(x => x.ShouldCheckForCompliance).ToArray(), notifier);
			return ret;
		}

		public bool PerformARCreditNoteLevelAuthorization(BasePostManager postManager, IPostingJobTransactionsApprovalGUIProvider arCreditNoteApprovalGUIProvider)
		{
			return postManager.PerformARCreditNoteLevelAuthorization(arCreditNoteApprovalGUIProvider, false);
		}

		bool VerifyTransactionsForCompliance(InvoicingBase[] aTransactions, IPostManagerUserNotifier notifier)
		{
			foreach (var transaction in aTransactions.Where(t => t.ShouldAllocateComplianceNumberOnPosting()))
			{
				var errMsg = transaction.AssignComplianceSubTypeAndCheckComplianceErrors();
				if (!errMsg.IsEmpty)
				{
					notifier.NotifyPostValidationError(errMsg);
					return false;
				}
			}
			return true;
		}
	}
}
