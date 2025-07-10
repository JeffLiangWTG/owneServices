using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Reports;

public class CustomsOfficesCodeListProvider : EU.Business.Reports.CustomsOfficesCodeListProvider, Integration.Customs.IT.ICustomsOfficesProvider
{
	protected override ZString CountryCode => Core.Constants.CountryCodes.Italy;
}
