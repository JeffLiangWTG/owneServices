using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class TariffGridFindBox : Universal.GUI.TariffGridFindBox
	{
		public TariffGridFindBox(string parameterForediTariff)
		{
			ParameterForediTariff = parameterForediTariff;
		}

		public readonly string ParameterForediTariff;

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(ParameterForediTariff, () => base.GetNewPopupForm());
		}
	}
}
