using System.Diagnostics;
using System.IO;

namespace CargoWise.Common.MemoryManagement
{
	public static class MemoryDump
	{
		/// <summary>
		/// Create a memory dump of the process
		/// </summary>
		/// <param name="dumpFileName">The fully qualified name for the file that the memory dump will be stored</param>
		/// <param name="process">The process that the memory dump will generate for</param>
		/// <returns>True if the memory dump was created successfully</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "This is a debugging process")]
		public static bool Create(string dumpFileName, Process process)
		{
			Argument.NotNull(process, nameof(process)); // Suggested By ReviewBot
			var nmpDir = Path.GetDirectoryName(typeof(MemoryDump).Assembly.Location);
#if NET
			nmpDir = Directory.GetParent(nmpDir).FullName;
#endif
			var nmp = Path.Combine(nmpDir, "NMPCore.exe");
			var args = "/a:" + process.Id.ToString() + " /cs:1 /sf:" + dumpFileName;
			var info = new ProcessStartInfo(nmp, args);
			info.UseShellExecute = false;
			using (var nmpProcess = new Process())  // This is a debugging process
			{
				nmpProcess.StartInfo = info;
				if (nmpProcess.Start()) // This is a debugging process
				{
					nmpProcess.WaitForExit();
				}
			}
			return File.Exists(dumpFileName);
		}
	}
}
