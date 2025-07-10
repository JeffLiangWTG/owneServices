using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.Module
{
	public class TagMagnitudeSubGroup : TagSubGroup<TagMagnitudeQueryProperty>
	{
		public TagMagnitudeSubGroup(ProcessHeaderFilterBusinessObject processHeaderFilter, string filterDescription)
			: base(processHeaderFilter, filterDescription)
		{
		}

		protected override TagMagnitudeQueryProperty CreateProperty(ModuleGuidAppliedToSubCollectionFilter filter)
		{
			return new TagMagnitudeQueryProperty(filter.Property, NotIn(filter.SqlComparisonOperator), IncludeInherited(filter.SqlComparisonOperator));
		}

		protected override ZQuery CreateQuery(List<TagMagnitudeQueryProperty> tagQueryProperties, ProcessHeaderFilterBusinessObject processHeaderFilter)
		{
			return TagQueryProvider.GetTagMagnitudeFilterForGroup(tagQueryProperties, processHeaderFilter.ParametersForTagFilters, processHeaderFilter.Factory);
		}
	}
}
