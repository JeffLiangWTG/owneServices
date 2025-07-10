using System;

namespace Enterprise.Builder.Generator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
	static class ProgressLoggerExtensions
	{
		public static void AddProcessHeaderLine(this IProgressLogger logger, string processText)
		{
			logger.ShowStatusLine("");
			logger.AddProgressText(System.Environment.NewLine + " [" +
				DateTime.Now.ToLongTimeString() + "] - " + processText + "...");
		}

		public static void AddProcessDetailLine(this IProgressLogger logger, string text)
		{
			string lineLabel = " [" + DateTime.Now.ToLongTimeString() + "]     - ";
			text = lineLabel + text.Replace(System.Environment.NewLine, System.Environment.NewLine + lineLabel);
			logger.AddProgressText(text);
		}
	}
}
