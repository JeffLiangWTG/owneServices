using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.TaxFramework.Business
{
	static class TaxCalculationExtensions
	{
		public static (ZInt numerator, ZInt denominator)? GetTaxRate(this OrgCompanyData companyData,
			AccTaxConfiguration taxConfiguration, ZDate taxDate)
		{
			AccOrgTaxRateCollection taxrates = null;
			if (taxConfiguration.ETC_Ledger == AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code)
			{
				taxrates = companyData.APOrgTaxConfigurations.FirstOrDefault(x => x.OTC_ETC == taxConfiguration.PK)?.TaxRates;
			}
			else if (taxConfiguration.ETC_Ledger == AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code)
			{
				taxrates = companyData.AROrgTaxConfigurations.FirstOrDefault(x => x.OTC_ETC == taxConfiguration.PK)?.TaxRates;
			}
			else
			{
				throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("CE3DC10F-488A-4AA4-B7EF-D7D77FE5D63F", "Tax Configuration for ledger {0} was not found.", taxConfiguration.ETC_Ledger));
			}

			if (taxrates == null)
			{
				return null;
			}

			var found = taxrates.OrderBy(x => GetSourceOrder(x.OTR_Source)).FirstOrDefault(x => x.OTR_EndDate >= taxDate && x.OTR_StartDate <= taxDate);
			if (found != null)
			{
				return (numerator: found.OTR_RateNumerator, denominator: found.OTR_RateDenominator);
			}

			return null;

			int GetSourceOrder(ZString source)
			{
				if (source == AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.ManualOverride.Code)
				{
					return 0;
				}

				if (source == AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Quarterly.Code)
				{
					return 1;
				}

				if (source == AccountingMasterFilesTaxFrameworkConstants.RateSourceMethods.Monthly.Code)
				{
					return 2;
				}

				throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("9E52E6B0-9A6E-4F7E-A5EC-9188307174ED", "{0} organization rate source was not found.", source));
			}
		}

		public static (ZInt numerator, ZInt denominator)? GetTaxRate(this AccTaxRate taxRate, ZDate taxDate)
		{
			var taxIDRate = taxRate.GetRateComponents(taxDate);
			return taxIDRate.numerator == 0 && !taxRate.DoesRateExists(taxDate) ? null : taxIDRate;
		}

		public static TaxSystemsConfiguration GetTaxSystemForCalculation(this ITaxFrameworkConfigurationHelper taxFrameworkConfigurationHelper, ZString taxSystemCode, BusinessObjectFactory factoryForCaching)
		{
			var taxSystemConfiguration = taxFrameworkConfigurationHelper.GetTaxSystem(taxSystemCode, factoryForCaching);

			return taxSystemConfiguration ?? throw new TaxFrameworkUnknownConfigurationValueException(ResString.GetMultilingualString("4B6CDC81-644A-440D-9C1E-62E3B6A23579", "Tax System {0} can't be found.", taxSystemCode));
		}
	}
}
