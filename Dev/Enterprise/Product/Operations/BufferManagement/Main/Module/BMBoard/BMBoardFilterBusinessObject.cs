using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class BMBoardFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Name", BMBoardSchema.MB_Name).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardFilter|Name", "Name");
			result.AddTextFilter("Description", BMBoardSchema.MB_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardFilter|Description", "Description"); // Filter name

			var flagsFilter = result.AddFlagsFilter("Published", new[] { Res.GetString("425dd81a-b368-47d0-a99a-7d5ecc31c444", "Published"), Res.GetString("92ba9b05-f12f-4dd5-b6ae-93662d67ee1b", "Not Published") }, new GetFlagsQuery[] { GetPublishedQuery, GetNotPublishedQuery });
			flagsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardFilter|Published", "Published");
			flagsFilter.ArePropertiesMutuallyExclusive = true;

			var globalStatusFilter = result.AddTextFilter("Global", GetGlobalStatusQuery, new GlobalStatusList());
			globalStatusFilter.Category = FilterCategories.StatusAndFlags;
			globalStatusFilter.DefaultProperty = GlobalStatusList.Codes.All;
			globalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardFilter|Global", "Global");

			result.AddGuidFilter("System", ModuleIDs.BMSystems, BMBoardSchema.MB_FS_System, () => new BMSystemCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardFilter|System", "System");
			result.AddGuidFilter("ReleaseGroup", ModuleIDs.GlbGroup, BMBoardSchema.MB_GG_ReleaseGroup, () => new GlbGroupCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardFilter|ReleaseGroup", "Release Group");
			result.AddNkFilter("Owner", BMBoardSchema.MB_GS_NKStaffCode, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|BMBoardFilter|Owner", "Owner");

			return result;
		}

		#region Published filter

		ZQuery GetPublishedQuery(ZBool value)
		{
			return new ZQuery(BMBoardSchema.MB_IsPublished, value);
		}

		ZQuery GetNotPublishedQuery(ZBool value)
		{
			return new ZQuery(BMBoardSchema.MB_IsPublished, !value);
		}

		#endregion

		#region Global filter

		static ZQuery GetGlobalStatusQuery(ZString status)
		{
			switch (status)
			{
				case GlobalStatusList.Codes.Global:
					return new ZQuery(BMBoardSchema.MB_GC_Company, null);
				case GlobalStatusList.Codes.CurrentCompany:
					var currentCompany = GlbCompany.CurrentCompany;
					return currentCompany == null ? ZQuery.NoResultQuery : new ZQuery(BMBoardSchema.MB_GC_Company, currentCompany.PK);
				default:
					return new ZQuery();
			}
		}

		#endregion

		public override ZQuery Filter
		{
			get
			{
				var companyQuery = new ZQuery();
				companyQuery.AddToFilter(BMBoardSchema.MB_GC_Company, null);

				var currentCompany = GlbCompany.CurrentCompany;
				if (currentCompany != null)
				{
					companyQuery.AddToFilter(JoinCondition.Or, BMBoardSchema.MB_GC_Company, currentCompany.PK);
				}

				var query = base.Filter;
				query.AddToFilter(companyQuery);
				return query;
			}
		}
	}
}
