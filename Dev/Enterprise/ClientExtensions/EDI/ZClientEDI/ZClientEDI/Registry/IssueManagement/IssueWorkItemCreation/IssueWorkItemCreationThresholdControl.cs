using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class IssueWorkItemCreationThresholdControl : RegistryZUserControl
	{
		public IssueWorkItemCreationThresholdControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.IssueWorkItemCreationThresholdGrid.ReadOnly = readOnly;
		}
	}
}
