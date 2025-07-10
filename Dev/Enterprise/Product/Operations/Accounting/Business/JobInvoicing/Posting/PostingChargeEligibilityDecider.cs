using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class PostingChargeEligibilityDecider
	{
		public PostingChargeEligibilityDecider(Charge[] charges)
			: this(charges, null)
		{
		}

		public PostingChargeEligibilityDecider(Charge[] charges, Predicate<Charge> isEligibleForPosting)
		{
			this.Charges = charges;
			this.isEligibleForPosting = isEligibleForPosting;
		}

		public IReceivablesPostingChargeCollection GetEligibleCharges(JobInvoicingPostingOption postingOption)
		{
			var result = new IReceivablesPostingChargeCollection();

			foreach (IReceivablesPostingCharge charge in Charges)
			{
				if (ShouldPost(charge))
				{
					if (postingOption == JobInvoicingPostingOption.Disbursement)
					{
						if (InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(charge.InvoiceType))
						{
							result.Add(charge);
						}
					}
					else if (postingOption == JobInvoicingPostingOption.Agent)
					{
						if (IsAgentCharge(charge))
						{
							result.Add(charge);
						}
					}
					else if (postingOption == JobInvoicingPostingOption.Gateway)
					{
						if (IsGatewayCharge(charge))
						{
							result.Add(charge);
						}
					}
					else if (postingOption == JobInvoicingPostingOption.LocalClient)
					{
						if (charge.IsLocalClientCharge)
						{
							result.Add(charge);
						}
					}
					else if (postingOption == JobInvoicingPostingOption.AllSisterCompanyCharges)
					{
						if (charge.IsSisterCompanyCharge(false))
						{
							result.Add(charge);
						}
					}
					else if (postingOption == JobInvoicingPostingOption.LocalSisterCompanyChargesOnly)
					{
						if (charge.IsSisterCompanyCharge(true))
						{
							result.Add(charge);
						}
					}
					else
					{
						result.Add(charge);
					}
				}
			}

			return result;
		}

		bool ShouldPost(IReceivablesPostingCharge charge)
		{
			return IsAllowedToPostThisCharge(charge) && ((Charge)charge).HasValidDataForRevenuePosting && IsPostingAllowedForDeferredCharge(charge) && (isEligibleForPosting == null || isEligibleForPosting((Charge)charge));
		}

		protected virtual bool IsAllowedToPostThisCharge(IReceivablesPostingCharge chargeBeingDecided) => chargeBeingDecided.IsAllowedToPostSellCharge;

		protected virtual bool IsAgentCharge(IReceivablesPostingCharge chargeBeingDecided) => chargeBeingDecided.IsAgentCharge;

		protected virtual bool IsGatewayCharge(IReceivablesPostingCharge chargeBeingDecided) => false;

		protected virtual bool IsPostingAllowedForDeferredCharge(IReceivablesPostingCharge chargeBeingDecided)
		{
			return !chargeBeingDecided.IsDeferredCharge;
		}
		
		protected readonly Charge[] Charges;
		readonly Predicate<Charge> isEligibleForPosting;
	}
}
