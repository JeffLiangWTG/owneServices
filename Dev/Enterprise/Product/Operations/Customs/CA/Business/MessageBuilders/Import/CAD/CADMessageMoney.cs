using CargoWise.Customs.CA.MessageContracts.CAD;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders;

public class CADMessageMoney : ICADMessageMoney
{
	public CADMessageMoney(Money money)
		: this(money.Amount, money.Currency)
	{
	}

	public CADMessageMoney(decimal amount, ICurrency currency)
	{
		this.amount = amount;
		this.currency = currency;
	}
	readonly decimal amount;
	readonly ICurrency currency;

	string ICADMessageMoney.CurrencyCode => currency?.Code ?? string.Empty;

	decimal ICADMessageMoney.Amount => amount;
}
