using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.GUI
{
	public class TariffColumnStyle : ZBaseFindBoxColumnStyle
	{
		public TariffColumnStyle(TariffColumnStyleInfo info)
			: base(() => new TariffGridFindBox(info.ParameterForediTariff), info)
		{
		}
	}
}
