using Enterprise.Registry.GUI;

namespace Enterprise.Client.TIP
{
	internal partial class OrgPartRelationRegistryControl : RegistryZUserControl
	{
		public OrgPartRelationRegistryControl()
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
