using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADEmptyWarehouseIdentificationWrapperTest : TestCaseWithFactory
{
	public void TestWrapper()
	{
		CombineAssertions(nameof(SADEmptyWarehouseIdentificationWrapper), () =>
		{
			var wrapper = new SADEmptyWarehouseIdentificationWrapper();
			AssertEquals(ZString.Empty, wrapper.Type);
			AssertEquals(ZString.Empty, wrapper.Identification);
			AssertEquals(ZString.Empty, wrapper.CinIdentification);
			AssertEquals(ZString.Empty, wrapper.AuthorizingCountry);
		});
	}
}
