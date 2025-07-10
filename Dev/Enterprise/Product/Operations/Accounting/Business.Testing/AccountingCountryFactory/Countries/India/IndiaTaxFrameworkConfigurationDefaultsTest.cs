using Enterprise.Accounting.Business.AccountingCountryFactory.Countries.India;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(TaxFrameworkConfigurationDefaults))]
	class IndiaTaxFrameworkConfigurationDefaultsTest : TaxFrameworkConfigurationDefaultsTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.India;
		protected override int ExpectedNumberOfDefaultTaxAuthorities => 1;
		protected override int ExpectedNumberOfDefaultTaxSystems => 1;
	}
}
