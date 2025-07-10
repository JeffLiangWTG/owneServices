using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

class OriginWrapperTest : DataProviderTestCase<OriginWrapper>
{
	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, Provider.SequenceNumeric);
	}

	public void TestCountryCode()
	{
		AssertEquals("S1", Provider.CountryCode);
	}

	public void TestRegionId()
	{
		AssertNull(Provider.RegionId);
	}

	public void TestTypeCode()
	{
		AssertEquals("1", Provider.TypeCode);
	}

	protected override OriginWrapper GetProvider() => new OriginWrapper("S1", "1", 1);
}
