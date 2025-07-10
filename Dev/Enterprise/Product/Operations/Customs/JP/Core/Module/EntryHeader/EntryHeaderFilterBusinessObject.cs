using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.JP.Module
{
	public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
	{
		public static class FilterConstants
		{
			public const string InspectionStatus = "Inspection Status";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			var inspectionStatusFilter = new InspectionStatusFilter(
				FilterConstants.InspectionStatus,
				(inpsectionStatus, _) =>
					{
						var res = new ZDBOnlyQuery(typeof(CusEntryHeader));
						if (!inpsectionStatus.IsEmpty)
						{
							var queryText = $"CH_AddInfo like '%InspectionStatus={inpsectionStatus}%'";
							res.AddFilterAndZSQLParameterCollection(queryText, new ZSqlParameterCollection());
						}

						return res;
					},
				Factory);
			inspectionStatusFilter.Category = FilterCategories.StatusAndFlags;
			inspectionStatusFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilterBusinessObject|InspectionStatusFilter", FilterConstants.InspectionStatus);
			result.AddFilter(inspectionStatusFilter);

			return result;
		}
	}
}
