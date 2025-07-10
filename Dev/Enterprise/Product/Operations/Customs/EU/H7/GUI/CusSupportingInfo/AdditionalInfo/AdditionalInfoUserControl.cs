using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class AdditionalInfoUserControl : ZUserControl, IAdditionalTabPage
	{
		public static ResourceStringData ResStringAdditionalInfo => Res.GetData("a7c8d9b6-6eee-4d3f-b8ea-1e6c4d1efe98", "Additional Infos");

		public AdditionalInfoUserControl()
		{
			InitializeComponent();
			AdditionalInfosGrid.AfterBind += AdditionalInfosGrid_AfterBind;
		}

		void AdditionalInfosGrid_AfterBind(object sender, EventArgs e)
		{
			DetailsPanel.UpdateLayout(new AdditionalInfoDetailsLayout());
		}

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("98389b83-4ad8-48d0-a77c-338066e46374", "Additional Info");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 30;

		#endregion
	}
}

