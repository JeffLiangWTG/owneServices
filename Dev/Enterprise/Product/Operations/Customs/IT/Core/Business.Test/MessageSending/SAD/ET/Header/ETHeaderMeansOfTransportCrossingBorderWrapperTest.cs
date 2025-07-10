using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETHeaderMeansOfTransportCrossingBorderWrapperTest : SADMeansOfTransportWrapperTest
{
	public void TestType()
	{
		var wrapper = new ETHeaderMeansOfTransportCrossingBorderWrapper("XX", "ZZ");
		AssertEquals(ZString.Empty, wrapper.Type);
	}
}
