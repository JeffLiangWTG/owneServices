using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using IExternalModuleColumnsAndFiltersProvider = Enterprise.Integration.Customs.CA.IExternalModuleColumnsAndFiltersProvider;
using IShipmentModuleColumnsAndFiltersProvider = Enterprise.Integration.Customs.CA.IShipmentModuleColumnsAndFiltersProvider;

namespace Enterprise.Customs.CA.Module
{
	partial class CAShipmentModuleColumnsAndFiltersProvider : CAExternalModuleColumnsAndFiltersProvider, IShipmentModuleColumnsAndFiltersProvider
	{
		protected override IEnumerable<IExternalModuleColumnsAndFiltersProvider> GetColumnsAndFiltersProviders()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada)
			{
				yield return new RNSColumnsAndFiltersProvider();
			}
			yield return new ACIColumnsAndFiltersProvider();
		}
	}
}
