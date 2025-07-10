using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Export.Business
{
	public static class TaxRate
	{
		public static ZDecimal GetExtraTaxAmountFromTaxAmount(decimal taxAmount, decimal taxRate, string taxRateType, decimal extraTaxRate, string extraTaxRateType, string country = "", decimal lineAmount = 0)
		{
			var effectiveExtraTaxRate = AccTaxRate.GetEffectiveExtraRate(taxRateType, taxRate, extraTaxRateType, extraTaxRate);

			if (AccTaxRate.GetIsVATRemittedByCustomer(country, taxRateType, extraTaxRateType))
			{
				return lineAmount * effectiveExtraTaxRate / 100;
			}

			var rate = AccTaxRate.GetRate(taxRateType, taxRate);

			return ObjectFactory.Get<ITaxAmountCalculator>().GetExtraTaxAmountFromTaxAmount(taxAmount, rate, effectiveExtraTaxRate);
		}
	}
}
