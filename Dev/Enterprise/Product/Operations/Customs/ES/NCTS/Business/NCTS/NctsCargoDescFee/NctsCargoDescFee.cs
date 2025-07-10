using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsCargoDescFee(BusinessObjectFactory factory, DataRow row) : EU.NCTS.Business.NctsCargoDescFee(factory, row)
{
	public override ZDecimal BFE_ChargeAmount
	{
		get => base.BFE_ChargeAmount;
		set
		{
			var oldValue = BFE_ChargeAmount;
			base.BFE_ChargeAmount = value;
			if (BFE_ChargeAmount != oldValue && Parent.IsPhase5Arrival)
			{
				Parent.Header.ApportionedAmountToGuaranteesLiabilityAmount();
			}
		}
	}
}
