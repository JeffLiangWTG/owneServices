using System.IO;

namespace ServiceManager.Logging.CW.Test
{
	public static class LoggerExtensions
	{
		public static FileInfo CurrentOutputFileForTest(this Enterprise.Integration.ILogger logger, string taskCode)
		{
			return ((LoggerNLogWrapper)((LoggerAdapter)logger).logger).CurrentOutputFileForTest(taskCode);
		}
	}
}
