using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class CustomsMessagingControl : IL.GUI.CustomsMessagingControl, IAdditionalTabPage
	{
		#region IAdditionalTabPage

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("80E39DE1-7306-4639-A5ED-5C31F3A02DF5", "Messages");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 6;

		#endregion
	}
}
