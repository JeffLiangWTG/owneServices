using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class MoneyProvider : IMoney
	{
		public MoneyProvider(decimal amount, string currency)
		{
			Amount = amount;
			Currency = currency;
		}

		public decimal Amount { get; }

		public string Currency { get; }
	}
}
