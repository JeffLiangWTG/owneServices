using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>))]
	class CusAuthorizationUsageInvoiceLineCollectionTest : CusAuthorizationUsageCollectionJobComInvoiceLineAbstractTest<CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>, CusAuthorizationUsage>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(Factory.New<JobComInvoiceLine>(), Factory);
	}
}
