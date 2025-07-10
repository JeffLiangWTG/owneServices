using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Forwarding.Module
{
	public class ForwardingConsolModuleCustomColumnsAndFiltersProvider : ModuleCustomColumnsAndFiltersProvider, Integration.Customs.IForwardingConsolModuleCustomColumnsAndFiltersProvider
	{
		#region Implement

		protected override IEnumerable<IModuleCustomFiltersProvider> GetFiltersProviders(BusinessObjectFactory factory, FilterStripBusinessObject filterBusinessObject)
		{
			yield return new ForwardingConsolModuleCustomsFiltersProvider(factory, filterBusinessObject);
		}

		protected override IEnumerable<IGridCustomColumnsProvider> GetColumnsProviders()
		{
			yield return new ForwardingConsolModuleCustomsColumnsProvider();
		}

		#endregion
	}
}
