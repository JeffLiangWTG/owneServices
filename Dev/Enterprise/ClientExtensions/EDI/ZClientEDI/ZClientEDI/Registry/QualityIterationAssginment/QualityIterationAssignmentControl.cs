using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class QualityIterationAssignmentControl : RegistryZUserControl
	{
		public QualityIterationAssignmentControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			kSplitContainer1.Enabled = !readOnly;
			AssignmentGrid.ReadOnly = readOnly;
			IsDefaultOptionSelectedCheckBox.ReadOnly = readOnly;
		}
	}
}
