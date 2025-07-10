using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business
{
	public class TariffFormatterDecider : ITariffFormatterDecider
	{
		public TariffFormatterDecider()
		{
		}

		public ITariffFormatter GetTariffFormatter(string countryCode)
		{
			return TariffFormatter.New(countryCode);
		}
	}
}

