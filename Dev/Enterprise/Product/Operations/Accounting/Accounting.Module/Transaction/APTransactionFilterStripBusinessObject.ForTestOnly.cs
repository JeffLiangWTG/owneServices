#if DEBUG

using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class APTransactionFilterStripBusinessObject
	{
		public ModuleFilter GetModuleFilterThatOverridesAllOtherFilters_ForTestOnly()
		{
			return GetModuleFilterThatOverridesAllOtherFilters();
		}
	}
}

#endif
