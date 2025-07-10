using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public static class NctsCargoDescFeeCollectionExtension
{
	public static IEnumerable<NctsCargoDescFee> IncludedInMessageSending(this NctsCargoDescFeeCollection fees) => fees.Cast<NctsCargoDescFee>().Where(fee => SADWrapperHelper.IsFeeIncludedInMessageSending(fee));

	public static ZDecimal GetTotalTaxedAmount(this IEnumerable<NctsCargoDescFee> fees) => fees.Sum(x => x.BFE_ChargeAmount);
}
