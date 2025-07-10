using System.Diagnostics;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.PrintProcessing.PrintJobAutomation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, printing not opening a file or url")]
	public static class PrintJobProcess
	{
		public static void PrintDoc(string fileName)
		{
			Process printDocProcess = new Process(); // False alarm, printing not opening a file or url
			printDocProcess.StartInfo.FileName = fileName;
			printDocProcess.StartInfo.UseShellExecute = true;
			printDocProcess.StartInfo.Verb = (NoResString)"Print"; // Check with Zubin
			printDocProcess.StartInfo.CreateNoWindow = true;
			printDocProcess.Start(); // False alarm, printing not opening a file or url
			printDocProcess.WaitForInputIdle(1000);
			if (!printDocProcess.HasExited)
			{
				printDocProcess.Kill();
			}
			printDocProcess.Dispose();
		}

		public static Process GetPrintDocProcess(string fileName)
		{
			Process printDocProcess = new Process(); // False alarm, printing not opening a file or url
			printDocProcess.StartInfo.FileName = fileName;
			printDocProcess.StartInfo.UseShellExecute = true;
			printDocProcess.StartInfo.Verb = (NoResString)"Print"; // Check with Zubin
			printDocProcess.StartInfo.CreateNoWindow = true;
			return printDocProcess;
		}
	}
}
