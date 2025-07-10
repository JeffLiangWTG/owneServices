using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class ProductAreaAssignmentsControl : RegistryZUserControl
	{
		public ProductAreaAssignmentsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.AssignmentGrid.ReadOnly = readOnly;
		}
	}
}
