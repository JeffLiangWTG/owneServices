using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IL.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class AsycudaSupportingDocumentUserControl : SupportingDocumentUserControl, IAdditionalTabPage
	{
		public AsycudaSupportingDocumentUserControl()
		{
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("BCF3EEFA-3D40-402C-9987-7A1A4FB3EF95", "Supporting Documents");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 1;
	}
}
