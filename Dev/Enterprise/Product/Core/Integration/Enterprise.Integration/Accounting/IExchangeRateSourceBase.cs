using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IExchangeRateSourceBase : IEnumerable<IExchangeRate>
	{
		ZGuid SourcePK { get; }
		ZString Description { get; }
		ZDecimal? GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger);
	}
}
