using Enterprise.Registry.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI
{
	public partial class DomainCredentialsCollectionRegistryControl : RegistryZUserControl
	{
		public DomainCredentialsCollectionRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DomainCredentialsCollectionGrid.ReadOnly = ReadOnly;
		}
	}
}
