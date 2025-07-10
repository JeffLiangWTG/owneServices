using CargoWise.Windows.UI;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public partial class NctsFallbackConfigurationRegistryItemControl : RegistryZUserControl
	{
		public NctsFallbackConfigurationRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			fallbackSettingsGroupBox.SetReadOnly(readOnly);
		}
	}
}
