using System.Collections.Generic;
using CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public sealed class TraderEqualityComparer : IEqualityComparer<IEoriTrader>
{
	public TraderEqualityComparer()
	{
		addressEqualityComparer = new AddressEqualityComparer();
	}

	public bool Equals(IEoriTrader trader1, IEoriTrader trader2)
	{
		if (ReferenceEquals(trader1, trader2))
		{
			return true;
		}

		if (trader1 == null || trader2 == null)
		{
			return false;
		}

		return trader1.EoriNumber == trader2.EoriNumber
				&& trader1.IdentificationNumber == trader2.IdentificationNumber
				&& addressEqualityComparer.Equals(trader1.Address, trader2.Address);
	}

	public int GetHashCode(IEoriTrader trader)
	{
		if (trader is null)
		{
			return 0;
		}

		var hashEori = trader.EoriNumber.GetHashCode();
		var hashIdentificationNumber = trader.IdentificationNumber.GetHashCode();

		return hashEori
				^ hashIdentificationNumber
				^ addressEqualityComparer.GetHashCode(trader.Address);
	}

	readonly AddressEqualityComparer addressEqualityComparer;
}
