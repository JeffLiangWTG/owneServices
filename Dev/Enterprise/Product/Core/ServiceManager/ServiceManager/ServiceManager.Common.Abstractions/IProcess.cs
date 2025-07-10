using System;
using System.Diagnostics;

namespace ServiceManager.Common.Abstractions
{
	public interface IProcess : IDisposable
	{
		bool HasExited { get; }
		int Id { get; }
		RunnerExitCode ExitCode { get; }
		IntPtr Handle { get; }
		ProcessThreadCollection Threads { get; }
		ProcessStartInfo StartInfo { get; }
		bool EnableRaisingEvents { get; }
		ProcessPriorityClass Priority { get; }

		event DataReceivedEventHandler ErrorDataReceived;
		event DataReceivedEventHandler OutputDataReceived;
		event EventHandler Exited;

		bool WaitForExit(int timeoutMs);
		void Kill();
		bool Start();
		void Close();
		void BeginErrorReadLine();
		void BeginOutputReadLine();
	}
}
