using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.TaxFramework.Business.Testing
{
	public class TaxFrameworkDependencyFactoryTest : TestCaseWithFactory
	{
		public void TestGetITaxFrameworkDependencyFactory()
		{
			AssertType<TaxFrameworkDependencyFactory>(ObjectFactory.Get<ITaxFrameworkDependencyFactory>());
		}

		public void TestGetAccTaxTransactionCriticalValidator()
		{
			var parent = Factory.New<AccTaxTransaction>();
			AssertType<AccTaxTransactionCriticalValidator>(ObjectFactory.Get<ITaxFrameworkDependencyFactory>().GetAccTaxTransactionCriticalValidator(parent));
		}

		public void TestGetRoundingMethodApplier()
		{
			AssertType<RoundingMethodApplier>(ObjectFactory.Get<ITaxFrameworkDependencyFactory>().GetRoundingMethodApplier());
		}

		public void TestGetOrganisationTaxRateImportDataProvider()
		{
			AssertType<OrganisationTaxRateImportDataProvider>(ObjectFactory.Get<ITaxFrameworkDependencyFactory>().GetOrganisationTaxRateImportDataProvider());
		}
	}
}
