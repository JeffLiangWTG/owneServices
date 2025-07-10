using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Brazil;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(TaxFrameworkConfigurationDefaults))]
	class BrazilTaxFrameworkConfigurationDefaultsTest : TaxFrameworkConfigurationDefaultsTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Brazil;
		protected override int ExpectedNumberOfDefaultTaxAuthorities => 25;
		protected override int ExpectedNumberOfDefaultTaxSystems => 9;
	}
}
