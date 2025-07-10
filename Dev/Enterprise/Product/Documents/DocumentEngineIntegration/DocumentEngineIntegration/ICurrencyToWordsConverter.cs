namespace Enterprise.DocumentEngineIntegration
{
	public interface ICurrencyToWordsConverter
	{
		string Convert(double amount, string currencyCode);
	}
}