namespace Enterprise.Customs.Common
{
	public interface ITariffFormatterDecider
	{
		ITariffFormatter GetTariffFormatter(string countryCode);
	}
}
