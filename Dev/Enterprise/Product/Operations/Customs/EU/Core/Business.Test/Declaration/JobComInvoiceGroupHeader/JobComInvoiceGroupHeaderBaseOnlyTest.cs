using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderBaseOnlyTest : JobComInvoiceGroupHeaderTest
	{
		public void TestTypeDecider()
		{
			AssertType<JobComInvoiceGroupHeaderTypeDecider>("TypeDecider", JobComInvoiceGroupHeader.TypeDecider);
		}
	}
}
