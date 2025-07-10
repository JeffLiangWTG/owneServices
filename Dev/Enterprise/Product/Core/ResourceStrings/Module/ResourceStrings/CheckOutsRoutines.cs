using Enterprise.ZArchitecture.Business;

namespace Enterprise.ResourceStrings.Module
{
	public static class CheckOutsRoutines
	{
		public static void FindMyCheckOuts(ResourceStringsFilterControl filterControl)
		{
			((ModuleTextFilter)filterControl.FilterBusinessObject["Check Out Status"]).Property = ResourceStringsFilterBusinessObject.CheckOutStatus.CheckedOut;
			filterControl.FirePerformSearch();
			filterControl.FilteredGrid.SelectAllElements();
		}
	}
}
