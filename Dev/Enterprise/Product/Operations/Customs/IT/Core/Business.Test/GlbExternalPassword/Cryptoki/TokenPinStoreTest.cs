using System;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class TokenPinStoreTest : TestCase
{
	public void TestTokenPinStore()
	{
		var tokenPinStore = (ITokenPinStore)new TokenPinStore();
		AssertEquals("Pin initial value", "", tokenPinStore.GetPin());

		tokenPinStore.SetPin("123");
		AssertEquals("Pin after being set", "123", tokenPinStore.GetPin());

		tokenPinStore.ResetPin();
		AssertEquals("Pin after being reset", "", tokenPinStore.GetPin());
	}

	public void TestIsPinCached()
	{
		var tokenPinStore = (ITokenPinStore)new TokenPinStore();
		tokenPinStore.SetPin("123");
		AssertEquals("IsPinCached", true, tokenPinStore.IsPinCached());

		tokenPinStore.ResetPin();
		AssertEquals("IsPinCached", false, tokenPinStore.IsPinCached());
	}

	public void TestSetPin_ThrowsExceptionIsEmpty()
	{
		var tokenPinStore = (ITokenPinStore)new TokenPinStore();
		AssertExceptionThrown<ArgumentException>(() => tokenPinStore.SetPin(""));
	}
}
