using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class BranchDepartmentCodeMappingRegistryItemControl : RegistryZUserControl
	{
		public BranchDepartmentCodeMappingRegistryItemControl()
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
