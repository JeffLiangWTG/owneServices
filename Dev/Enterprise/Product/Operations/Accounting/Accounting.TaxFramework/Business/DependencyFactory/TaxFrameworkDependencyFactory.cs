namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface ITaxFrameworkDependencyFactory
	{
		IAccTaxTransactionCriticalValidator GetAccTaxTransactionCriticalValidator(AccTaxTransaction parent);
		IRoundingMethodApplier GetRoundingMethodApplier();
		IOrganisationTaxRateImportDataProvider GetOrganisationTaxRateImportDataProvider();
	}

	public class TaxFrameworkDependencyFactory : ITaxFrameworkDependencyFactory
	{
		IAccTaxTransactionCriticalValidator ITaxFrameworkDependencyFactory.GetAccTaxTransactionCriticalValidator(AccTaxTransaction parent)
		{
			return new AccTaxTransactionCriticalValidator(parent);
		}

		IRoundingMethodApplier ITaxFrameworkDependencyFactory.GetRoundingMethodApplier()
		{
			return new RoundingMethodApplier();
		}

		IOrganisationTaxRateImportDataProvider ITaxFrameworkDependencyFactory.GetOrganisationTaxRateImportDataProvider()
		{
			return new OrganisationTaxRateImportDataProvider();
		}
	}
}

