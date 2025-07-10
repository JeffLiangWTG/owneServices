using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Reports.Testing;

sealed class CustomsOfficesCodeListProviderTest : EU.Business.Reports.Testing.CustomsOfficesCodeListProviderTest
{
	protected override ZString CountryCode => Core.Constants.CountryCodes.Italy;

	protected override EU.Business.Reports.CustomsOfficesCodeListProvider GetProvider() => new CustomsOfficesCodeListProvider();
}
