using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class UnderbondUserControl : ZUserControl
	{
		public UnderbondUserControl()
		{
			InitializeComponent();
		}

		public void ToggleRemovalTabVisibility(ICcsukCusAwb awb)
		{
			if (LicenceAndPimaHelper.IsSimpleAgentProfile(awb))
			{
				IsrsTabsPage.TabVisible = false;
				TsrTabPage.TabVisible = true;
				IarTabPage.TabVisible = true;
				FallbackTabPage.TabVisible = true;
			}
			else if (LicenceAndPimaHelper.IsFullShed(awb) || LicenceAndPimaHelper.IsFallbackShed(awb))
			{
				FallbackTabPage.TabVisible = false;
				IarTabPage.TabVisible = false;
				TsrTabPage.TabVisible = false;
				IsrsTabsPage.TabVisible = true;
			}
		}
	}
}
