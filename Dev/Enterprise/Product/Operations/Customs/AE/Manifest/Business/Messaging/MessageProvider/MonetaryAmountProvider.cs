namespace Enterprise.Customs.AE.Manifest.Business;

sealed class MonetaryAmountProvider : IMonetaryAmountProvider
{
	public MonetaryAmountProvider(string amountType, decimal amount, string currency)
	{
		AmountType = amountType;
		Amount = amount;
		Currency = currency;
	}

	public string AmountType { get; }

	public decimal Amount { get; }

	public string Currency { get; }
}
