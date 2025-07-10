using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class ARInvoiceReversing : InvoicingBaseReversing
	{
		public ARInvoiceReversing(ARInvoice payablesAndReceivablesTransaction)
			: base(payablesAndReceivablesTransaction)
		{
		}

		public ARInvoiceReversing(ARInvoice payablesAndReceivablesTransaction, JobInvoicingSecurityHelper securityHelper)
			: base(payablesAndReceivablesTransaction)
		{
			this.SecurityHelper = securityHelper;
		}

		public new InvoicingBase OriginalTransaction => (ARInvoice)base.OriginalTransaction;

		readonly JobInvoicingSecurityHelper SecurityHelper;

		protected override void DoReverseTransaction()
		{
			if (CheckForPaidRelatedInvoices())
			{
				base.DoReverseTransaction();
			}
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed()
					&& CheckForPaidRelatedInvoices()
					&& !ShouldPreventInvoiceReversing
					&& CanInvoiceReversingByCountry();
		}

		bool CanInvoiceReversingByCountry()
		{
				var canReversingFlag = EInvoicingReversingProvider?.CanReverseInvoice(OriginalTransaction);

				return canReversingFlag == null || canReversingFlag.Value;
		}

		IEInvoicingReversingProvider eInvoicingReversingProvider;
		IEInvoicingReversingProvider EInvoicingReversingProvider
		{
			get
			{
				if (eInvoicingReversingProvider == null)
				{
					eInvoicingReversingProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IEInvoicingReversingProvider>)?.Get();
				}
				return eInvoicingReversingProvider;
			}
		}

		protected override bool IsTransactionMatched => base.IsTransactionMatched && !((OriginalTransaction as IInvoiceAssociatedToCashAdvanceRequest)?.IsOutstandingAmountPaidOnlyViaCashAdvance() ?? false);

		bool ShouldPreventInvoiceReversing => AccountingMasterFilesUtils.ShouldPreventCreateReversalTransactions(LedgerTypes.AccountsReceivable, OriginalTransaction.AH_GC);

		bool CheckForPaidRelatedInvoices()
		{
			bool result = true;

			var invoice = OriginalTransaction as ARInvoice;
			if (invoice != null)
			{
				if (!OriginalTransaction.Factory.HasContext(BusinessContext.PeriodicInvoicePosting))
				{
					bool hasPaidRelatedTransactions = invoice.RelatedInvoices.ToArray<InvoicingBase>().Any(x =>
					x.AH_Ledger == LedgerTypes.AccountsPayable &&
					x.AH_TransactionType == TransactionTypes.Invoice &&
					!x.AH_IsCancelled && ((IMatching)x).IsMatched);

					if (hasPaidRelatedTransactions)
					{
						SecurityCheckpoint checkPoint = null;
						if (SecurityHelper != null)
						{
							checkPoint = SecurityHelper.GetInvSecurity(invoice.IsSelfBillingInvoice ?
								SecurityCore.AllowSelfBilledReversalWhenRelatedAPTrArePaid : SecurityCore.AllowReversalWhenRelatedAPTrArePaid);
						}

						invoice.GeneratePaidRelatedInvoicesSecurityCertificate(checkPoint);
						result = invoice.IsSelfBillingInvoice ? invoice.PaidRelatedSelfBilledInvoicesSecurityCertificate.IsAllowed : invoice.PaidRelatedInvoicesSecurityCertificate.IsAllowed;
					}
				}
			}

			return result;
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			string result = base.GenerateCantReverseErrorMessage();
			if (string.IsNullOrEmpty(result))
			{
				if (!CheckForPaidRelatedInvoices())
				{
					var checkPoint = ((ARInvoice)OriginalTransaction).IsSelfBillingInvoice ? Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid
										: Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid;
					result = checkPoint.ErrorMessageForNotAllowed;
				}
				else if (ShouldPreventInvoiceReversing)
				{
					result = AccountingMasterFilesUtils.ARInvoiceReversalDisallowedMessage;
				}
				else if (!CanInvoiceReversingByCountry())
				{
					result = EInvoicingReversingProvider?.GetPreventInvoiceReversingPrompt();
				}
			}
			return result;
		}
	}
}
