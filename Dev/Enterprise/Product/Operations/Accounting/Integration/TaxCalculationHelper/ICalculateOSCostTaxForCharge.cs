#define CODE_ANALYSIS

using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Accounting.Integration.CalculateTaxForCharge
{
	public interface ICalculateOSCostTaxForCharge
	{
		[SuppressMessage("Microsoft.Design", "CA1045: Do not pass types by reference")]
		[SuppressMessage("Microsoft.Design", "CA1007: Use generics where appropriate")]
		ZDecimal CalculateOSCostGSTAmountWhenNotOverridden(ZDecimal costAmount, ZString costCurrency, ZGuid taxRatePK, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZGuid chargeCodePK, ZGuid jr_E6, string countryCode, ZGuid companyPK, ref Object cache);
	}
}
