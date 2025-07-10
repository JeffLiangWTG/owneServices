using Enterprise.Registry.GUI;

namespace Enterprise.Customs.DE.GUI.Registry
{
	public partial class SendAcknowledgementsRegistryItemControl : RegistryZUserControl
	{
		public SendAcknowledgementsRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AcknowledgementsGrid.ReadOnly = readOnly;
		}
	}
}
