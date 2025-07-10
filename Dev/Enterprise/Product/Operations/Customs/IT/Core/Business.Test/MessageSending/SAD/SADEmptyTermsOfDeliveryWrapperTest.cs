using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADEmptyTermsOfDeliveryWrapperTest : TestCase
{
	public void TestWrapper()
	{
		var wrapper = new SADEmptyTermsOfDeliveryWrapper();
		AssertEquals(ZString.Empty, wrapper.ComplementaryCode);
		AssertEquals(ZString.Empty, wrapper.ComplementOfInfo);
		AssertEquals(ZString.Empty, wrapper.ComplementOfInfoLng);
		AssertEquals(ZString.Empty, wrapper.IncotermCode);
	}
}
