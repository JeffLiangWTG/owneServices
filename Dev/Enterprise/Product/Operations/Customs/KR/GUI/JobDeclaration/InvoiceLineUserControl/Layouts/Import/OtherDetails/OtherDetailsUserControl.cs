using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class OtherDetailsUserControl : ZUserControl
	{
		public OtherDetailsUserControl()
		{
			InitializeComponent();

			MaterialTaxPanel.UpdateLayout(new MaterialTaxLayout());
			AgencyPanel.UpdateLayout(new PostClearanceAgencyLayout());
			InspectionPanel.UpdateLayout(new InspectionLayout());
		}
	}
}
