#if DEBUG

namespace Enterprise.Accounting.GUI
{
	public partial class InvoicingSecurityOverrideProvider
	{
		public string GetClosedJobs_ForTestOnly()
		{
			return GetClosedJobs();
		}

		public bool UserInitiatorAndNotAllowedToApprove_ForTestOnly => UserInitiatorAndNotAllowedToApprove;

		public string InvoiceLevelsSecurityOverrideMessage_ForTestOnly => InvoiceLevelsSecurityOverrideMessage;

		public void PopulateAllLevelsSecurityRequiredBranchDepartment_ForTestOnly()
		{
			PopulateAllLevelsSecurityRequiredBranchDepartment();
		}
	}
}

#endif
