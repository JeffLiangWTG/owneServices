using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

abstract class SADEmptyTraderWrapperTest : TestCase
{
	public void TestWrapper()
	{
		CombineAssertions(nameof(SADEmptyTraderWrapper), () =>
		{
			var wrapper = new SADEmptyTraderWrapper();
			AssertEquals(ZString.Empty, wrapper.Address);
			AssertEquals(ZString.Empty, wrapper.City);
			AssertEquals(ZString.Empty, wrapper.CountryCode);
			AssertEquals(ZString.Empty, wrapper.ID);
			AssertEquals(ZString.Empty, wrapper.IdCountryCode);
			AssertEquals(ZString.Empty, wrapper.Name);
			AssertEquals(ZString.Empty, wrapper.Postcode);
		});
		AdditionalPropertiesCheck();
	}

	protected virtual void AdditionalPropertiesCheck() { }
}
