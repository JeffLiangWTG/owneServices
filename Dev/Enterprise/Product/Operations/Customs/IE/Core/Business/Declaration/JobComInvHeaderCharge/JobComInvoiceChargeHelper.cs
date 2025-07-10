using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public static class JobComInvoiceChargeHelper
	{
		public static Dictionary<ZString, Money> GetChargeDeductions(IEnumerable<BaseJobComInvHeaderCharge> aisCharges)
		{
			var result = new Dictionary<ZString, Money>();

			foreach (var aisCharge in aisCharges)
			{
				if (IsRelevant(aisCharge))
				{
					var (key, money) = GetChargeInfo(aisCharge);
					if (result.TryGetValue(key, out var existingMoney))
					{
						if (existingMoney.Currency == money.Currency)
						{
							result[key] = new Money(existingMoney.Amount + money.Amount, existingMoney.Currency);
						}
					}
					else
					{
						result[key] = money;
					}
				}
			}
			return result;
		}

		static bool IsRelevant(BaseJobComInvHeaderCharge charge)
		{
			return ((charge.J7_IsDutiable || charge.J7_IsGSTApplicable || charge.J7_IsStatisticalValueApplicable) && !charge.J7_IsIncludedInITOT)
					|| (charge.J7_IsIncludedInITOT && (!charge.J7_IsDutiable ||
														charge.J7_ChargeType == AISChargeCodeList.Codes.AK ||
														charge.J7_ChargeType == AISChargeCodeList.Codes.BA ||
														charge.J7_ChargeType == AISChargeCodeList.Codes._1X))
					|| ((!charge.J7_IsGSTApplicable || !charge.J7_IsStatisticalValueApplicable) && charge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		static (ZString key, Money money) GetChargeInfo(BaseJobComInvHeaderCharge charge)
		{
			var money = charge.Money;
			var currencyCode = money.Currency?.Code ?? ZString.Empty;
			var key = charge.J7_ChargeType + Period + currencyCode;
			return (key, money);
		}

		const string Period = ".";
	}
}
