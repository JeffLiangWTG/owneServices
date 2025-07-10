#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class GLJournalApprovalThresholdControl
	{
		public ZArchitecture.ZGrid ApprovalThresholdSetupGrid_ForTestOnly
		{
			get { return ApprovalThresholdSetupGrid; }
			set { ApprovalThresholdSetupGrid = value; }
		}
	}
}

#endif
