namespace Enterprise.BufferManagement.Integration
{
	public interface ITagRulePolicy
	{
		bool ShouldAddCompanyRelatedFilters
		{
			get;
		}
	}
}
