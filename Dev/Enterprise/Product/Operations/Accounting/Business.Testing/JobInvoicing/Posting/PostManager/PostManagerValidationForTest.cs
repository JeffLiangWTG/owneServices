using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	internal class PostManagerValidationForTest : PostManagerValidation
	{
		public PostManagerValidationForTest(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
			: base(jobs, postingOption, originalJobs)
		{
		}

		public bool IsCostEligibleToPost_Exposed(Charge charge, Job jobToPost)
		{
			return base.IsCostEligibleToPost(charge);
		}

		public bool IsSellEligibleToPost_Exposed(Charge charge, Job jobToPost)
		{
			return base.IsSellEligibleToPost(charge);
		}
	}
}