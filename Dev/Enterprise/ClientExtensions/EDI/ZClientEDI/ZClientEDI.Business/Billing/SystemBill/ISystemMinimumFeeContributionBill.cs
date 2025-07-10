using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface ISystemMinimumFeeContributionBill
	{
		IEnumerable<SystemMinimumFee> CalculateMinimumFeeContribution();
	}

	public class SystemMinimumFee
	{
		public SystemMinimumFee(ZGuid databasePk, ZDateTime periodStart, ZDecimal amount, ZString currency, ZString chargeCode, ClientLicencePriceItem priceItem = null, bool isNonProductionSystemFee = false)
		{
			DatabasePk = databasePk;
			PeriodStart = periodStart;
			Amount = amount;
			Currency = currency;
			ChargeCode = chargeCode;
			IsNonProductionSystemFee = isNonProductionSystemFee;
			PriceItem = priceItem;
		}

		public readonly ZGuid DatabasePk;
		public readonly ZDateTime PeriodStart;
		public readonly ZDecimal Amount;
		public readonly ZString Currency;
		public readonly ZString ChargeCode;
		public readonly bool IsNonProductionSystemFee;
		public readonly ClientLicencePriceItem PriceItem;
	}
}
