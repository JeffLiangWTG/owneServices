using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TempStoragePremisesFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Code = "Code";
			public const string Type = "Type";
			public const string Location = "Location";
			public const string Description = "Description";
		}
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var codeTextFilter = result.AddTextFilter(Schema.Code, CusTempStorageRegPremisesSchema.SRP_Code);
			codeTextFilter.Category = FilterCategories.NumbersAndReferences;
			codeTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStoragePremisesFilter|Code", Schema.Code);

			var typeTextFilter = result.AddTextFilter(Schema.Type, CusTempStorageRegPremisesSchema.SRP_Type, Lookups.TypeList);
			typeTextFilter.Category = FilterCategories.StatusAndFlags;
			typeTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStoragePremisesFilter|Type", Schema.Type);

			var locationTextFilter = result.AddTextFilter(Schema.Location, CusTempStorageRegPremisesSchema.SRP_CustomsLocation);
			locationTextFilter.Category = FilterCategories.Locations;
			locationTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStoragePremisesFilter|Location", Schema.Location);

			var descriptionTextFilter = result.AddTextFilter(Schema.Description, CusTempStorageRegPremisesSchema.SRP_Description);
			descriptionTextFilter.Category = FilterCategories.TextSearch;
			descriptionTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStoragePremisesFilter|Description", Schema.Description);

			return result;
		}

		CusTempStorageRegPremisesLookups Lookups => lookups ?? (lookups = Factory.GetNull<CusTempStorageRegPremises>().Lookups);
		CusTempStorageRegPremisesLookups lookups;
	}
}
