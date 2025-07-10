using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;

namespace Enterprise.BufferManagement.Business
{
	class TagRulePolicy : ITagRulePolicy
	{
		bool ITagRulePolicy.ShouldAddCompanyRelatedFilters
		{
			get
			{
				return BMSRegistry.Instance.AllowCompanyFiltersInTagRules.Value
					|| Env.Instance.ServiceTaskCode != TagServiceTask.Code;
			}
		}
	}
}
