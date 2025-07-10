using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class TransportDepartureUserControl : ZUserControl
	{
		public TransportDepartureUserControl()
		{
			InitializeComponent();

			VesselCodeFindBox.CodeBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			AdditionalWagonNumbersButton.ToolTipCaption = ResString.GetMultilingualString("6B2F8F0E-2890-4032-9245-FAE31D69721C", "Additional Wagon Numbers");
			SetCountryCodeBoxMaxSize();
		}

		void SetCountryCodeBoxMaxSize()
		{
			var maximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 0, true);
			TransportAtDepartureTrailer1NationalityCodeFindBox.CodeBox.MaximumSize = maximumSize;
			TransportAtDepartureTrailer2NationalityCodeFindBox.CodeBox.MaximumSize = maximumSize;
			TransportAtDepartureCountryCodeFindBox.CodeBox.MaximumSize = maximumSize;
			VesselCountryCodeFindBox.CodeBox.MaximumSize = maximumSize;
		}

		void AdditionalWagonNumbersButton_Click(object sender, System.EventArgs e)
		{
			var parentControl = (ZUserControl)((ZButton)sender).Parent;
			var inlandTransport = (IDepartureTransportMeansProvider)parentControl.BindingSource.Current;

			ZFormModaliser.Show(new AdditionalWagonNumbersForm(inlandTransport), parentControl.ParentForm);
		}
	}
}
