using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.GUI
{
	public partial class SupportingDocumentsUserControl : ZUserControl, IAdditionalTabPage
	{
		public SupportingDocumentsUserControl()
		{
			InitializeComponent();
		}

		public ZUserControl AdditionalTabPageUserControl => this;

		public ResourceStringData AdditionalTabPageCaption => Res.GetData("A6480CEA-FCE3-4404-AD66-91EB7630D336", "Supporting Documents");

		public AdditionalTabPageVisibility AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		public int TabPageSequence => 1;
	}
}

