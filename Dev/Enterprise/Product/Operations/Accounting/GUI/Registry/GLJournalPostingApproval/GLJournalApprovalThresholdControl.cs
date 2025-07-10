using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class GLJournalApprovalThresholdControl : RegistryZUserControl
	{
		public GLJournalApprovalThresholdControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			AccountTypeGrid.ReadOnly = readOnly;
			ApprovalThresholdSetupGrid.ReadOnly = readOnly;
		}
	}
}

