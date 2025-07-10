using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class CostWrapper : ICost
{
	public CostWrapper(decimal amount, string currency)
	{
		this.amount = amount;
		this.currency = currency;
	}

	readonly decimal amount;
	readonly string currency;

	public decimal Amount => amount;

	public string Currency => currency;
}
