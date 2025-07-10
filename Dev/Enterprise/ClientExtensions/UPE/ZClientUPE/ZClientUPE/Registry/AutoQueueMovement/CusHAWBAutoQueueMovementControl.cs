using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class CusHAWBAutoQueueMovementControl : RegistryZUserControl
	{
		public CusHAWBAutoQueueMovementControl()
		{
			InitializeComponent();
			QueueMovementGrid.DisableImportDataMenuItem = true;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			QueueMovementGrid.ReadOnly = readOnly;
		}
	}
}
