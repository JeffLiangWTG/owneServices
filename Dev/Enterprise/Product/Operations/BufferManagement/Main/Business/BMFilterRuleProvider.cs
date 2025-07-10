using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	public class BMFilterRuleProvider : FilterRuleProvider
	{
		public BMFilterRuleProvider(IRelatedModuleFilterSupportable parent, string filterName = null, ModuleIdentifier moduleID = null)
			: base(moduleID ?? ModuleIDs.BMFilterRule, parent, filterName)
		{
		}
	}
}
