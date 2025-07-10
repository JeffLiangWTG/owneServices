using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class MatchingFilterBusinessObject
	{
		public ModuleFilterCollection GetModuleFiltersCore_ForTestOnly()
		{
			return GetModuleFiltersCore();
		}
	}
}
