using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;

namespace Enterprise.BufferManagement.Module
{
	public class TagDefinitionCodeSubGroup : TagSubGroup<TagQueryProperty>
	{
		public TagDefinitionCodeSubGroup(ProcessHeaderFilterBusinessObject processHeaderFilter, string filterDescription)
			: base(processHeaderFilter, filterDescription)
		{
		}

		protected override TagQueryProperty CreateProperty(ModuleGuidAppliedToSubCollectionFilter filter)
		{
			return new TagQueryProperty(filter.Property, NotIn(filter.SqlComparisonOperator), IncludeInherited(filter.SqlComparisonOperator));
		}

		protected override ZQuery CreateQuery(List<TagQueryProperty> tagQueryProperties, ProcessHeaderFilterBusinessObject processHeaderFilter)
		{
			return TagQueryProvider.GetTagDefinitionCodeFilterForGroup(tagQueryProperties, processHeaderFilter.ParametersForTagFilters);
		}
	}
}
