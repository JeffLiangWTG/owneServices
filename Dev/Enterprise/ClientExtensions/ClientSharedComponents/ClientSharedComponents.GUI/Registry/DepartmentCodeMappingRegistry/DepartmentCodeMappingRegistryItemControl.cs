using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class DepartmentCodeMappingRegistryItemControl : RegistryZUserControl
	{
		public DepartmentCodeMappingRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			zGrid1.ReadOnly = readOnly;
		}
	}
}
