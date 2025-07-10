using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TIREmptyMeansOfTransportCrossingBorderWrapperTest : TestCaseWithFactory
{
	public void TestProperties()
	{
		var emptyMeansOfTransportCrossingBorderWrapper = new TIRHeaderEmptyMeansOfTransportCrossingBorderWrapper();

		CombineAssertions("Assert properties must be empty", () =>
		{
			AssertEquals("Type", ZString.Empty, emptyMeansOfTransportCrossingBorderWrapper.Type);
			AssertEquals("Nationality", ZString.Empty, emptyMeansOfTransportCrossingBorderWrapper.Nationality);
			AssertEquals("Identity", ZString.Empty, emptyMeansOfTransportCrossingBorderWrapper.Identity);
		});
	}
}
