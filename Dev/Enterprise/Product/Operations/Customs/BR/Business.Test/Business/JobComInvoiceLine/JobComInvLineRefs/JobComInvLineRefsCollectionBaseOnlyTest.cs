using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvLineRefsCollection<JobComInvLineRefs>))]
	class JobComInvLineRefsCollectionBaseOnlyTest : JobComInvLineRefsCollectionTest<JobComInvLineRefs>
	{
		protected override JobComInvLineRefsCollection<JobComInvLineRefs> GetJobComInvLineRefsCollection()
		{
			return new JobComInvLineRefsCollection<JobComInvLineRefs>(Factory.New<JobComInvoiceLine>(), "ABC");
		}
	}
}
