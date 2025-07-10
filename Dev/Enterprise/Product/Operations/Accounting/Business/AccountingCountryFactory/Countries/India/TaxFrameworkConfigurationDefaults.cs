using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.India
{
	public class TaxFrameworkConfigurationDefaults : ITaxFrameworkConfigurationDefaults
	{
		List<TaxAuthoritiesConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxAuthorities()
		{
			var result = new List<TaxAuthoritiesConfiguration>();
			result.Add(new TaxAuthoritiesConfiguration() { Country = "IN", Code = "CBDT", TaxAuthorityType = "NAT", Name = (NoResString)"CENTRAL BOARD OF DIRECT TAXES" });
			return result;
		}

		List<TaxSystemsConfiguration> ITaxFrameworkConfigurationDefaults.GetTaxSystems()
		{
			var result = new List<TaxSystemsConfiguration>();
			result.Add(new TaxSystemsConfiguration()
			{
				Country = "IN",
				Code = "TDS",
				TaxSuperType = "SPR",
				TaxAuthorityType = "NAT",
				RegistrationLevel = "CMP",
				IncludeInInvoceTotal = false,
				AdjustmentSign = "NEG",
				TaxBaseCalculationMethod = "ILA",
				TaxAmountCalculationMethod = "BTR",
				ThresholdRule = "NTR",
				TaxRateSource = "OFT",
				Name = (NoResString)"Tax Deducted at Source"
			});
			return result;
		}
	}
}
