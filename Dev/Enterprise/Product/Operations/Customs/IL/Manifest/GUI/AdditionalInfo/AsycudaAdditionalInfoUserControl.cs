using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaAdditionalInfoUserControl : ZUserControl, IAdditionalTabPage, ISupportingInfoUserControls
	{
		public AsycudaAdditionalInfoUserControl()
		{
			InitializeComponent();
			additionalInfosGrid.AfterBind += Grid_AfterBind;
		}

		void Grid_AfterBind(object sender, System.EventArgs e)
		{
			AdjustControlProperties();
		}

		void AdjustControlProperties()
		{
			Dock = DockStyle.Fill;
		}

		#region ISupportingInfoUserControls

		string ISupportingInfoUserControls.GridBindingMember => "AdditionalInfos";

		ZGrid ISupportingInfoUserControls.Grid => additionalInfosGrid;

		#endregion ISupportingInfoUserControls

		#region IAdditionalTabPage

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("20E6B1D7-0443-4A00-AEDB-DACC910A976E", "Additional Info");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 1;

		#endregion
	}
}
