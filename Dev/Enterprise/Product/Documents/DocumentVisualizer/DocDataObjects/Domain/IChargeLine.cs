using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IChargeLine
	{
		ZString Description { get; set; }
		ZBool IsPrepaid { get; set; }
		IMoney Cost { get; }
		IMoney Sell { get; }
		IMoney LocalCost { get; }
		IMoney LocalSell { get; }
		IPaymentBasisCollection PaymentBases { get; }
		ICodeDescription ChargeCode { get; set; }
		ZDecimal SellExchangeRate { get; }
		ZDecimal CostExchangeRate { get; }
		ZDecimal CFX { get; }
		IReadOnlyCollection<IChargeLineAttribute> ChargeLineAttributes { get; }
	}
}
