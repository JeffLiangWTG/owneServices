using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestsSubclassesOf(typeof(ITaxFrameworkConfigurationDefaults),
		RestrictedToAssemblies = new[] { "Enterprise.Accounting.Business" },
		ExcludeClientDlls = true)]
	public abstract class TaxFrameworkConfigurationDefaultsTest : TestCaseWithFactory
	{
		protected abstract string CountryCode { get; }

		protected abstract int ExpectedNumberOfDefaultTaxAuthorities { get; }

		protected abstract int ExpectedNumberOfDefaultTaxSystems { get; }

		public void TestCountrySpecificTaxAuthoritiesMustbePresentInDefaultTaxAuthorities()
		{
			var defaultValuesForTaxAuthorities = AccountingMasterFilesRegistry.Instance.TaxAuthorities.DefaultValue.OfType<TaxAuthoritiesConfiguration>().Where(x => x.Country == CountryCode).Select(x => new
			{
				code = x.Code.ToString(),
				country = x.Country.ToString(),
			});

			var taxAuthoritiesForCountry = ((IInstanceProvider<ITaxFrameworkConfigurationDefaults>)ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode)).Get().GetTaxAuthorities().Select(x => new
			{
				code = x.Code.ToString(),
				country = x.Country.ToString(),
			});

			AssertContainsExactElementsInAnyOrder(taxAuthoritiesForCountry, defaultValuesForTaxAuthorities);
		}

		public void TestCountrySpecificTaxSystemsMustbePresentInDefaultTaxSystems()
		{
			var defaultValuesForTaxSystems = AccountingMasterFilesRegistry.Instance.TaxSystems.DefaultValue.OfType<TaxSystemsConfiguration>().Where(x => x.Country == CountryCode).Select(x => new
			{
				code = x.Code.ToString(),
				country = x.Country.ToString(),
			});

			var taxSystemsForCountry = ((IInstanceProvider<ITaxFrameworkConfigurationDefaults>)ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode)).Get().GetTaxSystems().Select(x => new
			{
				code = x.Code.ToString(),
				country = x.Country.ToString(),
			});

			AssertContainsExactElementsInAnyOrder(taxSystemsForCountry, defaultValuesForTaxSystems);
		}

		public void TestExpectedNumberOfDefaultTaxAuthoritiesForCountry()
		{
			var actualNumberOfDefaultTaxAuthorities = ((IInstanceProvider<ITaxFrameworkConfigurationDefaults>)ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode)).Get().GetTaxAuthorities().Count;
			AssertEquals(ExpectedNumberOfDefaultTaxAuthorities, actualNumberOfDefaultTaxAuthorities);
		}

		public void TestExpectedNumberOfDefaultSystemsForCountry()
		{
			var actualNumberOfDefaultTaxSystems = ((IInstanceProvider<ITaxFrameworkConfigurationDefaults>)ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode)).Get().GetTaxSystems().Count;
			AssertEquals(ExpectedNumberOfDefaultTaxSystems, actualNumberOfDefaultTaxSystems);
		}
	}
}
