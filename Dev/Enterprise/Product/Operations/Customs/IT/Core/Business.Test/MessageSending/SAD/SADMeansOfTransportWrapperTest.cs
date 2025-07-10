using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SADMeansOfTransportWrapperTest : TestCaseWithFactory
{
	public void TestNationality()
	{
		var sadMeansOfTransportWrapper = new SADMeansOfTransportWrapper("IT", ZString.Empty);
		AssertEquals("IT", sadMeansOfTransportWrapper.Nationality);
	}

	public void TestIdentity()
	{
		var sadMeansOfTransportWrapper = new SADMeansOfTransportWrapper(ZString.Empty, "XPJ945");
		AssertEquals("XPJ945", sadMeansOfTransportWrapper.Identity);
	}
}
