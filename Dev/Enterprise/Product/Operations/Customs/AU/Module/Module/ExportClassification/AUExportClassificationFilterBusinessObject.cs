using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module FilterBusinessObject for AUExportClassification.
	/// Override validation and filter SQL generation here.
	/// </summary>
	public class AUExportClassificationFilterBusinessObject : Customs.Module.CusClassificationFilterBusinessObject
	{
		public AUExportClassificationFilterBusinessObject()
		{
		}

		#region Filters

		#region Tariff

		protected override void AddTariffFilters(ModuleFilterCollection filters)
		{
			filters.AddCustomFilter(new AUTariffModuleFilter("Tariff No", TariffModuleFilterType.Export, TariffFormatter));
		}

		#endregion

		#endregion

		#region Tariff Formatter

		protected override Business.TariffFormatter GetTariffFormatter()
		{
			return new AUExportTariffFormatter();
		}

		#endregion
	}
}
