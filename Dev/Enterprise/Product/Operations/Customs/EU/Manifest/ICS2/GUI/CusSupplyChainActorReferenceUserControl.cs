using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class CusSupplyChainActorReferenceUserControl : ZUserControl, IAdditionalTabPage
	{
		public CusSupplyChainActorReferenceUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("689890D7-DCF9-4232-8B8E-290D54FC2DFA", "Additional Supply Chain Actor");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new(h => h is AsycudaManifestHeader { IsCusSupplyChainActorReferencesEnabled: true },
			h => ((AsycudaManifestHeader)h).IsCusSupplyChainActorReferencesEnabledInfo);

		int IAdditionalTabPage.TabPageSequence => 4;

		#endregion
	}
}
