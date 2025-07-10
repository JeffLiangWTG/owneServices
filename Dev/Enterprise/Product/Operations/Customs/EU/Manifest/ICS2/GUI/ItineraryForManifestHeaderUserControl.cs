using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class ItineraryForManifestHeaderUserControl : ZUserControl, IAdditionalTabPage
	{
		public ItineraryForManifestHeaderUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => CaptionResourceString;

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 1;

		#endregion
	}
}
