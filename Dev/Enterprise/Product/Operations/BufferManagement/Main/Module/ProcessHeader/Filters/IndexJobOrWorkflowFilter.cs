using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.Module
{
	public class IndexJobOrWorkflowFilter : IndexSearchModuleTextFilter
	{
		const string FieldName = "JobOrWorkflow";

		public IndexJobOrWorkflowFilter(SearchField searchField, CodeDescriptionPairList list, FilterCategory category = null)
			: base(searchField, list, category)
		{
		}

		public override bool HasComparisonOperator => false;

		public override IGlowQuery GetGlowIndexQuery()
		{
			switch (Property)
			{
				case IndexJobOrWorkflowFilterOptions.Codes.All:
					return new EmptyQuery();
				case IndexJobOrWorkflowFilterOptions.Codes.Job:
					return new IsBlankQuery(new Term(FieldName, null), includeEmptyString: false);
				case IndexJobOrWorkflowFilterOptions.Codes.Workflow:
					return new IsNotBlankQuery(new Term(FieldName, null), includeEmptyString: false);
				default:
					return base.GetGlowIndexQuery();
			}
		}
	}
}
