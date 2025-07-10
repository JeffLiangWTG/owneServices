using Enterprise.Registry.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public partial class ADPasswordSettingsRegistryControl : RegistryZUserControl
	{
		public ADPasswordSettingsRegistryControl()
		{
			InitializeComponent();
		}

		void ADPasswordSettingsButton_Click(object sender, System.EventArgs e)
		{
			using (var adPasswordSettingsForm = new ADPasswordSettingsForm())
			{
				adPasswordSettingsForm.ShowDialog();
			}
		}
	}
}
