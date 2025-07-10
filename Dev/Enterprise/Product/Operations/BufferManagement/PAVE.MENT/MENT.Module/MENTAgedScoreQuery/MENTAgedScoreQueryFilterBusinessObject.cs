using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Module
{
	public class MENTAgedScoreQueryFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);
			return filters;
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var faultyFilter = filters.AddFlagsFilter("Is Faulty", new[] { Res.GetString("fda0c401-166c-4989-9228-3d86153eb4f2", "Is Faulty"), Res.GetString("4500cc86-a2a5-4f00-8e82-4d89964485a3", "Is Not Faulty") }, new GetFlagsQuery[] { GetFaultyQuery, GetNotFaultyQuery });
			faultyFilter.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|MENT|MENTAgedScoreQuery|IsFaulty", "Faulty");
			faultyFilter.ArePropertiesMutuallyExclusive = true;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", MENTAgedScoreQuerySchema.MAQ_Code).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|MENT|MENTAgedScoreQuery|Code", "Code");
			filters.AddTextFilter("Query Description", MENTAgedScoreQuerySchema.MAQ_QueryDescription).MultilingualDescription = ResString.GetMultilingualString("BufferManagement|MENT|MENTAgedScoreQuery|Query Description", "Query Description");
		}

		#region Faulty filter

		ZQuery GetFaultyQuery(ZBool value)
		{
			return new ZQuery(MENTAgedScoreQuerySchema.MAQ_IsFaulty, value);
		}

		ZQuery GetNotFaultyQuery(ZBool value)
		{
			return new ZQuery(MENTAgedScoreQuerySchema.MAQ_IsFaulty, !value);
		}

		#endregion
	}
}
