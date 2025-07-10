using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

public static class CusEntryLineFeeCollectionExtensions
{
	public static IEnumerable<CusEntryLineFee> InCustomsCompliantOrder(this IEnumerable<CusEntryLineFee> fees) => fees.OrderBy(x => x, new CusEntryLineFeeComparer());

	public static IEnumerable<CusEntryLineFee> IncludedInMessageSending(this CusEntryLineFeeCollection fees) => fees.Cast<CusEntryLineFee>().Where(fee => SADWrapperHelper.IsFeeIncludedInMessageSending(fee));

	public static ZDecimal GetTotalTaxedAmount(this IEnumerable<CusEntryLineFee> fees) => fees.Sum(x => x.CF_ChargeAmount);
}
