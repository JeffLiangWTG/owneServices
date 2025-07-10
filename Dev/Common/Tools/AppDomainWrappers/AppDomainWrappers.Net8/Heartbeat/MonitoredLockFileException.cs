namespace AppDomainWrappers.Net8
{
	[Serializable]
	public class MonitoredLockFileException : Exception
	{
		public MonitoredLockFileException()
			: base()
		{
		}

		public MonitoredLockFileException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public MonitoredLockFileException(string message)
			: base(message)
		{
		}
	}
}
