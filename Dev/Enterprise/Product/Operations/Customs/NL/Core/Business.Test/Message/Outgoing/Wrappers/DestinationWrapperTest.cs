using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

class DestinationWrapperTest : DataProviderTestCase<DestinationWrapper>
{
	public void TestCountryCode()
	{
		AssertEquals("NL", wrapper.CountryCode);
	}

	public void TestRegionId()
	{
		AssertNull(wrapper.RegionId);
	}

	public void TestCCQualifierCode()
	{
		AssertNull(wrapper.CCQualifierCode);
	}

	protected override DestinationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_RL_NKFinalDestination = "NLDRE";
		wrapper = new DestinationWrapper(declaration.JE_RL_NKFinalDestination);
	}
	DestinationWrapper wrapper;
	JobDeclaration declaration;
}
