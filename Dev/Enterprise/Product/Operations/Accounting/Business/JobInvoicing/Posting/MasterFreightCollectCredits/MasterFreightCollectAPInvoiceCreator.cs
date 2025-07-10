using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class MasterFreightCollectAPInvoiceCreator : APInvoiceCreator
	{
		public MasterFreightCollectAPInvoiceCreator(AgentChargePostingDetails postingDetails, Job job, IJobCostingPlugIn consol, bool hasJobsOnHold, JobConsolCostCollection consolCosts, List<ZGuid> postponedConsolCostPKs = null)
			: base(job, consol, hasJobsOnHold, consolCosts)
		{
			this.PostingDetails = postingDetails;
			this.PostponedConsolCostPKs = postponedConsolCostPKs;
		}

		readonly AgentChargePostingDetails PostingDetails;
		readonly List<ZGuid> PostponedConsolCostPKs;

		protected override bool ShouldPostAgentRelatedCharge(Charge charge)
		{
			return !base.ShouldPostAgentRelatedCharge(charge)
				&& (PostponedConsolCostPKs == null || PostponedConsolCostPKs.Count == 0 || !PostponedConsolCostPKs.Contains(charge.JR_E6));
		}

		protected override void SetJobAdditionalReferences(Charge charge, APInvoice aPInvoice, ZDateTime postingTime)
		{
			base.SetJobAdditionalReferences(charge, aPInvoice, postingTime);
			if (PostingDetails != null && !PostingDetails.AgentInvoices.Contains(aPInvoice))
			{
				PostingDetails.AgentInvoices.Add(aPInvoice);
			}
		}
	}
}
