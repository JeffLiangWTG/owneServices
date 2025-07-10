using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class HRCMScreeningResultUserControl : ZUserControl, IAdditionalTabPage
	{
		public HRCMScreeningResultUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("4a132127-d385-4233-8d30-85f06cac454b", "HRCM");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new (
			h => h is AsycudaManifestHeader { IsBillScreeningsEnabled: true },
			h => ((AsycudaManifestHeader)h).IsBillScreeningsEnabledInfo);

		int IAdditionalTabPage.TabPageSequence => 4;

		#endregion
	}
}
