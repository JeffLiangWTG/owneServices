using CargoWise.Types;

namespace Enterprise.Customs.CN.GUI
{
	public class CusClassificationUserControl : Customs.GUI.GeneralCountryClassificationUserControl
	{
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.China;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.China;
	}
}
