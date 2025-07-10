using Enterprise.Registry.GUI;

namespace Enterprise.Client.UPE.Registry.GUI
{
	public partial class ChaseQueueValidationControl : RegistryZUserControl
	{
		public ChaseQueueValidationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ChaseQueueValidationGrid.ReadOnly = readOnly;
		}
	}
}
