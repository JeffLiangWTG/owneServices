using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Registry
{
	public class SimultaneousInvoiceProcessingRateLimiterRegistryItem : IntRegistryItem
	{
		public SimultaneousInvoiceProcessingRateLimiterRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ImmutableList<(string countryCode, int defaultNumberOfInvoicesProcessedSimultaneously)> countriesThatSupportEInvoiceProcessThrottling)
				: base(new SimultaneousInvoiceProcessingRateLimiterRegistryItemImpl(name, category, caption, hint, storage, options, countriesThatSupportEInvoiceProcessThrottling))
		{
		}
	}

	class SimultaneousInvoiceProcessingRateLimiterRegistryItemImpl : RegistryItemImpl
	{
		readonly bool DefaultResult;
		readonly ImmutableList<(string countryCode, int defaultNumberOfInvoicesProcessedSimultaneously)> CountriesThatSupportEInvoiceProcessThrottling;

		public SimultaneousInvoiceProcessingRateLimiterRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ImmutableList<(string countryCode, int defaultNumberOfInvoicesProcessedSimultaneously)> countriesThatSupportEInvoiceProcessThrottling)
			: base(name, category, caption, hint, new IntRegistryDataType(0, 100), null, storage, options, 0)
		{
			CountriesThatSupportEInvoiceProcessThrottling = countriesThatSupportEInvoiceProcessThrottling;
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var result = DefaultResult;
			var company = companyPK == GlbCompany.CurrentCompany.PK ? GlbCompany.CurrentCompany : new BusinessObjectFactory().Load<GlbCompany>(companyPK);
			var companyCountry = company?.GC_RN_NKCountryCode ?? ZString.Empty;
			var defaultValueForCompanyCountry = CountriesThatSupportEInvoiceProcessThrottling.FirstOrDefault(c => c.countryCode == companyCountry);
			return defaultValueForCompanyCountry == default ? 0 : defaultValueForCompanyCountry.defaultNumberOfInvoicesProcessedSimultaneously;
		}
	}
}
