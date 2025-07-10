using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class InvoicingPostManager : BasePostManager
	{
		public InvoicingPostManager(Job job, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null)
			: this(job, null, null, apInvoicePostGUIProvider)
		{
		}

		public InvoicingPostManager(Job job, Predicate<Charge> isEligibleForAPPosting, Predicate<Charge> isEligibleForARPosting, IPostingJobTransactionsApprovalGUIProvider apInvoicePostGUIProvider = null)
			: base(job, apInvoicePostGUIProvider)
		{
			this.isEligibleForAPPosting = isEligibleForAPPosting;
			this.isEligibleForARPosting = isEligibleForARPosting;
		}

		readonly Predicate<Charge> isEligibleForAPPosting;
		readonly Predicate<Charge> isEligibleForARPosting;

		#region Create Agent Only Invoices

		protected override bool CreateAgentOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			return PostReceivablesCharges(JobInvoicingPostingOption.Agent, transactions);
		}

		#endregion

		#region Create Local Client Only Invoices

		protected override bool CreateLocalClientOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			return PostReceivablesCharges(JobInvoicingPostingOption.LocalClient, transactions);
		}

		#endregion

		#region Create Revenue Only Invoices

		protected override bool CreateRevenueOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			return PostReceivablesCharges(JobInvoicingPostingOption.Revenue, transactions);
		}

		#endregion

		#region Create Disbursement Only Invoices

		protected override bool CreateDisbursementOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			return PostReceivablesCharges(JobInvoicingPostingOption.Disbursement, transactions);
		}

		#endregion

		#region Create Cost Only Invoices

		protected override bool CreateCostsOnlyTransactions(TransactionCreatorHashtable transactions)
		{
			bool result = false;

			result |= CreateCostTransactions(transactions);
			result |= APPaymentApprovalCreator.CreateTransactions(transactions);
			result |= PaymentApprovalMatcher.CreateTransactions(transactions);
			result |= APCreditNoteCreator.CreateTransactions(transactions);

			return result;
		}

		#endregion

		#region Create Selected Charge Only Invoice

		protected override bool CreateSelectedAPInvoiceOnly(TransactionCreatorHashtable transactions)
		{
			bool result = false;

			result |= CreateCostTransactions(transactions);
			result |= APCreditNoteCreator.CreateTransactions(transactions);

			return result;
		}

		#endregion

		#region Create All Invoices

		protected override bool CreateAllTransactions(TransactionCreatorHashtable transactions)
		{
			bool result = false;

			result |= CreateCostTransactions(transactions);
			result |= APPaymentApprovalCreator.CreateTransactions(transactions);
			result |= PaymentApprovalMatcher.CreateTransactions(transactions);
			result |= APCreditNoteCreator.CreateTransactions(transactions);
			result |= PostReceivablesCharges(JobInvoicingPostingOption.All, transactions);

			return result;
		}

		#endregion

		#region Implementation

		#region Transaction Creators

		#region Cost Invoice Creation

		APInvoiceCreator fCostTransactionCreator;
		protected bool CreateCostTransactions(TransactionCreatorHashtable transactions)
		{
			if (fCostTransactionCreator == null)
			{
				fCostTransactionCreator = new APInvoiceCreator(Job, isEligibleForAPPosting, apInvoicePostGUIProvider);
				removeAPCreditNoteBankDetails(fCostTransactionCreator);
			}
			return fCostTransactionCreator.CreateTransactions(transactions);
		}

		void removeAPCreditNoteBankDetails(APInvoiceCreator costTransactionCreator)
		{
			var invoiceTotal = new Dictionary<string, decimal>();
			foreach (Charge charge in costTransactionCreator.Charges)
			{
				if (!invoiceTotal.ContainsKey(charge.JR_APInvoiceNum))
				{
					invoiceTotal.Add(charge.JR_APInvoiceNum, charge.JR_OSCostAmt);
				}
				else
				{
					invoiceTotal[charge.JR_APInvoiceNum] += charge.JR_OSCostAmt;
				}
			}
			foreach (Charge charge in costTransactionCreator.Charges)
			{
				if (invoiceTotal[charge.JR_APInvoiceNum] < 0)
				{
					charge.JR_PaymentType = "";
					charge.JR_ChequeNo = "";
					charge.JR_AB = ZGuid.Empty;
					charge.JR_AK = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region AP Payment Creator

		protected APPaymentApprovalCreator fAPPaymentApprovalCreator;
		protected APPaymentApprovalCreator APPaymentApprovalCreator
		{
			get
			{
				if (fAPPaymentApprovalCreator == null)
				{
					fAPPaymentApprovalCreator = new APPaymentApprovalCreator(Job);
				}

				return fAPPaymentApprovalCreator;
			}
		}

		#endregion

		#region Credit Note Creator

		APCreditNoteCreator fAPCreditNoteCreator;
		protected APCreditNoteCreator APCreditNoteCreator
		{
			get
			{
				if (fAPCreditNoteCreator == null)
				{
					fAPCreditNoteCreator = new APCreditNoteCreator(Job);
				}
				return fAPCreditNoteCreator;
			}
		}

		#endregion

		#region Job Invoicing Payment Matcher

		protected JobInvoicingPaymentApprovalMatcher fPaymentApprovalMatcher;
		protected JobInvoicingPaymentApprovalMatcher PaymentApprovalMatcher
		{
			get
			{
				if (fPaymentApprovalMatcher == null)
				{
					fPaymentApprovalMatcher = new JobInvoicingPaymentApprovalMatcher(Job, PostingTime);
				}

				return fPaymentApprovalMatcher;
			}
		}

		#endregion

		#endregion

		protected override void ProcessEligibleCharges(IReceivablesPostingChargeCollection filteredCharges)
		{
			base.ProcessEligibleCharges(filteredCharges);
			filteredCharges.Job = Job;
		}

		protected override PostingChargeEligibilityDecider GetEligibilityDecider(Charge[] charges)
		{
			return new PostingChargeEligibilityDecider(charges, isEligibleForARPosting);
		}

		internal Job Job
		{
			get { return Jobs.First(); }
		}

		protected override ZString GetDefaultJobTransactionDescription(IJobInvoicingPlugIn plugIn)
		{
			if (Jobs.Any())
			{
				return Jobs.First().JobDescription;
			}
			else
			{
				return base.GetDefaultJobTransactionDescription(plugIn);
			}
		}

		public override void PerformTransactionDescriptionDefaulting(IJobInvoicingPlugIn plugIn, TransactionCreatorHashtable transactions)
		{
			base.PerformTransactionDescriptionDefaulting(plugIn, transactions);

			var countryCode = Job.Company.AccountingCountry;
			if (AccountingUtils.ShouldShowRelatedDisbursementTransactions(countryCode))
			{
				var appendingRule = AccountingConfigurationRegistry.Instance.DSBSummaryAppendingRule.GetFallBackValueAtAllLevels(Job.JH_GC.ToGuid(), Guid.Empty, Guid.Empty);
				if (appendingRule.Count == 0)
				{
					return;
				}

				var groupedARInvoices = transactions.GetAllARInvoices().GroupBy(x => (x.AH_OH, x.AH_RX_NKTransactionCurrency)).ToList();
				var validTaxIds = appendingRule.Cast<KoreaSouthEInvoicingDSBSummaryAppendingRule>().OrderBy(x => x.Order).Select(x => x.TaxIdPK).ToArray();
				foreach (var arInvoiceGroup in groupedARInvoices)
				{
					var arInvoices = arInvoiceGroup.ToList().Cast<InvoicingBase>();
					if (arInvoices.Count() < 2)
					{
						continue;
					}

					InvoicingBase excludeInvoice = null;
					var taxInvoices = new List<(ZGuid, InvoicingBase)>();
					foreach (var invoice in arInvoices)
					{
						var lines = invoice.Lines.Cast<InvoicingLineBase>();
						if (excludeInvoice == null && lines.Any(x => x.AL_AT.IsValid && x.TaxRate.AT_Code == ExcludeTaxRateCode))
						{
							excludeInvoice = invoice;
						}

						var line = lines.FirstOrDefault(x => x.AL_AT.IsValid && validTaxIds.Contains(x.AL_AT));
						if (line != null)
						{
							taxInvoices.Add((line.AL_AT, invoice));
						}
					}

					if (excludeInvoice == null || taxInvoices.Count == 0)
					{
						continue;
					}

					var taxInvoice = taxInvoices.OrderBy(x => Array.IndexOf(validTaxIds, x.Item1)).First().Item2;

					var appendSummary = new StringBuilder();
					foreach (var line in excludeInvoice.Lines.Cast<InvoicingLineBase>())
					{
						appendSummary.AppendFormat(" {0}: {1}", line.ChargeCode.AC_Code, line.AL_LocalTotalAmount.ToString("N0"));
					}

					var provider = (ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IRelatedDisbursementTransaction>(countryCode));
					if (provider != null)
					{
						appendSummary.AppendFormat((NoResString)provider.AppendAdditionalDescription(excludeInvoice.AH_LocalTotalAmount.ToString("N0")));
					}

					taxInvoice.AH_Desc += appendSummary.ToString();
					if (taxInvoice.AH_Desc.Length > AccTransactionHeaderSchema.AH_Desc.MaxLength)
					{
						taxInvoice.AH_Desc = taxInvoice.AH_Desc.Truncate(AccTransactionHeaderSchema.AH_Desc.MaxLength);
					}

					taxInvoice.ReceivableDisbursementInvoicePK = excludeInvoice.PK;
				}
			}
		}

		const string ExcludeTaxRateCode = "EXCLUDE";

		#endregion
	}
}
