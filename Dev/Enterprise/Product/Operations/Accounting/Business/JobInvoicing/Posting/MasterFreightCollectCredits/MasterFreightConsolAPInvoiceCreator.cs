
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class MasterFreightConsolAPInvoiceCreator : ConsolAPInvoiceCreator
	{
		public MasterFreightConsolAPInvoiceCreator(AgentChargePostingDetails postingDetails, BusinessObjectFactory fallbackFactory, IEnumerable<Job> jobs, bool hasJobOnHold, IJobCostingPlugIn consol, JobConsolCostCollection consolCosts, List<ZGuid> postponedConsolCostPKs = null)
			: base(fallbackFactory, jobs, hasJobOnHold, consol, consolCosts)
		{
			this.PostingDetails = postingDetails;
			this.ConsolCosts = consolCosts;
			this.PostponedConsolCostPKs = postponedConsolCostPKs;
		}

		readonly AgentChargePostingDetails PostingDetails;
		readonly JobConsolCostCollection ConsolCosts;
		readonly List<ZGuid> PostponedConsolCostPKs;

		protected override APInvoiceCreator GetInvoiceCreator(Job job)
		{
			return new MasterFreightCollectAPInvoiceCreator(PostingDetails, job, Consol, HasJobOnHold, ConsolCosts, PostponedConsolCostPKs);
		}
	}
}
