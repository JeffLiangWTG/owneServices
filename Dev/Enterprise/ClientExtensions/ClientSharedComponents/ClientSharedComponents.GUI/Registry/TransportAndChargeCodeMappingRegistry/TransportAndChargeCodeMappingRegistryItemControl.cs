using Enterprise.Registry.GUI;

namespace Enterprise.ClientSharedComponents.Registry
{
	public partial class TransportAndChargeCodeMappingRegistryItemControl : RegistryZUserControl
	{
		public TransportAndChargeCodeMappingRegistryItemControl()
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
