#if DEBUG

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class ChinaJournalListingModule
	{
		public ZPopupController GetNewController_ForTestOnly()
		{
			return GetNewController();
		}
	}
}

#endif
