using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.ICS.GUI
{
	public partial class ItineraryForManifestHeaderUserControl : ZUserControl, IAdditionalTabPage
	{
		public ItineraryForManifestHeaderUserControl()
		{
			InitializeComponent();
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("44DF5EF1-0E48-4284-92E3-9FDEC56C64D8", "Itinerary");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new(IsVisible, t => t.AMA_ApplicationCodeInfo);
		int IAdditionalTabPage.TabPageSequence => 1;

		bool IsVisible(AsycudaManifestHeader header) => header.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.ShippingLine
			|| header.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.Consolidator;

		#endregion
	}
}
