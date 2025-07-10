#if DEBUG

using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class CustomsDisbursementChargePoster
	{
		public GlbDepartment GetParentDepartment_ForTestOnly(ICustomsJobInfo parent)
		{
			return GetParentDepartment(parent);
		}

		public Job GetOrCreateJob_ForTestOnly(ICustomsJobInfo customsJob)
		{
			return GetOrCreateJob(customsJob);
		}

		public Job GetOrCreateCustomsJob_ForTestOnly(ICustomsJobInfo parent)
		{
			return GetOrCreateCustomsJob(parent);
		}

		public bool PostInvoice_ForTestOnly(Job invoicingJob, JobInvoicingPostingOption option)
		{
			return PostInvoice(invoicingJob, option);
		}

		public bool NoCustomsDisbursementChargeToAdd_ForTestOnly(AutoRateInfoCollection rateInfos)
		{
			return NoCustomsDisbursementChargeToAdd(rateInfos);
		}
	}
}

#endif
