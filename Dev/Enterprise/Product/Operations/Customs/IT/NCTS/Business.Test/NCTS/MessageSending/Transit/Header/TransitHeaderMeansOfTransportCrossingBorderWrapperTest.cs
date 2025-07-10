using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitHeaderMeansOfTransportCrossingBorderWrapperTest : SADMeansOfTransportWrapperTest
{
	public void TestType()
	{
		var wrapper = new TransitHeaderMeansOfTransportCrossingBorderWrapper("XX", "ZZ");
		AssertEquals(nameof(wrapper.Type), ZString.Empty, wrapper.Type);
	}
}
