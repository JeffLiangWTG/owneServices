using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business
{
	public sealed class MoneyProviderCalculated : IMoney
	{
		public MoneyProviderCalculated(string currencyCode, decimal value)
		{
			Value = value;
			CurrencyCode = currencyCode;
		}
		public decimal Value { get; }

		public string CurrencyCode { get; }
	}
}
