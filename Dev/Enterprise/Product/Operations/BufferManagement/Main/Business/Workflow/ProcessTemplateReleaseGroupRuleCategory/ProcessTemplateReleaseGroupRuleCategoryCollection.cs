using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTemplateReleaseGroupRuleCategoryCollection : ActiveBusinessObjectCollection<ProcessTemplateReleaseGroupRuleCategory>, IProcessTemplateReleaseGroupRuleCategoryCollection
	{
		public ProcessTemplateReleaseGroupRuleCategoryCollection(ProcessTemplateReleaseGroupRule parent)
			: base(parent.Factory, parent, new ZQuery(), ProcessTemplateReleaseGroupRuleCategorySchema.PTC_PTR_Rule)
		{
		}

		IProcessTemplateReleaseGroupRuleCategory IProcessTemplateReleaseGroupRuleCategoryCollection.this[int index] => this[index];
	}
}
