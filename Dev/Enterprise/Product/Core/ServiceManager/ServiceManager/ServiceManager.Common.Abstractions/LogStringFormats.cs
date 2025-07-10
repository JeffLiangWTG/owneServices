namespace ServiceManager.Common.Abstractions
{
	public static class LogStringFormats
	{
		public static readonly string LoggerTimeMask = "yyyy-MM-dd HH:mm:ss.fff"; // Locale independent pattern that can be parsed by log readers
		public static readonly string LoggerTimeMaskWithoutMilliseconds = "yyyy-MM-dd HH:mm:ss"; // Locale independent pattern that can be parsed by log readers
	}
}
