using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class MyAccountHostingSiteLandingPageUrlUserControl : RegistryZUserControl
	{
		public MyAccountHostingSiteLandingPageUrlUserControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			SiteLandingPageUrlGrid.ReadOnly = readOnly;
		}
	}
}
