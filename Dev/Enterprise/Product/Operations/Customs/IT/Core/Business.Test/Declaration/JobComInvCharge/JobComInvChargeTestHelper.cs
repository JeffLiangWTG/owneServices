using Enterprise.Customs.Common;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

static class JobComInvChargeTestHelper
{
	public static JobComInvCharge AddNewWithTestValues(this IJobComInvChargeCollection<JobComInvCharge> charges, string chargeType, decimal amount = 0m, string currency = "EUR", bool isStatisticalValueApplicable = false, bool isIncludedInInvoice = false, bool isIncludedInLines = false)
	{
		var charge = charges.AddNew();
		charge.SetTestValues(chargeType, amount, currency, isStatisticalValueApplicable, isIncludedInInvoice, isIncludedInLines);
		return charge;
	}

	public static JobComInvCharge SetTestValues(this JobComInvCharge charge, string chargeType, decimal amount = 0m, string currency = "EUR", bool isStatisticalValueApplicable = false, bool isIncludedInInvoice = false, bool isIncludedInLines = false)
	{
		charge.J7_ChargeType = chargeType;
		charge.J7_Amount = amount;
		charge.J7_RX_NKCurrency = currency;
		charge.J7_IsStatisticalValueApplicable = isStatisticalValueApplicable;
		charge.J7_IsNotIncludedInInvoice = !isIncludedInInvoice;
		charge.J7_IsIncludedInITOT = isIncludedInLines;
		return charge;
	}
}
