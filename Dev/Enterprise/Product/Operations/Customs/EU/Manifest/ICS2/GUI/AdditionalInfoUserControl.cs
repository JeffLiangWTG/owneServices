using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class AdditionalInfoUserControl : ZUserControl, IAdditionalTabPage
	{
		public AdditionalInfoUserControl(ParentTabType parentTab)
		{
			this.parentTab = parentTab;
			InitializeComponent();
		}

		readonly ParentTabType parentTab;

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("6b5ef6fe-6ead-4eec-bdd4-39c740dc216a", "Additional Information");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new (
			h => parentTab != ParentTabType.Header || h is AsycudaManifestHeader { IsAdditionalInfosEnabled: true },
			h => ((AsycudaManifestHeader)h).IsAdditionalInfosEnabledInfo);

		int IAdditionalTabPage.TabPageSequence => 2;

		#endregion
	}

	public enum ParentTabType
	{
		Header,
		Bill,
		Pack,
	}
}
