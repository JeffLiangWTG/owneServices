using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsStatusOrderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions("Test CustomsStatusOrder constructor", () =>
		 {
			 AssertNoExceptionThrown("No exception expected", () => new CustomsStatusOrder("A", 1));
			 AssertNoExceptionThrown("No exception expected", () => new CustomsStatusOrder("", null));
			 AssertExceptionThrown<ArgumentNullException>("Should be exception when entryStatusCode parameter is null", () => new CustomsStatusOrder(null, 1));
		 });
	}
}
