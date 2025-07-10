using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	public partial class ClientPremiumServicesControl : ZUserControl
	{
		public ClientPremiumServicesControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			LicenceDatabase db = CurrentDataItem as LicenceDatabase;
			ProductionServiceLabel.Visible = db != null && db.LD_LicenceType != DatabaseTypes.Codes.Production;
		}
	}
}
