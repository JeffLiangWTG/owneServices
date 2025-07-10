using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module FilterBusinessObject for AUImportClassification.
	/// Override validation and filter SQL generation here.
	/// </summary>
	public class AUImportClassificationFilterBusinessObject : Customs.Module.CusClassificationFilterBusinessObject
	{
		public AUImportClassificationFilterBusinessObject()
		{
		}

		#region Filters

		#region Tariff

		protected override void AddTariffFilters(ModuleFilterCollection filters)
		{
			filters.AddCustomFilter(new AUTariffModuleFilter("Tariff No", TariffModuleFilterType.Import, TariffFormatter));
		}

		#endregion

		#region Custom

		protected override void AddCustomFilters(ModuleFilterCollection filters)
		{
			var instrumentCodeFilter = GetAddInfoTextFilter("Instrument Code", AUAddInfo.Schema.ZA_InstrumentCode_Hidden.Substring(3));
			instrumentCodeFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(instrumentCodeFilter);

			var instrumentTypeFilter = GetAddInfoTextFilter("Instrument Type", AUAddInfo.Schema.ZA_InstrumentType_Hidden.Substring(3));
			instrumentTypeFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(instrumentTypeFilter);

			var treatmentCodeFilter = GetAddInfoTextFilter("Treatment Code", AUAddInfo.Schema.ZA_TreatmentCode_Hidden.Substring(3));
			treatmentCodeFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(treatmentCodeFilter);
		}

		#endregion

		#endregion

		#region Tariff Formatter

		protected override Business.TariffFormatter GetTariffFormatter()
		{
			return new AUImportTariffFormatter();
		}

		#endregion
	}
}
