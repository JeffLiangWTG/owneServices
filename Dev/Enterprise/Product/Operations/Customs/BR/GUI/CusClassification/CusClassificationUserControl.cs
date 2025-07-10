using CargoWise.Types;

namespace Enterprise.Customs.BR.GUI
{
	public class CusClassificationUserControl : Customs.GUI.GeneralCountryClassificationUserControl
	{
		protected override string GetCustomsCountryCode() => Core.Constants.CountryCodes.Brazil;
		protected override ZString GetDataGroupingForUniversalTariff() => Core.Constants.CountryCodes.Brazil;
	}
}
