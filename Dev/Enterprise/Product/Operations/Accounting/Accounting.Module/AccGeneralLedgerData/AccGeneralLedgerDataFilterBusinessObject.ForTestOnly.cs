#if DEBUG

using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public partial class AccGeneralLedgerDataFilterBusinessObject
	{
		public ModuleFilterCollection GetModuleFiltersCore_ForTestOnly()
		{
			return GetModuleFiltersCore();
		}
	}
}

#endif
