using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class WebSecurityMappingControl : RegistryZUserControl
	{
		public WebSecurityMappingControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.MappingGrid.ReadOnly = readOnly;
		}
	}
}
