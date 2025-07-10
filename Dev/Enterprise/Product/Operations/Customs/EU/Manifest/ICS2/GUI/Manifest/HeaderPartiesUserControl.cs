using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI;

public partial class HeaderPartiesUserControl : ZUserControl, IAdditionalTabPage
{
	public HeaderPartiesUserControl()
	{
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		DynamicHeaderPartiesPanel.UpdateLayout(new EUICS2BillPartiesLayouts());
	}

	#region IAdditionalTabPage Members

	ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

	ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("FC975A87-415A-4E56-9453-F7DD497AF994", "Header Parties");

	AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => h is AsycudaManifestHeader { IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator: true },
				dependencies: [
					header => header.AMA_TransportModeInfo,
					header => header.SpecificCircumstanceIndicatorInfo
				]
		);

	int IAdditionalTabPage.TabPageSequence => 1;

	#endregion
}
