using Enterprise.Registry.GUI;

namespace Enterprise.Client.TGE.GUI
{
	public partial class TGEEventsRegistryItemControl : RegistryZUserControl
	{
		public TGEEventsRegistryItemControl()
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
