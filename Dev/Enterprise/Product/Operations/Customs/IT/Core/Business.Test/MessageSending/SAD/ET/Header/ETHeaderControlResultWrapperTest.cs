using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETHeaderControlResultWrapperTest : TestCase
{
	public void TestWrapper()
	{
		CombineAssertions(() =>
		{
			var wrapper = new ETHeaderControlResultWrapper();
			AssertEquals(ZDate.Empty, wrapper.DateLimitForTheExitFromEC);
			AssertEquals(ZDate.Empty, wrapper.DateLimitOfArrivalNotification);
		});
	}
}
