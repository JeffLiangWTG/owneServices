using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ServiceTypeRegistryControl : RegistryZUserControl
	{
		public ServiceTypeRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			ProductsGrid.ReadOnly = readOnly;
			ModuleGrid.ReadOnly = readOnly;
			SourceModuleGrid.ReadOnly = readOnly;
		}
	}
}
