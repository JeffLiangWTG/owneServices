namespace Enterprise.GraphEngine.ServiceTasks
{
	public static class ServiceTaskLogs
	{
		public static string ServiceTaskShutdown(string reason)
		{
			return $"Service Task Shutdown. Reason: {reason}";
		}
	}
}
