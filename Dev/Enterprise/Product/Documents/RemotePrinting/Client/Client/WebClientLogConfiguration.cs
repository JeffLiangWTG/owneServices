namespace Enterprise.RemotePrinting.Client
{
	public struct WebClientLogConfiguration
	{
		public int DayToKeepOldLogFile;
		public bool ShouldClearOldLog;

		public WebClientLogConfiguration(int dayToKeep, bool clearOldLog)
		{
			DayToKeepOldLogFile = dayToKeep;
			ShouldClearOldLog = clearOldLog;
		}
	}
}
