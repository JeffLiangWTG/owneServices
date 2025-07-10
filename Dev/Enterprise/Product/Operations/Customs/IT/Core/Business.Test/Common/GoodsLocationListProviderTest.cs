using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class GoodsLocationListProviderTest<T> : TestCaseWithFactory
	where T : GoodsLocationList, new()
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => GetNewGoodsLocationListProvider(new Mock<IAutHeaderWithCusOfficeProvider>().Object, null));
		AssertExceptionThrown<ArgumentNullException>("When header is null", () => GetNewGoodsLocationListProvider(null, Factory));
	}

	public abstract void TestLocationsWithEmptyAuthorizationNumber();
	public abstract void TestLocationsWithWrongAuthorizationNumber();
	public abstract void TestLocations();

	protected abstract GoodsLocationListProvider<T> GetNewGoodsLocationListProvider(IAutHeaderWithCusOfficeProvider authorisationWithCustomsOfficeProvider, BusinessObjectFactory factory);
}
