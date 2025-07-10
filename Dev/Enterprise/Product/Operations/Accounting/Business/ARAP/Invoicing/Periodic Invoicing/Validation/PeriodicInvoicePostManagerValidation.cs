using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class PeriodicInvoicePostManagerValidation : PostManagerValidation
	{
		public PeriodicInvoicePostManagerValidation(Job job, IEnumerable<Charge> chargesToPost, PeriodicInvoice periodicInvoice)
			: base(null, JobInvoicingPostingOption.Revenue, null)
		{
			var jobCollection = new[] { job };
			this.OriginalJobs = jobCollection;
			this.Jobs = jobCollection;
			this.chargesToPost = chargesToPost;
			this.CurrentPeriodicInvoice = periodicInvoice;
		}

		readonly IEnumerable<Charge> chargesToPost;
		readonly PeriodicInvoice CurrentPeriodicInvoice;

		protected override IEnumerable<Charge> GetCharges(Job job)
		{
			return chargesToPost;
		}

		protected override bool IsSellEligibleToPost(Charge charge)
		{
			if (InvoiceTypeCalculationProvider.IsDeferredInvoiceType(charge.JR_InvoiceType))
			{
				return base.IsSellEligibleToPost(charge);
			}
			else
			{
				return false;
			}
		}

		protected override bool IsCostEligibleToPost(Charge charge)
		{
			return false;
		}

		protected override bool IsAllowedToPostSellCharge(Charge charge)
		{
			return true;
		}

		protected override bool ShouldMandatorySellSupplyType(ZString invoiceType) => true;

		protected override string GetRevenueRecognitionValidationError(Charge charge)
		{
			if (IsSellEligibleToPost(charge))
			{
				return base.GetRevenueRecognitionValidationError(charge);
			}
			else
			{
				return string.Empty;
			}
		}

		protected override string RunExchangeRateValidation()
		{
			if (CurrentPeriodicInvoice != null)
			{
				var company = Jobs.FirstOrDefault()?.Company ?? GlbCompany.CurrentCompany;

				foreach (Job jobToPost in Jobs)
				{
					if (!jobToPost.HasErrors)
					{
						foreach (Charge charge in GetCharges(jobToPost))
						{
							var isChargeInLocalInvoiceCurrencyForPosting = charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR);
							if (!charge.IsRevenuePosted && IsSellEligibleToPost(charge) //the sell rate will be calculated via buy rate later.
								&& ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, isChargeInLocalInvoiceCurrencyForPosting, company.PK))
							{
								var errorMessage = ExchangeRateCalculator.CheckExchangeRate(
									jobToPost,
									charge.BillInInvoiceCurrency ? charge.JR_RX_NKSellInvoiceCurrency : charge.JR_SellCurrency,
									isChargeInLocalInvoiceCurrencyForPosting,
									ExchangeRateValidLedgerEnum.AR,
									CurrentPeriodicInvoice.DebtorPK,
									CurrentPeriodicInvoice.InvoiceDate,
									CurrentPeriodicInvoice.PostDate,
									CurrentPeriodicInvoice.InvoiceDate); //No tax date available for periodic invoice

								if (!string.IsNullOrWhiteSpace(errorMessage))
								{
									return Res.GetString("bf1678ae-d0ad-473c-a6df-e23fd7d0b85e", @"This job cannot be posted. {0}", errorMessage);
								}
							}
						}
					}
				}
			}
			return string.Empty;
		}

		protected override string SellSupplyTypeErrorMessage => Res.GetString("780441A2-7DA6-4E99-BA3F-B39976FC5C29", "Charges without Supply Type specified cannot be included in the invoice.");

		protected override string RunJobChargeSellTaxBranchValidation()
		{
			if (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value && CurrentPeriodicInvoice != null)
			{
				var chargePKs = chargesToPost.Select(x => x.PK);

				var chargesQuery = new ZQuery(JobChargeSchema.PK, chargePKs);
				chargesQuery.ReLoadExistingRows = true;
				var reloadedCharges = CurrentPeriodicInvoice.Factory.Load<Charge>(chargesQuery);
				if (reloadedCharges.Any(x => x.JR_GB_SellTaxBranch != CurrentPeriodicInvoice.TaxBranch))
				{
					return Res.GetString("d1030ffe-21c8-4f8b-8279-78e5b58f87a9", "Tax Branch values on unposted charges in the billing tab conflict with the Periodic Invoice value.");
				}
			}

			return string.Empty;
		}
	}
}
