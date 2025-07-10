using System;

namespace Enterprise.Builder.Generator
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Console application")]
	class ConsoleProgressLogger : IProgressLogger
	{
		public void AddProgressText(string text) => Console.WriteLine(text);
		public void ReportSkippedFile(string text) => Console.WriteLine("Skipping File: {0}", text);

		public void ShowStatusLine(string text)
		{
		}
	}
}
