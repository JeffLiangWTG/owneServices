using CargoWise.Customs.IT.MessageContracts;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

public static class TraderWrapperAssertionHelper
{
	public static void AssertTrader(ITrader trader, string expectedStreetAndNumber, string expectedCity, string expectedCountry, string expectedIdentificationNumber, string expectedName, string expectedZipCode)
	{
		AssertEquals(nameof(IAddress.StreetAndNumber), expectedStreetAndNumber, trader.Address.StreetAndNumber);
		AssertEquals(nameof(IAddress.City), expectedCity, trader.Address.City);
		AssertEquals(nameof(IAddress.Country), expectedCountry, trader.Address.Country);
		AssertEquals(nameof(ITrader.IdentificationNumber), expectedIdentificationNumber, trader.IdentificationNumber);
		AssertEquals(nameof(IAddress.Name), expectedName, trader.Address.Name);
		AssertEquals(nameof(IAddress.ZipCode), expectedZipCode, trader.Address.ZipCode);
	}
}
