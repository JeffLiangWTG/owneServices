using CargoWise.Common;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	public interface IHaveTaxFrameworkTestObjectCreator
	{
		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator { get; }
	}

	internal static class IHaveTaxFrameworkTestObjectCreatorExtensions
	{
		internal static TaxSystemsConfiguration CreateTaxSystem(this IHaveTaxFrameworkTestObjectCreator testClass, string taxSystemCode, string taxSystemDescription, string countryCode = "")
		{
			var taxSystem = testClass.TaxFrameworkTestObjectCreator.CreateTaxSystem(taxSystemCode);
			taxSystem.Name = taxSystemDescription;
			taxSystem.Country = !countryCode.IsNullOrEmpty() ? countryCode : Core.Constants.CountryCodes.Australia;

			return taxSystem;
		}

		internal static TaxAuthoritiesConfiguration CreateTaxAuthority(this IHaveTaxFrameworkTestObjectCreator testClass, string taxAuthorityCode, string taxAuthorityDescription, string countryCode = "")
		{
			var taxAuthority = testClass.TaxFrameworkTestObjectCreator.CreateTaxAuthority(taxAuthorityCode);
			taxAuthority.Name = taxAuthorityDescription;
			taxAuthority.Country = !countryCode.IsNullOrEmpty() ? countryCode : Core.Constants.CountryCodes.Australia;

			return taxAuthority;
		}
	}
}
