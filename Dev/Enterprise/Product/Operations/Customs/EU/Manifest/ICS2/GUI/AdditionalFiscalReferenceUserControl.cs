using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class AdditionalFiscalReferenceUserControl : ZUserControl, IAdditionalTabPage
	{
		public AdditionalFiscalReferenceUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("8EC1D292-A821-42DC-BE77-EEC273A4BA58", "Additional Fiscal Reference");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new(h => h is AsycudaManifestHeader { IsAdditionalFiscalReferenceEnabled: true },
			h => ((AsycudaManifestHeader)h).IsAdditionalFiscalReferenceEnabledInfo);

		int IAdditionalTabPage.TabPageSequence => 4;

		#endregion
	}
}
