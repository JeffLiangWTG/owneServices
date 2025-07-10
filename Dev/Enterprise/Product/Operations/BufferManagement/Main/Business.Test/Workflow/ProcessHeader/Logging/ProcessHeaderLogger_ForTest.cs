namespace Enterprise.BufferManagement.Business
{
	public static class ProcessHeaderLogger_ForTest
	{
		public static void AddJobStatusChangeEvent(ProcessHeader processHeader, bool isOpen)
		{
			ProcessHeaderLogger.AddJobStatusChangeEvent(processHeader, isOpen);
		}
	}
}
