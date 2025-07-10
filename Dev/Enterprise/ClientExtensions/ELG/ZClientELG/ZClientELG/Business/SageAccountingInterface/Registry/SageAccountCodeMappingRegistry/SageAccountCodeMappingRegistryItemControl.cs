using Enterprise.Registry.GUI;

namespace Enterprise.Client.ELG
{
	public partial class SageAccountCodeMappingRegistryItemControl : RegistryZUserControl
	{
		public SageAccountCodeMappingRegistryItemControl()
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
