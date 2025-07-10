using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Colombia;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(TaxFrameworkConfigurationDefaults))]
	class ColombiaTaxFrameworkConfigurationDefaultsTest : TaxFrameworkConfigurationDefaultsTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Colombia;
		protected override int ExpectedNumberOfDefaultTaxAuthorities => 13;
		protected override int ExpectedNumberOfDefaultTaxSystems => 4;
	}
}
