using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ActivitySubtypeAssignmentsControl : RegistryZUserControl
	{
		public ActivitySubtypeAssignmentsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AssignmentGrid.ReadOnly = readOnly;
		}
	}
}
