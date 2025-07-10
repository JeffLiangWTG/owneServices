using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Unmatching;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class InvoicingBaseReversing : PayablesAndReceivablesReversing
	{
		public InvoicingBaseReversing(IPayablesAndReceivables payablesAndReceivablesTransaction)
			: base(payablesAndReceivablesTransaction)
		{
		}

		InvoicingBase OriginalInvoicingBase => (InvoicingBase)OriginalTransaction;
		InvoicingBase ReverseInvoicingBase => (InvoicingBase)ReverseTransaction;

		protected override void DoReverseTransaction()
		{
			//cache the invoiced CAI journals before reversing the invoice, as after reversing the invoice, the match link is removed hence cannot find the CAI journal.
			var invoicedCAIJournals = new List<Journal>();
			var exxTransactions = new List<ExchangeDifference>();
			if (OriginalInvoicingBase is Invoice invoice)
			{
				var carInfoByInvoice = new CashAdvanceRequestInfoByInvoice(invoice);
				invoicedCAIJournals = carInfoByInvoice.InvoicedCashAdvanceJournals;
				exxTransactions = carInfoByInvoice.EXXTransactions;
			}

			//First undo the invoiced cash advance status and reverse overpayment CAI journals
			if (OriginalInvoicingBase is IInvoiceAssociatedToCashAdvanceRequest cahUpdater)
			{
				cahUpdater.Accept(new InvoicedCashAdvanceRequestReverser());
			}

			//it's important to reverse the invoice before reverse the invoiced CAI journal, due to Journal class doesn't support IUnmatchOnReversing,
			//so we rely on the invoice reversal logic to perform the unmatching for CAI journal, also note the invoice's unmatching logic reverses the EXX too.
			base.DoReverseTransaction();

			//Finally reverse the invoiced CAI journals
			if (invoicedCAIJournals.Any())
			{
				var cahJournalReverser = new InvoicedCashAdvanceRequestReverser();
				foreach (var cahUpdaterJournal in invoicedCAIJournals.OfType<IJournalAssociatedToCashAdvanceRequest>())
				{
					cahUpdaterJournal.Accept(cahJournalReverser);
				}
			}

			if (ReverseInvoicingBase is CreditNote creditNote)
			{
				creditNote.ReversedCashAdvanceCAIJournals = invoicedCAIJournals.Select(x => x.ReverseTransaction as Journal);
				creditNote.ReversedCashAdvanceEXXs = exxTransactions.Select(x => x.ReverseTransaction as ExchangeDifference);
			}
		}

		public override bool ShouldShowReverseConfirmationMessage()
		{
			return base.ShouldShowReverseConfirmationMessage() || HasActiveRealisedAPPaymentRetentionRecords;
		}

		public override ZString GetReverseConfirmationMessage()
		{
			var message = base.GetReverseConfirmationMessage();
			if (message.IsEmpty && HasActiveRealisedAPPaymentRetentionRecords)
			{
				message = Res.GetString("6BF968BB-CBAA-454E-8BBE-88EBB5730F57",
@"This transaction AP {0} '{1}' is linked to Realized Payments Basis Withholding Tax Journals. Do you want to proceed?
Select 'No' to cancel this reversal action should you want to review and reverse AP PBW JNL/s first.
Select 'Yes' to proceed without reviewing the related AP PBW JNL transaction/s.", OriginalInvoicingBase.AH_TransactionType, OriginalInvoicingBase.AH_TransactionNum);
			}

			return message;
		}

		bool IsAPJobConsolInvoice()
		{
			ZQuery costsQuery = new ZQuery(JobConsolCostSchema.E6_AH_APInvoice, OriginalTransaction.PK);
			costsQuery.AddToFilter(JobConsolCostSchema.E6_AH_ARInvoice, SQLComparisonOperator.NotEqual, null);
			JobConsolCost firstRelatedCost = OriginalTransaction.Factory.LoadTop1<JobConsolCost>(costsQuery);
			return firstRelatedCost != null;
		}

		protected bool IsRelatedToUnclosedClaim
		{
			get
			{
				bool result;
				TransactionHeader header = OriginalTransaction as TransactionHeader;

				if (header == null)
				{
					result = false;
				}
				else
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccQueryClaim));
					query.AddToFilter(new ZQuery(AccQueryClaimSchema.AY_QueryClaimStatus, QueryClaimStatusCodeList.Codes.QCStatus3AcceptedAndCreditNoteIssuedAndClosed), JoinCondition.Or);
					query.AddToFilter(new ZQuery(AccQueryClaimSchema.AY_QueryClaimStatus, QueryClaimStatusCodeList.Codes.QCStatus5RejectedClosed), JoinCondition.Or);
					ZDBOnlySubQuery transactionQuery = new ZDBOnlySubQuery(typeof(TransactionHeader), AccQueryClaimSchema.AY_AH);
					transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, header.AH_TransactionBelongsToGroup);
					query.AddSubQuery(transactionQuery, JoinCondition.And);

					result = header.Factory.ExistsInDatabase(AccQueryClaimSchema.Constants.TableName, query);
				}

				return result;
			}
		}

		protected string CheckInvoiceTermMLIRelatedErrorMessage()
		{
			var result = ZString.Empty;

			if (!IsMLIMatchedWithClearingJournal)
			{
				result = IsMLIWithUnmatchedClearingJournalErrorMessage;
			}
			if (IsMLIWithAnyMatchedJournal)
			{
				result = IsMLIWithAnyMatchedJournalErrorMessage;
			}
			if (IsMLIWithAnyReversedJournal)
			{
				result = OriginalTransaction.IsReversed
					? AlreadyReversedErrorMessage.ToString()
					: IsMLIWithAnyReversedJournalErrorMessage;
			}
			if (IsMLIWithAnyJournalInCollectionBatch)
			{
				result = IsMLIWithAnyJournalInCollectionBatchErrorMessage;
			}

			return result;
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			string result = base.GenerateCantReverseErrorMessage();
			if (IsAPJobConsolInvoice())
			{
				result = CantReverseProfitShareOrMasterFreightAPInvoiceErrorMessage;
			}
			if (IsTransactionAttachedToClaim)
			{
				result = CantReverseTransactionAttachedToClaimErrorMessage;
			}
			if (IsTransactionInInvoiceBatch)
			{
				result = TransactionIsInInvoiceBatchErrorMessage;
			}
			if (IsTransactionFromOtherCompany)
			{
				result = TransactionFromOtherCompanyErrorMessage;
			}
			if (HasNonCancelledAmendingTransactions)
			{
				result = HasNonCancelledAmendingTransactionsErrorMessage;
			}
			if (IsInvoiceTermMLI)
			{
				result = CheckInvoiceTermMLIRelatedErrorMessage();
			}
			if (HasGeneratedComplianceDocument)
			{
				result = HasGeneratedComplianceDocumentErrorMessage;
			}
			var cantReverseErrorMessage = GetCantReverseErrorMessage();
			if (!cantReverseErrorMessage.IsEmpty)
			{
				result = GetCantReverseErrorMessage();
			}

			return result;
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();

			ReverseRelatedJobConsolInvoice();

			DefaultReversalDatesIfNecessary();
			(ReverseTransaction as InvoicingBase)?.CalculateDueDate();
			ReverseRelatedPeriodApportionments();
			ReverseJournalsForMultipleInstallments();
		}

		protected override void DoReverseTaxTransaction()
		{
			ObjectFactory.Get<ITaxProcessor>().ProcessOnParentReversing(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(OriginalInvoicingBase),
																		TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(ReverseInvoicingBase),
																		ReverseInvoicingBase.AH_PostDate.Date);
		}

		void ReverseRelatedJobConsolInvoice()
		{
			JobConsolInvoiceReverser consolInvoiceReverser = new JobConsolInvoiceReverser((InvoicingBase)OriginalTransaction);
			if (consolInvoiceReverser.IsJobConsolInvoice())
			{
				consolInvoiceReverser.ReverseRelatedAPInvAndApportionment();
			}
		}

		void ReverseRelatedPeriodApportionments()
		{
			if (OriginalTransaction is TransactionHeader transactionHeader)
			{
				if (ReverseTransaction is InvoicingBase invoice)
				{
					var relatedJournals = transactionHeader.RelatedGLJournals;
					foreach (var journal in relatedJournals)
					{
						var journalReversing = new PeriodApportionmentGLJournalReversing(journal, invoice, ReverseTransaction.Factory, transactionHeader.IsPrePaymentTransaction);
						journalReversing.Reverse();
						invoice.RelatedApportionmentReversings.Add(journalReversing);

						var reversedJournal = journalReversing.ReverseTransaction as GLJournal;
						var aggregator = new AggregateWrapper(reversedJournal, reversedJournal);
						invoice.Factory.SaveInTransactionActions.Add(aggregator);
					}
				}
			}
		}

		void ReverseJournalsForMultipleInstallments()
		{
			if (!IsInvoiceTermMLI)
			{
				return;
			}

			var reversingFactory = new ReversingFactory();
			var mliJournals = OriginalInvoicingBase.GetMultipleInstallmentsJournals();
			foreach (var journal in mliJournals)
			{
				var reversing = reversingFactory.NewReversing(journal);
				reversing.Reverse();

				var reversedJournal = reversing.ReverseTransaction as ARJournal;
				reversedJournal.AH_Desc += " - " + journal.AH_Desc;
			}
		}

		protected override void UnmatchRelatedMatchingSessions()
		{
			base.UnmatchRelatedMatchingSessions();

			if (IsInvoiceTermMLI || IsOutstandingAmountOnlyPaidViaCashAdvance)
			{
				if (OriginalInvoicingBase.LatestMatchLink != null)
				{
					var matchGroupNum = OriginalInvoicingBase.LatestMatchLink.AP_MatchGroupNum;
					var unmatchingRow = new UnmatchingRow(OriginalInvoicingBase.Factory) { MatchGroupNum = matchGroupNum };
					unmatchingRow.UnmatchAnyGroup();
				}
			}
		}

		bool IsMLIMatchedWithClearingJournal
		{
			get
			{
				if (!IsTransactionMatched)
				{
					return false;
				}

				var clearingJournal = OriginalInvoicingBase.GetMultipleInstallmentsJournals(Constants.TransactionCategory.Codes.ClearingJournal).FirstOrDefault();
				if (clearingJournal == null)
				{
					return false;
				}

				var tempFactory = new BusinessObjectFactory();
				var matchLinksQuery = new ZQuery(AccTransactionMatchLinkSchema.AP_MatchGroupNum, OriginalInvoicingBase.LatestMatchLink.AP_MatchGroupNum);
				matchLinksQuery.AddToFilter(AccTransactionMatchLinkSchema.AP_AH, SQLComparisonOperator.NotEqual, OriginalInvoicingBase.PK);
				var matchedTransactions = tempFactory.Load<TransactionMatchLink>(matchLinksQuery);

				return matchedTransactions.Any(t => t.MatchingTransaction.PK == clearingJournal.PK);
			}
		}

		bool IsMLIWithAnyMatchedJournal
		{
			get
			{
				var mliJournals = OriginalInvoicingBase.GetMultipleInstallmentsJournals(Constants.TransactionCategory.Codes.InstalmentJournal);
				return mliJournals.Any(j => j.AH_LocalTotalAmount != j.AH_LocalOutstandingAmount);
			}
		}

		bool IsMLIWithAnyReversedJournal
		{
			get
			{
				var mliJournals = OriginalInvoicingBase.GetMultipleInstallmentsJournals();
				return mliJournals.Any(j => j.IsReversed);
			}
		}

		bool IsMLIWithAnyJournalInCollectionBatch
		{
			get
			{
				var mliJournals = OriginalInvoicingBase.GetMultipleInstallmentsJournals();
				var mliJournalsPKs = mliJournals.Select(j => j.PK);
				var query = new ZQuery(AccCollectionOrderLineSchema.AOL_AH, mliJournalsPKs);
				query.AddToFilter(AccCollectionOrderLineSchema.AOL_IsCancelled, false);
				if (OriginalTransaction.Factory.ExistsInDatabase(AccCollectionOrderLineSchema.Constants.TableName, query))
				{
					return true;
				}
				return false;
			}
		}

		bool CanMLIBeReversed => !IsInvoiceTermMLI ||
			(IsMLIMatchedWithClearingJournal
			&& !IsMLIWithAnyMatchedJournal
			&& !IsMLIWithAnyReversedJournal
			&& !IsMLIWithAnyJournalInCollectionBatch);

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed()
				&& CanMLIBeReversed
				&& !IsAPJobConsolInvoice()
				&& !IsRelatedToUnclosedClaim
				&& !IsTransactionAttachedToClaim
				&& !IsTransactionInInvoiceBatch
				&& !IsTransactionFromOtherCompany
				&& !HasNonCancelledAmendingTransactions
				&& !HasGeneratedComplianceDocument
				&& GetCantReverseErrorMessage().IsEmpty;
		}

		protected override bool CanTransactionBeUnmatched => base.CanTransactionBeUnmatched || IsOutstandingAmountOnlyPaidViaCashAdvance || IsInvoiceTermMLI;

		protected override void HookFactorySaveToSetNumberFountainAndDateFields()
		{
			if (!IsUATransaction)
			{
				base.HookFactorySaveToSetNumberFountainAndDateFields();
			}
		}

		string CantReverseProfitShareOrMasterFreightAPInvoiceErrorMessage
		{
			get
			{
				return Res.GetString("8c4eac4f-404b-4b5d-8b9d-7e47f051387a", "This transaction cannot be reversed because it contains Profit Share and/or Master Freight charges.\r\nYou will need to reverse corresponding agent invoice {0}. {1} will automatically reverse this AP invoice when the corresponding agent invoice is reversed.",
					GetCorrespondingARConsolInvoiceNumber(),
					Core.Constants.ProductName);
			}
		}

		string GetCorrespondingARConsolInvoiceNumber()
		{
			string result = "";
			if (IsAPJobConsolInvoice())
			{
				ZQuery costsQuery = new ZQuery(JobConsolCostSchema.E6_AH_APInvoice, OriginalTransaction.PK);
				costsQuery.AddToFilter(JobConsolCostSchema.E6_AH_ARInvoice, SQLComparisonOperator.NotEqual, null);
				JobConsolCost firstRelatedCost = OriginalTransaction.Factory.LoadTop1<JobConsolCost>(costsQuery);
				if (firstRelatedCost.ARInvoice != null)
				{
					result = firstRelatedCost.ARInvoice.AH_TransactionType + " " + firstRelatedCost.ARInvoice.AH_TransactionNum;
				}
			}
			return result;
		}

		protected bool IsInvoiceTermMLI
		{
			get
			{
				return OriginalPayablesAndReceivables is TransactionHeader transactionHeader
					&& transactionHeader.AH_InvoiceTerm == Constants.InvoiceTerms.MultipleInstallments;
			}
		}

		protected internal bool IsOutstandingAmountOnlyPaidViaCashAdvance => (OriginalTransaction as IInvoiceAssociatedToCashAdvanceRequest)?.IsOutstandingAmountPaidOnlyViaCashAdvance() ?? false;

		bool IsUATransaction
		{
			get
			{
				return ((ITransaction)OriginalPayablesAndReceivables).Ledger == ZArchitecture.Core.LedgerTypes.UnapprovedPayableTransactions;
			}
		}

		bool IsTransactionAttachedToClaim
		{
			get
			{
				InvoicingBase transaction = OriginalTransaction as InvoicingBase;
				return transaction != null && transaction.AH_TransactionCategory == Constants.TransactionCategory.Codes.ClaimRelated && transaction is UACreditNote;
			}
		}

		string CantReverseTransactionAttachedToClaimErrorMessage
		{
			get
			{
				return Res.GetString("9ec6e8e3-5f4b-4c39-8121-a8ba7553e998", @"This unapproved credit note is related to claim on Accounts Payable Invoices.
This transaction cannot be manually canceled. You can cancel the transaction from AP Claim screen.");
			}
		}

		bool IsTransactionInInvoiceBatch
		{
			get
			{
				InvoicingBase transaction = OriginalTransaction as InvoicingBase;
				return transaction != null && transaction.AH_AH_InvoiceStatement.IsValid;
			}
		}

		string TransactionIsInInvoiceBatchErrorMessage
		{
			get { return Res.GetString("480387be-6724-4f61-afcb-86396f9dbda6", "This transaction cannot be reversed because it is in Invoice Batch."); }
		}

		bool IsTransactionFromOtherCompany
		{
			get
			{
				InvoicingBase transaction = OriginalTransaction as InvoicingBase;
				return transaction != null && transaction.AH_GC != GlbCompany.CurrentCompany.PK;
			}
		}

		string TransactionFromOtherCompanyErrorMessage
		{
			get { return Res.GetString("E1F3608B-85E5-4568-A967-F7B6763EC1F1", "You cannot cancel invoices issued by other sister companies."); }
		}

		bool HasNonCancelledAmendingTransactions
		{
			get
			{
				var result = false;
				if (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.Value)
				{
					var transaction = OriginalTransaction as InvoicingBase;
					if (transaction != null && transaction is IAmending)
					{
						var amendingTransactions = transaction.GetRelatedAmendingTransactions();
						if (amendingTransactions.Any())
						{
							result = true;
							var numberBuilder = new ZStringBuilder();
							amendingTransactions.Cast<InvoicingBase>().ForEach(x => numberBuilder.Append(x.AH_Ledger + x.AH_TransactionType + x.AH_TransactionNum));
							nonCancelledAmendingTransactionNumbers = numberBuilder.ToStringWithNewLineBetweenAppends();
						}
					}
				}
				return result;
			}
		}

		ZString nonCancelledAmendingTransactionNumbers;

		string HasNonCancelledAmendingTransactionsErrorMessage
		{
			get
			{
				return Res.GetString("e3b47ede-2019-4c87-8080-b468c476e9f2", @"You are attempting to cancel an amended transaction.
Please cancel the following amendment documents before proceeding:
{0}", nonCancelledAmendingTransactionNumbers);
			}
		}

		string CantReverseMLINotes
		{
			get
			{
				return Res.GetString("1462EE79-2F7A-4B9C-A1DC-66FE0B86F395", @"Note: Invoices, Credit Notes and Adjustment Notes with Invoice Term INV for Multiple Installments can only be reversed if:
They are matched with their linked CLJ AR Journal;
They or any associated INJ journals are not reversed;
Any associated INJ journals are not matched;
They or any associated INJ journals are not included in an active Collection Order.");
			}
		}

		string IsMLIWithUnmatchedClearingJournalErrorMessage
		{
			get
			{
				return Res.GetString("0D9FC4C6-0CCE-4147-B8D6-AF4EC57452B8", "This transaction cannot be reversed because it is not matched with its linked CLJ type AR Journal.\r\n{0}", CantReverseMLINotes);
			}
		}

		string IsMLIWithAnyMatchedJournalErrorMessage
		{
			get
			{
				return Res.GetString("935C8CC3-5BAE-4F97-B96C-8F427E6023BA", "This transaction cannot be reversed because at least one of its linked INJ type AR Journals has been matched with other transactions.\r\n{0}", CantReverseMLINotes);
			}
		}

		string IsMLIWithAnyReversedJournalErrorMessage
		{
			get
			{
				return Res.GetString("A9381164-16DC-4D08-99E6-C4AD375E894D", "This transaction cannot be reversed because it has linked INJ type AR Journal/s which have already been reversed.\r\n{0}", CantReverseMLINotes);
			}
		}

		string IsMLIWithAnyJournalInCollectionBatchErrorMessage
		{
			get
			{
				return Res.GetString("C381EE1D-A682-48E5-A4F1-8F2B8C3DD5EA", "This transaction cannot be reversed because it has linked INJ type AR Journal/s which are included in an active collection batch order.\r\n{0}", CantReverseMLINotes);
			}
		}

		ZString GetCantReverseErrorMessage()
		{
			var message = ZString.Empty;
			var originalTransaction = OriginalTransaction as InvoicingBase;

			var additionalValidation = CountryComplianceEInvoicingHelper.GetAdditionalValidation(originalTransaction?.Company);
			if (additionalValidation != null)
			{
				message = additionalValidation.GetCantReverseErrorMessage(OriginalTransaction);
			}

			return message;
		}

		protected void DefaultReversalDatesIfNecessary()
		{
			InvoicingBase invoice = ReverseTransaction as InvoicingBase;
			if (invoice != null && (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote))
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					ARReversalDateHelper.DefaultReversalDates(invoice);
				}
				else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					APReversalDateHelper.DefaultReversalDates(invoice);
				}
			}
		}

		bool HasActiveRealisedAPPaymentRetentionRecords => ObjectFactory.Get<ITaxProcessor>().HasRealisedAPPaymentRetentionRecords(TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(OriginalInvoicingBase));

		bool HasGeneratedComplianceDocument => TransactionHeaderHelper.CheckHasGeneratedComplianceDocumentBeforeReverse(OriginalTransaction);

		ZString HasGeneratedComplianceDocumentErrorMessage => TransactionHeaderHelper.HasGeneratedComplianceDocumentErrorMessage;
	}
}
