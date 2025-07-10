using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class IncoTermsUserControl : ZUserControl
	{
		public IncoTermsUserControl()
		{
			InitializeComponent();
			CommercialInvoiceDetailsIncoTermsUserControl.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			CommercialInvoiceDetailsIncoTermsUserControl.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 0, true);
		}
	}
}
