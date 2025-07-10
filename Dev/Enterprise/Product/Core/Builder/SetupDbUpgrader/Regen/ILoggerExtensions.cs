using System;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	static class ILoggerExtensions
	{
		public static void AddReportLine(this ILogger logger, string line)
		{
			logger.LogLineRaw("                 " + line);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "This is an internal developer tool.")]
		public static void AddReportHeader(this ILogger logger, string header)
		{
			logger.LogLineRaw(System.Environment.NewLine + " [" + DateTime.Now.ToLongTimeString() + "] - " + header);
		}
	}
}
