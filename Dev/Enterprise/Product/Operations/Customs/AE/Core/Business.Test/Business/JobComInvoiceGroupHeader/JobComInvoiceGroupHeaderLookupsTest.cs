using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

class JobComInvoiceGroupHeaderLookupsTest : TestCaseWithFactory
{
	public void TestGroupHeader()
	{
		AssertEquals(lookup.GroupHeader, groupHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();
		groupHeader = Factory.New<JobComInvoiceGroupHeader>();
		lookup = new JobComInvoiceGroupHeaderLookups(groupHeader);
	}

	protected JobComInvoiceGroupHeaderLookups lookup;
	protected JobComInvoiceGroupHeader groupHeader;
}
