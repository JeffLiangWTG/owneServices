namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class ShelfStatuses
	{
		public const string CheckedIn = "CIN";
		public const string Rejected = "REJ";
		public const string RejectedForPendingAspectData = "RPA";
		public const string BuildIsImported = "BIM";
		public const string CheckedInAndNotified = "CNF";
		public const string RejectedAndNotified = "RNF";
		public const string RejectedForPendingAspectDataAndNotified = "RAN";
		public const string Passed = "PAS";
		public const string Queued = "QUE";
		public const string PassedAndNotified = "PAN";
		public const string QueuedForBranchDetection = "QBD";
		public const string DeploymentJobFailed = "DJF";
		public const string DeploymentJobFailedAndNotified = "DNF";
		public const string Cancelled = "CAN";
	}
}
