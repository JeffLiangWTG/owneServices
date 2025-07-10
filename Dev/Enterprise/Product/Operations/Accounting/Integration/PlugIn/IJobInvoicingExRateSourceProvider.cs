namespace Enterprise.Accounting.Integration
{
	public interface IJobInvoicingExRateSourceProvider
	{
		IExchangeRateSource GetExRateSource(ExRateSourceType sourceType);
	}
}
