namespace Enterprise.BufferManagement.Integration
{
	public static class ProcessHeaderResponsiveActionConstants
	{
		public const string ProcessHeaderResponsiveUpdateSchedulerActionCode = "PHU";

		public const string UpdateAllExceptDedicatedBuffer = "AXB"; // required when the user switches BMS to live - we need to schedule all kind of updates except dedicated buffer update, because dedicated buffers will be updated separately by the BMS service task that we also nudge on BMS activation
		public const string UpdateDedicatedBuffer = "BUF"; // required on numerous changes on a BMS configuration
		public const string UpdateEffectiveBranchAndDepartment = "EBD"; // required when a component changes its aging branch/department only
		public const string UpdateDedicatedBufferEffectiveBranchAndDepartment = "BBD"; // required when a component changes both its type and aging branch/department
	}
}
