using System;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class OriginProviderTest : Customs.Business.Testing.DataProviderTestCase<OriginProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new OriginProvider(null));
	}

	public void TestCountryOfOrigin()
	{
		invoiceLine.JI_CountryOfOrigin = "BE";
		AssertEquals("BE", provider.CountryOfOrigin);
	}

	public void TestRegionOfDispatch()
	{
		invoiceLine.JI_StateOrRegionOfOrigin = "D";
		AssertEquals("D", provider.RegionOfDispatch);
	}

	public void TestCountryOfPreferentialOrigin()
	{
		invoiceLine.ZG_CountryOfSupply = "DE";
		AssertEquals("DE", provider.CountryOfPreferentialOrigin);
	}

	protected override OriginProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		invoiceLine = Factory.NewWithValidTestData<JobComInvoiceLine>();
		provider = new OriginProvider(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	OriginProvider provider;
}
