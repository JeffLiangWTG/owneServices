using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class IncidentDetailsUserControl : ZUserControl
	{
		public IncidentDetailsUserControl()
		{
			InitializeComponent();
			LocationOfGoodsUserControl.CusGoodsLocationProviderType = typeof(Business.EnRouteIncident);
		}
	}
}
