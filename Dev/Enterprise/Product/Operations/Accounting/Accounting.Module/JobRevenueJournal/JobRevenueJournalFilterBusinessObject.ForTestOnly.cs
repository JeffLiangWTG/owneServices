#if DEBUG

using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class JobRevenueJournalFilterBusinessObject
	{
		public ModuleFilterCollection GetModuleFiltersCore_ForTestOnly()
		{
			return GetModuleFiltersCore();
		}
	}
}

#endif
