using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Argentina;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(TaxFrameworkConfigurationDefaults))]
	class ArgentinaTaxFrameworkConfigurationDefaultsTest : TaxFrameworkConfigurationDefaultsTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Argentina;
		protected override int ExpectedNumberOfDefaultTaxAuthorities => 25;
		protected override int ExpectedNumberOfDefaultTaxSystems => 1;
	}
}
