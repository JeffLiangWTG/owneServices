using System;

namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	sealed class ConsoleLogger : ILogger
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Baseline")]
		public void LogLineRaw(string line) => Console.WriteLine(line);
	}
}
