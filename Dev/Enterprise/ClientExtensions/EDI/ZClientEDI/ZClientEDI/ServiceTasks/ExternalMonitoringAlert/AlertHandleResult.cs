namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert
{
	class AlertHandleResult
	{
		internal int TotalProcessedAlerts;
		internal int TotalGeneratedTargets;
		internal int TotalRecentlyCreatedTargets;
		internal int TotalStillOpenTargets;
		internal int TotalNoMatchedRuleTargets;
		internal int TotalFailedToCreateTargets;
		internal int TotalGeneratedTasks;
		internal int TotalReachedMaximumTargets;

		internal AlertHandleResult()
		{
		}
	}
}
