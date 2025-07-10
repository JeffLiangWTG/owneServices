using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	class CustomsOfficesCodeListProviderTest : EU.Business.Reports.Testing.CustomsOfficesCodeListProviderTest
	{
		protected override ZString CountryCode => Core.Constants.CountryCodes.France;

		protected override EU.Business.Reports.CustomsOfficesCodeListProvider GetProvider() => new CustomsOfficesCodeListProvider();
	}
}
