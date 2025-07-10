using System.Diagnostics;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common
{
	public class ProcessWrapperFactory : IProcessFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, running Service Task process, not opening a file or url")]
		public IProcess Create(ProcessStartInfo startInfo, bool enableRaisingEvents, ProcessPriorityClass priority)
		{
			var process = new Process()
			{
				StartInfo = startInfo,
				EnableRaisingEvents = enableRaisingEvents,
			};

			return new ProcessAdapter(process, priority);
		}
	}
}
