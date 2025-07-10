using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class JobLevelWorkflowFilter : ModuleGuidFilter
	{
		public JobLevelWorkflowFilter(ZString description, IBusinessObjectCollection list)
			: base(description, ModuleIDs.ProcessHeader, ProcessHeaderSchema.PK, list)
		{
			MultilingualDescription = ResString.GetMultilingualString("D1F47BA3-6F5D-41F4-9E62-5DDD4DEDB750", "Job-level Workflow");
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			ModuleTextFilter.ComparisonConstants.FiltersMatch
		};

		protected override void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			subQuery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);
			query.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, subQuery, JoinCondition.And);
		}
	}
}
