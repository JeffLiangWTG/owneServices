using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using ICFSShipmentModuleColumnsAndFiltersProvider = Enterprise.Integration.Customs.CA.ICFSShipmentModuleColumnsAndFiltersProvider;
using IExternalModuleColumnsAndFiltersProvider = Enterprise.Integration.Customs.CA.IExternalModuleColumnsAndFiltersProvider;

namespace Enterprise.Customs.CA.Module
{
	partial class CACFSShipmentModuleColumnsAndFiltersProvider : CAShipmentModuleColumnsAndFiltersProvider, ICFSShipmentModuleColumnsAndFiltersProvider
	{
		protected override IEnumerable<IExternalModuleColumnsAndFiltersProvider> GetColumnsAndFiltersProviders()
		{
			var providers = new List<IExternalModuleColumnsAndFiltersProvider>();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				providers.Add(new RNSColumnsAndFiltersProvider());
				providers.Add(new ArrivalCertificationColumnsAndFiltersProvider());
			}

			return providers.AsEnumerable();
		}
	}
}
