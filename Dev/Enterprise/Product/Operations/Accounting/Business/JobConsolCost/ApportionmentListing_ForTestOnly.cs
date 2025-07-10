#if DEBUG

using System.Collections.Generic;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public partial class ApportionmentListing
	{
		public List<Job> JobsWithMutexes_ForTestOnly => JobsWithMutexes;
	}
}

#endif
