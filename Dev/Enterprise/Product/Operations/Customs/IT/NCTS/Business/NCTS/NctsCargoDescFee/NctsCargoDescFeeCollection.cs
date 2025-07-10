using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsCargoDescFeeCollection : CusInBondFeeCollection<NctsCargoDescFee>
{
	public NctsCargoDescFeeCollection(NctsDepartureCargoDesc master) : base(master)
	{
	}

	public ZDecimal GetTotalAmount() => GetTotalAmount(filterClause: null);

	public ZDecimal GetTotalAmount(ZString methodOfPayment) => GetTotalAmount(x => x.BFE_MethodOfPayment == methodOfPayment);

	protected override void SetDefaultsForNewElementCore(NctsCargoDescFee newElement)
	{
		base.SetDefaultsForNewElementCore(newElement);
		if (newElement is NctsCargoDescFee fee)
		{
			fee.BFE_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
		}
	}

	ZDecimal GetTotalAmount(Func<NctsCargoDescFee, bool> filterClause = null) => this.Cast<NctsCargoDescFee>().Where(filterClause ?? (x => true)).Sum(x => x.BFE_ChargeAmount);
}
