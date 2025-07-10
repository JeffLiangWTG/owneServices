using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class ReferralRequestUserControl : ZUserControl, IAdditionalTabPage
	{
		public ReferralRequestUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("d0bb969e-2551-4417-ae07-962bd1a3d821", "Communications");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(header => true, null);

		int IAdditionalTabPage.TabPageSequence => 3;

		#endregion
	}
}
