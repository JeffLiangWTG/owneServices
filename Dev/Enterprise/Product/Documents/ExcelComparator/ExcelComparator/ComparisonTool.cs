using System.Diagnostics;
using System.IO;

namespace Enterprise.ExcelComparator
{
	class ComparisonTool : IComparisonTool
	{
		public ComparisonTool(string path)
		{
			this.Path = path;
		}

		public string Path
		{
			get;
			private set;
		}

		public bool IsInstalled()
		{
			return File.Exists(Path);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		public void RunComparison(string filePath1, string filePath2)
		{
			var processStartInfo = new ProcessStartInfo(Path, "\"" + filePath1 + "\" \"" + filePath2 + "\"");
			using (Process process = Process.Start(processStartInfo))
			{
				process.WaitForExit();
			}
		}
	}
}
