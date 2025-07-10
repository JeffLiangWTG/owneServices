namespace Enterprise.Customs.Common
{
	public interface ITariffFormatProvider
	{
		ITariffFormatter TariffFormatter { get; }
	}
}
