using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class CMREstablishmentCodesFilterBusinessObject : FilterStripBusinessObject
	{
		public CMREstablishmentCodesFilterBusinessObject()
		{
			QueryObjectType = typeof(CMREstablishmentCodes);
		}

		public static class Schema
		{
			public const string Code = "Code";
			public const string Type = "Type";
			public const string SubType = "Sub Type";
			public const string Name = "Name";
			public const string StartDate = "Start Date";
			public const string EndDate = "End Date";
			public const string PremisesIndicator = "Premises Indicator";
			public const string PortCode = "Port Code";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagsFilter(filters);
			AddDateFilters(filters);
			AddNkFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Schema.Code, CMREstablishmentCodesSchema.EC_EstablishmentCode);
			filters.AddTextFilter(Schema.Type, CMREstablishmentCodesSchema.EC_EstablishmentType);
			filters.AddTextFilter(Schema.SubType, CMREstablishmentCodesSchema.EC_EstablishmentSubType);
			filters.AddTextFilter(Schema.Name, CMREstablishmentCodesSchema.EC_EstablishmentName);
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Schema.StartDate, CMREstablishmentCodesSchema.EC_EstablishmentStartDate);
			filters.AddDateFilter(Schema.EndDate, CMREstablishmentCodesSchema.EC_EstablishmentEndDate);
		}

		void AddFlagsFilter(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter(Schema.PremisesIndicator, new string[] { "Yes" }, new SchemaBoolColumn[] { CMREstablishmentCodesSchema.EC_EstablishmentAQISPremisesIndicator });
		}

		void AddNkFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter(Schema.PortCode, CMREstablishmentCodesSchema.EC_EstablishmentPortCode, ModuleIDs.RefUNLOCO, GetAustraliaPortCodes()).Category = FilterCategories.Locations;
		}

		ActiveBusinessObjectCollection<RefUNLOCO> GetAustraliaPortCodes()
		{
			var result = new RefUNLOCOCollection(Factory);
			result.AdditionalFilter = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Australia);
			return result;
		}

		#endregion
	}
}
