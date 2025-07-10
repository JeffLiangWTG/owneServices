using Enterprise.Registry.GUI;

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public partial class MXWsVucemRegistryItemUserControl : RegistryZUserControl
	{
		public MXWsVucemRegistryItemUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			WSAirModeURLTextBox.ReadOnly = readOnly;
			WSAirModeUsernameTextBox.ReadOnly = readOnly;
			WSAirModePasswordTextBox.ReadOnly = readOnly;
			WSSeaModeURLTextBox.ReadOnly = readOnly;
			WSSeaModeUsernameTextBox.ReadOnly = readOnly;
			WSSeaModePasswordTextBox.ReadOnly = readOnly;
		}
	}
}
