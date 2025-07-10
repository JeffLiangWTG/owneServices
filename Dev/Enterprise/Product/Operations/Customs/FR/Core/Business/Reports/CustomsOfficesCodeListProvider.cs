using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Reports
{
	public class CustomsOfficesCodeListProvider : EU.Business.Reports.CustomsOfficesCodeListProvider, Integration.Customs.FR.ICustomsOfficesProvider
	{
		protected override ZString CountryCode => Core.Constants.CountryCodes.France;
	}
}
