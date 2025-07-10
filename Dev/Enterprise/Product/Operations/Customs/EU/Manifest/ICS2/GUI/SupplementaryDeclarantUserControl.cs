using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class SupplementaryDeclarantUserControl : ZUserControl, IAdditionalTabPage
	{
		public SupplementaryDeclarantUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("94ccf8f6-20e2-40a3-aa10-2b0bab928529", "Supplementary Declarant");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new(
			h => h is AsycudaManifestHeader { IsSupplementaryDeclarantsEnabled: true },
			h => ((AsycudaManifestHeader)h).IsSupplementaryDeclarantsEnabledInfo);

		int IAdditionalTabPage.TabPageSequence => 5;

		#endregion
	}
}
