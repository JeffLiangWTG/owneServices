using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCOJobComInvLineRefsCollection))]
	class LPCOJobComInvLineRefsCollectionTest : JobComInvLineRefsCollectionTest<LPCOJobComInvLineRefs>
	{
		protected override JobComInvLineRefsCollection<LPCOJobComInvLineRefs> GetJobComInvLineRefsCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new LPCOJobComInvLineRefsCollection(jobComInvoice);
		}
	}
}
