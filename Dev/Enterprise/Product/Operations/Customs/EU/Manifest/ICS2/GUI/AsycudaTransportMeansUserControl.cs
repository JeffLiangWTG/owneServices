using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class AsycudaTransportMeansUserControl : ZUserControl, IAdditionalTabPage
	{
		public AsycudaTransportMeansUserControl()
		{
			InitializeComponent();
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("2DBE756C-400F-428B-B1C5-E4661103BD89", "Passive Border Transport");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new(h => h is AsycudaManifestHeader { IsAsycudaTransportMeansEnabled: var isEnabled } && isEnabled,
			h => ((AsycudaManifestHeader)h).IsAsycudaTransportMeansEnabledInfo);

		int IAdditionalTabPage.TabPageSequence => 5;
	}
}
