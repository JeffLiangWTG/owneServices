using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class ProcessTemplateReleaseGroupRuleFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		internal ProcessTemplateReleaseGroupRuleFetchStrategy(ProcessTemplateReleaseGroupRule businessObject)
			: base(businessObject)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			BusinessObject.Factory.AddFetchHint(ProcessTemplateReleaseGroupRuleMappingSchema.PTM_PTR_Rule, BusinessObject.PK);
			BusinessObject.Factory.AddFetchHint(ProcessTemplateReleaseGroupRuleCategorySchema.PTC_PTR_Rule, BusinessObject.PK);
		}
	}
}
