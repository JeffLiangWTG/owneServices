using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public static class JobComInvChargeHelper
	{
		public static Dictionary<ZString, Money> GetCDSChargeDeductions(IEnumerable<BaseJobComInvHeaderCharge> cdsCharges)
		{
			var result = new Dictionary<ZString, Money>();

			foreach (var cdsCharge in cdsCharges)
			{
				if (IsRelevant(cdsCharge))
				{
					var (key, money) = GetCDSChargeInfo(cdsCharge);
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
														charge.J7_ChargeType == EU.Business.UCCCustomsChargeTypeList.Codes.TransportCostsCharge ||
														charge.J7_ChargeType == EU.Business.UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge))
					|| ((!charge.J7_IsGSTApplicable || !charge.J7_IsStatisticalValueApplicable) && charge.J7_Calc_IsIncludedInInvoiceAmount);
		}

		static (ZString key, Money money) GetCDSChargeInfo(BaseJobComInvHeaderCharge charge)
		{
			var money = charge.Money;
			var currencyCode = money.Currency?.Code ?? ZString.Empty;
			var key = charge.GetCDSChargeCode() + Period + currencyCode;
			return (key, money);
		}

		const string Period = ".";
	}
}
