using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public partial class JPTemporaryLandingUserControl : ZUserControl, IAdditionalTabPage
	{
		public JPTemporaryLandingUserControl()
		{
			InitializeComponent();
			JPTemporaryLandingDynamicLayoutPanel.UpdateLayout(new JPTemporaryLandingLayouts());
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("617d212d-9f3f-4537-be93-a4f271d88f4a", "Temporary Landing");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => h.IsImport && h.IsSea, h => h.AMA_NatureInfo, h => h.AMA_TransportModeInfo);
		int IAdditionalTabPage.TabPageSequence => 0;
	}
}
