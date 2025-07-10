using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Forwarding.Module
{
	public class ForwardingShipmentModuleCustomColumnsAndFiltersProvider : ModuleCustomColumnsAndFiltersProvider, Integration.Customs.IForwardingShipmentModuleCustomColumnsAndFiltersProvider
	{
		#region Implement

		protected override IEnumerable<IModuleCustomFiltersProvider> GetFiltersProviders(BusinessObjectFactory factory, FilterStripBusinessObject filterBusinessObject = null)
		{
			yield return new ForwardingShipmentModuleCustomsFiltersProvider(factory);
		}

		protected override IEnumerable<IGridCustomColumnsProvider> GetColumnsProviders()
		{
			yield return new ForwardingShipmentModuleCustomsColumnsProvider();
		}

		#endregion
	}
}
