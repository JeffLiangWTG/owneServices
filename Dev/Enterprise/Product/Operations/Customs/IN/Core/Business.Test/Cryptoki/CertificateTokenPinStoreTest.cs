using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CertificateTokenPinStore))]
sealed class CertificateTokenPinStoreTest : TestCaseWithFactory
{
	public void TestTokenPinStore()
	{
		var tokenPinStore = CertificateTokenPinStore.Instance as ITokenPinStore;
		AssertNullOrEmpty("Pin not initialized", tokenPinStore.GetPin());

		tokenPinStore.SetPin("1234");
		AssertEquals("Store returns valid pin", "1234", tokenPinStore.GetPin());

		tokenPinStore.ResetPin();
		AssertNullOrEmpty("Pin is reset", tokenPinStore.GetPin());
	}
}
