using System;
using System.Diagnostics;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common
{
	sealed class ProcessAdapter : IProcess
	{
		readonly Process process;

		public event DataReceivedEventHandler ErrorDataReceived;
		public event DataReceivedEventHandler OutputDataReceived;
		public event EventHandler Exited;

		public ProcessAdapter(Process process, ProcessPriorityClass priority)
		{
			this.process = process;
			this.process.Exited += ProcessExited;
			this.process.OutputDataReceived += ProcessOutputDataReceived;
			this.process.ErrorDataReceived += ProcessErrorDataReceived;
			this.processStartPriority = priority;
		}

		void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e) => ErrorDataReceived?.Invoke(this, e);
		void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e) => OutputDataReceived?.Invoke(this, e);
		void ProcessExited(object sender, EventArgs e) => Exited?.Invoke(this, e);

		public bool HasExited => process.HasExited;
		public int Id => process.Id;
		public RunnerExitCode ExitCode => (RunnerExitCode)process.ExitCode;
		public IntPtr Handle => process.Handle;
		public ProcessThreadCollection Threads => process.Threads;
		public ProcessPriorityClass Priority => process.PriorityClass;
		readonly ProcessPriorityClass processStartPriority;
		public ProcessStartInfo StartInfo => process.StartInfo;
		public bool EnableRaisingEvents => process.EnableRaisingEvents;

		public void Dispose()
		{
			process.Exited -= ProcessExited;
			process.OutputDataReceived -= ProcessOutputDataReceived;
			process.ErrorDataReceived -= ProcessErrorDataReceived;
			process.Dispose();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Is a complex case not satisfied by ProgramLauncher.Launch")]
		public bool Start()
		{
			if (!process.Start())
			{
				return false;
			}

			process.Refresh();
			if (process.PriorityClass != processStartPriority)
			{
				process.PriorityClass = processStartPriority;
			}
			return true;
		}

		public bool WaitForExit(int timeoutMs) => process.WaitForExit(timeoutMs);
		public void Kill() => process.Kill();
		public void Close() => process.Close();
		public void BeginErrorReadLine() => process.BeginErrorReadLine();
		public void BeginOutputReadLine() => process.BeginOutputReadLine();
	}
}
