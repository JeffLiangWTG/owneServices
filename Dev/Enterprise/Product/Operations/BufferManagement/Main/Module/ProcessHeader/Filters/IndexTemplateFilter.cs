using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module
{
	public class IndexTemplateFilter : IndexSearchModuleTextFilter
	{
		const string FieldName = "WorkflowType";

		public IndexTemplateFilter(SearchField searchField, CodeDescriptionPairList list, FilterCategory category = null)
			: base(searchField, list, category)
		{
		}

		public override bool HasComparisonOperator => false;

		public override IGlowQuery GetGlowIndexQuery()
		{
			switch (Property)
			{
				case TemplateFilterOptions.Codes.All:
					return new EmptyQuery();
				case TemplateFilterOptions.Codes.NonTemplate:
					return new NotEqualQuery(new Term(FieldName, "P0"));
				case TemplateFilterOptions.Codes.Template:
					return new EqualQuery(new Term(FieldName, "P0"));
				default:
					return base.GetGlowIndexQuery();
			}
		}
	}
}
